using CodeRocket.DbTools.IntegrationTests.Infrastructure;
using CodeRocket.DbTools.Services;
using MySqlConnector;

namespace CodeRocket.DbTools.IntegrationTests;

/// <summary>
/// Integration tests for DbTools using MariaDB in Docker container
/// </summary>
[TestClass]
public class DbToolsBasicTests
{
    private DockerMariaDbContainer? _mariaDbContainer;
    private DatabaseService? _databaseService;
    private MigrationService? _migrationService;
    private string? _connectionString;

    [TestInitialize]
    public async Task TestInitialize()
    {
        // Start MariaDB container for each test
        _mariaDbContainer = new DockerMariaDbContainer();
        await _mariaDbContainer.StartAsync();
        
        _connectionString = _mariaDbContainer.ConnectionString;
        _databaseService = new DatabaseService(_connectionString);
        
        // Create MigrationService with path to migrations from the main project
        var migrationsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "CodeRocket.DbTools", "Migrations");
        _migrationService = new MigrationService(_databaseService, migrationsPath);
        
        Console.WriteLine($"Test initialized with database: {_connectionString}");
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        // Stop and remove container after each test
        if (_mariaDbContainer != null)
        {
            try
            {
                await _mariaDbContainer.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during cleanup: {ex.Message}");
            }
            finally
            {
                _mariaDbContainer.Dispose();
                _mariaDbContainer = null;
            }
        }
    }

    [TestMethod]
    [TestCategory("Database")]
    public async Task Test01_InitialDatabaseCreation_ShouldCreateDatabaseAndVersionTable()
    {
        // Arrange & Act
        var result = await _migrationService!.SetupDatabaseAsync();

        // Assert
        Assert.IsTrue(result.Success, $"Database setup should be successful. Error: {result.Message}");
        
        // Verify that the database is created
        var dbExists = await _databaseService!.EnsureDatabaseExistsAsync();
        Assert.IsTrue(dbExists, "Database should be created");
        
        // Verify that the db_versions table is created
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'db_versions'";
        var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        Assert.AreEqual(1, tableCount, "The db_versions table should be created");
        
        Console.WriteLine("✅ Test 1: Successfully created database and version table");
    }

    [TestMethod]
    [TestCategory("Migrations")]
    public async Task Test02_ApplyMigrations_ShouldExecuteInitialMigration()
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();

        // Act
        var migrations = _migrationService.GetAvailableMigrations();
        var executedMigrations = await _databaseService!.GetExecutedVersionsAsync();

        // Assert
        Assert.IsTrue(migrations.Count > 0, "Migrations should be available");
        
        var initialMigration = migrations.FirstOrDefault(m => m.Version == "Initial");
        Assert.IsNotNull(initialMigration, "Initial migration should be available");
        
        var executedInitial = executedMigrations.FirstOrDefault(m => m.Version == "Initial");
        Assert.IsNotNull(executedInitial, "Initial migration should be executed");
        
        Console.WriteLine("✅ Test 2: Migrations successfully applied");
    }

    [TestMethod]
    [TestCategory("Versions")]
    public async Task Test03_GetLatestMigration_ShouldReturnCorrectVersion()
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();

        // Act
        var executedMigrations = await _databaseService!.GetExecutedVersionsAsync();
        var latestVersion = executedMigrations.OrderByDescending(m => m.ExecutedAt).FirstOrDefault();

        // Assert
        Assert.IsNotNull(latestVersion, "Latest version should be retrieved");
        Assert.AreEqual("Initial", latestVersion.Version, "Latest version should be Initial");
        Assert.IsTrue(executedMigrations.Count > 0, "There should be executed migrations");
        
        Console.WriteLine($"✅ Test 3: Retrieved correct version: {latestVersion.Version}");
    }

    [TestMethod]
    [TestCategory("Database")]
    public async Task Test04_RunSetupTwice_ShouldNotRecreateExistingDatabase()
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();
        var firstSetupVersions = await _databaseService!.GetExecutedVersionsAsync();
        var firstSetupVersion = firstSetupVersions.OrderByDescending(m => m.ExecutedAt).FirstOrDefault();

        // Act - run setup second time
        var secondSetupResult = await _migrationService.SetupDatabaseAsync();
        var secondSetupVersions = await _databaseService.GetExecutedVersionsAsync();
        var secondSetupVersion = secondSetupVersions.OrderByDescending(m => m.ExecutedAt).FirstOrDefault();

        // Assert
        Assert.IsTrue(secondSetupResult.Success, "Second setup should be successful");
        Assert.IsNotNull(firstSetupVersion, "Initial version should exist");
        Assert.IsNotNull(secondSetupVersion, "Version after second setup should exist");
        Assert.AreEqual(firstSetupVersion.Version, secondSetupVersion.Version, 
            "Version should not change when running setup again");
        
        // Check that duplicate version records are not created
        var executedMigrations = await _databaseService.GetExecutedVersionsAsync();
        var initialMigrations = executedMigrations.Where(m => m.Version == "Initial").ToList();
        Assert.AreEqual(1, initialMigrations.Count, 
            "There should be only one Initial migration record, no duplicates");
        
        Console.WriteLine("✅ Test 4: Second setup does not recreate existing database");
    }

    [TestCategory("Versions")]
    public async Task Test05_CheckDatabaseVersionAgain_ShouldReturnSameVersion()
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();
        var firstVersions = await _databaseService!.GetExecutedVersionsAsync();
        var firstVersionCheck = firstVersions.OrderByDescending(m => m.ExecutedAt).FirstOrDefault();

        // Act - wait a bit and check version again
        await Task.Delay(100);
        var secondVersions = await _databaseService.GetExecutedVersionsAsync();
        var secondVersionCheck = secondVersions.OrderByDescending(m => m.ExecutedAt).FirstOrDefault();

        // Assert
        Assert.IsNotNull(firstVersionCheck, "First version check should return result");
        Assert.IsNotNull(secondVersionCheck, "Second version check should return result");
        Assert.AreEqual(firstVersionCheck.Version, secondVersionCheck.Version, 
            "Versions should match on repeated checks");
        Assert.AreEqual(firstVersionCheck.ExecutedAt, secondVersionCheck.ExecutedAt, 
            "Migration execution time should match");
        
        Console.WriteLine($"✅ Test 5: Repeated version check returned same result: {secondVersionCheck.Version}");
    }

    [TestMethod]
    [TestCategory("Connection")]
    public async Task Test06_TestDatabaseConnection_ShouldBeSuccessful() 
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();

        // Act
        var connectionResult = await _databaseService!.TestConnectionAsync();

        // Assert
        Assert.IsTrue(connectionResult, "Database connection test should be successful");
        
        Console.WriteLine("✅ Test 6: Database connection successfully tested");
    }

    [TestMethod]
    [TestCategory("Migrations")]
    public async Task Test07_ListMigrations_ShouldShowAvailableAndExecutedMigrations()
    {
        // Arrange
        await _migrationService!.SetupDatabaseAsync();

        // Act
        var availableMigrations = _migrationService.GetAvailableMigrations();
        var executedMigrations = await _databaseService!.GetExecutedVersionsAsync();

        // Assert
        Assert.IsTrue(availableMigrations.Count > 0, "Available migrations should exist");
        Assert.IsTrue(executedMigrations.Count > 0, "Executed migrations should exist");
        
        var initialMigration = availableMigrations.FirstOrDefault(m => m.Version == "Initial");
        Assert.IsNotNull(initialMigration, "Initial migration should be in available list");
        
        var executedInitial = executedMigrations.FirstOrDefault(m => m.Version == "Initial");
        Assert.IsNotNull(executedInitial, "Initial migration should be in executed list");
        
        Console.WriteLine($"✅ Test 7: Found {availableMigrations.Count} available and {executedMigrations.Count} executed migrations");
    }

    [TestMethod]
    [TestCategory("Commands")]
    public async Task Test08_SetupDbInitial_ShouldExecuteOnlyInitialMigration()
    {
        // Arrange & Act - use SetupDatabaseAsync which is what setup-db-initial command calls
        var result = await _migrationService!.SetupDatabaseAsync();

        // Assert
        Assert.IsTrue(result.Success, $"setup-db-initial should be successful. Error: {result.Message}");
        
        var executedMigrations = await _databaseService!.GetExecutedVersionsAsync();
        
        // Should have only the Initial migration
        Assert.AreEqual(1, executedMigrations.Count, "Only Initial migration should be executed");
        Assert.AreEqual("Initial", executedMigrations.First().Version, "The executed migration should be Initial");
        
        // Verify database and table exist
        var dbExists = await _databaseService.EnsureDatabaseExistsAsync();
        Assert.IsTrue(dbExists, "Database should be created");
        
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users'";
        var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        Assert.AreEqual(1, tableCount, "Users table should be created");
        
        Console.WriteLine("✅ Test 8: setup-db-initial successfully executed only Initial migration");
    }

    [TestMethod]
    [TestCategory("Commands")]
    public async Task Test09_SetupDbLatest_ShouldExecuteInitialAndAllMigrations()
    {
        // Arrange
        var availableMigrations = _migrationService!.GetAvailableMigrations();
        var totalMigrationsCount = availableMigrations.Count;

        // Act - simulate setup-db-latest: first setup, then update to latest
        var setupResult = await _migrationService.SetupDatabaseAsync();
        Assert.IsTrue(setupResult.Success, "Initial setup should succeed");
        
        var updateResult = await _migrationService.UpdateToLatestAsync();
        Assert.IsTrue(updateResult.Success, $"Update to latest should succeed. Error: {updateResult.Message}");

        // Assert
        var executedMigrations = await _databaseService!.GetExecutedVersionsAsync();
        
        // Should have all available migrations executed
        Assert.AreEqual(totalMigrationsCount, executedMigrations.Count, 
            $"All {totalMigrationsCount} migrations should be executed");
        
        // Verify Initial migration is among executed
        var initialMigration = executedMigrations.FirstOrDefault(m => m.Version == "Initial");
        Assert.IsNotNull(initialMigration, "Initial migration should be executed");
        
        // Verify latest version
        var latestVersion = await _databaseService.GetLatestVersionAsync();
        Assert.IsNotNull(latestVersion, "Latest version should be available");
        
        // Verify that we have more than just Initial migration
        if (totalMigrationsCount > 1)
        {
            Assert.IsTrue(executedMigrations.Count > 1, 
                "More than one migration should be executed when using setup-db-latest");
        }
        
        Console.WriteLine($"✅ Test 9: setup-db-latest successfully executed all {executedMigrations.Count} migrations");
    }
}

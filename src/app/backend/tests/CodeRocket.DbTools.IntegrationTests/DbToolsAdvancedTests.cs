using CodeRocket.DbTools.IntegrationTests.Infrastructure;
using CodeRocket.DbTools.Services;
using MySqlConnector;

namespace CodeRocket.DbTools.IntegrationTests;

/// <summary>
/// Advanced integration tests for checking migrations and data operations
/// </summary>
[TestClass]
public class DbToolsAdvancedIntegrationTests
{
    private DockerMariaDbContainer? _mariaDbContainer;
    private DatabaseService? _databaseService;
    private MigrationService? _migrationService;
    private string? _connectionString;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _mariaDbContainer = new DockerMariaDbContainer();
        await _mariaDbContainer.StartAsync();
        
        _connectionString = _mariaDbContainer.ConnectionString;
        _databaseService = new DatabaseService(_connectionString);
        
        var migrationsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "CodeRocket.DbTools", "Migrations");
        _migrationService = new MigrationService(_databaseService, migrationsPath);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
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
    [TestCategory("Data")]
    public async Task Test08_CheckUsersTableCreation_ShouldCreateTableWithCorrectStructure()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check users table structure
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check that users table exists
        var tableExistsCommand = connection.CreateCommand();
        tableExistsCommand.CommandText = @"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users'";
        var tableExists = Convert.ToInt32(await tableExistsCommand.ExecuteScalarAsync());
        
        Assert.AreEqual(1, tableExists, "Users table should be created");

        // Check main columns
        var columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText = @"
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users'
            ORDER BY ORDINAL_POSITION";

        var columns = new List<(string Name, string Type, string Nullable)>();
        using var reader = await columnsCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add((
                reader.GetString("COLUMN_NAME"),
                reader.GetString("DATA_TYPE"),
                reader.GetString("IS_NULLABLE")
            ));
        }

        // Check presence of key columns
        Assert.IsTrue(columns.Any(c => c.Name == "Id" && c.Type == "int"), "Id column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "Email" && c.Type == "varchar"), "Email column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "Role" && c.Type == "int"), "Role column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "CreatedAt" && c.Type == "datetime"), "CreatedAt column should exist");

        Console.WriteLine("✅ Test 8: Users table created with correct structure");
    }

    [TestMethod]
    [TestCategory("Data")]
    public async Task Test09_CheckTestDataInsertion_ShouldInsertInitialUsers()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check that test data is inserted
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM users WHERE IsDeleted = FALSE";
        var userCount = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.IsTrue(userCount >= 2, "There should be at least 2 test users");

        // Check administrator
        var adminCommand = connection.CreateCommand();
        adminCommand.CommandText = "SELECT Email, Role FROM users WHERE Email = 'admin@coderocket.com'";
        using var adminReader = await adminCommand.ExecuteReaderAsync();
        
        Assert.IsTrue(await adminReader.ReadAsync(), "Administrator should exist");
        Assert.AreEqual("admin@coderocket.com", adminReader.GetString("Email"));
        Assert.AreEqual(4, adminReader.GetInt32("Role")); // SuperAdmin role

        Console.WriteLine($"✅ Test 9: Inserted {userCount} test users, including administrator");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public async Task Test10_DatabaseSetup_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _migrationService!.SetupDatabaseAsync();
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(result.Success, "Setup should be successful");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, 
            $"Setup should complete in less than 10 seconds, actual: {stopwatch.ElapsedMilliseconds}ms");

        Console.WriteLine($"✅ Test 10: Database setup completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    [TestCategory("Errors")]
    public async Task Test11_InvalidConnectionString_ShouldHandleGracefully()
    {
        // Arrange
        var invalidConnectionString = "Server=invalid_host;Port=3306;Database=test;User=root;Password=wrong;";
        var invalidDatabaseService = new DatabaseService(invalidConnectionString);

        // Act & Assert
        var connectionResult = await invalidDatabaseService.TestConnectionAsync();
        Assert.IsFalse(connectionResult, "Connection with invalid credentials should fail");

        Console.WriteLine("✅ Test 11: Invalid connection string handled correctly");
    }

    [TestMethod]
    [TestCategory("Indexes")]
    public async Task Test12_CheckIndexesCreation_ShouldCreateProperIndexes()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check indexes
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var indexesCommand = connection.CreateCommand();
        indexesCommand.CommandText = @"
            SELECT INDEX_NAME, COLUMN_NAME
            FROM INFORMATION_SCHEMA.STATISTICS 
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'users'
            AND INDEX_NAME != 'PRIMARY'
            ORDER BY INDEX_NAME, SEQ_IN_INDEX";

        var indexes = new List<(string IndexName, string ColumnName)>();
        using var reader = await indexesCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add((
                reader.GetString("INDEX_NAME"),
                reader.GetString("COLUMN_NAME")
            ));
        }

        // Check presence of key indexes
        Assert.IsTrue(indexes.Any(i => i.IndexName.Contains("email")), "There should be an index on Email");
        Assert.IsTrue(indexes.Any(i => i.IndexName.Contains("telegram")), "There should be an index on TelegramId");
        Assert.IsTrue(indexes.Any(i => i.IndexName.Contains("role")), "There should be an index on Role");

        Console.WriteLine($"✅ Test 12: Created {indexes.Count} indexes for query optimization");
    }

    [TestMethod]
    [TestCategory("Charset")]
    public async Task Test13_CheckDatabaseCharset_ShouldUseUtf8mb4()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check database charset
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var charsetCommand = connection.CreateCommand();
        charsetCommand.CommandText = @"
            SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME
            FROM INFORMATION_SCHEMA.SCHEMATA 
            WHERE SCHEMA_NAME = DATABASE()";

        using var reader = await charsetCommand.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync(), "Charset information should be retrieved");

        var charset = reader.GetString("DEFAULT_CHARACTER_SET_NAME");
        var collation = reader.GetString("DEFAULT_COLLATION_NAME");

        Assert.AreEqual("utf8mb4", charset, "Database should use UTF8MB4");
        Assert.IsTrue(collation.StartsWith("utf8mb4"), "Collation should be UTF8MB4");

        Console.WriteLine($"✅ Test 13: Database uses charset {charset} with collation {collation}");
    }

    [TestMethod]
    [TestCategory("Commands")]
    public async Task Test14_CompareSetupInitialVsLatest_ShouldShowDifference()
    {
        // This test demonstrates the difference between setup-db-initial and setup-db-latest
        
        // Arrange
        var availableMigrations = _migrationService!.GetAvailableMigrations();
        var totalAvailableMigrations = availableMigrations.Count;

        // Act 1: Simulate setup-db-initial
        var initialSetupResult = await _migrationService.SetupDatabaseAsync();
        var migrationsAfterInitial = await _databaseService!.GetExecutedVersionsAsync();
        var countAfterInitial = migrationsAfterInitial.Count;

        // Act 2: Simulate the "latest" part (update-to-latest)
        var updateToLatestResult = await _migrationService.UpdateToLatestAsync();
        var migrationsAfterLatest = await _databaseService.GetExecutedVersionsAsync();
        var countAfterLatest = migrationsAfterLatest.Count;

        // Assert
        Assert.IsTrue(initialSetupResult.Success, "Initial setup should succeed");
        Assert.IsTrue(updateToLatestResult.Success, "Update to latest should succeed");
        
        // setup-db-initial executes only Initial migration
        Assert.AreEqual(1, countAfterInitial, "setup-db-initial should execute only 1 migration (Initial)");
        
        // setup-db-latest = setup-db-initial + update-to-latest
        Assert.AreEqual(totalAvailableMigrations, countAfterLatest, 
            "setup-db-latest should execute all available migrations");
        
        // Verify the difference
        var additionalMigrations = countAfterLatest - countAfterInitial;
        
        Console.WriteLine("📊 Comparison Results:");
        Console.WriteLine($"   - setup-db-initial executed: {countAfterInitial} migration(s)");
        Console.WriteLine($"   - setup-db-latest executed: {countAfterLatest} migration(s)");
        Console.WriteLine($"   - Additional migrations in latest: {additionalMigrations}");
        Console.WriteLine($"✅ Test 14: Successfully demonstrated difference between commands");
        
        // Log executed migrations after setup-db-latest
        if (migrationsAfterLatest.Count > 0)
        {
            Console.WriteLine("\n   Migrations executed by setup-db-latest:");
            foreach (var migration in migrationsAfterLatest.OrderBy(m => m.ExecutedAt))
            {
                Console.WriteLine($"      - {migration.Version} (executed at {migration.ExecutedAt:yyyy-MM-dd HH:mm:ss})");
            }
        }
    }

    [TestMethod]
    [TestCategory("Commands")]
    public async Task Test15_SetupDbLatestIdempotency_ShouldHandleMultipleCalls()
    {
        // Arrange & Act - call setup-db-latest equivalent twice
        
        // First call
        await _migrationService!.SetupDatabaseAsync();
        await _migrationService.UpdateToLatestAsync();
        var migrationsAfterFirst = await _databaseService!.GetExecutedVersionsAsync();
        var countAfterFirst = migrationsAfterFirst.Count;
        
        // Second call (should be idempotent)
        await _migrationService.SetupDatabaseAsync();
        var updateResult = await _migrationService.UpdateToLatestAsync();
        var migrationsAfterSecond = await _databaseService.GetExecutedVersionsAsync();
        var countAfterSecond = migrationsAfterSecond.Count;

        // Assert
        Assert.IsTrue(updateResult.Success, "Second setup-db-latest should succeed");
        Assert.AreEqual(countAfterFirst, countAfterSecond, 
            "Migration count should remain the same after second execution");
        
        // Verify no duplicate migrations were created
        var groupedMigrations = migrationsAfterSecond.GroupBy(m => m.Version);
        foreach (var group in groupedMigrations)
        {
            Assert.AreEqual(1, group.Count(), 
                $"Migration {group.Key} should appear only once, found {group.Count()} times");
        }
        
        Console.WriteLine($"✅ Test 15: setup-db-latest is idempotent - {countAfterSecond} migrations, no duplicates");
    }
}


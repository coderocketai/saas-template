using CodeRocket.DbTools.IntegrationTests.Infrastructure;
using CodeRocket.DbTools.Services;
using Npgsql;

namespace CodeRocket.DbTools.IntegrationTests;

/// <summary>
/// Advanced integration tests for checking migrations and data operations
/// </summary>
[TestClass]
public class DbToolsAdvancedIntegrationTests
{
    private DockerPostgresContainer? _postgresContainer;
    private DatabaseService? _databaseService;
    private MigrationService? _migrationService;
    private string? _connectionString;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _postgresContainer = new DockerPostgresContainer();
        await _postgresContainer.StartAsync();
        
        _connectionString = _postgresContainer.ConnectionString;
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
        if (_postgresContainer != null)
        {
            try
            {
                await _postgresContainer.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning during cleanup: {ex.Message}");
            }
            finally
            {
                _postgresContainer.Dispose();
                _postgresContainer = null;
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
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check that users table exists
        var tableExistsCommand = connection.CreateCommand();
        tableExistsCommand.CommandText = @"
            SELECT COUNT(*) 
            FROM information_schema.tables 
            WHERE table_schema = 'public' AND table_name = 'users'";
        var tableExists = Convert.ToInt32(await tableExistsCommand.ExecuteScalarAsync());
        
        Assert.AreEqual(1, tableExists, "Users table should be created");

        // Check main columns
        var columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText = @"
            SELECT column_name, data_type, is_nullable
            FROM information_schema.columns 
            WHERE table_schema = 'public' AND table_name = 'users'
            ORDER BY ordinal_position";

        var columns = new List<(string Name, string Type, string Nullable)>();
        using var reader = await columnsCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }

        // Check presence of key columns (PostgreSQL uses lowercase names by default)
        Assert.IsTrue(columns.Any(c => c.Name == "id" && c.Type == "integer"), "id column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "email" && c.Type == "character varying"), "email column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "role" && c.Type == "integer"), "role column should exist");
        Assert.IsTrue(columns.Any(c => c.Name == "created_at" && c.Type.Contains("timestamp")), "created_at column should exist");

        Console.WriteLine("✅ Test 8: Users table created with correct structure");
    }

    [TestMethod]
    [TestCategory("Data")]
    public async Task Test09_CheckTestDataInsertion_ShouldInsertInitialUsers()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check that test data is inserted
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM users WHERE is_deleted = FALSE";
        var userCount = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.IsTrue(userCount >= 2, "There should be at least 2 test users");

        // Check administrator
        var adminCommand = connection.CreateCommand();
        adminCommand.CommandText = "SELECT email, role FROM users WHERE email = 'admin@coderocket.com'";
        using var adminReader = await adminCommand.ExecuteReaderAsync();
        
        Assert.IsTrue(await adminReader.ReadAsync(), "Administrator should exist");
        Assert.AreEqual("admin@coderocket.com", adminReader.GetString(0));
        Assert.AreEqual(4, adminReader.GetInt32(1)); // SuperAdmin role

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
        var invalidConnectionString = "Host=invalid_host;Port=5432;Database=test;Username=postgres;Password=wrong;";
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
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var indexesCommand = connection.CreateCommand();
        indexesCommand.CommandText = @"
            SELECT indexname, indexdef
            FROM pg_indexes 
            WHERE tablename = 'users' AND indexname NOT LIKE 'users_pkey%'
            ORDER BY indexname";

        var indexes = new List<string>();
        using var reader = await indexesCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add(reader.GetString(0));
        }

        // Check presence of key indexes
        Assert.IsTrue(indexes.Any(i => i.Contains("email")), "There should be an index on email");
        Assert.IsTrue(indexes.Any(i => i.Contains("telegram")), "There should be an index on telegram_id");
        Assert.IsTrue(indexes.Any(i => i.Contains("role")), "There should be an index on role");

        Console.WriteLine($"✅ Test 12: Created {indexes.Count} indexes for query optimization");
    }

    [TestMethod]
    [TestCategory("Encoding")]
    public async Task Test13_CheckDatabaseEncoding_ShouldUseUTF8()
    {
        // Arrange & Act
        await _migrationService!.SetupDatabaseAsync();

        // Assert - check database encoding in PostgreSQL
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var encodingCommand = connection.CreateCommand();
        encodingCommand.CommandText = @"
            SELECT datname, pg_encoding_to_char(encoding) as encoding
            FROM pg_database 
            WHERE datname = current_database()";

        using var reader = await encodingCommand.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync(), "Encoding information should be retrieved");

        var dbName = reader.GetString(0);
        var encoding = reader.GetString(1);

        // PostgreSQL uses UTF8 by default for new databases
        Assert.AreEqual("UTF8", encoding, "Database should use UTF8 encoding");

        Console.WriteLine($"✅ Test 13: Database '{dbName}' uses encoding {encoding}");
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


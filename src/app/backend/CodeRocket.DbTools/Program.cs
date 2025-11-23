using Microsoft.Extensions.Configuration;
using CodeRocket.DbTools.Services;

namespace CodeRocket.DbTools;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("CodeRocket Database Tools");
        Console.WriteLine("========================");

        try
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("❌ Connection string not found. Check appsettings.json or environment variables.");
                return 1;
            }

            var databaseService = new DatabaseService(connectionString);
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");
            var migrationService = new MigrationService(databaseService, migrationsPath);

            // Parse command line arguments
            if (args.Length == 0)
            {
                ShowHelp();
                return 0;
            }

            var command = args[0].ToLowerInvariant();
            
            switch (command)
            {
                case "setup-db-latest":
                    return await SetupDatabaseLatestCommand(migrationService);
                
                case "setup-db-initial":
                    return await SetupDatabaseInitialCommand(migrationService);
                
                case "get-db-version":
                    return await GetDatabaseVersionCommand(databaseService);
                
                case "update-to-version":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("❌ Version number required. Usage: update-to-version 1.0.1");
                        return 1;
                    }
                    return await UpdateToVersionCommand(migrationService, args[1]);
                
                case "update-to-latest":
                    return await UpdateToLatestCommand(migrationService);
                
                case "list-migrations":
                    return await ListMigrationsCommand(migrationService, databaseService);
                
                case "test-connection":
                    return await TestConnectionCommand(databaseService);
                
                default:
                    Console.WriteLine($"❌ Unknown command: {command}");
                    ShowHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("  setup-db-latest    - Initialize database and update to the latest version");
        Console.WriteLine("  setup-db-initial   - Initialize database with Initial migration only");
        Console.WriteLine("  get-db-version     - Get current database version");
        Console.WriteLine("  update-to-version <version> - Update database to specific version (e.g., 1.0.1)");
        Console.WriteLine("  update-to-latest   - Update database to latest available version");
        Console.WriteLine("  list-migrations    - List all available and executed migrations");
        Console.WriteLine("  test-connection    - Test database connection");
    }

    static async Task<int> SetupDatabaseLatestCommand(MigrationService migrationService)
    {
        Console.WriteLine("🚀 Setting up database with latest migrations...\n");
        
        // First, setup the database with initial migration
        var setupResult = await migrationService.SetupDatabaseAsync();
        
        if (!setupResult.Success)
        {
            Console.WriteLine($"❌ Setup failed: {setupResult.Message}");
            return 1;
        }
        
        Console.WriteLine($"✅ {setupResult.Message}");
        if (setupResult.ExecutedScripts.Count > 0)
        {
            Console.WriteLine("\nExecuted setup scripts:");
            foreach (var script in setupResult.ExecutedScripts)
            {
                Console.WriteLine($"  - {script}");
            }
        }
        
        Console.WriteLine("\n🔄 Updating to latest version...\n");
        
        // Then, update to latest
        var updateResult = await migrationService.UpdateToLatestAsync();
        if (!updateResult.Success)
        {
            Console.WriteLine($"❌ Update failed: {updateResult.Message}");
            return 1;
        }
            
        Console.WriteLine($"✅ {updateResult.Message}");
        if (updateResult.ExecutedScripts.Count > 0)
        {
            Console.WriteLine("\nExecuted migration scripts:");
            foreach (var script in updateResult.ExecutedScripts)
            {
                Console.WriteLine($"  - {script}");
            }
        }
        return 0;
    }

    static async Task<int> SetupDatabaseInitialCommand(MigrationService migrationService)
    {
        Console.WriteLine("🔧 Setting up database with initial migration only...\n");
        
        var result = await migrationService.SetupDatabaseAsync();
        
        if (result.Success)
        {
            Console.WriteLine($"✅ {result.Message}");
            if (result.ExecutedScripts.Count > 0)
            {
                Console.WriteLine("\nExecuted scripts:");
                foreach (var script in result.ExecutedScripts)
                {
                    Console.WriteLine($"  - {script}");
                }
            }
            return 0;
        }
        else
        {
            Console.WriteLine($"❌ {result.Message}");
            return 1;
        }
    }

    static async Task<int> GetDatabaseVersionCommand(DatabaseService databaseService)
    {
        Console.WriteLine("📋 Getting current database version...\n");
        
        if (!await databaseService.TestConnectionAsync())
        {
            Console.WriteLine("❌ Cannot connect to database");
            return 1;
        }

        var version = await databaseService.GetLatestVersionAsync();
        
        if (version != null)
        {
            Console.WriteLine($"Current database version: {version}");
            return 0;
        }
        else
        {
            Console.WriteLine("Database not initialized or no migrations executed");
            return 1;
        }
    }

    static async Task<int> UpdateToVersionCommand(MigrationService migrationService, string targetVersion)
    {
        Console.WriteLine($"🔄 Updating database to version {targetVersion}...\n");
        
        var result = await migrationService.UpdateToVersionAsync(targetVersion);
        
        if (result.Success)
        {
            Console.WriteLine($"✅ {result.Message}");
            if (result.ExecutedScripts.Count > 0)
            {
                Console.WriteLine("\nExecuted scripts:");
                foreach (var script in result.ExecutedScripts)
                {
                    Console.WriteLine($"  - {script}");
                }
            }
            return 0;
        }
        else
        {
            Console.WriteLine($"❌ {result.Message}");
            return 1;
        }
    }

    static async Task<int> UpdateToLatestCommand(MigrationService migrationService)
    {
        Console.WriteLine("🔄 Updating database to latest version...\n");
        
        var result = await migrationService.UpdateToLatestAsync();
        
        if (result.Success)
        {
            Console.WriteLine($"✅ {result.Message}");
            if (result.ExecutedScripts.Count > 0)
            {
                Console.WriteLine("\nExecuted scripts:");
                foreach (var script in result.ExecutedScripts)
                {
                    Console.WriteLine($"  - {script}");
                }
            }
            return 0;
        }
        else
        {
            Console.WriteLine($"❌ {result.Message}");
            return 1;
        }
    }

    static async Task<int> ListMigrationsCommand(MigrationService migrationService, DatabaseService databaseService)
    {
        Console.WriteLine("📋 Listing migrations...\n");
        
        try
        {
            var availableMigrations = migrationService.GetAvailableMigrations();
            var executedVersions = await databaseService.GetExecutedVersionsAsync();
            var executedVersionStrings = executedVersions.Select(v => v.Version).ToHashSet();

            Console.WriteLine("Available migrations:");
            foreach (var migration in availableMigrations)
            {
                var status = executedVersionStrings.Contains(migration.Version) ? "✅ Executed" : "⏳ Pending";
                var executedInfo = "";
                
                if (executedVersionStrings.Contains(migration.Version))
                {
                    var executedVersion = executedVersions.First(v => v.Version == migration.Version);
                    executedInfo = $" (executed: {executedVersion.ExecutedAt:yyyy-MM-dd HH:mm:ss})";
                }
                
                Console.WriteLine($"  {migration.Version} - {status}{executedInfo}");
                
                if (!string.IsNullOrEmpty(migration.Description))
                {
                    Console.WriteLine($"    Description: {migration.Description}");
                }
                
                Console.WriteLine($"    Scripts: {string.Join(", ", migration.SqlFiles.Select(Path.GetFileName))}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing migrations: {ex.Message}");
            return 1;
        }
    }

    static async Task<int> TestConnectionCommand(DatabaseService databaseService)
    {
        Console.WriteLine("🔗 Testing database connection...\n");
        
        if (await databaseService.TestConnectionAsync())
        {
            Console.WriteLine("✅ Database connection successful");
            return 0;
        }
        else
        {
            Console.WriteLine("❌ Database connection failed");
            return 1;
        }
    }
}

using CodeRocket.DbTools.Models;

namespace CodeRocket.DbTools.Services;

/// <summary>
/// Service for managing database migrations with version control
/// </summary>
public class MigrationService
{
    private readonly DatabaseService _databaseService;
    private readonly string _migrationsPath;

    public MigrationService(DatabaseService databaseService, string migrationsPath)
    {
        _databaseService = databaseService;
        _migrationsPath = migrationsPath;
    }

    /// <summary>
    /// Get all available migrations from file system
    /// </summary>
    public List<Migration> GetAvailableMigrations()
    {
        var migrations = new List<Migration>();

        if (!Directory.Exists(_migrationsPath))
        {
            Console.WriteLine($"Migrations directory not found: {_migrationsPath}");
            return migrations;
        }

        var directories = Directory.GetDirectories(_migrationsPath);
        
        foreach (var directory in directories)
        {
            var folderName = Path.GetFileName(directory);
            var sqlFiles = Directory.GetFiles(directory, "*.sql")
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            if (sqlFiles.Count > 0)
            {
                migrations.Add(new Migration
                {
                    Version = folderName,
                    FolderPath = directory,
                    SqlFiles = sqlFiles,
                    Description = GetMigrationDescription(directory)
                });
            }
        }

        // Sort migrations by version (Initial first, then by version number)
        return migrations.OrderBy(m => m.Version == MigrationConstants.Initial ? 0 : 1)
                        .ThenBy(m => m.GetParsedVersion())
                        .ToList();
    }

    /// <summary>
    /// Get migrations that need to be executed to reach target version
    /// </summary>
    public async Task<List<Migration>> GetPendingMigrationsAsync(string? targetVersion = null)
    {
        var allMigrations = GetAvailableMigrations();
        var executedVersions = await _databaseService.GetExecutedVersionsAsync();
        var executedVersionStrings = executedVersions.Select(v => v.Version).ToHashSet();

        var pendingMigrations = allMigrations
            .Where(m => !executedVersionStrings.Contains(m.Version))
            .ToList();

        if (!string.IsNullOrEmpty(targetVersion))
        {
            var targetVersionParsed = targetVersion == MigrationConstants.Initial
                ? new Version(0, 0, 0)
                : Version.Parse(targetVersion);

            pendingMigrations = pendingMigrations
                .Where(m => m.GetParsedVersion() <= targetVersionParsed)
                .ToList();
        }

        return pendingMigrations;
    }

    /// <summary>
    /// Execute initial database setup
    /// </summary>
    public async Task<MigrationResult> SetupDatabaseAsync()
    {
        var result = new MigrationResult();

        try
        {
            Console.WriteLine("Setting up database...");

            // Ensure database exists
            if (!await _databaseService.EnsureDatabaseExistsAsync())
            {
                result.Message = "Failed to create database";
                return result;
            }

            // Initialize version table
            if (!await _databaseService.InitializeVersionTableAsync())
            {
                result.Message = "Failed to initialize version table";
                return result;
            }

            // Check if Initial migration already executed
            var latestVersion = await _databaseService.GetLatestVersionAsync();
            if (latestVersion != null)
            {
                result.Success = true;
                result.Message = $"Database already initialized. Current version: {latestVersion}";
                return result;
            }

            // Execute Initial migration
            var initialMigration = GetAvailableMigrations()
                .FirstOrDefault(m => m.Version == MigrationConstants.Initial);

            if (initialMigration == null)
            {
                result.Message = "Initial migration not found in Migrations/Initial folder";
                return result;
            }

            var migrationResult = await ExecuteMigrationAsync(initialMigration);
            if (!migrationResult.Success)
            {
                result.Message = $"Failed to execute initial migration: {migrationResult.Message}";
                return result;
            }

            result.Success = true;
            result.Message = "Database setup completed successfully";
            result.ExecutedScripts = migrationResult.ExecutedScripts;

            Console.WriteLine("✓ Database setup completed successfully");
        }
        catch (Exception ex)
        {
            result.Message = $"Setup failed: {ex.Message}";
            result.Exception = ex;
            Console.WriteLine($"✗ Setup failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Update database to specific version
    /// </summary>
    public async Task<MigrationResult> UpdateToVersionAsync(string targetVersion)
    {
        var result = new MigrationResult();

        try
        {
            Console.WriteLine($"Updating database to version {targetVersion}...");

            var pendingMigrations = await GetPendingMigrationsAsync(targetVersion);
            
            if (pendingMigrations.Count == 0)
            {
                result.Success = true;
                result.Message = $"Database is already at version {targetVersion} or higher";
                Console.WriteLine("✓ No migrations needed");
                return result;
            }

            Console.WriteLine($"Found {pendingMigrations.Count} migration(s) to execute:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  - {migration.Version}");
            }

            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"\nExecuting migration {migration.Version}...");
                var migrationResult = await ExecuteMigrationAsync(migration);
                
                if (!migrationResult.Success)
                {
                    result.Message = $"Failed at migration {migration.Version}: {migrationResult.Message}";
                    result.Exception = migrationResult.Exception;
                    return result;
                }

                result.ExecutedScripts.AddRange(migrationResult.ExecutedScripts);
                Console.WriteLine($"✓ Migration {migration.Version} completed");
            }

            result.Success = true;
            result.Message = $"Successfully updated to version {targetVersion}";
            Console.WriteLine($"\n✓ Database updated to version {targetVersion}");
        }
        catch (Exception ex)
        {
            result.Message = $"Update failed: {ex.Message}";
            result.Exception = ex;
            Console.WriteLine($"✗ Update failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Update database to latest available version
    /// </summary>
    public async Task<MigrationResult> UpdateToLatestAsync()
    {
        var allMigrations = GetAvailableMigrations();
        var latestMigration = allMigrations
            .Where(m => m.Version != MigrationConstants.Initial)
             .OrderByDescending(m => m.GetParsedVersion())
             .FirstOrDefault();

        if (latestMigration == null)
        {
            var result = new MigrationResult
            {
                Success = true,
                Message = "No versioned migrations found"
            };
            Console.WriteLine("✓ No versioned migrations found");
            return result;
        }

        Console.WriteLine($"Latest available version: {latestMigration.Version}");
        return await UpdateToVersionAsync(latestMigration.Version);
    }

    /// <summary>
    /// Execute a single migration
    /// </summary>
    private async Task<MigrationResult> ExecuteMigrationAsync(Migration migration)
    {
        var result = new MigrationResult();

        try
        {
            foreach (var sqlFile in migration.SqlFiles)
            {
                Console.WriteLine($"  Executing: {Path.GetFileName(sqlFile)}");
                
                var sqlContent = await File.ReadAllTextAsync(sqlFile);
                
                if (!await _databaseService.ExecuteSqlScriptAsync(sqlContent))
                {
                    result.Message = $"Failed to execute script: {Path.GetFileName(sqlFile)}";
                    return result;
                }

                result.ExecutedScripts.Add(Path.GetFileName(sqlFile));
            }

            // Record migration as executed
            if (!await _databaseService.RecordMigrationAsync(migration.Version, migration.Description))
            {
                result.Message = "Failed to record migration in db_versions table";
                return result;
            }

            result.Success = true;
            result.Message = $"Migration {migration.Version} executed successfully";
        }
        catch (Exception ex)
        {
            result.Message = $"Migration execution failed: {ex.Message}";
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// Read migration description from README.md or description.txt if exists
    /// </summary>
    private string? GetMigrationDescription(string migrationFolder)
    {
        var readmeFile = Path.Combine(migrationFolder, "README.md");
        var descFile = Path.Combine(migrationFolder, "description.txt");

        if (File.Exists(readmeFile))
        {
            var content = File.ReadAllText(readmeFile);
            return content.Length > 500 ? content.Substring(0, 500) + "..." : content;
        }

        if (File.Exists(descFile))
        {
            var content = File.ReadAllText(descFile);
            return content.Length > 500 ? content.Substring(0, 500) + "..." : content;
        }

        return null;
    }
}

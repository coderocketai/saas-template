using MySqlConnector;
using System.Data;
using CodeRocket.DbTools.Models;

namespace CodeRocket.DbTools.Services;

/// <summary>
/// Database service for managing migrations using pure ADO.NET
/// </summary>
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Create database if it doesn't exist
    /// </summary>
    public async Task<bool> EnsureDatabaseExistsAsync()
    {
        try
        {
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.Database;
            builder.Database = "";

            using var connection = new MySqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            var createDbCommand = connection.CreateCommand();
            createDbCommand.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
            await createDbCommand.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating database: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initialize db_versions table if it doesn't exist
    /// </summary>
    public async Task<bool> InitializeVersionTableAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS db_versions (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    version VARCHAR(50) NOT NULL,
                    executed_at DATETIME NOT NULL,
                    description TEXT
                ) ENGINE=InnoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";

            await createTableCommand.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing version table: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the latest executed migration version
    /// </summary>
    public async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT version FROM db_versions ORDER BY executed_at DESC LIMIT 1";

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting latest version: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all executed versions
    /// </summary>
    public async Task<List<DatabaseVersion>> GetExecutedVersionsAsync()
    {
        var versions = new List<DatabaseVersion>();

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT id, version, executed_at, description FROM db_versions ORDER BY executed_at";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                versions.Add(new DatabaseVersion
                {
                    Id = reader.GetInt32("id"),
                    Version = reader.GetString("version"),
                    ExecutedAt = reader.GetDateTime("executed_at"),
                    Description = reader.IsDBNull("description") ? null : reader.GetString("description")
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting executed versions: {ex.Message}");
        }

        return versions;
    }

    /// <summary>
    /// Record a migration as executed
    /// </summary>
    public async Task<bool> RecordMigrationAsync(string version, string? description = null)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO db_versions (version, executed_at, description) VALUES (@version, @executed_at, @description)";
            command.Parameters.AddWithValue("@version", version);
            command.Parameters.AddWithValue("@executed_at", DateTime.UtcNow);
            command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recording migration: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Execute SQL script
    /// </summary>
    public async Task<bool> ExecuteSqlScriptAsync(string sqlContent)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Split by semicolon and execute each statement separately
            var statements = sqlContent.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var statement in statements)
            {
                var trimmedStatement = statement.Trim();
                if (string.IsNullOrWhiteSpace(trimmedStatement))
                    continue;

                var command = connection.CreateCommand();
                command.CommandText = trimmedStatement;
                await command.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing SQL script: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test database connection
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }
}

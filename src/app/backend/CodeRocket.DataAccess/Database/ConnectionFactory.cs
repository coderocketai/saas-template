using System.Data;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace CodeRocket.DataAccess.Database;

/// <summary>
/// Database connection factory for MariaDB
/// </summary>
public class ConnectionFactory
{
    /// <summary>
    /// Connection string name in configuration
    /// </summary>
    private static readonly string ConnectionStringName = "DbConnection";
    
    private readonly string _connectionString;

    public ConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString(ConnectionStringName) 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'MainConnection' not found");
    }
    
    /// <summary>
    /// Create new database connection
    /// </summary>
    /// <returns>Database connection instance</returns>
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    /// <summary>
    /// Create and open database connection
    /// </summary>
    /// <returns>Opened database connection instance</returns>
    public async Task<IDbConnection> CreateOpenConnectionAsync()
    {
        var connection = CreateConnection();
        await ((MySqlConnection)connection).OpenAsync();
        return connection;
    }
}

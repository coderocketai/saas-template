using Docker.DotNet;
using Docker.DotNet.Models;
using MySqlConnector;
using System.Net;

namespace CodeRocket.DbTools.IntegrationTests.Infrastructure;

/// <summary>
/// Manages Docker container with MariaDB for integration tests
/// </summary>
public class DockerMariaDbContainer : IDisposable
{
    /// <summary>
    /// MariaDb Docker Image 
    /// </summary>
    /// <remarks>
    /// Run command 'docker pull mariadb:11.8.3' before tests execution.
    /// </remarks> 
    private readonly string _mariaDbImageName = "mariadb:11.8.3";
    private readonly string _rootPassword = "test_password_123";
    private readonly string _databaseName = "coderocket_test_db";

    private readonly DockerClient _dockerClient;
    private string? _containerId;
    private readonly string _containerName;
    private readonly int _port;
    private bool _disposed;

    public string ConnectionString => $"Server=localhost;Port={_port};Database={_databaseName};User=root;Password={_rootPassword};";

    public DockerMariaDbContainer()
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _containerName = $"mariadb_test_{Guid.NewGuid():N}";
        _port = GetAvailablePort();
    }

    /// <summary>
    /// Starts MariaDB container
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            // Create container
            var createResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Name = _containerName,
                Image = _mariaDbImageName,
                Env = new[]
                {
                    $"MYSQL_ROOT_PASSWORD={_rootPassword}",
                    $"MYSQL_DATABASE={_databaseName}",
                    "MYSQL_CHARSET=utf8mb4",
                    "MYSQL_COLLATION=utf8mb4_unicode_ci"
                },
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    ["3306/tcp"] = new EmptyStruct()
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        ["3306/tcp"] = new List<PortBinding>
                        {
                            new() { HostPort = _port.ToString() }
                        }
                    },
                    AutoRemove = true
                }
            });

            _containerId = createResponse.ID;

            // Start container
            await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

            // Wait for database readiness
            await WaitForDatabaseReadyAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start MariaDB container: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Stops and removes container
    /// </summary>
    public async Task StopAsync()
    {
        if (_containerId == null)
            return;

        try
        {
            // Check if container exists and is running
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true
            });
            
            var container = containers.FirstOrDefault(c => c.ID == _containerId);
            if (container == null)
                return;
                
            // Stop container if it's running
            if (container.State == "running")
            {
                await _dockerClient.Containers.StopContainerAsync(_containerId, new ContainerStopParameters
                {
                    WaitBeforeKillSeconds = 5
                });
            }
                
            // Remove container if it exists and is not being removed
            if (container.State != "removing")
            {
                await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters
                {
                    Force = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping container: {ex.Message}");
        }
        finally
        {
            _containerId = null;
        }
    }

    /// <summary>
    /// Waits for database readiness for connection
    /// </summary>
    private async Task WaitForDatabaseReadyAsync()
    {
        var maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                Console.WriteLine($"MariaDB container is ready on port {_port}");
                return;
            }
            catch (Exception)
            {
                if (i == maxAttempts - 1)
                    throw new TimeoutException("MariaDB container is not ready within the specified time");

                await Task.Delay(delay);
            }
        }
    }

    /// <summary>
    /// Finds available port for MariaDB
    /// </summary>
    private static int GetAvailablePort()
    {
        using var socket = new System.Net.Sockets.Socket(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);

        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        return port;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().GetAwaiter().GetResult();
            _dockerClient.Dispose();
            _disposed = true;
        }
    }
}

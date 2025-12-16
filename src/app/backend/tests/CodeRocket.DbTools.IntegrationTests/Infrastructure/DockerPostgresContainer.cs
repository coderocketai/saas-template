using Docker.DotNet;
using Docker.DotNet.Models;
using Npgsql;
using System.Net;

namespace CodeRocket.DbTools.IntegrationTests.Infrastructure;

/// <summary>
/// Manages Docker container with PostgreSQL for integration tests
/// </summary>
public class DockerPostgresContainer : IDisposable
{
    /// <summary>
    /// PostgreSQL Docker Image 
    /// </summary>
    /// <remarks>
    /// Run command 'docker pull postgres:18.1' before tests execution.
    /// </remarks> 
    private readonly string _postgresImageName = "postgres:18.1";
    private readonly string _password = "test_password_123";
    private readonly string _databaseName = "coderocket_test_db";
    private readonly string _username = "postgres";

    private readonly DockerClient _dockerClient;
    private string? _containerId;
    private readonly string _containerName;
    private readonly int _port;
    private bool _disposed;

    public string ConnectionString => $"Host=localhost;Port={_port};Database={_databaseName};Username={_username};Password={_password};";

    public DockerPostgresContainer()
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _containerName = $"postgres_test_{Guid.NewGuid():N}";
        _port = GetAvailablePort();
    }

    /// <summary>
    /// Starts PostgreSQL container
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            // Create container
            var createResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Name = _containerName,
                Image = _postgresImageName,
                Env = new[]
                {
                    $"POSTGRES_PASSWORD={_password}",
                    $"POSTGRES_DB={_databaseName}",
                    $"POSTGRES_USER={_username}"
                },
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    ["5432/tcp"] = new EmptyStruct()
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        ["5432/tcp"] = new List<PortBinding>
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
            throw new InvalidOperationException($"Failed to start PostgreSQL container: {ex.Message}", ex);
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
                using var connection = new NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();
                Console.WriteLine($"PostgreSQL container is ready on port {_port}");
                return;
            }
            catch (Exception)
            {
                if (i == maxAttempts - 1)
                    throw new TimeoutException("PostgreSQL container is not ready within the specified time");

                await Task.Delay(delay);
            }
        }
    }

    /// <summary>
    /// Finds available port for PostgreSQL
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

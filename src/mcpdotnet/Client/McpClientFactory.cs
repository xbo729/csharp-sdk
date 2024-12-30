// Client/McpClientFactory.cs
namespace McpDotNet.Client;

using McpDotNet.Configuration;
using McpDotNet.Protocol.Transport;

/// <summary>
/// Factory for creating MCP clients based on configuration.
/// </summary>
public class McpClientFactory
{
    private readonly Dictionary<string, McpServerConfig> _serverConfigs;
    private readonly McpClientOptions _clientOptions;
    private readonly Dictionary<string, IMcpClient> _clients = new();
    private readonly Func<McpServerConfig, IMcpTransport> _transportFactoryMethod;
    private readonly Func<IMcpTransport, McpClientOptions, IMcpClient> _clientFactoryMethod;

    public McpClientFactory(
        IEnumerable<McpServerConfig> serverConfigs,
        McpClientOptions clientOptions,
        Func<McpServerConfig, IMcpTransport>? transportFactoryMethod = null,
        Func<IMcpTransport, McpClientOptions, IMcpClient>? clientFactoryMethod = null)
    {
        _serverConfigs = serverConfigs.ToDictionary(c => c.Id);
        _clientOptions = clientOptions;
        _transportFactoryMethod = transportFactoryMethod ?? CreateTransport;
        _clientFactoryMethod = clientFactoryMethod ?? ((transport, options) => new McpClient(transport, options));
    }

    /// <summary>
    /// Gets or creates a client for the specified server.
    /// </summary>
    public async Task<IMcpClient> GetClientAsync(string serverId, CancellationToken cancellationToken = default)
    {
        if (!_serverConfigs.TryGetValue(serverId, out var config))
        {
            throw new ArgumentException($"Server with ID '{serverId}' not found.", nameof(serverId));
        }

        if (_clients.TryGetValue(serverId, out var existingClient))
        {
            return existingClient;
        }

        var transport = _transportFactoryMethod(config);
        var client = _clientFactoryMethod(transport, _clientOptions);
        await client.ConnectAsync(cancellationToken);

        _clients[serverId] = client;
        return client;
    }

    private IMcpTransport CreateTransport(McpServerConfig config)
    {
        return config.TransportType.ToLowerInvariant() switch
        {
            "stdio" => new StdioTransport(new StdioTransportOptions
            {
                Command = GetCommand(config),
                Arguments = config.TransportOptions?.GetValueOrDefault("arguments")?.Split(' '),
                WorkingDirectory = config.TransportOptions?.GetValueOrDefault("workingDirectory"),
                EnvironmentVariables = config.TransportOptions?
                    .Where(kv => kv.Key.StartsWith("env:"))
                    .ToDictionary(kv => kv.Key.Substring(4), kv => kv.Value)
            }),
            // Add other transport types here
            _ => throw new ArgumentException($"Unsupported transport type '{config.TransportType}'.", nameof(config))
        };
    }

    private string GetCommand(McpServerConfig config)
    {
        if (config.TransportOptions == null)
        {
            return config.Location!;
        }

        var command = config.TransportOptions.GetValueOrDefault("command");
        if (string.IsNullOrEmpty(command))
        {
            return config.Location!;
        }

        if (config.TransportOptions.ContainsKey("arguments"))
        {
            var arguments = config.TransportOptions.GetValueOrDefault("arguments");
            config.TransportOptions["arguments"] = $"/c {command} {arguments}";
        }
        else
        {
            config.TransportOptions["arguments"] = $"/c {command}";
        }
        return $"cmd.exe";
    }
}
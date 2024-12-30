namespace McpDotNet.Client;

using McpDotNet.Protocol.Types;
using McpDotNet.Protocol.Messages;

/// <summary>
/// Represents an instance of the MCP client connecting to a specific server.
/// </summary>
public interface IMcpClient : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the client has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the capabilities that the server supports.
    /// </summary>
    ServerCapabilities? ServerCapabilities { get; }

    /// <summary>
    /// Gets the version information of the server.
    /// </summary>
    Implementation? ServerInfo { get; }

    /// <summary>
    /// Connects to the server.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A simple ping to the server to check if it is alive. Task completes when the ping is successful.
    /// </summary>
    Task PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the server for a list of tools that are available (if any)
    /// </summary>
    Task<ListToolsResponse> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls a tool on the server.
    /// </summary>
    /// <param name="toolName">The name of the tool as given when listing tool definitions.</param>
    /// <param name="arguments">Arguments for the tool (if any)</param>
    Task<CallToolResponse> CallToolAsync(string toolName, Dictionary<string, object>? arguments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// A generic method to send a request to the server and get a response.
    /// </summary>
    Task<T> SendRequestAsync<T>(JsonRpcRequest request, CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Registers a handler for a specific notification method.
    /// </summary>
    void OnNotification(string method, Func<JsonRpcNotification, Task> handler);
}

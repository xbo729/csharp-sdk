// Client/McpClientOptions.cs
namespace McpDotNet.Client;

/// <summary>
/// Represents errors that occur in the MCP client.
/// </summary>
public class McpClientException : Exception
{
    /// <summary>
    /// Gets the error code if this exception was caused by a JSON-RPC error response.
    /// </summary>
    public int? ErrorCode { get; }

    public McpClientException(string message) : base(message)
    {
    }

    public McpClientException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public McpClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
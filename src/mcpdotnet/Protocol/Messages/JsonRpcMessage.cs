namespace McpDotNet.Protocol.Messages;

using System.Text.Json.Serialization;

/// <summary>
/// Base interface for all JSON-RPC messages in the MCP protocol.
/// </summary>
public interface IJsonRpcMessage
{
    /// <summary>
    /// JSON-RPC protocol version. Must be "2.0".
    /// </summary>
    string JsonRpc { get; }
}

/// <summary>
/// Base interface for JSON-RPC messages that include an ID.
/// </summary>
public interface IJsonRpcMessageWithId : IJsonRpcMessage
{
    /// <summary>
    /// The message identifier.
    /// </summary>
    RequestId Id { get; }
}

/// <summary>
/// A request message in the JSON-RPC protocol.
/// </summary>
public record JsonRpcRequest : IJsonRpcMessageWithId
{
    /// <summary>
    /// JSON-RPC protocol version. Always "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    /// <summary>
    /// Request identifier. Must be a string or number and unique within the session.
    /// </summary>
    [JsonPropertyName("id")]
    public RequestId Id { get; set; }

    /// <summary>
    /// Name of the method to invoke.
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>
    /// Optional parameters for the method.
    /// </summary>
    [JsonPropertyName("params")]
    public object? Params { get; init; }
}

/// <summary>
/// A notification message in the JSON-RPC protocol (a request that doesn't expect a response).
/// </summary>
public record JsonRpcNotification : IJsonRpcMessage
{
    /// <summary>
    /// JSON-RPC protocol version. Always "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    /// <summary>
    /// Name of the notification method.
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>
    /// Optional parameters for the notification.
    /// </summary>
    [JsonPropertyName("params")]
    public object? Params { get; init; }
}

/// <summary>
/// A successful response message in the JSON-RPC protocol.
/// </summary>
public record JsonRpcResponse : IJsonRpcMessageWithId
{
    /// <summary>
    /// JSON-RPC protocol version. Always "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    /// <summary>
    /// Request identifier matching the original request.
    /// </summary>
    [JsonPropertyName("id")]
    public required RequestId Id { get; init; }

    /// <summary>
    /// The result of the method invocation.
    /// </summary>
    [JsonPropertyName("result")]
    public required object Result { get; init; }
}

/// <summary>
/// An error response message in the JSON-RPC protocol.
/// </summary>
public record JsonRpcError : IJsonRpcMessageWithId
{
    /// <summary>
    /// JSON-RPC protocol version. Always "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    /// <summary>
    /// Request identifier matching the original request.
    /// </summary>
    [JsonPropertyName("id")]
    public required RequestId Id { get; init; }

    /// <summary>
    /// Error information.
    /// </summary>
    [JsonPropertyName("error")]
    public required JsonRpcErrorDetail Error { get; init; }
}

/// <summary>
/// Detailed error information for JSON-RPC error responses.
/// </summary>
public record JsonRpcErrorDetail
{
    /// <summary>
    /// Integer error code.
    /// </summary>
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    /// <summary>
    /// Short description of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// Optional additional error data.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; init; }
}
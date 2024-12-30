namespace McpDotNet.Protocol.Types;

using System.Text.Json.Serialization;

/// <summary>
/// Result of the initialization request sent to the server.
/// </summary>
public record InitializeResult
{
    /// <summary>
    /// The version of the Model Context Protocol that the server wants to use.
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public required string ProtocolVersion { get; init; }

    /// <summary>
    /// The server's capabilities.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public required ServerCapabilities Capabilities { get; init; }

    /// <summary>
    /// Information about the server implementation.
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public required Implementation ServerInfo { get; init; }

    /// <summary>
    /// Optional instructions for using the server and its features.
    /// </summary>
    [JsonPropertyName("instructions")]
    public string? Instructions { get; init; }
}

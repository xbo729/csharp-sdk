// Features/Tools/ToolTypes.cs
namespace mcpdotnet.Features.Tools;

/// <summary>
/// Represents a tool available from an MCP server.
/// </summary>
public record ToolDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Dictionary<string, ToolParameter> Parameters { get; init; }
}


/// <summary>
/// Represents a tool parameter.
/// </summary>
public record ToolParameter
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }
    public bool Required { get; init; }
    public string[]? AllowedValues { get; init; }
}

/// <summary>
/// Result of a tool execution, from server to client
public record McpToolCallResponse
{
    // TODO
}

/// <summary>
/// Result of a tool execution, from client to host.
/// </summary>
public record ToolResult
{
    public required bool Succeeded { get; init; }
    public List<ToolContent>? Content { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Content returned by a tool execution.
/// </summary>
public record ToolContent
{
    /// <summary>
    /// The type of content.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The content value.
    /// </summary>
    public required object Value { get; init; }

    /// <summary>
    /// Optional MIME type for binary content.
    /// </summary>
    public string? MimeType { get; init; }
}

/// <summary>
/// A list of tools available from an MCP server.
/// </summary>
public record ToolDefinitionList
{
    /// <summary>
    /// The list of tools.
    /// </summary>
    public required List<ToolDefinition> Tools { get; init; }
}
// Protocol/Types/Tools.cs
namespace McpDotNet.Protocol.Types;

using mcpdotnet.Features.Tools;
using System.Text.Json.Serialization;

public class ListToolsResponse
{
    public List<Tool> Tools { get; set; } = new();
}

public class Tool
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JsonSchema? InputSchema { get; set; }
}

public class JsonSchema
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, JsonSchemaProperty>? Properties { get; set; }
    public List<string>? Required { get; set; }
}

public class JsonSchemaProperty
{
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CallToolResponse
{
    public List<ToolContent> Content { get; set; } = new();
    public bool IsError { get; set; }
}

public class ToolContent
{
    public string Type { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string? Data { get; set; }
    public string? MimeType { get; set; }
}
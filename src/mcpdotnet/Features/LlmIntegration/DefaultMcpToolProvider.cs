// Features/LlmIntegration/DefaultMcpToolProvider.cs
namespace McpDotNet.Features.LlmIntegration;

using System.Text.Json;
using mcpdotnet.Features.Tools;
using McpDotNet.Client;
using McpDotNet.Features.Tools;

/// <summary>
/// Default implementation of IMcpToolProvider that handles conversion between MCP and LLM-friendly formats.
/// </summary>
public class DefaultMcpToolProvider : IMcpToolProvider
{
    private readonly McpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DefaultMcpToolProvider(McpClient client, JsonSerializerOptions? jsonOptions = null)
    {
        _client = client;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc/>
    public async Task<List<ToolDefinition>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        return await _client.ListToolDefsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ToolResult> ExecuteToolCallAsync(
        string toolName,
        Dictionary<string, object> arguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute tool call
            var result = await _client.CallToolAsync(toolName, arguments, cancellationToken);
            if (result != null)
            {
                return new ToolResult()
                {
                    Succeeded = !result.IsError,
                    Content = result.Content.Select(c => new ToolContent()
                    {
                        Type = c.Type,
                        Value = (c.Data ?? c.Text) ?? "",
                        MimeType = c.MimeType
                    }).ToList()
                };
            }
            else
            {
                return new ToolResult
                {
                    Succeeded = false,
                    Content = [new ToolContent
                    {
                        Type = "error",
                        Value = "Tool execution failed with no error message"
                    }],
                };
            }
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Succeeded = false,
                Content = [new ToolContent
                {
                    Type = "error",
                    Value = ex.Message
                }],
            };
        }
    }

    

    //private static (Dictionary<string, ToolArgument> Arguments, List<string> Required) ParseInputSchema(ToolInputSchema schema)
    //{
    //    var arguments = new Dictionary<string, ToolArgument>();
    //    var required = new List<string>();

    //    if (schema.Properties == null)
    //    {
    //        return (arguments, required);
    //    }

    //    foreach (var (name, propObj) in schema.Properties)
    //    {
    //        // Convert property to JsonElement for easier parsing
    //        var prop = (propObj as JsonElement?) ?? JsonSerializer.SerializeToElement(propObj);

    //        if (prop.TryGetProperty("type", out var typeElement))
    //        {
    //            var type = typeElement.GetString() ?? "unknown";
    //            var description = prop.TryGetProperty("description", out var descElement)
    //                ? descElement.GetString()
    //                : string.Empty;

    //            var isRequired = prop.TryGetProperty("required", out var reqElement) && reqElement.GetBoolean();
    //            if (isRequired)
    //            {
    //                required.Add(name);
    //            }

    //            string[]? enumValues = null;
    //            if (type == "string" && prop.TryGetProperty("enum", out var enumElement))
    //            {
    //                enumValues = enumElement.EnumerateArray()
    //                    .Select(e => e.GetString())
    //                    .Where(s => s != null)
    //                    .Cast<string>()
    //                    .ToArray();
    //            }

    //            arguments[name] = new ToolArgument
    //            {
    //                Name = name,
    //                Type = type,
    //                Description = description ?? string.Empty,
    //                Required = isRequired,
    //                EnumValues = enumValues
    //            };
    //        }
    //    }

    //    // Also check for required array at schema root
    //    if (schema.Properties.TryGetValue("required", out var rootRequired))
    //    {
    //        var requiredArray = rootRequired as JsonElement?;
    //        if (requiredArray?.ValueKind == JsonValueKind.Array)
    //        {
    //            foreach (var item in requiredArray.Value.EnumerateArray())
    //            {
    //                var name = item.GetString();
    //                if (name != null && !required.Contains(name))
    //                {
    //                    required.Add(name);

    //                    // Update the argument's required flag if it exists
    //                    if (arguments.TryGetValue(name, out var arg))
    //                    {
    //                        arguments[name] = arg with { Required = true };
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    return (arguments, required);
    //}

    //private static ToolResultContent ConvertContent(IContent content)
    //{
    //    return content switch
    //    {
    //        TextContent text => new ToolResultContent
    //        {
    //            Type = "text",
    //            Value = text.Text,
    //        },
    //        ImageContent image => new ToolResultContent
    //        {
    //            Type = "image",
    //            Value = image.Data,
    //            MimeType = image.MimeType
    //        },
    //        _ => new ToolResultContent
    //        {
    //            Type = "unknown",
    //            Value = content.ToString() ?? string.Empty
    //        }
    //    };
    //}

    //private static string ExtractErrorMessage(IReadOnlyList<IContent> content)
    //{
    //    // Try to extract error message from content, defaulting to first text content if available
    //    var textContent = content.OfType<TextContent>().FirstOrDefault();
    //    return textContent?.Text ?? "Tool execution failed with no error message";
    //}
}
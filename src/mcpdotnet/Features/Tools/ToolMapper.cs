using System.Text.Json;
using mcpdotnet.Features.Tools;
using McpDotNet.Protocol.Types;

namespace McpDotNet.Features.Tools;

internal static class ToolMapper
{
    public static ToolDefinition ToToolDefinition(Tool protocolTool)
    {
        ArgumentNullException.ThrowIfNull(protocolTool);
        ArgumentNullException.ThrowIfNull(protocolTool.InputSchema);

        var parameters = new Dictionary<string, ToolParameter>();

        if (protocolTool.InputSchema.Properties != null)
        {
            foreach (var property in protocolTool.InputSchema.Properties)
            {
                // Convert the dynamic property value to a JsonElement to access its properties
                var propertySchema = property.Value;

                parameters[property.Key] = new ToolParameter
                {
                    Name = property.Key,
                    Type = propertySchema.Type,
                    Description = propertySchema.Description,
                    Required = IsPropertyRequired(protocolTool.InputSchema, property.Key),
                    AllowedValues = GetEnumValues(propertySchema)
                };
            }
        }

        return new ToolDefinition
        {
            Name = protocolTool.Name,
            Description = protocolTool.Description ?? string.Empty,
            Parameters = parameters
        };
    }

    public static ToolResult ToToolResult(CallToolResponse protocolResult)
    {
        ArgumentNullException.ThrowIfNull(protocolResult);

        return new ToolResult
        {
            Succeeded = protocolResult.IsError,
            Content = protocolResult.Content?.Select(ToToolContent).ToList(),
            ErrorMessage = protocolResult.IsError == true ?
                GetErrorMessage(protocolResult.Content) : null
        };
    }

    private static mcpdotnet.Features.Tools.ToolContent ToToolContent(Protocol.Types.ToolContent c)
    {
        return new mcpdotnet.Features.Tools.ToolContent()
        {
            Type = c.Type,
            Value = (c.Data ?? c.Text) ?? "",
            MimeType = c.MimeType
        };
    }

    private static string GetSchemaType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out var typeElement))
        {
            return "object";
        }

        return typeElement.GetString() ?? "object";
    }

    private static string GetSchemaDescription(JsonElement schema)
    {
        if (!schema.TryGetProperty("description", out var descElement))
        {
            return string.Empty;
        }

        return descElement.GetString() ?? string.Empty;
    }

    private static bool IsPropertyRequired(JsonSchema? schema, string propertyName)
    {
        if (schema?.Required != null && schema.Required.Contains(propertyName))
        {
            return true;
        }

        return false;
    }

    private static string[]? GetEnumValues(JsonSchemaProperty? schema)
    {
        // TODO
        return null;
    }

    private static string GetErrorMessage(IReadOnlyList<Protocol.Types.ToolContent>? contents)
    {
        if (contents == null || contents.Count == 0)
        {
            return "Unknown error";
        }

        // Concatenate all text content as the error message
        var errorMessages = contents
            .Where(c => !string.IsNullOrEmpty(c.Text))
            .Select(c => c.Text);

        return string.Join(" ", errorMessages);
    }
}
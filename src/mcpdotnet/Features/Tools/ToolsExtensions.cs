using mcpdotnet.Features.Tools;
using McpDotNet.Client;
using McpDotNet.Protocol.Messages;
using McpDotNet.Protocol.Types;

namespace McpDotNet.Features.Tools;

/// <summary>
/// Extension methods for working with MCP tools.
/// </summary>
public static class ToolsExtensions
{
    /// <summary>
    /// Lists all available tools from the server.
    /// </summary>
    public static async Task<List<ToolDefinition>> ListToolDefsAsync(
        this McpClient client,
        CancellationToken cancellationToken = default)
    {
        if (client.ServerCapabilities?.Tools == null)
        {
            throw new McpClientException("Server does not support tools");
        }

        var response = await client.ListToolsAsync(cancellationToken);

        return response.Tools
            .Select(ToolMapper.ToToolDefinition)
            .ToList();
    }

    /// <summary>
    /// Calls a tool with the specified arguments.
    /// </summary>
    public static async Task<ToolResult> CallToolAsync(
        this McpClient client,
        string toolName,
        object? arguments = null,
        CancellationToken cancellationToken = default)
    {
        if (client.ServerCapabilities?.Tools == null)
        {
            throw new McpClientException("Server does not support tools");
        }

        var response = await client.SendRequestAsync<CallToolResponse>(
            new JsonRpcRequest
            {
                Method = "tools/call",
                Params = new
                {
                    name = toolName,
                    arguments = arguments
                }
            },
            cancellationToken
        );

        return ToolMapper.ToToolResult(response);
    }
}
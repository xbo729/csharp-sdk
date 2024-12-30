// Features/LlmIntegration/IMcpToolProvider.cs
using mcpdotnet.Features.Tools;

namespace McpDotNet.Features.LlmIntegration;

/// <summary>
/// Provides MCP tool information and execution capabilities to an LLM.
/// </summary>
public interface IMcpToolProvider
{
    /// <summary>
    /// Gets the available tools and their descriptions.
    /// </summary>
    Task<List<ToolDefinition>> GetToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool call.
    /// </summary>
    Task<ToolResult> ExecuteToolCallAsync(
        string toolName,
        Dictionary<string, object> arguments,
        CancellationToken cancellationToken = default);
}
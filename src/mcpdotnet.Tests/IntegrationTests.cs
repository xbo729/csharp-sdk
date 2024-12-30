using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Messages;
using McpDotNet.Protocol.Transport;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace McpDotNet.Tests;

public class IntegrationTests
{
    private readonly McpClientOptions _defaultOptions = new()
    {
        ClientInfo = new() { Name = "IntegrationTestClient", Version = "1.0.0" }
    };

    [Fact]
    public async Task ConnectAndPing_Stdio_EverythingServer()
    {
        // Arrange
        var config = new McpServerConfig
        {
            Id = "everything",
            Name = "Everything",
            TransportType = "stdio",
            TransportOptions = new Dictionary<string, string>
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-everything",
            }
        };

        // Inject the mock transport into the factory
        var factory = new McpClientFactory(
            new[] { config },
            _defaultOptions
        );

        // Act
        var client = await factory.GetClientAsync("everything");
        await client.PingAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task ListTools_Stdio_EverythingServer()
    {
        // arrange
        var config = new McpServerConfig
        {
            Id = "everything",
            Name = "Everything",
            TransportType = "stdio",
            TransportOptions = new Dictionary<string, string>
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-everything"
            }
        };

        var factory = new McpClientFactory(
            new[] { config },
            _defaultOptions
        );

        // act
        var client = await factory.GetClientAsync("everything");
        var tools = await client.ListToolsAsync(CancellationToken.None);

        // assert
        Assert.NotNull(tools);
        Assert.NotEmpty(tools.Tools);
        // We could add more specific assertions about expected tools
    }

    [Fact]
    public async Task CallTool_Stdio_EchoServer()
    {
        // arrange
        var config = new McpServerConfig
        {
            Id = "everything",
            Name = "Everything",
            TransportType = "stdio",
            TransportOptions = new Dictionary<string, string>
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-everything"
            }
        };

        var factory = new McpClientFactory(
            new[] { config },
            _defaultOptions
        );

        // act
        var client = await factory.GetClientAsync("everything");
        var result = await client.CallToolAsync(
            "echo",
            new Dictionary<string, object>
            {
                ["message"] = "Hello MCP!"
            },
            CancellationToken.None
        );

        // assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var textContent = Assert.Single(result.Content.Where(c => c.Type == "text"));
        Assert.Equal("Echo: Hello MCP!", textContent.Text);
    }
}

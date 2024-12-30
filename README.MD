# mcpdotnet

A .NET client implementation of the Model Context Protocol (MCP), enabling .NET applications to connect to and interact with MCP servers.

## About MCP

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information about MCP:
- [Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
- [GitHub Organization](https://github.com/modelcontextprotocol)

## Status

This project is in early development. While functional, it may undergo significant changes as the protocol and implementation evolve.

### Current Limitations
- Only supports the Tool capability (Resources, Prompts, Sampling, and Roots not yet implemented)
- Only stdio transport is currently supported (HTTP/SSE transport coming soon)
- Client library only - server implementation support may be added in the future
- Provider-agnostic implementation focused purely on the MCP protocol
- Applications using this library need to implement their own integration with LLM APIs/clients

The library's current focus is on implementing the core MCP protocol, with the main utility being the ability to launch and connect to MCP servers based on configuration objects.

## Features

- MCP client implementation for .NET applications
- Support for stdio transport
- Tool discovery and execution
- Async/await pattern throughout
- Compatible with .NET 8.0 and later

## Getting Started

Documentation and examples are coming soon. For now, please refer to the test project for usage examples.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
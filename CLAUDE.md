# UniCortex

A toolkit for controlling the Unity Editor externally via REST API + MCP.

## Documentation

- Do not edit `README.md` directly. Make README documentation changes in `.github/DRAFT_README.md` instead.

## Tech Stack

- Unity Editor side: C# HttpListener HTTP server
- MCP Server: .NET 10 + Model Context Protocol C# SDK
- CLI: .NET 10 + ConsoleAppFramework
- Shared Core: UniCortex.Core (class library shared by MCP and CLI)
- UPM package: com.veyron-sakai.uni-cortex

## Example MCP server configuration (`.mcp.json`)

  ```json
  {
    "mcpServers": {
      "Unity": {
        "type": "stdio",
        "command": "/bin/bash",
        "args": ["-c", "dotnet run --project ${UNICORTEX_PROJECT_PATH}/Library/PackageCache/com.veyron-sakai.uni-cortex@*/Tools~/UniCortex.Mcp/"],
        "env": {
          "UNICORTEX_PROJECT_PATH": "/path/to/your/unity/project"
        }
      }
    }
  }
  ```

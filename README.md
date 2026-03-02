# UniCortex

> [!CAUTION]
> This project is still under active development. The API and command structure may change without notice.

A toolkit for controlling Unity Editor externally via REST API and MCP (Model Context Protocol).

Primarily designed for AI agents (Claude Code, Codex CLI, etc.) to operate Unity Editor through MCP.

## Requirements

- Unity 2022.3 or later
- .NET 8 SDK (for MCP server)

## Installation

Add via Unity Package Manager using a Git URL:

1. Open Package Manager
2. Click the `+` button
3. Select "Add package from git URL"
4. Enter the following URL:

```
https://github.com/VeyronSakai/UniCortex.git
```

### MCP Server Setup

Add the following MCP server configuration to your MCP client's settings file (e.g., `.mcp.json`, `claude_desktop_config.json`, etc.). Refer to your client's documentation for the exact configuration location.

```json
{
  "mcpServers": {
    "Unity": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/your/unity/project/Library/PackageCache/com.veyron-sakai.uni-cortex@0.1.0/Tools~/UniCortex.Mcp/"],
      "env": {
        "UNICORTEX_PROJECT_PATH": "/path/to/your/unity/project"
      }
    }
  }
}
```

Replace `/path/to/your/unity/project` with the absolute path of your Unity project (set it in both `args` and `UNICORTEX_PROJECT_PATH`). After saving the configuration, restart the client to apply the changes.

The MCP server reads the port number from `Library/UniCortex/config.json` (written automatically when Unity Editor starts) and connects to the HTTP server.

No pre-build or tool installation is required. The MCP server is built and started automatically via `dotnet run`.

Alternatively, you can specify the URL directly via the `UNICORTEX_URL` environment variable (takes priority over `UNICORTEX_PROJECT_PATH`):

```json
{
  "mcpServers": {
    "Unity": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/your/unity/project/Library/PackageCache/com.veyron-sakai.uni-cortex@0.1.0/Tools~/UniCortex.Mcp/"],
      "env": {
        "UNICORTEX_URL": "http://localhost:12345"
      }
    }
  }
}
```

## Available MCP Tools

### Editor Control

| Tool | Description |
|------|-------------|
| `ping_editor` | Check connectivity with the Unity Editor |
| `enter_play_mode` | Start Play Mode |
| `exit_play_mode` | Stop Play Mode |
| `reload_domain` | Request script recompilation (domain reload) |
| `undo` | Undo the last operation |
| `redo` | Redo an undone operation |

### Scene

| Tool | Description |
|------|-------------|
| `open_scene` | Open a scene by path |
| `save_scene` | Save all open scenes |
| `get_scene_hierarchy` | Get the GameObject hierarchy tree of the current scene |

### GameObject

| Tool | Description |
|------|-------------|
| `get_game_objects` | Search GameObjects by name, tag, component type, instanceId, layer, path, or state |
| `create_game_object` | Create a new empty GameObject |
| `delete_game_object` | Delete a GameObject (supports Undo) |
| `modify_game_object` | Modify name, active state, tag, layer, or parent |

### Component

| Tool | Description |
|------|-------------|
| `add_component` | Add a component to a GameObject |
| `remove_component` | Remove a component from a GameObject |
| `get_component_properties` | Get serialized properties of a component |
| `set_component_property` | Set a serialized property on a component |

### Prefab

| Tool | Description |
|------|-------------|
| `create_prefab` | Save a scene GameObject as a Prefab asset |
| `instantiate_prefab` | Instantiate a Prefab into the scene |

### Asset

| Tool | Description |
|------|-------------|
| `refresh_asset_database` | Refresh the Unity Asset Database |

### Console

| Tool | Description |
|------|-------------|
| `get_console_logs` | Get console log entries from the Unity Editor |
| `clear_console_logs` | Clear all console logs |

### Test

| Tool | Description |
|------|-------------|
| `run_tests` | Run Unity Test Runner tests and return results |

### Menu Item

| Tool | Description |
|------|-------------|
| `execute_menu_item` | Execute a Unity Editor menu item by path |

### Screenshot

| Tool | Description |
|------|-------------|
| `capture_screenshot` | Capture a screenshot of the Game View |

## Settings

Configurable from **Project Settings > UniCortex**.

| Setting | Default | Description |
|---------|---------|-------------|
| AutoStart | true | Start automatically on Editor launch |
| Current Port | — | Read-only. The port assigned at startup (random, persisted across domain reloads) |

The HTTP server is assigned a random free port on each Editor launch. The port is written to `Library/UniCortex/config.json` and read automatically by the MCP server.

## Architecture

```
AI Agent ←(MCP/stdio)→ MCP Server (.NET 8) ←(HTTP)→ Unity Editor HTTP Server
```

- **Unity Editor side**: C# `HttpListener` HTTP server embedded in the Editor
- **MCP Server side**: .NET 8 + [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- **UPM Package**: `com.veyron-sakai.uni-cortex`

## Documentation

- [`Documentations~/SPEC.md`](Documentations~/SPEC.md) — Full API endpoint and MCP tool definitions
- [`Documentations~/TASKS.md`](Documentations~/TASKS.md) — Implementation task list and progress

## Contributing

When developing this package locally:

```bash
dotnet run --project Tools~/UniCortex.Mcp/
```

## License

MIT License - [LICENSE.txt](LICENSE.txt)

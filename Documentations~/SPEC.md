# UniCortex Specification

## Overview

UniCortex is a toolkit for controlling the Unity Editor from external processes.
It embeds an HTTP server inside the Unity Editor and lets AI agents drive the Editor directly through an MCP server.

The primary goal is to let AI agents (Claude Code, Codex CLI, etc.) operate the Unity Editor through the MCP protocol.

## Design Principles

- **C#-only stack**: no dependency on external runtimes such as Python or Node.js
- **MCP protocol support**: AI agents can drive the Editor directly via MCP
- **REST API remains available**: direct access from tools like `curl` still works
- **Distributed as a UPM package**

## Naming

- GitHub repository: `UniCortex`
- UPM package name: `com.veyron-sakai.uni-cortex`
- MCP server launch: `dotnet run --project <path>/Tools~/UniCortex.Mcp/`

---

## Directory Layout

```
UniCortex/
├── Editor/                      ← Unity Editor extensions
│   ├── Domains/
│   │   ├── Interfaces/          ← Abstractions over Unity APIs
│   │   └── Models/              ← DTOs and route constants (shared with Core)
│   ├── Handlers/                ← HTTP request handlers
│   ├── Infrastructures/         ← HttpListener, MainThreadDispatcher, etc.
│   └── UseCases/                ← Business logic
├── Tools~/
│   ├── UniCortex.sln            ← Solution file
│   ├── UniCortex.Core/          ← Shared library (use case layer + HTTP infrastructure)
│   │   ├── Domains/
│   │   ├── Extensions/
│   │   ├── Infrastructures/
│   │   └── UseCases/            ← 11 use case classes
│   ├── UniCortex.Mcp/           ← MCP server (thin wrapper around Core)
│   │   └── Tools/               ← MCP tool definitions
│   ├── UniCortex.Core.Test/     ← Core integration tests
│   ├── UniCortex.Cli/           ← CLI tool
│   │   └── Commands/            ← CLI command definitions
├── Tests~/
│   └── Editor/
│       ├── TestDoubles/         ← Fakes, spies, and other test doubles
│       ├── UseCases/            ← UseCase unit tests
│       └── Presentations/       ← Handler unit tests
└── Documentations~/
    └── SPEC.md                  ← This document
```

- `Editor/` — Unity Editor extensions. asmdef uses `includePlatforms: ["Editor"]`
- `Tools~/` — Excluded from Unity import via the `~` suffix. .NET 10 projects

---

## Component 1: Unity Editor HTTP Server

### Technical Elements

- Listens on `http://localhost:<port>/` via `System.Net.HttpListener`
- The port is auto-assigned to a random free port at Editor startup (obtained with `TcpListener` port 0)
- The port number is preserved across domain reloads via `SessionState` (it only changes when the Editor restarts)
- Starts automatically at Editor startup using `[InitializeOnLoad]`
- Main-thread dispatch using `EditorApplication.update` + `ConcurrentQueue<Action>`
- Graceful shutdown on `AssemblyReloadEvents.beforeAssemblyReload`, then restarts after the reload
- On successful server startup, the URL is written to `Library/UniCortex/config.json`
- `Library/UniCortex/config.json` is deleted on `EditorApplication.quitting`

### URL File

`Library/UniCortex/config.json` contains the server URL (e.g. `http://localhost:54321`).

- Project-specific (under `Library/`), so multiple Unity instances remain isolated
- `Library/` is typically gitignored, so the file is not committed
- The MCP server reads this file via the `UNICORTEX_PROJECT_PATH` environment variable

### Settings (ScriptableSingleton)

| Item | Default | Description |
|------|---------|-------------|
| AutoStart | true | Start the server automatically |

### Main-thread Dispatch

Unity APIs can only be called from the main thread. Since `HttpListener` callbacks run on the thread pool, they are bridged like so:

1. On the HTTP thread, call `MainThreadDispatcher.RunOnMainThread<T>(Func<T> func)`
2. Create a `TaskCompletionSource<T>` and enqueue it into a `ConcurrentQueue`
3. On the main thread (`EditorApplication.update`), dequeue → run `func()` → `tcs.SetResult()`
4. The HTTP thread awaits completion → returns the response

---

### JSON Serialization

Request/response JSON serialization uses DTO classes.

- Placed under `Editor/Domains/Models/`. namespace: `UniCortex.Editor.Domains.Models`
- `[Serializable]` attribute + public fields (camelCase)
- Must not contain Unity dependencies (`using UnityEngine`, etc.) since they are shared with the MCP server
- Unity side: `JsonUtility.ToJson()` / `JsonUtility.FromJson<T>()`
- MCP server / CLI side: `System.Text.Json` + `JsonSerializerOptions { IncludeFields = true }`
- The UniCortex.Core .csproj shares source via `<Compile Include="../../Editor/Domains/Models/**/*.cs" LinkBase="Models" />`

---

## API Endpoints

Responses are always `application/json; charset=utf-8`.
On error: an HTTP status code plus `{"error": "message"}`.
All scene-mutating operations support Undo.

### Editor Control

#### GET `/editor/ping`

Server reachability check. **Logs `pong` to the Unity Console** and returns a response.

Response:
```json
{"status": "ok", "message": "pong"}
```

#### GET `/editor/status`
Returns the current state of the Editor. Also used for internal polling inside MCP tools.

Response:
```json
{"isPlaying": false, "isPaused": false}
```

#### POST `/editor/play`
Enters Play mode. `EditorApplication.isPlaying = true`

Response: `{"success": true}`

#### POST `/editor/stop`
Exits Play mode. `EditorApplication.isPlaying = false`

Response: `{"success": true}`

#### POST `/editor/step`
Advances one frame while paused. `EditorApplication.Step()`. Used for frame-by-frame game control.

Response: `{"success": true}`

#### POST `/editor/domain-reload`
Requests a domain reload (script recompilation). `CompilationPipeline.RequestScriptCompilation()`

Response: `{"success": true}`

#### POST `/editor/undo`
Undoes the most recent operation. `Undo.PerformUndo()`

Response: `{"success": true}`

#### POST `/editor/redo`
Redoes the most recently undone operation. `Undo.PerformRedo()`

Response: `{"success": true}`

#### POST `/editor/save`
Executes File/Save, saving the currently active stage. Applies to anything File/Save can save — scenes, Prefabs, Timeline, etc. `EditorApplication.ExecuteMenuItem("File/Save")`

Request body: none

Response: `{"success": true}`

### Scene

#### POST `/scene/create`
Creates a new empty scene and saves it to the specified asset path.

Request body:
```json
{"scenePath": "Assets/Scenes/NewScene.unity"}
```

Response: `{"success": true}`

#### POST `/scene/open`
Opens a scene. `EditorSceneManager.OpenScene()`

Request body:
```json
{"scenePath": "Assets/Scenes/Main.unity"}
```

Response: `{"success": true}`

#### GET `/scene/hierarchy`
Returns the GameObject hierarchy of the current scene as a tree. Includes scene information.

Response:
```json
{
  "sceneName": "SampleScene",
  "scenePath": "Assets/Scenes/SampleScene.unity",
  "gameObjects": [
    {
      "name": "Main Camera",
      "instanceId": 10200,
      "activeSelf": true,
      "tag": "MainCamera",
      "layer": 0,
      "isStatic": false,
      "hideFlags": 0,
      "components": ["UnityEngine.Transform", "UnityEngine.Camera", "UnityEngine.AudioListener"],
      "children": []
    },
    {
      "name": "Canvas",
      "instanceId": 10300,
      "activeSelf": true,
      "tag": "Untagged",
      "layer": 5,
      "isStatic": false,
      "hideFlags": 0,
      "components": ["UnityEngine.RectTransform", "UnityEngine.Canvas"],
      "children": [
        {
          "name": "Button",
          "instanceId": 10400,
          "activeSelf": true,
          "tag": "Untagged",
          "layer": 5,
          "isStatic": false,
          "hideFlags": 0,
          "components": ["UnityEngine.RectTransform", "UnityEngine.UI.Image", "UnityEngine.UI.Button"],
          "children": []
        }
      ]
    }
  ]
}
```

### GameObject

#### GET `/gameobjects?query=...`
Searches GameObjects in the scene. Supports Unity Search-style query syntax.

Query parameters:
- `query`: search query string (optional; if omitted, all GameObjects are returned)

Delegates to Unity Search's (`SearchService` API) `scene` provider. Unity Search subfilter syntax is supported as-is.

Main query tokens:

| Token | Example | Description |
|-------|---------|-------------|
| Plain text | `Main Camera` | Partial name match |
| `t:` | `t:Camera` | Component type |
| `tag:` | `tag:resp` | Tag (partial match) |
| `tag=` | `tag=Player` | Tag (exact match) |
| `id:` | `id:12345` | instanceId |
| `layer:` | `layer:5` | Layer number |
| `path:` | `path:Canvas/Button` | Hierarchy path |
| `is:` | `is:root` / `is:child` / `is:leaf` / `is:static` | State filter |

See Unity's official Search documentation for full query syntax.

Response:
```json
{
  "gameObjects": [
    {
      "name": "Player",
      "instanceId": 10500,
      "activeSelf": true,
      "tag": "Untagged",
      "layer": 0,
      "isStatic": false,
      "hideFlags": 0,
      "components": ["UnityEngine.Transform", "UnityEngine.CharacterController"]
    }
  ]
}
```

#### POST `/gameobject/create`
Creates a GameObject. Undo-supported via `Undo.RegisterCreatedObjectUndo`.

Request body:
```json
{
  "name": "MyObject"
}
```

- `name`: name of the GameObject to create (required)

Response:
```json
{"name": "MyCube", "instanceId": 12345}
```

#### POST `/gameobject/delete`
Deletes a GameObject. Undo-supported via `Undo.DestroyObjectImmediate`.

Request body: `{"instanceId": 12345}`

Response: `{"success": true}`

#### POST `/gameobject/modify`
Modifies properties of a GameObject. Only the supplied fields are updated. Undo-supported via `Undo.RecordObject`.

Request body:
```json
{
  "instanceId": 12345,
  "name": "RenamedCube",
  "activeSelf": false,
  "tag": "Player",
  "layer": 8,
  "parentInstanceId": 67890
}
```

All fields other than `instanceId` are optional. Setting `parentInstanceId` to `0` moves the object to the root.

Response: `{"success": true}`

### Component

Type resolution uses the pair `componentType` (fully qualified type name including namespace) and `assemblyName` (the name of the assembly that defines the type). Internally `Type.GetType($"{componentType}, {assemblyName}")` is used, so `assemblyName` should be the CLR assembly simple name (e.g. `UnityEngine.PhysicsModule`, `Assembly-CSharp`).

#### POST `/component/add`
Adds a component to a GameObject. Undo-supported via `Undo.AddComponent`.

Request body: `{"instanceId": 12345, "componentType": "UnityEngine.Rigidbody", "assemblyName": "UnityEngine.PhysicsModule"}`

Response: `{"success": true}`

#### POST `/component/remove`
Removes a component from a GameObject. Undo-supported via `Undo.DestroyObjectImmediate`.

Request body: `{"instanceId": 12345, "componentType": "UnityEngine.Rigidbody", "assemblyName": "UnityEngine.PhysicsModule", "componentIndex": 0}`

- `componentIndex`: index used when multiple components of the same type exist (default: 0)

Response: `{"success": true}`

#### GET `/component/properties?instanceId=12345&componentType=UnityEngine.Transform&assemblyName=UnityEngine.CoreModule`
Returns the serialized properties of the specified component.

Query parameters:
- `instanceId`: instanceId of the target GameObject (required)
- `componentType`: fully qualified component type name including namespace (required)
- `assemblyName`: assembly name that defines the type (required)
- `componentIndex`: index used when multiple components of the same type exist (optional, default: 0)

Response:
```json
{
  "componentType": "UnityEngine.Transform",
  "properties": [
    {"path": "m_LocalPosition", "type": "Vector3", "value": {"x": 0, "y": 1, "z": 0}},
    {"path": "m_LocalRotation", "type": "Quaternion", "value": {"x": 0, "y": 0, "z": 0, "w": 1}},
    {"path": "m_LocalScale", "type": "Vector3", "value": {"x": 1, "y": 1, "z": 1}}
  ]
}
```

#### POST `/component/property`
Modifies a serialized property of a component. Uses the `SerializedObject` / `SerializedProperty` APIs, which automatically record an Undo entry.

Request body:
```json
{
  "instanceId": 12345,
  "componentType": "UnityEngine.Transform",
  "assemblyName": "UnityEngine.CoreModule",
  "propertyPath": "m_LocalPosition.x",
  "value": "1.5"
}
```

- `propertyPath`: Unity's `SerializedProperty.propertyPath` format
- `value`: passed as a string. The type is inferred automatically from `SerializedProperty.propertyType`

Response: `{"success": true}`

### ScriptableObject

Create, read, and write `.asset` files (ScriptableObjects). Type resolution uses the `typeName` + `assemblyName` pair, the same as components. Property read/write reuses `SerializedPropertyValueConverter` / `SerializedPropertyValueParser` and shares the same string-based format as `get_component_properties` / `set_component_property`.

Reads only enumerate top-level properties. Nested `[Serializable]` types appear as a single `Generic` entry. Writes use `SerializedObject.FindProperty`, so dotted paths like `nestedField.x` or `arrayField.Array.data[0].value` are supported.

#### POST `/scriptable-object/create`
Creates a new `.asset` given a fully qualified type name. `ScriptableObject.CreateInstance` + `AssetDatabase.CreateAsset` + `Undo.RegisterCreatedObjectUndo`.

Request body:
```json
{
  "typeName": "MyNamespace.MyScriptableObject",
  "assemblyName": "Assembly-CSharp",
  "assetPath": "Assets/Data/MyData.asset"
}
```

Response: `{"success": true, "instanceId": 56789}`

#### GET `/scriptable-object/properties?assetPath=Assets/Data/MyData.asset`
Returns the list of top-level properties of an existing `.asset` file.

Query parameters:
- `assetPath`: asset path of the target `.asset` file (required)

Response:
```json
{
  "typeName": "MyNamespace.MyScriptableObject",
  "properties": [
    {"path": "m_Speed", "type": "Float", "value": "1.5"}
  ]
}
```

#### POST `/scriptable-object/property`
Updates a specific property on an `.asset`. Automatic Undo via `SerializedObject.ApplyModifiedProperties`, then `EditorUtility.SetDirty` + `AssetDatabase.SaveAssets`.

Request body:
```json
{
  "assetPath": "Assets/Data/MyData.asset",
  "propertyPath": "m_Speed",
  "value": "2.5"
}
```

Response: `{"success": true}`

### Prefab

#### POST `/prefab/create`
Saves a GameObject in the scene as a Prefab asset. `PrefabUtility.SaveAsPrefabAsset()`

Request body:
```json
{"instanceId": 12345, "assetPath": "Assets/Prefabs/MyCube.prefab"}
```

Response: `{"success": true}`

#### POST `/prefab/instantiate`
Instantiates a Prefab into the scene. `PrefabUtility.InstantiatePrefab()` + `Undo.RegisterCreatedObjectUndo`

Request body:
```json
{"assetPath": "Assets/Prefabs/MyCube.prefab"}
```

Response:
```json
{"name": "MyCube", "instanceId": 56789}
```

#### POST `/prefab/open`
Opens a Prefab asset in Prefab Mode. `PrefabStageUtility.OpenPrefab()`

Request body:
```json
{"assetPath": "Assets/Prefabs/MyCube.prefab"}
```

Response: `{"success": true}`

#### POST `/prefab/close`
Closes Prefab Mode and returns to the main stage. `StageUtility.GoToMainStage()`

Request body: none

Response: `{"success": true}`

### Asset

#### POST `/asset-database/refresh`
Refreshes the asset database. `AssetDatabase.Refresh()`

Response: `{"success": true}`

### Project Window

#### POST `/project-window/select`
Selects the specified asset in the Project window, brings the window to the front, and pings the asset.

Request body:
```json
{"assetPath": "Assets/Scenes/Main.unity"}
```

- `assetPath`: required. Asset path to select

Response:
```json
{"success": true}
```

### Console

#### GET `/console/logs`
Returns the most recent Unity Console log entries.

Query parameters:
- `count` (optional, default: 100): number of entries to fetch

Response:
```json
{
  "logs": [
    {
      "message": "NullReferenceException: ...",
      "stackTrace": "at MyScript.Update() ...",
      "type": "Error"
    }
  ]
}
```

- `type`: one of `Log`, `Warning`, `Error`

#### POST `/console/clear`
Clears the Unity Console logs. `LogEntries.Clear()`

Response: `{"success": true}`

### Menu Items

#### POST `/menu-item/execute`
Executes a Unity menu item. `EditorApplication.ExecuteMenuItem()`

Request body: `{"menuPath": "GameObject/3D Object/Cube"}`

Response: `{"success": true}`

#### POST `/tests/run`
Runs tests via the Unity Test Runner, waits for completion, and returns the results. `TestRunnerApi`

Request body:
```json
{"testMode": "EditMode", "testNames": ["MyTests.TestA"]}
```

- `testMode`: `EditMode` or `PlayMode` (optional, default: `EditMode`)
- `testNames`: array of test names (optional)
- `groupNames`: array of test group names (optional)
- `categoryNames`: array of test category names (optional)
- `assemblyNames`: array of test assembly names (optional)

Response:
```json
{
  "passed": 10,
  "failed": 1,
  "skipped": 2,
  "results": [
    {"name": "MyTests.ShouldWork", "status": "Passed", "duration": 0.05},
    {"name": "MyTests.ShouldFail", "status": "Failed", "duration": 0.02, "message": "Expected true but was false"}
  ]
}
```

### Screenshot

#### GET `/screenshot/capture`
Captures a screenshot of the current Unity rendering output. Play Mode only.
Normally captures the Game View, but the Scene View is captured instead if the Game View is unfocused or closed.

- Uses `ScreenCapture.CaptureScreenshotAsTexture()` to capture the current rendering output (including UI overlays)
- Returns 400 in Edit Mode

Response: `{ "pngDataBase64": "<base64>" }` (`Content-Type: application/json`)

### View

#### POST `/scene-view/focus`
Switches focus to the Scene View.

Response: `{"success": true}`

#### POST `/game-view/focus`
Switches focus to the Game View.

Response: `{"success": true}`

#### GET `/game-view/size`
Gets the current Game View size (width and height in pixels).

Response:
```json
{"screenWidth": 1920, "screenHeight": 1080}
```

#### GET `/game-view/size/list`
Returns the list of available Game View sizes (built-in + custom).

Response:
```json
{
  "sizes": [
    {"index": 0, "name": "Free Aspect", "width": 0, "height": 0, "sizeType": "AspectRatio"},
    {"index": 1, "name": "1920x1080", "width": 1920, "height": 1080, "sizeType": "FixedResolution"}
  ],
  "selectedIndex": 1
}
```

#### POST `/game-view/size`
Sets the Game View resolution. Specify the index obtained from `GET /game-view/size/list`.

Request body:
```json
{"index": 1}
```

Response: `{"success": true}`

### Recording

Recording features powered by the Unity Recorder package (`com.unity.recorder`). Lets you list recorders and add, remove, start, and stop Movie Recorders. The recorder list is reset on domain reload.

#### GET `/recorder/all/list`
Returns all registered recorders together with their settings and errors.

Response:
```json
{
  "recorders": [
    {
      "index": 0,
      "type": "Movie",
      "name": "MyRecorder",
      "enabled": true,
      "outputPath": "/path/to/output.mp4",
      "encoder": "UnityMediaEncoder",
      "encodingQuality": "Low",
      "errors": []
    }
  ]
}
```

#### POST `/recorder/movie/add`
Adds a Movie Recorder to the list. Source is fixed to Game View, resolution is fixed to Game View Resolution. Audio is OFF by default.

Request body:
```json
{
  "name": "MyRecorder",
  "outputPath": "/path/to/output.mp4",
  "encoder": "UnityMediaEncoder",
  "encodingQuality": "Low",
  "captureAudio": false
}
```
- `name`: required. Name of the Movie Recorder
- `outputPath`: required. Output file path
- `encoder`: optional. `"UnityMediaEncoder"` (default), `"ProRes"`, `"GIF"`
- `encodingQuality`: optional. Only valid for UnityMediaEncoder. `"Low"` (default), `"Medium"`, `"High"`
- `captureAudio`: optional. Capture audio. Default `false`
- Odd resolutions produce an error in MP4. Set the Game View size to an even resolution beforehand.

Response: `{"name": "MyRecorder"}`

#### POST `/recorder/movie/remove`
Removes the Movie Recorder at the given index from the list.

Request body: `{"index": 0}`

- Returns 400 if the index is out of range

Response: `{"success": true}`

#### POST `/recorder/movie/start`
Starts recording with the Movie Recorder at the given index. Play Mode only. Manual mode, Constant frame rate.

- Records using `RecorderController` + `MovieRecorderSettings`
- Recording is automatically stopped and cleaned up when leaving Play Mode
- Returns 400 if Unity Recorder is not installed
- Returns 400 in Edit Mode
- Returns 400 if the Movie Recorder has errors

Request body:
```json
{
  "index": 0,
  "fps": 30
}
```
- `index`: index of the Movie Recorder to use (obtained via `get_all_recorders`)
- `fps`: optional. Default 30

Response: `{"success": true}`

#### POST `/recorder/movie/stop`
Stops recording and writes the file out.

- Returns 400 if not currently recording

Response: `{"outputPath": "/path/to/output.mp4"}`

### Input

Device-level input dispatch via the Unity Input System package (`com.unity.inputsystem`). Queues device-level events using `InputSystem.QueueEvent()`. Requires Play mode. Only available when the Input System package is installed.

**Behavior**: Directly updates Input System actions (`InputAction`, `PlayerInput`) and the state of `Keyboard.current` / `Mouse.current`. Legacy `UnityEngine.Input.GetKey()` / `Input.GetMouseButton()` are not triggered.

**Optional dependency**: `UNICORTEX_INPUT_SYSTEM` is defined via `versionDefines` in `UniCortex.Editor.asmdef` when `com.unity.inputsystem` is installed. When it is not installed, a fallback adapter throws `NotSupportedException`.

#### POST `/input/key`
Sends a keyboard event through the Input System while in Play mode.

Request body:
```json
{"key": "Space", "eventType": "press"}
```

- `key`: required. Input System `Key` enum name (e.g. `"Space"`, `"A"`, `"LeftArrow"`, `"Return"`, `"LeftShift"`)
- `eventType`: optional. `"press"` (default) or `"release"`

Response: `{"success": true}`

#### POST `/input/mouse`
Sends a mouse event through the Input System while in Play mode.

Request body:
```json
{"x": 100.0, "y": 200.0, "button": "left", "eventType": "press"}
```

- `x`, `y`: screen coordinates in pixels. The origin (0, 0) is the bottom-left of the screen. X increases to the right, Y increases upward. The value range depends on the Game View resolution (e.g. for 800x600: x: 0–800, y: 0–600). Same coordinate system as `Mouse.current.position.ReadValue()`. Note: screenshots from `capture_screenshot` use top-left origin with Y increasing downward, so a coordinate transform is required.
- `button`: optional. `"left"` (default), `"right"`, `"middle"`
- `eventType`: optional. `"click"` (default: press, wait one frame, then release), `"press"`, `"release"`, or `"move"` (only update position, no button action)

Response: `{"success": true}`

### Timeline

Timeline control via the Unity Timeline package (`com.unity.timeline`). Operates on tracks, clips, and bindings through PlayableDirector components. Only available when the Timeline package is installed.


#### POST `/timeline/create`
Creates a TimelineAsset (`.playable` file).

Request body:
```json
{"assetPath": "Assets/Timelines/MyTimeline.playable"}
```

- `assetPath`: required. Destination path of the TimelineAsset

Response:
```json
{"success": true, "assetPath": "Assets/Timelines/MyTimeline.playable"}
```

#### POST `/timeline/track/add`
Adds a track to a TimelineAsset. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackType": "UnityEngine.Timeline.AnimationTrack", "trackName": "My Track"}
```

- `trackType`: required. Fully qualified type name (e.g. `UnityEngine.Timeline.AnimationTrack`, `UnityEngine.Timeline.AudioTrack`)
- `trackName`: optional. Track name

Response: `{"success": true}`

#### POST `/timeline/track/remove`
Removes a track from a TimelineAsset. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0}
```

Response: `{"success": true}`

#### POST `/timeline/track/bind`
Sets the track binding on a PlayableDirector. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "targetInstanceId": 67890}
```

Response: `{"success": true}`

#### POST `/timeline/clip/add`
Adds a default clip to a track. The clip kind is chosen automatically based on the track type. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "start": 1.0, "duration": 3.0, "clipName": "My Clip"}
```

- `trackIndex`: required. Index of the track to add the clip to (0-based)
- `start`: optional. Clip start time in seconds. Default: 0
- `duration`: optional. Clip duration in seconds. If 0, uses the track's default
- `clipName`: optional. Display name of the clip

Response: `{"success": true}`

#### POST `/timeline/clip/remove`
Removes a clip from a track. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "clipIndex": 0}
```

Response: `{"success": true}`

#### POST `/timeline/play`
Starts Timeline playback on a PlayableDirector.

Request body:
```json
{"instanceId": 12345}
```

- `instanceId`: required. instanceId of the GameObject that has the PlayableDirector

Response: `{"success": true}`

#### POST `/timeline/stop`
Stops Timeline playback on a PlayableDirector and rewinds to the start.

Request body:
```json
{"instanceId": 12345}
```

- `instanceId`: required. instanceId of the GameObject that has the PlayableDirector

Response: `{"success": true}`

### Extension

List and execute user-defined Extensions. Implementing a class derived from `ExtensionHandler` on the Unity Editor side automatically registers it for discovery.

#### GET `/extensions/list`
Returns the list of registered Extensions. Returns metadata for all Extensions discovered via `TypeCache.GetTypesDerivedFrom<ExtensionHandler>()`.

Response:
```json
{
  "extensions": [
    {
      "name": "spawn_enemy",
      "description": "Spawn an enemy prefab at the specified position.",
      "readOnly": false,
      "inputSchema": "{\"type\":\"object\",\"properties\":{\"prefabPath\":{\"type\":\"string\",\"description\":\"Path to the enemy prefab\"}},\"required\":[\"prefabPath\"]}"
    }
  ]
}
```

- `inputSchema`: JSON Schema string, generated automatically from `ExtensionSchema`. `null` for Extensions without parameters

#### POST `/extensions/execute`
Executes an Extension by name. `ExtensionHandler.Execute()` runs on the main thread, so Unity API calls are safe.

Request body:
```json
{"name": "spawn_enemy", "arguments": "{\"prefabPath\":\"Assets/Prefabs/Enemy.prefab\"}"}
```

- `name`: required. Name of the custom tool to execute
- `arguments`: optional. Tool arguments as a JSON string

Response:
```json
{"result": "Spawned Enemy (instanceId: 12345)"}
```

- Returns 404 if the Extension is not found
- Returns 400 if `name` is not provided

---

## Component 2: MCP Server (dotnet run --project)

### Structure

```
AI Agent ←(MCP/stdio)→ MCP Server ←(HTTP)→ Unity Editor HTTP Server
Terminal ←(CLI)→ CLI Tool ←(HTTP)→ Unity Editor HTTP Server

UniCortex.Core (shared library)
  ├── UniCortex.Mcp (MCP server)
  └── UniCortex.Cli (CLI tool)
```

The MCP server and CLI share their common HTTP communication logic and service layer through the `UniCortex.Core` library.

### UniCortex.Core (Shared Library)

The use case layer and HTTP infrastructure shared between the MCP server and CLI.

- **Use case layer**: `EditorUseCase`, `GameObjectUseCase`, `ComponentUseCase`, `SceneUseCase`, `PrefabUseCase`, `TestUseCase`, `ConsoleUseCase`, `AssetUseCase`, `MenuItemUseCase`, `ScreenshotUseCase`, `SceneViewUseCase`, `GameViewUseCase`, `InputUseCase`, `TimelineUseCase`
- **Infrastructure**: `HttpRequestHandler`, `UnityServerUrlProvider`, `HttpResponseMessageExtensions`
- **DI extension**: `ServiceCollectionExtensions.AddUniCortexCore()` registers all use cases and infrastructure in one call

Each use case receives `IHttpClientFactory` and `IUnityServerUrlProvider` via constructor DI and communicates with the Unity Editor HTTP server. Return values are `string` (JSON or message) or `byte[]` (screenshots). Exceptions propagate to the caller as-is.

### MCP Server (UniCortex.Mcp)

A thin wrapper that is only responsible for MCP tool definitions. Each tool class receives the corresponding Core use case via constructor DI and wraps the result in a `CallToolResult`.

### Technical Stack

- .NET 10 (`net10.0`)
- ModelContextProtocol SDK (1.0.0)
- Microsoft.Extensions.Hosting (10.0.3)
- UniCortex.Core (ProjectReference)
- Transport: stdio
- Launched directly via `dotnet run --project` (no prebuild required)

### Entry Point (Program.cs)

- Builds the MCP server with `Host.CreateApplicationBuilder`
- `.WithStdioServerTransport()` for stdio transport
- `.WithToolsFromAssembly()` for automatic tool discovery
- `builder.Services.AddUniCortexCore()` registers Core services with DI
- URL resolution priority:
  1. `UNICORTEX_URL` environment variable (direct URL)
  2. `Library/UniCortex/config.json` under the `UNICORTEX_PROJECT_PATH` environment variable
  3. Exits with an error if neither is set
- Logs go to stderr (stdout is reserved for the MCP protocol)

### MCP Tools (43 tools total)

To prevent AI agents from getting confused, each tool maps to a clearly distinct operation and overlap is eliminated.
Each tool is defined as an `[McpServerTool]` method inside a `[McpServerToolType]` class.
The tool receives the corresponding Core service via constructor DI and wraps the result in a `CallToolResult`.

#### Editor Control (11)

| Tool | API | Description |
|------|-----|-------------|
| `ping_editor` | GET `/editor/ping` | Check connectivity with the Unity Editor |
| `enter_play_mode` | POST `/editor/play` | Enter Play mode |
| `exit_play_mode` | POST `/editor/stop` | Exit Play mode |
| `get_editor_status` | GET `/editor/status` | Get the Editor's current state (Play mode, paused) |
| `pause_editor` | POST `/editor/pause` | Pause the Editor. Combine with `step_editor` for frame-by-frame control |
| `unpause_editor` | POST `/editor/unpause` | Resume the Editor from pause |
| `step_editor` | POST `/editor/step` | Advance one frame while paused. For frame-by-frame game control |
| `reload_domain` | POST `/editor/domain-reload` | Trigger script recompilation (domain reload) |
| `undo` | POST `/editor/undo` | Undo the most recent operation |
| `redo` | POST `/editor/redo` | Redo the most recently undone operation |
| `save` | POST `/editor/save` | Execute File/Save and save the currently active stage (scenes, Prefabs, Timeline, etc.) |

#### Scene (3)

| Tool | API | Description |
|------|-----|-------------|
| `create_scene` | POST `/scene/create` | Create a new empty scene and save it to an asset path |
| `open_scene` | POST `/scene/open` | Open a scene by path |
| `get_hierarchy` | GET `/hierarchy` | Get the GameObject hierarchy of the scene or Prefab as a tree |

#### GameObject (4)

| Tool | API | Description |
|------|-----|-------------|
| `find_game_objects` | GET `/gameobjects` | Search the scene with query syntax (name, tag, component type, instanceId, layer, path, state) |
| `create_gameobject` | POST `/gameobject/create` | Create a GameObject (primitive specification supported) |
| `delete_gameobject` | POST `/gameobject/delete` | Delete a GameObject |
| `modify_gameobject` | POST `/gameobject/modify` | Rename, enable/disable, reparent, change tag/layer |

#### Component (4)

| Tool | API | Description |
|------|-----|-------------|
| `add_component` | POST `/component/add` | Add a component to a GameObject |
| `remove_component` | POST `/component/remove` | Remove a component from a GameObject |
| `get_component_properties` | GET `/component/properties` | Get the serialized properties of a component |
| `set_component_property` | POST `/component/property` | Modify a component property |

Types are specified with `componentType` + `assemblyName` (e.g. `UnityEngine.Rigidbody` + `UnityEngine.PhysicsModule`).

#### ScriptableObject (3)

| Tool | API | Description |
|------|-----|-------------|
| `create_scriptable_object` | POST `/scriptable-object/create` | Create a new `.asset` from `typeName` + `assemblyName` |
| `get_scriptable_object_properties` | GET `/scriptable-object/properties` | Get the top-level property list of an `.asset` file |
| `set_scriptable_object_property` | POST `/scriptable-object/property` | Modify a specific property on an `.asset` file |

#### Prefab (4)

| Tool | API | Description |
|------|-----|-------------|
| `create_prefab` | POST `/prefab/create` | Save a scene GameObject as a Prefab asset |
| `instantiate_prefab` | POST `/prefab/instantiate` | Instantiate a Prefab into the scene |
| `open_prefab` | POST `/prefab/open` | Open a Prefab in Prefab Mode |
| `close_prefab` | POST `/prefab/close` | Close Prefab Mode and return to the main stage |

#### Asset (1)

| Tool | API | Description |
|------|-----|-------------|
| `refresh_asset_database` | POST `/asset-database/refresh` | Refresh the AssetDatabase |

#### Project Window (1)

| Tool | API | Description |
|------|-----|-------------|
| `select_project_window_asset` | POST `/project-window/select` | Select an asset in the Project window, bring it to the front, and ping it |

#### Console (2)

| Tool | API | Description |
|------|-----|-------------|
| `get_console_logs` | GET `/console/logs` | Get logs from the Unity Console |
| `clear_console_logs` | POST `/console/clear` | Clear the Unity Console logs |

#### Test (1)

| Tool | API | Description |
|------|-----|-------------|
| `run_tests` | POST `/tests/run` | Run tests via the Test Runner and return the results |

#### Menu Items (1)

| Tool | API | Description |
|------|-----|-------------|
| `execute_menu_item` | POST `/menu-item/execute` | Execute a Unity menu item by path |

#### Screenshot (1)

| Tool | API | Description |
|------|-----|-------------|
| `capture_screenshot` | GET `/screenshot/capture` | Capture a screenshot of the current rendering output |

#### View (5)

| Tool | API | Description |
|------|-----|-------------|
| `focus_scene_view` | POST `/scene-view/focus` | Switch focus to the Scene View |
| `focus_game_view` | POST `/game-view/focus` | Switch focus to the Game View |
| `get_game_view_size` | GET `/game-view/size` | Get the current Game View size |
| `get_game_view_size_list` | GET `/game-view/size/list` | Get the list of available Game View sizes |
| `set_game_view_size` | POST `/game-view/size` | Set the Game View resolution by index |

#### Input (2)

| Tool | API | Description |
|------|-----|-------------|
| `send_key_event` | POST `/input/key` | Send a key event through the Input System (requires com.unity.inputsystem) |
| `send_mouse_event` | POST `/input/mouse` | Send a mouse event through the Input System (requires com.unity.inputsystem) |

#### Timeline (8)

| Tool | API | Description |
|------|-----|-------------|
| `create_timeline` | POST `/timeline/create` | Create a TimelineAsset (requires com.unity.timeline) |
| `add_timeline_track` | POST `/timeline/track/add` | Add a track to a TimelineAsset (requires com.unity.timeline) |
| `remove_timeline_track` | POST `/timeline/track/remove` | Remove a track from a TimelineAsset (requires com.unity.timeline) |
| `bind_timeline_track` | POST `/timeline/track/bind` | Set a track binding (requires com.unity.timeline) |
| `add_timeline_clip` | POST `/timeline/clip/add` | Add a clip to a track (requires com.unity.timeline) |
| `remove_timeline_clip` | POST `/timeline/clip/remove` | Remove a clip from a track (requires com.unity.timeline) |
| `play_timeline` | POST `/timeline/play` | Start Timeline playback (requires com.unity.timeline) |
| `stop_timeline` | POST `/timeline/stop` | Stop Timeline playback (requires com.unity.timeline) |

#### Extension (dynamic)

Extensions are discovered dynamically from the Unity Editor's `GET /extensions/list` when the MCP server starts. They are integrated with the existing static tools via `WithListToolsHandler` / `WithCallToolHandler`.

Users implement an `ExtensionHandler`-derived class on the Unity Editor side and define input parameters in C# via `ExtensionSchema`.

#### Design Decisions

**Included because:**
- `remove_component` — Symmetric with `add_component`. Undo-supported, so it's safe to remove
- Separation of `find_game_objects` and `get_component_properties` — The former returns GameObject summaries (type list only); the latter returns details for a specific component. This avoids returning a flood of properties at once
- `execute_menu_item` — A general escape hatch for edge cases that dedicated tools don't cover
- `capture_screenshot` — Required for multimodal AI agents to inspect state visually
- `focus_scene_view` / `focus_game_view` — Required to reliably capture the intended view together with `capture_screenshot`
- Dedicated ScriptableObject tools — Edits `.asset` files with the same `SerializedProperty`-based vocabulary as components, providing a consistent API for agents. Editing files directly on the filesystem easily breaks format and loses Undo / Inspector reflection

**Excluded:**
- `execute_csharp` (arbitrary C# execution) — High security risk. Most edge cases are covered by `execute_menu_item`
- `get_editor_status` — Not needed as an MCP tool. Exists as a REST API (`GET /editor/status`) for internal polling
- Dedicated Material / shader tools — Shader files can be edited directly through the filesystem by agents
- `find_assets` — Agents can search the filesystem directly
- Dedicated Transform tools — Covered generically by `set_component_property`

---

## MCP Server Setup

Just place a `.mcp.json` file at the root of your Unity project to use it. No prior build or tool installation is required.

```json
{
  "mcpServers": {
    "Unity": {
      "type": "stdio",
      "command": "/bin/bash",
      "args": ["-c", "dotnet run --project /path/to/your/unity/project/Library/PackageCache/com.veyron-sakai.uni-cortex@*/Tools~/UniCortex.Mcp/"],
      "env": {
        "UNICORTEX_PROJECT_PATH": "/path/to/your/unity/project"
      }
    }
  }
}
```

Replace `/path/to/your/unity/project` with the absolute path to your Unity project. The same path must be set both for the `--project` argument and for `UNICORTEX_PROJECT_PATH` (MCP clients don't support environment-variable expansion inside `args`). The `@*` glob pattern matches both version numbers (`@0.1.0`) and Git commit hashes (`@7bec663133`) automatically.

On first run, `dotnet run` builds automatically and launches the MCP server.

### Summary of URL Resolution

| Method | Setting | Priority |
|--------|---------|----------|
| Direct URL | `UNICORTEX_URL=http://localhost:XXXXX` | High |
| Project path | `UNICORTEX_PROJECT_PATH=/path/to/project` | Low |

If neither is set, the MCP server exits with an error.

---

## Component 3: CLI Tool (UniCortex.Cli)

A CLI tool for operating the Unity Editor from a terminal. Uses Core services directly.

### Technical Stack

- .NET 10 (`net10.0`)
- ConsoleAppFramework (5.6.0)
- UniCortex.Core (ProjectReference)
- Launched directly via `dotnet run --project` (no prebuild required)

### Command Structure

```
editor ping|play|stop|status|pause|unpause|step|undo|redo|reload-domain
scene create|open|save|hierarchy
gameobject find|create|delete|modify
component add|remove
component property list|set
prefab create|instantiate|open|close|save
scriptable-object create
scriptable-object property list|set
test run
console logs|clear
asset refresh
project-window select
menu execute
screenshot capture
scene-view focus
game-view focus
game-view size get|list|set
input send-key|send-mouse
timeline create|play|stop
timeline track add|remove|bind
timeline clip add|remove
extension list|execute
```

### Parameter Conventions

- Required parameters without a default value are annotated with `[Argument]` and treated as positional arguments
- Optional parameters with a default value are not annotated with `[Argument]` (they become named options in `--option-name` form)

### Entry Point (Program.cs)

- Builds the CLI app with `ConsoleApp.Create()`
- Calls `AddUniCortexCore()` inside `.ConfigureServices()` to register DI
- Registers command groups with `app.Add<XxxCommands>("prefix")`
- Each command class receives the corresponding Core service via constructor DI

### Usage Examples

```bash
export UNICORTEX_PROJECT_PATH=/path/to/your/unity/project

dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- editor ping
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- scene hierarchy
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- gameobject find --query "t:Camera"
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- screenshot capture ./screenshot.png
```

---

## UPM Package (package.json)

```json
{
  "name": "com.veyron-sakai.uni-cortex",
  "displayName": "UniCortex",
  "version": "0.1.0",
  "description": "Control Unity Editor via REST API and MCP.",
  "author": {
    "name": "veyron-sakai",
    "url": "https://github.com/veyron-sakai"
  }
}
```

### Assembly Definition (UniCortex.Editor.asmdef)

```json
{
  "name": "UniCortex.Editor",
  "rootNamespace": "UniCortex.Editor",
  "includePlatforms": ["Editor"]
}
```

---

## Test Conventions

### UseCase Unit Tests

When creating or modifying a UseCase class, always create a corresponding unit test under `Tests/Editor/UseCases/`.

- Test class name: `<UseCase name>Test` (e.g. `PlayUseCaseTest`)
- namespace: `UniCortex.Editor.Tests.UseCases`
- Stub the dispatcher with `FakeMainThreadDispatcher` and verify invocation counts via `CallCount`
- Inject spies (`Tests/Editor/TestDoubles/`) for Unity-API-dependent interfaces (`IEditorApplication`, `ICompilationPipeline`, etc.) to verify state and calls
- Run async methods synchronously with `.GetAwaiter().GetResult()` (for compatibility with Unity Test Framework 1.1.x)

---

## Usage Examples

```bash
# Check the port number in Library/UniCortex/config.json
# Example of extracting server_url from config.json:
URL=$(grep -o '"server_url":"[^"]*"' Library/UniCortex/config.json | cut -d'"' -f4)

# You can also call the API directly with curl
curl ${URL}/editor/ping
curl -X POST ${URL}/editor/play
```

To use it through MCP, add the configuration to your AI agent's (e.g. Claude Code) MCP settings.

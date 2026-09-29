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
Returns the GameObject hierarchy of every loaded scene as a tree, one entry per scene in Hierarchy window order.

- Scenes are enumerated with `SceneManager.sceneCount` / `GetSceneAt(i)`; scenes with `isLoaded == false` are skipped
- In Play Mode, the `DontDestroyOnLoad` scene is appended when it has root objects. The scene is obtained by calling the internal `EditorSceneManager.GetDontDestroyOnLoadScene()` through reflection (present in 2022.3 / 6000.2 / 6000.3); if the method is missing, the scene is skipped
- `isActive` is `true` for the scene returned by `SceneManager.GetActiveScene()`
- In Prefab Mode, a single entry for the Prefab contents is returned (`sceneName` is the root name, `scenePath` is the Prefab asset path, `isActive` is `true`)

Response:
```json
{
  "scenes": [
    {
      "sceneName": "Boot",
      "scenePath": "Assets/Scenes/Boot.unity",
      "isActive": true,
      "gameObjects": [
        {
          "name": "Main Camera",
          "instanceId": 10200,
          "children": []
        }
      ]
    },
    {
      "sceneName": "Menu",
      "scenePath": "Assets/Scenes/Menu.unity",
      "isActive": false,
      "gameObjects": [
        {
          "name": "Canvas",
          "instanceId": 10300,
          "children": [
            {
              "name": "Button",
              "instanceId": 10400,
              "children": []
            }
          ]
        }
      ]
    }
  ]
}
```

### GameObject

#### GET `/gameobjects?query=...`
Searches GameObjects in every loaded scene (including additively loaded scenes and, in Play Mode, the `DontDestroyOnLoad` scene) with the same query syntax as the Hierarchy window's search field. In Prefab Mode, only the Prefab contents are searched.

Query parameters:
- `query`: search query string (required)

Uses `HierarchyProperty(HierarchyType.GameObjects)` with `SetSearchFilter(query, SearchMode.All)`, the same search that backs the Hierarchy window. It reads the live scene state on every call, so objects created or loaded at runtime are always included. In Prefab Mode, `SetCustomScenes` restricts the search to the Prefab stage's scene. Scene header rows returned by `HierarchyProperty` are skipped. Results are in Hierarchy order and include inactive objects.

Unity Search (`SearchService`) is not used: its `scene` provider caches the object list and stops refreshing it after the domain reload on entering Play Mode, so runtime changes were missed.

Query tokens:

| Token | Example | Description |
|-------|---------|-------------|
| Plain text | `Main Camera` | Partial name match, case-insensitive. Multiple words must all match (AND) |
| `t:` | `t:Camera` / `t:Graphic` | Component type, case-insensitive. Derived types match too. Multiple `t:` tokens match any of them (OR) |
| `ref:` | `ref:12345:` | GameObjects that reference the object with that instanceId (including the object itself) |

Wildcards (`*`) are not supported. To narrow results by tag, layer, active state or scene, use the corresponding fields of each result; use `get_hierarchy` for paths and parent-child structure.

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
      "components": ["UnityEngine.Transform", "UnityEngine.CharacterController"],
      "sceneName": "Menu"
    }
  ]
}
```

#### POST `/gameobject/create`
Creates a GameObject. Undo-supported via `Undo.RegisterCreatedObjectUndo`.

Request body:
```json
{
  "name": "MyObject",
  "parentInstanceId": 67890,
  "siblingIndex": 0,
  "useRectTransform": true
}
```

- `name`: name of the GameObject to create (required)
- `parentInstanceId`: instanceId of the parent GameObject (optional). The object is placed with `GameObjectUtility.SetParentAndAlign`, so its local transform is reset and it inherits the parent's layer. If omitted, the object is created at the root of the active scene. An unknown instanceId returns an error
- `siblingIndex`: position among siblings, `0` = first (optional). Values beyond the last sibling place the object last; negative values are rejected. If omitted, the object is placed last
- `useRectTransform`: create the object with a `RectTransform` (optional). A `RectTransform` is also used automatically when the parent has one, mirroring the Editor's "Create Empty Child"

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
  "parentInstanceId": 67890,
  "siblingIndex": 0,
  "worldPositionStays": false
}
```

All fields other than `instanceId` are optional. Setting `parentInstanceId` to `0` moves the object to the root.

- `siblingIndex`: position among siblings, `0` = first. Without `parentInstanceId` it reorders the object within its current parent; with it, the index is applied after reparenting. Values beyond the last sibling place the object last; negative values are rejected. Undo-supported via `Undo.SetSiblingIndex`
- `worldPositionStays`: when changing the parent, keep the world transform (`true`, default) or keep the local transform (`false`). `false` is useful when moving between parents with different scales (e.g. another Canvas) or when the local values should be kept as-is. Passed to `Undo.SetTransformParent`

Response: `{"success": true}`

#### POST `/gameobject/duplicate`
Duplicates a GameObject, deep-copying its children and components. The copy is placed under the same parent, directly after the original (mirroring the Editor's "Duplicate" command). Undo-supported via `Undo.RegisterCreatedObjectUndo`.

Request body:
```json
{
  "instanceId": 12345,
  "name": "MyCopy"
}
```

- `instanceId`: instance ID of the GameObject to duplicate (required)
- `name`: name for the duplicate (optional). If omitted, a Unity-style unique sibling name (e.g. `Cube (1)`) is assigned via `GameObjectUtility.GetUniqueNameForSibling`.

Response:
```json
{"name": "Cube (1)", "instanceId": 67890}
```

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

##### Object reference values

`ObjectReference` / `ExposedReference` properties accept the following `value` formats. The same rules apply to every tool that writes serialized properties (`/scriptable-object/property`, `/timeline/track/property`, `/timeline/clip/property`).

| Format | Example | Resolution |
|--------|---------|------------|
| Asset path | `Assets/Timelines/Intro.playable` | `AssetDatabase.LoadMainAssetAtPath` |
| Asset GUID (32 hex characters) | `0123456789abcdef0123456789abcdef` | `AssetDatabase.GUIDToAssetPath` |
| instanceId | `12345` | `EditorUtility.InstanceIDToObject` (scene / Prefab Mode objects, components, assets) |
| `null` | `null` | Clears the reference |

The resolved object is assigned as-is when it matches the field type. Otherwise the following candidates are tried in order, and the first one Unity accepts is assigned:

1. Components of the GameObject (for an instanceId of a GameObject, or a Prefab asset's root GameObject), in `GetComponents` order
2. Sub-assets stored in the same asset file (`AssetDatabase.LoadAllAssetsAtPath`, e.g. a Sprite inside a Texture)

If no candidate is accepted (type mismatch, or a scene object assigned to an asset), the property is left unchanged and 400 is returned.

Example: assign a Timeline to a PlayableDirector:
```json
{
  "instanceId": 12345,
  "componentType": "UnityEngine.Playables.PlayableDirector",
  "assemblyName": "UnityEngine.DirectorModule",
  "propertyPath": "m_PlayableAsset",
  "value": "Assets/Timelines/Intro.playable"
}
```

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

### AnimationClip

Create `.anim` files (AnimationClip) and edit their float curves, e.g. to author clips for Timeline Animation tracks. Implemented with `AnimationUtility.SetEditorCurve` / `GetCurveBindings` / `GetEditorCurve` and `AnimationClipSettings`. Only float curves are supported (object reference curves such as sprite swaps are not).

A curve is identified by the binding triple `animatorRelativePath` + component type + `propertyName`:
- `animatorRelativePath`: path of the animated object relative to the Animator root (`EditorCurveBinding.path`) (e.g. `Root/Child`). Empty string (default) targets the Animator's own GameObject
- `componentType` + `assemblyName`: resolved the same way as components (e.g. `UnityEngine.UI.Image` + `UnityEngine.UI`, `UnityEngine.Transform` + `UnityEngine.CoreModule`). `UnityEngine.GameObject` is also accepted (e.g. `m_IsActive`)
- `propertyName`: serialized property name (e.g. `m_Color.a`, `m_AnchoredPosition.y`, `m_LocalScale.x`)

Curve writes are Undo-supported (`Undo.RegisterCompleteObjectUndo`), followed by `EditorUtility.SetDirty` + `AssetDatabase.SaveAssets`.

#### POST `/animation-clip/create`
Creates an empty AnimationClip. `AssetDatabase.CreateAsset` + `Undo.RegisterCreatedObjectUndo`.

Request body:
```json
{"assetPath": "Assets/Animations/FadeIn.anim", "loop": false, "frameRate": 60}
```

- `assetPath`: required. Must end with `.anim`
- `loop`: optional. Loop Time setting. Default: false
- `frameRate`: optional. Sample rate. `0` or omitted uses 60

Response: `{"success": true, "assetPath": "Assets/Animations/FadeIn.anim"}`

#### GET `/animation-clip/curves?assetPath=Assets/Animations/FadeIn.anim`
Returns the clip settings and all float curves with their keys.

Response:
```json
{
  "frameRate": 60,
  "loop": false,
  "length": 0.5,
  "curves": [
    {
      "animatorRelativePath": "Root/Child",
      "componentType": "UnityEngine.UI.Image",
      "assemblyName": "UnityEngine.UI",
      "propertyName": "m_Color.a",
      "keys": [
        {"time": 0, "value": 0, "inTangent": 0, "outTangent": 2, "leftTangentMode": "Linear", "rightTangentMode": "Linear"},
        {"time": 0.5, "value": 1, "inTangent": 2, "outTangent": 0, "leftTangentMode": "Linear", "rightTangentMode": "Linear"}
      ]
    }
  ]
}
```

- `leftTangentMode` / `rightTangentMode`: `AnimationUtility.TangentMode` names (`Free`, `Auto`, `ClampedAuto`, `Linear`, `Constant`)
- Infinite tangents (stepped keys, e.g. `Constant`) are returned as `±3.4028235E+38` (`float.MaxValue`), since JSON has no literal for infinity

#### POST `/animation-clip/curve/set`
Replaces one curve entirely with the given keys (creates it if missing).

Request body:
```json
{
  "assetPath": "Assets/Animations/FadeIn.anim",
  "animatorRelativePath": "Root/Child",
  "componentType": "UnityEngine.UI.Image",
  "assemblyName": "UnityEngine.UI",
  "propertyName": "m_Color.a",
  "keys": [
    {"time": 0, "value": 0, "tangentMode": "Linear"},
    {"time": 0.5, "value": 1, "tangentMode": "Linear"}
  ]
}
```

- `keys`: required, at least one. Sorted by `time`; duplicate times are rejected
- `keys[].inTangent` / `keys[].outTangent`: optional slopes, used when `tangentMode` is `Free`. `±3.4028235E+38` (`float.MaxValue`) is converted to an infinite slope, so values read by `GET /animation-clip/curves` can be passed back as-is
- `keys[].tangentMode`: optional, applied to both sides of the key. `Free` (default), `Auto`, `ClampedAuto`, `Linear`, or `Constant`. `Free` with the tangents omitted (0) gives flat tangents, i.e. a smooth ease in / ease out

Response: `{"success": true}`

#### POST `/animation-clip/curve/remove`
Removes one curve. Returns 400 when the curve does not exist.

Request body:
```json
{"assetPath": "Assets/Animations/FadeIn.anim", "animatorRelativePath": "Root/Child", "componentType": "UnityEngine.UI.Image", "assemblyName": "UnityEngine.UI", "propertyName": "m_Color.a"}
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

### View

#### POST `/scene-view/focus`
Switches focus to the Scene View.

Response: `{"success": true}`

#### GET `/scene-view/capture`
Captures the Scene View as a PNG image. Available in both Edit Mode and Play Mode.

- Focuses the Scene View first, then on a later main thread tick renders `SceneView.lastActiveSceneView.camera` into an offscreen RenderTexture at the Scene View's pixel size. The Scene View camera is only set up while the Scene View draws itself (e.g. it stays at the origin after a domain reload while hidden behind the Game View tab), so it must be shown before capturing
- In Prefab Mode, the Prefab contents are captured, because the Scene View camera renders the Prefab stage's preview scene
- Gizmos, the grid and Screen Space - Overlay UI are not included
- Returns 400 if no Scene View is open

Response: `{ "pngDataBase64": "<base64>" }` (`Content-Type: application/json`)

#### POST `/game-view/focus`
Switches focus to the Game View.

Response: `{"success": true}`

#### GET `/game-view/capture`
Captures the Game View as a PNG image. Play Mode only.

- Returns 400 in Edit Mode (use `GET /scene-view/capture` instead). The Game View is not opened or focused in that case
- Captures only the game image at the Game View resolution (e.g. 1920x1080), including Screen Space - Overlay UI and without the editor chrome
- Opens the Game View if none is open (`EditorWindow.GetWindow`) and focuses it first (it only renders while visible), then on a later main thread tick reads the focused Game View's render target (`PlayModeView.m_TargetTexture`, accessed via reflection)
- On graphics APIs whose UV origin is at the top (`SystemInfo.graphicsUVStartsAtTop`, e.g. Metal / Direct3D / Vulkan), the render target is stored upside down, so it is flipped vertically before encoding
- Returns 400 if the Game View is not open or has not been rendered yet

Response: `{ "pngDataBase64": "<base64>" }` (`Content-Type: application/json`)

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

#### GET `/game-view/scale`
Gets the current Game View scale (zoom factor, `1.0` = 100%) together with the valid range. The range is dynamic and depends on the window size and the selected resolution.

Response:
```json
{"scale": 1.0, "minScale": 0.5, "maxScale": 10.0}
```

#### POST `/game-view/scale`
Sets the Game View scale (zoom factor). The value is clamped to the valid range, and the applied (clamped) scale is returned.

Request body:
```json
{"scale": 2.0}
```

Response: `{"success": true, "scale": 2.0}`

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

- `x`, `y`: screen coordinates in pixels. The origin (0, 0) is the bottom-left of the screen. X increases to the right, Y increases upward. The value range depends on the Game View resolution (e.g. for 800x600: x: 0–800, y: 0–600). Same coordinate system as `Mouse.current.position.ReadValue()`. Note: images from `capture_game_view` are at the Game View resolution with a top-left origin and Y increasing downward, so a pixel (px, py) in the image corresponds to x = px, y = imageHeight - py.
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

#### GET `/timeline/tracks?instanceId=12345`
Returns the tracks and clips of a Timeline. Specify either `instanceId` (a GameObject with a PlayableDirector) or `assetPath` (e.g. `?assetPath=Assets/Timelines/MyTimeline.playable`).

Query parameters:
- `instanceId`: instanceId of the GameObject that has the PlayableDirector. Takes precedence over `assetPath`
- `assetPath`: asset path of the TimelineAsset. Used when `instanceId` is omitted or 0

Response:
```json
{
  "assetPath": "Assets/Timelines/MyTimeline.playable",
  "duration": 3.0,
  "frameRate": 60,
  "tracks": [
    {
      "index": 0,
      "name": "Anim",
      "type": "UnityEngine.Timeline.AnimationTrack",
      "groupName": "",
      "muted": false,
      "locked": false,
      "bindingInstanceId": 12345,
      "bindingName": "Panel",
      "bindingType": "UnityEngine.Animator",
      "clips": [
        {
          "index": 0,
          "displayName": "FadeIn",
          "assetType": "UnityEngine.Timeline.AnimationPlayableAsset",
          "start": 1.0,
          "duration": 2.0,
          "timeScale": 1.0,
          "clipIn": 0.0,
          "easeInDuration": 0.0,
          "easeOutDuration": 0.0,
          "preExtrapolation": "Hold",
          "postExtrapolation": "Hold",
          "animationClipPath": "Assets/Animations/FadeIn.anim"
        }
      ]
    }
  ]
}
```

- `tracks[].index` / `clips[].index`: the `trackIndex` / `clipIndex` used by the other Timeline endpoints. Tracks are the output tracks (tracks inside groups included, group tracks themselves excluded)
- `groupName`: name of the parent group track; empty at the root
- `bindingInstanceId` / `bindingName` / `bindingType`: the bound object. `0` / empty when unbound or when read by `assetPath`
- `animationClipPath`: AnimationClip used by the clip; empty when none
- Serialized properties are not included; read them with `/timeline/track/properties` and `/timeline/clip/properties`

#### GET `/timeline/track/properties?instanceId=12345&trackIndex=0`
Returns the top-level visible serialized properties of a track (`m_Script` excluded), in the same format as `get_component_properties`. The paths and values can be passed to `/timeline/track/property/set`.

Query parameters:
- `instanceId` / `assetPath`: the target timeline, same as `/timeline/tracks`
- `trackIndex`: required

Response:
```json
{
  "typeName": "UnityEngine.Timeline.AnimationTrack",
  "properties": [{"path": "m_TrackOffset", "type": "Enum", "value": "Apply Transform Offsets"}]
}
```

- Hidden properties (e.g. `m_Muted`) are not listed but can still be set

#### GET `/timeline/clip/properties?instanceId=12345&trackIndex=0&clipIndex=0`
Returns the top-level visible serialized properties of the clip's content (its PlayableAsset), in the same format as `/timeline/track/properties`. The paths and values can be passed to `/timeline/clip/property/set`.

Query parameters:
- `instanceId` / `assetPath`: the target timeline, same as `/timeline/tracks`
- `trackIndex`, `clipIndex`: required

Response:
```json
{
  "typeName": "UnityEngine.Timeline.AnimationPlayableAsset",
  "properties": [{"path": "m_Clip", "type": "ObjectReference", "value": "Assets/Animations/FadeIn.anim"}]
}
```

- ExposedReference values are resolved through the PlayableDirector, so they are `null` when read by `assetPath`
- Returns 400 when the clip has no PlayableAsset

#### POST `/timeline/clip/modify`
Changes the timing and settings of a clip. Only the fields present in the body are changed. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "clipIndex": 0, "start": 0.5, "duration": 1.5, "easeInDuration": 0.25, "postExtrapolation": "Loop"}
```

- `start` (>= 0), `duration` (> 0), `timeScale` (> 0), `clipIn` (>= 0), `easeInDuration` / `easeOutDuration` (>= 0): seconds (timeScale is a multiplier)
- `preExtrapolation` / `postExtrapolation`: `None`, `Hold`, `Loop`, `PingPong`, or `Continue` (case-insensitive)
- `displayName`: display name of the clip
- Returns 400 when a value is out of range, or when the clip type does not support the change (`timeScale`, `clipIn`, ease, and extrapolation depend on the clip's `ClipCaps`; e.g. Activation / Control clips have no extrapolation). Ease durations are clamped to the clip duration by Timeline
- Extrapolation modes have internal setters in the Timeline package, so they are set through reflection

Response: `{"success": true}`

#### POST `/timeline/clip/property/set`
Sets a serialized property on the clip's content (its PlayableAsset), e.g. the AnimationClip of an Animation clip (`m_Clip`) or the settings of a custom clip. Timing and settings of the TimelineClip itself (start, duration, ease, extrapolation) are changed with `/timeline/clip/modify` instead. Uses `SerializedObject(asset, director)` so ExposedReference properties are resolved through the PlayableDirector. Values use the same format as `set_component_property`. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "clipIndex": 0, "propertyPath": "m_Clip", "value": "Assets/Animations/FadeIn.anim"}
```

- Assigning `m_Clip` does not change the clip duration; use `/timeline/clip/modify` to adjust it

Response: `{"success": true}`

#### POST `/timeline/track/property/set`
Sets a serialized property on a track (e.g. `m_TrackOffset`, `m_Position`, `m_EulerAngles` of an Animation track, or `m_Muted`). Values use the same format as `set_component_property`. Undo-supported.

Request body:
```json
{"instanceId": 12345, "trackIndex": 0, "propertyPath": "m_Muted", "value": "true"}
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

#### POST `/timeline/evaluate`
Evaluates a Timeline at the specified time without playing it, so the scene reflects the state at that moment. Useful for inspecting (or capturing) an intermediate state of a Timeline.

Request body:
```json
{"instanceId": 12345, "time": 1.5}
```

- `instanceId`: required. instanceId of the GameObject that has the PlayableDirector
- `time`: required. Time in seconds to evaluate at. Must be a non-negative finite number (400 otherwise)
- Edit Mode (including Prefab Mode): opens the Timeline window (`TimelineEditor.GetOrCreateWindow()`), sets the director to it, and moves the playhead with `TimelinePlaybackControls.SetCurrentTime()`. Animated values are applied via the Timeline preview (AnimationMode), so they are reverted when the preview ends and are not written into the scene
- Play Mode: sets `PlayableDirector.time` and calls `PlayableDirector.Evaluate()`
- Returns 400 if the PlayableDirector has no PlayableAsset assigned

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

- **Use case layer**: `EditorUseCase`, `GameObjectUseCase`, `ComponentUseCase`, `SceneUseCase`, `PrefabUseCase`, `TestUseCase`, `ConsoleUseCase`, `AssetUseCase`, `MenuItemUseCase`, `SceneViewUseCase`, `GameViewUseCase`, `InputUseCase`, `TimelineUseCase`
- **Infrastructure**: `HttpRequestHandler`, `UnityServerUrlProvider`, `HttpResponseMessageExtensions`
- **DI extension**: `ServiceCollectionExtensions.AddUniCortexCore()` registers all use cases and infrastructure in one call

Each use case receives `IHttpClientFactory` and `IUnityServerUrlProvider` via constructor DI and communicates with the Unity Editor HTTP server. Return values are `string` (JSON or message) or `byte[]` (captured images). Exceptions propagate to the caller as-is.

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

### MCP Tools (45 tools total)

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
| `get_hierarchy` | GET `/hierarchy` | Get the GameObject hierarchy of every loaded scene (or the Prefab) as a tree per scene |

#### GameObject (5)

| Tool | API | Description |
|------|-----|-------------|
| `find_game_objects` | GET `/gameobjects` | Search every loaded scene with the Hierarchy window's query syntax (name, component type, references) |
| `create_gameobject` | POST `/gameobject/create` | Create a GameObject (parent, sibling index, and RectTransform specification supported) |
| `delete_gameobject` | POST `/gameobject/delete` | Delete a GameObject |
| `modify_gameobject` | POST `/gameobject/modify` | Rename, enable/disable, reparent, reorder siblings, change tag/layer |
| `duplicate_game_object` | POST `/gameobject/duplicate` | Duplicate a GameObject (deep copy of children and components) |

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

#### View (7)

| Tool | API | Description |
|------|-----|-------------|
| `focus_scene_view` | POST `/scene-view/focus` | Switch focus to the Scene View |
| `capture_scene_view` | GET `/scene-view/capture` | Capture the Scene View (Edit Mode and Play Mode, including Prefab Mode) |
| `focus_game_view` | POST `/game-view/focus` | Switch focus to the Game View |
| `capture_game_view` | GET `/game-view/capture` | Capture the Game View (Play Mode only) |
| `get_game_view_size` | GET `/game-view/size` | Get the current Game View size |
| `get_game_view_size_list` | GET `/game-view/size/list` | Get the list of available Game View sizes |
| `set_game_view_size` | POST `/game-view/size` | Set the Game View resolution by index |

#### Input (2)

| Tool | API | Description |
|------|-----|-------------|
| `send_key_event` | POST `/input/key` | Send a key event through the Input System (requires com.unity.inputsystem) |
| `send_mouse_event` | POST `/input/mouse` | Send a mouse event through the Input System (requires com.unity.inputsystem) |

#### Timeline (15)

| Tool | API | Description |
|------|-----|-------------|
| `create_timeline` | POST `/timeline/create` | Create a TimelineAsset (requires com.unity.timeline) |
| `get_timeline_tracks` | GET `/timeline/tracks` | Get an overview of the tracks and clips of a Timeline (requires com.unity.timeline) |
| `get_timeline_track_properties` | GET `/timeline/track/properties` | Get the serialized properties of a track (requires com.unity.timeline) |
| `get_timeline_clip_properties` | GET `/timeline/clip/properties` | Get the serialized properties of a clip's PlayableAsset (requires com.unity.timeline) |
| `add_timeline_track` | POST `/timeline/track/add` | Add a track to a TimelineAsset (requires com.unity.timeline) |
| `remove_timeline_track` | POST `/timeline/track/remove` | Remove a track from a TimelineAsset (requires com.unity.timeline) |
| `bind_timeline_track` | POST `/timeline/track/bind` | Set a track binding (requires com.unity.timeline) |
| `add_timeline_clip` | POST `/timeline/clip/add` | Add a clip to a track (requires com.unity.timeline) |
| `remove_timeline_clip` | POST `/timeline/clip/remove` | Remove a clip from a track (requires com.unity.timeline) |
| `modify_timeline_clip` | POST `/timeline/clip/modify` | Change a clip's timing, ease, and extrapolation (requires com.unity.timeline) |
| `set_timeline_clip_property` | POST `/timeline/clip/property/set` | Set a serialized property on a clip's PlayableAsset, e.g. its AnimationClip (requires com.unity.timeline) |
| `set_timeline_track_property` | POST `/timeline/track/property/set` | Set a serialized property on a track (requires com.unity.timeline) |
| `play_timeline` | POST `/timeline/play` | Start Timeline playback (requires com.unity.timeline) |
| `stop_timeline` | POST `/timeline/stop` | Stop Timeline playback (requires com.unity.timeline) |
| `evaluate_timeline` | POST `/timeline/evaluate` | Evaluate a Timeline at the specified time without playing it (requires com.unity.timeline) |

#### Extension (dynamic)

Extensions are discovered dynamically from the Unity Editor's `GET /extensions/list` when the MCP server starts. They are integrated with the existing static tools via `WithListToolsHandler` / `WithCallToolHandler`.

Users implement an `ExtensionHandler`-derived class on the Unity Editor side and define input parameters in C# via `ExtensionSchema`.

#### Design Decisions

**Included because:**
- `remove_component` — Symmetric with `add_component`. Undo-supported, so it's safe to remove
- Separation of `find_game_objects` and `get_component_properties` — The former returns GameObject summaries (type list only); the latter returns details for a specific component. This avoids returning a flood of properties at once
- `execute_menu_item` — A general escape hatch for edge cases that dedicated tools don't cover
- `capture_game_view` / `capture_scene_view` — Required for multimodal AI agents to inspect state visually. Split by view so that the captured view is explicit: the Game View only in Play Mode, and the Scene View (including Prefab Mode) in any mode
- `focus_scene_view` / `focus_game_view` — Switch the view the user sees in the Editor
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
game-object find|create|delete|modify
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
scene-view focus|capture
game-view focus|capture
game-view size get|list|set
input send-key|send-mouse
timeline create|play|stop
timeline track list|add|remove|bind
timeline track property list|set
timeline clip add|remove|modify
timeline clip property list|set
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
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- game-object find --query "t:Camera"
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- scene-view capture ./sceneview.png
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

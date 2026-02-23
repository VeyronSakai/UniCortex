# UniCortex 仕様書

## 概要

UniCortex は、Unity Editor を外部から操作するためのツールキットです。
Unity Editor 内に HTTP サーバーを埋め込み、MCP サーバーを介して AI エージェントが直接 Editor を制御します。

AI エージェント（Claude Code, Codex CLI 等）が MCP プロトコルを通じて Unity Editor を操作することを主な目的としています。

## 設計原則

- **C# のみで完結**: Python, Node.js 等の外部ランタイムに依存しない
- **MCP プロトコル対応**: AI エージェントが MCP を通じて直接操作可能
- **REST API も維持**: curl 等での直接アクセスも引き続き可能
- **UPM パッケージとして配布**

## 名前

- GitHub リポジトリ: `UniCortex`
- UPM パッケージ名: `com.veyron-sakai.uni-cortex`
- MCP サーバー起動: `dotnet run --project <path>/Tools~/UniCortex.Mcp/`

---

## ディレクトリ構成

```
UniCortex/
├── package.json
├── README.md
├── LICENSE
├── Editor/
│   ├── UniCortex.Editor.asmdef
│   ├── AssemblyInfo.cs
│   ├── EntryPoint.cs                      ← Handler 登録・サーバー起動
│   ├── Domains/
│   │   ├── Interfaces/
│   │   │   ├── ICompilationPipeline.cs
│   │   │   ├── IEditorApplication.cs
│   │   │   ├── IHttpServer.cs
│   │   │   ├── IMainThreadDispatcher.cs
│   │   │   ├── IRequestContext.cs
│   │   │   └── IRequestRouter.cs
│   │   └── Models/
│   │       ├── ApiRoutes.cs               ← ルート定数定義
│   │       ├── ErrorResponse.cs
│   │       ├── HttpMethodType.cs
│   │       ├── *Response.cs               ← 各エンドポイントのレスポンス DTO
│   │       └── UnityServerConfig.cs
│   ├── Handlers/
│   │   └── Editor/
│   │       ├── PingHandler.cs
│   │       ├── PlayHandler.cs
│   │       ├── StopHandler.cs
│   │       ├── PauseHandler.cs
│   │       ├── ResumeHandler.cs
│   │       ├── EditorStatusHandler.cs
│   │       └── DomainReloadHandler.cs
│   ├── Infrastructures/
│   │   ├── CompilationPipelineAdapter.cs  ← CompilationPipeline ラッパー
│   │   ├── EditorApplicationAdapter.cs    ← EditorApplication ラッパー
│   │   ├── HttpListenerRequestContext.cs
│   │   ├── HttpListenerServer.cs          ← HttpListener HTTP サーバー
│   │   ├── MainThreadDispatcher.cs        ← メインスレッドディスパッチ
│   │   └── RequestRouter.cs              ← パスルーティング
│   ├── UseCases/
│   │   ├── PingUseCase.cs
│   │   ├── PlayUseCase.cs
│   │   ├── StopUseCase.cs
│   │   ├── PauseUseCase.cs
│   │   ├── ResumeUseCase.cs
│   │   ├── GetEditorStatusUseCase.cs
│   │   └── RequestDomainReloadUseCase.cs
│   └── Settings/
│       ├── UniCortexSettings.cs
│       ├── UniCortexSettingsProvider.cs   ← Project Settings UI
│       └── ServerUrlFile.cs               ← Library/UniCortex/config.json 操作
├── Tools~/
│   └── UniCortex.Mcp/
│       ├── UniCortex.Mcp.csproj
│       ├── Program.cs
│       ├── Domains/
│       │   └── Interfaces/
│       │       └── IUnityServerUrlProvider.cs
│       ├── Extensions/
│       │   └── HttpResponseMessageExtensions.cs
│       ├── Infrastructures/
│       │   ├── HttpRequestHandler.cs
│       │   └── UnityServerUrlProvider.cs
│       ├── Tools/
│       │   └── EditorTools.cs             ← MCP ツール定義
│       └── UseCases/
│           └── DomainReloadUseCase.cs
├── Tests/
│   └── Editor/
│       ├── UniCortex.Editor.Tests.asmdef
│       ├── TestDoubles/
│       │   ├── FakeMainThreadDispatcher.cs
│       │   ├── FakeRequestContext.cs
│       │   ├── SpyCompilationPipeline.cs
│       │   └── SpyEditorApplication.cs
│       ├── UseCases/
│       │   └── *UseCaseTest.cs            ← UseCase 単体テスト
│       └── Presentations/
│           └── *HandlerTest.cs            ← Handler 単体テスト
└── Documentations~/
    └── SPEC.md                            ← この文書
```

- `Editor/` — Unity Editor 拡張。asmdef で `includePlatforms: ["Editor"]`
- `Tools~/` — `~` サフィックスにより Unity のインポート対象外。.NET 8 MCP サーバープロジェクト

---

## コンポーネント 1: Unity Editor HTTP サーバー

### 技術要素

- `System.Net.HttpListener` で `http://localhost:<port>/` をリッスン
- ポートは Editor 起動時にランダムな空きポートを自動割り当て（`TcpListener` port 0 で取得）
- `SessionState` でポート番号をドメインリロード間で維持（Editor 再起動時のみ変わる）
- `[InitializeOnLoad]` で Editor 起動時に自動開始
- `EditorApplication.update` + `ConcurrentQueue<Action>` でメインスレッドディスパッチ
- `AssemblyReloadEvents.beforeAssemblyReload` で graceful shutdown、リロード後に再起動
- サーバー起動成功時に `Library/UniCortex/config.json` へ URL を書き出す
- `EditorApplication.quitting` で `Library/UniCortex/config.json` を削除

### URL ファイル

`Library/UniCortex/config.json` にサーバーの URL（例: `http://localhost:54321`）を書き出す。

- プロジェクト固有（`Library/` 以下）なので複数 Unity インスタンスでも独立
- `Library/` は通常 `.gitignore` 対象なのでリポジトリには含まれない
- MCP サーバーが `UNICORTEX_PROJECT_PATH` 環境変数経由でこのファイルを読む

### 設定（ScriptableSingleton）

| 項目 | デフォルト | 説明 |
|------|----------|------|
| AutoStart | true | 自動開始 |

Project Settings UI（`Project/UniCortex`）から変更可能。現在のポート番号は同画面に読み取り専用で表示。

### メインスレッドディスパッチ

Unity API はメインスレッドからのみ呼び出し可能。HttpListener のコールバックはスレッドプールで実行されるため、以下のパターンでブリッジする:

1. HTTP スレッドで `MainThreadDispatcher.RunOnMainThread<T>(Func<T> func)` を呼ぶ
2. `TaskCompletionSource<T>` を作成し `ConcurrentQueue` にエンキュー
3. メインスレッド（`EditorApplication.update`）でデキュー → `func()` 実行 → `tcs.SetResult()`
4. HTTP スレッドで await 完了 → レスポンスを返す

---

### JSON シリアライズ

リクエスト/レスポンスの JSON シリアライズには DTO クラスを使用する。

- `Editor/Domains/Models/` に配置。namespace: `UniCortex.Editor.Domains.Models`
- `[Serializable]` 属性 + public fields（camelCase）
- Unity 依存（`using UnityEngine` 等）を含めないこと（MCP サーバーと共有するため）
- Unity 側: `JsonUtility.ToJson()` / `JsonUtility.FromJson<T>()`
- MCP サーバー側: `System.Text.Json` + `JsonSerializerOptions { IncludeFields = true }`
- MCP サーバーの .csproj で `<Compile Include="../../Editor/Domains/Models/**/*.cs" LinkBase="Models" />` としてソース共有

---

## API エンドポイント

レスポンスは常に `application/json; charset=utf-8`。
エラー時: HTTP ステータスコード + `{"error": "メッセージ"}`
シーン変更操作はすべて Undo 対応する。

### Editor 制御

#### GET `/editor/ping`

サーバー疎通確認。**Unity Console に `pong` とログ出力**し、レスポンスを返す。

レスポンス:
```json
{"status": "ok", "message": "pong"}
```

#### GET `/editor/status`

エディターの現在の状態を取得する。MCP ツール内部でのポーリングにも使用。

レスポンス:
```json
{"isPlaying": false, "isPaused": false}
```

#### POST `/editor/play`

Play モードを開始する。`EditorApplication.isPlaying = true`

レスポンス: `{"success": true}`

#### POST `/editor/stop`

Play モードを停止する。`EditorApplication.isPlaying = false`

レスポンス: `{"success": true}`

#### POST `/editor/pause`

Play モードを一時停止する。`EditorApplication.isPaused = true`

レスポンス: `{"success": true}`

#### POST `/editor/resume`

Play モードの一時停止を解除する。`EditorApplication.isPaused = false`

レスポンス: `{"success": true}`

#### POST `/editor/domain-reload`

ドメインリロード（スクリプト再コンパイル）を要求する。`CompilationPipeline.RequestScriptCompilation()`

レスポンス: `{"success": true}`

#### POST `/editor/undo`

直前の操作を Undo する。`Undo.PerformUndo()`

レスポンス: `{"success": true}`

#### POST `/editor/redo`

Undo した操作を Redo する。`Undo.PerformRedo()`

レスポンス: `{"success": true}`

### シーン

#### POST `/scene/open`

シーンを開く。`EditorSceneManager.OpenScene()`

リクエストボディ:
```json
{"scenePath": "Assets/Scenes/Main.unity"}
```

レスポンス: `{"success": true}`

#### POST `/scene/save`

開いているシーンを保存する。`EditorSceneManager.SaveOpenScenes()`

レスポンス: `{"success": true}`

#### GET `/scene/hierarchy`

現在のシーンの GameObject 階層をツリー構造で返す。シーン情報を含む。

レスポンス:
```json
{
  "sceneName": "SampleScene",
  "scenePath": "Assets/Scenes/SampleScene.unity",
  "gameObjects": [
    {
      "name": "Main Camera",
      "instanceId": 10200,
      "activeSelf": true,
      "components": ["Transform", "Camera", "AudioListener"],
      "children": []
    },
    {
      "name": "Canvas",
      "instanceId": 10300,
      "activeSelf": true,
      "components": ["RectTransform", "Canvas"],
      "children": [
        {
          "name": "Button",
          "instanceId": 10400,
          "activeSelf": true,
          "components": ["RectTransform", "Image", "Button"],
          "children": []
        }
      ]
    }
  ]
}
```

### GameObject

#### GET `/gameobject/find`

シーン内の GameObject を検索する。

クエリパラメータ（いずれか 1 つ以上指定）:
- `name`: 名前で検索（部分一致）
- `tag`: タグで検索（完全一致）
- `componentType`: 指定コンポーネントを持つ GameObject を検索

レスポンス:
```json
{
  "gameObjects": [
    {"name": "Player", "instanceId": 10500, "activeSelf": true}
  ]
}
```

#### POST `/gameobject/create`

GameObject を作成する。`Undo.RegisterCreatedObjectUndo` で Undo 対応。

リクエストボディ:
```json
{
  "name": "MyCube",
  "primitive": "Cube"
}
```

- `name`: 作成する GameObject の名前（必須）
- `primitive`: `PrimitiveType` の名前。省略時は空の GameObject を作成
  - 有効値: `Cube`, `Sphere`, `Capsule`, `Cylinder`, `Plane`, `Quad`

レスポンス:
```json
{"name": "MyCube", "instanceId": 12345}
```

#### POST `/gameobject/delete`

GameObject を削除する。`Undo.DestroyObjectImmediate` で Undo 対応。

リクエストボディ: `{"instanceId": 12345}`

レスポンス: `{"success": true}`

#### GET `/gameobject/info?instanceId=12345`

指定した GameObject の基本情報とコンポーネント型一覧を返す（軽量）。詳細なプロパティは `/component/properties` で取得する。

レスポンス:
```json
{
  "name": "MyCube",
  "instanceId": 12345,
  "activeSelf": true,
  "tag": "Untagged",
  "layer": 0,
  "components": [
    {"type": "Transform", "index": 0},
    {"type": "MeshFilter", "index": 1},
    {"type": "MeshRenderer", "index": 2},
    {"type": "BoxCollider", "index": 3}
  ]
}
```

#### POST `/gameobject/modify`

GameObject のプロパティを変更する。指定したフィールドのみ更新。`Undo.RecordObject` で Undo 対応。

リクエストボディ:
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

すべてのフィールド（`instanceId` 以外）は任意。`parentInstanceId` に `0` を指定するとルートに移動。

レスポンス: `{"success": true}`

### コンポーネント

#### POST `/component/add`

GameObject にコンポーネントを追加する。`Undo.AddComponent` で Undo 対応。

リクエストボディ: `{"instanceId": 12345, "componentType": "Rigidbody"}`

レスポンス: `{"success": true}`

#### POST `/component/remove`

GameObject からコンポーネントを削除する。`Undo.DestroyObjectImmediate` で Undo 対応。

リクエストボディ: `{"instanceId": 12345, "componentType": "Rigidbody", "componentIndex": 0}`

- `componentIndex`: 同じ型のコンポーネントが複数ある場合のインデックス（デフォルト: 0）

レスポンス: `{"success": true}`

#### GET `/component/properties?instanceId=12345&componentType=Transform`

指定コンポーネントのシリアライズ済みプロパティを返す。

クエリパラメータ:
- `instanceId`: 対象 GameObject の instanceId（必須）
- `componentType`: コンポーネント型名（必須）
- `componentIndex`: 同型が複数ある場合のインデックス（任意、デフォルト: 0）

レスポンス:
```json
{
  "componentType": "Transform",
  "properties": [
    {"path": "m_LocalPosition", "type": "Vector3", "value": {"x": 0, "y": 1, "z": 0}},
    {"path": "m_LocalRotation", "type": "Quaternion", "value": {"x": 0, "y": 0, "z": 0, "w": 1}},
    {"path": "m_LocalScale", "type": "Vector3", "value": {"x": 1, "y": 1, "z": 1}}
  ]
}
```

#### POST `/component/set-property`

コンポーネントのシリアライズ済みプロパティを変更する。`SerializedObject` / `SerializedProperty` API を使用し、`Undo` に自動記録される。

リクエストボディ:
```json
{
  "instanceId": 12345,
  "componentType": "Transform",
  "propertyPath": "m_LocalPosition.x",
  "value": "1.5"
}
```

- `propertyPath`: Unity の `SerializedProperty.propertyPath` 形式
- `value`: 文字列で指定。型は `SerializedProperty.propertyType` から自動判定

レスポンス: `{"success": true}`

### Prefab

#### POST `/prefab/create`

シーン内の GameObject を Prefab アセットとして保存する。`PrefabUtility.SaveAsPrefabAsset()`

リクエストボディ:
```json
{"instanceId": 12345, "assetPath": "Assets/Prefabs/MyCube.prefab"}
```

レスポンス: `{"success": true}`

#### POST `/prefab/instantiate`

Prefab をシーンにインスタンス化する。`PrefabUtility.InstantiatePrefab()` + `Undo.RegisterCreatedObjectUndo`

リクエストボディ:
```json
{"assetPath": "Assets/Prefabs/MyCube.prefab"}
```

レスポンス:
```json
{"name": "MyCube", "instanceId": 56789}
```

### アセット

#### POST `/asset/refresh`

アセットデータベースをリフレッシュする。`AssetDatabase.Refresh()`

レスポンス: `{"success": true}`

#### POST `/asset/create`

新規アセットを作成する。Material, ScriptableObject 等に対応。

リクエストボディ:
```json
{"type": "Material", "assetPath": "Assets/Materials/NewMat.mat"}
```

- `type`: `Material` またはプロジェクト内の `ScriptableObject` サブクラスの型名

レスポンス: `{"success": true}`

#### GET `/asset/info?assetPath=Assets/Materials/NewMat.mat`

アセットのシリアライズ済みプロパティを返す。Material, ScriptableObject 等に対応。

レスポンス:
```json
{
  "assetPath": "Assets/Materials/NewMat.mat",
  "type": "Material",
  "properties": [
    {"path": "_Color", "type": "Color", "value": {"r": 1, "g": 1, "b": 1, "a": 1}},
    {"path": "_MainTex", "type": "Texture", "value": null}
  ]
}
```

#### POST `/asset/set-property`

アセットのシリアライズ済みプロパティを変更する。`SerializedObject` API を使用。

リクエストボディ:
```json
{
  "assetPath": "Assets/Materials/NewMat.mat",
  "propertyPath": "_Color",
  "value": "{\"r\":1,\"g\":0,\"b\":0,\"a\":1}"
}
```

レスポンス: `{"success": true}`

### コンソール

#### GET `/console/logs`

Unity Console の最新ログエントリを返す。

クエリパラメータ:
- `count`（任意、デフォルト: 100）: 取得件数

レスポンス:
```json
{
  "logs": [
    {
      "message": "NullReferenceException: ...",
      "stackTrace": "at MyScript.Update() ...",
      "type": "Error",
      "timestamp": "2026-02-23T10:30:00"
    }
  ]
}
```

- `type`: `Log`, `Warning`, `Error` のいずれか

#### POST `/console/clear`

Unity Console のログをクリアする。`LogEntries.Clear()`

レスポンス: `{"success": true}`

### ユーティリティ

#### POST `/menu/execute`

Unity のメニューアイテムを実行する。`EditorApplication.ExecuteMenuItem()`

リクエストボディ: `{"menuPath": "GameObject/3D Object/Cube"}`

レスポンス: `{"success": true}`

#### POST `/tests/run`

Unity Test Runner でテストを実行し、完了まで待機して結果を返す。`TestRunnerApi`

リクエストボディ:
```json
{"testMode": "EditMode", "nameFilter": "MyTest"}
```

- `testMode`: `EditMode` または `PlayMode`（任意、デフォルト: `EditMode`）
- `nameFilter`: テスト名フィルタ（任意、省略時は全テスト実行）

レスポンス:
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

#### GET `/editor/screenshot`

Game View のスクリーンショットを取得する。`ScreenCapture.CaptureScreenshotAsTexture()`

レスポンス: PNG 画像バイナリ（`Content-Type: image/png`）

---

## コンポーネント 2: MCP サーバー（dotnet run --project）

### 構成

`AI Agent ←(MCP/stdio)→ MCP Server ←(HTTP)→ Unity Editor HTTP Server`

### 技術スタック

- .NET 8（`net8.0`）
- ModelContextProtocol SDK（0.9.0-preview.1）
- Microsoft.Extensions.Hosting（8.0.0）
- トランスポート: stdio
- `dotnet run --project` で直接起動（事前ビルド不要）

### プロジェクトファイル（UniCortex.Mcp.csproj）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>UniCortex.Mcp</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.9.0-preview.1" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="../../Editor/Domains/Models/**/*.cs" LinkBase="Models" />
  </ItemGroup>
</Project>
```

### エントリポイント（Program.cs）

- `Host.CreateApplicationBuilder` で MCP サーバーを構築
- `.WithStdioServerTransport()` で stdio トランスポート
- `.WithToolsFromAssembly()` でツール自動検出
- `HttpClient` を DI に登録（ベースアドレスは以下の優先順で決定）
  1. 環境変数 `UNICORTEX_URL`（直接 URL 指定）
  2. 環境変数 `UNICORTEX_PROJECT_PATH` 配下の `Library/UniCortex/config.json`
  3. どちらもなければエラーで終了
- ログは stderr に出力（stdout は MCP プロトコル用）

### MCP ツール（全 31 ツール — 実装済み 6 / 未実装 25）

AI エージェントが混乱なく使えるよう、各ツールは明確に異なる操作に対応し重複を排除している。
各ツールは `[McpServerToolType]` クラス内に `[McpServerTool]` メソッドとして定義。
`IHttpClientFactory` をコンストラクタ DI で受け取り、Unity Editor HTTP サーバーにリクエストを送信する。

状態の凡例: 済 = REST API + MCP ツール両方実装済み / 未 = 未実装

#### Editor 制御（8 — 済 6 / 未 2）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `ping_editor` | GET `/editor/ping` | Unity Editor との疎通確認 | 済 |
| `enter_play_mode` | POST `/editor/play` | Play モード開始 | 済 |
| `exit_play_mode` | POST `/editor/stop` | Play モード停止 | 済 |
| `pause_editor` | POST `/editor/pause` | エディターを一時停止 | 済 |
| `resume_editor` | POST `/editor/resume` | エディターの一時停止を解除 | 済 |
| `reload_domain` | POST `/editor/domain-reload` | スクリプト再コンパイル（ドメインリロード） | 済 |
| `undo` | POST `/editor/undo` | 直前の操作を Undo | 未 |
| `redo` | POST `/editor/redo` | Undo した操作を Redo | 未 |

※ `GET /editor/status` は MCP ツールとしては公開しないが、REST API として実装済み（MCP ツール内部のポーリング用）。

#### シーン（3 — 済 0 / 未 3）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `open_scene` | POST `/scene/open` | シーンをパス指定で開く | 未 |
| `save_scene` | POST `/scene/save` | 開いているシーンを保存 | 未 |
| `get_scene_hierarchy` | GET `/scene/hierarchy` | シーン内の GameObject 階層をツリーで取得 | 未 |

#### GameObject（5 — 済 0 / 未 5）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `find_gameobjects` | GET `/gameobject/find` | 名前・タグ・コンポーネント型でシーン内検索 | 未 |
| `create_gameobject` | POST `/gameobject/create` | GameObject を作成（プリミティブ指定可） | 未 |
| `delete_gameobject` | POST `/gameobject/delete` | GameObject を削除 | 未 |
| `get_gameobject_info` | GET `/gameobject/info` | GameObject の基本情報とコンポーネント型一覧を取得 | 未 |
| `modify_gameobject` | POST `/gameobject/modify` | 名前変更・有効/無効・親子関係・タグ・レイヤーの変更 | 未 |

※ `create_gameobject` は `ApiRoutes.cs` にルート定義のみ存在。Handler・UseCase・MCP ツールは未実装。

#### コンポーネント（4 — 済 0 / 未 4）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `add_component` | POST `/component/add` | GameObject にコンポーネントを追加 | 未 |
| `remove_component` | POST `/component/remove` | GameObject からコンポーネントを削除 | 未 |
| `get_component_properties` | GET `/component/properties` | コンポーネントのシリアライズ済みプロパティを取得 | 未 |
| `set_component_property` | POST `/component/set-property` | コンポーネントのプロパティを変更 | 未 |

#### Prefab（2 — 済 0 / 未 2）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `create_prefab` | POST `/prefab/create` | シーン内 GameObject を Prefab アセットとして保存 | 未 |
| `instantiate_prefab` | POST `/prefab/instantiate` | Prefab をシーンにインスタンス化 | 未 |

#### アセット（4 — 済 0 / 未 4）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `refresh_assets` | POST `/asset/refresh` | AssetDatabase をリフレッシュ | 未 |
| `create_asset` | POST `/asset/create` | Material・ScriptableObject 等のアセットを新規作成 | 未 |
| `get_asset_info` | GET `/asset/info` | アセットのシリアライズ済みプロパティを取得 | 未 |
| `set_asset_property` | POST `/asset/set-property` | アセットのプロパティを変更 | 未 |

#### コンソール（2 — 済 0 / 未 2）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `get_console_logs` | GET `/console/logs` | Unity Console のログを取得 | 未 |
| `clear_console_logs` | POST `/console/clear` | Unity Console のログをクリア | 未 |

#### ユーティリティ（3 — 済 0 / 未 3）

| ツール | API | 説明 | 状態 |
|--------|-----|------|------|
| `execute_menu_item` | POST `/menu/execute` | Unity メニューアイテムをパス指定で実行 | 未 |
| `run_tests` | POST `/tests/run` | Test Runner でテスト実行し結果を返す | 未 |
| `capture_screenshot` | GET `/editor/screenshot` | Game View のスクリーンショットを取得 | 未 |

#### 設計判断

**採用理由:**
- `remove_component` — `add_component` との対称性。Undo 対応により安全に削除可能
- `get_gameobject_info` と `get_component_properties` の分離 — 前者は軽量な概要（型一覧のみ）、後者は特定コンポーネントの詳細。大量のプロパティを一度に返すことを防ぐ
- `execute_menu_item` — 専用ツールでカバーできないエッジケースの汎用エスケープハッチ
- `capture_screenshot` — マルチモーダル AI エージェントが視覚的に状態を確認するために必要
- `get_asset_info` / `set_asset_property` — Material・ScriptableObject 操作を汎用的にカバー。専用の Material ツールやシェーダーツールは不要

**不採用:**
- `execute_csharp`（任意 C# 実行） — セキュリティリスクが高い。`execute_menu_item` で大半のエッジケースをカバー可能
- `get_editor_status` — MCP ツールとしては不要。REST API（`GET /editor/status`）として内部ポーリング用に存在
- 専用 Material / シェーダーツール — `create_asset` + `set_asset_property` で汎用対応。シェーダーファイルはエージェントがファイルシステムで直接編集可能
- `find_assets` — エージェントはファイルシステムを直接検索可能
- 専用 Transform ツール — `set_component_property` で汎用対応

---

## MCP サーバーセットアップ

Unity プロジェクトのルートに `.mcp.json` を配置するだけで利用可能。事前のビルドやツールインストールは不要。

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

`/path/to/your/unity/project` を Unity プロジェクトの絶対パスに置き換える。`args` の `--project` と `UNICORTEX_PROJECT_PATH` の両方に同じパスを設定する必要がある（`args` 内での環境変数展開は MCP クライアントが対応していないため）。

`dotnet run` が初回実行時に自動でビルドし、MCP サーバーを起動する。

### URL 指定方法のまとめ

| 方法 | 設定 | 優先度 |
|------|------|--------|
| 直接 URL 指定 | `UNICORTEX_URL=http://localhost:XXXXX` | 高 |
| プロジェクトパス指定 | `UNICORTEX_PROJECT_PATH=/path/to/project` | 低 |

どちらも未設定の場合、MCP サーバーはエラーで終了する。

---

## UPM パッケージ（package.json）

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

### Assembly Definition（UniCortex.Editor.asmdef）

```json
{
  "name": "UniCortex.Editor",
  "rootNamespace": "UniCortex.Editor",
  "includePlatforms": ["Editor"]
}
```

---

## テスト規約

### UseCase 単体テスト

UseCase クラスを新規作成・変更する際は、必ず対応する単体テストを `Tests/Editor/UseCases/` に作成すること。

- テストクラス名: `<UseCase名>Test`（例: `PlayUseCaseTest`）
- namespace: `UniCortex.Editor.Tests.UseCases`
- `FakeMainThreadDispatcher` でディスパッチャをスタブ化し、`CallCount` で呼び出し回数を検証する
- Unity API に依存するインターフェース（`IEditorApplication`, `ICompilationPipeline` 等）はスパイ（`Tests/Editor/TestDoubles/`）を注入して状態・呼び出しを検証する
- 非同期メソッドは `.GetAwaiter().GetResult()` で同期実行する（Unity Test Framework 1.1.x 互換のため）

---

## 使用例

```bash
# ポート番号は Project Settings > UniCortex または Library/UniCortex/config.json で確認
# config.json から server_url を取得する例:
URL=$(grep -o '"server_url":"[^"]*"' Library/UniCortex/config.json | cut -d'"' -f4)

# curl で直接 API を呼ぶことも可能
curl ${URL}/editor/ping
curl -X POST ${URL}/editor/play
```

MCP 経由の操作は AI エージェント（Claude Code 等）の MCP 設定に追加することで利用可能。

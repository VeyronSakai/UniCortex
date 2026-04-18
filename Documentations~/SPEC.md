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
├── Editor/                      ← Unity Editor 拡張
│   ├── Domains/
│   │   ├── Interfaces/          ← Unity API のインターフェース抽象
│   │   └── Models/              ← DTO・ルート定数（Core と共有）
│   ├── Handlers/                ← HTTP リクエストハンドラー
│   ├── Infrastructures/         ← HttpListener, MainThreadDispatcher 等
│   └── UseCases/                ← ビジネスロジック
├── Tools~/
│   ├── UniCortex.sln            ← ソリューションファイル
│   ├── UniCortex.Core/          ← 共有ライブラリ（ユースケース層 + HTTP インフラ）
│   │   ├── Domains/
│   │   ├── Extensions/
│   │   ├── Infrastructures/
│   │   └── UseCases/            ← 11 ユースケースクラス
│   ├── UniCortex.Mcp/           ← MCP サーバー（Core の薄いラッパー）
│   │   └── Tools/               ← MCP ツール定義
│   ├── UniCortex.Core.Test/     ← Core インテグレーションテスト
│   ├── UniCortex.Cli/           ← CLI ツール
│   │   └── Commands/            ← CLI コマンド定義
├── Tests~/
│   └── Editor/
│       ├── TestDoubles/         ← Fake, Spy 等のテストダブル
│       ├── UseCases/            ← UseCase 単体テスト
│       └── Presentations/       ← Handler 単体テスト
└── Documentations~/
    └── SPEC.md                  ← この文書
```

- `Editor/` — Unity Editor 拡張。asmdef で `includePlatforms: ["Editor"]`
- `Tools~/` — `~` サフィックスにより Unity のインポート対象外。.NET 10 プロジェクト群

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
- MCP サーバー / CLI 側: `System.Text.Json` + `JsonSerializerOptions { IncludeFields = true }`
- UniCortex.Core の .csproj で `<Compile Include="../../Editor/Domains/Models/**/*.cs" LinkBase="Models" />` としてソース共有

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

#### POST `/editor/step`
一時停止中に 1 フレーム進める。`EditorApplication.Step()`。フレーム単位でゲームを制御するために使用する。

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

#### POST `/editor/save`
File/Save を実行し、現在アクティブなステージを保存する。シーン、Prefab、Timeline など File/Save で保存可能なすべてのアセットが対象。`EditorApplication.ExecuteMenuItem("File/Save")`

リクエストボディ: なし

レスポンス: `{"success": true}`

### シーン

#### POST `/scene/create`
新しい空のシーンを作成し、指定されたアセットパスに保存する。

リクエストボディ:
```json
{"scenePath": "Assets/Scenes/NewScene.unity"}
```

レスポンス: `{"success": true}`

#### POST `/scene/open`
シーンを開く。`EditorSceneManager.OpenScene()`

リクエストボディ:
```json
{"scenePath": "Assets/Scenes/Main.unity"}
```

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
シーン内の GameObject を検索する。Unity Search スタイルのクエリ構文をサポート。

クエリパラメータ:
- `query`: 検索クエリ文字列（任意。省略時は全 GameObject を返す）

Unity Search (`SearchService` API) の `scene` プロバイダに委譲。Unity Search のサブフィルター構文をそのままサポートする。

主なクエリ構文:

| トークン | 例 | 説明 |
|---------|---|------|
| プレーンテキスト | `Main Camera` | 名前の部分一致 |
| `t:` | `t:Camera` | コンポーネント型 |
| `tag:` | `tag:resp` | タグ（部分一致） |
| `tag=` | `tag=Player` | タグ（完全一致） |
| `id:` | `id:12345` | instanceId 指定 |
| `layer:` | `layer:5` | レイヤー番号 |
| `path:` | `path:Canvas/Button` | 階層パス |
| `is:` | `is:root` / `is:child` / `is:leaf` / `is:static` | 状態フィルタ |

※ クエリ構文の詳細は Unity 公式の Search 機能ドキュメントを参照。

レスポンス:
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
GameObject を作成する。`Undo.RegisterCreatedObjectUndo` で Undo 対応。

リクエストボディ:
```json
{
  "name": "MyObject"
}
```

- `name`: 作成する GameObject の名前（必須）

レスポンス:
```json
{"name": "MyCube", "instanceId": 12345}
```

#### POST `/gameobject/delete`
GameObject を削除する。`Undo.DestroyObjectImmediate` で Undo 対応。

リクエストボディ: `{"instanceId": 12345}`

レスポンス: `{"success": true}`

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

リクエストボディ: `{"instanceId": 12345, "componentType": "UnityEngine.Rigidbody"}`

レスポンス: `{"success": true}`

#### POST `/component/remove`
GameObject からコンポーネントを削除する。`Undo.DestroyObjectImmediate` で Undo 対応。

リクエストボディ: `{"instanceId": 12345, "componentType": "UnityEngine.Rigidbody", "componentIndex": 0}`

- `componentIndex`: 同じ型のコンポーネントが複数ある場合のインデックス（デフォルト: 0）

レスポンス: `{"success": true}`

#### GET `/component/properties?instanceId=12345&componentType=UnityEngine.Transform`
指定コンポーネントのシリアライズ済みプロパティを返す。

クエリパラメータ:
- `instanceId`: 対象 GameObject の instanceId（必須）
- `componentType`: namespace を含む完全修飾コンポーネント型名（必須）
- `componentIndex`: 同型が複数ある場合のインデックス（任意、デフォルト: 0）

レスポンス:
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

#### POST `/component/set-property`
コンポーネントのシリアライズ済みプロパティを変更する。`SerializedObject` / `SerializedProperty` API を使用し、`Undo` に自動記録される。

リクエストボディ:
```json
{
  "instanceId": 12345,
  "componentType": "UnityEngine.Transform",
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

#### POST `/prefab/open`
Prefab アセットを Prefab Mode で開く。`PrefabStageUtility.OpenPrefab()`

リクエストボディ:
```json
{"assetPath": "Assets/Prefabs/MyCube.prefab"}
```

レスポンス: `{"success": true}`

#### POST `/prefab/close`
Prefab Mode を閉じてメインステージに戻る。`StageUtility.GoToMainStage()`

リクエストボディ: なし

レスポンス: `{"success": true}`

### アセット

#### POST `/asset-database/refresh`
アセットデータベースをリフレッシュする。`AssetDatabase.Refresh()`

レスポンス: `{"success": true}`

### Project Window

#### POST `/project-window/select`
Project ウィンドウで指定アセットを選択し、Project ウィンドウを前面化して ping 表示する。

リクエストボディ:
```json
{"assetPath": "Assets/Scenes/Main.unity"}
```

- `assetPath`: 必須。選択対象のアセットパス

レスポンス:
```json
{"success": true}
```

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
      "type": "Error"
    }
  ]
}
```

- `type`: `Log`, `Warning`, `Error` のいずれか

#### POST `/console/clear`
Unity Console のログをクリアする。`LogEntries.Clear()`

レスポンス: `{"success": true}`

### メニューアイテム

#### POST `/menu-item/execute`
Unity のメニューアイテムを実行する。`EditorApplication.ExecuteMenuItem()`

リクエストボディ: `{"menuPath": "GameObject/3D Object/Cube"}`

レスポンス: `{"success": true}`

#### POST `/tests/run`
Unity Test Runner でテストを実行し、完了まで待機して結果を返す。`TestRunnerApi`

リクエストボディ:
```json
{"testMode": "EditMode", "testNames": ["MyTests.TestA"]}
```

- `testMode`: `EditMode` または `PlayMode`（任意、デフォルト: `EditMode`）
- `testNames`: テスト名の配列（任意）
- `groupNames`: テストグループ名の配列（任意）
- `categoryNames`: テストカテゴリ名の配列（任意）
- `assemblyNames`: テストアセンブリ名の配列（任意）

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

### Screenshot

#### GET `/screenshot/capture`
現在の Unity レンダリング出力のスクリーンショットを取得する。Play Mode 専用。
通常は Game View をキャプチャするが、Game View にフォーカスがない場合や閉じている場合は Scene View がキャプチャされる。

- `ScreenCapture.CaptureScreenshotAsTexture()` で現在のレンダリング出力（UI オーバーレイ含む）をキャプチャ
- Edit Mode 時は 400 エラー

レスポンス: `{ "pngDataBase64": "<base64>" }`（`Content-Type: application/json`）

### View

#### POST `/scene-view/focus`
Scene View にフォーカスを切り替える。

レスポンス: `{"success": true}`

#### POST `/game-view/focus`
Game View にフォーカスを切り替える。

レスポンス: `{"success": true}`

#### GET `/game-view/size`
現在の Game View のサイズ（幅と高さ、ピクセル単位）を取得する。

レスポンス:
```json
{"screenWidth": 1920, "screenHeight": 1080}
```

#### GET `/game-view/size/list`
利用可能な Game View サイズ一覧（ビルトイン＋カスタム）を取得する。

レスポンス:
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
Game View の解像度を設定する。`GET /game-view/size/list` で取得したインデックスを指定する。

リクエストボディ:
```json
{"index": 1}
```

レスポンス: `{"success": true}`

### Recording

Unity Recorder パッケージ（`com.unity.recorder`）を使用した録画機能。Recorder のリストを取得し、Movie Recorder の追加・削除・録画開始・停止を行う。Recorder のリストはドメインリロードでリセットされる。

#### GET `/recorder/all/list`
登録済みの全 Recorder とその設定・エラーを取得する。

レスポンス:
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
Movie Recorder をリストに追加する。Source は Game View 固定、解像度は Game View Resolution 固定。Audio はデフォルト OFF。

リクエストボディ:
```json
{
  "name": "MyRecorder",
  "outputPath": "/path/to/output.mp4",
  "encoder": "UnityMediaEncoder",
  "encodingQuality": "Low",
  "captureAudio": false
}
```
- `name`: 必須。Movie Recorder の名前
- `outputPath`: 必須。出力ファイルパス
- `encoder`: 任意。`"UnityMediaEncoder"`（デフォルト）、`"ProRes"`、`"GIF"`
- `encodingQuality`: 任意。UnityMediaEncoder のみ有効。`"Low"`（デフォルト）、`"Medium"`、`"High"`
- `captureAudio`: 任意。音声キャプチャ。デフォルト `false`
- MP4 時の奇数解像度はエラーとなる。事前に Game View のサイズを偶数に設定すること

レスポンス: `{"name": "MyRecorder"}`

#### POST `/recorder/movie/remove`
指定したインデックスの Movie Recorder をリストから削除する。

リクエストボディ: `{"index": 0}`

- インデックスが範囲外の場合は 400 エラー

レスポンス: `{"success": true}`

#### POST `/recorder/movie/start`
指定したインデックスの Movie Recorder で録画を開始する。Play Mode 専用。Manual モード・Constant フレームレート固定。

- `RecorderController` + `MovieRecorderSettings` で録画
- Play Mode 離脱時は自動的に録画を停止してクリーンアップ
- Unity Recorder 未インストール時は 400 エラー
- Edit Mode 時は 400 エラー
- Movie Recorder にエラーがある場合は 400 エラー

リクエストボディ:
```json
{
  "index": 0,
  "fps": 30
}
```
- `index`: 使用する Movie Recorder のインデックス（`get_all_recorders` で取得）
- `fps`: 任意。デフォルト 30

レスポンス: `{"success": true}`

#### POST `/recorder/movie/stop`
録画を停止し、ファイルを書き出す。

- 録画中でない場合は 400 エラー

レスポンス: `{"outputPath": "/path/to/output.mp4"}`

### 入力

Unity Input System パッケージ（`com.unity.inputsystem`）を使用したデバイスレベルの入力送信。`InputSystem.QueueEvent()` でデバイスレベルのイベントをキューに入れる。Play モード必須。Input System パッケージがインストールされている場合のみ利用可能。

**特徴**: Input System のアクション（`InputAction`, `PlayerInput`）および `Keyboard.current` / `Mouse.current` の状態を直接変更する。レガシー `UnityEngine.Input.GetKey()` / `Input.GetMouseButton()` はトリガーされない。

**オプショナル依存**: `UniCortex.Editor.asmdef` の `versionDefines` により、`com.unity.inputsystem` がインストールされている場合に `UNICORTEX_INPUT_SYSTEM` が定義される。未インストール時はフォールバックアダプターが `NotSupportedException` を返す。

#### POST `/input/key`
Play モード中に Input System 経由でキーボードイベントを送信する。

リクエストボディ:
```json
{"key": "Space", "eventType": "press"}
```

- `key`: 必須。Input System の `Key` enum 名（例: `"Space"`, `"A"`, `"LeftArrow"`, `"Return"`, `"LeftShift"`）
- `eventType`: 任意。`"press"`（デフォルト）または `"release"`

レスポンス: `{"success": true}`

#### POST `/input/mouse`
Play モード中に Input System 経由でマウスイベントを送信する。

リクエストボディ:
```json
{"x": 100.0, "y": 200.0, "button": "left", "eventType": "press"}
```

- `x`, `y`: スクリーン座標（ピクセル単位）。原点 (0, 0) は画面左下。X は右方向、Y は上方向に増加する。値の範囲は Game View の解像度に依存する（例: 800x600 の場合、x: 0–800, y: 0–600）。`Mouse.current.position.ReadValue()` と同じ座標系。注意: `capture_screenshot` のスクリーンショットは左上原点・Y 下方向増加のため、座標変換が必要
- `button`: 任意。`"left"`（デフォルト）, `"right"`, `"middle"`
- `eventType`: 任意。`"click"`（デフォルト、press 後 1 フレーム待機して release）, `"press"`, `"release"`, または `"move"`（位置のみ更新、ボタン操作なし）

レスポンス: `{"success": true}`

### Timeline

Unity Timeline パッケージ（`com.unity.timeline`）を使用した Timeline 制御。PlayableDirector コンポーネントを通じてトラック・クリップ・バインディングの操作を行う。Timeline パッケージがインストールされている場合のみ利用可能。


#### POST `/timeline/create`
TimelineAsset（.playable ファイル）を作成する。

リクエストボディ:
```json
{"assetPath": "Assets/Timelines/MyTimeline.playable"}
```

- `assetPath`: 必須。TimelineAsset の保存先パス

レスポンス:
```json
{"success": true, "assetPath": "Assets/Timelines/MyTimeline.playable"}
```

#### POST `/timeline/track/add`
TimelineAsset にトラックを追加する。`Undo` 対応。

リクエストボディ:
```json
{"instanceId": 12345, "trackType": "UnityEngine.Timeline.AnimationTrack", "trackName": "My Track"}
```

- `trackType`: 必須。完全修飾型名（例: `UnityEngine.Timeline.AnimationTrack`, `UnityEngine.Timeline.AudioTrack`）
- `trackName`: 任意。トラック名

レスポンス: `{"success": true}`

#### POST `/timeline/track/remove`
TimelineAsset からトラックを削除する。`Undo` 対応。

リクエストボディ:
```json
{"instanceId": 12345, "trackIndex": 0}
```

レスポンス: `{"success": true}`

#### POST `/timeline/track/bind`
PlayableDirector のトラックバインディングを設定する。`Undo` 対応。

リクエストボディ:
```json
{"instanceId": 12345, "trackIndex": 0, "targetInstanceId": 67890}
```

レスポンス: `{"success": true}`

#### POST `/timeline/clip/add`
トラックにデフォルトクリップを追加する。クリップ種別はトラック種別に応じて自動決定される。`Undo` 対応。

リクエストボディ:
```json
{"instanceId": 12345, "trackIndex": 0, "start": 1.0, "duration": 3.0, "clipName": "My Clip"}
```

- `trackIndex`: 必須。クリップを追加するトラックのインデックス（0始まり）
- `start`: 任意。クリップの開始時刻（秒）。デフォルト: 0
- `duration`: 任意。クリップの長さ（秒）。0の場合はトラックのデフォルト値を使用
- `clipName`: 任意。クリップの表示名

レスポンス: `{"success": true}`

#### POST `/timeline/clip/remove`
トラックからクリップを削除する。`Undo` 対応。

リクエストボディ:
```json
{"instanceId": 12345, "trackIndex": 0, "clipIndex": 0}
```

レスポンス: `{"success": true}`

#### POST `/timeline/play`
PlayableDirector の Timeline 再生を開始する。

リクエストボディ:
```json
{"instanceId": 12345}
```

- `instanceId`: 必須。PlayableDirector を持つ GameObject の instanceId

レスポンス: `{"success": true}`

#### POST `/timeline/stop`
PlayableDirector の Timeline 再生を停止し、先頭に戻す。

リクエストボディ:
```json
{"instanceId": 12345}
```

- `instanceId`: 必須。PlayableDirector を持つ GameObject の instanceId

レスポンス: `{"success": true}`

### Extension

ユーザーが定義した Extension の一覧取得と実行。`ExtensionHandler` を継承したクラスを Unity Editor 側で実装すると、自動的に発見・登録される。

#### GET `/extensions/list`
登録済みの Extension 一覧を返す。`TypeCache.GetTypesDerivedFrom<ExtensionHandler>()` で発見されたすべての Extension のメタデータを返す。

レスポンス:
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

- `inputSchema`: JSON Schema 文字列。`ExtensionSchema` から自動生成される。パラメータなしの Extension は `null`

#### POST `/extensions/execute`
Extension を名前指定で実行する。`ExtensionHandler.Execute()` はメインスレッドで実行されるため、Unity API の呼び出しが安全に行える。

リクエストボディ:
```json
{"name": "spawn_enemy", "arguments": "{\"prefabPath\":\"Assets/Prefabs/Enemy.prefab\"}"}
```

- `name`: 必須。実行するカスタムツール名
- `arguments`: 任意。JSON 文字列としてのツール引数

レスポンス:
```json
{"result": "Spawned Enemy (instanceId: 12345)"}
```

- Extension が見つからない場合: 404 エラー
- `name` が未指定の場合: 400 エラー

---

## コンポーネント 2: MCP サーバー（dotnet run --project）

### 構成

```
AI Agent ←(MCP/stdio)→ MCP Server ←(HTTP)→ Unity Editor HTTP Server
Terminal ←(CLI)→ CLI Tool ←(HTTP)→ Unity Editor HTTP Server

UniCortex.Core (shared library)
  ├── UniCortex.Mcp (MCP server)
  └── UniCortex.Cli (CLI tool)
```

MCP サーバーと CLI は、共通の HTTP 通信ロジックとサービス層を `UniCortex.Core` ライブラリとして共有する。

### UniCortex.Core（共有ライブラリ）

MCP サーバーと CLI が共有するユースケース層・HTTP インフラストラクチャ。

- **ユースケース層**: `EditorUseCase`, `GameObjectUseCase`, `ComponentUseCase`, `SceneUseCase`, `PrefabUseCase`, `TestUseCase`, `ConsoleUseCase`, `AssetUseCase`, `MenuItemUseCase`, `ScreenshotUseCase`, `SceneViewUseCase`, `GameViewUseCase`, `InputUseCase`, `TimelineUseCase`
- **インフラ**: `HttpRequestHandler`, `UnityServerUrlProvider`, `HttpResponseMessageExtensions`
- **DI 拡張**: `ServiceCollectionExtensions.AddUniCortexCore()` で全ユースケース・インフラを一括登録

各ユースケースは `IHttpClientFactory` と `IUnityServerUrlProvider` をコンストラクタ DI で受け取り、Unity Editor HTTP サーバーと通信する。戻り値は `string`（JSON またはメッセージ）または `byte[]`（スクリーンショット）。例外はそのまま呼び出し元に伝播する。

### MCP サーバー（UniCortex.Mcp）

MCP ツール定義のみを担当する薄いラッパー。各ツールクラスは対応する Core ユースケースをコンストラクタ DI で受け取り、結果を `CallToolResult` でラップする。

### 技術スタック

- .NET 10（`net10.0`）
- ModelContextProtocol SDK（1.0.0）
- Microsoft.Extensions.Hosting（10.0.3）
- UniCortex.Core（ProjectReference）
- トランスポート: stdio
- `dotnet run --project` で直接起動（事前ビルド不要）

### エントリポイント（Program.cs）

- `Host.CreateApplicationBuilder` で MCP サーバーを構築
- `.WithStdioServerTransport()` で stdio トランスポート
- `.WithToolsFromAssembly()` でツール自動検出
- `builder.Services.AddUniCortexCore()` で Core サービスを DI 登録
- URL 決定の優先順:
  1. 環境変数 `UNICORTEX_URL`（直接 URL 指定）
  2. 環境変数 `UNICORTEX_PROJECT_PATH` 配下の `Library/UniCortex/config.json`
  3. どちらもなければエラーで終了
- ログは stderr に出力（stdout は MCP プロトコル用）

### MCP ツール（全 40 ツール）

AI エージェントが混乱なく使えるよう、各ツールは明確に異なる操作に対応し重複を排除している。
各ツールは `[McpServerToolType]` クラス内に `[McpServerTool]` メソッドとして定義。
対応する Core サービスをコンストラクタ DI で受け取り、結果を `CallToolResult` でラップする。

#### Editor 制御（11）

| ツール | API | 説明 |
|--------|-----|------|
| `ping_editor` | GET `/editor/ping` | Unity Editor との疎通確認 |
| `enter_play_mode` | POST `/editor/play` | Play モード開始 |
| `exit_play_mode` | POST `/editor/stop` | Play モード停止 |
| `get_editor_status` | GET `/editor/status` | エディターの現在の状態（Play モード、一時停止）を取得 |
| `pause_editor` | POST `/editor/pause` | エディターを一時停止。`step_editor` と組み合わせてフレーム単位制御 |
| `unpause_editor` | POST `/editor/unpause` | エディターの一時停止を解除 |
| `step_editor` | POST `/editor/step` | 一時停止中に 1 フレーム進める。フレーム単位のゲーム制御用 |
| `reload_domain` | POST `/editor/domain-reload` | スクリプト再コンパイル（ドメインリロード） |
| `undo` | POST `/editor/undo` | 直前の操作を Undo |
| `redo` | POST `/editor/redo` | Undo した操作を Redo |
| `save` | POST `/editor/save` | File/Save を実行し、現在アクティブなステージを保存（シーン、Prefab、Timeline 等） |

#### シーン（3）

| ツール | API | 説明 |
|--------|-----|------|
| `create_scene` | POST `/scene/create` | 新しい空のシーンを作成しアセットパスに保存 |
| `open_scene` | POST `/scene/open` | シーンをパス指定で開く |
| `get_hierarchy` | GET `/hierarchy` | シーンまたは Prefab 内の GameObject 階層をツリーで取得 |

#### GameObject（4）

| ツール | API | 説明 |
|--------|-----|------|
| `find_game_objects` | GET `/gameobjects` | クエリ構文でシーン内検索（名前・タグ・コンポーネント型・instanceId・レイヤー・パス・状態） |
| `create_gameobject` | POST `/gameobject/create` | GameObject を作成（プリミティブ指定可） |
| `delete_gameobject` | POST `/gameobject/delete` | GameObject を削除 |
| `modify_gameobject` | POST `/gameobject/modify` | 名前変更・有効/無効・親子関係・タグ・レイヤーの変更 |

#### コンポーネント（4）

| ツール | API | 説明 |
|--------|-----|------|
| `add_component` | POST `/component/add` | GameObject にコンポーネントを追加 |
| `remove_component` | POST `/component/remove` | GameObject からコンポーネントを削除 |
| `get_component_properties` | GET `/component/properties` | コンポーネントのシリアライズ済みプロパティを取得 |
| `set_component_property` | POST `/component/set-property` | コンポーネントのプロパティを変更 |

#### Prefab（4）

| ツール | API | 説明 |
|--------|-----|------|
| `create_prefab` | POST `/prefab/create` | シーン内 GameObject を Prefab アセットとして保存 |
| `instantiate_prefab` | POST `/prefab/instantiate` | Prefab をシーンにインスタンス化 |
| `open_prefab` | POST `/prefab/open` | Prefab を Prefab Mode で開く |
| `close_prefab` | POST `/prefab/close` | Prefab Mode を閉じてメインステージに戻る |

#### アセット（1）

| ツール | API | 説明 |
|--------|-----|------|
| `refresh_asset_database` | POST `/asset-database/refresh` | AssetDatabase をリフレッシュ |

#### Project Window（1）

| ツール | API | 説明 |
|--------|-----|------|
| `select_project_window_asset` | POST `/project-window/select` | Project ウィンドウでアセットを選択し、前面化して ping 表示 |

#### コンソール（2）

| ツール | API | 説明 |
|--------|-----|------|
| `get_console_logs` | GET `/console/logs` | Unity Console のログを取得 |
| `clear_console_logs` | POST `/console/clear` | Unity Console のログをクリア |

#### テスト（1）

| ツール | API | 説明 |
|--------|-----|------|
| `run_tests` | POST `/tests/run` | Test Runner でテスト実行し結果を返す |

#### メニューアイテム（1）

| ツール | API | 説明 |
|--------|-----|------|
| `execute_menu_item` | POST `/menu-item/execute` | Unity メニューアイテムをパス指定で実行 |

#### スクリーンショット（1）

| ツール | API | 説明 |
|--------|-----|------|
| `capture_screenshot` | GET `/screenshot/capture` | 現在のレンダリング出力のスクリーンショットを取得 |

#### View（5）

| ツール | API | 説明 |
|--------|-----|------|
| `focus_scene_view` | POST `/scene-view/focus` | Scene View にフォーカスを切り替え |
| `focus_game_view` | POST `/game-view/focus` | Game View にフォーカスを切り替え |
| `get_game_view_size` | GET `/game-view/size` | Game View の現在のサイズを取得 |
| `get_game_view_size_list` | GET `/game-view/size/list` | 利用可能な Game View サイズ一覧を取得 |
| `set_game_view_size` | POST `/game-view/size` | Game View の解像度をインデックス指定で設定 |

#### 入力（2）

| ツール | API | 説明 |
|--------|-----|------|
| `send_key_event` | POST `/input/key` | Input System 経由でキーイベントを送信（要 com.unity.inputsystem） |
| `send_mouse_event` | POST `/input/mouse` | Input System 経由でマウスイベントを送信（要 com.unity.inputsystem） |

#### Timeline（8）

| ツール | API | 説明 |
|--------|-----|------|
| `create_timeline` | POST `/timeline/create` | TimelineAsset を作成（要 com.unity.timeline） |
| `add_timeline_track` | POST `/timeline/track/add` | TimelineAsset にトラックを追加（要 com.unity.timeline） |
| `remove_timeline_track` | POST `/timeline/track/remove` | TimelineAsset からトラックを削除（要 com.unity.timeline） |
| `bind_timeline_track` | POST `/timeline/track/bind` | トラックバインディングを設定（要 com.unity.timeline） |
| `add_timeline_clip` | POST `/timeline/clip/add` | トラックにクリップを追加（要 com.unity.timeline） |
| `remove_timeline_clip` | POST `/timeline/clip/remove` | トラックからクリップを削除（要 com.unity.timeline） |
| `play_timeline` | POST `/timeline/play` | Timeline の再生を開始（要 com.unity.timeline） |
| `stop_timeline` | POST `/timeline/stop` | Timeline の再生を停止（要 com.unity.timeline） |

#### Extension（動的）

MCP サーバー起動時に Unity Editor の `GET /extensions/list` から動的に発見される。`WithListToolsHandler` / `WithCallToolHandler` で既存の静的ツールと統合される。

ユーザーは `ExtensionHandler` を継承したクラスを Unity Editor 側に実装し、`ExtensionSchema` で入力パラメータを C# で定義する。

#### 設計判断

**採用理由:**
- `remove_component` — `add_component` との対称性。Undo 対応により安全に削除可能
- `find_game_objects` と `get_component_properties` の分離 — 前者は GameObject の概要（型一覧のみ）、後者は特定コンポーネントの詳細。大量のプロパティを一度に返すことを防ぐ
- `execute_menu_item` — 専用ツールでカバーできないエッジケースの汎用エスケープハッチ
- `capture_screenshot` — マルチモーダル AI エージェントが視覚的に状態を確認するために必要
- `focus_scene_view` / `focus_game_view` — `capture_screenshot` と組み合わせて目的のビューを確実にキャプチャするために必要

**不採用:**
- `execute_csharp`（任意 C# 実行） — セキュリティリスクが高い。`execute_menu_item` で大半のエッジケースをカバー可能
- `get_editor_status` — MCP ツールとしては不要。REST API（`GET /editor/status`）として内部ポーリング用に存在
- 専用 Material / シェーダーツール — シェーダーファイルはエージェントがファイルシステムで直接編集可能
- 専用 ScriptableObject ツール — `.asset` ファイルはエージェントがファイルシステムで直接読み書き可能。`execute_menu_item` でも代替可能
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
      "command": "/bin/bash",
      "args": ["-c", "dotnet run --project /path/to/your/unity/project/Library/PackageCache/com.veyron-sakai.uni-cortex@*/Tools~/UniCortex.Mcp/"],
      "env": {
        "UNICORTEX_PROJECT_PATH": "/path/to/your/unity/project"
      }
    }
  }
}
```

`/path/to/your/unity/project` を Unity プロジェクトの絶対パスに置き換える。`args` の `--project` と `UNICORTEX_PROJECT_PATH` の両方に同じパスを設定する必要がある（`args` 内での環境変数展開は MCP クライアントが対応していないため）。`@*` グロブパターンにより、バージョン番号（`@0.1.0`）でも Git コミットハッシュ（`@7bec663133`）でも自動的にマッチする。

`dotnet run` が初回実行時に自動でビルドし、MCP サーバーを起動する。

### URL 指定方法のまとめ

| 方法 | 設定 | 優先度 |
|------|------|--------|
| 直接 URL 指定 | `UNICORTEX_URL=http://localhost:XXXXX` | 高 |
| プロジェクトパス指定 | `UNICORTEX_PROJECT_PATH=/path/to/project` | 低 |

どちらも未設定の場合、MCP サーバーはエラーで終了する。

---

## コンポーネント 3: CLI ツール（UniCortex.Cli）

ターミナルから Unity Editor を操作するための CLI ツール。Core サービスを直接利用する。

### 技術スタック

- .NET 10（`net10.0`）
- ConsoleAppFramework（5.6.0）
- UniCortex.Core（ProjectReference）
- `dotnet run --project` で直接起動（事前ビルド不要）

### コマンド構造

```
editor ping|play|stop|status|pause|unpause|step|undo|redo|reload-domain
scene create|open|save|hierarchy
gameobject find|create|delete|modify
component add|remove|properties|set-property
prefab create|instantiate|open|close|save
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

### パラメータ規約

- デフォルト値を持たない必須パラメータには `[Argument]` 属性を付与し、位置引数として扱う
- デフォルト値を持つオプショナルパラメータには `[Argument]` を付与しない（`--option-name` 形式の名前付きオプションとなる）

### エントリポイント（Program.cs）

- `ConsoleApp.Create()` で CLI アプリを構築
- `.ConfigureServices()` で `AddUniCortexCore()` を呼び出し DI 登録
- `app.Add<XxxCommands>("prefix")` でコマンドグループを登録
- 各コマンドクラスは対応する Core サービスをコンストラクタ DI で受け取る

### 使用例

```bash
export UNICORTEX_PROJECT_PATH=/path/to/your/unity/project

dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- editor ping
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- scene hierarchy
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- gameobject find --query "t:Camera"
dotnet run --project /path/to/UniCortex/Tools~/UniCortex.Cli/ -- screenshot capture ./screenshot.png
```

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
# ポート番号は Library/UniCortex/config.json で確認
# config.json から server_url を取得する例:
URL=$(grep -o '"server_url":"[^"]*"' Library/UniCortex/config.json | cut -d'"' -f4)

# curl で直接 API を呼ぶことも可能
curl ${URL}/editor/ping
curl -X POST ${URL}/editor/play
```

MCP 経由の操作は AI エージェント（Claude Code 等）の MCP 設定に追加することで利用可能。

# UniCortex 実装タスクリスト

実装状況の一覧。詳細な仕様は [SPEC.md](SPEC.md) を参照。

> 最終更新: 2026-02-23

---

## 実装済み

### REST API エンドポイント（7/32）

| エンドポイント | Handler | UseCase | テスト |
|---------------|---------|---------|--------|
| GET `/editor/ping` | PingHandler | PingUseCase | UseCase + Handler |
| POST `/editor/play` | PlayHandler | PlayUseCase | UseCase |
| POST `/editor/stop` | StopHandler | StopUseCase | UseCase |
| POST `/editor/pause` | PauseHandler | PauseUseCase | UseCase |
| POST `/editor/resume` | ResumeHandler | ResumeUseCase | UseCase |
| POST `/editor/domain-reload` | DomainReloadHandler | RequestDomainReloadUseCase | UseCase |
| GET `/editor/status` | EditorStatusHandler | GetEditorStatusUseCase | UseCase |

### MCP ツール（6/31）

| ツール名 | 対応 API | 状態 |
|----------|---------|------|
| `ping_editor` | GET `/editor/ping` | 済 |
| `enter_play_mode` | POST `/editor/play` | 済 |
| `exit_play_mode` | POST `/editor/stop` | 済 |
| `pause_editor` | POST `/editor/pause` | 済 |
| `resume_editor` | POST `/editor/resume` | 済 |
| `reload_domain` | POST `/editor/domain-reload` | 済 |

### インフラ・基盤

- [x] HttpListener HTTP サーバー（ランダムポート自動割り当て）
- [x] MainThreadDispatcher（ConcurrentQueue + EditorApplication.update）
- [x] RequestRouter（パスルーティング + 404/405 ハンドリング）
- [x] config.json への URL 書き出し（Library/UniCortex/config.json）
- [x] Project Settings UI（AutoStart トグル + ポート表示）
- [x] MCP サーバー（stdio トランスポート + ツール自動検出）
- [x] MCP サーバー URL 解決（UNICORTEX_URL / UNICORTEX_PROJECT_PATH）
- [x] MCP サーバー HTTP リトライ（ドメインリロード対応）
- [x] DTO ソース共有（Editor/Domains/Models → MCP .csproj Link）

---

## 未実装タスク

### Editor 制御（残り 2）

- [ ] POST `/editor/undo` + MCP `undo`
  - `Undo.PerformUndo()` をメインスレッドで実行
  - Handler, UseCase, MCP ツール, テスト
- [ ] POST `/editor/redo` + MCP `redo`
  - `Undo.PerformRedo()` をメインスレッドで実行
  - Handler, UseCase, MCP ツール, テスト

### シーン（残り 3）

- [ ] POST `/scene/open` + MCP `open_scene`
  - `EditorSceneManager.OpenScene(scenePath)`
  - リクエスト: `{"scenePath": "Assets/Scenes/Main.unity"}`
- [ ] POST `/scene/save` + MCP `save_scene`
  - `EditorSceneManager.SaveOpenScenes()`
- [ ] GET `/scene/hierarchy` + MCP `get_scene_hierarchy`
  - シーンの GameObject 階層をツリー構造で返す
  - レスポンスに sceneName, scenePath, 再帰的な gameObjects 配列

### GameObject（残り 5）

- [ ] GET `/gameobject/find` + MCP `find_gameobjects`
  - name（部分一致）, tag（完全一致）, componentType で検索
  - ※ `ApiRoutes.cs` にルート定義のみ存在、実装は未着手
- [ ] POST `/gameobject/create` + MCP `create_gameobject`
  - name + primitive（省略可）で作成、`Undo.RegisterCreatedObjectUndo`
  - ※ `ApiRoutes.cs` にルート定義のみ存在、実装は未着手
- [ ] POST `/gameobject/delete` + MCP `delete_gameobject`
  - instanceId 指定で削除、`Undo.DestroyObjectImmediate`
- [ ] GET `/gameobject/info` + MCP `get_gameobject_info`
  - instanceId 指定で基本情報 + コンポーネント型一覧を返す（軽量）
- [ ] POST `/gameobject/modify` + MCP `modify_gameobject`
  - name, activeSelf, tag, layer, parentInstanceId の部分更新
  - `Undo.RecordObject`

### コンポーネント（残り 4）

- [ ] POST `/component/add` + MCP `add_component`
  - instanceId + componentType で追加、`Undo.AddComponent`
- [ ] POST `/component/remove` + MCP `remove_component`
  - instanceId + componentType + componentIndex で削除
  - `Undo.DestroyObjectImmediate`
- [ ] GET `/component/properties` + MCP `get_component_properties`
  - SerializedProperty でシリアライズ済みプロパティを列挙
- [ ] POST `/component/set-property` + MCP `set_component_property`
  - SerializedObject / SerializedProperty API でプロパティ変更

### Prefab（残り 2）

- [ ] POST `/prefab/create` + MCP `create_prefab`
  - `PrefabUtility.SaveAsPrefabAsset()`
- [ ] POST `/prefab/instantiate` + MCP `instantiate_prefab`
  - `PrefabUtility.InstantiatePrefab()` + `Undo.RegisterCreatedObjectUndo`

### アセット（残り 4）

- [ ] POST `/asset/refresh` + MCP `refresh_asset_database`
  - `AssetDatabase.Refresh()`
- [ ] POST `/asset/create` + MCP `create_asset`
  - Material / ScriptableObject の新規作成
- [ ] GET `/asset/info` + MCP `get_asset_info`
  - SerializedObject でアセットのプロパティを列挙
- [ ] POST `/asset/set-property` + MCP `set_asset_property`
  - SerializedObject API でアセットプロパティ変更

### コンソール（残り 2）

- [ ] GET `/console/logs` + MCP `get_console_logs`
  - `Application.logMessageReceived` でログ収集、count パラメータ対応
- [ ] POST `/console/clear` + MCP `clear_console_logs`
  - `LogEntries.Clear()`

### ユーティリティ（残り 3）

- [ ] POST `/menu/execute` + MCP `execute_menu_item`
  - `EditorApplication.ExecuteMenuItem(menuPath)`
- [ ] POST `/tests/run` + MCP `run_tests`
  - `TestRunnerApi` でテスト実行、完了まで待機して結果を返す
  - testMode (EditMode/PlayMode) + nameFilter
- [ ] GET `/editor/screenshot` + MCP `capture_screenshot`
  - `ScreenCapture.CaptureScreenshotAsTexture()` → PNG バイナリ返却

---

## 進捗サマリー

| カテゴリ | 済 | 未 | 合計 |
|---------|----|----|------|
| Editor 制御 | 7 | 2 | 9 |
| シーン | 0 | 3 | 3 |
| GameObject | 0 | 5 | 5 |
| コンポーネント | 0 | 4 | 4 |
| Prefab | 0 | 2 | 2 |
| アセット | 0 | 4 | 4 |
| コンソール | 0 | 2 | 2 |
| ユーティリティ | 0 | 3 | 3 |
| **合計** | **7** | **25** | **32** |

MCP ツール: 6/31 実装済み（`GET /editor/status` は MCP ツール対象外）

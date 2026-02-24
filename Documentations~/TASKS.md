# UniCortex 実装タスクリスト

実装状況の一覧。詳細な仕様は [SPEC.md](SPEC.md) を参照。

> 最終更新: 2026-02-23

---

## 実装済み

### REST API エンドポイント（7/30）

| エンドポイント | Handler | UseCase | テスト |
|---------------|---------|---------|--------|
| GET `/editor/ping` | PingHandler | PingUseCase | UseCase + Handler |
| POST `/editor/play` | PlayHandler | PlayUseCase | UseCase |
| POST `/editor/stop` | StopHandler | StopUseCase | UseCase |
| POST `/editor/domain-reload` | DomainReloadHandler | RequestDomainReloadUseCase | UseCase |
| GET `/editor/status` | EditorStatusHandler | GetEditorStatusUseCase | UseCase |
| POST `/editor/undo` | UndoHandler | UndoUseCase | UseCase + Handler |
| POST `/editor/redo` | RedoHandler | RedoUseCase | UseCase + Handler |

### MCP ツール（6/29）

| ツール名 | 対応 API | 状態 |
|----------|---------|------|
| `ping_editor` | GET `/editor/ping` | 済 |
| `enter_play_mode` | POST `/editor/play` | 済 |
| `exit_play_mode` | POST `/editor/stop` | 済 |
| `reload_domain` | POST `/editor/domain-reload` | 済 |
| `undo` | POST `/editor/undo` | 済 |
| `redo` | POST `/editor/redo` | 済 |

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

## 推奨実装順序

依存関係と開発効率を考慮した優先順位。上から順に実装する。

| 優先度 | タスク | 理由 |
|--------|--------|------|
| 1 | undo / redo | 既存パターンの踏襲で最小工数。Editor 制御カテゴリ完成 |
| 2 | run_tests | 以降の全実装で MCP 経由のセルフテストが可能になる |
| 3 | console (logs / clear) | テスト失敗時のデバッグに直結。独立性が高い |
| 4 | scene (open / save / hierarchy) | GameObject 操作の前提となる基盤機能 |
| 5 | GameObject (find / create / delete / info / modify) | シーン構築の基本フロー成立 |
| 6 | component (add / remove / properties / set-property) | GameObject 操作の次に自然な流れ |
| 7 | prefab (create / instantiate) | GameObject + シーン操作に依存 |
| 8 | asset (refresh / create / info / set-property) | 独立性はあるが優先度は低め |
| 9 | menu execute / screenshot | 汎用ユーティリティ、最後でよい |

---

## 未実装タスク

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
- [x] POST `/tests/run` + MCP `run_tests`
  - `TestRunnerApi` でテスト実行、完了まで待機して結果を返す
  - testMode (EditMode/PlayMode) + nameFilter
- [ ] GET `/editor/screenshot` + MCP `capture_screenshot`
  - `ScreenCapture.CaptureScreenshotAsTexture()` → PNG バイナリ返却

---

## 進捗サマリー

| カテゴリ | 済 | 未 | 合計 |
|---------|----|----|------|
| Editor 制御 | 7 | 0 | 7 |
| シーン | 0 | 3 | 3 |
| GameObject | 0 | 5 | 5 |
| コンポーネント | 0 | 4 | 4 |
| Prefab | 0 | 2 | 2 |
| アセット | 0 | 4 | 4 |
| コンソール | 0 | 2 | 2 |
| ユーティリティ | 1 | 2 | 3 |
| **合計** | **8** | **22** | **30** |

MCP ツール: 7/29 実装済み（`GET /editor/status` は MCP ツール対象外）

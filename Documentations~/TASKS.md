# UniCortex 実装タスクリスト

実装状況の一覧。詳細な仕様は [SPEC.md](SPEC.md) を参照。

> 最終更新: 2026-02-28

---

## 実装済み

### REST API エンドポイント（21/29）

| エンドポイント | Handler | UseCase | テスト |
|---------------|---------|---------|--------|
| GET `/editor/ping` | PingHandler | PingUseCase | UseCase + Handler |
| POST `/editor/play` | PlayHandler | PlayUseCase | UseCase |
| POST `/editor/stop` | StopHandler | StopUseCase | UseCase |
| POST `/editor/domain-reload` | DomainReloadHandler | RequestDomainReloadUseCase | UseCase |
| GET `/editor/status` | EditorStatusHandler | GetEditorStatusUseCase | UseCase |
| POST `/editor/undo` | UndoHandler | UndoUseCase | UseCase + Handler |
| POST `/editor/redo` | RedoHandler | RedoUseCase | UseCase + Handler |
| POST `/tests/run` | RunTestsHandler | RunTestsUseCase | Handler |
| GET `/console/logs` | ConsoleLogsHandler | GetConsoleLogsUseCase | UseCase + Handler |
| POST `/console/clear` | ConsoleClearHandler | ClearConsoleLogsUseCase | UseCase + Handler |
| POST `/scene/open` | OpenSceneHandler | OpenSceneUseCase | UseCase + Handler |
| POST `/scene/save` | SaveSceneHandler | SaveSceneUseCase | UseCase + Handler |
| GET `/scene/hierarchy` | SceneHierarchyHandler | GetSceneHierarchyUseCase | UseCase + Handler |
| GET `/gameobjects` | GetGameObjectsHandler | GetGameObjectsUseCase | UseCase + Handler |
| POST `/gameobject/create` | CreateGameObjectHandler | CreateGameObjectUseCase | — |
| POST `/gameobject/delete` | DeleteGameObjectHandler | DeleteGameObjectUseCase | — |
| POST `/gameobject/modify` | ModifyGameObjectHandler | ModifyGameObjectUseCase | — |
| POST `/component/add` | AddComponentHandler | AddComponentUseCase | UseCase + Handler |
| POST `/component/remove` | RemoveComponentHandler | RemoveComponentUseCase | UseCase + Handler |
| GET `/component/properties` | ComponentPropertiesHandler | GetComponentPropertiesUseCase | UseCase + Handler |
| POST `/component/set-property` | SetComponentPropertyHandler | SetComponentPropertyUseCase | UseCase + Handler |

### MCP ツール（20/28）

| ツール名 | 対応 API | 状態 |
|----------|---------|------|
| `ping_editor` | GET `/editor/ping` | 済 |
| `enter_play_mode` | POST `/editor/play` | 済 |
| `exit_play_mode` | POST `/editor/stop` | 済 |
| `reload_domain` | POST `/editor/domain-reload` | 済 |
| `undo` | POST `/editor/undo` | 済 |
| `redo` | POST `/editor/redo` | 済 |
| `run_tests` | POST `/tests/run` | 済 |
| `get_console_logs` | GET `/console/logs` | 済 |
| `clear_console_logs` | POST `/console/clear` | 済 |
| `open_scene` | POST `/scene/open` | 済 |
| `save_scene` | POST `/scene/save` | 済 |
| `get_scene_hierarchy` | GET `/scene/hierarchy` | 済 |
| `get_game_objects` | GET `/gameobjects` | 済 |
| `create_gameobject` | POST `/gameobject/create` | 済 |
| `delete_gameobject` | POST `/gameobject/delete` | 済 |
| `modify_gameobject` | POST `/gameobject/modify` | 済 |
| `add_component` | POST `/component/add` | 済 |
| `remove_component` | POST `/component/remove` | 済 |
| `get_component_properties` | GET `/component/properties` | 済 |
| `set_component_property` | POST `/component/set-property` | 済 |

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
| 2 | ~~run_tests~~ **実装済み** | 以降の全実装で MCP 経由のセルフテストが可能になる |
| 3 | ~~console (logs / clear)~~ **実装済み** | テスト失敗時のデバッグに直結。独立性が高い |
| 4 | ~~scene (open / save / hierarchy)~~ **実装済み** | GameObject 操作の前提となる基盤機能 |
| 5 | ~~GameObject (get_game_objects / create / delete / modify)~~ **実装済み** | シーン構築の基本フロー成立 |
| 6 | ~~component (add / remove / properties / set-property)~~ **実装済み** | GameObject 操作の次に自然な流れ |
| 7 | prefab (create / instantiate) | GameObject + シーン操作に依存 |
| 8 | asset (refresh / create / info / set-property) | 独立性はあるが優先度は低め |
| 9 | menu execute / screenshot | 汎用ユーティリティ、最後でよい |

---

## 未実装タスク

### Prefab（残り 2）

- [ ] POST `/prefab/create` + MCP `create_prefab`
  - `PrefabUtility.SaveAsPrefabAsset()`
- [ ] POST `/prefab/instantiate` + MCP `instantiate_prefab`
  - `PrefabUtility.InstantiatePrefab()` + `Undo.RegisterCreatedObjectUndo`

### アセット（1）

- [ ] POST `/asset/refresh` + MCP `refresh_asset_database`
  - `AssetDatabase.Refresh()`

### ScriptableObject（3）

- [ ] POST `/scriptable-object/create` + MCP `create_scriptable_object`
  - ScriptableObject の新規作成
- [ ] GET `/scriptable-object/info` + MCP `get_scriptable_object_info`
  - SerializedObject で ScriptableObject のプロパティを列挙
- [ ] POST `/scriptable-object/set-property` + MCP `set_scriptable_object_property`
  - SerializedObject API で ScriptableObject プロパティ変更

### ユーティリティ（残り 2）

- [ ] POST `/menu/execute` + MCP `execute_menu_item`
  - `EditorApplication.ExecuteMenuItem(menuPath)`
- [ ] GET `/editor/screenshot` + MCP `capture_screenshot`
  - `ScreenCapture.CaptureScreenshotAsTexture()` → PNG バイナリ返却

---

## 進捗サマリー

| カテゴリ | 済 | 未 | 合計 |
|---------|----|----|------|
| Editor 制御 | 7 | 0 | 7 |
| シーン | 3 | 0 | 3 |
| GameObject | 4 | 0 | 4 |
| コンポーネント | 4 | 0 | 4 |
| Prefab | 0 | 2 | 2 |
| アセット | 0 | 4 | 4 |
| コンソール | 2 | 0 | 2 |
| ユーティリティ | 1 | 2 | 3 |
| **合計** | **21** | **8** | **29** |

MCP ツール: 20/28 実装済み（`GET /editor/status` は MCP ツール対象外）

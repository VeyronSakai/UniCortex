# Tasks

## Play Mode 時の安定性改善

### 1. MainThreadDispatcher を経由しないエンドポイント

Editor が Pause 状態でも応答できる HTTP エンドポイントを追加する。

- `GET /editor/state` — `EditorApplication.isPlaying`, `EditorApplication.isPaused` を返す
- `POST /editor/unpause` — `EditorApplication.isPaused = false` を直接セット

これにより AI が「タイムアウト → state 確認 → Pause 中と判明 → unpause → 再試行」の自己回復フローを実行できる。

### 2. ハンドラー側の Play Mode ガード

Play Mode 中に使えない API を呼ぶハンドラーに `EditorApplication.isPlaying` チェックを入れ、
例外が throw → `Debug.LogError` → Error Pause のチェーンを防ぐ。

対象ハンドラー（要調査）:
- `OpenSceneHandler` (`EditorSceneManager.OpenScene`)
- `CreateSceneHandler` (`EditorSceneManager.NewScene`)
- その他 `EditorSceneManager` を使うハンドラー

### 3. 新しい MCP Tool

| Tool | 用途 |
|------|------|
| `get_editor_state` | Play Mode / Pause 状態の確認 |
| `unpause_editor` | Pause 解除（MainThread 不要で実装） |

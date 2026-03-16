---
name: unity-remote-control
description: Unity ゲームに DebugManager + McpRemoteControl を実装し、エージェント（Claude Code / MCP）が Play Mode 中のゲーム状態を自動制御できるようにする。God モード、画面遷移、スコア操作を MCP から直接実行し、受け入れテストを人手なしで自動化する。「デバッグ」「DebugManager」「God モード」「MCP リモート制御」「自動テスト」「受け入れテスト自動化」といったリクエスト時、または dev-game スキルの Phase 6（DebugManager 実装）・Phase 7（テスト検証）から参照されて使用。
---

# Unity DebugManager + MCP Remote Control

## 目的

Unity MCP は UI ボタンクリックやキー入力をシミュレートできない。そのため、エージェントが Play Mode 中にゲーム状態を自動制御し、受け入れテストを人手なしで完了するための仕組みが必要になる。

DebugManager + McpRemoteControl はこの制約を解消し、エージェントによる以下の自動化を可能にする:
- 画面遷移（タイトル→ゲーム→ゲームオーバー→リトライ）
- God モードによるゲームオーバー回避（長時間プレイの自動検証）
- 各画面のスクリーンショット取得と目視確認
- コンソールログによるランタイムエラー検出

## アーキテクチャ

```
[Claude Code / MCP]
  │
  ├─ execute_menu_item("MCP/StartGame")   ← Play Mode 中にメニュー経由で制御（推奨）
  │     ↓
  │  McpRemoteControl.cs (Editor スクリプト、[MenuItem] 属性)
  │     ↓
  │  GameManager.Instance.StartGame()
  │
  └─ manage_components → DebugManager.mcpCommand = "god_on"  ← Inspector 経由で制御（代替）
        ↓
     DebugManager.cs (ランタイムスクリプト)
        ↓
     ProcessMcpCommand() → 各機能実行
```

**2 つの制御経路:**
1. **McpRemoteControl**（推奨）: `execute_menu_item` で直接呼び出し。レスポンスが速い
2. **mcpCommand フィールド**: `manage_components` で文字列を設定。次フレームで実行される

## 実装手順

### 1. GameManager にデバッグ用メソッドを追加

ゲームの状態管理に応じて以下のようなメソッドを追加する:

```csharp
// 例: 状態遷移
public void ForceReturnToTitle() { ChangeState(GameState.Title); }
// 例: タイマー制御（タイマーのあるゲーム）
public void SetRemainingTime(float time) { RemainingTime = time; }
// 例: スコア制御
public void SetScore(int score) { Score = score; OnScoreChanged?.Invoke(Score); }
```

### 2. DebugManager を作成

テンプレート: [debug-manager-template.md](references/debug-manager-template.md)

共通キーバインド（手動テスト用の補助。エージェント自動テストでは McpRemoteControl を使う）:

| キー | 機能 | 有効条件 |
|------|------|---------|
| F1 | デバッグ情報オーバーレイ | 常時 |
| G | God モード ON/OFF | 常時 |
| S | ゲーム開始 | Title 時 |
| R | タイトルへ帰還 | Title 以外 |
| N | 強制ゲームオーバー | Playing 時 |
| Space | リトライ | GameOver 時 |

### 3. McpRemoteControl を作成（Editor スクリプト）

テンプレート: [mcp-remote-control-template.md](references/mcp-remote-control-template.md)

`Assets/Scripts/Editor/McpRemoteControl.cs` に配置。`execute_menu_item` から呼び出す。

### 4. God モードを設計・実装

God モードの目的は **エージェントがゲームオーバーにならずにプレイを継続し、各種機能を自動検証できるようにする** こと。

実装はゲームのジャンルやメカニクスに応じて設計する:

| ゲームジャンル | God モードで無効化する要素 | 実装例 |
|--------------|------------------------|-------|
| ランゲーム | 障害物との衝突死 | `Physics2D.IgnoreLayerCollision` + `isInvincible` |
| シューティング | 被弾ダメージ | `isDamageable = false` |
| パズル | 制限時間切れ | `timerFrozen = true` |
| アクション | 体力減少・落下死 | `hp = Mathf.Infinity` + 落下判定スキップ |
| もぐらたたき | （God モード不要） | タイマー操作で代替 |

設計原則:
- **ゲームオーバー条件を無効化する** — ゲームが何で終了するかを特定し、その条件をバイパスする
- **ゲームプレイは維持する** — スコア加算、コイン取得、レベル進行などは通常どおり動作させる
- **ON/OFF を切り替えられる** — God モード OFF でゲームオーバー遷移もテストできるようにする

## エージェント自動テスト手順

### 画面遷移テスト

```
1. manage_editor(action="play")
2. sleep 2秒（初期化待ち）
3. manage_camera(action="screenshot", include_image=true)  → タイトル画面確認
4. execute_menu_item("MCP/StartGame")
5. sleep 1秒
6. manage_camera(action="screenshot", include_image=true)  → ゲーム画面確認
7. execute_menu_item("MCP/ForceGameOver")
8. sleep 2秒（ゲームオーバー演出待ち）
9. manage_camera(action="screenshot", include_image=true)  → ゲームオーバー画面確認
10. execute_menu_item("MCP/RetryGame")
11. sleep 1秒
12. manage_camera(action="screenshot", include_image=true) → リトライ後確認
13. execute_menu_item("MCP/ReturnToTitle")
14. manage_camera(action="screenshot", include_image=true) → タイトル復帰確認
15. manage_editor(action="stop")
```

### ゲームプレイ長時間テスト（God モード使用）

```
1. manage_editor(action="play")
2. execute_menu_item("MCP/StartGame")
3. execute_menu_item("MCP/GodMode")         → ゲームオーバーを防ぐ
4. sleep 30-70秒（難易度上昇・スコア蓄積を待つ）
5. manage_camera(action="screenshot", include_image=true)  → プレイ中確認
6. read_console(types=["error"])             → ランタイムエラー確認
7. execute_menu_item("MCP/GodModeOff")      → God モード解除
8. sleep 数秒（ゲームオーバーを待つ）
9. manage_camera(action="screenshot", include_image=true)  → ゲームオーバー確認
10. manage_editor(action="stop")
```

### 注意事項

- McpRemoteControl は `Assets/Scripts/Editor/` に配置（ビルドに含まれない）
- DebugManager は `#if UNITY_EDITOR || DEVELOPMENT_BUILD` で囲む（リリースビルドに含めない）
- God モード解除時は状態を確実に元に戻す（物理レイヤー設定等）

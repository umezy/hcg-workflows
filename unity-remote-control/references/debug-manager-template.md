# DebugManager テンプレート

## テンプレートコード

```csharp
using UnityEngine;

/// <summary>
/// デバッグ用隠しキーコマンド + MCP リモート制御
/// Editor / Development Build でのみ有効
/// </summary>
public class DebugManager : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("MCP Remote Control")]
    [SerializeField] string mcpCommand = "";

    bool showDebugInfo;
    bool godMode;

    void Update()
    {
        if (GameManager.Instance == null) return;

        ProcessMcpCommand();

        // F1: デバッグ情報オーバーレイ
        if (Input.GetKeyDown(KeyCode.F1))
            showDebugInfo = !showDebugInfo;

        // G: God モード
        if (Input.GetKeyDown(KeyCode.G))
            ToggleGodMode();

        // S: ゲーム開始
        if (Input.GetKeyDown(KeyCode.S) && GameManager.Instance.CurrentState == GameState.Title)
        {
            GameManager.Instance.StartGame();
            Debug.Log("[DEBUG] Start game");
        }

        // R: タイトルへ帰還
        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance.CurrentState != GameState.Title)
        {
            GameManager.Instance.ReturnToTitle();
            Debug.Log("[DEBUG] Return to title");
        }

        // N: 強制ゲームオーバー
        if (Input.GetKeyDown(KeyCode.N) && GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.GameOver();
            Debug.Log("[DEBUG] Force game over");
        }

        // Space: リトライ
        if (Input.GetKeyDown(KeyCode.Space) && GameManager.Instance.CurrentState == GameState.GameOver)
        {
            GameManager.Instance.RetryGame();
            Debug.Log("[DEBUG] Retry game");
        }

        // === カスタムコマンド（ゲーム固有） ===
        // タイマー制御: if (Input.GetKeyDown(KeyCode.T)) { ... }
        // ステージ選択: if (Input.GetKeyDown(KeyCode.Alpha1)) { ... }
    }

    // --- MCP コマンド処理 ---

    void ProcessMcpCommand()
    {
        if (string.IsNullOrEmpty(mcpCommand)) return;
        string cmd = mcpCommand;
        mcpCommand = "";

        switch (cmd)
        {
            case "start":
                GameManager.Instance.StartGame();
                Debug.Log("[MCP] Start game");
                break;
            case "gameover":
                GameManager.Instance.GameOver();
                Debug.Log("[MCP] Force game over");
                break;
            case "retry":
                GameManager.Instance.RetryGame();
                Debug.Log("[MCP] Retry");
                break;
            case "title":
                GameManager.Instance.ReturnToTitle();
                Debug.Log("[MCP] Return to title");
                break;
            case "god_on":
                SetGodMode(true);
                break;
            case "god_off":
                SetGodMode(false);
                break;
            default:
                Debug.LogWarning($"[MCP] Unknown command: {cmd}");
                break;
        }
    }

    // --- God モード ---
    // ゲームのジャンルに合わせて実装を変更する。
    // 目的: エージェントがゲームオーバーにならずにプレイを継続できるようにする。

    void ToggleGodMode()
    {
        SetGodMode(!godMode);
    }

    public void SetGodMode(bool enabled)
    {
        godMode = enabled;
        // ★ ゲーム固有の God モード実装をここに記述する
        // 例（ランゲーム）: PlayerController.SetGodMode(enabled) で障害物衝突無効化
        // 例（シューティング）: PlayerHealth.SetInvincible(enabled)
        // 例（パズル）: TimerManager.SetFrozen(enabled)
        Debug.Log("[DEBUG] God mode: " + (godMode ? "ON" : "OFF"));
    }

    // --- デバッグ情報オーバーレイ ---

    void OnGUI()
    {
        if (!showDebugInfo) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;

        // ゲームに合わせて表示内容をカスタマイズ
        string info = $"[DEBUG]\n" +
            $"State: {gm.CurrentState}\n" +
            $"Score: {gm.Score}\n" +
            $"God: {(godMode ? "ON" : "OFF")}\n\n" +
            $"F1=Toggle G=God\n" +
            $"S=Start R=Title\n" +
            $"N=GameOver Space=Retry";

        GUI.Box(new Rect(10, 10, 260, 220), info, style);
    }
#endif
}
```

## God モード実装ガイド

God モードの目的は **ゲームオーバー条件を無効化** し、エージェントが長時間プレイで各種機能（スコア、難易度、UI 等）を自動検証できるようにすること。

ゲームのジャンルに応じて「何がゲームオーバーを引き起こすか」を特定し、その条件をバイパスする。

### 実装パターン

**パターン A: 衝突ベース（ランゲーム、アクション等）**
```csharp
// PlayerController に追加
public void SetGodMode(bool enabled)
{
    isInvincible = enabled;
    int playerLayer = gameObject.layer;
    int obstacleLayer = LayerMask.NameToLayer("Obstacle");
    if (obstacleLayer >= 0)
        Physics2D.IgnoreLayerCollision(playerLayer, obstacleLayer, enabled);
}
```

**パターン B: HP ベース（シューティング、RPG 等）**
```csharp
public void SetGodMode(bool enabled)
{
    isDamageable = !enabled;
    if (enabled) hp = maxHp;
}
```

**パターン C: タイマーベース（もぐらたたき、パズル等）**
```csharp
// God モード不要。代わりに SetRemainingTime() で制御する。
// McpRemoteControl に MCP/Timer3s 等のメニューを追加。
```

**パターン D: 複合型（複数のゲームオーバー条件がある場合）**
```csharp
public void SetGodMode(bool enabled)
{
    isInvincible = enabled;     // 衝突死を防ぐ
    isDamageable = !enabled;    // ダメージを防ぐ
    canFall = !enabled;         // 落下死を防ぐ
}
```

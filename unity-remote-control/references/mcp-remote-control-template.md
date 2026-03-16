# McpRemoteControl テンプレート

`Assets/Scripts/Editor/McpRemoteControl.cs` に配置する Editor スクリプト。
`execute_menu_item("MCP/XXX")` で Play Mode 中のゲーム状態を MCP から直接制御する。

## テンプレートコード

```csharp
using UnityEditor;
using UnityEngine;

/// <summary>
/// MCP Remote Control - Play Mode 中にメニューアイテムからゲーム状態を制御する。
/// execute_menu_item で呼び出し可能。
/// </summary>
public static class McpRemoteControl
{
    // === 画面遷移 ===

    [MenuItem("MCP/StartGame")]
    static void StartGame()
    {
        if (!CanExecute()) return;
        GameManager.Instance.StartGame();
        Debug.Log("[MCP] Start game");
    }

    [MenuItem("MCP/ForceGameOver")]
    static void ForceGameOver()
    {
        if (!CanExecute()) return;
        GameManager.Instance.GameOver();
        Debug.Log("[MCP] Force game over");
    }

    [MenuItem("MCP/RetryGame")]
    static void RetryGame()
    {
        if (!CanExecute()) return;
        GameManager.Instance.RetryGame();
        Debug.Log("[MCP] Retry game");
    }

    [MenuItem("MCP/ReturnToTitle")]
    static void ReturnToTitle()
    {
        if (!CanExecute()) return;
        GameManager.Instance.ReturnToTitle();
        Debug.Log("[MCP] Return to title");
    }

    // === God モード ===

    [MenuItem("MCP/GodMode")]
    static void GodModeOn()
    {
        if (!CanExecute()) return;
        var dm = Object.FindObjectOfType<DebugManager>();
        if (dm != null) dm.SetGodMode(true);
        Debug.Log("[MCP] God mode ON");
    }

    [MenuItem("MCP/GodModeOff")]
    static void GodModeOff()
    {
        if (!CanExecute()) return;
        var dm = Object.FindObjectOfType<DebugManager>();
        if (dm != null) dm.SetGodMode(false);
        Debug.Log("[MCP] God mode OFF");
    }

    // === スコア操作 ===

    // ゲーム固有: スコア設定、タイマー操作、ステージ選択など
    // [MenuItem("MCP/Set Score 100")]
    // static void SetScore100() { ... }

    // === ヘルパー ===

    static bool CanExecute()
    {
        if (!Application.isPlaying || GameManager.Instance == null)
        {
            Debug.LogWarning("[MCP] Not in play mode or no GameManager");
            return false;
        }
        return true;
    }
}
```

## 使用方法（MCP から）

```
execute_menu_item("MCP/StartGame")
execute_menu_item("MCP/GodMode")
execute_menu_item("MCP/ForceGameOver")
execute_menu_item("MCP/RetryGame")
execute_menu_item("MCP/ReturnToTitle")
execute_menu_item("MCP/GodModeOff")
```

## カスタマイズ

ゲーム固有のコマンドを `[MenuItem("MCP/XXX")]` で追加する:

- タイマー制御: `MCP/Timer3s`
- ステージ選択: `MCP/Stage1` 〜 `MCP/Stage9`
- スコア操作: `MCP/SetScore100`
- モード切替: `MCP/TimeAttack`

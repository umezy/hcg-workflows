# DebugManager テンプレート

Unity MCP ではゲーム内のボタンクリックやキー入力をシミュレートできない。
そのため、テスト・検証時の画面遷移やゲーム状態操作を効率化するために、
**DebugManager（隠しキーコマンド）** を早期に実装する。

## なぜ必要か

- Unity MCP は UI ボタンのクリックやキー入力のシミュレーションに非対応
- テスト・検証のたびにユーザーに手動操作を依頼するのは非効率
- DebugManager があれば「キー○を押してください」の一言で画面遷移が完了する
- ゲーム状態を直接操作でき、特定の場面を素早く再現できる

## 実装タイミング

Phase 6（実装）の **GameManager 完成直後** に作成する。
GameManager の状態管理メソッドに依存するため、GameManager 側に以下のデバッグ用メソッドを追加する:

```csharp
// GameManager に追加するデバッグ支援メソッド
public void SetRemainingTime(float time) { RemainingTime = time; }
public void ForceReturnToTitle() { /* 状態リセット＋タイトル遷移 */ }
```

## テンプレートコード

```csharp
using UnityEngine;

/// <summary>
/// デバッグ用隠しキーコマンド（Editor / Development Build でのみ有効）
/// GameManager オブジェクトにアタッチする。
///
/// ゲーム固有のコマンドは「カスタムコマンド」セクションに追加すること。
/// </summary>
public class DebugManager : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool showDebugInfo;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // === 共通コマンド ===

        // F1: デバッグ情報表示トグル
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugInfo = !showDebugInfo;
        }

        // R: タイトルへ強制帰還
        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance.CurrentState != GameState.Title)
        {
            GameManager.Instance.ForceReturnToTitle();
            Debug.Log("[DEBUG] Return to title");
        }

        // === プレイ中コマンド ===

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // T: タイマーを残り3秒に
            if (Input.GetKeyDown(KeyCode.T))
            {
                GameManager.Instance.SetRemainingTime(3f);
                Debug.Log("[DEBUG] Timer set to 3 seconds");
            }

            // N: 即座にゲーム終了
            if (Input.GetKeyDown(KeyCode.N))
            {
                GameManager.Instance.SetRemainingTime(0.1f);
                Debug.Log("[DEBUG] Force ending game");
            }
        }

        // === カスタムコマンド（ゲーム固有） ===
        // 例: ステージ即開始、モード切替など
        //
        // if (GameManager.Instance.CurrentState != GameState.Playing)
        // {
        //     // 0: タイムアタック即開始
        //     if (Input.GetKeyDown(KeyCode.Alpha0))
        //     {
        //         GameManager.Instance.StartGame(0);
        //         Debug.Log("[DEBUG] Quick start: Time Attack");
        //     }
        //
        //     // 1-9: ステージ即開始
        //     for (int i = 1; i <= 9; i++)
        //     {
        //         if (Input.GetKeyDown(KeyCode.Alpha0 + i))
        //         {
        //             GameManager.Instance.StartGame(i);
        //             Debug.Log($"[DEBUG] Quick start: Stage {i}");
        //         }
        //     }
        // }
    }

    private void OnGUI()
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
                      $"Score: {gm.CurrentScore}\n" +
                      $"Time: {gm.RemainingTime:F1}\n" +
                      $"\nKeys:\n" +
                      $"F1=Toggle  R=Title\n" +
                      $"T=Timer 3s  N=End now";

        GUI.Box(new Rect(10, 100, 250, 200), info, style);
    }
#endif
}
```

## 推奨キーバインド

| カテゴリ | キー | 機能 | 備考 |
|---------|------|------|------|
| 共通 | F1 | デバッグ情報オーバーレイ表示/非表示 | 全画面で有効 |
| 共通 | R | タイトルへ強制帰還 | Title 以外で有効 |
| プレイ中 | T | タイマーを残り3秒に | リザルト画面を素早く確認 |
| プレイ中 | N | 即座にゲーム終了 | タイマー0.1秒に設定 |
| カスタム | 0-9 | ステージ/モード即開始 | ゲーム固有に実装 |
| カスタム | P | ポーズトグル | 必要に応じて |
| カスタム | L | レベルスキップ | 必要に応じて |
| カスタム | G | 無敵モードトグル | 必要に応じて |
| カスタム | M | マネー/ポイント追加 | 必要に応じて |

## ドキュメント化

デバッグコマンドは `docs/DEBUG-COMMANDS.md` に記載する。
このファイルはチームメンバーやテスターが素早くコマンドを参照できるよう、
docs/ 直下に大文字ファイル名で配置する。

## テスト・検証時の運用

Unity MCP でのテスト手順:

1. `manage_editor(action="play")` で Play モード開始
2. ユーザーにデバッグキーで目的の画面へ遷移してもらう（例:「0キーでタイムアタック開始してください」）
3. `manage_camera(action="screenshot", include_image=true)` でスクリーンショット取得
4. スクリーンショットと `read_console` でエラー確認
5. 問題があれば `manage_editor(action="stop")` で停止し修正

例（もぐらたたきの場合）:
```
Claude: 「0キーを押してタイムアタックを開始してください」
User: (0キー押下)
Claude: manage_camera(action="screenshot", include_image=true) → ゲーム画面確認
Claude: 「Tキーで残り3秒にしてください」
User: (Tキー押下)
Claude: manage_camera(action="screenshot", include_image=true) → リザルト画面確認
```

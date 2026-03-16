---
name: unity-check
description: Unity MCP を使った実装検証スキル。コンパイルエラー確認（L1）、Play Mode 動作確認（L2）、スクリーンショット目視確認（L3）の3段階で検証する。「検証して」「動作確認」「Play Mode で確認」「スクリーンショットで確認」「エラーチェック」といったリクエスト時、または他スキル（dev-game 等）から参照されて使用。
---

# Unity Check

Unity MCP ツールを使い、実装の正しさを3段階のレベルで検証する。

## 検証レベル

### L1: コンパイル確認（~1-2K tokens）

`read_console` でエラー・警告を確認する。

手順:
1. `read_console(type="Error")` を実行
2. エラーがあれば修正し、再度確認
3. エラーが0件になるまで繰り返す

### L2: Play Mode 確認（~3-5K tokens）

Play Mode で実行し、ランタイムエラーがないことを確認する。

手順:
1. `manage_editor(action="play")` で Play Mode 開始
2. 3秒待機（初期化処理の完了を待つ）
3. `read_console(type="Error")` でランタイムエラーを確認
4. `manage_editor(action="stop")` で Play Mode 停止
5. エラーがあれば修正し、L1 から再実行

### L3: スクリーンショット確認（~100-300K tokens）

Play Mode 中のゲーム画面をスクリーンショットで取得し、視覚的に確認する。

前提条件:
- Canvas の Render Mode が `Screen Space - Camera` に設定済み
- Main Camera が Canvas の `worldCamera` に割り当て済み

手順:
1. `manage_editor(action="play")` で Play Mode 開始
2. 3秒待機
3. `manage_camera(action="screenshot", include_image=true, max_resolution=640)` で取得
4. スクリーンショットを確認し、表示崩れや異常がないか判定
5. `read_console(type="Error")` でランタイムエラーも確認
6. `manage_editor(action="stop")` で Play Mode 停止
7. 問題があれば修正し、L1 から再実行

## 使い分けガイド

| 状況 | レベル |
|---|---|
| スクリプト作成・編集後 | L1 |
| コミット前の動作確認 | L2 |
| UI/演出の実装後 | L3 |
| マイルストーン到達時 | L3 |

## 呼び出し例

他スキルから参照する場合:
- 「L1 検証を実行」→ read_console のみ
- 「L2 検証を実行」→ Play Mode + console
- 「L3 検証を実行」→ Play Mode + screenshot + console

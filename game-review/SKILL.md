---
name: game-review
description: ゲーム画面全てのスクリーンショットを撮影し、スクショとコードベースを分析して改善提案を行う。「レビュー」「改善提案」「ゲームレビュー」「スクショして改善」「画面チェック」「ビジュアルレビュー」「UI改善」「UX改善」「フィードバック」といったリクエスト時に使用。
---

# Game Review

ゲーム全画面のスクリーンショット撮影 → コードベース分析 → 改善提案を一貫して行う。

## 前提条件

- Unity MCP が接続済み
- Canvas の Render Mode が `Screen Space - Camera` に設定済み

## 手順

### Step 0: McpRemoteControl の確認

`McpRemoteControl.cs` が存在するか確認する（Grep で `McpRemoteControl` を検索）。

- **存在する場合** → Step 1 へ進む
- **存在しない場合** → `unity-remote-control` スキルを実行して DebugManager + McpRemoteControl を実装してから Step 1 へ進む

### Step 1: ゲーム画面の特定

コードベースから全画面を特定する。

1. `GameState.cs` を読み、全状態（Title, ModeSelect, StageSelect, Playing, Result 等）を把握
2. `McpRemoteControl.cs` を読み、利用可能なメニューコマンドを把握

### Step 2: 全画面スクリーンショット撮影

Play Mode を起動し、各画面を順に撮影する。

```
1. manage_editor(action="play") で Play Mode 開始
2. 3秒待機
3. 各画面について:
   a. manage_camera(action="screenshot", include_image=true, max_resolution=640, screenshot_file_name="screen_{画面名}") で撮影
   b. execute_menu_item(menu_path="MCP/Go To {次の画面}") で遷移
   c. 1秒待機
4. manage_editor(action="stop") で Play Mode 停止
```

撮影順序の例:
- Title → (自動表示)
- ModeSelect → `MCP/Go To ModeSelect`
- StageSelect → `MCP/Go To StageSelect`
- Playing → `MCP/Start Stage 1`（2秒待機）
- Result → `MCP/Force End Game`（2秒待機）

保存先: `Assets/Screenshots/screen_{画面名}.png`

### Step 3: コードベース分析

並行してコードベースを調査する（Agent ツールで Explore サブエージェントを使用）。

調査対象:
- 全 C# スクリプト（Core, Gameplay, UI, Data）
- ScriptableObject 設定値
- オーディオ構成
- プレハブ構成

分析観点:
- バグ・コードスメル
- UX 問題（フィードバック不足、視認性、一貫性）
- 不足機能
- パフォーマンス懸念

### Step 4: 改善提案の作成

スクリーンショットとコード分析を統合し、以下のカテゴリで提案をまとめる。

#### 提案フォーマット

以下の4カテゴリに分類し、各項目に優先度（高/中/低）を個別に付与する。
優先度はカテゴリ単位で固定せず、ユーザー体験への影響度で判断する。

```markdown
## 改善提案

### A. ビジュアル・UI
| # | 優先度 | 問題 | 画面 | 提案 |
|---|--------|------|------|------|

### B. ゲームプレイ
| # | 優先度 | 問題 | 提案 |
|---|--------|------|------|

### C. UX・機能
| # | 優先度 | 問題 | 提案 |
|---|--------|------|------|

### D. コード品質
| # | 優先度 | 問題 | 提案 |
|---|--------|------|------|

## 特に効果が大きい改善 TOP 3
1. ...
2. ...
3. ...
```

#### 優先度の判断基準

- **高**: ゲーム体験を大きく損なう問題、バグ、クラッシュリスク
- **中**: 体験向上に貢献するが必須ではない改善
- **低**: あると良いが無くても支障がない改善

#### 分析の重点

- **スクリーンショットから**: レイアウト崩れ、色の一貫性、視認性、ゲームらしさ、フィードバック演出
- **コードから**: バグリスク、ハードコード値、欠落機能、アーキテクチャ改善

### Step 5: レビュー結果の保存

`docs/reviews/review_YYYY-MM-DD/` フォルダを作成し、レビュー結果とスクリーンショットをまとめて保存する。

フォルダ構成:
```
docs/reviews/review_YYYY-MM-DD/
├── review.md          # 改善提案とステータス
├── screen_title.png   # Step 2 で撮影したスクリーンショットのコピー
├── screen_modeselect.png
├── screen_stageselect.png
├── screen_playing.png
└── screen_result.png
```

手順:
1. `docs/reviews/review_YYYY-MM-DD/` フォルダを作成
2. `Assets/Screenshots/screen_*.png` をフォルダにコピー
3. `review.md` を作成（以下のフォーマット）

review.md の構成:
```markdown
# Game Review - YYYY-MM-DD

## スクリーンショット
| 画面 | ファイル |
|------|---------|
| Title | screen_title.png |
| (以下、撮影した全画面) |

## 改善提案
(Step 4 のフォーマットをそのまま記載)

## ステータス
| # | 提案 | 状態 |
|---|------|------|
| A1 | ... | 未着手 / 進行中 / 完了 |
```

ユーザーが改善を実施したら、該当項目の状態を更新する。

### Step 6: 結果の提示

提案を表形式で提示し、レビューフォルダの保存先を案内する。

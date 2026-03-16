---
name: dev-game
description: Unity カジュアルゲームを仕様駆動開発（Specification-Driven Development）で作成する。仕様を先に確定させ、設計・実装・テストを行うことで手戻りを最小化する。「ゲームを作って」「仕様駆動開発」「SDD」「ゲーム企画書」「機能仕様書」「技術仕様書」「テスト仕様書」「タスクリスト」「Unityゲーム開発」「カジュアルゲーム」といったリクエスト時に使用。Phase 1（企画書）からPhase 8（リリース）まで段階的にドキュメントを作成し、仕様に基づいて実装・テスト・検証を行う。
---

# 仕様駆動ゲーム開発 (dev-game)

Unity カジュアルゲームを仕様駆動で開発するためのワークフロースキル。仕様を先に確定し、それに基づいて設計・実装・テストを行う。

## Unity MCP について

実装Phase（Phase 6〜8）では Unity MCP ツールで Unity Editor を直接操作する。

主要ツール: `manage_scene`, `manage_gameobject`, `manage_components`, `manage_ui`, `manage_material`, `manage_prefabs`, `manage_animation`, `manage_vfx`, `create_script`, `script_apply_edits`, `validate_script`, `run_tests`, `read_console`, `manage_editor`, `find_gameobjects`, `refresh_unity`

## 開発フロー

```
Phase 0: Unity MCP 接続確認    → 接続済みなら即スキップ
Phase 1: ゲーム企画書作成      → docs/01-game-design.md
Phase 2: 機能仕様書作成        → docs/02-functional-spec.md
Phase 3: 技術仕様書作成        → docs/03-technical-spec.md
Phase 4: テスト仕様書作成      → docs/04-test-spec.md
Phase 5: タスクリスト作成      → docs/05-task-list.md
Phase 6: 実装
Phase 7: テスト・検証
Phase 8: リリース
```

各Phaseは順番に実施する。前のPhaseの成果物が次のPhaseの入力となる。

## コミットルール

各フェーズの完了時・タスク完了時に自動コミットを行う。

| フェーズ | コミット粒度 | メッセージ例 |
|---|---|---|
| Phase 1〜5（ドキュメント） | フェーズ単位 | `Phase 2: 機能仕様書を追加` |
| Phase 6（実装） | タスク単位 | `Task 6-1: PlayerControllerを実装・テスト追加 (UT-001 PASS)` |
| Phase 7（検証） | 検証種別単位 | `Phase 7: 受け入れテストを完了` |
| Phase 8（リリース） | フェーズ単位 | `Phase 8: WebGLビルド設定を完了` |
| バグ修正 | 修正単位 | `Fix: 重力反転時の判定を修正` |

コミット原則:
- ドキュメントフェーズはフェーズ完了時に1コミット
- 実装フェーズはタスクリストの各タスク完了ごとに1コミット（実装コード＋テストコードを同時コミット）
- コミット前に必ず L2 検証（unity-check スキル参照）を実行し、エラーがないことを確認する
- コミットメッセージにテストIDと結果を含める（例: `UT-001 PASS`）

## ユーザーへの質問ルール

ユーザーへの質問・確認・選択は **AskUserQuestion ツール** を使う。テキスト出力で質問を列挙しない。

## ワークフロー

### Phase 0: Unity MCP 接続確認

`manage_scene(action="get_hierarchy")` を実行する。

- **成功** → Phase判定へ進む
- **失敗** → 以下をユーザーに案内する:
  1. Unity Editor を開き、下部「MCP For Unity」→ Connect タブで Client を「Claude Code」に変更
  2. 「Configure」→「Start Session」を押す
  3. VSCode で `Ctrl+Shift+P` →「Developer: Reload Window」で再起動し、再度このスキルを呼び出す

### Phase判定

ユーザーの依頼内容から現在のPhaseを判定する:

- **タスクリストが存在し、未完了タスクがある** → タスク継続フローへ
- **ゲームの内容が未定** → Phase 1 からユーザーと対話してゲーム内容を決定
- **ゲーム内容は決まっているが仕様書がない** → Phase 1 の企画書作成から開始
- **既存の仕様書がある** → 該当するPhaseから継続
- **「次のPhaseへ」と言われたら** → 現在のPhaseの成果物を確認し、次のPhaseへ進む

### タスク継続フロー

`docs/05-task-list.md` が存在し、未完了タスク（🔲）がある場合に実行する。

1. **コンテキスト読み込み**: サブフォルダ内含めて、`docs/` 内のすべてのファイルを読む。
2. **残タスク特定**: タスクリストから未完了タスクを洗い出し、依存関係と実装順序を確認する
3. **マイルストーンゲートチェック**: 未完了のマイルストーンタスク（TASK-M1, M2, ...）があるか確認する。**マイルストーンより後のフェーズのタスクに着手してはならない。** マイルストーンが未完了なら、通常タスクより先にマイルストーンの L3 検証を実施する
   - 例: TASK-M1（コアメカニクス後の L3 検証）が 🔲 なら、フェーズ4（スコアシステム）以降のタスクには進まない
   - 例: TASK-M2（UI 実装後の L3 検証）が 🔲 なら、フェーズ6（統合テスト）以降のタスクには進まない
4. **懸念事項の確認**: 以下のいずれかに該当する場合は **AskUserQuestion** で確認してから着手する:
   - 仕様書とコードの間に不整合がある（仕様変更の可能性）
   - タスクの依存先が未完了のまま残っている
   - タスクの実装方針に複数の選択肢がある
   - 前回の実装で発生した問題が残タスクに影響しそう
   - SE・BGMアセット等、手動作業が必要なタスク
5. **実装再開**: 確認が取れたら、残タスクを実装順序に従って進める。Phase 6 の実装原則に従う

### Phase 1: ゲーム企画書作成

テンプレート: [phase1-game-design-template.md](references/phase1-game-design-template.md)

1. ゲームの内容を確認（ジャンル、コンセプト、ゲームモード、ビジュアル等）
2. 不足情報があれば質問して補完
3. テンプレートに沿って `docs/01-game-design.md` を作成
4. ユーザーに承認を求め、フィードバックを反映

### Phase 2: 機能仕様書作成

テンプレート: [phase2-functional-spec-template.md](references/phase2-functional-spec-template.md)

企画書に基づき、以下を定義する:

1. 画面一覧・画面遷移図
2. 各ゲームメカニクスの入力→処理→出力→境界条件
3. スコア計算式・難易度曲線・パラメータ一覧
4. UI/UXレイアウト・演出概要

成果物: `docs/02-functional-spec.md`

### Phase 3: 技術仕様書作成

テンプレート: [phase3-technical-spec-template.md](references/phase3-technical-spec-template.md)

機能仕様書に基づき、以下を定義する:

1. プロジェクト構成（フォルダ構造）
2. クラス設計（クラス名・責務・依存関係・主要メソッド）
3. 状態遷移図と遷移条件
4. データ設計（保存データ・ScriptableObject）

成果物: `docs/03-technical-spec.md`

### Phase 4: テスト仕様書作成

テンプレート: [phase4-test-spec-template.md](references/phase4-test-spec-template.md)

実装前にテスト項目を定義する（仕様駆動の要）:

1. 単体テスト（機能仕様の各項目に対応するテストケース）
2. 統合テスト（画面遷移・コンポーネント連携）
3. レンダリング整合性テスト（全オブジェクトのスプライト・コライダー整合性を自動検証）
4. 受け入れテスト（機能仕様を満たしているかのチェックリスト）

成果物: `docs/04-test-spec.md`

### Phase 5: タスクリスト作成

テンプレート: [phase5-task-list-template.md](references/phase5-task-list-template.md)

1. 機能仕様書・技術仕様書の各項目をタスク単位に分解
2. テスト仕様書のテストIDと対応づける
3. タスク間の依存関係を明確にし、実装順序を決定

成果物: `docs/05-task-list.md`

### Phase 6: 実装

タスクリストに基づき実装を進める。Unity MCP ツールを活用する:

- `create_script` でC#スクリプトを作成、`validate_script` で検証
- `manage_gameobject` / `manage_components` でシーン上にオブジェクトを構築
- `manage_ui` でCanvas・UI要素を配置
- `manage_prefabs` でPrefab化
- `manage_material` でマテリアル設定
- `manage_animation` / `manage_vfx` で演出
- 変更後は `refresh_unity` で反映確認、`read_console` でエラー確認

#### 1タスクの実装サイクル（実装→テスト→検証→コミット）

各タスクは以下の順で進める:

1. **実装**: Unity MCP で機能コードを作成
2. **テストコード作成**: テスト仕様書（Phase 4）の対応テストIDに基づき、テストコードを作成
   - 単体テスト（UT-xxx）: `Assets/Tests/EditMode/` に EditMode テストとして実装
   - 統合テスト（IT-xxx）: `Assets/Tests/PlayMode/` に PlayMode テストとして実装
   - レンダリング整合性テスト（RT-xxx）: 最初のマイルストーン到達時にまとめて作成
   - 対応テストIDがないタスク（シーン配置等）はテストコード不要
3. **テスト実行**: `run_tests` で対象テストを実行し、合格を確認
4. **L1 検証**: `read_console` でコンパイルエラー確認
5. **L2 検証**: Play Mode で実行しランタイムエラー確認
6. **コミット**: 実装コード＋テストコードを同時にコミット
7. **タスクリスト更新**: ステータスを `✅完了` に更新
8. **マイルストーン到達時のみ L3 検証**: レンダリング整合性テスト実行 → スクリーンショット確認
   ※ 検証手順の詳細は unity-check スキルを参照

実装原則:
- **仕様書にない機能は作らない**
- **1タスク1コミット** — 実装コードとテストコードをセットでコミットする（例: `Task 6-3: スコア加算ロジックを実装・テスト追加 (UT-001 PASS)`）
- **テストIDがコミットメッセージにある = テストが合格済み** — テスト未実行のまま PASS を記載しない
- **パラメータはハードコードしない** — ScriptableObject または定数クラスで管理
- **タスクリスト逐次更新** — タスクを完了するたびに `docs/05-task-list.md` のステータスを `✅完了` に更新する。新たなタスクが発生した場合はタスクIDを採番して追加する。進捗サマリーと残タスク一覧も同時に更新する
- **バグ発見時の自動タスク追加** — 下記「バグ検出・自動起票ルール」に従う

#### バグ検出・自動起票ルール

実装・検証中に以下のいずれかでバグを検出した場合、**その場でタスクリストにバグ修正タスクを追加**する:

検出トリガー:
- L2/L3 検証のスクリーンショットで仕様と異なる表示を発見
- `run_tests` でテストが失敗し、テストコード側ではなく実装側の問題と判断
- `read_console` でランタイムエラーを検出
- MCP 操作（`execute_menu_item` 等）の結果が仕様書の期待と不一致

起票手順:
1. `docs/05-task-list.md` の「バグ修正」セクションに `TASK-FIX-{連番}` でタスクを追加する。セクションが存在しない場合は最終フェーズの直前に作成する
2. タスク内容に **現象・原因（推定）・影響する仕様/テストID** を記載する
3. 依存タスクに、バグの原因となったタスクIDを記載する
4. 進捗サマリーにバグ修正行を追加し、残タスク一覧も更新する
5. タスクリスト更新をコミットする（メッセージ例: `タスクリスト更新: TASK-FIX-001 追加 — GameOver→StartGame UI遷移バグ`）
6. バグ修正タスクは最終検証（Phase 7/9）より前に完了させる。最終検証タスクの依存タスクに追加する

バグ修正セクションのフォーマット:
```
### バグ修正

| ステータス | タスクID | タスク内容 | MCPツール | 対応テストID | テスト結果 | 依存タスク |
|:---------:|---------|-----------|----------|-------------|:---------:|-----------|
| 🔲 | TASK-FIX-001 | {現象の要約} | スクリプト編集 | {関連テストID} | - | {原因タスクID} |
```

修正完了時のコミットメッセージ: `Fix: {修正内容の要約}`

L3 検証を行うマイルストーン（**各マイルストーンは次フェーズへのゲート**）:
1. GameManager + コアメカニクス完成後 → 完了しないとスコアシステムに進めない
2. UI 実装完成後 → 完了しないと統合テスト作成に進めない
3. 演出・エフェクト完成後 → 完了しないとサウンドに進めない
4. サウンド実装完成後 → 完了しないと最終検証に進めない
5. 全実装完了後（Phase 7 開始前）

**マイルストーン完了時のセッション切り替え推奨**:
マイルストーンのコミット完了後、ユーザーに新しいセッションでの継続を推奨する。

DebugManager + McpRemoteControl:
- GameManager 完成直後に **unity-remote-control スキル** を参照して以下を実装する:
  1. DebugManager（隠しキーコマンド + mcpCommand フィールド）
  2. McpRemoteControl（Editor スクリプト、`execute_menu_item` で MCP から直接制御）
  3. God モード（ゲームのジャンルに応じて設計 — unity-remote-control スキルの God モード実装ガイド参照）
- Unity MCP は UI ボタンクリックやキー入力をシミュレートできないため、McpRemoteControl 経由でゲーム状態を制御する
- Phase 7 の受け入れテストでは God モードと McpRemoteControl を組み合わせて自動化する

スクリーンショット管理:
- MCP の `manage_camera(action="screenshot")` は `Assets/Screenshots/` に保存される（変更不可）
- Phase 6 開始時に `.gitignore` へ `Assets/Screenshots/` を追加すること
- `.gitignore` に以下を追記:
  ```
  # MCP Screenshots (auto-generated, not committed)
  /[Aa]ssets/[Ss]creenshots/
  ```
- **スクリーンショットクリーンアップ**: 会話セッション終了時に `Assets/Screenshots/` 内を一括削除する:
  ```bash
  find Assets/Screenshots -name "*.png" -delete && find Assets/Screenshots -name "*.meta" -delete
  ```

推奨実装順序:
1. プロジェクト構成・フォルダ作成（`.gitignore` にスクリーンショット除外を追加）
2. GameManager（状態管理の骨格）+ DebugManager
3. コアゲームメカニクス
4. スコアシステム
5. UI実装
6. 演出・エフェクト
7. サウンド
8. データ保存

### Phase 7: 統合検証・受け入れテスト

Phase 6 で各タスクごとにテストコードは作成済み。Phase 7 では全テストの一括実行と受け入れテストを行う。

1. **全テスト一括実行**: `run_tests` で EditMode（UT + RT）・PlayMode（IT）の全テストを実行
2. **結果確認**: `read_console` で結果を確認、不合格項目は修正後に再テスト
3. **L2 検証**: `manage_editor` で Play Mode 実行 + `read_console` でランタイムエラー確認
4. **McpRemoteControl による受け入れテスト自動化**（unity-remote-control スキル参照）:
   - `execute_menu_item("MCP/StartGame")` でゲーム開始
   - `execute_menu_item("MCP/GodMode")` で God モード有効化
   - `manage_camera(action="screenshot", include_image=true)` で各画面を確認
   - `execute_menu_item("MCP/ForceGameOver")` でゲームオーバー画面を確認
   - 受け入れテスト仕様の各項目をスクリーンショットで検証
5. **難易度バランスの調整**（パラメータ変更のみで対応）

#### Game ビューのスクリーンショットについて

Canvas の Render Mode を **Screen Space - Camera** に設定し、Main Camera を割り当てることで、`manage_camera(action="screenshot", include_image=true)` で UI を含むスクリーンショットを自動取得できる。

セットアップ:
- Canvas の Render Mode を `Screen Space - Camera` に変更
- `worldCamera` に Main Camera を割り当て（`CanvasSetup.cs` で Awake 時に自動設定）
- `planeDistance` は 10 程度に設定

スクリーンショット取得:
```
manage_camera(action="screenshot", include_image=true, max_resolution=640)
```

注意: Screen Space - Overlay のままだと `manage_camera` では UI が映らないため、必ず Camera モードにすること。

### Phase 8: リリース

1. `manage_editor` でビルド設定の確認（解像度、プラットフォーム固有設定）
2. ビルド実行・動作確認
3. 配布（WebGL → itch.io / GitHub Pages など）

## 重要な原則

- 各Phaseの成果物はユーザーの承認を得てから次のPhaseへ進む
- 仕様書は実装前に完成させる（仕様駆動の原則）
- 実装中に仕様の不備が見つかった場合は、仕様書を先に更新してから実装を修正する
- ドキュメントは `docs/` ディレクトリに保存する

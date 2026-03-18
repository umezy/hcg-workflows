---
name: github-pages-deploy
description: WebGL ビルド成果物を StatiCrypt で暗号化し、GitHub Pages にデプロイする。「デプロイ」「GitHub Pages」「公開して」「アップロード」といったリクエスト時に使用。パスワードは Build/ 内で管理し、リポジトリ作成からプッシュまでを自動化する。
---

# GitHub Pages Deploy

WebGL ビルド成果物を StatiCrypt でパスワード保護し、GitHub Pages に公開する。

## 前提条件

- `Build/WebGL/` にビルド成果物が存在すること（`unity-webgl-builder` スキルで生成）
- `gh` CLI がインストール・認証済みであること

## ワークフロー

### Step 1: リポジトリ名の確認

`Build/deploy/` 内に既存のデプロイフォルダ（git リポジトリ）があるか確認する。

- **既存フォルダがある場合** → そのフォルダ名をリポジトリ名として自動採用し、ユーザーへの確認はスキップする
- **既存フォルダがない場合** → プロジェクトのフォルダ名をリポジトリ名として提案し、ユーザーに確認する

公開 URL: `https://<ユーザー名>.github.io/<リポジトリ名>/`

### Step 2: パスワード管理

`Build/deploy/.staticrypt.json` を確認する。

- 存在する → 前回と同じパスワードで暗号化（StatiCrypt が自動管理）
- 存在しない → 16文字の英数字パスワードを自動生成（大文字・小文字・数字混在）

### Step 3: リポジトリ作成・ファイルコピー

1. `gh repo create <ユーザー名>/<リポジトリ名> --public` でリポジトリ作成（既存ならスキップ）
2. `Build/deploy/<リポジトリ名>/` にクローン（既存ならプル）
3. `Build/WebGL/` の中身をデプロイディレクトリにコピー
4. [assets/deploy.yml](assets/deploy.yml) を `.github/workflows/deploy.yml` としてコピー

### Step 4: StatiCrypt で暗号化（デプロイディレクトリ内で実行）

**重要**: `Build/WebGL/` 内では暗号化しない。必ずデプロイディレクトリ内で実行すること。`Build/WebGL/` を汚すと次回の Unity ビルドで `index.html` が上書きされず競合する。

```bash
cd Build/deploy/<リポジトリ名> && npx staticrypt index.html -p "<パスワード>" --short
```

暗号化後、`encrypted/index.html` → `index.html` に置き換える。
`.staticrypt.json` は `Build/deploy/` に保存して次回のパスワード再利用に備える:
```bash
cp Build/deploy/<リポジトリ名>/.staticrypt.json Build/deploy/.staticrypt.json
```
5. GitHub Pages を **workflow モード** で有効化する:
   ```bash
   gh api repos/<ユーザー名>/<リポジトリ名>/pages -X POST --input - <<'EOF'
   {"build_type":"workflow","source":{"branch":"main","path":"/"}}
   EOF
   ```
   既に Pages が有効な場合は `-X PUT` で更新する。
6. コミット・プッシュする
7. ワークフロー完了を確認する:
   ```bash
   gh api repos/<ユーザー名>/<リポジトリ名>/actions/runs --jq '.workflow_runs[0] | {status, conclusion}'
   ```
   `conclusion: "success"` になるまで待つ（通常 30-60 秒）。

**重要**: `build_type: "legacy"` は使わない。デプロイが発動しない場合がある。必ず `build_type: "workflow"` + GitHub Actions ワークフローを使う。

### Step 6: 結果の報告

公開 URL とパスワードをユーザーに伝える。

## トラブルシューティング

| 症状 | 原因 | 対処 |
|------|------|------|
| 404 "There isn't a GitHub Pages site here" | `build_type: "legacy"` を使っている | `build_type: "workflow"` に切り替え、`deploy.yml` を追加して再プッシュ |
| ワークフローが `queued` のまま | Pages の environment 未設定 | `gh api repos/.../pages` で Pages 有効か確認。無効なら再作成 |
| ワークフローが `failure` | `permissions` 不足 or Pages 未有効化 | YAML に `pages: write`, `id-token: write` があるか確認。Settings → Pages で source が "GitHub Actions" か確認 |
| パスワード画面が出ない | 暗号化前の `index.html` がプッシュされた | `encrypted/index.html` → `index.html` 置き換えを確認し再プッシュ |

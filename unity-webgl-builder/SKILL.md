---
name: unity-webgl-builder
description: Unity プロジェクトの WebGL ビルドとローカルサーバー起動を一貫して行う。「WebGLビルド」「ブラウザビルド」「WebGL」「ローカルサーバー」「ビルドして」「デプロイ」「Build WebGL」といったリクエスト時に使用。Editor スクリプト作成 → プラットフォーム切り替え → ビルド実行 → ローカル HTTP サーバー起動までを自動化する。
---

# Unity WebGL Builder

Unity プロジェクトを WebGL ビルドし、ローカルサーバーで確認するまでの一連のフロー。

## ワークフロー

### Step 1: WebGL Build Support 確認

MCP ツールでコンソールを確認し、WebGL 関連のエラーがないかチェックする。

`BuildTarget.WebGL` が未サポートの場合、ユーザーに以下を案内:

```
WebGL Build Support モジュールが必要です:
1. Unity Hub → Installs → 対象バージョンの歯車 → Add modules
2. 「WebGL Build Support」にチェック → Install
3. Unity エディタを再起動
```

### Step 2: Editor スクリプト作成

`Assets/Scripts/Editor/WebGLBuilder.cs` を作成する。サンプルは [assets/WebGLBuilder.cs](assets/WebGLBuilder.cs) を参照。

要点:
- `Editor/` フォルダに配置（ビルドに含まれない）
- `[MenuItem("Tools/Switch to WebGL")]` でプラットフォーム切り替え
- `[MenuItem("Tools/Build WebGL")]` でビルド実行
- **圧縮は無効化**（`WebGLCompressionFormat.Disabled`）— ローカルサーバーとの互換性のため
- シーンパスはプロジェクトに合わせて調整（デフォルト: `Assets/Scenes/SampleScene.unity`）
- 出力先: `Build/WebGL`

```csharp
// 圧縮無効化（Brotli は Python 簡易サーバーで非対応）
PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
PlayerSettings.WebGL.decompressionFallback = false;
```

### Step 3: ビルド前クリーンアップ

ビルド前に `Build/WebGL/` 内の StatiCrypt 残留ファイルを確認・削除する。これらが残っていると Unity ビルドが `index.html` を正しく生成できない場合がある。

```bash
# 以下のファイル・フォルダがあれば削除
rm -f Build/WebGL/.staticrypt.json
rm -rf Build/WebGL/encrypted
```

既に HTTP サーバーが `Build/WebGL` で起動中の場合、ファイルがロックされて削除できないため、先にサーバーを停止すること。

### Step 4: ビルド実行

MCP の `execute_menu_item` で順に実行:

1. `Tools/Switch to WebGL` — プラットフォーム切り替え（初回のみ、数分かかる）
2. `Tools/Build WebGL` — ビルド実行（2-5分）

ビルド完了後、コンソールで `[WebGLBuilder] Build succeeded!` を確認する。
`Build target 'WebGL' not supported` エラーが出た場合は Step 1 に戻る。

### Step 5: ローカルサーバー起動

ビルド出力ディレクトリで HTTP サーバーを起動:

```bash
cd Build/WebGL && python -m http.server 8080
```

バックグラウンドで実行し、ユーザーに `http://localhost:8080` を案内する。

### Step 6（オプション）: GitHub Pages にデプロイ

GitHub Pages への公開を希望する場合は、`github-pages-deploy` スキルを使用する。

## サンプル

[assets/WebGLBuilder.cs](assets/WebGLBuilder.cs) — 完全な Editor スクリプト。プラットフォーム切り替え・圧縮無効化・ビルド実行・結果レポートを含む。

## トラブルシューティング

| 症状 | 原因 | 対処 |
|------|------|------|
| `Build target 'WebGL' not supported` | WebGL モジュール未インストール | Unity Hub で追加 + エディタ再起動 |
| `.br` ファイルがブラウザで読めない | Brotli 圧縮有効 | `WebGLCompressionFormat.Disabled` に設定して再ビルド |
| ビルド出力が空 | ビルド失敗 | コンソールのエラーログを確認 |
| `Build succeeded` だが Size が 0 MB | シーンパスが間違い | `scenes` 配列のパスを確認 |
| ブラウザでパスワードを求められる | StatiCrypt の `index.html` が残留 | `Build/WebGL` を削除してクリーンビルド（HTTP サーバー停止が必要） |

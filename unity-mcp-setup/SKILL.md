---
name: unity-mcp-setup
description: Unity MCPのセットアップと接続を行う。Unity EditorとClaude Codeの間でMCP接続を確立するための手順を実行する。「Unity MCP セットアップ」「MCP接続」「Unity MCP設定」「UnityMCP」「MCP For Unity」「MCPが繋がらない」「Unity Editorと接続」といったリクエスト時に使用。新規セットアップとトラブルシューティングの両方に対応。
---

# Unity MCP セットアップ

Unity MCPは2つのコンポーネントで構成される：
- **Unity Editor側**: `com.coplaydev.unity-mcp` (MCP サーバー)
- **Claude Code側**: `mcpforunityserver` (MCPクライアント)

## セットアップ手順

### 1. Unityパッケージのインストール

`Packages/manifest.json` の `dependencies` に追加：

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity"
```

### 2. Unity Editorでの設定

1. Unity Editor下部の「MCP For Unity」タブ → Connectタブを開く
2. ClientドロップダウンをClaude Codeに変更
3. Claude CLI Path を確認: `C:\Users\<user>\AppData\Roaming\npm\claude.cmd`
4. Client Project Dir をUnityプロジェクトルートに設定
5. **Configure** ボタンを押す（Configured 緑丸になればOK）
6. **Start Session** ボタンを押す（Session Active 緑丸になればOK）

### 3. Claude Code再起動

VSCodeで `Ctrl+Shift+P` → Developer: Reload Window

### 4. 接続確認

```
manage_scene(action="get_hierarchy")
```

## 重要な注意事項

- Claude Code側のMCPクライアントは**手動で `claude mcp add` しない**。Unity Editorの Configure ボタンに任せる
- `coplay-mcp-server` と `mcpforunityserver` は**別パッケージ**。正しいのは `mcpforunityserver`
- 複数のMCPサーバーを登録しない。不要なものは `claude mcp remove` で削除

## トラブルシューティング

詳細な手順は [references/unity-mcp-setup-guide.md](references/unity-mcp-setup-guide.md) の「トラブルシューティング」セクションを参照。

### よくある問題

| 症状 | 対処 |
|---|---|
| UnityMCPが `claude mcp list` に表示されない | Unity EditorのConfigureを再実行、またはClaude Code側で手動再登録 |
| Failed to connect | Unity EditorでSession Activeか確認。削除→再Configure |
| 間違ったパッケージ(coplay-mcp等)が登録済み | `claude mcp remove` で削除後、ConfigureでFix |

### 手動再登録コマンド（Configureが効かない場合）

```bash
claude mcp add --scope local --transport stdio UnityMCP \
  -- "C:\Users\<user>\.local\bin\uvx.exe" \
  --offline --prerelease explicit \
  --from "mcpforunityserver>=0.0.0a0" mcp-for-unity
```

### 完全リセット

```bash
claude mcp remove --scope local UnityMCP
claude mcp remove --scope user UnityMCP
claude mcp remove --scope project UnityMCP
```

その後、ステップ2からやり直す。

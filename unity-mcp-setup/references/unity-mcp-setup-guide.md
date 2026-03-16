# Unity MCP セットアップガイド

## 概要

Unity MCPは**2つのコンポーネント**で構成されている：

| 側 | パッケージ名 | 役割 |
|---|---|---|
| Unity Editor側 | `com.coplaydev.unity-mcp` | Unity内でMCPサーバーを起動する |
| Claude Code側 | `mcpforunityserver` | Unity EditorのMCPサーバーに接続するクライアント |

どちらもCoplayDevが提供しており、**両方が正しくセットアップされて初めて接続が成立する**。

---

## 正しい手順

### ステップ1: Unityパッケージのインストール

`Packages/manifest.json` の `dependencies` に以下を追加する：

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity"
```

Unity Editorを開く（または再読み込み）と自動でインストールされる。

### ステップ2: Unity Editorでの設定

1. Unity Editor下部に「**MCP For Unity**」タブが表示される
2. **Connect** タブを開く
3. **Client** ドロップダウンを「**Claude Code**」に変更する
4. 以下のパスが正しいことを確認する：
   - **Claude CLI Path**: `C:\Users\<ユーザー名>\AppData\Roaming\npm\claude.cmd`
   - **Client Project Dir**: Unityプロジェクトのルートパス
5. **「Configure」ボタンを押す**
   - これにより Claude Code側に `mcpforunityserver` が自動登録される
   - 「Configured」（緑丸）になればOK
6. **「Start Session」ボタンを押す**
   - 「Session Active」（緑丸）になればOK

### ステップ3: Claude Codeの再起動

VSCodeで `Ctrl+Shift+P` → 「Developer: Reload Window」を実行する。
新しい会話を開始すると、UnityMCPのツールが使えるようになる。

### ステップ4: 接続確認

Claude Codeで以下のようなツールが使えれば成功：

```
manage_scene(action="get_hierarchy")
```

---

## 今回の失敗と原因

### 失敗1: Claude側のMCPクライアントを手動で先に登録した

**やったこと：**

```bash
claude mcp add --scope user --transport stdio coplay-mcp \
  --env MCP_TOOL_TIMEOUT=720000 \
  -- uvx --python ">=3.11" coplay-mcp-server@latest
```

**何が起きたか：**
- `coplay-mcp-server@latest` というパッケージが登録された
- これは `mcpforunityserver` とは**別のパッケージ**
- Claude Code側では「Connected」と表示されたが、Unity Editorとの通信プロトコルが合わず「Unity Editor is not running」エラーが発生した

**どうすべきだったか：**
- Claude Code側のMCPクライアントは手動で登録しない
- Unity Editorの「Configure」ボタンに任せる（正しいパッケージ `mcpforunityserver` が自動登録される）

### 失敗2: 間違ったパッケージと正しいパッケージが共存した

**やったこと：**
- 手動登録した `coplay-mcp`（ユーザースコープ）がある状態で、Unity Editorの「Configure」で `UnityMCP`（ローカルスコープ）が追加された

**何が起きたか：**
- 2つのMCPサーバーが共存し、どちらが使われているか分かりにくくなった
- `coplay-mcp` を削除した際に、ローカルスコープの `UnityMCP` だけが残ったが、セッションで認識されなかった

**どうすべきだったか：**
- 最初から手動登録をせず、Unity Editorの「Configure」だけで設定する

### 失敗3: スコープの問題で認識されなかった

**やったこと：**
- Unity Editorの「Configure」は `UnityMCP` をローカルスコープ（プロジェクト設定）に登録した
- `claude mcp list` では表示されたり消えたりした

**何が起きたか：**
- ローカルスコープの設定は `.claude.json` の `projects` セクション内に保存される
- プロジェクトパスのマッチングの問題で、VSCode拡張からは認識されないことがあった

**最終的な解決策：**
- ユーザースコープに再登録した：

```bash
claude mcp add --scope user --transport stdio UnityMCP \
  -- "C:\Users\umezy\.local\bin\uvx.exe" \
  --offline --prerelease explicit \
  --from "mcpforunityserver>=0.0.0a0" mcp-for-unity
```

---

## まとめ

| やりがちな間違い | 正しい対応 |
|---|---|
| `claude mcp add` で手動登録する | Unity Editorの「Configure」ボタンに任せる |
| `coplay-mcp-server` を登録する | `mcpforunityserver` が正しいパッケージ |
| 複数のMCPサーバーを登録してしまう | 1つだけにする（不要なものは `claude mcp remove` で削除） |

**最もシンプルな手順：Unityパッケージを入れる → Unity Editorで「Configure」→「Start Session」→ Claude Code再起動**

---

## トラブルシューティング

Unity MCPが接続できない場合、以下のステップを順番に確認する。

### ステップ1: Unity Editor側の状態を確認する

**確認方法：** Unity Editorの「MCP For Unity」タブ → Connectタブを開く

| 確認項目 | 正常な状態 | 異常な場合の対処 |
|---|---|---|
| Session | 🟢 Session Active | 「Start Session」ボタンを押す |
| Client | Claude Code | ドロップダウンで「Claude Code」を選択 |
| Configured | 🟢 Configured | 「Configure」ボタンを押す（ステップ2へ） |
| Claude CLI Path | `C:\Users\<ユーザー名>\AppData\Roaming\npm\claude.cmd` | 正しいパスをBrowseで指定 |
| Client Project Dir | Unityプロジェクトのルートパス | 正しいパスをBrowseで指定 |

**原因：** Sessionが開始されていない場合、Unity Editor側のMCPサーバーが起動しておらず、Claude Codeからの接続先が存在しない。

### ステップ2: Claude Code側の登録状態を確認する

**確認方法：** ターミナルで以下を実行する

```bash
claude mcp list
```

**正常な出力例：**
```
UnityMCP: C:\Users\umezy\.local\bin\uvx.exe --offline --prerelease explicit --from mcpforunityserver>=0.0.0a0 mcp-for-unity - ✓ Connected
```

**異常パターンと対処：**

#### パターンA: UnityMCPが一覧に表示されない

**原因：** Unity Editorの「Configure」が実行されていないか、設定が消えている。Unity Editor側では「Configured」（緑丸）でも、Claude Code側の設定（`.claude.json`）から消えていることがある。

**対処：** Unity Editor側は「Configured」のまま触る必要はない。Claude Code側で手動再登録する：

1. 既存の設定を削除する：
   ```bash
   claude mcp remove --scope local UnityMCP
   ```
2. 再登録する：
   ```bash
   claude mcp add --scope local --transport stdio UnityMCP \
     -- "C:\Users\umezy\.local\bin\uvx.exe" \
     --offline --prerelease explicit \
     --from "mcpforunityserver>=0.0.0a0" mcp-for-unity
   ```
3. `claude mcp list` で `UnityMCP ... ✓ Connected` になることを確認する
4. VSCodeを再起動（またはReload Window）して新しいセッションを開始する

#### パターンB: UnityMCPが表示されるが「Failed to connect」になる

**原因：** 以下のいずれか
- Unity Editorが起動していない、またはSessionが開始されていない
- `uvx.exe` のパスが間違っている
- `mcpforunityserver` パッケージが壊れている

**対処：**
1. Unity Editorで「Session Active」（緑丸）になっているか確認する
2. 一度削除して再登録する：
   ```bash
   claude mcp remove --scope local UnityMCP
   ```
3. Unity Editorの「Configure」ボタンを再度押す
4. `claude mcp list` で `✓ Connected` になることを確認する

#### パターンC: 別名のMCPサーバー（coplay-mcpなど）が登録されている

**原因：** 手動で間違ったパッケージを登録した。`coplay-mcp-server` と `mcpforunityserver` は別物。

**対処：**
1. 不要なサーバーを全て削除する：
   ```bash
   claude mcp remove --scope local coplay-mcp
   claude mcp remove --scope user coplay-mcp
   ```
2. Unity Editorの「Configure」ボタンで正しいパッケージを再登録する

### ステップ3: Claude Codeセッションを再起動する

**原因：** MCPサーバーの設定はセッション開始時に読み込まれるため、設定変更後は再起動が必要。

**対処方法（いずれか1つ）：**

| 方法 | 手順 |
|---|---|
| VSCodeを再起動 | VSCodeを閉じて再度開く |
| ウィンドウをリロード | `Ctrl+Shift+P` →「Developer: Reload Window」 |
| Claude Codeを再起動 | `/quit` で終了し、再度Claude Codeを開始 |

**注意：** 同じセッション内では、`claude mcp list` で `✓ Connected` と表示されていても、新しく登録したMCPツールは使えない。必ず再起動が必要。

### ステップ4: 再起動後の接続確認

新しいセッションで以下を試す：

```
manage_scene(action="get_hierarchy")
```

ツールが実行できれば接続成功。

### それでも接続できない場合

全てクリーンアップしてやり直す：

```bash
# 全スコープからUnityMCP関連を削除
claude mcp remove --scope local UnityMCP
claude mcp remove --scope user UnityMCP
claude mcp remove --scope project UnityMCP
```

その後、[ステップ2: Unity Editorでの設定](#ステップ2-unity-editorでの設定) からやり直す。

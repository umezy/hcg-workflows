# hcg-workflows

Unity ハイパーカジュアルゲームを Claude Code + Unity MCP で開発するためのスキル集です。

## 使い方

1. このリポジトリをクローンまたはダウンロード
2. 各スキルフォルダを `.claude/skills/` にコピー
3. Unity MCP をセットアップ（`unity-mcp-setup` スキル参照）
4. `/dev-game` を実行し、エージェントの質問に回答していく

## 全体フロー

```mermaid
flowchart TD
    Start([ユーザー: ゲームを作って]) --> P1

    subgraph spec["仕様策定フェーズ（Phase 1〜5）"]
        P1["Phase 1: 企画書<br/>ゲームの全体像を決める"]
        P2["Phase 2: 機能仕様書<br/>何ができるかを定義する"]
        P3["Phase 3: 技術仕様書<br/>どう作るかを設計する"]
        P4["Phase 4: テスト仕様書<br/>どう検証するかを決める"]
        P5["Phase 5: タスクリスト<br/>作業を分解・順序づけする"]
    end

    P1 -->|承認| P2 -->|承認| P3 -->|承認| P4 -->|承認| P5

    subgraph impl["実装・検証フェーズ（Phase 6〜8）"]
        P6["Phase 6: 実装＋テスト<br/>機能コードとテストコードを<br/>セットで作成"]
        P7["Phase 7: 統合検証<br/>全テスト一括実行<br/>受け入れテスト"]
        P8["Phase 8: リリース<br/>ビルド・配布"]
    end

    P5 -->|承認| P6 --> P7 --> P8 --> Done([完成])

    style spec fill:#e7f5ff,stroke:#4a9eff
    style impl fill:#fff3e0,stroke:#ff6b6b
```

## スキル一覧

| スキル | 説明 |
|--------|------|
| [dev-game](dev-game/) | 仕様駆動開発（SDD）でゲームを作る。企画書→仕様書→タスクリスト→実装→テスト→リリースまで一貫して管理 |
| [unity-mcp-setup](unity-mcp-setup/) | Unity Editor と Claude Code の MCP 接続をセットアップ |
| [unity-check](unity-check/) | コンパイル確認・Play Mode 動作確認・スクリーンショット検証の 3 段階で実装を検証 |
| [unity-remote-control](unity-remote-control/) | DebugManager + McpRemoteControl でゲーム状態を AI が直接操作。受け入れテストを自動化 |
| [unity-audio-generator](unity-audio-generator/) | C# Editor スクリプトで SE・BGM を生成。外部ツール不要 |
| [unity-webgl-builder](unity-webgl-builder/) | WebGL ビルドとローカルサーバー起動を自動化 |

```mermaid
flowchart LR
    SDD["dev-game"]

    MCP["unity-mcp-setup<br/>MCP接続セットアップ"]
    Verify["unity-check<br/>スクショで動作確認"]
    Debug["unity-remote-control<br/>ゲーム状態をAIが直接操作"]
    Audio["unity-audio-generator<br/>コードだけでSE/BGMを生成"]
    WebGL["unity-webgl-builder<br/>WebGLビルド+ローカルサーバー"]

    SDD -->|"Phase 0: 接続失敗時"| MCP
    SDD -->|"Phase 6: 毎タスクの検証"| Verify
    SDD -->|"Phase 6: DebugManager 実装"| Debug
    SDD -->|"Phase 6: サウンド実装"| Audio
    SDD -->|"Phase 7: 受け入れテスト自動化"| Debug
    SDD -->|"Phase 8: ビルド・配布"| WebGL

    style SDD fill:#4a9eff,color:#fff
    style MCP fill:#51cf66,color:#fff
    style Verify fill:#ffa94d,color:#fff
    style Debug fill:#845ef7,color:#fff
    style Audio fill:#ff6b6b,color:#fff
    style WebGL fill:#20c997,color:#fff
```

## 前提条件

- Unity Editor（2022.3 LTS 以降推奨）
- [MCP For Unity](https://github.com/nicknisi/unity-mcp) がインストール済み
- Claude Code

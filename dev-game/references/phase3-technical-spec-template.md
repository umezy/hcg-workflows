# Phase 3: 技術仕様書テンプレート

成果物: `docs/03-technical-spec.md`

ゲームを「どう作るか」を定義する。

```markdown
# 技術仕様書

## 1. プロジェクト構成

```
Assets/
├── Scripts/
│   ├── Core/           # ゲームマネージャー、状態管理
│   ├── Gameplay/       # ゲームプレイロジック
│   ├── UI/             # UI制御
│   └── Data/           # データクラス、ScriptableObject
├── Prefabs/            # 再利用可能なオブジェクト
├── Scenes/             # シーンファイル
├── Materials/          # マテリアル
├── Sprites/            # 2D画像
└── Audio/              # 効果音・BGM
```

## 2. クラス設計

| クラス名 | 責務 | 依存先 | 作成方法 |
|---------|------|--------|---------|
| GameManager | ゲーム全体の状態管理 | - | `create_script` |
| ScoreManager | スコアの計算・保持 | GameManager | `create_script` |
| UIController | UI表示の更新 | GameManager, ScoreManager | `create_script` |

### [クラス名]
- **責務:** [何を管理するか]
- **主要メソッド:**
  - `MethodName()`: [処理内容]
- **イベント/コールバック:**
  - `OnEventName`: [発火条件]

## 2.5 シーン構成（Unity MCP で構築）

Unity MCP ツールで直接構築するオブジェクト構成を定義する。

| オブジェクト名 | 種別 | コンポーネント | MCPツール |
|--------------|------|-------------|----------|
| GameManager | Empty | GameManager.cs | `manage_gameobject` + `manage_components` |
| Canvas | UI | Canvas, CanvasScaler | `manage_ui` |
| [Prefab名] | Prefab | [コンポーネント一覧] | `manage_prefabs` |

## 3. 状態遷移図

```
[Init] → [Title] → [Playing] → [GameOver] → [Result] → [Title]
```

各状態の遷移条件:
- Init → Title: 初期化完了時
- Title → Playing: スタートボタン押下時
- Playing → GameOver: [敗北条件達成時]
- GameOver → Result: 演出終了時
- Result → Title: リトライ/タイトルボタン押下時

## 4. データ設計

### 保存データ
| データ名 | 型 | 保存方式 | 説明 |
|---------|-----|---------|------|
| ハイスコア | int | PlayerPrefs | 最高スコア |

### ScriptableObject パラメータ
| パラメータ名 | 型 | 初期値 | 説明 |
|-------------|-----|--------|------|
| [名前] | [型] | [値] | [説明] |
```

# Phase 4: テスト仕様書テンプレート

成果物: `docs/04-test-spec.md`

実装前にテスト項目を定義する（仕様駆動の要）。

```markdown
# テスト仕様書

## 1. 単体テスト

機能仕様の各項目に対応するテストケースを作成する。

| テストID | テスト対象 | 入力 | 期待結果 |
|---------|-----------|------|---------|
| UT-001 | [機能名] | [操作/入力] | [期待される結果] |

## 2. 統合テスト

画面遷移・複数コンポーネント連携のテスト。

| テストID | テスト対象 | 手順 | 期待結果 |
|---------|-----------|------|---------|
| IT-001 | [テスト対象] | [操作手順] | [期待される結果] |

## 3. レンダリング整合性テスト

シーン上のオブジェクトが正しく表示・衝突できる状態かを自動検証する。
Phase 6 の L3 検証で「何も映らない」バグを早期に検出するために必須。

| テストID | テスト対象 | 期待結果 |
|---------|-----------|---------|
| RT-001 | 全 SpriteRenderer にスプライトが割り当てられている | sprite != null |
| RT-002 | 全 SpriteRenderer の bounds が非ゼロ | bounds.size > 0 |
| RT-003 | 全 BoxCollider2D のサイズが適切 | size.x * size.y > 0.001 |
| RT-004 | Player が可視スプライトを持つ | SpriteRenderer.sprite != null && enabled |
| RT-005 | Floor/Ceiling が可視スプライトを持つ | SpriteRenderer.sprite != null |
| RT-006 | 障害物テンプレートがスプライトを持つ | 各タグのオブジェクトに sprite 割当済み |
| RT-007 | Player のコライダーがスプライトと整合 | collider size ≥ sprite size × 10% |

実装: `Assets/Tests/EditMode/RenderingIntegrityTests.cs` に EditMode テストとして実装する。
シーンを `EditorSceneManager.OpenScene` で読み込んで検証する。

## 4. 受け入れテスト

機能仕様を満たしているかの最終確認チェックリスト。

- [ ] [チェック項目1]
- [ ] [チェック項目2]
```

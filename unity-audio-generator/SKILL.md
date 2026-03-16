---
name: unity-audio-generator
description: Unity プロジェクト向けにプロシージャル音声（SE・BGM）を C# Editor スクリプトで生成する。「音声を生成」「SEを作って」「BGMを作って」「サウンドアセット」「AudioGenerator」「効果音」「プロシージャルオーディオ」といったリクエスト時に使用。WAV ファイル（44100Hz/16bit/Mono）を Assets フォルダに直接出力し、外部ツール不要で完結する。
---

# Unity Audio Generator

Unity Editor スクリプトで SE・BGM を WAV ファイルとしてプロシージャル生成する。外部ツールや素材サイト不要。

## ワークフロー

1. ユーザーの要件を確認（必要な SE 種類、BGM の雰囲気）
2. `Assets/Scripts/Editor/AudioGenerator.cs` を作成
3. Unity で `Tools/Generate All Audio Assets` メニューを実行
4. AudioManager 等に生成した AudioClip を割り当て

## スクリプト作成ガイド

### 基本構造

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public static class AudioGenerator
{
    private const int SampleRate = 44100;

    [MenuItem("Tools/Generate All Audio Assets")]
    public static void GenerateAll()
    {
        GenerateSE();
        GenerateBGM();
        AssetDatabase.Refresh();
        Debug.Log("[AudioGenerator] All audio assets generated.");
    }

    // SE/BGM 生成メソッド...
    // SaveWav メソッド...
}
```

- `Editor/` フォルダに配置（ビルドに含まれない）
- `[MenuItem]` で Unity メニューからワンクリック生成
- 生成後 `AssetDatabase.Refresh()` で自動インポート

### 出力先

```
Assets/Audio/SE/   → SE ファイル群
Assets/Audio/BGM/  → BGM ファイル群
```

ディレクトリは `SaveWav` 内で自動作成される。

### 波形パターンの選び方

| 用途 | 推奨波形 | パラメータ目安 |
|------|---------|--------------|
| ヒット・タップ | Sine + 高 decay | 0.1-0.2s, 400-600Hz, decay 8-15 |
| ボタンクリック | Sine + 高 decay | 0.05-0.1s, 600-800Hz, decay 12-20 |
| 爆発・衝撃 | Noise + ローパス | 0.3-0.5s, cutoff 80-150Hz |
| 出現・スライド | Sweep (上昇) | 0.15-0.3s, 300→600Hz |
| ダメージ・ミス | Sweep (下降) | 0.2-0.3s, 500→100Hz |
| コイン・アイテム | Sine 高音 | 0.1s, 1000-1500Hz, decay 10-15 |
| クリアジングル | Arpeggio (上昇) | C5-E5-G5-C6, 0.2s/note |
| ゲームオーバー | Arpeggio (下降) | B4-G4-E4-C4, 0.2s/note |
| BGM | Melody+Bass+Perc | 8-16s loop, sine 0.3vol |

波形生成の詳細パターンとコード例は [waveform-patterns.md](references/waveform-patterns.md) を参照。

### AudioManager との連携

生成した WAV は Unity が自動で AudioClip としてインポートする。AudioManager の SerializeField に Inspector からドラッグ＆ドロップ、または MCP ツールで割り当てる。

```csharp
// AudioManager 側の典型的な構成
[SerializeField] private AudioClip hitSE;
[SerializeField] private AudioClip bombSE;
[SerializeField] private AudioClip bgmClip;
```

## サンプル

[assets/samples/](assets/samples/) にもぐら叩きゲーム用に生成した実例を同梱。

- **AudioGenerator.cs** - 完全な生成スクリプト（SE 8種 + BGM 1曲）
- **SE**: hit, bomb, appear, combo, warning, button, clear, gameover（各 WAV）
- **BGM**: bgm.wav（8秒ループ、メロディ+ベース+パーカッション）

新規プロジェクトでは `AudioGenerator.cs` をベースにカスタマイズするのが最速。

## 注意事項

- 生成スクリプトは `Editor/` フォルダ必須（`UnityEditor` 名前空間使用のため）
- WAV は 44100Hz / 16bit / Mono 固定（ブラウザゲーム向けに軽量）
- BGM はループ再生前提で末尾を自然に終わらせる
- volume は 0.3-0.8 程度に抑える（音割れ防止）

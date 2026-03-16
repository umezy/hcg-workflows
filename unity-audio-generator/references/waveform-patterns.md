# Waveform Generation Patterns

Unity C# Editor script で使える波形生成パターン集。すべて `44100Hz / 16bit / Mono` WAV 出力。

## Table of Contents

1. [基本波形](#基本波形)
2. [SE パターン](#se-パターン)
3. [BGM パターン](#bgm-パターン)
4. [音階リファレンス](#音階リファレンス)
5. [WAV 書き出し](#wav-書き出し)

---

## 基本波形

### Sine Tone (減衰付き)
```csharp
// パラメータ: duration(秒), freq(Hz), volume(0-1), decay(減衰速度)
float t = (float)i / SampleRate;
float envelope = Mathf.Exp(-decay * t);
data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
```
用途: ビープ音、ヒット音、ボタン音

### Square Wave
```csharp
float val = Mathf.Sin(2f * Mathf.PI * freq * t) >= 0 ? 1f : -1f;
data[i] = val * volume * envelope;
```
用途: レトロゲーム風SE、8bit風BGM

### Triangle Wave
```csharp
float phase = (t * freq) % 1f;
float val = 4f * Mathf.Abs(phase - 0.5f) - 1f;
data[i] = val * volume * envelope;
```
用途: 柔らかい音色、ファミコン風

### Noise (ローパスフィルタ付き)
```csharp
// filterFreq: カットオフ周波数 (低いほど暗い音)
float rc = 1f / (2f * Mathf.PI * filterFreq);
float alpha = dt / (rc + dt);
float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
prev = prev + alpha * (noise - prev);
data[i] = prev * volume * envelope * 2f;
```
用途: 爆発音、風音、ホワイトノイズ系

---

## SE パターン

### ヒット音 / タップ音
```csharp
SaveWav("hit.wav", GenerateTone(0.15f, 440f, 0.8f, decay: 8f));
```

### ボタンクリック
```csharp
SaveWav("button.wav", GenerateTone(0.08f, 660f, 0.4f, decay: 15f));
```

### 爆発 / 衝撃
```csharp
SaveWav("bomb.wav", GenerateNoiseBurst(0.4f, 0.9f, 100f));
```

### 出現 / スライドイン (周波数スイープ)
```csharp
// startFreq → endFreq へスイープ
float progress = t / duration;
float freq = Mathf.Lerp(startFreq, endFreq, progress);
// 上昇: (300, 600)  下降: (600, 300)
SaveWav("appear.wav", GenerateSweep(0.2f, 300f, 600f, 0.5f));
```

### コイン / アイテム取得
```csharp
SaveWav("coin.wav", GenerateTone(0.1f, 1320f, 0.5f, decay: 12f));
```

### 警告ビープ
```csharp
SaveWav("warning.wav", GenerateTone(0.1f, 1000f, 0.7f, decay: 12f));
```

### ジングル (アルペジオ)
```csharp
// ascending=true: クリア / ascending=false: ゲームオーバー
float[] notes = ascending
    ? new float[] { 523.25f, 659.25f, 783.99f, 1046.5f } // C5-E5-G5-C6
    : new float[] { 493.88f, 392f, 329.63f, 261.63f };   // B4-G4-E4-C4
float noteDuration = 0.2f;
// 各ノートを順番に減衰トーンで配置
```

### ジャンプ
```csharp
SaveWav("jump.wav", GenerateSweep(0.15f, 200f, 800f, 0.6f));
```

### ダメージ / ミス
```csharp
SaveWav("damage.wav", GenerateSweep(0.25f, 500f, 100f, 0.7f));
```

### パワーアップ
```csharp
// 3段階上昇
float[] notes = { 440f, 554.37f, 659.25f }; // A4-C#5-E5
float noteDuration = 0.15f;
```

---

## BGM パターン

### シンプルメロディ BGM
```csharp
// melody: ノート配列, beatDuration: 1ビートの秒数
float melodyVol = 0.3f * Mathf.Clamp01(1f - beatT * 1.5f);
float melodyVal = Mathf.Sin(2f * Mathf.PI * melody[beatIndex] * t) * melodyVol;
// + ベース (1オクターブ下) + パーカッション (ビート頭ノイズ)
```

### キー/スケール別メロディ配列

**C Major (明るい):**
```
{ 261.63f, 293.66f, 329.63f, 349.23f, 392f, 440f, 493.88f, 523.25f }
```

**A Minor (暗い):**
```
{ 220f, 246.94f, 261.63f, 293.66f, 329.63f, 349.23f, 392f, 440f }
```

**Pentatonic (和風/アジアン):**
```
{ 261.63f, 293.66f, 329.63f, 392f, 440f }
```

### BGM 構成テンプレート
```
duration: 8-16秒 (ループ)
melody:   sine wave, 0.3 volume
bass:     melody * 0.5 freq, 0.15 volume
perc:     noise burst at beat head, 0.2 volume, 0.05s decay
```

---

## 音階リファレンス

| 音名 | Oct3 | Oct4 | Oct5 | Oct6 |
|------|------|------|------|------|
| C | 130.81 | 261.63 | 523.25 | 1046.50 |
| D | 146.83 | 293.66 | 587.33 | 1174.66 |
| E | 164.81 | 329.63 | 659.25 | 1318.51 |
| F | 174.61 | 349.23 | 698.46 | 1396.91 |
| G | 196.00 | 392.00 | 783.99 | 1567.98 |
| A | 220.00 | 440.00 | 880.00 | 1760.00 |
| B | 246.94 | 493.88 | 987.77 | 1975.53 |

---

## WAV 書き出し

```csharp
static void SaveWav(string path, float[] samples)
{
    string dir = Path.GetDirectoryName(path);
    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    using (var stream = new FileStream(path, FileMode.Create))
    using (var writer = new BinaryWriter(stream))
    {
        int channels = 1, bitsPerSample = 16;
        int byteRate = SampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;
        int dataSize = samples.Length * blockAlign;
        writer.Write(new char[] { 'R','I','F','F' });
        writer.Write(36 + dataSize);
        writer.Write(new char[] { 'W','A','V','E' });
        writer.Write(new char[] { 'f','m','t',' ' });
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(SampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)bitsPerSample);
        writer.Write(new char[] { 'd','a','t','a' });
        writer.Write(dataSize);
        for (int i = 0; i < samples.Length; i++)
        {
            short val = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
            writer.Write(val);
        }
    }
}
```

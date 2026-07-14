# v1.0.0 - 暗所視 for YMM4

YukkuriMovieMaker4 向けの暗所視エフェクトプラグインの初回リリースです。
ガウスぼかしを 2 段掛けて細部を落とし、その差から輪郭を残します。
映像の明るさから桿体側の輝度を求め、明るい部分は昼の見えのまま残しつつ、暗い部分ほど暗所視の見えへ寄せます。
暗部では色を青へずらし、シード値とシーン座標から決まる桿体ノイズを重ねます。
8 言語のリソース構成の UI を備えます。

---

## 新機能

### 1. ピクセルシェーダー

`ScotopicVision.hlsl` の `main` は、元映像と 2 段のガウスぼかしを入力に取り、暗所視の見えを合成します。ソースのアルファが 0 以下、または暗さが 0 以下のときは、ソースをそのまま返します。ノイズは `Hash.hlsli` の `hash33` を用います。

#### 輪郭の保持

`NightFilter` は、近距離と遠距離のぼかしの差を求め、その符号を保ったまま `edgeGamma` でべき乗して鋭くします。差に `sign(diff) × pow(abs(diff), 1 / edgeGamma)` を掛け、遠距離のぼかしへ足し戻します。細部を落としながら輪郭を残します。

#### 暗所視への寄せ

`ScotopicLuminance` は、映像を XYZ へ変換し、桿体側の輝度を求めます。この輝度を白色基準 `2.31` で正規化し、`smoothstep(threshold × 0.6, threshold, scotopic)` で昼の見えの割合を求めます。しきい値より明るい部分は昼の見えのまま残ります。合成の重みは `darkness × (1 − photopic)` です。

#### 色シフトとノイズ

`PurkinjeShift` は、映像を LMS へ変換し、桿体の応答から色を青へずらす差分を求めます。差分に `0.05 × purkinje` を掛けて足します。`RodNoise` は、シーン座標の整数位置とシード値から `hash33` で桿体ノイズを求めます。色を持たない粒状ノイズを暗部へ重ねます。

| 値 | 説明 |
|---|---|
| `edgeGamma` | 輪郭を残すべき指数 |
| `threshold` | 昼の見えのまま残る明るさの境界 |
| `purkinje` | 青へ寄せる色シフトの強さ |
| `noiseLevel` | 暗部へ重ねる桿体ノイズの量 |
| `seed` | ノイズの分布を固定する値 |

#### 合成

`result = lerp(daylight, shifted, weight)` で、元の見えと暗所視の見えを重みで混ぜます。出力はプリマルチプライドを保ち、`float4(result × source.a, source.a)` を返します。

---

### 2. カスタムシェーダーエフェクト

`ScotopicVisionCustomEffect` は `[CustomEffect(3)]` の 3 入力エフェクトです。入力 0 は元映像、入力 1 と 2 は近距離と遠距離のガウスぼかしです。公開プロパティは `SetValue` を介して定数バッファーへ転送します。各プロパティは代入時にシェーダーが前提とする範囲へ制限します。

| プロパティ | 型 | 範囲 |
|---|---|---|
| `EdgeGamma` | `float` | 1〜3 |
| `Purkinje` | `float` | 0〜4 |
| `Darkness` | `float` | 0〜1 |
| `Threshold` | `float` | 1e-3〜2 |
| `NoiseLevel` | `float` | 0〜0.25 |
| `Seed` | `int` | 0〜9999 |

`ConstantBuffer` のレイアウトは以下のとおりです。末尾に 2 つの詰め物を置き、合計 32 バイトを 16 バイトの倍数に揃えます。

| フィールド | 型 | 説明 |
|---|---|---|
| `EdgeGamma` | `float` | 輪郭を残す指数 |
| `Purkinje` | `float` | 色シフトの強さ |
| `Darkness` | `float` | 暗所視へ寄せる強さ |
| `Threshold` | `float` | 中間視しきい値 |
| `NoiseLevel` | `float` | 桿体ノイズの量 |
| `Seed` | `int` | ノイズの分布を固定する値 |
| `Pad0` / `Pad1` | `float` | 詰め物 |

`MapInputRectsToOutputRect` は出力矩形を先頭の入力矩形へ合わせます。`MapOutputRectToInputRects` は各入力矩形を出力矩形へ合わせます。効果は各ピクセルの位置でのみ映像を参照し、位置をずらさないため、出力範囲は入力範囲と一致します。

シェーダーリソース: `pack://application:,,,/ScotopicVision;component/Shaders/ScotopicVision.cso`（ps_5_0、`ShaderResourceUri.Get` が生成）

---

### 3. エフェクト定義

`ScotopicVisionEffect` は YMM4 の映像エフェクトとして宣言されます。

`[VideoEffect]` 属性は以下のパラメーターで宣言されます。

- 表示名: `Texts.ScotopicVisionEffectName`（ローカライズキー、日本語では「暗所視」）
- カテゴリー: `VideoEffectCategories.Filtering`
- 検索タグ: `TagScotopic`・`TagNightVision`・`TagPurkinje`
- `IsAviUtlSupported = false` により AviUtl 向け EXO 出力は非対応
- `ResourceType = typeof(Texts)` でローカライズリソースを指定

`Label` プロパティは `Texts.ScotopicVisionEffectName` を返します。

公開プロパティは以下のとおりです。すべて「暗所視」グループに属します。

| プロパティ | 型 | デフォルト | 内部範囲 | アニメーション |
|---|---|---|---|---|
| `Darkness` | `Animation` | 70 | 0〜100 | あり |
| `Threshold` | `Animation` | 70 | 0〜100 | あり |
| `Acuity` | `Animation` | 2.5 | 0〜64 | あり |
| `EdgeCrispness` | `Animation` | 1.25 | 1〜3 | あり |
| `Purkinje` | `Animation` | 100 | 0〜200 | あり |
| `Noise` | `Animation` | 30 | 0〜100 | あり |
| `Seed` | `int` | 0 | 0〜9999 | なし |

`GetAnimatables` は `Darkness`・`Threshold`・`Acuity`・`EdgeCrispness`・`Purkinje`・`Noise` を返します。

`CreateExoVideoFilters` は空のシーケンスを返します。`CreateVideoEffect` は映像処理用のインスタンスを生成します。

---

### 4. フレームごとの更新

各フレームで YMM4 の `EffectDescription` からフレーム位置、アイテム長、FPS を取得し、アニメーション値を評価します。前フレームと値が異なる項目だけをカスタムシェーダーとガウスぼかしへ転送します。

| パラメータ | 変換 |
|---|---|
| `Acuity` | 近距離のぼかしの標準偏差へ、遠距離は 1.6 倍 |
| `EdgeCrispness` | そのまま `EdgeGamma` へ |
| `Purkinje` | `value / 100` |
| `Darkness` | `value / 100` |
| `Threshold` | `value / 100` |
| `Noise` | `value / 100 × 0.05` を `NoiseLevel` へ |
| `Seed` | 整数のまま |

近距離と遠距離のガウスぼかしは、いずれも品質優先の設定で生成します。元映像を入力 0 へ、それぞれのぼかしの出力をカスタムシェーダーの入力 1 と 2 へ接続します。エフェクトチェーンのクリア時は、すべての入力を `null` へ戻します。

---

### 5. ローカライズ

`Texts` クラスは `[AutoGenLocalizer]` 属性を持つ `partial` クラスとして宣言されます。
`YukkuriMovieMaker.Generator` のソースジェネレーターが `Texts.csv` を処理し、各ロケールのリソースファイルを自動生成します。

対応リソース: 日本語（`ja-jp`）・英語（`en-us`）・中国語簡体字（`zh-cn`）・中国語繁体字（`zh-tw`）・韓国語（`ko-kr`）・スペイン語（`es-es`）・アラビア語（`ar-sa`）・インドネシア語（`id-id`）

ローカライズキーの一覧は以下のとおりです。

| キー | ja-jp |
|---|---|
| `ScotopicVisionEffectName` | 暗所視 |
| `ScotopicVisionDarkness` | 暗さ |
| `ScotopicVisionThreshold` | 中間視しきい値 |
| `ScotopicVisionAcuity` | 視力低下 |
| `ScotopicVisionEdgeCrispness` | エッジの鋭さ |
| `ScotopicVisionPurkinje` | 色シフト |
| `ScotopicVisionNoise` | ノイズ |
| `ScotopicVisionSeed` | シード |
| `TagScotopic` | 暗所視 |
| `TagNightVision` | 暗視 |
| `TagPurkinje` | プルキンエ |

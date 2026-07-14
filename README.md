# 暗所視 for YMM4

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](#)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](#)
[![Release](https://img.shields.io/github/v/release/routersys/YMM4-ScotopicVision.svg)](https://github.com/routersys/YMM4-ScotopicVision/releases)

---

YukkuriMovieMaker4（YMM4）上で動作する、映像を人の暗所視に近づける映像エフェクトプラグインです。
暗い部分の細部と色を落とし、輪郭を残しながら、暗部を青へ寄せて桿体由来の粒状ノイズを重ねます。
明るい部分は昼の見えのまま残ります。
数値パラメータはアニメーションに対応しています。

![Image](https://github.com/routersys/YMM4-ScotopicVision/blob/main/docs/ScotopicVision.png)

---

## 目次

1. [概要](#概要)
2. [動作要件](#動作要件)
3. [インストール方法](#インストール方法)
4. [主な機能](#主な機能)
   - [1. 暗所視への変換](#1-暗所視への変換)
   - [2. 視力低下と輪郭](#2-視力低下と輪郭)
   - [3. 色シフト](#3-色シフト)
   - [4. 桿体ノイズ](#4-桿体ノイズ)
5. [パラメータ一覧](#パラメータ一覧)
6. [制限事項](#制限事項)
7. [注意事項](#注意事項)
8. [免責事項](#免責事項)
9. [サードパーティライセンス](#サードパーティライセンス)
10. [ライセンス](#ライセンス)

---

## 概要

本プラグインは YMM4 の映像エフェクトとして動作し、エフェクトの種類一覧では「暗所視」として表示されます。カテゴリはフィルタリングです。

暗い環境では、人の網膜は錐体から桿体へ感度の中心が移ります。桿体は色を見分けず、解像度が低く、青い光へ強く反応します。本プラグインはこの見えの変化を再現します。ガウスぼかしを 2 段掛けて細部を落とし、その差から輪郭を残します。映像の明るさから桿体側の輝度を求め、明るい部分は昼の見えのまま残しつつ、暗い部分ほど暗所視の見えへ寄せます。暗部では色を青へずらし、桿体由来の粒状ノイズを重ねます。

ノイズはシード値とシーン座標から決定論的に決まります。同じ条件では常に同じ結果になり、フレーム間でちらつきません。

このエフェクトは AviUtl 向けの EXO 出力に対応していません。

---

## 動作要件

| 項目 | 要件 |
|---|---|
| OS | Windows 10 バージョン 2004 以降、または Windows 11 の 64 ビット版 |
| YukkuriMovieMaker4 | 最新版を推奨 |
| ランタイム | .NET 10.0 |

---

## インストール方法

1. [Releases](https://github.com/routersys/YMM4-ScotopicVision/releases/latest) ページから最新のプラグインファイル（`.ymme`）をダウンロードしてください。
2. YMM4 が起動していないことを確認し、ダウンロードしたファイルを実行してインストールします。
3. YMM4 を起動し、タイムライン上のアイテムに映像エフェクトを追加します。
4. 映像エフェクトの種類として「暗所視」を選択してください。

---

## 主な機能

### 1. 暗所視への変換

映像の明るさから桿体側の輝度を求め、暗い部分ほど暗所視の見えへ寄せます。暗さで全体の強さを調整します。中間視しきい値で、昼の見えのまま残す明るさの境界を決めます。しきい値より明るい部分は元の色を保ち、暗い部分だけが暗所視へ変わります。

### 2. 視力低下と輪郭

視力低下でぼかしの大きさを決めます。大きいほど細部が失われ、夜に細かい模様が見えにくくなる様子を再現します。エッジの鋭さで、細部を落としながら輪郭をどれだけ残すかを決めます。高いほど輪郭がはっきりと残ります。

### 3. 色シフト

色シフトで、暗部の色を青へずらす強さを決めます。桿体が青い光へ強く反応する性質を再現します。値を上げると、暗い部分ほど青みが増します。

### 4. 桿体ノイズ

ノイズで、暗部へ重ねる粒状ノイズの量を決めます。桿体が働くときに現れる、ざらついた見えを再現します。シード値でノイズの分布を固定します。シード値が同じであれば、模様は常に同じになります。

---

## パラメータ一覧

| パラメータ名 | 型 | デフォルト | スライダー表示範囲 | アニメーション | 説明 |
|---|---|---|---|---|---|
| 暗さ | 数値 | 70% | 0 〜 100% | ✔ | 暗所視へ寄せる全体の強さです。 |
| 中間視しきい値 | 数値 | 70% | 0 〜 100% | ✔ | 明部が昼の見えのまま残る明るさの境界です。 |
| 視力低下 | 数値 | 2.5px | 0 〜 20px | ✔ | 夜に見えなくなる細部の大きさです。 |
| エッジの鋭さ | 数値 | 1.25 | 1 〜 3 | ✔ | 細部を落としながら輪郭を残す強さです。 |
| 色シフト | 数値 | 100% | 0 〜 200% | ✔ | 暗部を青へ寄せる色ずれの強さです。 |
| ノイズ | 数値 | 30% | 0 〜 100% | ✔ | 暗部へ重ねる桿体ノイズの量です。 |
| シード | 整数 | 0 | 0 〜 9999 | ✗ | ノイズの分布を固定する値です。 |

---

## 制限事項

- AviUtl 向けの EXO 出力に対応していません。
- 完全に透明な部分には効果が乗りません。
- 中間視しきい値より明るい部分は、暗所視へ変わらず元の見えのまま残ります。

---

## 注意事項

- AviUtl 非対応: 本プラグインは AviUtl 向けの EXO 出力に対応していません。
- 効果の掛かる範囲: 効果は映像の明るさに応じて働きます。明るい部分ほど元の見えを保ち、暗い部分ほど暗所視へ寄ります。
- ノイズの再現性: ノイズはシード値とシーン座標から決まるため、同じ条件では常に同じ結果になり、フレーム間でちらつきません。
- 本プラグインを使用する前に、YMM4 プロジェクトファイルのバックアップを作成することを推奨します。

---

## 免責事項

本プラグインは MIT ライセンスのもとで公開されています。

本ソフトウェアは「現状のまま」提供されており、明示・黙示を問わず、商品性、特定目的への適合性、および権利非侵害に関する保証を含む、いかなる種類の保証も行いません。

作者は、本プラグインの使用または使用不能に起因するいかなる損害についても、一切の責任を負いません。
ご利用は自己責任でお願いします。

---

## サードパーティライセンス

本プラグインは以下のサードパーティのコードを使用しています。ライセンスの全文は、リポジトリの [`ScotopicVision/Shaders/Hash.hlsli`](ScotopicVision/Shaders/Hash.hlsli) の冒頭に収録しています。

| ソフトウェア | 用途 | ライセンス |
|---|---|---|
| [Hash without Sine](https://www.shadertoy.com/view/4djSRW) | 桿体ノイズに使うハッシュ関数（`Hash.hlsli`） | MIT License |

### Hash without Sine（MIT License）

`ScotopicVision/Shaders/Hash.hlsli` は David Hoskins 氏の "Hash without Sine" を基にし、饅頭遣い（manju-summoner）氏が改変したものです。以下は同ファイルに収録された著作権表示です。

```
Copyright (c)2014 David Hoskins.
Modifications Copyright (c) 2023 manju-summoner.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## ライセンス

[MIT License](LICENSE.txt)

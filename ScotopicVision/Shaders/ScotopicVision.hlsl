#include "Hash.hlsli"

Texture2D SourceTexture : register(t0);
SamplerState SourceSampler : register(s0);
Texture2D BlurNearTexture : register(t1);
SamplerState BlurNearSampler : register(s1);
Texture2D BlurFarTexture : register(t2);
SamplerState BlurFarSampler : register(s2);

cbuffer Constants : register(b0)
{
    float edgeGamma  : packoffset(c0.x);
    float purkinje   : packoffset(c0.y);
    float darkness   : packoffset(c0.z);
    float threshold  : packoffset(c0.w);
    float noiseLevel : packoffset(c1.x);
    int seed         : packoffset(c1.y);
    float pad0       : packoffset(c1.z);
    float pad1       : packoffset(c1.w);
};

static const float3x3 RgbToXyz = float3x3(
    0.5149, 0.3244, 0.1607,
    0.2654, 0.6704, 0.0642,
    0.0248, 0.1248, 0.8504);

static const float3x3 RgbToLms = float3x3(
    0.38159, 0.57849, 0.03996,
    0.19690, 0.72455, 0.07851,
    0.02480, 0.12480, 0.85040);

static const float3x3 LmsToRgb = float3x3(
     4.46096, -3.58251,  0.12113,
    -1.21753,  2.38007, -0.16252,
     0.04858, -0.24482,  1.19629);

static const float ScotopicWhite = 2.31;
static const float RodShiftScale = 0.05;

float4 SafeSample(Texture2D tex, SamplerState smp, float2 uv)
{
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
        return float4(0.0, 0.0, 0.0, 0.0);
    return tex.SampleLevel(smp, uv, 0);
}

float3 Straighten(float4 premultiplied)
{
    float alpha = max(premultiplied.a, 1e-5);
    return saturate(premultiplied.rgb / alpha);
}

float ScotopicLuminance(float3 straight)
{
    float3 xyz = mul(RgbToXyz, straight);
    float denom = max(xyz.x, 1e-4);
    float v = xyz.y * (1.33 * (1.0 + (xyz.y + xyz.z) / denom) - 1.68);
    return max(v, 0.0);
}

float3 NightFilter(float3 near, float3 far, float gamma)
{
    float3 diff = near - far;
    float3 sharpened = sign(diff) * pow(abs(diff), 1.0 / max(gamma, 1e-4));
    return saturate(far + sharpened);
}

float3 PurkinjeShift(float3 straight, float rod, float strength)
{
    float3 q = mul(RgbToLms, straight);
    float gL = 1.0 / sqrt(1.0 + 0.33 * (q.x + 0.25 * rod));
    float gM = 1.0 / sqrt(1.0 + 0.33 * (q.y + 0.25 * rod));
    float gS = 1.0 / sqrt(1.0 + 0.33 * (q.z + 0.40 * rod));
    float lTerm = gL / 0.637;
    float mTerm = gM / 0.392;
    float sTerm = gS / 1.606;
    float lumTerm = 0.619 * lTerm + 0.381 * mTerm;
    float redGreen = 3.0 * (1.111 * mTerm - 0.939 * lTerm) * rod;
    float blueYellow = 3.0 * (0.4 * sTerm - 0.15 * lumTerm) * rod;
    float luminance = lumTerm * rod;
    float3 dq = float3(
        0.5 * (luminance - redGreen),
        0.5 * (redGreen + luminance),
        blueYellow + luminance);
    return mul(LmsToRgb, dq) * (RodShiftScale * strength);
}

float3 RodNoise(float2 scene, float amount)
{
    float3 samples = hash33(float3(floor(scene), (float)seed));
    return (samples.x + samples.y + samples.z - 1.5) * 2.0 * amount;
}

float4 main(
    float4 position : SV_POSITION,
    float4 scenePosition : SCENE_POSITION,
    float4 uv0 : TEXCOORD0,
    float4 uv1 : TEXCOORD1,
    float4 uv2 : TEXCOORD2
) : SV_TARGET
{
    float4 source = SafeSample(SourceTexture, SourceSampler, uv0.xy);
    if (darkness <= 0.0 || source.a <= 0.0)
        return source;

    float3 daylight = Straighten(source);
    float3 near = Straighten(SafeSample(BlurNearTexture, BlurNearSampler, uv1.xy));
    float3 far = Straighten(SafeSample(BlurFarTexture, BlurFarSampler, uv2.xy));

    float3 night = NightFilter(near, far, edgeGamma);

    float scotopic = ScotopicLuminance(near) / ScotopicWhite;
    float photopic = smoothstep(threshold * 0.6, threshold, scotopic);
    float weight = darkness * (1.0 - photopic);

    float3 shifted = saturate(night + PurkinjeShift(night, scotopic, purkinje));
    shifted = saturate(shifted + RodNoise(scenePosition.xy, noiseLevel));

    float3 result = lerp(daylight, shifted, weight);
    return float4(result * source.a, source.a);
}

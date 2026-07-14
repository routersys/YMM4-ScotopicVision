using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace ScotopicVision
{
    internal sealed class ScotopicVisionCustomEffect(IGraphicsDevicesAndContext devices) : D2D1CustomShaderEffectBase(Create<EffectImpl>(devices))
    {
        private enum PropertyIndex
        {
            EdgeGamma = 0,
            Purkinje,
            Darkness,
            Threshold,
            NoiseLevel,
            Seed,
        }

        public float EdgeGamma { set => SetValue((int)PropertyIndex.EdgeGamma, value); }
        public float Purkinje { set => SetValue((int)PropertyIndex.Purkinje, value); }
        public float Darkness { set => SetValue((int)PropertyIndex.Darkness, value); }
        public float Threshold { set => SetValue((int)PropertyIndex.Threshold, value); }
        public float NoiseLevel { set => SetValue((int)PropertyIndex.NoiseLevel, value); }
        public int Seed { set => SetValue((int)PropertyIndex.Seed, value); }

        [CustomEffect(3)]
        private sealed class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
        {
            private ConstantBuffer _cb;

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.EdgeGamma)]
            public float EdgeGamma { get => _cb.EdgeGamma; set { _cb.EdgeGamma = Clamp(value, 1f, 3f, 1.25f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Purkinje)]
            public float Purkinje { get => _cb.Purkinje; set { _cb.Purkinje = Clamp(value, 0f, 4f, 1f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Darkness)]
            public float Darkness { get => _cb.Darkness; set { _cb.Darkness = Clamp(value, 0f, 1f, 0.7f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Threshold)]
            public float Threshold { get => _cb.Threshold; set { _cb.Threshold = Clamp(value, 1e-3f, 2f, 0.7f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.NoiseLevel)]
            public float NoiseLevel { get => _cb.NoiseLevel; set { _cb.NoiseLevel = Clamp(value, 0f, 0.25f, 0.015f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Int32, (int)PropertyIndex.Seed)]
            public int Seed { get => _cb.Seed; set { _cb.Seed = Math.Clamp(value, 0, 9999); UpdateConstants(); } }

            public EffectImpl() : base(ShaderResourceUri.Get("ScotopicVision"))
            {
                _cb.EdgeGamma = 1.25f;
                _cb.Purkinje = 1f;
                _cb.Darkness = 0.7f;
                _cb.Threshold = 0.7f;
                _cb.NoiseLevel = 0.015f;
            }

            protected override void UpdateConstants()
            {
                drawInformation?.SetPixelShaderConstantBuffer(_cb);
            }

            public override void MapInputRectsToOutputRect(RawRect[] inputRects, RawRect[] inputOpaqueSubRects, out RawRect outputRect, out RawRect outputOpaqueSubRect)
            {
                outputRect = inputRects.Length > 0 ? inputRects[0] : default;
                outputOpaqueSubRect = default;
            }

            public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
            {
                for (int i = 0; i < inputRects.Length; i++)
                    inputRects[i] = outputRect;
            }

            private static float Clamp(float value, float minimum, float maximum, float fallback)
            {
                if (!float.IsFinite(value))
                    return fallback;
                return Math.Clamp(value, minimum, maximum);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct ConstantBuffer
            {
                public float EdgeGamma;
                public float Purkinje;
                public float Darkness;
                public float Threshold;
                public float NoiseLevel;
                public int Seed;
                public float Pad0;
                public float Pad1;
            }
        }
    }
}

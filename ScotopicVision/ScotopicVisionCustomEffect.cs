using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace ScotopicVision
{
    internal sealed class ScotopicVisionCustomEffect(IGraphicsDevicesAndContext devices) : D2D1CustomShaderEffectBase(Create<EffectImpl>(devices))
    {
        public float EdgeGamma { set => SetValue((int)EffectImpl.PropertyIndex.EdgeGamma, value); }
        public float Purkinje { set => SetValue((int)EffectImpl.PropertyIndex.Purkinje, value); }
        public float Darkness { set => SetValue((int)EffectImpl.PropertyIndex.Darkness, value); }
        public float Threshold { set => SetValue((int)EffectImpl.PropertyIndex.Threshold, value); }
        public float NoiseLevel { set => SetValue((int)EffectImpl.PropertyIndex.NoiseLevel, value); }
        public int Seed { set => SetValue((int)EffectImpl.PropertyIndex.Seed, value); }

        [CustomEffect(3)]
        sealed class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
        {
            ConstantBuffer _constants = new()
            {
                EdgeGamma = 1.25f,
                Purkinje = 1f,
                Darkness = 0.7f,
                Threshold = 0.7f,
                NoiseLevel = 0.015f,
                Seed = 0,
            };

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.EdgeGamma)]
            public float EdgeGamma { get => _constants.EdgeGamma; set { _constants.EdgeGamma = Clamp(value, 1f, 3f, 1.25f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Purkinje)]
            public float Purkinje { get => _constants.Purkinje; set { _constants.Purkinje = Clamp(value, 0f, 4f, 1f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Darkness)]
            public float Darkness { get => _constants.Darkness; set { _constants.Darkness = Clamp(value, 0f, 1f, 0.7f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.Threshold)]
            public float Threshold { get => _constants.Threshold; set { _constants.Threshold = Clamp(value, 1e-3f, 2f, 0.7f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)PropertyIndex.NoiseLevel)]
            public float NoiseLevel { get => _constants.NoiseLevel; set { _constants.NoiseLevel = Clamp(value, 0f, 0.25f, 0.015f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Int32, (int)PropertyIndex.Seed)]
            public int Seed { get => _constants.Seed; set { _constants.Seed = Math.Clamp(value, 0, 9999); UpdateConstants(); } }

            public EffectImpl() : base(ShaderResourceUri.Get("ScotopicVision"))
            {
            }

            protected override void UpdateConstants()
            {
                drawInformation?.SetPixelShaderConstantBuffer(_constants);
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

            static float Clamp(float value, float minimum, float maximum, float fallback)
            {
                if (!float.IsFinite(value))
                    return fallback;
                return Math.Clamp(value, minimum, maximum);
            }

            [StructLayout(LayoutKind.Sequential)]
            struct ConstantBuffer
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

            public enum PropertyIndex
            {
                EdgeGamma,
                Purkinje,
                Darkness,
                Threshold,
                NoiseLevel,
                Seed,
            }
        }
    }
}

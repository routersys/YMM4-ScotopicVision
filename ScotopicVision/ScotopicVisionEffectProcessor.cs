using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;
using static ScotopicVision.ParameterNormalizer;

namespace ScotopicVision
{
    internal sealed class ScotopicVisionEffectProcessor(IGraphicsDevicesAndContext devices, ScotopicVisionEffect item) : VideoEffectProcessorBase(devices)
    {
        readonly ScotopicVisionEffect item = item;
        GaussianBlur? blurNear;
        GaussianBlur? blurFar;
        ScotopicVisionCustomEffect? effect;
        Parameters parameters;
        bool isFirst = true;

        public override DrawDescription Update(EffectDescription effectDescription)
        {
            if (IsPassThroughEffect || effect is null || blurNear is null || blurFar is null)
                return effectDescription.DrawDescription;

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;
            var next = new Parameters(
                Finite(item.Acuity.GetValue(frame, length, fps), 0f, 64f, 2.5f),
                Finite(item.EdgeCrispness.GetValue(frame, length, fps), 1f, 3f, 1.25f),
                Percent(item.Purkinje.GetValue(frame, length, fps), 0f, 2f, 1f),
                Percent(item.Darkness.GetValue(frame, length, fps), 0f, 1f, 0.7f),
                Percent(item.Threshold.GetValue(frame, length, fps), 0f, 1f, 0.7f),
                Percent(item.Noise.GetValue(frame, length, fps), 0f, 1f, 0.3f) * 0.05f,
                Math.Clamp(item.Seed, 0, 9999));

            if (isFirst || parameters.Sigma != next.Sigma)
            {
                blurNear.StandardDeviation = next.Sigma;
                blurFar.StandardDeviation = next.Sigma * 1.6f;
            }
            if (isFirst || parameters.EdgeGamma != next.EdgeGamma)
                effect.EdgeGamma = next.EdgeGamma;
            if (isFirst || parameters.Purkinje != next.Purkinje)
                effect.Purkinje = next.Purkinje;
            if (isFirst || parameters.Darkness != next.Darkness)
                effect.Darkness = next.Darkness;
            if (isFirst || parameters.Threshold != next.Threshold)
                effect.Threshold = next.Threshold;
            if (isFirst || parameters.NoiseLevel != next.NoiseLevel)
                effect.NoiseLevel = next.NoiseLevel;
            if (isFirst || parameters.Seed != next.Seed)
                effect.Seed = next.Seed;

            parameters = next;
            isFirst = false;

            return effectDescription.DrawDescription;
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            blurNear = new GaussianBlur(devices.DeviceContext)
            {
                Optimization = GaussianBlurOptimization.Quality,
                BorderMode = BorderMode.Hard,
            };
            blurFar = new GaussianBlur(devices.DeviceContext)
            {
                Optimization = GaussianBlurOptimization.Quality,
                BorderMode = BorderMode.Hard,
            };
            effect = new ScotopicVisionCustomEffect(devices);
            if (!effect.IsEnabled)
            {
                blurNear.Dispose();
                blurFar.Dispose();
                effect.Dispose();
                blurNear = null;
                blurFar = null;
                effect = null;
                return null;
            }

            disposer.Collect(blurNear);
            disposer.Collect(blurFar);
            disposer.Collect(effect);

            using (var output = blurNear.Output)
                effect.SetInput(1, output, true);
            using (var output = blurFar.Output)
                effect.SetInput(2, output, true);

            var result = effect.Output;
            disposer.Collect(result);
            return result;
        }

        protected override void setInput(ID2D1Image? input)
        {
            blurNear?.SetInput(0, input, true);
            blurFar?.SetInput(0, input, true);
            effect?.SetInput(0, input, true);
        }

        protected override void ClearEffectChain()
        {
            blurNear?.SetInput(0, null, true);
            blurFar?.SetInput(0, null, true);
            effect?.SetInput(0, null, true);
            effect?.SetInput(1, null, true);
            effect?.SetInput(2, null, true);
            isFirst = true;
        }

        readonly record struct Parameters(
            float Sigma,
            float EdgeGamma,
            float Purkinje,
            float Darkness,
            float Threshold,
            float NoiseLevel,
            int Seed);
    }
}

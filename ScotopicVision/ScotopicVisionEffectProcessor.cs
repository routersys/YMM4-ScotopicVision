using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;

namespace ScotopicVision
{
    internal sealed class ScotopicVisionEffectProcessor(IGraphicsDevicesAndContext devices, ScotopicVisionEffect item) : VideoEffectProcessorBase(devices)
    {
        private readonly ScotopicVisionEffect _item = item;
        private GaussianBlur? _blurNear;
        private GaussianBlur? _blurFar;
        private ScotopicVisionCustomEffect? _effect;

        private bool _isFirst = true;
        private Parameters _parameters;

        public override DrawDescription Update(EffectDescription effectDescription)
        {
            if (IsPassThroughEffect || _effect is null || _blurNear is null || _blurFar is null)
                return effectDescription.DrawDescription;

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var parameters = new Parameters(
                (float)_item.Acuity.GetValue(frame, length, fps),
                (float)_item.EdgeCrispness.GetValue(frame, length, fps),
                (float)(_item.Purkinje.GetValue(frame, length, fps) / 100.0),
                (float)(_item.Darkness.GetValue(frame, length, fps) / 100.0),
                (float)(_item.Threshold.GetValue(frame, length, fps) / 100.0),
                (float)(_item.Noise.GetValue(frame, length, fps) / 100.0 * 0.05),
                _item.Seed);

            if (_isFirst || _parameters.Sigma != parameters.Sigma)
            {
                _blurNear.StandardDeviation = parameters.Sigma;
                _blurFar.StandardDeviation = parameters.Sigma * 1.6f;
            }
            if (_isFirst || _parameters.EdgeGamma != parameters.EdgeGamma)
                _effect.EdgeGamma = parameters.EdgeGamma;
            if (_isFirst || _parameters.Purkinje != parameters.Purkinje)
                _effect.Purkinje = parameters.Purkinje;
            if (_isFirst || _parameters.Darkness != parameters.Darkness)
                _effect.Darkness = parameters.Darkness;
            if (_isFirst || _parameters.Threshold != parameters.Threshold)
                _effect.Threshold = parameters.Threshold;
            if (_isFirst || _parameters.NoiseLevel != parameters.NoiseLevel)
                _effect.NoiseLevel = parameters.NoiseLevel;
            if (_isFirst || _parameters.Seed != parameters.Seed)
                _effect.Seed = parameters.Seed;

            _parameters = parameters;
            _isFirst = false;

            return effectDescription.DrawDescription;
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            _blurNear = new GaussianBlur(devices.DeviceContext)
            {
                Optimization = GaussianBlurOptimization.Quality,
                BorderMode = BorderMode.Hard,
            };
            _blurFar = new GaussianBlur(devices.DeviceContext)
            {
                Optimization = GaussianBlurOptimization.Quality,
                BorderMode = BorderMode.Hard,
            };
            _effect = new ScotopicVisionCustomEffect(devices);
            if (!_effect.IsEnabled)
            {
                _blurNear.Dispose();
                _blurFar.Dispose();
                _effect.Dispose();
                _blurNear = null;
                _blurFar = null;
                _effect = null;
                return null;
            }

            disposer.Collect(_blurNear);
            disposer.Collect(_blurFar);
            disposer.Collect(_effect);

            using (var output = _blurNear.Output)
                _effect.SetInput(1, output, true);
            using (var output = _blurFar.Output)
                _effect.SetInput(2, output, true);

            var result = _effect.Output;
            disposer.Collect(result);
            return result;
        }

        protected override void setInput(ID2D1Image? input)
        {
            _blurNear?.SetInput(0, input, true);
            _blurFar?.SetInput(0, input, true);
            _effect?.SetInput(0, input, true);
        }

        protected override void ClearEffectChain()
        {
            _blurNear?.SetInput(0, null, true);
            _blurFar?.SetInput(0, null, true);
            _effect?.SetInput(0, null, true);
            _effect?.SetInput(1, null, true);
            _effect?.SetInput(2, null, true);
            _isFirst = true;
        }

        private readonly record struct Parameters(
            float Sigma,
            float EdgeGamma,
            float Purkinje,
            float Darkness,
            float Threshold,
            float NoiseLevel,
            int Seed);
    }
}

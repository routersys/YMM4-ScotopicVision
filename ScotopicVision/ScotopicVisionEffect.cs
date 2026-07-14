using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace ScotopicVision
{
    [PluginDetails(AuthorName = "routersys")]
    [VideoEffect(nameof(Texts.ScotopicVisionEffectName), [VideoEffectCategories.Filtering], [nameof(Texts.TagScotopic), nameof(Texts.TagNightVision), nameof(Texts.TagPurkinje)], IsAviUtlSupported = false, ResourceType = typeof(Texts))]
    public sealed class ScotopicVisionEffect : VideoEffectBase
    {
        public override string Label => Texts.ScotopicVisionEffectName;

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionDarkness), Description = nameof(Texts.ScotopicVisionDarknessDescription), Order = 0, ResourceType = typeof(Texts))]
        [AnimationSlider("F1", "%", 0d, 100d)]
        public Animation Darkness { get; } = new(70, 0, 100);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionThreshold), Description = nameof(Texts.ScotopicVisionThresholdDescription), Order = 10, ResourceType = typeof(Texts))]
        [AnimationSlider("F1", "%", 0d, 100d)]
        public Animation Threshold { get; } = new(70, 0, 100);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionAcuity), Description = nameof(Texts.ScotopicVisionAcuityDescription), Order = 20, ResourceType = typeof(Texts))]
        [AnimationSlider("F1", "px", 0d, 20d)]
        public Animation Acuity { get; } = new(2.5, 0, 64);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionEdgeCrispness), Description = nameof(Texts.ScotopicVisionEdgeCrispnessDescription), Order = 30, ResourceType = typeof(Texts))]
        [AnimationSlider("F2", "", 1d, 3d)]
        public Animation EdgeCrispness { get; } = new(1.25, 1, 3);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionPurkinje), Description = nameof(Texts.ScotopicVisionPurkinjeDescription), Order = 40, ResourceType = typeof(Texts))]
        [AnimationSlider("F1", "%", 0d, 200d)]
        public Animation Purkinje { get; } = new(100, 0, 200);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionNoise), Description = nameof(Texts.ScotopicVisionNoiseDescription), Order = 50, ResourceType = typeof(Texts))]
        [AnimationSlider("F1", "%", 0d, 100d)]
        public Animation Noise { get; } = new(30, 0, 100);

        [Display(GroupName = nameof(Texts.ScotopicVisionEffectName), Name = nameof(Texts.ScotopicVisionSeed), Description = nameof(Texts.ScotopicVisionSeedDescription), Order = 60, ResourceType = typeof(Texts))]
        [Range(0, 9999)]
        [DefaultValue(0)]
        [TextBoxSlider("F0", "", 0, 9999)]
        public int Seed { get => _seed; set => Set(ref _seed, Math.Clamp(value, 0, 9999)); }
        int _seed;

        IAnimatable[]? _animatables;

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices) => new ScotopicVisionEffectProcessor(devices, this);

        protected override IEnumerable<IAnimatable> GetAnimatables() => _animatables ??= [Darkness, Threshold, Acuity, EdgeCrispness, Purkinje, Noise];
    }
}

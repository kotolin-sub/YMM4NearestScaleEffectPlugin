using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace NearestScaleEffectPlugin
{
    [VideoEffect("ニアレストネイバー拡大", ["描画"], [],isAviUtlSupported:false)]
    internal class NearestScaleEffect : VideoEffectBase
    {
        public override string Label => "ニアレストネイバー拡大";

        [Display(Name = "拡大率", Description = "画像の拡大率")]
        [AnimationSlider("F2", "%", 100, 1000)]
        public Animation Scale { get; } = new Animation(100, 100, 10000);

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            var fps = exoOutputDescription.VideoInfo.FPS;
            return
            [
                $"ないです"
            ];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new NearestScaleProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [Scale];
    }

    internal class NearestScaleProcessor : IVideoEffectProcessor
    {
        readonly NearestScaleEffect fx;
        readonly ID2D1Effect scaleEffect;
        bool isFirst = true;
        float scale;

        public ID2D1Image Output { get; }

        public NearestScaleProcessor(IGraphicsDevicesAndContext devices, NearestScaleEffect fx)
        {
            this.fx = fx;
            scaleEffect = (ID2D1Effect)devices.DeviceContext.CreateEffect(EffectGuids.Scale);
            scaleEffect.SetValue((int)ScaleProperties.InterpolationMode, ScaleInterpolationMode.NearestNeighbor);
            Output = scaleEffect.Output;
        }

        public void SetInput(ID2D1Image? input)
        {
            scaleEffect.SetInput(0, input, true);
        }

        public void ClearInput()
        {
            scaleEffect.SetInput(0, null, true);
        }

        public DrawDescription Update(EffectDescription desc)
        {
            var frame = desc.ItemPosition.Frame;
            var length = desc.ItemDuration.Frame;
            var fps = desc.FPS;

            var s = fx.Scale.GetValue(frame, length, fps) / 100f;

            if (isFirst || scale != s)
            {
                scaleEffect.SetValue((int)ScaleProperties.Scale, new Vector2((float)s));
                scale = (float)s;
            }

            isFirst = false;
            return desc.DrawDescription;
        }

        public void Dispose()
        {
            scaleEffect.SetInput(0, null, true);
            Output.Dispose();
            scaleEffect.Dispose();
        }
    }
}
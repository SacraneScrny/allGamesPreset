using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sackrany.Actor.DefaultFeatures.VolumeFeature.Entities
{
    public struct TonemappingVar : 
        ICustomVariable<TonemappingVar>, 
        IVolumeVariable<Tonemapping>
    {
        public TonemappingMode tonemappingMode;
        
        public TonemappingVar(Tonemapping tonemapping)
        {
            tonemappingMode = tonemapping.mode.value;
        }
        
        public void Add(TonemappingVar variable)
        {
            tonemappingMode = variable.tonemappingMode;
        }
        public void Multiply(TonemappingVar variable)
        {
            
        }
        
        public void Apply(Tonemapping component)
        {
            
        }
    }
    
    public struct VignetteVar : 
        ICustomVariable<VignetteVar>, 
        IVolumeVariable<Vignette>
    {
        public Color color;
        public Vector2 center;
        public float intensity;
        public float smoothness;
        public bool rounded;
        
        public VignetteVar(Vignette vignette)
        {
            color = vignette.color.value;
            center = vignette.center.value;
            intensity = vignette.intensity.value;
            smoothness = vignette.smoothness.value;
            rounded = vignette.rounded.value;
        }
        
        public void Add(VignetteVar variable)
        {
            color += variable.color;
            center += variable.center;
            intensity += variable.intensity;
            smoothness += variable.smoothness;
            rounded |= variable.rounded;
        }
        public void Multiply(VignetteVar variable)
        {
            color *= variable.color;
            center *= variable.center;
            intensity *= variable.intensity;
            smoothness *= variable.smoothness;
        }
        
        public void Apply(Vignette component)
        {
            component.color.value = color;
            component.center.value = center;
            component.intensity.value = intensity;
            component.smoothness.value = smoothness;
            component.rounded.value = rounded;
        }
    }
    
    public struct BloomVar :
        ICustomVariable<BloomVar>,
        IVolumeVariable<Bloom>
    {
        public float intensity;
        public float threshold;

        public BloomVar(Bloom bloom)
        {
            intensity = bloom.intensity.value;
            threshold = bloom.threshold.value;
        }

        public void Add(BloomVar v)
        {
            intensity += v.intensity;
            threshold += v.threshold;
        }

        public void Multiply(BloomVar v)
        {
            intensity *= v.intensity;
            threshold *= v.threshold;
        }

        public void Apply(Bloom c)
        {
            c.intensity.value = intensity;
            c.threshold.value = threshold;
        }
    }
    
    public struct MotionBlurVar :
        ICustomVariable<MotionBlurVar>,
        IVolumeVariable<MotionBlur>
    {
        public float intensity;

        public MotionBlurVar(MotionBlur mb)
        {
            intensity = mb.intensity.value;
        }

        public void Add(MotionBlurVar v) => intensity += v.intensity;
        public void Multiply(MotionBlurVar v) => intensity *= v.intensity;

        public void Apply(MotionBlur c) => c.intensity.value = intensity;
    }

    public struct ChannelMixerVar :
        ICustomVariable<ChannelMixerVar>,
        IVolumeVariable<ChannelMixer>
    {
        public float red;
        public float green;
        public float blue;

        public ChannelMixerVar(ChannelMixer c)
        {
            red = c.redOutRedIn.value;
            green = c.greenOutGreenIn.value;
            blue = c.blueOutBlueIn.value;
        }

        public void Add(ChannelMixerVar v)
        {
            red += v.red;
            green += v.green;
            blue += v.blue;
        }

        public void Multiply(ChannelMixerVar v)
        {
            red *= v.red;
            green *= v.green;
            blue *= v.blue;
        }

        public void Apply(ChannelMixer c)
        {
            c.redOutRedIn.value = red;
            c.greenOutGreenIn.value = green;
            c.blueOutBlueIn.value = blue;
        }
    }

    public struct ChromaticAberrationVar :
        ICustomVariable<ChromaticAberrationVar>,
        IVolumeVariable<ChromaticAberration>
    {
        public float intensity;

        public ChromaticAberrationVar(ChromaticAberration c)
        {
            intensity = c.intensity.value;
        }

        public void Add(ChromaticAberrationVar v) => intensity += v.intensity;
        public void Multiply(ChromaticAberrationVar v) => intensity *= v.intensity;
        public void Apply(ChromaticAberration c) => c.intensity.value = intensity;
    }

    public struct ColorAdjustmentsVar :
        ICustomVariable<ColorAdjustmentsVar>,
        IVolumeVariable<ColorAdjustments>
    {
        public float postExposure;
        public float contrast;
        public Color colorFilter;

        public ColorAdjustmentsVar(ColorAdjustments c)
        {
            postExposure = c.postExposure.value;
            contrast = c.contrast.value;
            colorFilter = c.colorFilter.value;
        }

        public void Add(ColorAdjustmentsVar v)
        {
            postExposure += v.postExposure;
            contrast += v.contrast;
            colorFilter += v.colorFilter;
        }

        public void Multiply(ColorAdjustmentsVar v)
        {
            postExposure *= v.postExposure;
            contrast *= v.contrast;
            colorFilter *= v.colorFilter;
        }

        public void Apply(ColorAdjustments c)
        {
            c.postExposure.value = postExposure;
            c.contrast.value = contrast;
            c.colorFilter.value = colorFilter;
        }
    }

    public struct DepthOfFieldVar :
        ICustomVariable<DepthOfFieldVar>,
        IVolumeVariable<DepthOfField>
    {
        public float focusDistance;
        public float aperture;

        public DepthOfFieldVar(DepthOfField d)
        {
            focusDistance = d.focusDistance.value;
            aperture = d.aperture.value;
        }

        public void Add(DepthOfFieldVar v)
        {
            focusDistance += v.focusDistance;
            aperture += v.aperture;
        }

        public void Multiply(DepthOfFieldVar v)
        {
            focusDistance *= v.focusDistance;
            aperture *= v.aperture;
        }

        public void Apply(DepthOfField c)
        {
            c.focusDistance.value = focusDistance;
            c.aperture.value = aperture;
        }
    }

    public struct WhiteBalanceVar :
        ICustomVariable<WhiteBalanceVar>,
        IVolumeVariable<WhiteBalance>
    {
        public float temperature;
        public float tint;

        public WhiteBalanceVar(WhiteBalance w)
        {
            temperature = w.temperature.value;
            tint = w.tint.value;
        }

        public void Add(WhiteBalanceVar v)
        {
            temperature += v.temperature;
            tint += v.tint;
        }

        public void Multiply(WhiteBalanceVar v)
        {
            temperature *= v.temperature;
            tint *= v.tint;
        }

        public void Apply(WhiteBalance c)
        {
            c.temperature.value = temperature;
            c.tint.value = tint;
        }
    }
    
    public struct ColorCurvesVar :
        ICustomVariable<ColorCurvesVar>,
        IVolumeVariable<ColorCurves>
    {
        public TextureCurve master;
        public TextureCurve red;
        public TextureCurve green;
        public TextureCurve blue;

        public ColorCurvesVar(ColorCurves c)
        {
            master = c.master.value;
            red = c.red.value;
            green = c.green.value;
            blue = c.blue.value;
        }

        public void Add(ColorCurvesVar v) { }
        public void Multiply(ColorCurvesVar v) { }

        public void Apply(ColorCurves c)
        {
            c.master.value = master;
            c.red.value = red;
            c.green.value = green;
            c.blue.value = blue;
        }
    }

    public struct ColorLookupVar :
        ICustomVariable<ColorLookupVar>,
        IVolumeVariable<ColorLookup>
    {
        public Texture texture;
        public float contribution;

        public ColorLookupVar(ColorLookup c)
        {
            texture = c.texture.value;
            contribution = c.contribution.value;
        }

        public void Add(ColorLookupVar v) => contribution += v.contribution;
        public void Multiply(ColorLookupVar v) => contribution *= v.contribution;

        public void Apply(ColorLookup c)
        {
            c.texture.value = texture;
            c.contribution.value = contribution;
        }
    }

    public struct FilmGrainVar :
        ICustomVariable<FilmGrainVar>,
        IVolumeVariable<FilmGrain>
    {
        public float intensity;
        public float response;

        public FilmGrainVar(FilmGrain f)
        {
            intensity = f.intensity.value;
            response = f.response.value;
        }

        public void Add(FilmGrainVar v)
        {
            intensity += v.intensity;
            response += v.response;
        }

        public void Multiply(FilmGrainVar v)
        {
            intensity *= v.intensity;
            response *= v.response;
        }

        public void Apply(FilmGrain c)
        {
            c.intensity.value = intensity;
            c.response.value = response;
        }
    }

    public struct LensDistortionVar :
        ICustomVariable<LensDistortionVar>,
        IVolumeVariable<LensDistortion>
    {
        public float intensity;
        public float scale;

        public LensDistortionVar(LensDistortion l)
        {
            intensity = l.intensity.value;
            scale = l.scale.value;
        }

        public void Add(LensDistortionVar v)
        {
            intensity += v.intensity;
            scale += v.scale;
        }

        public void Multiply(LensDistortionVar v)
        {
            intensity *= v.intensity;
            scale *= v.scale;
        }

        public void Apply(LensDistortion c)
        {
            c.intensity.value = intensity;
            c.scale.value = scale;
        }
    }

    public struct LiftGammaGainVar :
        ICustomVariable<LiftGammaGainVar>,
        IVolumeVariable<LiftGammaGain>
    {
        public Vector4 lift;
        public Vector4 gamma;
        public Vector4 gain;

        public LiftGammaGainVar(LiftGammaGain l)
        {
            lift = l.lift.value;
            gamma = l.gamma.value;
            gain = l.gain.value;
        }

        public void Add(LiftGammaGainVar v)
        {
            lift += v.lift;
            gamma += v.gamma;
            gain += v.gain;
        }

        public void Multiply(LiftGammaGainVar v)
        {
            lift *= v.lift.x;
            gamma *= v.gamma.x;
            gain *= v.gain.x;
        }

        public void Apply(LiftGammaGain c)
        {
            c.lift.value = lift;
            c.gamma.value = gamma;
            c.gain.value = gain;
        }
    }

    public struct PaniniProjectionVar :
        ICustomVariable<PaniniProjectionVar>,
        IVolumeVariable<PaniniProjection>
    {
        public float distance;
        public float cropToFit;

        public PaniniProjectionVar(PaniniProjection p)
        {
            distance = p.distance.value;
            cropToFit = p.cropToFit.value;
        }

        public void Add(PaniniProjectionVar v)
        {
            distance += v.distance;
            cropToFit += v.cropToFit;
        }

        public void Multiply(PaniniProjectionVar v)
        {
            distance *= v.distance;
            cropToFit *= v.cropToFit;
        }

        public void Apply(PaniniProjection c)
        {
            c.distance.value = distance;
            c.cropToFit.value = cropToFit;
        }
    }

    public struct ScreenSpaceLensFlareVar :
        ICustomVariable<ScreenSpaceLensFlareVar>,
        IVolumeVariable<ScreenSpaceLensFlare>
    {
        public float intensity;

        public ScreenSpaceLensFlareVar(ScreenSpaceLensFlare s)
        {
            intensity = s.intensity.value;
        }

        public void Add(ScreenSpaceLensFlareVar v) => intensity += v.intensity;
        public void Multiply(ScreenSpaceLensFlareVar v) => intensity *= v.intensity;

        public void Apply(ScreenSpaceLensFlare c)
        {
            c.intensity.value = intensity;
        }
    }

    public struct ShadowsMidtonesHighlightsVar :
        ICustomVariable<ShadowsMidtonesHighlightsVar>,
        IVolumeVariable<ShadowsMidtonesHighlights>
    {
        public Vector4 shadows;
        public Vector4 midtones;
        public Vector4 highlights;

        public ShadowsMidtonesHighlightsVar(ShadowsMidtonesHighlights s)
        {
            shadows = s.shadows.value;
            midtones = s.midtones.value;
            highlights = s.highlights.value;
        }

        public void Add(ShadowsMidtonesHighlightsVar v)
        {
            shadows += v.shadows;
            midtones += v.midtones;
            highlights += v.highlights;
        }

        public void Multiply(ShadowsMidtonesHighlightsVar v)
        {
            shadows *= v.shadows.x;
            midtones *= v.midtones.x;
            highlights *= v.highlights.x;
        }

        public void Apply(ShadowsMidtonesHighlights c)
        {
            c.shadows.value = shadows;
            c.midtones.value = midtones;
            c.highlights.value = highlights;
        }
    }

    public struct SplitToningVar :
        ICustomVariable<SplitToningVar>,
        IVolumeVariable<SplitToning>
    {
        public Color shadows;
        public Color highlights;
        public float balance;

        public SplitToningVar(SplitToning s)
        {
            shadows = s.shadows.value;
            highlights = s.highlights.value;
            balance = s.balance.value;
        }

        public void Add(SplitToningVar v)
        {
            shadows += v.shadows;
            highlights += v.highlights;
            balance += v.balance;
        }

        public void Multiply(SplitToningVar v)
        {
            shadows *= v.shadows;
            highlights *= v.highlights;
            balance *= v.balance;
        }

        public void Apply(SplitToning c)
        {
            c.shadows.value = shadows;
            c.highlights.value = highlights;
            c.balance.value = balance;
        }
    }

    public interface IVolumeVariable<in T>
        where T : VolumeComponent
    {
        public void Apply(T component);
    }
}
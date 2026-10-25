using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public sealed class ScreenSpaceGlobalIlluminationVolume
{
	public enum DenoiserAlgorithm
	{
		[Tooltip("Produces results with more detail, but may retain some noise.")]
		Conservative,
		[Tooltip("Produces cleaner results.")]
		Aggressive
	}

	[Serializable]
	public sealed class DenoiserAlgorithmParameter : VolumeParameter<DenoiserAlgorithm>
	{
		public DenoiserAlgorithmParameter(DenoiserAlgorithm value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}

	public enum ThicknessMode
	{
		[InspectorName("Constant")]
		[Tooltip("Apply constant thickness to every scene object.")]
		Constant,
		[InspectorName("Automatic")]
		[Tooltip("Render the back-faces of scene objects to compute thickness.")]
		ComputeBackface
	}

	[Serializable]
	public sealed class ThicknessParameter : VolumeParameter<ThicknessMode>
	{
		public ThicknessParameter(ThicknessMode value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}

	public enum QualityMode
	{
		[Tooltip("When selected, choices are made to reduce execution time of the effect.")]
		Low,
		[Tooltip("When selected, choices are made to increase the visual quality of the effect.")]
		Medium,
		[Tooltip("When selected, choices are made to increase the visual quality of the effect.")]
		High,
		[Tooltip("When selected, choices are made to increase the visual quality of the effect.")]
		Custom
	}

	[Serializable]
	public sealed class RayMarchingModeParameter : VolumeParameter<QualityMode>
	{
		public RayMarchingModeParameter(QualityMode value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}

	public enum RayMarchingFallbackHierarchy
	{
		[InspectorName("Nothing")]
		[Tooltip("When selected, ray marching will return a black color.")]
		None,
		[InspectorName("Sky")]
		[Tooltip("When selected, ray marching will fall back on the sky.")]
		Sky,
		[InspectorName("Reflection Probes")]
		[Tooltip("When selected, ray marching will fall back on reflection probes (if any).")]
		ReflectionProbes,
		[InspectorName("Reflection Probes and Sky")]
		[Tooltip("When selected, ray marching will fall back on reflection probes (if any) then on the sky.")]
		ReflectionProbesAndSky
	}

	[Serializable]
	public sealed class RayMarchingFallbackHierarchyParameter : VolumeParameter<RayMarchingFallbackHierarchy>
	{
		public RayMarchingFallbackHierarchyParameter(RayMarchingFallbackHierarchy value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}

	[Tooltip("Enable screen space global illumination.")]
	private static bool? m_enable = false;

	[Tooltip("The thickness mode of screen space global illumination.")]
	public ThicknessMode thicknessMode;

	[Tooltip("Controls the thickness (or fallback thickness) of the depth buffer used for ray marching.")]
	[Range(0f, 0.5f)]
	public float depthBufferThickness = 0.1f;

	[Tooltip("Specifies the quality of screen space global illumination.")]
	public QualityMode quality;

	[InspectorName("Full Resolution")]
	[Tooltip("Controls if the screen space global illumination should be evaluated at full resolution.")]
	public bool fullResolutionSS;

	[InspectorName("Resolution Scale")]
	[Tooltip("Controls the resolution used to evaluate screen space global illumination.")]
	[Range(0.25f, 0.75f)]
	public float resolutionScaleSS = 0.5f;

	[Tooltip("Controls the number of samples for global illumination.")]
	[Range(1f, 16f)]
	public int sampleCount = 2;

	[Tooltip("Controls the number of steps used for ray marching.")]
	[Range(16f, 128f)]
	public int maxRaySteps = 32;

	[InspectorName("Denoise")]
	[Tooltip("Controls if the screen space global illumination should be denoised.")]
	public bool denoiseSS = true;

	[InspectorName("Algorithm")]
	[Tooltip("Controls the denoising mode for screen space global illumination.")]
	public DenoiserAlgorithm denoiserAlgorithmSS = DenoiserAlgorithm.Aggressive;

	[InspectorName("Intensity")]
	[Tooltip("Controls the intensity of temporal denoising pass.")]
	[Range(0.5f, 0.95f)]
	public float denoiseIntensitySS = 0.95f;

	[InspectorName("Denoiser Radius")]
	[Tooltip("Controls the radius of the GI denoiser (First Pass).")]
	[Range(0.001f, 1f)]
	public float denoiserRadiusSS = 0.6f;

	[InspectorName("Second Denoiser Pass")]
	[Tooltip("Enable second denoising pass.")]
	public bool secondDenoiserPassSS = true;

	[Tooltip("Controls the fallback hierarchy for indirect diffuse in case the ray misses.")]
	public RayMarchingFallbackHierarchy rayMiss = RayMarchingFallbackHierarchy.ReflectionProbesAndSky;

	[Header("Artistic Overrides")]
	[InspectorName("Indirect Diffuse Lighting Multiplier")]
	[Tooltip("Controls the indirect diffuse lighting from screen space global illumination.")]
	[Min(0f)]
	public float indirectDiffuseLightingMultiplier = 1f;

	public static bool enable
	{
		get
		{
			bool? flag = m_enable;
			if (!flag.HasValue)
			{
				bool? flag2 = (m_enable = PlayerPrefs.GetInt("krSSGIEnabled", 1) == 1);
				return flag2.Value;
			}
			return flag == true;
		}
		set
		{
			m_enable = value;
			PlayerPrefs.SetInt("krSSGIEnabled", enable ? 1 : 0);
		}
	}

	public bool IsTileCompatible()
	{
		return false;
	}

	public bool IsActive()
	{
		return enable;
	}

	public bool IsFallbackSky()
	{
		return (rayMiss & RayMarchingFallbackHierarchy.Sky) != 0;
	}

	public bool IsFallbackReflectionProbes()
	{
		return (rayMiss & RayMarchingFallbackHierarchy.ReflectionProbes) != 0;
	}

	private void ApplyCurrentQualityMode()
	{
		switch (quality)
		{
		case QualityMode.Low:
			sampleCount = 1;
			maxRaySteps = 24;
			break;
		case QualityMode.Medium:
			sampleCount = 2;
			maxRaySteps = 32;
			break;
		case QualityMode.High:
			sampleCount = 4;
			maxRaySteps = 64;
			break;
		}
	}
}

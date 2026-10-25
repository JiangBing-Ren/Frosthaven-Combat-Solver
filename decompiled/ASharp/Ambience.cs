using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using FH.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;

[ExecuteAlways]
public class Ambience : MonoBehaviour
{
	private class AmbientColors
	{
		public Color colorSky;

		public Color colorEquator;

		public Color colorGround;
	}

	private Volume[] volumes;

	private Ambience staticAmbience;

	private Dictionary<Volume, float> startingVolumeWeights;

	private AmbientColors startingAmbientColors;

	private Color startingBgMistTint = Color.white;

	private Color startingWallFadeEffect = Color.black;

	[Header("Environment")]
	public bool applyInEditor;

	[Tooltip("Scene skybox material")]
	public Material skyboxMaterial;

	[Tooltip("Override Reflections (cubemap)")]
	public Cubemap reflectionCubemapOverride;

	[SerializeField]
	private GameObject _charactersReflectionProbePrefab;

	[NonSerialized]
	public RenderingLayer currentRenderingLayer = new RenderingLayer(0);

	[Tooltip("Use the skybox for ambient lighting colours?")]
	private readonly AmbientMode skyboxAmbientLight = AmbientMode.Trilight;

	private static readonly int bgMistProperty = Shader.PropertyToID("_Global_BG_Mist_Tint");

	private static readonly int wallFadeEffectProperty = Shader.PropertyToID("_Global_Wall_Fade_Effect_Tint");

	public static readonly int characterLightMainProperty = Shader.PropertyToID("_Character_Light_Main");

	public static readonly int characterLightFillProperty = Shader.PropertyToID("_Character_Light_Fill");

	public static readonly int characterLightRimProperty = Shader.PropertyToID("_Character_Light_Rim");

	public static readonly int characterLightMainSpecProperty = Shader.PropertyToID("_Character_Light_Main_Spec");

	public static readonly int characterLightFillSpecProperty = Shader.PropertyToID("_Character_Light_Fill_Spec");

	public static readonly int characterLightRimSpecProperty = Shader.PropertyToID("_Character_Light_Rim_Spec");

	[ColorUsage(false, true)]
	[SerializeField]
	private Color skyColor = Color.magenta;

	[ColorUsage(false, true)]
	[SerializeField]
	private Color equatorColor = Color.magenta;

	[ColorUsage(false, true)]
	[SerializeField]
	private Color groundColor = Color.magenta;

	[ColorUsage(false, true)]
	[SerializeField]
	private Color backgroundMistTint = Color.white;

	[ColorUsage(false, true)]
	[SerializeField]
	private Color wallFadeEffect = Color.black;

	[Header("Character Lights")]
	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightMain = new Color(0.276f, 0.276f, 0.276f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightFill = new Color(0.13803f, 0.13803f, 0.13803f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightRim = new Color(0.13803f, 0.13803f, 0.13803f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightMainSpec = new Color(0.4f, 0.4f, 0.4f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightFillSpec = new Color(0.4f, 0.4f, 0.4f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color characterLightRimSpec = new Color(0.4f, 0.4f, 0.4f, 1f);

	private GameObject _charactersReflectionProbeInstance;

	private ProceduralMapTile maptileParent;

	public Transform CharactersReflectionProbe
	{
		get
		{
			if (!_charactersReflectionProbeInstance)
			{
				if (!_charactersReflectionProbePrefab)
				{
					return null;
				}
				return (_charactersReflectionProbeInstance = UnityEngine.Object.Instantiate(_charactersReflectionProbePrefab, transform)).transform;
			}
			return _charactersReflectionProbeInstance.transform;
		}
	}

	public ProceduralMapTile.Visibility Visibility
	{
		get
		{
			if (!maptileParent)
			{
				return ProceduralMapTile.Visibility.Revealed;
			}
			return maptileParent.GetVisibility();
		}
	}

	private void OnEnable()
	{
		ProceduralScenario proceduralScenario = null;
		if (EnvironmentLoader.IsInstanceValid && EnvironmentLoader.Instance.EnvironmentRoot != null)
		{
			EnvironmentLoader.Instance.EnvironmentRoot.GetComponent<ProceduralScenario>();
		}
		else
		{
			proceduralScenario = UnityEngine.Object.FindObjectOfType<ProceduralScenario>(includeInactive: false);
		}
		if (!proceduralScenario || !proceduralScenario.isActiveAndEnabled || !MultiplaneSystem.GetInstance())
		{
			KronosFogOfWar.SetFowMaskActive(enabled: false);
		}
	}

	public static bool Same(Ambience l, Ambience r)
	{
		if (!(l == r))
		{
			return l.name == r.name;
		}
		return true;
	}

	internal void Init(AmbienceDiscriminator.Differentiation ambienceDifferentiation, ProceduralMapTile maptileParent)
	{
		currentRenderingLayer = ambienceDifferentiation.Layer;
		Light[] componentsInChildren = GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren)
		{
			light.SetLayer(ambienceDifferentiation.Layer);
			if (ambienceDifferentiation.DisableAmbience)
			{
				light.enabled = false;
			}
		}
		this.maptileParent = maptileParent;
	}

	public void ApplyBlend(float target, float blend)
	{
		LazyInitVol();
		blend = Easing.OutCubic(blend);
		_ = gameObject.scene;
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		Volume[] array = volumes;
		foreach (Volume volume in array)
		{
			volume.weight = Mathf.Lerp(startingVolumeWeights[volume], target, blend);
		}
		if (target > 0.5f)
		{
			if (startingAmbientColors != null)
			{
				RenderSettings.skybox = skyboxMaterial;
				RenderSettings.ambientMode = skyboxAmbientLight;
				RenderSettings.ambientSkyColor = Color.Lerp(startingAmbientColors.colorSky, staticAmbience.skyColor, blend);
				RenderSettings.ambientEquatorColor = Color.Lerp(startingAmbientColors.colorEquator, staticAmbience.equatorColor, blend);
				RenderSettings.ambientGroundColor = Color.Lerp(startingAmbientColors.colorGround, staticAmbience.groundColor, blend);
			}
			Shader.SetGlobalColor(bgMistProperty, Color.Lerp(startingBgMistTint, staticAmbience.backgroundMistTint, blend));
			Shader.SetGlobalColor(wallFadeEffectProperty, Color.Lerp(startingWallFadeEffect, staticAmbience.wallFadeEffect, blend));
		}
	}

	private void LazyInitVol()
	{
		if (volumes == null || !staticAmbience || volumes.Any((Volume a) => a == null))
		{
			volumes = GetComponentsInChildren<Volume>(includeInactive: true);
			staticAmbience = GetComponent<Ambience>();
		}
		if (startingVolumeWeights == null)
		{
			InitReferenceValues();
		}
	}

	public void OnBeginBlending()
	{
		LazyInitVol();
		InitReferenceValues();
	}

	private void InitReferenceValues()
	{
		startingVolumeWeights = volumes.ToDictionary((Volume a) => a, (Volume a) => a.weight);
		startingBgMistTint = Shader.GetGlobalColor(bgMistProperty);
		startingWallFadeEffect = Shader.GetGlobalColor(wallFadeEffectProperty);
		startingAmbientColors = new AmbientColors
		{
			colorSky = RenderSettings.ambientSkyColor,
			colorEquator = RenderSettings.ambientEquatorColor,
			colorGround = RenderSettings.ambientGroundColor
		};
	}

	public void SetCharacterLightsTo(Material characterMaterial)
	{
		if ((bool)characterMaterial && characterMaterial.HasProperty(characterLightMainProperty))
		{
			characterMaterial.SetColor(characterLightMainProperty, characterLightMain);
			characterMaterial.SetColor(characterLightFillProperty, characterLightFill);
			characterMaterial.SetColor(characterLightRimProperty, characterLightRim);
			characterMaterial.SetColor(characterLightMainSpecProperty, characterLightMainSpec);
			characterMaterial.SetColor(characterLightFillSpecProperty, characterLightFillSpec);
			characterMaterial.SetColor(characterLightRimSpecProperty, characterLightRimSpec);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetReflectionsTo(Transform parent)
	{
		Renderer[] componentsInChildren = parent.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer reflectionsTo in componentsInChildren)
		{
			SetReflectionsTo(reflectionsTo);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetReflectionsTo(Renderer renderer)
	{
		renderer.probeAnchor = CharactersReflectionProbe;
	}

	[ContextMenu("Apply Static Ambience")]
	private void ApplyStaticAmbience()
	{
		ApplyBlend(1f, 1f);
	}

	[ContextMenu("Apply Character Materials")]
	private void ApplyCharacterMaterials()
	{
		foreach (Material item in UnityEngine.Object.FindObjectsOfType<Renderer>().SelectMany((Renderer a) => a.sharedMaterials))
		{
			if ((bool)item)
			{
				SetCharacterLightsTo(item);
			}
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && applyInEditor)
		{
			ApplyStaticAmbience();
		}
	}

	internal RenderingLayer LayerFromVisibility(bool visible)
	{
		if (!visible)
		{
			return AmbienceDiscriminator.UnrevealedLayer;
		}
		return currentRenderingLayer;
	}

	internal bool GetDirectionalLight(out Light directionalLight)
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		if (componentsInChildren.Length < 1)
		{
			directionalLight = null;
			return false;
		}
		directionalLight = componentsInChildren.FirstOrDefault((Light x) => x.type == LightType.Directional);
		return directionalLight != null;
	}

	internal bool GetClosestLight(Vector3 position, out Light closestLight)
	{
		closestLight = null;
		Light[] componentsInChildren = GetComponentsInChildren<Light>();
		if (componentsInChildren.Length < 1)
		{
			if (!EnvironmentLoader.IsInstanceValid)
			{
				return false;
			}
			componentsInChildren = EnvironmentLoader.Instance.EnvironmentRoot.GetComponentsInChildren<Light>();
			if (componentsInChildren.Length < 1)
			{
				return false;
			}
		}
		closestLight = componentsInChildren.OrderBy((Light x) => Vector3.SqrMagnitude(x.transform.position - position)).First();
		return true;
	}
}

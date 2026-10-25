using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using TMPro;
using UnityEngine;

namespace Gloomhaven;

public class GraphicSettings : LocalizedListener
{
	private class GraphicSelector<T> : SelectorWrapper<T> where T : Enum
	{
		public GraphicSelector(TMP_Dropdown dropdown)
			: base(dropdown, (from T it in Enum.GetValues(typeof(T))
				select new SelectorOptData<T>(it, () => LocalizationManager.GetTranslation(string.Format("{0}_{1}", typeof(T).Name.SplitCamelCase().Replace(" ", "_"), it)))).ToList())
		{
		}

		public GraphicSelector(TMP_Dropdown dropdown, IEnumerable<T> validOptions)
			: base(dropdown, validOptions.Select((T it) => new SelectorOptData<T>(it, () => LocalizationManager.GetTranslation(string.Format("{0}_{1}", typeof(T).Name.SplitCamelCase().Replace(" ", "_"), it)))).ToList())
		{
		}
	}

	[SerializeField]
	private ButtonSwitch advancedOptionsToggle;

	[SerializeField]
	private GameObject advancedOptionsContainer;

	[SerializeField]
	private ExtendedDropdown levelSelector;

	[Header("Fields")]
	[SerializeField]
	private ExtendedDropdown anisotropicSelector;

	[SerializeField]
	private ExtendedDropdown antialiasingSelector;

	[SerializeField]
	private TMP_InputField pixelLightInput;

	[SerializeField]
	private ButtonSwitch reflectionProbesCheckbox;

	[SerializeField]
	private ExtendedDropdown shadowResolutionSelector;

	[SerializeField]
	private ExtendedDropdown shadowSelector;

	[SerializeField]
	private ExtendedDropdown skinWeightsSelector;

	[SerializeField]
	private ButtonSwitch softParticlesCheckbox;

	[SerializeField]
	private ExtendedDropdown textureQualitySelector;

	[SerializeField]
	private ExtendedDropdown vsyncSelector;

	[SerializeField]
	private ExtendedDropdown fpsSelector;

	[SerializeField]
	private GameObject fpsOption;

	private string selectedLevel;

	private bool customProfileIsListed;

	private GraphicSelector<AnisotropicFiltering> anisotropicFilteringOpts;

	private GraphicSelector<ShadowQuality> shadowsQualityOpts;

	private GraphicSelector<ShadowResolution> shadowResolutionOpts;

	private GraphicSelector<SkinWeights> skinWeightsOpts;

	private GraphicSelector<TextureQualityRender> textureQualityOpts;

	private GraphicSelector<AntialiasingRender> antialisingOpts;

	private GraphicSelector<VSyncQuality> vSyncOps;

	private SelectorWrapper<string> levelOpts;

	private Dictionary<string, GraphicProfile> unityProfiles;

	private List<int> fpsOptions = new List<int> { -1, 120, 90, 60, 59, 30, 20 };

	private void Awake()
	{
		unityProfiles = new Dictionary<string, GraphicProfile>();
		List<SelectorOptData<string>> values = QualitySettings.names.Select((string it) => new SelectorOptData<string>(it, () => LocalizationManager.GetTranslation("GUI_OPT_GRAPHICS_" + it))).ToList();
		levelOpts = new SelectorWrapper<string>(levelSelector, values);
		levelOpts.OnValuedChanged.AddListener(SetQualityLevel);
		shadowsQualityOpts = new GraphicSelector<ShadowQuality>(shadowSelector);
		shadowsQualityOpts.OnValuedChanged.AddListener(SetShadows);
		anisotropicFilteringOpts = new GraphicSelector<AnisotropicFiltering>(anisotropicSelector);
		anisotropicFilteringOpts.OnValuedChanged.AddListener(SetAnisotropicTexture);
		shadowResolutionOpts = new GraphicSelector<ShadowResolution>(shadowResolutionSelector);
		shadowResolutionOpts.OnValuedChanged.AddListener(SetShadowResolution);
		skinWeightsOpts = new GraphicSelector<SkinWeights>(skinWeightsSelector, new List<SkinWeights>
		{
			SkinWeights.OneBone,
			SkinWeights.TwoBones,
			SkinWeights.FourBones,
			SkinWeights.Unlimited
		});
		skinWeightsOpts.OnValuedChanged.AddListener(SetSkinWeights);
		textureQualityOpts = new GraphicSelector<TextureQualityRender>(textureQualitySelector);
		textureQualityOpts.OnValuedChanged.AddListener(SetTextureQuality);
		antialisingOpts = new GraphicSelector<AntialiasingRender>(antialiasingSelector, new List<AntialiasingRender>
		{
			AntialiasingRender.DISABLED,
			AntialiasingRender.FXAA,
			AntialiasingRender.SMAA
		});
		antialisingOpts.OnValuedChanged.AddListener(SetAntiAliasing);
		vSyncOps = new GraphicSelector<VSyncQuality>(vsyncSelector);
		vSyncOps.OnValuedChanged.AddListener(SetVSync);
		fpsSelector.onValueChanged.AddListener((int index) =>
		{
			SetFPS(fpsOptions[index]);
		});
		if ((bool)pixelLightInput)
		{
			pixelLightInput.onValueChanged.AddListener(SetPixelLight);
		}
		softParticlesCheckbox.OnValueChanged.AddListener(SetSoftParticles);
		reflectionProbesCheckbox.OnValueChanged.AddListener(SetRealtimeReflectionProbes);
		advancedOptionsToggle.OnValueChanged.AddListener(ToggleAdvancedOptions);
		UpdateFields();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshFpsSelector();
	}

	private void RefreshFpsSelector()
	{
		int fPS = GraphicsController.GetFPS(ignoreVSyncSetting: true);
		fpsSelector.ClearOptions();
		List<string> list = new List<string>(fpsOptions.Count);
		int valueWithoutNotify = 0;
		for (int i = 0; i < fpsOptions.Count; i++)
		{
			if (fpsOptions[i] == -1)
			{
				list.Add(LocalizationManager.GetTranslation("SKIN_WEIGHTS_UNLIMITED"));
			}
			else
			{
				list.Add(fpsOptions[i].ToString());
			}
			if (fpsOptions[i] == fPS)
			{
				valueWithoutNotify = i;
			}
		}
		fpsSelector.AddOptions(list);
		fpsSelector.SetValueWithoutNotify(valueWithoutNotify);
	}

	private void ToggleAdvancedOptions(bool toggle)
	{
		advancedOptionsContainer.SetActive(toggle);
	}

	private void SetQualityLevel(string level)
	{
		if (!(selectedLevel == level))
		{
			GraphicsController.SetQualityLevel(level);
			UpdateFields();
			Save();
		}
	}

	public void SetTextureQuality(TextureQualityRender textureQuality)
	{
		GraphicsController.SetTextureQuality(textureQuality);
		UpdateFields();
		Save();
	}

	public void SetAntiAliasing(AntialiasingRender antialiasingRender)
	{
		GraphicsController.SetAntiAliasing(antialiasingRender);
		UpdateFields();
		Save();
	}

	public void SetRealtimeReflectionProbes(bool reflectionProbes)
	{
		GraphicsController.SetRealtimeReflectionProbes(reflectionProbes);
		UpdateFields();
		Save();
	}

	public void SetSoftParticles(bool softParticles)
	{
		GraphicsController.SetSoftParticles(softParticles);
		UpdateFields();
		Save();
	}

	public void SetShadowResolution(ShadowResolution shadowResolution)
	{
		GraphicsController.SetShadowResolution(shadowResolution);
		UpdateFields();
		Save();
	}

	public void SetShadows(ShadowQuality shadowQuality)
	{
		GraphicsController.SetShadowQuality(shadowQuality);
		UpdateFields();
		Save();
	}

	public void SetSkinWeights(SkinWeights skinWeights)
	{
		GraphicsController.SetSkinWeights(skinWeights);
		UpdateFields();
		Save();
	}

	public void SetVSync(VSyncQuality vsync)
	{
		GraphicsController.SetVSync(vsync);
		UpdateFields();
		Save();
	}

	public void SetFPS(int fps)
	{
		GraphicsController.SetFPS(fps, disableVSync: true);
		UpdateFields();
		Save(force: true);
	}

	public void SetPixelLight(string pixelLight)
	{
		if (int.TryParse(pixelLight, out var result))
		{
			GraphicsController.SetPixelLightCount(result);
			UpdateFields();
			Save();
		}
	}

	public void SetAnisotropicTexture(AnisotropicFiltering anisotropicFiltering)
	{
		GraphicsController.SetAnisotropicFiltering(anisotropicFiltering);
		UpdateFields();
		Save();
	}

	private void UpdateFields()
	{
		if (GraphicsController.IsUsingCustomProfile)
		{
			if (!customProfileIsListed)
			{
				levelOpts.AddOption(new SelectorOptData<string>("Custom", () => LocalizationManager.GetTranslation("GUI_OPT_GRAPHICS_Custom")));
			}
			levelOpts.SetValueWithoutNotify("Custom");
		}
		else
		{
			if (customProfileIsListed)
			{
				levelOpts.RemoveOption("Custom");
			}
			GraphicsController.GetQualitySetting(out var valueWithoutNotify);
			levelOpts.SetValueWithoutNotify(valueWithoutNotify);
		}
		anisotropicFilteringOpts.SetValueWithoutNotify(GraphicsController.GetAnisotropicFiltering());
		shadowResolutionOpts.SetValueWithoutNotify(GraphicsController.GetShadowResolution());
		skinWeightsOpts.SetValueWithoutNotify(GraphicsController.GetSkinWeights());
		shadowsQualityOpts.SetValueWithoutNotify(GraphicsController.GetShadowQuality());
		if ((bool)pixelLightInput)
		{
			pixelLightInput.SetValue(GraphicsController.GetPixelLightCount().ToString());
		}
		antialisingOpts.SetValueWithoutNotify(GraphicsController.GetAntiAliasing());
		textureQualityOpts.SetValueWithoutNotify(GraphicsController.GetTextureQuality());
		VSyncQuality vSync = GraphicsController.GetVSync();
		vSyncOps.SetValueWithoutNotify(vSync);
		reflectionProbesCheckbox.SetValue(GraphicsController.GetRealtimeReflectionProbes());
		softParticlesCheckbox.SetValue(GraphicsController.GetSoftParticles());
		fpsSelector.SetValueWithoutNotify(fpsSelector.options.FindIndex((TMP_Dropdown.OptionData option) => option.text == GraphicsController.GetFPS(ignoreVSyncSetting: true).ToString()));
		fpsOption.SetActive(vSync == VSyncQuality.DISABLED);
	}

	private async void Save(bool force = false)
	{
		if (force || !(SaveData.Instance.Global.QualityLevel != "Custom") || !(selectedLevel == SaveData.Instance.Global.QualityLevel) || SaveData.Instance.Global.CustomQualityProfile != null)
		{
			await SaveData.Instance.SaveGlobalData();
		}
	}

	public void CloseSelectors()
	{
		anisotropicSelector.Hide();
		antialiasingSelector.Hide();
		shadowResolutionSelector.Hide();
		shadowSelector.Hide();
		skinWeightsSelector.Hide();
		textureQualitySelector.Hide();
		vsyncSelector.Hide();
	}

	protected override void OnLanguageChanged()
	{
		anisotropicFilteringOpts.RefreshTexts();
		antialisingOpts.RefreshTexts();
		levelOpts.RefreshTexts();
		shadowResolutionOpts.RefreshTexts();
		shadowsQualityOpts.RefreshTexts();
		skinWeightsOpts.RefreshTexts();
		textureQualityOpts.RefreshTexts();
		vSyncOps.RefreshTexts();
		RefreshFpsSelector();
	}
}

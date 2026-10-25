using System;
using Libs.GameConsole;
using UnityEngine;

namespace Gloomhaven;

public class GraphicsController : MonoBehaviour
{
	public PerformanceProfiles _profiles;

	private AntialiasingRender _currentAA;

	private bool _isUsingCustomProfile;

	private HardwareIdentification _hardwareIdentification;

	public static Action OnQualityLevelChanged;

	private static GraphicsController _instance;

	public const string CUSTOM_PROFILE = "Custom";

	public static bool IsUsingCustomProfile => _instance._isUsingCustomProfile;

	private void Start()
	{
		if (SaveData.Instance != null && SaveData.Instance.Global != null)
		{
			Initialize();
		}
		else
		{
			SceneController.Instance.OnSaveDataLoaded += Initialize;
		}
	}

	private void Initialize()
	{
		SceneController.Instance.OnSaveDataLoaded -= Initialize;
		_instance = this;
		int num = -1;
		bool flag = true;
		string qualityLevel = SaveData.Instance.Global.QualityLevel;
		if (!string.IsNullOrEmpty(qualityLevel))
		{
			num = QualitySettings.names.IndexOf(qualityLevel);
			flag = num < 0;
		}
		if (flag)
		{
			num = IdentifyHardwareQualitySettings();
		}
		QualitySettings.SetQualityLevel(num);
		if (SaveData.Instance.Global.CustomQualityProfile != null)
		{
			_instance._isUsingCustomProfile = true;
			SaveData.Instance.Global.CustomQualityProfile.Setup();
			SetFPS(SaveData.Instance.Global.TargetFrameRate, disableVSync: false);
		}
		else
		{
			_instance._isUsingCustomProfile = false;
		}
	}

	private int IdentifyHardwareQualitySettings()
	{
		if ((object)_hardwareIdentification == null)
		{
			_hardwareIdentification = HardwareIdentification.LoadResource();
		}
		_hardwareIdentification.IdentifyHardware(SystemInfo.processorType, SystemInfo.graphicsDeviceName, out var identifiedCPU, out var identifiedGPU);
		int qSIndex = _profiles.MatchProfile(identifiedCPU?.Score ?? 0f, identifiedGPU?.Score ?? 0f).QSIndex;
		Debug.Log("[GraphicsController] Identified hardware: (CPU: " + (identifiedCPU?.Name ?? "Unknown") + ") (GPU: " + (identifiedGPU?.Name ?? "Unknown") + ") - Assigned Quality Setting: " + QualitySettings.names[qSIndex]);
		return qSIndex;
	}

	private static AntialiasingRender GetDefaultAntiAliasingForProfile(string profileName)
	{
		return profileName switch
		{
			"Fastest" => AntialiasingRender.DISABLED, 
			"Fast" => AntialiasingRender.DISABLED, 
			"Medium" => AntialiasingRender.FXAA, 
			"Good" => AntialiasingRender.FXAA, 
			"Beautiful" => AntialiasingRender.FXAA, 
			"Fantastic" => AntialiasingRender.SMAA, 
			_ => AntialiasingRender.DISABLED, 
		};
	}

	public static void SetQualityLevel(string level)
	{
		if (level == "Custom")
		{
			_instance._isUsingCustomProfile = true;
			GraphicProfile customQualityProfile = SaveData.Instance.Global.CustomQualityProfile;
			if (customQualityProfile != null)
			{
				customQualityProfile.Setup();
			}
			else
			{
				customQualityProfile = new GraphicProfile();
				customQualityProfile.LoadCurrentValues();
				SaveData.Instance.Global.CustomQualityProfile = customQualityProfile;
			}
		}
		else
		{
			_instance._isUsingCustomProfile = false;
			int num = QualitySettings.names.IndexOf(level);
			if (num < 0)
			{
				Debug.LogError("GraphicsController -> Tried to set quality level that doesn't exist: " + level);
				num = 0;
			}
			QualitySettings.SetQualityLevel(num);
			SaveData.Instance.Global.QualityLevel = level;
			SaveData.Instance.Global.CustomQualityProfile = null;
		}
		OnQualityLevelChanged?.Invoke();
	}

	private static GraphicProfile GetCustomProfile()
	{
		if (!_instance._isUsingCustomProfile)
		{
			SetQualityLevel("Custom");
		}
		return SaveData.Instance.Global.CustomQualityProfile;
	}

	public static void SetTextureQuality(TextureQualityRender textureQuality)
	{
		GetCustomProfile().TextureQuality = textureQuality;
		QualitySettings.globalTextureMipmapLimit = (int)textureQuality;
	}

	public static void SetAntiAliasing(AntialiasingRender antiAliasing)
	{
		_instance._currentAA = antiAliasing;
		GetCustomProfile().AntiAliasing = antiAliasing;
		PerCameraSettings[] array = UnityEngine.Object.FindObjectsOfType<PerCameraSettings>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init(antiAliasing);
		}
	}

	public static void SetRealtimeReflectionProbes(bool reflectionProbes)
	{
		GetCustomProfile().RealtimeReflectionProbes = reflectionProbes;
		QualitySettings.realtimeReflectionProbes = reflectionProbes;
	}

	public static void SetSoftParticles(bool softParticles)
	{
		GetCustomProfile().SoftParticles = softParticles;
		QualitySettings.softParticles = softParticles;
	}

	public static void SetShadowResolution(ShadowResolution shadowResolution)
	{
		GetCustomProfile().ShadowResolution = shadowResolution;
		QualitySettings.shadowResolution = shadowResolution;
	}

	public static void SetShadowQuality(ShadowQuality shadowQuality)
	{
		GetCustomProfile().Shadows = shadowQuality;
		QualitySettings.shadows = shadowQuality;
	}

	public static void SetSkinWeights(SkinWeights skinWeights)
	{
		GetCustomProfile().SkinWeights = skinWeights;
		QualitySettings.skinWeights = skinWeights;
	}

	public static void SetVSync(VSyncQuality vsync)
	{
		GetCustomProfile().VSync = vsync;
		QualitySettings.vSyncCount = (int)vsync;
		if (vsync == VSyncQuality.DISABLED)
		{
			Application.targetFrameRate = SaveData.Instance.Global.TargetFrameRate;
		}
		else
		{
			Application.targetFrameRate = -1;
		}
	}

	public static void SetFPS(int fps, bool disableVSync)
	{
		if (disableVSync)
		{
			SetVSync(VSyncQuality.DISABLED);
		}
		SaveData.Instance.Global.TargetFrameRate = fps;
		Application.targetFrameRate = ((QualitySettings.vSyncCount == 0) ? fps : (-1));
	}

	public static void SetPixelLightCount(int pixelLight)
	{
		GetCustomProfile().PixelLight = pixelLight;
		QualitySettings.pixelLightCount = pixelLight;
	}

	public static void SetAnisotropicFiltering(AnisotropicFiltering anisotropicFiltering)
	{
		GetCustomProfile().AnisotropicTexture = anisotropicFiltering;
		QualitySettings.anisotropicFiltering = anisotropicFiltering;
	}

	public static int GetQualitySetting(out string name)
	{
		int qualityLevel = QualitySettings.GetQualityLevel();
		name = QualitySettings.names[qualityLevel];
		return qualityLevel;
	}

	public static TextureQualityRender GetTextureQuality()
	{
		return (TextureQualityRender)QualitySettings.globalTextureMipmapLimit;
	}

	public static AntialiasingRender GetAntiAliasing()
	{
		if (!(_instance != null))
		{
			return AntialiasingRender.DISABLED;
		}
		return _instance._currentAA;
	}

	public static bool GetRealtimeReflectionProbes()
	{
		return QualitySettings.realtimeReflectionProbes;
	}

	public static bool GetSoftParticles()
	{
		return QualitySettings.softParticles;
	}

	public static ShadowResolution GetShadowResolution()
	{
		return QualitySettings.shadowResolution;
	}

	public static ShadowQuality GetShadowQuality()
	{
		return QualitySettings.shadows;
	}

	public static SkinWeights GetSkinWeights()
	{
		return QualitySettings.skinWeights;
	}

	public static VSyncQuality GetVSync()
	{
		return (VSyncQuality)QualitySettings.vSyncCount;
	}

	public static int GetFPS(bool ignoreVSyncSetting)
	{
		if (!ignoreVSyncSetting)
		{
			return Application.targetFrameRate;
		}
		return SaveData.Instance.Global.TargetFrameRate;
	}

	public static int GetPixelLightCount()
	{
		return QualitySettings.pixelLightCount;
	}

	public static AnisotropicFiltering GetAnisotropicFiltering()
	{
		return QualitySettings.anisotropicFiltering;
	}

	[ConsoleCommand(Command = "Autodetect_QualitySettings", Description = "Tries to identify CPU and GPU, and choose a QualitySettings accordingly.")]
	public static void Console_AutodetectQualitySettings(IConsole console)
	{
		GraphicsController instance = _instance;
		if ((object)instance._hardwareIdentification == null)
		{
			instance._hardwareIdentification = HardwareIdentification.LoadResource();
		}
		_instance._hardwareIdentification.IdentifyHardware(SystemInfo.processorType, SystemInfo.graphicsDeviceName, out var identifiedCPU, out var identifiedGPU);
		int qSIndex = _instance._profiles.MatchProfile(identifiedCPU?.Score ?? 0f, identifiedGPU?.Score ?? 0f).QSIndex;
		string text = QualitySettings.names[qSIndex];
		SetQualityLevel(text);
		console.WriteLine("Identified hardware: (CPU: " + (identifiedCPU?.Name ?? "Unknown") + ") (GPU: " + (identifiedGPU?.Name ?? "Unknown") + ") - Assigned Quality Setting: " + text);
	}
}

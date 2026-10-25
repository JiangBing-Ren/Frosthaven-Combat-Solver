using System;
using Newtonsoft.Json;

[Serializable]
public class EditorProperties
{
	private static EditorProperties s_Instance;

	public static EditorProperties Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new EditorProperties();
				s_Instance.Load();
			}
			return s_Instance;
		}
	}

	[JsonProperty]
	public bool DisplayBasicTilesAtRuntimeEditor { get; private set; }

	[JsonProperty]
	public bool DisableRoomHiding { get; private set; }

	[JsonProperty]
	public bool MMBPanning { get; private set; }

	[JsonProperty]
	public bool AnalyticsDebug { get; private set; }

	[JsonProperty]
	public bool SteamInEditor { get; private set; }

	[JsonProperty]
	public bool GoGGalaxyInEditor { get; private set; }

	[JsonProperty]
	public bool EOSInEditor { get; private set; }

	[JsonProperty]
	public bool LogCameraDetails { get; private set; }

	[JsonProperty]
	public bool EnableFogOfWar { get; private set; }

	[JsonProperty]
	public string CustomEditorDataPath { get; private set; }

	[JsonProperty]
	public bool LocalMultiplayer { get; private set; }

	[JsonProperty]
	public bool WriteSaveAssemblyInfo { get; private set; }

	[JsonProperty]
	public bool CompileRulebaseOnPlay { get; private set; }

	[JsonProperty]
	public bool MPEndOfTurnCompare { get; set; }

	[JsonProperty]
	public bool LoadMostRecentSave { get; private set; }

	[JsonProperty]
	public bool EnableUseAssetBundlesInEditor { get; private set; }

	[JsonProperty]
	public bool EnableCardCacheInEditor { get; private set; }

	[JsonProperty]
	public bool UseLanNetworking { get; private set; }

	[JsonProperty]
	public bool RecoverOldSaves { get; private set; }

	public EditorProperties()
	{
		DisplayBasicTilesAtRuntimeEditor = false;
		DisableRoomHiding = false;
		MMBPanning = false;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		EOSInEditor = IsDefineSet("EPICSTORE");
		LogCameraDetails = false;
		EnableFogOfWar = false;
		CustomEditorDataPath = string.Empty;
		LocalMultiplayer = false;
		WriteSaveAssemblyInfo = false;
		CompileRulebaseOnPlay = false;
		MPEndOfTurnCompare = false;
		LoadMostRecentSave = false;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
		EnableCardCacheInEditor = false;
		UseLanNetworking = false;
		RecoverOldSaves = false;
	}

	public void Save()
	{
	}

	public void Load()
	{
	}

	private void SetProperties(EditorProperties props)
	{
		DisplayBasicTilesAtRuntimeEditor = props.DisplayBasicTilesAtRuntimeEditor;
		DisableRoomHiding = props.DisableRoomHiding;
		MMBPanning = props.MMBPanning;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		LogCameraDetails = props.LogCameraDetails;
		EnableFogOfWar = props.EnableFogOfWar;
		CustomEditorDataPath = props.CustomEditorDataPath;
		LocalMultiplayer = props.LocalMultiplayer;
		WriteSaveAssemblyInfo = props.WriteSaveAssemblyInfo;
		CompileRulebaseOnPlay = props.CompileRulebaseOnPlay;
		MPEndOfTurnCompare = props.MPEndOfTurnCompare;
		LoadMostRecentSave = props.LoadMostRecentSave;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
		EnableCardCacheInEditor = props.EnableCardCacheInEditor;
		UseLanNetworking = props.UseLanNetworking;
		RecoverOldSaves = props.RecoverOldSaves;
	}

	public void SetProperties(bool displayBasicTilesAtRuntime, bool disableRoomHiding, bool mmbPanning, bool logCameraDetails, bool enableFogOfWar, string customEditorDataPath, bool localMultiplayer, bool writeSaveAssemblyInfo, bool compileRulebaseOnPlay, bool mpEndOfTurnCompare, bool loadMostRecentSave, bool enableAssetBundlesInEditor, bool enableCardCache, bool useLanNetworking, bool recoverOldSaves)
	{
		DisplayBasicTilesAtRuntimeEditor = displayBasicTilesAtRuntime;
		DisableRoomHiding = disableRoomHiding;
		MMBPanning = mmbPanning;
		AnalyticsDebug = IsDefineSet("DEBUG_ANALYTICS");
		SteamInEditor = IsDefineSet("STEAM") && IsDefineSet("FACEPUNCHAGENT");
		GoGGalaxyInEditor = IsDefineSet("GOGGALAXY");
		EOSInEditor = IsDefineSet("EPICSTORE");
		LogCameraDetails = logCameraDetails;
		EnableFogOfWar = enableFogOfWar;
		CustomEditorDataPath = customEditorDataPath;
		LocalMultiplayer = localMultiplayer;
		WriteSaveAssemblyInfo = writeSaveAssemblyInfo;
		CompileRulebaseOnPlay = compileRulebaseOnPlay;
		MPEndOfTurnCompare = mpEndOfTurnCompare;
		LoadMostRecentSave = loadMostRecentSave;
		EnableUseAssetBundlesInEditor = IsDefineSet("DEBUG_ASSET_BUNDLES");
		EnableCardCacheInEditor = enableCardCache;
		UseLanNetworking = useLanNetworking;
		RecoverOldSaves = recoverOldSaves;
	}

	private bool IsDefineSet(string define)
	{
		return false;
	}
}

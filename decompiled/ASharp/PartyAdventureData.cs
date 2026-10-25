using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using FFSNet;
using FH.YML;
using FHRL;
using Gloomhaven;
using Libs.Build;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using Newtonsoft.Json;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Achievments;

[Serializable]
public class PartyAdventureData : ISerializable
{
	public enum SaveType
	{
		Manual,
		AutoSave,
		QuickSave
	}

	public class CharacterInfo
	{
		public string ID;

		public int Level;

		public int Gold;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1024)]
	public struct Header
	{
		[StructLayout(LayoutKind.Sequential, Size = 128)]
		public struct HeaderCharacterInfo
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string ID;

			public int Level;

			public int Gold;

			public bool IsValid => !string.IsNullOrEmpty(ID);
		}

		public static readonly byte[] MagicNumber = new byte[4] { 83, 83, 71, 75 };

		public const int Size = 1024;

		public const int CampaignNameSizeLimit = 128;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		public string GameVersion;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string CampaignName;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public HeaderCharacterInfo[] LastSavedCharacterInfo;

		public SaveType Type;

		public int Seed;

		public EGameMode GameMode;

		public EAdventureDifficulty Difficulty;

		public long LastSavedTimeStamp;

		public int LastSavedProsperity;

		public double TimeActiveSecRunLTD;

		public int GameRevision;

		public bool IsIronman;

		public int LastSaveWeek;

		public int CurrentScenarioID;

		public int CurrentScenarioRound;

		public ERulesFlag Rules;

		public bool IsValid => !string.IsNullOrEmpty(CampaignName);
	}

	public static int InvalidateSavesBeforeRevision = 25294;

	public const int MaxAutoSaves = 15;

	public const int MaxQuickSaves = 25;

	public const string QuickSaveName = "QuickSave";

	public const string AutoSaveName = "AutoSave";

	public const string HostSaveName = "HostSave";

	[NonSerialized]
	public static Action OnRulesChanged;

	private int _headerCurrentScenarioRound = -1;

	private int _headerCurrentScenarioID = -1;

	private string _saveFileName;

	private bool isSaving;

	public EGameMode GameMode { get; private set; }

	public string PartyName { get; private set; }

	public string PartyNameNoSpaces => PartyName.Replace(" ", "_");

	public int Seed { get; private set; }

	public EAdventureDifficulty Difficulty { get; private set; }

	public ERulesFlag Rules { get; private set; }

	public DateTime LastSavedTimeStamp { get; private set; }

	public int LastSavedProsperity { get; private set; }

	public int LastSavedWeek { get; private set; }

	public List<CharacterInfo> LastSavedSelectedCharacterInfo { get; private set; }

	public int FromGameRevision { get; private set; }

	public string FromGameVersion { get; private set; }

	public bool IsUserIDSet { get; set; }

	public SaveOwner Owner { get; set; }

	public string RunSessionID { get; set; }

	public double TimeActiveSecRunLTD { get; set; }

	public ScenarioID? CurrentScenarioID => GetCurrentScenarioID();

	public int CurrentScenarioRound => GetCurrentScenarioRound();

	public FHSaveData FrosthavenSaveData { get; set; }

	public int SaveVersion => FrosthavenSaveData?.SaveVersion ?? (-1);

	public CMapState AdventureMapState => FrosthavenSaveData?.MapState;

	public ScenarioRuleLibrary.ScenarioState FastReloadState
	{
		get
		{
			return FrosthavenSaveData?.FastReloadState;
		}
		set
		{
			FrosthavenSaveData.FastReloadState = value;
		}
	}

	public CampaignState FrosthavenCampaign
	{
		get
		{
			return FrosthavenSaveData?.GameState;
		}
		set
		{
			FrosthavenSaveData.GameState = value;
		}
	}

	public FHScenarioResult ScenarioResult
	{
		get
		{
			return FrosthavenSaveData?.ScenarioResult;
		}
		set
		{
			FrosthavenSaveData.ScenarioResult = value;
		}
	}

	public bool IsOldSaveFileFormat { get; private set; }

	public Task<string> PartySaveRoot
	{
		get
		{
			switch (GameMode)
			{
			case EGameMode.Campaign:
				return SaveData.Instance.RootData.CampaignSavePath;
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				return SaveData.Instance.RootData.SingleScenarioSavePath;
			default:
				Log.Error("Invalid Game Mode is set for PartyAdventureData: " + GameMode);
				return null;
			}
		}
	}

	public string PartySaveFolderOnly
	{
		get
		{
			switch (GameMode)
			{
			case EGameMode.Campaign:
				return "Campaign";
			case EGameMode.SingleScenario:
				return "SingleScenarios";
			default:
				Log.Error("Invalid Gamde Mode is set for PartyAdventureData: " + GameMode);
				return null;
			}
		}
	}

	public string SaveFileName
	{
		get
		{
			return _saveFileName;
		}
		set
		{
			_saveFileName = value;
			string[] array = SaveFileName.Take(20).Split('_');
			if (array.Length == 2 && uint.TryParse(array[1], out var result))
			{
				SafeFileIndex = (int)result;
				string text = array[0];
				if (!(text == "QuickSave"))
				{
					if (text == "AutoSave")
					{
						SaveFileType = SaveType.AutoSave;
					}
					else
					{
						SaveFileType = SaveType.Manual;
					}
				}
				else
				{
					SaveFileType = SaveType.QuickSave;
				}
			}
			else
			{
				SaveFileType = SaveType.Manual;
				SafeFileIndex = 0;
			}
		}
	}

	public SaveType SaveFileType { get; set; }

	public int SafeFileIndex { get; private set; }

	public bool IsIronMan { get; private set; }

	public Task<string> PartySaveDir => FHTaskExtensions.CombineAsyncPaths(PartySaveRoot, PartyNameNoSpaces);

	public string PartySaveFileName => SaveFileName + ".dat";

	public Task<string> PartyMainSaveFile => FHTaskExtensions.CombineAsyncPaths(PartySaveDir, PartySaveFileName);

	private string ScreenshotFileName => SaveFileName + ".png";

	public Task<string> ScreenShotFile => FHTaskExtensions.CombineAsyncPaths(PartySaveDir, ScreenshotFileName);

	public bool IsSaving => isSaving;

	public PartyAdventureData(int seed, string partyName, EAdventureDifficulty difficulty, SaveOwner owner, EGameMode gameMode, ERulesFlag rules, string campaignID = "Frosthaven", bool isIronMan = false)
	{
		PartyName = partyName;
		Seed = seed;
		Difficulty = difficulty;
		Rules = rules;
		LastSavedSelectedCharacterInfo = new List<CharacterInfo>();
		IsUserIDSet = true;
		Owner = owner;
		GameMode = gameMode;
		IsIronMan = rules.HasFlag(ERulesFlag.Ironman);
		FromGameVersion = RuntimeBuildInfo.BuildVersion;
		FromGameRevision = RuntimeBuildInfo.BuildRevisionNumber;
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		SetDefaultFHSaveData();
		FrosthavenSaveData.CampaignID = campaignID;
	}

	private PartyAdventureData(Header header)
	{
		ApplyHeader(header);
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
		Owner = new SaveOwner();
		IsUserIDSet = true;
	}

	public PartyAdventureData(FHSaveData saveData, string partyName, SaveOwner owner, EGameMode gameMode, ERulesFlag rules, bool isIronMan)
	{
		FrosthavenSaveData = saveData;
		CreateDefaultMapStateIfNeeded(FrosthavenSaveData);
		PartyName = partyName;
		SaveFileName = partyName + "." + owner.PlatformPlayerID;
		Seed = FrosthavenSaveData?.GameState?.Random.Seed ?? new SharedLibrary.Random().Next();
		Difficulty = AdventureMapState.DifficultySetting;
		LastSavedSelectedCharacterInfo = new List<CharacterInfo>();
		IsUserIDSet = true;
		Owner = owner;
		GameMode = gameMode;
		Rules = rules;
		IsIronMan = isIronMan;
		FromGameVersion = RuntimeBuildInfo.BuildVersion;
		FromGameRevision = RuntimeBuildInfo.BuildRevisionNumber;
		RunSessionID = Gloomhaven.Utility.RandomMD5Hash();
	}

	public static PartyAdventureData GenerateDataFromFHSave(FHSaveData saveData, string saveFileName, DateTime timestamp, string partyName, SaveOwner owner, EGameMode gameMode, ERulesFlag rules, bool isIronMan, bool loadAsOldSave = false)
	{
		if (saveData == null)
		{
			return null;
		}
		PartyAdventureData partyAdventureData = new PartyAdventureData(saveData, partyName, owner, gameMode, rules, isIronMan)
		{
			SaveFileName = Path.GetFileNameWithoutExtension(saveFileName),
			LastSavedTimeStamp = timestamp,
			IsOldSaveFileFormat = loadAsOldSave
		};
		Header header = partyAdventureData.GenerateHeader();
		partyAdventureData.ApplyHeader(header);
		return partyAdventureData;
	}

	public static async Task<PartyAdventureData> GenerateDataFromFile(string filePath, string partyName, SaveOwner owner, EGameMode gameMode, ERulesFlag rules, bool isIronMan, bool loadAsOldSave = false)
	{
		return GenerateDataFromFHSave((await LoadGameStateFromFile(filePath, deleteOnError: false, loadAsOldSave)).Item1, filePath, File.GetLastWriteTime(filePath), partyName, owner, gameMode, rules, isIronMan, loadAsOldSave);
	}

	public PartyAdventureData()
	{
	}

	private int GetCurrentScenarioRound()
	{
		if (FrosthavenSaveData == null)
		{
			return _headerCurrentScenarioRound;
		}
		return (AdventureMapState?.CurrentMapScenarioState?.CurrentState)?.RoundNumber ?? (-1);
	}

	private ScenarioID? GetCurrentScenarioID()
	{
		if (FrosthavenSaveData == null)
		{
			if (_headerCurrentScenarioID > 0)
			{
				return new ScenarioID((uint)_headerCurrentScenarioID);
			}
			return null;
		}
		ScenarioRuleLibrary.ScenarioState scenarioState = AdventureMapState?.CurrentMapScenarioState?.CurrentState;
		if (scenarioState == null)
		{
			return null;
		}
		if (scenarioState.ScenarioSetup != null && scenarioState.ScenarioSetup.ScenarioID != 0)
		{
			return new ScenarioID(scenarioState.ScenarioSetup.ScenarioID);
		}
		string text = scenarioState.ID.Split("_").Skip(1).FirstOrDefault();
		if (text != null && uint.TryParse(text, out var result))
		{
			return new ScenarioID(result);
		}
		return null;
	}

	public void ResolveFHGameState(YMLDefRepository defRepo, bool recreateMapParty)
	{
		if (FrosthavenSaveData != null && FrosthavenSaveData.GameState == null)
		{
			FrosthavenSaveData.GameState = FHSaveData.NewGameState(Seed, Difficulty, defRepo, FrosthavenSaveData.CampaignID);
			ChangeCampaignRules(Rules, networkAction: true, force: true);
		}
		if (FrosthavenSaveData == null)
		{
			return;
		}
		FrosthavenSaveData.ResolveDefs(defRepo.DefRepository);
		CampaignStatRepository.RestoreInstance(FrosthavenSaveData.StatsRepo);
		ScenarioStatRepository.RestoreInstance(FrosthavenSaveData.ScenarioStatsRepo, triggerChanged: false);
		if (FrosthavenSaveData.SaveVersion != 89)
		{
			int saveVersion = FrosthavenSaveData.SaveVersion;
			try
			{
				FrosthavenSaveData.ApplyMigration(defRepo.DefRepository);
			}
			catch (Exception exception)
			{
				Log.Exception(exception, "Error while trying to migrate old save data.");
			}
			FrosthavenSaveData.SaveVersion = 89;
			Log.Info($"[SAVE] Migrated FHSaveData '{SaveFileName}' from version {saveVersion} to {FrosthavenSaveData.SaveVersion}");
		}
		FrosthavenSaveData.EnsureUsingCharacterIDs();
		FrosthavenSaveData.EnsureValidParty();
		if (!recreateMapParty || AdventureMapState == null)
		{
			return;
		}
		List<CMapCharacter> checkCharacters = AdventureMapState.MapParty.CheckCharacters;
		foreach (CMapCharacter item in checkCharacters)
		{
			AdventureMapState.MapParty.RemoveSelectedCharacter(item);
		}
		int num = 0;
		foreach (CharacterInfo charInfo in LastSavedSelectedCharacterInfo)
		{
			if (charInfo != null)
			{
				CMapCharacter cMapCharacter = checkCharacters.FirstOrDefault((CMapCharacter c) => c.CharacterID == charInfo.ID);
				if (cMapCharacter == null)
				{
					cMapCharacter = new CMapCharacter(charInfo.ID, charInfo.ID, 0);
					AdventureMapState.MapParty.AddCharacterToCharactersList(cMapCharacter);
					checkCharacters.Add(cMapCharacter);
				}
				AdventureMapState.MapParty.AddSelectedCharacterToSlot(cMapCharacter, num);
			}
			num++;
		}
	}

	private async Task UpdateFastReloadState()
	{
		if (FastReload.CanSave() && FastReload.FastUndoEnabled)
		{
			FastReloadState = await FastReload.SaveScenarioState();
		}
		else
		{
			FastReloadState = null;
		}
	}

	public async Task<bool> ManualSave(string newSaveName)
	{
		if (IsIronMan)
		{
			throw new Exception("ManualSave is not allowed on IronMan save");
		}
		if (newSaveName == SaveFileName && SaveFileType != SaveType.Manual)
		{
			throw new Exception($"Tried to manual save campaign '{PartyName}' with save file type {SaveFileType}");
		}
		await UpdateFastReloadState();
		if (!string.IsNullOrEmpty(newSaveName) && newSaveName != SaveFileName)
		{
			PartyAdventureData copy = new PartyAdventureData(GenerateHeader())
			{
				SaveFileName = SaveFileName
			};
			SaveFileName = newSaveName;
			string filePath = await PartyMainSaveFile;
			SaveFileName = copy.SaveFileName;
			return await SaveGameStateToFile(filePath);
		}
		if (string.IsNullOrEmpty(SaveFileName))
		{
			throw new Exception("Tried to manual save campaign '" + PartyName + "' without a save name");
		}
		return await SaveGameStateToFile(await PartyMainSaveFile);
	}

	public async Task<bool> AutoSave()
	{
		if (IsIronMan)
		{
			SaveFileName = "AutoSave";
		}
		else
		{
			IEnumerable<PartyAdventureData> source = (await SaveData.Instance.Global.LoadSavesFromDisk()).Where((PartyAdventureData s) => s.SaveFileType == SaveType.AutoSave && s.PartyName == PartyName);
			int num = (source.Any() ? source.Max((PartyAdventureData t) => t.SafeFileIndex) : (-1));
			num++;
			SaveFileName = string.Format("{0}_{1:D3}", "AutoSave", num);
		}
		FastReload.TriggerAutoSaveGame = false;
		await UpdateFastReloadState();
		bool success = await SaveGameStateToFile(await PartyMainSaveFile);
		if (success)
		{
			await TrimSaveTypes(SaveType.AutoSave);
		}
		return success;
	}

	public async Task<bool> QuickSave()
	{
		if (IsIronMan)
		{
			throw new Exception("QuickSave is not allowed on IronMan save");
		}
		IEnumerable<PartyAdventureData> source = (await SaveData.Instance.Global.LoadSavesFromDisk()).Where((PartyAdventureData s) => s.SaveFileType == SaveType.QuickSave && s.PartyName == PartyName);
		int num = (source.Any() ? source.Max((PartyAdventureData t) => t.SafeFileIndex) : (-1));
		num++;
		SaveFileName = string.Format("{0}_{1:D3}", "QuickSave", num);
		await UpdateFastReloadState();
		bool success = await SaveGameStateToFile(await PartyMainSaveFile);
		if (success)
		{
			await TrimSaveTypes(SaveType.QuickSave);
		}
		return success;
	}

	private async Task TrimSaveTypes(SaveType type)
	{
		int max = 0;
		switch (type)
		{
		case SaveType.AutoSave:
			max = 15;
			break;
		case SaveType.QuickSave:
			max = 25;
			break;
		}
		if (max <= 0)
		{
			return;
		}
		List<PartyAdventureData> allSaves = await SaveData.Instance.Global.LoadSavesFromDisk();
		List<PartyAdventureData> trimSaves = (from p in allSaves
			where p.SaveFileType == type && p.PartyName == PartyName
			orderby p.SafeFileIndex descending
			select p).ToList();
		int maxTrimSavesPerIteration = 5;
		try
		{
			while (trimSaves.Count > max && maxTrimSavesPerIteration > 0)
			{
				PartyAdventureData s = trimSaves.Last();
				File.Delete(await s.PartyMainSaveFile);
				string path = await s.ScreenShotFile;
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				allSaves.Remove(s);
				trimSaves.Remove(s);
				int num = maxTrimSavesPerIteration - 1;
				maxTrimSavesPerIteration = num;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"An exception occurred attempting to trim saves of type {type}.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00014", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private static async Task<bool> IsValidSaveFile(Stream sw)
	{
		if (sw.Length < 1024)
		{
			return false;
		}
		byte[] signature = new byte[4];
		await sw.ReadAsync(signature, 0, 4);
		if (!Enumerable.SequenceEqual(signature, Header.MagicNumber))
		{
			return false;
		}
		return true;
	}

	public static async Task<PartyAdventureData> LoadSaveInfoFromFile(string filePath)
	{
		if (filePath == null)
		{
			Log.Error("Null filepath sent to LoadSaveHeaderFromFile");
			return null;
		}
		if (!File.Exists(filePath))
		{
			Log.Error("Save filePath does not exist. " + filePath);
			return null;
		}
		using (FileStream sw = File.OpenRead(filePath))
		{
			if (await IsValidSaveFile(sw))
			{
				byte[] buffer = new byte[1024];
				await sw.ReadAsync(buffer);
				GCHandle gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				Header header = Marshal.PtrToStructure<Header>(gCHandle.AddrOfPinnedObject());
				gCHandle.Free();
				if (header.IsValid)
				{
					return new PartyAdventureData(header)
					{
						SaveFileName = Path.GetFileNameWithoutExtension(filePath)
					};
				}
			}
		}
		Log.Error("File is not a valid save: " + filePath);
		return null;
	}

	public static async Task<PartyAdventureData> LoadOldSaveInfoFromFile(string filePath)
	{
		if (filePath == null)
		{
			Log.Error("Null filepath sent to LoadSaveHeaderFromFile");
			return null;
		}
		if (!File.Exists(filePath))
		{
			Log.Error("Save filePath does not exist. " + filePath);
			return null;
		}
		Log.Warning("Trying to recover save with old format: " + filePath);
		string overrideFileName = null;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
		if (fileNameWithoutExtension == "PartySave")
		{
			fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(filePath));
		}
		EGameMode gameMode = (filePath.Contains("Campaign") ? EGameMode.Campaign : EGameMode.SingleScenario);
		PartyAdventureData partyAdventureData = await GenerateDataFromFile(filePath, fileNameWithoutExtension, new SaveOwner(), gameMode, ERulesFlag.None, isIronMan: false, loadAsOldSave: true);
		if (partyAdventureData == null)
		{
			Log.Warning("Failed to recover save with old save format: " + filePath);
			return null;
		}
		if (partyAdventureData.AdventureMapState.DLLMode != ScenarioManager.EDLLMode.None)
		{
			partyAdventureData.GameMode = (partyAdventureData.AdventureMapState.IsCampaign ? EGameMode.Campaign : EGameMode.SingleScenario);
		}
		if (!string.IsNullOrEmpty(overrideFileName))
		{
			partyAdventureData.SaveFileName = overrideFileName;
		}
		Log.Warning("Recovered save with old save format successfully: " + filePath);
		return partyAdventureData;
	}

	private static string GetPlayer0Position(ScenarioRuleLibrary.ScenarioState state)
	{
		if (state == null)
		{
			return "null-state";
		}
		if (state.Players == null || state.Players.Count == 0)
		{
			return "null-players";
		}
		PlayerState playerState = state.Players[0];
		if (playerState.Location == null)
		{
			return "null-location";
		}
		return playerState.Location.ToString();
	}

	public void ChangeDifficulty(EAdventureDifficulty difficulty)
	{
		Difficulty = difficulty;
		if (FrosthavenSaveData?.GameState != null)
		{
			FrosthavenSaveData.GameState.Difficulty = difficulty.ToString();
		}
		if (AdventureMapState != null)
		{
			AdventureMapState.ChangeDifficulty(difficulty);
		}
	}

	public void ChangeCampaignRules(ERulesFlag rules, bool networkAction = true, bool force = false)
	{
		if (Rules == rules && !force)
		{
			return;
		}
		Rules = rules;
		if (FrosthavenSaveData?.GameState != null)
		{
			FrosthavenSaveData.GameState.Settings.EnableCharacterRespecWithoutBuilding = Rules.HasFlag(ERulesFlag.CanRespecCharacters);
		}
		if (networkAction)
		{
			if (FFSNetwork.IsOnline)
			{
				Synchronizer.SendSideAction(GameActionType.ChangeHouseRuleSettings, null, canBeUnreliable: false, sendToHostOnly: false, 0, (int)rules);
			}
		}
		else
		{
			OnRulesChanged?.Invoke();
		}
	}

	private Header GenerateHeader()
	{
		Header result = new Header
		{
			GameMode = GameMode,
			Difficulty = Difficulty,
			Rules = Rules,
			Seed = (FrosthavenSaveData?.GameState?.Random.Seed ?? Seed),
			CampaignName = PartyName,
			LastSavedTimeStamp = DateTime.Now.ToBinary(),
			GameVersion = RuntimeBuildInfo.BuildVersion,
			LastSavedProsperity = ((int?)FrosthavenSaveData?.GameState?.Outpost.Prosperity).GetValueOrDefault(),
			LastSaveWeek = ((int?)FrosthavenSaveData?.GameState?.Outpost.Week).GetValueOrDefault(),
			TimeActiveSecRunLTD = TimeActiveSecRunLTD,
			GameRevision = RuntimeBuildInfo.BuildRevisionNumber,
			Type = SaveFileType,
			IsIronman = IsIronMan,
			CurrentScenarioID = (CurrentScenarioID.HasValue ? ((int)CurrentScenarioID.Value.ID) : (-1)),
			CurrentScenarioRound = CurrentScenarioRound
		};
		if (AdventureMapState?.MapParty != null)
		{
			result.LastSavedCharacterInfo = new Header.HeaderCharacterInfo[4];
			int num = 0;
			CMapCharacter[] selectedCharactersArray = AdventureMapState.MapParty.SelectedCharactersArray;
			foreach (CMapCharacter cMapCharacter in selectedCharactersArray)
			{
				if (num == result.LastSavedCharacterInfo.Length)
				{
					break;
				}
				if (cMapCharacter == null)
				{
					result.LastSavedCharacterInfo[num] = default;
				}
				else
				{
					result.LastSavedCharacterInfo[num] = new Header.HeaderCharacterInfo
					{
						ID = cMapCharacter.CharacterID,
						Gold = cMapCharacter.CharacterGold,
						Level = cMapCharacter.Level
					};
				}
				num++;
			}
		}
		return result;
	}

	private void ApplyHeader(Header header)
	{
		PartyName = header.CampaignName;
		Seed = header.Seed;
		Difficulty = header.Difficulty;
		GameMode = header.GameMode;
		IsIronMan = header.IsIronman;
		TimeActiveSecRunLTD = header.TimeActiveSecRunLTD;
		LastSavedTimeStamp = DateTime.FromBinary(header.LastSavedTimeStamp);
		LastSavedProsperity = header.LastSavedProsperity;
		LastSavedWeek = header.LastSaveWeek;
		FromGameVersion = header.GameVersion;
		FromGameRevision = header.GameRevision;
		_headerCurrentScenarioID = header.CurrentScenarioID;
		_headerCurrentScenarioRound = header.CurrentScenarioRound;
		LastSavedSelectedCharacterInfo = header.LastSavedCharacterInfo.Select((Header.HeaderCharacterInfo s) => s.IsValid ? new CharacterInfo
		{
			ID = s.ID,
			Gold = s.Gold,
			Level = s.Level
		} : null).ToList();
	}

	private async Task<bool> SaveGameStateToFile(string filePath)
	{
		if (!SaveData.Instance.IsSavingThreadActive && Thread.CurrentThread != SceneController.Instance.MainThread)
		{
			Log.Error("Attempting to call SaveGameStateToFile from a thread that is not the main thread.  This is not allowed.\nMainThread: " + SceneController.Instance.MainThread.Name.ToString() + "\nCurrentThread: " + Thread.CurrentThread.Name.ToString());
			return false;
		}
		ScenarioRuleLibrary.ScenarioState scenarioState = FrosthavenSaveData?.MapState?.CurrentSingleScenario?.ScenarioState;
		if (scenarioState == null)
		{
			scenarioState = FrosthavenSaveData?.MapState?.CurrentMapScenarioState?.CurrentState;
		}
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		string player0Position = GetPlayer0Position(scenarioState);
		string player0Position2 = GetPlayer0Position(currentScenarioState);
		Log.Info("[SAVE] Saving gamestate to filepath: " + filePath + "\nSaved / current player 0 position: " + player0Position + " / " + player0Position2);
		isSaving = true;
		bool savedSuccessfully = true;
		Header header = GenerateHeader();
		try
		{
			string path = await PartySaveDir;
			if (!Directory.Exists(path))
			{
				Log.Info("Creating PartySaveDir directory: " + PartySaveDir);
				Directory.CreateDirectory(path);
			}
			int numberAttempts = 1;
			Exception lastException = null;
			byte[] managedBuffer = new byte[1024];
			GCHandle handle = GCHandle.Alloc(managedBuffer, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			Marshal.StructureToPtr(header, ptr, fDeleteOld: false);
			MemoryStream ms = new MemoryStream();
			await FrosthavenSaveData.BinarySerialize(ms);
			byte[] saveDataBinary = ms.ToArray();
			ms.Dispose();
			while (numberAttempts-- > 0)
			{
				try
				{
					using FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
					await fs.WriteAsync(Header.MagicNumber);
					await fs.WriteAsync(managedBuffer);
					await fs.WriteAsync(saveDataBinary);
					lastException = null;
				}
				catch (IOException ex)
				{
					Log.Exception(ex, "Exception while trying to save game state to file '" + filePath + "'");
					lastException = ex;
					Thread.Sleep(500);
					continue;
				}
				break;
			}
			handle.Free();
			if (lastException != null)
			{
				throw lastException;
			}
		}
		catch (Exception ex2)
		{
			savedSuccessfully = false;
			Log.Exception(ex2, "An exception occurred attempting to save Map State data.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00014", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
		}
		finally
		{
			isSaving = false;
			if (savedSuccessfully)
			{
				ApplyHeader(header);
				SaveFileName = Path.GetFileNameWithoutExtension(filePath);
				SaveData.Instance.Global.InvalidateCache = true;
				IsOldSaveFileFormat = false;
			}
			if (Singleton<AutoSaveProgress>.IsInitialized)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
		return true;
	}

	public void OverwriteAdventureMapStateFilePath(string campaignSaveFile)
	{
		throw new NotImplementedException();
	}

	public async Task Load(EGameMode mode, bool isJoiningMPClient)
	{
		string filePath = await PartyMainSaveFile;
		FrosthavenSaveData = (await LoadGameStateFromFile(filePath, deleteOnError: false, IsOldSaveFileFormat)).Item1;
		if (FrosthavenSaveData == null)
		{
			if (File.Exists(filePath))
			{
				Log.Error("No valid checkpoint map state file found");
				await MoveGameStateFileToCorruptedSaveFolder(mode);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SAVEDATA_00013", "GUI_ERROR_CONTINUE_BUTTON", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide);
			}
			else
			{
				await SaveData.Instance.SaveGlobalData();
			}
		}
		else
		{
			CreateDefaultMapStateIfNeeded(FrosthavenSaveData);
		}
	}

	public void WinScenario()
	{
		AdventureMapState.CurrentMapScenarioState = null;
	}

	public void LoseScenario(bool reset = true)
	{
		AdventureMapState.CurrentMapScenarioState = null;
	}

	public void RetryScenario()
	{
		AdventureMapState.CurrentMapScenarioState.RegenerateMapScenario();
		AdventureMapState.CurrentMapScenarioState.CheckForNonSerializedInitialScenario();
	}

	public static async Task<string> ConvertMapStateToJSON(string filePath)
	{
		FHSaveData item = (await LoadGameStateFromFile(filePath)).Item1;
		if (item != null)
		{
			return JsonConvert.SerializeObject(item, Formatting.Indented);
		}
		return string.Empty;
	}

	public static string ConvertScenarioStateToJSON(ScenarioRuleLibrary.ScenarioState state)
	{
		if (state != null)
		{
			return JsonConvert.SerializeObject(state, Formatting.Indented);
		}
		return string.Empty;
	}

	private static async Task<(FHSaveData data, bool gameClosedWhileSaving)> LoadGameStateFromFile(string filePath, bool deleteOnError = false, bool loadOldSaveFileFormat = false)
	{
		(FHSaveData data, bool gameClosedWhileSaving) gameState = default;
		gameState.gameClosedWhileSaving = false;
		Log.Info("[SAVE] Load game state from file '" + filePath + "'");
		if (File.Exists(filePath))
		{
			try
			{
				using FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
				SaveData.Instance.CurrentFileBeingDeserialized = filePath;
				if (!loadOldSaveFileFormat)
				{
					if (!(await IsValidSaveFile(fs)))
					{
						throw new Exception("Tried to load  FHSaveData from invalid save file: '" + filePath + "'");
					}
					fs.Position += 1024L;
				}
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				BinaryFormatter binaryFormatter2 = binaryFormatter;
				binaryFormatter2.Binder = new SerializationBinding(await SaveData.Instance.AssemblyInfoSaveDir);
				object obj = binaryFormatter.Deserialize(fs);
				gameState.data = obj as FHSaveData;
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("End of Stream encountered before parsing was completed"))
				{
					gameState.gameClosedWhileSaving = true;
				}
				Log.Error("Map state file could not be deserialized" + Log.GetExceptionText(ex));
				if (deleteOnError)
				{
					File.Delete(filePath);
					SaveData.Instance.Global.InvalidateCache = true;
				}
				gameState.data = null;
			}
		}
		else
		{
			Log.Warning("Map state file path doesn't exist");
			gameState.data = null;
		}
		return gameState;
	}

	public async Task MoveGameStateFileToCorruptedSaveFolder(EGameMode mode)
	{
		string text = await PartyMainSaveFile;
		if (File.Exists(text))
		{
			await MoveGameStateToCorruptedSaveFolderInternal(PartySaveFileName, text);
		}
		string path = await ScreenShotFile;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		FrosthavenSaveData = null;
		SaveData.Instance.Global.InvalidateCache = true;
	}

	public static async Task MoveGameStateToInvalidatedSaveFolderInternal(PartyAdventureData save, string oldFile)
	{
		string text = await SaveData.Instance.RootData.InvalidatedSavePath;
		if (!Directory.Exists(text))
		{
			Log.Info("Creating InvalidatedSavePath directory: " + text);
			Directory.CreateDirectory(text);
		}
		string campaignDir = Path.Combine(text, save.PartyName);
		if (!Directory.Exists(campaignDir))
		{
			Log.Info("Creating InvalidatedSavePath\\CampaignName directory: " + campaignDir);
			Directory.CreateDirectory(campaignDir);
		}
		string text2 = Path.Combine(campaignDir, save.PartySaveFileName);
		if (File.Exists(text2))
		{
			Log.Info("Deleting existing file: " + text2);
			File.Delete(text2);
		}
		Log.Info("Moving save file: " + oldFile);
		File.Move(oldFile, text2);
		string text3 = await save.ScreenShotFile;
		if (File.Exists(text3))
		{
			string text4 = Path.Combine(campaignDir, save.ScreenshotFileName);
			if (File.Exists(text4))
			{
				Log.Info("Deleting existing file: " + text4);
				File.Delete(text4);
			}
			Log.Info("Moving screenshot file: " + text3);
			File.Move(text3, text4);
		}
		string text5 = await save.PartySaveDir;
		if (Directory.Exists(text5) && !Directory.EnumerateFileSystemEntries(text5).Any())
		{
			Log.Info("Deleting empty dir: " + text5);
			Directory.Delete(text5);
		}
	}

	public static async Task MoveGameStateToCorruptedSaveFolderInternal(string partySaveFileName, string mapStateFilePath)
	{
		string text = await SaveData.Instance.RootData.CorruptedSavePath;
		if (!Directory.Exists(text))
		{
			Log.Info("Creating CorruptedSavePath directory: " + text);
			Directory.CreateDirectory(text);
		}
		if (File.Exists(Path.Combine(text, partySaveFileName)))
		{
			File.Delete(Path.Combine(text, partySaveFileName));
		}
		File.Move(mapStateFilePath, Path.Combine(text, partySaveFileName));
	}

	public async Task CopyGametateFileToBackupFolder(EGameMode mode)
	{
		string adventureMapStateFilePath = await PartyMainSaveFile;
		if (File.Exists(adventureMapStateFilePath))
		{
			string text = await SaveData.Instance.RootData.PreDLCBackupSavePath;
			if (!Directory.Exists(text))
			{
				Log.Info("Creating BackupSave directory: " + text);
				Directory.CreateDirectory(text);
			}
			string text2 = Path.Combine(text, PartySaveFileName);
			int num = 0;
			while (File.Exists(text2))
			{
				num++;
				text2 = Path.Combine(text, SaveFileName + "_" + num + ".dat");
			}
			File.Copy(adventureMapStateFilePath, text2);
		}
	}

	private void SetDefaultFHSaveData()
	{
		FrosthavenSaveData = new FHSaveData();
		CreateDefaultMapStateIfNeeded(FrosthavenSaveData);
	}

	private bool CreateDefaultMapStateIfNeeded(FHSaveData saveData)
	{
		if (saveData != null && saveData.MapState == null)
		{
			saveData.CreateDefaultMapState(Difficulty, Owner, GameMode);
			return true;
		}
		return false;
	}

	public static ScenarioManager.EDLLMode ConvertGameModeToDLLMode(EGameMode gameMode)
	{
		switch (gameMode)
		{
		case EGameMode.Campaign:
			return ScenarioManager.EDLLMode.Campaign;
		case EGameMode.LevelEditor:
		case EGameMode.SingleScenario:
		case EGameMode.Autotest:
		case EGameMode.FrontEndTutorial:
			return ScenarioManager.EDLLMode.CustomScenario;
		default:
			return ScenarioManager.EDLLMode.None;
		}
	}

	public async Task UpdateSavePaths()
	{
		string path = await PartySaveDir;
		if (!Directory.Exists(path))
		{
			Log.Info("Creating PartySaveDir directory: " + PartySaveDir);
			Directory.CreateDirectory(path);
		}
		string partyMainSaveFile = await PartyMainSaveFile;
		if (File.Exists(partyMainSaveFile))
		{
			File.Delete(partyMainSaveFile);
		}
		File.Copy(await PartyMainSaveFile, partyMainSaveFile);
		FrosthavenSaveData = (await LoadGameStateFromFile(partyMainSaveFile, deleteOnError: false, IsOldSaveFileFormat)).Item1;
		await ManualSave(Path.GetFileNameWithoutExtension(partyMainSaveFile));
	}

	public void ScrapeStats(EResult result)
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			SaveData.Instance.Global.m_StatsDataStorage.ScrapeEventLog(result, initial: false, endScenario: true, null, ScenarioManager.CurrentScenarioState.RoundNumber);
		}
	}

	public void EndCurrentScenario(EResult result)
	{
		try
		{
			ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
			DefRepository repo;
			if (AdventureMapState.CurrentMapScenarioState != null)
			{
				repo = YMLDefRepository.Repo.DefRepository;
				CampaignController campaignController = new CampaignController(FrosthavenCampaign, repo);
				FHScenarioResult fHScenarioResult = new FHScenarioResult
				{
					ScenarioID = AdventureMapState.CurrentMapScenarioState.ID,
					eResult = result,
					ConclusionIndex = currentScenarioState.GetConclusionIndex(),
					InspirationChange = 0,
					MoraleSpent = currentScenarioState.MoraleSpent
				};
				fHScenarioResult.ConclusionParameters.AddRange(currentScenarioState.GetConclusionParameters());
				ScenarioResult = fHScenarioResult;
				if ((result == EResult.Win || result == EResult.Lose || result == EResult.Resign || result == EResult.Restart) && AdventureMapState.IsCampaign && !AdventureMapState.CurrentMapScenarioState.ID.ToUpper().Contains("JOTL") && !AdventureMapState.CurrentMapScenarioState.ID.ToUpper().Contains("_SOLO_"))
				{
					foreach (CPlayerActor playerActor in ScenarioManager.Scenario.AllPlayers)
					{
						FHScenarioResult.Character fhCharacter = new FHScenarioResult.Character
						{
							ClassID = playerActor.CharacterClass.CharacterID,
							Name = playerActor.CharacterName
						};
						fHScenarioResult.Characters.Add(fhCharacter);
						fhCharacter.Exhausted = playerActor.IsDeadForObjectives;
						fhCharacter.CapturedPets = (from y in currentScenarioState.CapturedEnemies
							where y.characterID == fhCharacter.ClassID
							select y.petID).ToList();
						int num = playerActor.LootAmount(LootCard.EType.Money);
						if (playerActor.CompanionSummon != null)
						{
							num = playerActor.CompanionSummon.LootAmount(LootCard.EType.Money);
						}
						int xP = playerActor.XP;
						fhCharacter.GoldChange = num;
						fhCharacter.ExperienceChange = xP;
						if (playerActor.TotalLoot?.XpFromSources != null && playerActor.TotalLoot.XpFromSources.TryGetValue("AbilityCards", out var value))
						{
							fhCharacter.ExperienceChangeFromAbilityCards = value;
						}
						if (result == EResult.Win || result == EResult.Lose || result == EResult.Resign)
						{
							fhCharacter.ResourceChange.Lumber = playerActor.LootAmount(LootCard.EType.Lumber);
							fhCharacter.ResourceChange.Metal = playerActor.LootAmount(LootCard.EType.Metal);
							fhCharacter.ResourceChange.Hide = playerActor.LootAmount(LootCard.EType.Hide);
							fhCharacter.ResourceChange.Arrowvine = playerActor.LootAmount(LootCard.EType.Arrowvine);
							fhCharacter.ResourceChange.Axenut = playerActor.LootAmount(LootCard.EType.Axenut);
							fhCharacter.ResourceChange.Corpsecap = playerActor.LootAmount(LootCard.EType.Corpsecap);
							fhCharacter.ResourceChange.Flamefruit = playerActor.LootAmount(LootCard.EType.Flamefruit);
							fhCharacter.ResourceChange.Rockroot = playerActor.LootAmount(LootCard.EType.Rockroot);
							fhCharacter.ResourceChange.Snowthistle = playerActor.LootAmount(LootCard.EType.Snowthistle);
							fhCharacter.ResourceChangeFromLootCards.Lumber = playerActor.LootAmount(LootCard.EType.Lumber, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Metal = playerActor.LootAmount(LootCard.EType.Metal, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Hide = playerActor.LootAmount(LootCard.EType.Hide, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Arrowvine = playerActor.LootAmount(LootCard.EType.Arrowvine, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Axenut = playerActor.LootAmount(LootCard.EType.Axenut, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Corpsecap = playerActor.LootAmount(LootCard.EType.Corpsecap, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Flamefruit = playerActor.LootAmount(LootCard.EType.Flamefruit, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Rockroot = playerActor.LootAmount(LootCard.EType.Rockroot, fromLootCards: true);
							fhCharacter.ResourceChangeFromLootCards.Snowthistle = playerActor.LootAmount(LootCard.EType.Snowthistle, fromLootCards: true);
							fhCharacter.LootCardTypesDrawn.Lumber = playerActor.LootAmount(LootCard.EType.Lumber, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Metal = playerActor.LootAmount(LootCard.EType.Metal, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Hide = playerActor.LootAmount(LootCard.EType.Hide, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Arrowvine = playerActor.LootAmount(LootCard.EType.Arrowvine, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Axenut = playerActor.LootAmount(LootCard.EType.Axenut, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Corpsecap = playerActor.LootAmount(LootCard.EType.Corpsecap, fromLootCards: true);
							fhCharacter.LootCardTypesDrawn.Flamefruit = playerActor.LootAmount(LootCard.EType.Flamefruit, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Rockroot = playerActor.LootAmount(LootCard.EType.Rockroot, fromLootCards: false, cardCount: true);
							fhCharacter.LootCardTypesDrawn.Snowthistle = playerActor.LootAmount(LootCard.EType.Snowthistle, fromLootCards: false, cardCount: true);
							fhCharacter.PerkChecks = playerActor.TotalLoot.PerkChecks;
						}
						foreach (uint lootedItemBlueprintID in playerActor.TotalLoot.LootedItemBlueprintIDs)
						{
							if (lootedItemBlueprintID == currentScenarioState.ScenarioSetup.NextRandomItemBlueprintID)
							{
								fHScenarioResult.LootedRandomItemBlueprintID = lootedItemBlueprintID;
								continue;
							}
							int param = GetOutpostItemID((int)lootedItemBlueprintID);
							fHScenarioResult.Effects.Add(new CampaignEffect(CampaignEffectType.GainItemBlueprint, param));
						}
						foreach (int item7 in playerActor.TotalLoot.ItemIds.Concat(playerActor.TotalLoot.SoldItemIds))
						{
							int item = GetOutpostItemID(item7);
							fhCharacter.NewItems.Add(item);
						}
						if (playerActor.TotalLoot.LootedRandomSideScenarioID != 0)
						{
							fHScenarioResult.LootedRandomSideScenarioID = playerActor.TotalLoot.LootedRandomSideScenarioID;
						}
						if (playerActor.TotalLoot.LootedScenarioID != 0)
						{
							fHScenarioResult.LootedScenarioID = playerActor.TotalLoot.LootedScenarioID;
						}
						fHScenarioResult.InspirationChange += playerActor.TotalLoot.NumEmptyRandomScenarioDraws + playerActor.TotalLoot.NumEmptyItemBlueprintDraws;
						if (result == EResult.Win || result == EResult.Lose || result == EResult.Resign)
						{
							foreach (string campaignSticker in playerActor.TotalLoot.CampaignStickers)
							{
								if (campaignSticker == "Brummix")
								{
									fHScenarioResult.Effects.Add(new CampaignEffect(CampaignEffectType.AddBrummix));
								}
								else
								{
									fHScenarioResult.Effects.Add(new CampaignEffect(CampaignEffectType.GainCampaignSticker, campaignSticker));
								}
							}
							foreach (Tuple<string, int> delayedCampaignSection in playerActor.TotalLoot.DelayedCampaignSections)
							{
								fHScenarioResult.Effects.Add(new CampaignEffect(CampaignEffectType.AddCalendarCampaignSection, delayedCampaignSection.Item1, delayedCampaignSection.Item2));
							}
						}
						fHScenarioResult.TriggeredCampaignSections.AddRange(playerActor.TotalLoot.TriggeredCampaignSections);
						if (playerActor.LootAmount(LootCard.EType.RandomItem) > 0)
						{
							fHScenarioResult.RandomItemLooted = true;
						}
						if (result == EResult.Win || playerActor.CanBattleGoalsCompleteOnLoseScenario())
						{
							foreach (Tuple<string, CBattleGoalState> chosenBattleGoalState in AdventureMapState.CurrentMapScenarioState.ChosenBattleGoalStates)
							{
								string item2 = chosenBattleGoalState.Item1;
								CBattleGoalState item3 = chosenBattleGoalState.Item2;
								if (!(fhCharacter.ClassID != item2))
								{
									CUnlockConditionState battleGoalConditionState = item3.BattleGoalConditionState;
									bool battleGoalCompleted = ((!battleGoalConditionState.NegativeCondition) ? (battleGoalConditionState.TotalConditionsAndTargets <= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed) : (battleGoalConditionState.TotalConditionsAndTargets >= battleGoalConditionState.CurrentProgress && !battleGoalConditionState.Failed));
									fhCharacter.BattleGoalCompleted = battleGoalCompleted;
								}
							}
						}
						if (result == EResult.Win)
						{
							foreach (Tuple<string, CBattleGoalState> masteryBattleGoalState in AdventureMapState.CurrentMapScenarioState.MasteryBattleGoalStates)
							{
								string item4 = masteryBattleGoalState.Item1;
								CBattleGoalState item5 = masteryBattleGoalState.Item2;
								if (!(fhCharacter.ClassID != item4))
								{
									CUnlockConditionState battleGoalConditionState2 = item5.BattleGoalConditionState;
									if ((!battleGoalConditionState2.NegativeCondition) ? (battleGoalConditionState2.TotalConditionsAndTargets <= battleGoalConditionState2.CurrentProgress && !battleGoalConditionState2.Failed) : (battleGoalConditionState2.TotalConditionsAndTargets >= battleGoalConditionState2.CurrentProgress && !battleGoalConditionState2.Failed))
									{
										fhCharacter.MasteryGoalsCompletedIDs.Add(item5.ID);
									}
								}
							}
						}
						CMapCharacter cMapCharacter = AdventureMapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == playerActor.CharacterClass.ID);
						if (cMapCharacter != null)
						{
							if (AdventureMapState.GoldMode == EGoldMode.CharacterGold)
							{
								cMapCharacter.ModifyGold(num, useGoldModifier: true);
							}
							cMapCharacter.GainEXP(playerActor.XP, AdventureMapState.Difficulty.HasXPModifier ? AdventureMapState.Difficulty.XPModifier : 1f);
							cMapCharacter.RemoveMapConditions(RewardCondition.EConditionMapDuration.NextScenario);
						}
						if (AdventureMapState.GoldMode == EGoldMode.PartyGold)
						{
							AdventureMapState.MapParty.ModifyPartyGold(num, useGoldModifier: true);
						}
						if (result == EResult.Restart)
						{
							CharacterState character = FrosthavenCampaign.PartyCharacters.SingleOrDefault((CharacterState c) => c.ClassID.ID == fhCharacter.ClassID && c.Name == fhCharacter.Name);
							for (int num2 = fhCharacter.NewItems.Count - 1; num2 >= 0; num2--)
							{
								ItemID key = new ItemID((uint)fhCharacter.NewItems[num2]);
								ItemDef item6 = repo.Items[key];
								if (!campaignController.CharacterController.CanHaveItem(character, item6))
								{
									fhCharacter.NewItems.RemoveAt(num2);
								}
							}
						}
						if (currentScenarioState?.Stats?.m_PlayerStats != null && currentScenarioState.Stats.m_PlayerStats.TryGetValue(playerActor.CharacterClass.ID, out var value2))
						{
							List<ScenarioResult.CharacterResult.KillStat> collection = value2.Kills.Select((CPlayerStatsKill killStat) => new ScenarioResult.CharacterResult.KillStat(killStat.ActorWhoDiedClassID, killStat.Items.ToList())).ToList();
							fhCharacter.KillStats.AddRange(collection);
						}
					}
					fHScenarioResult.Effects.AddRange(GetOrmiEffects(result));
					AdventureMapState.MapParty.ResetItems();
					foreach (CBattleGoalState item8 in AdventureMapState.CurrentMapScenarioState.AllActiveBattleGoals.Select((Tuple<string, CBattleGoalState> x) => x.Item2).ToList())
					{
						item8.ResetBattleGoalProgress();
					}
				}
				fHScenarioResult.LootedTreasureTableIDs.AddRange(currentScenarioState.LootedChests);
			}
			ScenarioRuleClient.Reset();
			ItemDef GetOutpostItem(int scenarioID)
			{
				if (repo.Items.Any((KeyValuePair<ItemID, ItemDef> itemEntry) => itemEntry.Value.ScenarioItemID == scenarioID))
				{
					return repo.Items.SingleOrDefault((KeyValuePair<ItemID, ItemDef> itemEntry) => itemEntry.Value.ScenarioItemID == scenarioID).Value;
				}
				return null;
			}
			int GetOutpostItemID(int scenarioID)
			{
				if (scenarioID == 1255000002)
				{
					scenarioID = 1255;
				}
				return ((int?)GetOutpostItem(scenarioID)?.ID.ID) ?? scenarioID;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "An exception occurred at PartyAdventureData.EndCurrentScenario");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_PARTYADVENTUREDATA_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static IEnumerable<CampaignEffect> GetOrmiEffects(EResult result)
	{
		ScenarioRuleLibrary.ScenarioState scenarioState = ScenarioManager.CurrentScenarioState;
		bool flag = scenarioState.ScenarioModifiers.Any((CScenarioModifier x) => x.Name == "Ormi1_Modifier");
		bool checkOrmi2 = scenarioState.ScenarioModifiers.Any((CScenarioModifier x) => x.Name == "Ormi2_Modifier");
		if (result == EResult.Win)
		{
			if (flag)
			{
				EnemyState enemyState = scenarioState.AllyMonsters.FirstOrDefault((EnemyState x) => x.ClassID == "Ormi1ID");
				if (enemyState?.Actor != null && !enemyState.Actor.IsDead)
				{
					yield return new CampaignEffect(CampaignEffectType.Ormi1Survived);
				}
				else
				{
					yield return new CampaignEffect(CampaignEffectType.Ormi1Died);
				}
			}
			if (checkOrmi2)
			{
				EnemyState enemyState2 = scenarioState.AllyMonsters.FirstOrDefault((EnemyState x) => x.ClassID == "Ormi2ID");
				if (enemyState2?.Actor != null && !enemyState2.Actor.IsDead)
				{
					yield return new CampaignEffect(CampaignEffectType.Ormi2Survived);
				}
				else
				{
					yield return new CampaignEffect(CampaignEffectType.Ormi2Died);
				}
			}
		}
		else
		{
			if (flag)
			{
				yield return new CampaignEffect(CampaignEffectType.Ormi1Died);
			}
			if (checkOrmi2)
			{
				yield return new CampaignEffect(CampaignEffectType.Ormi2Died);
			}
		}
	}

	public IEnumerator UpdateScenarioCheckpointAsync(ScenarioRuleLibrary.ScenarioState currentState)
	{
		yield return new WaitForTask(UpdateScenarioCheckpoint(currentState));
	}

	public async Task UpdateScenarioCheckpoint(ScenarioRuleLibrary.ScenarioState currentState)
	{
		while (Choreographer.s_Choreographer.HasPendingAnimations)
		{
			await Task.Delay(10);
		}
		if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario)
		{
			return;
		}
		try
		{
			SaveData.Instance.IsSavingData = true;
			switch (SaveData.Instance.Global.GameMode)
			{
			case EGameMode.Campaign:
				AdventureMapState.CurrentMapScenarioState.UpdateAutoSaveState(currentState);
				await SaveData.Instance.AutoSaveCurrentAdventureData();
				break;
			case EGameMode.SingleScenario:
			case EGameMode.FrontEndTutorial:
				AdventureMapState.CurrentSingleScenario.ScenarioState = currentState;
				await SaveData.Instance.AutoSaveCurrentAdventureData();
				break;
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Failed to save Scenario Checkpoint.  Exception: ");
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.IsInitialized)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
			StartStateCompare();
		}
	}

	private void StartStateCompare()
	{
		if (!Choreographer.s_Choreographer.IsFirstTurnPlaying && FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null && PlayerRegistry.MyPlayer.IsParticipant)
		{
			if (SaveData.Instance.Global.MPEndOfTurnCompare)
			{
				Choreographer.s_Choreographer.StartMPEndOfRoundCompare();
			}
			else
			{
				SaveData.Instance.IsSavingData = false;
			}
		}
		else
		{
			SaveData.Instance.IsSavingData = false;
		}
	}

	public PartyAdventureData(SerializationInfo info, StreamingContext context)
	{
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
}

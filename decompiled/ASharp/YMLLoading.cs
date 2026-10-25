using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using FH.YML;
using Ionic.Zip;
using Libs.Threading;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Validation;
using UnityEngine;

public class YMLLoading
{
	public enum EYMLParserType
	{
		None = 0,
		RemoveYML = 1,
		HeroSummon = 2,
		AttackModifier = 3,
		AttackModifierDeck = 4,
		Character = 5,
		AbilityCard = 6,
		ItemCard = 7,
		Perk = 8,
		Enhancements = 9,
		IconGlossary = 10,
		EnemyCard = 11,
		Enemy = 12,
		MonsterData = 13,
		TreasureTable = 14,
		Rooms = 15,
		ScenarioAbility = 16,
		Scenario = 17,
		Difficulty = 18,
		RoadEvent = 19,
		VisibilitySphere = 20,
		Temple = 21,
		Village = 22,
		Quest = 23,
		Message = 24,
		Headquarters = 25,
		CityEvent = 26,
		InitialEvents = 27,
		BattleGoal = 28,
		PersonalQuest = 29,
		StoreLocation = 30,
		ItemConfig = 31,
		AbilityCardConfig = 32,
		CharacterConfig = 33,
		MonsterConfig = 34,
		MapConfig = 35,
		CharacterResources = 36,
		GameMode = 37,
		FHScenario = 38,
		FHItem = 39,
		CampaignSection = 40,
		FHEnhancement = 41,
		Mastery = 42,
		PetCard = 43,
		CustomLevel = 255
	}

	private static readonly EYMLParserType[] EYMLParserTypes = (EYMLParserType[])Enum.GetValues(typeof(EYMLParserType));

	public const string RulesetExtension = ".ruleset";

	private RulebaseFile m_BaseZips;

	private Dictionary<DLCRegistry.EDLCKey, RulebaseFile> m_DLCZips = new Dictionary<DLCRegistry.EDLCKey, RulebaseFile>();

	private const string AudioItemPrefix = "PlaySound_ScenarioUIEquipmentToggle_";

	private const string Default = "Default";

	private static List<IBinarySerializableYMLProcessor> _ymlProcessorCache = null;

	private static Dictionary<EYMLParserType, Type> _ymlDataTypeCache = new Dictionary<EYMLParserType, Type>();

	public bool LastLoadResult { get; private set; }

	public bool InitResult { get; private set; }

	private static string DataPath
	{
		get
		{
			if (!(SceneController.Instance == null))
			{
				return SceneController.Instance.ApplicationDataPath;
			}
			return Application.dataPath;
		}
	}

	public static string RulebaseFolder
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXPlayer)
			{
				return Path.GetFullPath(Path.Combine(DataPath, "..", "Rulebase"));
			}
			return Path.Combine(DataPath, "Rulebase");
		}
	}

	public static string BaseGameRulesetsFolder => Path.Combine(RulebaseFolder, "Base");

	public static string ModdedRulesetsFolder => Path.Combine(RulebaseFolder, "Mods");

	private static string GuildmasterAutoCompleteFileName => "GuildmasterAutoComplete.yml";

	public static string GuildmasterAutoCompletePath => Path.Combine(Application.streamingAssetsPath, "AutoComplete", GuildmasterAutoCompleteFileName);

	private static string CampaignAutoCompleteFileName => "CampaignAutoComplete.yml";

	public static string CampaignAutoCompletePath => Path.Combine(Application.streamingAssetsPath, "AutoComplete", CampaignAutoCompleteFileName);

	public bool IsUnloading { get; private set; }

	public static string GetRulesetPath()
	{
		return Path.Combine(BaseGameRulesetsFolder);
	}

	public static string GetRulesetZipPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "Rulebase", "Base.ruleset");
	}

	public static string GetDlcRulesetPath(DLCRegistry.EDLCKey dlc)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlc));
	}

	public static string GetDlcRulesetZipPath(DLCRegistry.EDLCKey dlc)
	{
		return Path.Combine(DLCPackageFolder(dlc), GloomUtility.GetEnumCategory(dlc) + ".ruleset");
	}

	private static string DLCPackageFolder(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(Application.streamingAssetsPath, "Rulebase", GloomUtility.GetEnumCategory(dlcEnum));
	}

	public static string DLCLanguageCSV(DLCRegistry.EDLCKey dlc)
	{
		return Path.Combine(RulebaseFolder, GloomUtility.GetEnumCategory(dlc), "LangUpdates", "LangUpdate.csv");
	}

	public static string DLCLanguageCSVPackage(DLCRegistry.EDLCKey dlcEnum)
	{
		return Path.Combine(DLCPackageFolder(dlcEnum), "LangUpdate.csv");
	}

	public IEnumerator InitAsyncCrt()
	{
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-YMLLoading => Init()", () =>
		{
			Init();
		});
		yield return threadHandle.WaitForCompletionCrt();
	}

	public bool Init()
	{
		try
		{
			m_BaseZips = new RulebaseFile(GetRulesetZipPath());
			if (DataLoadingFlags.IsBinaryYMLSerializationEnabled)
			{
				GetYMLDataProcessors();
			}
			InitResult = true;
		}
		catch (Exception exception)
		{
			Log.YMLException(exception, "Failed to initialise YML\n");
			InitResult = false;
		}
		return InitResult;
	}

	public void Close()
	{
		m_BaseZips?.Dispose();
		foreach (RulebaseFile value in m_DLCZips.Values)
		{
			value.Dispose();
		}
		m_DLCZips?.Clear();
	}

	public static bool CompileRuleset(string rulesetPath, string zipPath, bool onlyIfOutdated, List<string> includeList = null, bool compileBinary = false)
	{
		bool result = true;
		try
		{
			if (onlyIfOutdated && File.Exists(zipPath) && File.GetLastWriteTime(zipPath) >= GetRulesetLastWriteTime(rulesetPath))
			{
				return true;
			}
		}
		catch (Exception exception)
		{
			Log.YMLException(exception, "An exception occurred while checking if yml rulebase expired.");
			return false;
		}
		try
		{
			RulebaseFile.Compile(rulesetPath, zipPath, includeList, compileBinary);
		}
		catch (Exception exception2)
		{
			Log.YMLException(exception2, "An exception occurred while compiling a ruleset.\n");
			result = false;
		}
		return result;
	}

	public static bool CopyLangUpdatesToDLCPackage(string csvPath, string outputPath)
	{
		bool result = true;
		try
		{
			if (File.Exists(outputPath))
			{
				File.Delete(outputPath);
			}
			if (!File.Exists(csvPath))
			{
				result = false;
			}
			else
			{
				File.Copy(csvPath, outputPath);
			}
		}
		catch (Exception exception)
		{
			Log.YMLException(exception, "An exception occurred while copy a LangUpdate csv.\n");
			result = false;
		}
		return result;
	}

	public bool LoadAll()
	{
		DateTime now = DateTime.Now;
		CSRLYML.IsLoading = true;
		CSRLYML.AllAbilities.Clear();
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.None;
		LastLoadResult = m_BaseZips.ProcessFiles();
		if (LastLoadResult)
		{
			foreach (RulebaseFile value in m_DLCZips.Values)
			{
				LastLoadResult = value.ProcessFiles();
				if (!LastLoadResult)
				{
					Log.YMLError("Failed to load All DLC YML data");
					CSRLYML.IsLoading = false;
					return false;
				}
			}
			ScenarioRuleClient.SRLYML.FinishDataLoad();
			MapRuleLibraryClient.MRLYML.FinishDataLoad();
			Log.YMLInfo("Finished loading all YML. Duration: " + (DateTime.Now - now).TotalSeconds);
			now = DateTime.Now;
			TempStringBuilder.CleanPool();
			CSRLYML.IsLoading = false;
			return LastLoadResult;
		}
		Log.YMLError("Failed to load All YML data");
		CSRLYML.IsLoading = false;
		return false;
	}

	public IEnumerator LoadSingleScenariosAsyncCrt(ScenarioManager.EDLLMode mode)
	{
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-YMLLoading => LoadSingleScenarios()", () =>
		{
			SceneController.Instance.YML.LoadSingleScenarios(mode);
		});
		yield return threadHandle.WaitForCompletionCrt();
	}

	public bool LoadSingleScenarios(ScenarioManager.EDLLMode mode)
	{
		LastLoadResult = true;
		if (mode == ScenarioManager.EDLLMode.Campaign || mode == ScenarioManager.EDLLMode.CustomScenario)
		{
			if (!LoadCampaign())
			{
				Log.YMLError("Failed to load Campaign Data");
				LastLoadResult = false;
			}
		}
		else
		{
			Log.YMLError("Invalid mode " + mode.ToString() + " sent to load single scenarios");
			LastLoadResult = false;
		}
		return LastLoadResult;
	}

	public IEnumerator LoadCampaignAsyncCrt()
	{
		IThreadHandle threadHandle = ThreadManagerComponent.RunThread("GH-YMLLoading => LoadCampaign()", () =>
		{
			SceneController.Instance.YML.LoadCampaign();
		});
		yield return threadHandle.WaitForCompletionCrt();
	}

	public bool LoadCampaign()
	{
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.Campaign;
		try
		{
			MapRuleLibraryClient.Instance.Reset();
			MapRuleLibraryClient.Instance.Start();
		}
		catch (Exception exception)
		{
			Log.YMLException(exception, "Exception when loading ScenarioRuleClient data.\n");
			return false;
		}
		return true;
	}

	public void ReturnToMainMenu()
	{
		IsUnloading = true;
		CoroutineHelper.RunCoroutine(ReturnToMainMenuCoroutine());
	}

	private IEnumerator ReturnToMainMenuCoroutine()
	{
		SceneController.Instance.ShowLoadingScreen();
		yield return null;
		MapRuleLibraryClient.MRLYML.MapMode = ScenarioManager.EDLLMode.None;
		AsyncOperation asyncOp = Resources.UnloadUnusedAssets();
		while (!asyncOp.isDone)
		{
			yield return null;
		}
		IsUnloading = false;
		SceneController.Instance.DisableLoadingScreen();
	}

	private bool ProcessCustomLevel(ZipEntry entry)
	{
		bool flag = true;
		using MemoryStream memoryStream = new MemoryStream();
		entry.Extract(memoryStream);
		flag = ProcessCustomLevel(memoryStream, entry.FileName);
		memoryStream.Close();
		return flag;
	}

	public static bool ProcessCustomLevel(MemoryStream stream, string fileName)
	{
		bool result = true;
		using StreamReader streamReader = new StreamReader(stream);
		streamReader.DiscardBufferedData();
		streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		ScenarioRuleClient.SRLYML.Data.CustomLevels.Enqueue(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(fileName), stream.ToArray()));
		return result;
	}

	public static bool ProcessCustomLevel(string file)
	{
		ScenarioRuleClient.SRLYML.Data.CustomLevels.Enqueue(new Tuple<string, byte[]>(Path.GetFileNameWithoutExtension(file), File.ReadAllBytes(file)));
		return true;
	}

	private bool ProcessAutoComplete(ScenarioManager.EDLLMode mode)
	{
		bool result = true;
		string campaignAutoCompletePath = CampaignAutoCompletePath;
		if (!MapRuleLibraryClient.MRLYML.Data.AutoCompletes.ProcessFile(campaignAutoCompletePath))
		{
			result = false;
			ShowYMLErrors(campaignAutoCompletePath, null);
		}
		return result;
	}

	public static bool ProcessYMLFile(MemoryStream stream, string fileName, string writeErrorsToFile = null)
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		bool result = true;
		using (StreamReader streamReader = new StreamReader(stream))
		{
			streamReader.DiscardBufferedData();
			streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			string text = streamReader.ReadLine();
			switch (GetParserType(text, fileName))
			{
			case EYMLParserType.None:
			{
				string text2 = "No parser specified for file " + fileName;
				Log.YMLError(text2);
				if (writeErrorsToFile != null)
				{
					WriteValidationToFile(writeErrorsToFile, text2);
				}
				result = false;
				break;
			}
			case EYMLParserType.HeroSummon:
				if (!ScenarioRuleClient.SRLYML.Data.HeroSummons.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Character:
				if (!ScenarioRuleClient.SRLYML.Data.Characters.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.AbilityCard:
				if (!ScenarioRuleClient.SRLYML.Data.AbilityCards.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.ItemCard:
				if (!ScenarioRuleClient.SRLYML.Data.ItemCards.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.PetCard:
				if (!ScenarioRuleClient.SRLYML.Data.PetCards.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Perk:
				if (!ScenarioRuleClient.SRLYML.Data.Perks.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Enhancements:
				if (!ScenarioRuleClient.SRLYML.Data.Enhancements.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.IconGlossary:
				if (!ScenarioRuleClient.SRLYML.Data.IconGlossary.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.AttackModifier:
				if (!ScenarioRuleClient.SRLYML.Data.AttackModifiers.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.AttackModifierDeck:
				if (!ScenarioRuleClient.SRLYML.Data.AttackModifierDecks.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.EnemyCard:
				if (!ScenarioRuleClient.SRLYML.Data.MonsterCards.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Enemy:
				if (!ScenarioRuleClient.SRLYML.Data.Monsters.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.TreasureTable:
				if (!ScenarioRuleClient.SRLYML.Data.TreasureTables.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Scenario:
				if (!ScenarioRuleClient.SRLYML.Data.Scenarios.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.MonsterData:
				if (!ScenarioRuleClient.SRLYML.Data.MonsterData.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Rooms:
				if (!ScenarioRuleClient.SRLYML.Data.ScenarioRooms.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.ScenarioAbility:
				if (!ScenarioRuleClient.SRLYML.Data.ScenarioAbilities.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.CharacterResources:
				if (!ScenarioRuleClient.SRLYML.Data.CharacterResources.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.ItemConfig:
				if (!ScenarioRuleClient.SRLYML.Data.ItemConfigs.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.AbilityCardConfig:
				if (!ScenarioRuleClient.SRLYML.Data.AbilityCardConfigs.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.CharacterConfig:
				if (!ScenarioRuleClient.SRLYML.Data.CharacterConfigs.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.MonsterConfig:
				if (!ScenarioRuleClient.SRLYML.Data.MonsterConfigs.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.MapConfig:
				if (!ScenarioRuleClient.SRLYML.Data.MapConfigs.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Difficulty:
				if (!MapRuleLibraryClient.MRLYML.Data.Difficulty.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.BattleGoal:
				if (!MapRuleLibraryClient.MRLYML.Data.BattleGoals.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Mastery:
				if (!MapRuleLibraryClient.MRLYML.Data.Masteries.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.PersonalQuest:
				if (!MapRuleLibraryClient.MRLYML.Data.PersonalQuests.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.RoadEvent:
				if (!MapRuleLibraryClient.MRLYML.Data.RoadEvents.ProcessFile(streamReader, fileName, "RoadEvent"))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.CityEvent:
				if (!MapRuleLibraryClient.MRLYML.Data.CityEvents.ProcessFile(streamReader, fileName, "CityEvent"))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.InitialEvents:
				if (!MapRuleLibraryClient.MRLYML.Data.InitialEvents.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.VisibilitySphere:
				if (!MapRuleLibraryClient.MRLYML.Data.VisibilitySpheres.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Temple:
				if (!MapRuleLibraryClient.MRLYML.Data.Temples.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Village:
				if (!MapRuleLibraryClient.MRLYML.Data.Villages.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.StoreLocation:
				if (!MapRuleLibraryClient.MRLYML.Data.StoreLocations.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Quest:
				if (!MapRuleLibraryClient.MRLYML.Data.Quests.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Message:
				if (!MapRuleLibraryClient.MRLYML.Data.MapMessages.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.Headquarters:
				if (!MapRuleLibraryClient.MRLYML.Data.Headquarters.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.GameMode:
				if (!YMLDefRepository.Repo.GameModes.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.FHScenario:
				if (!YMLDefRepository.Repo.FHScenarios.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.FHItem:
				if (!YMLDefRepository.Repo.FHItems.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.CampaignSection:
				if (!YMLDefRepository.Repo.CampaignSections.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			case EYMLParserType.FHEnhancement:
				if (!YMLDefRepository.Repo.FHEnhancements.ProcessFile(streamReader, fileName))
				{
					result = false;
					ShowYMLErrors(fileName, writeErrorsToFile);
				}
				break;
			default:
				result = false;
				Log.YMLError("Invalid parser type " + text + " for Global Data in file " + fileName);
				break;
			}
		}
		return result;
	}

	public static IEnumerable<object> GetYMLDataFromType(EYMLParserType type)
	{
		if (type == EYMLParserType.EnemyCard)
		{
			return ScenarioRuleClient.SRLYML.MonsterCards;
		}
		if (type == EYMLParserType.Enemy)
		{
			return ScenarioRuleClient.SRLYML.Monsters;
		}
		if (type == EYMLParserType.ScenarioAbility)
		{
			return ScenarioRuleClient.SRLYML.ScenarioAbilities;
		}
		PropertyInfo propertyInfo = ScenarioRuleClient.SRLYML.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((PropertyInfo p) => p.Name == type.ToString() || p.Name == type.ToString() + "s");
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(ScenarioRuleClient.SRLYML) as IEnumerable<object>;
		}
		propertyInfo = MapRuleLibraryClient.MRLYML.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((PropertyInfo p) => p.Name == type.ToString() || p.Name == type.ToString() + "s");
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(MapRuleLibraryClient.MRLYML) as IEnumerable<object>;
		}
		propertyInfo = YMLDefRepository.Repo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((PropertyInfo p) => p.Name == type.ToString() || p.Name == type.ToString() + "s");
		if (propertyInfo != null)
		{
			return (propertyInfo.GetValue(YMLDefRepository.Repo) as IGenericYMLRepo).GetAllItems();
		}
		return null;
	}

	private static List<IBinarySerializableYMLProcessor> GetYMLDataProcessors()
	{
		if (_ymlProcessorCache != null)
		{
			return _ymlProcessorCache;
		}
		List<IBinarySerializableYMLProcessor> list = new List<IBinarySerializableYMLProcessor>();
		PropertyInfo[] properties = ScenarioRuleClient.SRLYML.Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (typeof(IBinarySerializableYMLProcessor).IsAssignableFrom(propertyInfo.PropertyType))
			{
				list.Add((IBinarySerializableYMLProcessor)propertyInfo.GetValue(ScenarioRuleClient.SRLYML.Data));
			}
		}
		properties = MapRuleLibraryClient.MRLYML.Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo2 in properties)
		{
			if (typeof(IBinarySerializableYMLProcessor).IsAssignableFrom(propertyInfo2.PropertyType))
			{
				list.Add((IBinarySerializableYMLProcessor)propertyInfo2.GetValue(ScenarioRuleClient.SRLYML.Data));
			}
		}
		properties = YMLDefRepository.Repo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo3 in properties)
		{
			if (typeof(IBinarySerializableYMLProcessor).IsAssignableFrom(propertyInfo3.PropertyType))
			{
				list.Add((IBinarySerializableYMLProcessor)propertyInfo3.GetValue(YMLDefRepository.Repo));
			}
		}
		_ymlProcessorCache = list;
		return list;
	}

	public static Type GetYMLDataType(EYMLParserType parser)
	{
		if (_ymlDataTypeCache.TryGetValue(parser, out var value))
		{
			return value;
		}
		IEnumerable<object> yMLDataFromType = GetYMLDataFromType(parser);
		value = ((yMLDataFromType != null) ? yMLDataFromType.GetType().GenericTypeArguments[0] : null);
		_ymlDataTypeCache.Add(parser, value);
		return value;
	}

	public static void AddYMLDataFromType(object obj, string fileName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (obj is BinarySerializableYMLData binarySerializableYMLData)
		{
			foreach (string item in binarySerializableYMLData.RawYML)
			{
				stringBuilder.AppendLine(item);
			}
		}
		string text = stringBuilder.ToString();
		IBinarySerializableYMLProcessor binarySerializableYMLProcessor = GetYMLDataProcessors().FirstOrDefault((IBinarySerializableYMLProcessor p) => p.YMLDataType == obj.GetType());
		if (binarySerializableYMLProcessor == null)
		{
			throw new InvalidDataException();
		}
		if (!string.IsNullOrEmpty(text))
		{
			binarySerializableYMLProcessor.ProcessRawData(fileName, obj, text);
		}
		binarySerializableYMLProcessor.AddToDataSource(obj);
	}

	private static void ShowYMLErrors(string fileName, string writeErrorsToFile)
	{
		string text = "Failed to parse file " + fileName + "\n" + string.Join("\n", SharedClient.ValidationRecord.RecordedFailures.Select((YMLValidationRecord.CYMLValidationFailure s) => s.Message));
		Log.YMLError(text);
		if (writeErrorsToFile != null)
		{
			WriteValidationToFile(writeErrorsToFile, text);
		}
		SharedClient.ValidationRecord.RecordedFailures.Clear();
	}

	public static EYMLParserType GetParserType(string parserLine, string fileName, string writeErrorsToFile = null)
	{
		try
		{
			string parser = parserLine.Split(':')[1].Trim();
			return EYMLParserTypes.SingleOrDefault((EYMLParserType s) => s.ToString() == parser);
		}
		catch
		{
			string text = "Invalid Parser " + parserLine + " in file " + fileName;
			Log.YMLError(text);
			if (writeErrorsToFile != null)
			{
				WriteValidationToFile(writeErrorsToFile, text);
			}
			return EYMLParserType.None;
		}
	}

	private static string GetSafeFileName(ZipFile zip, string file, string zipFolder)
	{
		int num = 0;
		string text = file;
		while (zip.EntryFileNames.Contains(zipFolder + "/" + Path.GetFileName(text)))
		{
			num++;
			text = file.Insert(file.LastIndexOf("."), num.ToString());
		}
		return text;
	}

	public static void WriteValidationToFile(string filePath, string appendText)
	{
		try
		{
			File.AppendAllText(filePath, appendText + "\n");
		}
		catch (Exception exception)
		{
			Log.YMLException(exception, "Exception running YMLLoading.WriteValidationToFile.\n");
		}
	}

	public static bool GetYMLParserTypes(string[] ymlFiles, out Dictionary<string, List<string>> filesByParser, string writeErrorsToFile = null)
	{
		filesByParser = new Dictionary<string, List<string>>();
		foreach (string text in ymlFiles)
		{
			string text2 = GetParserType(File.ReadLines(text).First(), text).ToString();
			if (text2 == "None")
			{
				if (writeErrorsToFile != null)
				{
					WriteValidationToFile(writeErrorsToFile, "Invalid parser type in file " + text);
				}
				return false;
			}
			if (filesByParser.ContainsKey(text2))
			{
				filesByParser[text2].Add(text);
				continue;
			}
			filesByParser[text2] = new List<string> { text };
		}
		return true;
	}

	private static DateTime GetRulesetLastWriteTime(string rulesetPath)
	{
		DateTime dateTime = default;
		string[] files = Directory.GetFiles(rulesetPath, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			DateTime lastWriteTime = File.GetLastWriteTime(files[i]);
			if (lastWriteTime > dateTime)
			{
				dateTime = lastWriteTime;
			}
		}
		foreach (string item in Directory.GetDirectories(rulesetPath, "*.*", SearchOption.AllDirectories).Append(rulesetPath))
		{
			DateTime lastWriteTime2 = Directory.GetLastWriteTime(item);
			if (lastWriteTime2 > dateTime)
			{
				dateTime = lastWriteTime2;
			}
		}
		return dateTime;
	}
}

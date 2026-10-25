using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Scenario.DTO;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingScenarioMechanicsInfo : MonoBehaviour
{
	[SerializeField]
	private ScenarioMechanicsInfoModule InfoModule;

	[Header("Helper Controls")]
	[SerializeField]
	private TMP_Dropdown m_ScenarioLevelsList;

	[SerializeField]
	private FHButton m_Prev;

	[SerializeField]
	private FHButton m_Next;

	[SerializeField]
	private FHButton m_Export;

	private List<string> m_ScenarioIds = new List<string>();

	private int m_selectedScenario;

	private void Awake()
	{
		m_Prev.onClick.AddListener(OnPrevEvent);
		m_Next.onClick.AddListener(OnNextEvent);
		m_Export.onClick.AddListener(ExportScenarioRulesAsCSV);
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(3f);
		yield return CSRLYML.IsLoading;
		yield return StartCoroutine(SetupTestScenarioEnvironment());
		ExtractScenarioData();
		PopulateOptions();
		OnEventSelected(0);
	}

	private IEnumerator SetupTestScenarioEnvironment()
	{
		yield return UIInfoTools.InitInstanceCrt();
		yield return GlobalSettings.InitInstanceCrt();
		DataLoadingFlags.HandleAbilitiesOnMainThread = true;
		MonsterClassManager.Load();
		CharacterClassManager.Load();
		ScenarioManager.Load(CreateMockScenario(), firstLoad: true);
		ScenarioRuleClient.SetMessageHandler((CMessageData message) =>
		{
		});
	}

	private void ExtractScenarioData()
	{
		foreach (ScenarioDef item in YMLDefRepository.Repo.FHScenarios.Items)
		{
			m_ScenarioIds.Add(item.LevelID);
		}
		m_ScenarioIds.Sort();
	}

	private void PopulateOptions()
	{
		if ((object)m_ScenarioLevelsList == null)
		{
			return;
		}
		m_ScenarioLevelsList.ClearOptions();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>(m_ScenarioIds.Count);
		foreach (string scenarioId in m_ScenarioIds)
		{
			list.Add(new TMP_Dropdown.OptionData(scenarioId));
		}
		m_ScenarioLevelsList.AddOptions(list);
		m_ScenarioLevelsList.onValueChanged.AddListener((int index) =>
		{
			OnEventSelected(index);
		});
	}

	private void UpdateControls()
	{
		m_Prev.interactable = m_selectedScenario > 0;
		m_Next.interactable = m_selectedScenario < m_ScenarioIds.Count;
	}

	private void OnEventSelected(int index)
	{
		m_selectedScenario = index;
		CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(m_ScenarioIds[index]);
		if (customLevel == null)
		{
			InfoModule.gameObject.SetActive(value: false);
			return;
		}
		InfoModule.gameObject.SetActive(value: true);
		foreach (CObjective winObjective in customLevel.ScenarioState.WinObjectives)
		{
			typeof(CObjective).GetProperty("IsActive")?.SetValue(winObjective, true);
		}
		int.TryParse(m_ScenarioIds[index].Split("_").Skip(1).FirstOrDefault(), out var result);
		string scenarioNameKey = $"FS_{result}_NAME";
		List<ScenarioMechanicsConfig.Entry> list = InfoModule.MechanicsData.Entries.Where((ScenarioMechanicsConfig.Entry entry) => entry.ID.Contains(m_ScenarioIds[index])).ToList();
		List<ScenarioMechanicDTO> list2 = new List<ScenarioMechanicDTO>();
		foreach (ScenarioMechanicsConfig.Entry item in list)
		{
			list2.Add(new ScenarioMechanicDTO
			{
				ID = item.ID,
				Content = item.Content,
				StyleCategory = item.StyleCategory,
				Seen = false
			});
		}
		InfoModule.Init(new ScenarioMechanicsInfoModule.Params
		{
			LevelID = m_ScenarioIds[index],
			ScenarioNameKey = scenarioNameKey,
			WinObjectives = customLevel.ScenarioState.WinObjectives,
			LoseObjectives = customLevel.ScenarioState.LoseObjectives,
			ScenarioRules = list2,
			Challenges = GetRandomTestChallenges(),
			Favors = GetRandomTestFavors()
		});
		UpdateControls();
	}

	private List<SuggestibleSlotDTO> GetRandomTestFavors()
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		List<ScenarioFavorType> list2 = (from ScenarioFavorType type in Enum.GetValues(typeof(ScenarioFavorType))
			where type != ScenarioFavorType.None && type != ScenarioFavorType.Potential
			select type).ToList();
		list2.Shuffle();
		foreach (ScenarioFavorType item in list2.Take(UnityEngine.Random.Range(0, 3)).ToList())
		{
			FavorConfigUI favorConfig = UIInfoTools.Instance.GetFavorConfig(item);
			if (favorConfig == null)
			{
				Log.Info($"Missing config for {item}");
				continue;
			}
			list.Add(new SuggestibleSlotDTO
			{
				NameLiteral = LocalizationManager.GetTranslation(favorConfig.Name),
				InfoLiteral = LocalizationManager.GetTranslation(favorConfig.Description),
				Image = favorConfig.Icon
			});
		}
		return list;
	}

	private List<SuggestibleSlotDTO> GetRandomTestChallenges()
	{
		List<SuggestibleSlotDTO> list = new List<SuggestibleSlotDTO>();
		foreach (ChallengeDef item in YMLDefRepository.Repo.DefRepository.Challenges.Values.OrderBy((ChallengeDef _) => Guid.NewGuid()).Take(UnityEngine.Random.Range(0, 3)).ToList())
		{
			uint iD = item.ID.ID;
			string id = iD.ToString();
			list.Add(new SuggestibleSlotDTO
			{
				Id = id,
				NameLiteral = LocalizationManager.GetTranslation(item.NameLocKey),
				InfoLiteral = LocalizationManager.GetTranslation(item.DescriptionLocKey),
				Interactable = false
			});
		}
		return list;
	}

	private void OnNextEvent()
	{
		if (m_selectedScenario + 1 < m_ScenarioIds.Count)
		{
			m_selectedScenario++;
			m_ScenarioLevelsList.value = m_selectedScenario;
		}
	}

	private void OnPrevEvent()
	{
		if (m_selectedScenario - 1 >= 0)
		{
			m_selectedScenario--;
			m_ScenarioLevelsList.value = m_selectedScenario;
		}
	}

	private ScenarioRuleLibrary.ScenarioState CreateMockScenario()
	{
		List<CMap> list = new List<CMap>
		{
			new CMap("Room", EMapType.A, "ScenarioRoom", null, "")
			{
				MapTiles = 
				{
					new CMapTile(edgeTile: false, new TileIndex(0, 0), GloomUtility.VToCV(new Vector3(0f, 0f, 0f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(0, 1), GloomUtility.VToCV(new Vector3(0f, 0f, 1f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(1, 0), GloomUtility.VToCV(new Vector3(1f, 0f, 0f)), SharedClient.GlobalRNG),
					new CMapTile(edgeTile: false, new TileIndex(1, 1), GloomUtility.VToCV(new Vector3(1f, 0f, 1f)), SharedClient.GlobalRNG)
				}
			}
		};
		CMap.SetChildren(list);
		foreach (CMap item in list)
		{
			item.Revealed = true;
		}
		ApparanceStyle style = new ApparanceStyle(ScenarioPossibleRoom.EBiome.Crypt);
		ScenarioRuleLibrary.ScenarioState scenarioState = new ScenarioRuleLibrary.ScenarioState("Name", "Description", "ID", SharedClient.GlobalRNG.Next(), 1, EScenarioType.Custom, "FileName", new List<CObjective>(), new List<CObjective>(), new List<CScenarioModifier>(), style, new List<string>(), new List<string>())
		{
			Maps = list
		};
		typeof(ScenarioRuleLibrary.ScenarioState).GetProperty("Width", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
		typeof(ScenarioRuleLibrary.ScenarioState).GetProperty("Height", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
		return scenarioState;
	}

	private void OnDestroy()
	{
		ScenarioManager.Unload();
	}

	private void ExportScenarioRulesAsCSV()
	{
		Debug.Log("Export !");
		List<ScenarioDef> list = YMLDefRepository.Repo.FHScenarios.Items.OrderBy((ScenarioDef def) => def.LevelID).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ScenarioDef item in list)
		{
			CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(item.LevelID);
			if (customLevel == null)
			{
				Log.Warning("fail top load data for " + item.LevelID);
				continue;
			}
			foreach (string item2 in ToCSVRows(item.LevelID, customLevel))
			{
				stringBuilder.AppendLine(item2);
			}
		}
		try
		{
			File.WriteAllText("../TestScenarioSpecialRules.csv", stringBuilder.ToString());
		}
		catch (Exception)
		{
			Log.Error("Fail to Export Default Scenario Special Rules CSV");
		}
		static IEnumerable<string> ToCSVRows(string LevelID, CCustomLevelData levelData)
		{
			foreach (CScenarioModifier scenarioModifier in levelData.ScenarioState.ScenarioModifiers)
			{
				if (!string.IsNullOrEmpty(scenarioModifier.LocKey))
				{
					yield return $"{LevelID},{scenarioModifier.LocKey},{scenarioModifier.ActivationRound},{scenarioModifier.ScenarioModifierType},{scenarioModifier.IsHidden},{LocalizationManager.GetTranslation(scenarioModifier.LocKey)}";
				}
			}
		}
	}
}

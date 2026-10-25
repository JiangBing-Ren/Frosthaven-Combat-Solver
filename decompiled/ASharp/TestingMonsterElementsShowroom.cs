using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AStar;
using FHGUI.Common;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestingMonsterElementsShowroom : MonoBehaviour
{
	[Header("Data")]
	public MonsterUIDataSO MonsterUIDataSo;

	[Header("Controls")]
	public Button ButtonPrev;

	public Button ButtonNext;

	public TMP_Dropdown DropdownClass;

	public TMP_Dropdown DropdownPartySize;

	public TMP_Dropdown DropdownMonsterLevel;

	public Toggle ToggleOnlyRuntimeCards;

	public RectTransform CardsList;

	public InitiativeTrackActorBehaviourBase InitiativeTrackElement;

	public UIGeneralInfoMonster GeneralInfoMonster;

	private string[] _monsterNames;

	private List<TestingMonsterCardPairElement> _pairs = new List<TestingMonsterCardPairElement>();

	private int _monsterId;

	private int _level;

	private List<CPlayerActor> _actors = new List<CPlayerActor>();

	private List<PlayerState> _playerStates = new List<PlayerState>();

	private List<CMap> _maps = new List<CMap>();

	private const string PREFABS_PATH = "Assets/Prefabs/GUI/Monster Cards/";

	private void Awake()
	{
		_monsterNames = (from m in MonsterClassManager.Classes
			where m.Boss
			select m.MonsterYML.ID).ToArray();
		_pairs.AddRange(CardsList.GetComponentsInChildren<TestingMonsterCardPairElement>());
		StartCoroutine(DelayedStart());
	}

	private IEnumerator DelayedStart()
	{
		yield return null;
		yield return null;
		yield return UIInfoTools.InitInstanceCrt();
		DropdownClass.ClearOptions();
		DropdownClass.AddOptions(_monsterNames.Select((string m) => new TMP_Dropdown.OptionData(m)).ToList());
		DropdownClass.onValueChanged.AddListener(ChangeMonster);
		DropdownClass.SetValueWithoutNotify(0);
		CreateMockScenario();
		DropdownMonsterLevel.ClearOptions();
		DropdownMonsterLevel.AddOptions(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });
		DropdownMonsterLevel.onValueChanged.AddListener(ChangeMonsterLevel);
		DropdownMonsterLevel.SetValueWithoutNotify(0);
		DropdownPartySize.ClearOptions();
		DropdownPartySize.AddOptions(new List<string> { "2", "3", "4" });
		DropdownPartySize.onValueChanged.AddListener(ChangePartySize);
		DropdownPartySize.SetValueWithoutNotify(0);
		ChangePartySize(0);
		ButtonPrev.onClick.AddListener(PrevMonster);
		ButtonNext.onClick.AddListener(NextMonster);
		ToggleOnlyRuntimeCards.onValueChanged.AddListener((bool _) =>
		{
			ChangeMonster(DropdownClass.value);
		});
		ChangeMonster(0);
	}

	private void CreateMockScenario()
	{
		try
		{
			_maps = new List<CMap>
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
			CMap.SetChildren(_maps);
			foreach (CMap map in _maps)
			{
				map.Revealed = true;
			}
			ApparanceStyle style = new ApparanceStyle(ScenarioPossibleRoom.EBiome.Crypt);
			ScenarioState scenarioState = new ScenarioState("Name", "Description", "ID", SharedClient.GlobalRNG.Next(), _level, EScenarioType.Custom, "FileName", new List<CObjective>(), new List<CObjective>(), new List<CScenarioModifier>(), style, new List<string>(), new List<string>())
			{
				Maps = _maps
			};
			typeof(ScenarioState).GetProperty("Width", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
			typeof(ScenarioState).GetProperty("Height", BindingFlags.Instance | BindingFlags.Public).SetValue(scenarioState, 1);
			ScenarioManager.Load(scenarioState, firstLoad: true);
			ScenarioManager.CurrentScenarioState.ScenarioSetup = FHScenarioSetup.MakeDummy(new SharedLibrary.Random());
			ScenarioRuleClient.SetMessageHandler((CMessageData _) =>
			{
			});
			CCharacterClass characterClass = CharacterClassManager.Classes.FirstOrDefault((CCharacterClass c) => c.CharacterID.Contains("Drifter", StringComparison.InvariantCultureIgnoreCase));
			_actors.Add(new CPlayerActor(new Point(0, 0), characterClass, 1, 1, 1, Guid.Empty.ToString(), 0, "ID1", 0, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, 0, 0, 0));
			_actors.Add(new CPlayerActor(new Point(0, 1), characterClass, 1, 1, 1, Guid.Empty.ToString(), 0, "ID2", 0, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, 0, 0, 0));
			_actors.Add(new CPlayerActor(new Point(1, 0), characterClass, 1, 1, 1, Guid.Empty.ToString(), 0, "ID3", 0, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, 0, 0, 0));
			_actors.Add(new CPlayerActor(new Point(1, 1), characterClass, 1, 1, 1, Guid.Empty.ToString(), 0, "ID4", 0, CPlayerActor.EAgility.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, CPlayerActor.EForm.None, 0, 0, 0));
			_playerStates.Add(new PlayerState(_actors[0], _maps[0].MapGuid));
			_playerStates.Add(new PlayerState(_actors[1], _maps[0].MapGuid));
			_playerStates.Add(new PlayerState(_actors[2], _maps[0].MapGuid));
			_playerStates.Add(new PlayerState(_actors[3], _maps[0].MapGuid));
		}
		catch
		{
		}
	}

	private void ChangePartySize(int arg0)
	{
		ScenarioManager.CurrentScenarioState.OverridePlayersInState(_playerStates.Take(2 + arg0).ToList());
		ChangeMonster(_monsterId);
	}

	private void ChangeMonsterLevel(int arg0)
	{
		_level = arg0;
		ScenarioManager.CurrentScenarioState.SetScenarioLevel(_level);
		ChangeMonster(_monsterId);
	}

	private void ChangeMonster(int index)
	{
		_monsterId = index;
		string monsterID = _monsterNames[index];
		CMonsterClass cMonsterClass = MonsterClassManager.Classes.First((CMonsterClass m) => m.ID == monsterID);
		cMonsterClass.SetMonsterStatLevel(_level);
		HelperTools.NormalizePool(ref _pairs, _pairs.First().gameObject, CardsList, cMonsterClass.MonsterYML.AbilityCards?.Count ?? 0);
		for (int num = 0; num < cMonsterClass.MonsterYML.AbilityCards.Count; num++)
		{
			MonsterCardYMLData monsterCardYMLData = cMonsterClass.MonsterYML.AbilityCards[num];
			_pairs[num].Init(monsterCardYMLData, InstantiateCardGO(cMonsterClass, monsterCardYMLData, generated: false), InstantiateCardGO(cMonsterClass, monsterCardYMLData, generated: false));
		}
		InitiativeTrack.ActorAttributesParams attributesDirect = new InitiativeTrack.ActorAttributesParams
		{
			Class = cMonsterClass,
			TextInitiative = "99",
			CurrentHealth = 1,
			MaxHealth = 1
		};
		InitiativeTrackElement.SetAttributesDirect(attributesDirect);
		UIGeneralInfoMonster.MonsterGeneralInfoData monster = UIGeneralInfoUtils.GetMonster(cMonsterClass);
		GeneralInfoMonster.Hide();
		GeneralInfoMonster.Show(monster);
	}

	private int GetMonsterIndexByStep(int step)
	{
		int value = DropdownClass.value;
		int num = _monsterNames.Length - 1;
		int num2 = value + step;
		if (num2 > num)
		{
			num2 = 0;
		}
		else if (num2 < 0)
		{
			num2 = num;
		}
		return num2;
	}

	private void NextMonster()
	{
		DropdownClass.value = GetMonsterIndexByStep(1);
	}

	private void PrevMonster()
	{
		DropdownClass.value = GetMonsterIndexByStep(-1);
	}

	private GameObject InstantiateCardGO(CMonsterClass monster, MonsterCardYMLData cardData, bool generated)
	{
		return null;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AStar;
using Chronos;
using DevLocker.GFrame.Input.Contexts;
using DevLocker.GFrame.Input.UIInputDisplay;
using FFSNet;
using FHGUI.Elements;
using FHGUI.Scenario;
using FHGUI.Scenario.Elements;
using FHGUI.Scenario.Modules;
using FHRL;
using Libs.GameConsole;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;

public class DebugMenu : MonoBehaviour
{
	public enum CaptureTileFunctions
	{
		None,
		SpawnMonster,
		SelectCharacter,
		SpawnProp,
		SpawnSpawner,
		SpawnObject,
		LOSSourceTile,
		LOSTargetTile,
		SetPosition,
		SetFacing
	}

	private class DisplayText
	{
		private float startTime;

		private Rect area;

		public bool Show { get; set; }

		public float ElapsedTime => Time.realtimeSinceStartup - startTime;

		public int ID { get; private set; }

		public int Position { get; private set; }

		public string Text { get; private set; }

		public Rect Area => area;

		public DisplayText(int id, string text)
		{
			ID = id;
			Text = text;
			Show = true;
			startTime = Time.realtimeSinceStartup;
			area = new Rect(Screen.width - 400, 20f, 300f, 20f);
		}

		public void UpdateText(string text)
		{
			Text = text;
			startTime = Time.realtimeSinceStartup;
		}

		public void UpdatePosition(int position)
		{
			Position = position;
			area.y = position * 20;
		}
	}

	public class DisplayTextList
	{
		private List<DisplayText> displayTexts;

		public float DisplayTextTime { get; set; }

		public DisplayTextList()
		{
			displayTexts = new List<DisplayText>();
			DisplayTextTime = 3f;
		}

		public void Add(int id, string text)
		{
			DisplayText displayText = displayTexts.SingleOrDefault((DisplayText x) => x.ID == id);
			if (displayText == null)
			{
				DisplayText displayText2 = new DisplayText(id, text);
				displayTexts.Add(displayText2);
				displayText2.UpdatePosition(displayTexts.Count);
			}
			else
			{
				displayText.UpdateText(text);
			}
		}

		public void Update()
		{
			if (displayTexts.Count <= 0)
			{
				return;
			}
			bool flag = false;
			foreach (DisplayText displayText in displayTexts)
			{
				GUILayout.BeginArea(displayText.Area);
				GUILayout.Label(new GUIContent(displayText.Text));
				GUILayout.EndArea();
				if (displayText.ElapsedTime > DisplayTextTime)
				{
					displayText.Show = false;
					flag = true;
				}
			}
			if (flag)
			{
				displayTexts.RemoveAll((DisplayText x) => !x.Show);
				for (int num = 0; num < displayTexts.Count; num++)
				{
					displayTexts[num].UpdatePosition(num + 1);
				}
			}
		}
	}

	private class RefComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	[Header("Defaults")]
	public bool FollowCameraDisabled;

	[Header("Internals")]
	public Sprite QUISprite;

	public TextMeshProUGUI MenuTitle;

	public GameObject Frame;

	public GameObject ScenarioOptions;

	public GameObject DebugKeys;

	public UnityEngine.UI.Toggle FollowCamToggle;

	public TMP_InputField SetCameraHeightText;

	public AutoCompleteComboBox SpawnMonsterDropdown;

	public TMP_Dropdown SpawnMonsterLevelDropdown;

	public UnityEngine.UI.Toggle SpawnMonsterAllyToggle;

	public UnityEngine.UI.Toggle SpawnMonsterNeutralToggle;

	public UnityEngine.UI.Toggle SpawnMonsterEnemy2Toggle;

	public TMP_InputField SetAttackStrengthText;

	public TMP_Dropdown InfuseElementDropdown;

	public TextMeshProUGUI SelectedCharacterText;

	public TMP_Dropdown PostiveConditionDropdown;

	public TMP_Dropdown NegativeConditionDropdown;

	public UnityEngine.UI.Button EditInventoryButton;

	public TMP_Dropdown EditCardsDropdown;

	public UnityEngine.UI.Button SetAttackModButton;

	public UnityEngine.UI.Button AddPositiveConditionButton;

	public UnityEngine.UI.Button AddNegativeConditionButton;

	public UnityEngine.UI.Button KillButton;

	public UnityEngine.UI.Button KillAllEnemiesButton;

	public UnityEngine.UI.Button ShowDebugKeysButton;

	public TextMeshProUGUI ShowDebugKeysText;

	public DebugEditInventory DebugEditInventory;

	public TMP_InputField GainXPText;

	public TMP_InputField GainMoneyTokenText;

	public TMP_InputField SetHealthText;

	public TMP_Dropdown SpawnPropDropDown;

	public UnityEngine.UI.Button SpawnPropButton;

	public GameObject UpdateCharacter;

	public TMP_Dropdown SetBiomeDropDown;

	public UnityEngine.UI.Button SetBiomeButton;

	public TMP_Dropdown SetSubBiomeDropDown;

	public UnityEngine.UI.Button SetSubBiomeButton;

	public TMP_Dropdown SetThemeDropDown;

	public UnityEngine.UI.Button SetThemeButton;

	public TMP_Dropdown SetSubThemeDropDown;

	public UnityEngine.UI.Button SetSubThemeButton;

	public TMP_Dropdown SetToneDropDown;

	public UnityEngine.UI.Button SetToneButton;

	public TMP_InputField SaveCustomStateInputField;

	public UnityEngine.UI.Toggle Vignette;

	public GameObject RestartButton;

	public TMP_Dropdown MoveCardsDropdown;

	public UnityEngine.UI.Button MoveAvailableButton;

	public UnityEngine.UI.Button MoveDiscardButton;

	public UnityEngine.UI.Button MoveBurnButton;

	public UnityEngine.UI.Toggle DisplayScenarioRNG;

	public AutoCompleteComboBox SpawnObjectDropdown;

	public TMP_Dropdown SpawnObjectLevelDropdown;

	public UnityEngine.UI.Button SendErrorReport;

	public UnityEngine.UI.Button QuickSpawnButton;

	public TMP_Dropdown SwapCardsUnselectedDropdown;

	public TMP_Dropdown SwapCardsCurrentDropdown;

	public UnityEngine.UI.Button SwapUnselectedCardsButton;

	public TMP_Dropdown AddAttackModifierCardDropdown;

	public UnityEngine.UI.Button AddAttackModifierButton;

	public UnityEngine.UI.Toggle MPEndOfTurnCompare;

	public UnityEngine.UI.Button ForceDesyncButton;

	public InputBindingDisplayAsset ControllerModeAsset;

	public UnityEngine.UI.Toggle[] ControllerModeToggles;

	public TMP_InputField QueueActionDelayInput;

	public UnityEngine.UI.Toggle ShowTileIndexToggle;

	public UnityEngine.UI.Toggle AutoRestorePlayerCardsToggle;

	[Header("ClosedBeta Options")]
	public GameObject[] PanelsToDisable;

	[Header("Other Options")]
	public bool isSlowmoDisabled;

	[ConsoleVariable(Alias = "show_tile_index", Persistent = false)]
	public static bool ConsoleVar_ShowTileIndex = false;

	[ConsoleVariable(Alias = "show_axial_tile_index", Persistent = false)]
	public static bool ConsoleVar_ShowAxialTileIndex = false;

	[ConsoleVariable(Alias = "show_negative_hexes", Persistent = false)]
	public static bool ConsoleVar_ShowNegativeHexes = false;

	[ConsoleVariable(Alias = "show_ice_hexes", Persistent = false)]
	public static bool ConsoleVar_ShowIceHexes = false;

	[ConsoleVariable(Alias = "show_deactivated_actors", Persistent = false)]
	public static bool ConsoleVar_ShowDeactivatedActors = false;

	[ConsoleVariable(Alias = "show_loot_deck", Persistent = false)]
	public static int ShowLootDeck_kvar = 0;

	[ConsoleVariable(Alias = "show_actors_loot", Persistent = false)]
	public static int ShowActorsLoot_kvar = 0;

	[ConsoleVariable(Alias = "show_pathing_blockers", Persistent = false)]
	public static bool ShowPathingBlockers_kvar = false;

	private const string m_MenuTitle = "Debug Menu";

	private bool _selectedCharacterChanged;

	private static CPathFinderContext _pfContext;

	private static List<Point> _floodedPoints;

	private bool isDebugKeysShowing;

	private bool isGUIVisible;

	private bool isPaused;

	public DisplayTextList displayTextList;

	private bool isWallFadeDisabled;

	private CClientTile LOSSourceTile;

	private CClientTile LOSTargetTile;

	[Header("UI Toolkit")]
	public UnityEngine.UI.Toggle DisplayReorderableAmc;

	public UIDocument AttackModifiersUIDoc;

	public VisualTreeAsset ListItem;

	private UnityEngine.UIElements.Button _attackModifiersResetButton;

	private Label _availableAttackModifiersHeader;

	private Label _discardedAttackModifiersHeader;

	private ListView _availableAttackModifiersListView;

	private ListView _discardedAttackModifiersListView;

	private List<string> _availableAttackModifiersNames = new List<string>();

	private List<string> _discardedAttackModifiersNames = new List<string>();

	[Header("Events")]
	public UnityEvent OnDebugMenuShownEvent = new UnityEvent();

	public UnityEvent OnDebugMenuHiddenEvent = new UnityEvent();

	public static bool displayCardsId;

	public static bool displayScenarioRNG;

	public static bool displayCardPile;

	private static List<GameObject> _dbgVFX = new List<GameObject>();

	[ConsoleVariable(Alias = "dbg_show_actor_info")]
	public static bool DbgShowActorInfo_kvar = false;

	[ConsoleVariable(Alias = "dbg_show_prop_info")]
	public static bool DbgShowPropInfo_kvar = false;

	[ConsoleVariable(Alias = "dbg_show_spawner_info")]
	public static bool DbgShowSpawnerInfo_kvar = false;

	[ConsoleVariable(Alias = "dbg_show_vfx_info")]
	public static bool DbgShowVFXInfo_kvar = false;

	[ConsoleVariable(Alias = "dbg_skip_procedural_scenario")]
	public static bool DbgSkipProceduralScenario_kvar = false;

	private bool quickspawning;

	private string quickSpawnMonsterName = "AlgoxArcherID";

	private int quickSpawnMonsterLevel = 1;

	private static CActor s_SelectedActor;

	private static Point s_ProxyPickedPoint = new Point(-1, -1);

	private static bool s_InProxyExec = false;

	public static DebugMenu Instance { get; private set; }

	public int AttackValueOverride { get; set; }

	public bool CaptureTile { get; private set; }

	public bool IsMenuOpen { get; private set; }

	public bool IsMapTeleporting { get; set; }

	public bool ShowSpeedUp { get; set; }

	public CaptureTileFunctions CaptureTileFunction { get; private set; }

	private CActor SelectedCharacter
	{
		get
		{
			return s_SelectedActor;
		}
		set
		{
			if (value != s_SelectedActor)
			{
				_selectedCharacterChanged = true;
			}
			s_SelectedActor = value;
		}
	}

	[ConsoleVariable(Alias = "mul_player_health", Persistent = false)]
	public static float MulPlayerHealthCVar
	{
		get
		{
			return GameState.s_MulPlayerHealth;
		}
		set
		{
			if (value <= 0f)
			{
				return;
			}
			float s_MulPlayerHealth = GameState.s_MulPlayerHealth;
			if (s_MulPlayerHealth == value)
			{
				return;
			}
			GameState.s_MulPlayerHealth = value;
			List<CPlayerActor> list = ScenarioManager.Scenario?.PlayerActors;
			if (list == null)
			{
				return;
			}
			float num = GameState.s_MulPlayerHealth / s_MulPlayerHealth;
			foreach (CPlayerActor item in list)
			{
				item.MaxHealth = (int)Math.Ceiling((float)item.MaxHealth * num);
				item.Health = (int)Math.Ceiling((float)item.Health * num);
				CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(item, item, item.MaxHealth);
				Choreographer.s_Choreographer.PostMessageFromUI(message);
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		CaptureTile = false;
		IsMenuOpen = false;
		isDebugKeysShowing = false;
		isGUIVisible = true;
		isPaused = false;
		displayTextList = new DisplayTextList();
		isWallFadeDisabled = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		AttackValueOverride = int.MaxValue;
		FollowCameraDisabled = false;
		FollowCamToggle.isOn = !FollowCameraDisabled;
		ToggleSelectedCharacterMenus(toggle: false);
		CommandExecEngine engine = GameConsole.Engine;
		engine.CommandPostprocessor = (Action<IConsole, ICommand>)Delegate.Combine(engine.CommandPostprocessor, new Action<IConsole, ICommand>(PostProcessConsoleCommand));
		DisplayReorderableAmc.isOn = false;
		ObjectPool.onSpawned = (Action<GameObject>)Delegate.Combine(ObjectPool.onSpawned, new Action<GameObject>(RegisterNewVFX));
		GameObject[] panelsToDisable = PanelsToDisable;
		for (int i = 0; i < panelsToDisable.Length; i++)
		{
			panelsToDisable[i].SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		Instance = null;
		CommandExecEngine engine = GameConsole.Engine;
		engine.CommandPostprocessor = (Action<IConsole, ICommand>)Delegate.Remove(engine.CommandPostprocessor, new Action<IConsole, ICommand>(PostProcessConsoleCommand));
	}

	private void OnEnable()
	{
		_attackModifiersResetButton = AttackModifiersUIDoc.rootVisualElement.Q<UnityEngine.UIElements.Button>("Button_Reset");
		_attackModifiersResetButton.clicked += ResetAttackModifiersOfSelectedCharacter;
		_availableAttackModifiersHeader = AttackModifiersUIDoc.rootVisualElement.Q<Label>("Header_Available");
		_discardedAttackModifiersHeader = AttackModifiersUIDoc.rootVisualElement.Q<Label>("Header_Discarded");
		if (SelectedCharacter != null)
		{
			_availableAttackModifiersHeader.text = "Available Attack Modifiers (" + SelectedCharacter.Class.ID + ")";
			_discardedAttackModifiersHeader.text = "Discarded Attack Modifiers (" + SelectedCharacter.Class.ID + ")";
		}
		_availableAttackModifiersListView = AttackModifiersUIDoc.rootVisualElement.Q<ListView>("List_AvailableAttackModifiers");
		_availableAttackModifiersListView.itemsSource = _availableAttackModifiersNames;
		_availableAttackModifiersListView.makeItem = () => ListItem.CloneTree();
		_availableAttackModifiersListView.bindItem = (VisualElement e, int i) =>
		{
			e.Q<Label>("Label").text = _availableAttackModifiersNames[i];
		};
		_discardedAttackModifiersListView = AttackModifiersUIDoc.rootVisualElement.Q<ListView>("List_DiscardedAttackModifiers");
		_discardedAttackModifiersListView.itemsSource = _discardedAttackModifiersNames;
		_discardedAttackModifiersListView.makeItem = () => ListItem.CloneTree();
		_discardedAttackModifiersListView.bindItem = (VisualElement e, int i) =>
		{
			e.Q<Label>("Label").text = _discardedAttackModifiersNames[i];
		};
		_selectedCharacterChanged = true;
		Init();
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			ScenarioOptions.SetActive(value: false);
			DebugKeys.SetActive(value: false);
		}
		else
		{
			ScenarioOptions.SetActive(value: true);
			DebugKeys.SetActive(value: false);
		}
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign && SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.IsInScenarioPhase)
		{
			RestartButton.SetActive(value: true);
		}
		else
		{
			RestartButton.SetActive(value: false);
		}
		ShowDebugKeysText.text = "Show Debug Keys";
		(UpdateCharacter.transform as RectTransform).sizeDelta = new Vector2(880f, 400f);
		OnDebugMenuShownEvent?.Invoke();
	}

	private void OnDisable()
	{
		OnDebugMenuHiddenEvent?.Invoke();
		if (!DisplayReorderableAmc.isOn)
		{
			return;
		}
		_attackModifiersResetButton.clicked -= ResetAttackModifiersOfSelectedCharacter;
		if (SelectedCharacter == null || _availableAttackModifiersNames.Count <= 0)
		{
			return;
		}
		string text = "attack_modifier_cards";
		foreach (string availableAttackModifiersName in _availableAttackModifiersNames)
		{
			text = text + " " + availableAttackModifiersName;
		}
		SyncExec(text, SelectedCharacter, SelectedCharacter.ArrayIndex);
	}

	private void Update()
	{
		if (!AttackModifiersUIDoc.rootVisualElement.visible && DisplayReorderableAmc.isOn)
		{
			_selectedCharacterChanged = true;
		}
		AttackModifiersUIDoc.rootVisualElement.visible = DisplayReorderableAmc.isOn;
		ShowTileIndexToggle.SetIsOnWithoutNotify(ConsoleVar_ShowTileIndex);
		AutoRestorePlayerCardsToggle.SetIsOnWithoutNotify(GameState.s_AutoRestorePlayerCards);
		if (_selectedCharacterChanged)
		{
			SelectedCharacterText.text = SelectedCharacter?.GetPrefabName() ?? "No Character Selected";
			ToggleSelectedCharacterMenus(SelectedCharacter != null);
			if (SelectedCharacter is CPlayerActor cPlayerActor)
			{
				RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
			}
			if (SelectedCharacter?.Class is CMonsterClass { CardsPoolClass: var cardsPoolClass })
			{
				EditCardsDropdown.ClearOptions();
				EditCardsDropdown.AddOptions(cardsPoolClass.AbilityCardsPool.Select((CMonsterAbilityCard x) => string.Format("{0} (Init:{1} - {2})", x.ID, x.Initiative, string.Join(", ", x.Action.Abilities.Select((CAbility y) => y.AbilityType.ToString() + " " + y.Strength)))).ToList());
			}
		}
		if (_selectedCharacterChanged && DisplayReorderableAmc.isOn)
		{
			List<string> list = null;
			List<string> list2 = null;
			if (SelectedCharacter?.Class is CCharacterClass { AttackModifierCards: var attackModifierCards } cCharacterClass)
			{
				list = attackModifierCards.Select((AttackModifierYMLData c) => c.Name).ToList();
				list2 = cCharacterClass.DiscardedAttackModifierCards.Select((AttackModifierYMLData c) => c.Name).ToList();
			}
			else if (SelectedCharacter is CEnemyActor enemyActor)
			{
				CMonsterAttackModifierDeck attackModifiersDeck = MonsterClassManager.GetAttackModifiersDeck(enemyActor);
				list = attackModifiersDeck.AttackModifierCards.Select((AttackModifierYMLData c) => c.Name).ToList();
				list2 = attackModifiersDeck.DiscardedAttackModifierCards.Select((AttackModifierYMLData c) => c.Name).ToList();
			}
			_availableAttackModifiersHeader.text = "Available Attack Modifiers (" + SelectedCharacter?.Class?.ID + ")";
			_discardedAttackModifiersHeader.text = "Discarded Attack Modifiers (" + SelectedCharacter?.Class?.ID + ")";
			_availableAttackModifiersNames.Clear();
			if (list != null)
			{
				_availableAttackModifiersNames.AddRange(list);
			}
			_availableAttackModifiersListView.Rebuild();
			_discardedAttackModifiersNames.Clear();
			if (list2 != null)
			{
				_discardedAttackModifiersNames.AddRange(list2);
			}
			_discardedAttackModifiersListView.Rebuild();
		}
		_selectedCharacterChanged = false;
	}

	private void ResetAttackModifiersOfSelectedCharacter()
	{
		if (SelectedCharacter != null)
		{
			SyncExec("attack_modifier_cards reset", SelectedCharacter, SelectedCharacter.ArrayIndex);
		}
	}

	private static List<AttackModifierYMLData> GetAttackModifierCards(CActor actor)
	{
		if (actor.Class is CCharacterClass cCharacterClass)
		{
			return cCharacterClass.AttackModifierCards;
		}
		if (actor is CEnemyActor enemyActor)
		{
			return MonsterClassManager.GetAttackModifiersDeck(enemyActor).AttackModifierCards;
		}
		return null;
	}

	public static void UpdateQGL()
	{
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState == null)
		{
			return;
		}
		WrapBox wrapBox = new WrapBox
		{
			w = Screen.width,
			h = Screen.height
		};
		if (ShowPathingBlockers_kvar)
		{
			CClientTile[,] array = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (array != null)
			{
				CNode[,] nodes = ScenarioManager.PathFinder.Nodes;
				foreach (CNode cNode in nodes)
				{
					if (cNode.Blocked)
					{
						CClientTile cClientTile = array[cNode.Location.X, cNode.Location.Y];
						if (!(cClientTile.m_GameObject == null))
						{
							Vector3 position = cClientTile.m_GameObject.transform.position;
							Hexes.DrawHexWithLinesWorld(position, 1f, Color.red);
							QGL.LatePrintWorld("Blocked", position, Color.red, 1.5f);
						}
					}
				}
			}
		}
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		LevelEditorUI levelEditorUI = s_Instance?.m_LevelEditorUIInstance;
		if (levelEditorUI != null && s_Instance.IsEditing && levelEditorUI.PropPanel.isActiveAndEnabled)
		{
			List<CObjectProp> list = new List<CObjectProp>(currentScenarioState.Props);
			while (list.Count > 0)
			{
				CObjectProp cObjectProp = list[0];
				list.RemoveAt(0);
				if (!cObjectProp.HasLinkedProps)
				{
					continue;
				}
				foreach (CObjectProp linkedProp in cObjectProp.LinkedProps)
				{
					Vector3 vector = new Vector3(cObjectProp.Position.X, cObjectProp.Position.Y, cObjectProp.Position.Z);
					Vector3 vector2 = new Vector3(linkedProp.Position.X, linkedProp.Position.Y, linkedProp.Position.Z) - vector;
					float num = vector2.magnitude;
					vector2 /= num;
					Vector3 vector3 = vector;
					bool flag = false;
					while (num >= 0f)
					{
						float num2 = Math.Min(0.2f, num);
						Vector3 vector4 = vector3 + vector2 * num2;
						num -= 0.2f;
						QGL.LateDrawLineWorld(vector3, vector4, flag ? Color.green : Color.red);
						flag = !flag;
						vector3 = vector4;
					}
					list.Remove(linkedProp);
				}
			}
		}
		ShowLootDeck();
		if (ShowActorsLoot_kvar > 0 && Choreographer.s_Choreographer != null)
		{
			float num3 = Mathf.Clamp(ShowActorsLoot_kvar, 1, 10);
			foreach (CPlayerActor item3 in ScenarioManager.Scenario.PlayerActors.ToList())
			{
				string text = "Loot:\n";
				LootCard.EType[] allTypes = LootCard.AllTypes;
				foreach (LootCard.EType eType in allTypes)
				{
					int num4 = item3.TotalLoot.ResourceAmount(eType);
					if (num4 > 0)
					{
						text += $"\n{eType}: +{num4}";
					}
				}
				foreach (CCharacterResource characterResource in item3.CharacterResources)
				{
					if (characterResource.Amount > 0)
					{
						text += $"\n{characterResource.ID}: +{characterResource.Amount}";
					}
				}
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(item3);
				if ((bool)gameObject)
				{
					string str = text;
					Vector3 position2 = gameObject.transform.position;
					float scale = num3;
					QGL.LatePrintWorld(str, position2, Color.white, scale);
				}
			}
		}
		if (DbgShowPropInfo_kvar)
		{
			List<CObjectProp> props = ScenarioManager.CurrentScenarioState.Props;
			Dictionary<TileIndex, string> dictionary = new Dictionary<TileIndex, string>();
			foreach (CObjectProp item4 in props)
			{
				TileIndex arrayIndex = item4.ArrayIndex;
				dictionary[arrayIndex] = "";
			}
			foreach (CObjectProp item5 in props)
			{
				TileIndex arrayIndex2 = item5.ArrayIndex;
				string text2 = item5.PrefabName + "\n" + item5.PropGuid.Substring(0, 8) + "-...\n" + (item5.NegativeHex ? "NegHex\n" : "") + ((item5 is CObjectChest cObjectChest) ? $"Loot Type: {cObjectChest.Type}\n" : "");
				dictionary[arrayIndex2] += text2;
			}
			foreach (KeyValuePair<TileIndex, string> item6 in dictionary)
			{
				TileIndex key = item6.Key;
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[key.X, key.Y];
				if ((bool)cClientTile2.m_GameObject)
				{
					Vector3 position3 = cClientTile2.m_GameObject.transform.position;
					QGL.LatePrintWorld(item6.Value, position3, new Color32(byte.MaxValue, 144, 0, byte.MaxValue));
				}
			}
		}
		if (DbgShowActorInfo_kvar)
		{
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				print(allActor);
			}
		}
		if (DbgShowSpawnerInfo_kvar)
		{
			Dictionary<TileIndex, string> dictionary2 = new Dictionary<TileIndex, string>();
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				string text3 = (spawner.IsActive ? "" : "[x] ") + "Spawner: " + spawner.SpawnerGuid.Substring(0, 8) + "\n" + $"  round: {spawner.SpawnerData.SpawnStartRound}\n";
				if (!dictionary2.ContainsKey(spawner.ArrayIndex))
				{
					dictionary2[spawner.ArrayIndex] = "";
				}
				dictionary2[spawner.ArrayIndex] += text3;
			}
			foreach (KeyValuePair<TileIndex, string> item7 in dictionary2)
			{
				TileIndex key2 = item7.Key;
				Vector3 cTPos = ScenarioScriptFrontend.GetCTPos(key2.X, key2.Y);
				QGL.LatePrintWorld(item7.Value, cTPos, Color.yellow);
			}
		}
		if (_floodedPoints != null)
		{
			int num5 = int.MaxValue;
			int num6 = int.MinValue;
			foreach (Point floodedPoint in _floodedPoints)
			{
				int cost = _pfContext.GetCost(floodedPoint);
				if (num6 < cost)
				{
					num6 = cost;
				}
				if (num5 > cost)
				{
					num5 = cost;
				}
			}
			CClientTile[,] clientTileArray = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray;
			foreach (Point floodedPoint2 in _floodedPoints)
			{
				int cost2 = _pfContext.GetCost(floodedPoint2);
				Color color = Color.Lerp(Color.green, Color.red, (float)(cost2 - num5) / (float)(num6 - num5));
				Vector3 position4 = clientTileArray[floodedPoint2.X, floodedPoint2.Y].m_GameObject.transform.position;
				Hexes.DrawHexWithLinesWorld(position4, 1f, color);
				QGL.LatePrintWorld(cost2.ToString(), position4, color, 2f);
			}
		}
		if (WorldspaceStarHexDisplay.ConsoleVar_ShowPathCost && WorldspaceStarHexDisplay.s_DebugPathPoints.Count > 0)
		{
			CClientTile[,] clientTileArray2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray;
			foreach (Tuple<Point, int> s_DebugPathPoint in WorldspaceStarHexDisplay.s_DebugPathPoints)
			{
				Point item = s_DebugPathPoint.Item1;
				int item2 = s_DebugPathPoint.Item2;
				Vector3 position5 = clientTileArray2[item.X, item.Y].m_GameObject.transform.position;
				Hexes.DrawHexWithLinesWorld(position5, 1f, Color.green);
				QGL.LatePrintWorld(item2.ToString(), position5, Color.green, 2f);
			}
		}
		if (ConsoleVar_ShowNegativeHexes)
		{
			CClientTile[,] array2 = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (array2 != null)
			{
				foreach (Point item8 in ScenarioManager.CurrentScenarioState.NegativeHexes.Concat(ScenarioManager.CurrentScenarioState.SuperNegativeHexes))
				{
					Vector3 position6 = array2[item8.X, item8.Y].m_GameObject.transform.position;
					Color red = Color.red;
					Hexes.DrawHexWithLinesWorld(position6, 1f, red);
					QGL.LatePrintWorld("Neg", position6, red, 2f);
				}
			}
		}
		if (ConsoleVar_ShowIceHexes)
		{
			CClientTile[,] array3 = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (array3 != null)
			{
				CNode[,] nodes = ScenarioManager.PathFinder.Nodes;
				foreach (CNode cNode2 in nodes)
				{
					if (cNode2.IsIceTerrain)
					{
						Vector3 position7 = array3[cNode2.Location.X, cNode2.Location.Y].m_GameObject.transform.position;
						Color cyan = Color.cyan;
						Hexes.DrawHexWithLinesWorld(position7, 1f, cyan);
						QGL.LatePrintWorld("Ice", position7, cyan, 2f);
					}
				}
			}
		}
		if (ConsoleVar_ShowAxialTileIndex)
		{
			CClientTile[,] array4 = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (array4 != null)
			{
				CClientTile[,] array5 = array4;
				foreach (CClientTile cClientTile3 in array5)
				{
					if (cClientTile3 != null)
					{
						Point point = Point.OddRToAxial(cClientTile3.m_Tile.m_ArrayIndex);
						string str2 = $"({point.X},{point.Y})";
						Vector3 position8 = cClientTile3.m_GameObject.transform.position;
						QGL.LatePrintWorld(str2, position8, Color.cyan, 2f);
					}
				}
			}
		}
		if (ConsoleVar_ShowDeactivatedActors)
		{
			CClientTile[,] array6 = ClientScenarioManager.s_ClientScenarioManager?.ClientTileArray;
			if (array6 != null)
			{
				foreach (CActor item9 in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x.Deactivated))
				{
					Vector3 position9 = array6[item9.ArrayIndex.X, item9.ArrayIndex.Y].m_GameObject.transform.position;
					Color magenta = Color.magenta;
					Hexes.DrawHexWithLinesWorld(position9, 1f, magenta);
					QGL.LatePrintWorld("Actor", position9, magenta, 2f);
				}
			}
		}
		if (DbgShowVFXInfo_kvar)
		{
			for (int num7 = 0; num7 < _dbgVFX.Count; num7++)
			{
				GameObject gameObject2 = _dbgVFX[num7];
				if (gameObject2 == null || !gameObject2.activeSelf)
				{
					_dbgVFX.Remove(gameObject2);
					num7--;
					continue;
				}
				float num8 = 0f;
				for (int num9 = 0; num9 < num7; num9++)
				{
					if ((double)Vector3.Distance(_dbgVFX[num9].transform.position, gameObject2.transform.position) < 0.2)
					{
						num8 += 0.2f;
					}
				}
				Vector3 position10 = gameObject2.transform.position;
				position10.y += num8;
				QGL.LatePrintWorld(gameObject2.name, position10, Color.cyan, 2f);
			}
		}
		else
		{
			_dbgVFX.Clear();
		}
		SingleShot.TickSeconds(Time.deltaTime);
		static void print(CActor actor)
		{
			if (actor != null)
			{
				GameObject gameObject3 = Choreographer.s_Choreographer.FindClientActorGameObject(actor);
				if ((bool)gameObject3)
				{
					ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject3);
					QGL.LatePrintWorld(actor.Class.ID + "\n" + actor.ActorGuid.Substring(0, 8), color: (actor.Type != CActor.EType.Player && actor.Type != CActor.EType.HeroSummon && actor.Type != CActor.EType.Ally && actor.Type != CActor.EType.Neutral) ? Color.red : Color.green, worldPos: actorBehaviour.transform.position);
				}
			}
		}
	}

	private void RegisterNewVFX(GameObject gameobject)
	{
		if (DbgShowVFXInfo_kvar && (!(gameobject.GetComponent<ParticleSystem>() == null) || !(gameobject.GetComponentInChildren<ParticleSystem>() == null)))
		{
			_dbgVFX.Add(gameobject);
		}
	}

	private static void ShowLootDeck()
	{
		if (ShowLootDeck_kvar == 0)
		{
			return;
		}
		WrapBox wrapBox = new WrapBox
		{
			w = Screen.width,
			h = Screen.height
		};
		ScenarioRuleLibrary.ScenarioState css = ScenarioManager.CurrentScenarioState;
		string text = "Loot Table:";
		for (int i = 0; i < css.LootTable.Length; i++)
		{
			int num = css.LootTable[i];
			if (num > 0)
			{
				text += $"\n{(LootCard.EType)i}: {num}";
			}
		}
		int num2 = css.LootDeck.Count;
		for (int j = 0; j < css.LootDeck.Count; j++)
		{
			if (getCard(j).Type == LootCard.EType.None)
			{
				num2 = j;
				break;
			}
		}
		string text2 = "Loot Deck:";
		int num3 = 1;
		int num4 = num2 + 1;
		while (num4 < css.LootDeck.Count)
		{
			LootCard lootCard = getCard(num4);
			text2 += string.Format("\n{0} [X] {1}, {2}, {3} {4} {5}", $"{num3,2}", lootCard.ID, lootCard.ID, lootCard.Type, lootCard.Amount, (lootCard.Bonus != 0) ? ("Bonus: " + lootCard.Bonus) : "");
			num4++;
			num3++;
		}
		int num5 = 0;
		while (num5 < num2)
		{
			LootCard lootCard2 = getCard(num5);
			text2 = ((lootCard2.Type != LootCard.EType.RandomItem || !css.ScenarioSetup.RandomItemLooted) ? (text2 + string.Format("\n{0} {1}, {2} {3} {4}", $"{num3,2}", lootCard2.ID, lootCard2.Type, lootCard2.Amount, (lootCard2.Bonus != 0) ? ("Bonus: " + lootCard2.Bonus) : "")) : (text2 + string.Format("\n{0} [X] {1}, {2} {3} {4}", $"{num3,2}", lootCard2.ID, lootCard2.Type, lootCard2.Amount, (lootCard2.Bonus != 0) ? ("Bonus: " + lootCard2.Bonus) : "")));
			num5++;
			num3++;
		}
		string text3 = null;
		CMapState cMapState = SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState;
		if (cMapState != null && cMapState.IsInScenarioPhase && !LevelEditorController.s_Instance.IsEditing)
		{
			text3 = "Random Item Loot:\n";
			FHScenarioSetup ss = ScenarioManager.CurrentScenarioState.ScenarioSetup;
			if (ss.RandomItemLooted)
			{
				text3 += "Random Item already looted.\n";
			}
			else
			{
				text3 = ((ss.NextRandomItemID != 0) ? (text3 + ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData itemCardYMLData) => itemCardYMLData.ID == ss.NextRandomItemID).Name + "\n") : (text3 + "Zero Random Item.\n"));
			}
		}
		else if (cMapState != null && !cMapState.IsInScenarioPhase && !LevelEditorController.s_Instance.IsEditing)
		{
			text3 = "Random Item Deck:";
			if (css.LootTable[11] == 0)
			{
				text3 += "\nZero RandomItem drop in Loot Table";
			}
			else
			{
				IEnumerable<ItemDef> enumerable = SaveData.Instance?.Global?.CurrentAdventureData?.FrosthavenCampaign?.RandomItemDeck;
				if (enumerable != null)
				{
					bool flag = false;
					foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
					{
						if (allPlayer.LootAmount(LootCard.EType.RandomItem) > 0)
						{
							flag = true;
							break;
						}
					}
					int num6 = 1;
					foreach (ItemDef item in enumerable)
					{
						string text4 = $"{num6,2}";
						if (flag)
						{
							text3 = text3 + "\n" + text4 + " [X] " + item.Name;
							flag = false;
						}
						else
						{
							text3 = text3 + "\n" + text4 + " " + item.Name;
						}
						num6++;
					}
				}
			}
		}
		string text5 = text + "\n\n" + text2 + ((text3 != null) ? ("\n\n" + text3) : "");
		Vector2 vector = QGL.MeasureString(text5);
		float num7 = Mathf.Clamp(ShowLootDeck_kvar, 1, 10);
		vector *= WrapBox.Unscale(num7);
		WrapBox wrapBox2 = wrapBox.TopRight(vector.x + 20f, vector.y + 20f);
		Color black = Color.black;
		black.a = 0.75f;
		QGL.LateBlit(wrapBox2.x, wrapBox2.y, wrapBox2.w, wrapBox2.h, float.MaxValue, black);
		wrapBox = wrapBox2.Center(vector.x, vector.y);
		float x = wrapBox.x;
		float y = wrapBox.y;
		float scale = num7;
		QGL.LatePrint_tl(text5, x, y, Color.white, scale);
		LootCard getCard(int index)
		{
			int num8 = css.LootDeck[index];
			if (num8 >= 0 && num8 < CLootDeck.AllCards.Length)
			{
				return CLootDeck.AllCards[num8];
			}
			Log.GUIInfo("The loot deck 'AllCards' seems off.");
			return default;
		}
	}

	public void Init()
	{
		try
		{
			List<string> list = MonsterClassManager.Classes.Select((CMonsterClass x) => x.ID).ToList();
			list.Sort();
			SpawnMonsterDropdown.AvailableOptions = list;
			SpawnObjectDropdown.AvailableOptions = MonsterClassManager.ObjectClasses.Select((CObjectClass x) => x.ID).ToList();
			SpawnMonsterLevelDropdown.options.Clear();
			SpawnMonsterLevelDropdown.AddOptions(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });
			SpawnObjectLevelDropdown.options.Clear();
			SpawnObjectLevelDropdown.AddOptions(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });
			InfuseElementDropdown.options.Clear();
			InfuseElementDropdown.AddOptions((from x in (ElementInfusionBoardManager.EElement[])Enum.GetValues(typeof(ElementInfusionBoardManager.EElement))
				where x != ElementInfusionBoardManager.EElement.Any
				select x.ToString()).ToList());
			NegativeConditionDropdown.options.Clear();
			NegativeConditionDropdown.AddOptions((from x in (CCondition.ENegativeCondition[])Enum.GetValues(typeof(CCondition.ENegativeCondition))
				where x != CCondition.ENegativeCondition.NA && CCondition.AbilityTypeFromNegativeCondition(x) != CAbility.EAbilityType.None
				select x.ToString()).ToList());
			PostiveConditionDropdown.options.Clear();
			PostiveConditionDropdown.AddOptions((from x in (CCondition.EPositiveCondition[])Enum.GetValues(typeof(CCondition.EPositiveCondition))
				where x != CCondition.EPositiveCondition.NA && CCondition.AbilityTypeFromPositiveCondition(x) != CAbility.EAbilityType.None
				select x.ToString()).ToList());
			SpawnPropDropDown.options.Clear();
			SpawnPropDropDown.AddOptions((from s in CObjectProp.PropTypes
				where s != EPropType.None
				select s.ToString()).ToList());
			SetBiomeDropDown.options.Clear();
			SetBiomeDropDown.AddOptions((from s in (ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))
				where s != ScenarioPossibleRoom.EBiome.Inherit
				select s.ToString()).ToList());
			SetSubBiomeDropDown.options.Clear();
			SetSubBiomeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))
				where s != ScenarioPossibleRoom.ESubBiome.Inherit
				select s.ToString()).ToList());
			SetThemeDropDown.options.Clear();
			SetThemeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))
				where s != ScenarioPossibleRoom.ETheme.Inherit
				select s.ToString()).ToList());
			SetSubThemeDropDown.options.Clear();
			SetSubThemeDropDown.AddOptions((from s in (ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))
				where s != ScenarioPossibleRoom.ESubTheme.Inherit
				select s.ToString()).ToList());
			SetToneDropDown.options.Clear();
			SetToneDropDown.AddOptions((from s in (ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))
				where s != ScenarioPossibleRoom.ETone.Inherit
				select s.ToString()).ToList());
			MoveCardsDropdown.options.Clear();
			DisplayScenarioRNG.SetValue(displayScenarioRNG);
			AddAttackModifierCardDropdown.options.Clear();
			AddAttackModifierCardDropdown.AddOptions(ScenarioRuleClient.SRLYML.AttackModifiers.Select((AttackModifierYMLData x) => x.Name).ToList());
			EditCardsDropdown.options.Clear();
			EditCardsDropdown.onValueChanged.AddListener(delegate
			{
				EditCards();
			});
			SpawnMonsterNeutralToggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterAllyToggle.SetValue(value: false);
				SpawnMonsterEnemy2Toggle.SetValue(value: false);
			});
			SpawnMonsterAllyToggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterNeutralToggle.SetValue(value: false);
				SpawnMonsterEnemy2Toggle.SetValue(value: false);
			});
			SpawnMonsterEnemy2Toggle.onValueChanged.AddListener(delegate
			{
				SpawnMonsterAllyToggle.SetValue(value: false);
				SpawnMonsterNeutralToggle.SetValue(value: false);
			});
			SwapCardsUnselectedDropdown.ClearOptions();
			SwapCardsCurrentDropdown.ClearOptions();
			SetCameraHeightText.text = ((int)CameraController.s_CameraController.m_ZoomOutExtraHeight).ToString();
			if (QueueActionDelayInput != null)
			{
				QueueActionDelayInput.text = ActionProcessor.QueueDelay.ToString();
			}
			for (int num = 0; num < ControllerModeToggles.Length; num++)
			{
				ControllerModeToggles[num].onValueChanged.RemoveListener(ToggleControllerMode);
				ControllerModeToggles[num].onValueChanged.AddListener(ToggleControllerMode);
			}
			MPEndOfTurnCompare.SetValue(SaveData.Instance.Global.MPEndOfTurnCompare);
		}
		catch (Exception ex)
		{
			Log.GUIError("An error occurred initialising the Debug menu\n" + ex.StackTrace);
		}
	}

	public void ToggleMenu()
	{
		IsMenuOpen = !IsMenuOpen;
		gameObject.SetActive(IsMenuOpen);
	}

	public static bool InterceptTileAction(CClientTile clientTile)
	{
		if (Instance != null && Instance.CaptureTile)
		{
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnMonster)
			{
				Instance.SpawnMonsterAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SelectCharacter)
			{
				Instance.CharacterSelected(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnProp)
			{
				Instance.SpawnPropAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnSpawner)
			{
				Instance.SpawnSpawnerAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SpawnObject)
			{
				Instance.SpawnObjectAtLocation(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.LOSSourceTile)
			{
				Instance.SetLOSSourceTile(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.LOSTargetTile)
			{
				Instance.SetLOSTargetTile(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SetPosition)
			{
				Instance.SetActorPosition(clientTile);
				return true;
			}
			if (Instance.CaptureTileFunction == CaptureTileFunctions.SetFacing)
			{
				Instance.SetActorFacing(clientTile);
				return true;
			}
		}
		return false;
	}

	public void ToggleControllerMode(bool toggled)
	{
		ForceInputDevice forceInputDevice = UnityEngine.Object.FindObjectOfType<ForceInputDevice>();
		if (forceInputDevice == null)
		{
			forceInputDevice = EventSystem.current?.gameObject.AddComponent<ForceInputDevice>();
		}
		forceInputDevice.ForcedDevice = (toggled ? ControllerModeAsset : null);
		for (int i = 0; i < ControllerModeToggles.Length; i++)
		{
			ControllerModeToggles[i].SetIsOnWithoutNotify(toggled);
		}
	}

	public void StallMainThread(int seconds)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		while (realtimeSinceStartup + (float)seconds > Time.realtimeSinceStartup)
		{
			Thread.Sleep(100);
		}
	}

	public void SetQueueActionDelay()
	{
		ActionProcessor.QueueDelay = float.Parse(QueueActionDelayInput.text);
		Log.GUIInfo("Set Queue Action Delay to " + ActionProcessor.QueueDelay);
	}

	public void ToggleDisplayCardsId(bool active)
	{
		displayCardsId = active;
	}

	public void ToggleScenarioRNG(bool active)
	{
		displayScenarioRNG = active;
	}

	public void ToggleDisplayCardPile(bool active)
	{
		displayCardPile = active;
	}

	public void ToggleMPEndOfTurnCompare(bool active)
	{
		SaveData.Instance.Global.MPEndOfTurnCompare = active;
		SaveData.Instance.Global.Save();
	}

	public void ToggleShowAStar(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleAStarDisplay();
		Log.GUIInfo("Show AStar has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleShowAIPath(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleAIPathDisplay();
		Log.GUIInfo("Show AI Path has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleStatusLog(bool value)
	{
		GUIInterface.s_GUIInterface.ToggleStatusWindow(value);
	}

	public void ToggleDebugSlowMo(bool value)
	{
		if (TimeManager.IsSlowMo)
		{
			TimeManager.UnslowTime();
		}
		else
		{
			TimeManager.SlowTime(0.25f);
		}
	}

	public void ToggleTileIndex(bool value)
	{
		ConsoleVar_ShowTileIndex = value;
	}

	public void ToggleShowLevelFlow(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleLevelFlowDisplay();
		Log.GUIInfo("Show Level Flow " + (value ? "enabled" : "disabled"));
	}

	public void ToggleShowLOS(bool value)
	{
		ClientScenarioManager.s_ClientScenarioManager.ToggleLOSDisplay(value);
		Log.GUIInfo("Show LOS " + (value ? "enabled" : "disabled"));
	}

	public void ToggleSelectLOSSourceTile()
	{
		MenuTitle.text = "Select an LOS Source Tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.LOSSourceTile;
		IsMenuOpen = false;
	}

	private void ToggleSelectLOSTargetTile()
	{
		MenuTitle.text = "Select an LOS Target Tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.LOSTargetTile;
		IsMenuOpen = false;
	}

	public void ToggleAutoRestorePlayersCards(bool value)
	{
		SyncExec($"auto_restore_player_cards {value}");
		if (value)
		{
			SyncExec("restore_player_cards");
		}
	}

	public void ToggleAdvantageOutputText(bool value)
	{
		GameState.OutputAdvantageInfo = value;
		Log.GUIInfo("Advantage Debug Output has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleCameraFollow(bool value)
	{
		FollowCameraDisabled = !value;
		if (FollowCameraDisabled)
		{
			CameraController.s_CameraController.DisableCameraInput(disableInput: false, Choreographer.s_Choreographer);
		}
		Log.GUIInfo("Camera Follow has been " + (value ? "enabled" : "disabled"));
	}

	public void ToggleDisableSlowmo(bool value)
	{
		isSlowmoDisabled = value;
		Log.GUIInfo("Disable slowmo " + (value ? "enabled" : "disabled"));
	}

	public void SetCameraAngle()
	{
		if (int.TryParse(SetCameraHeightText.text, out var result))
		{
			CameraController.s_CameraController.m_ZoomOutExtraHeight = result;
			Log.GUIInfo("Camera height set to " + result);
		}
	}

	public void SpawnSelectLocation()
	{
		MenuTitle.text = "Select an empty tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnMonster;
		IsMenuOpen = false;
	}

	public void SpawnMonsterAtLocation(CClientTile tile)
	{
		string arg = (quickspawning ? quickSpawnMonsterName : SpawnMonsterDropdown.Text);
		int result;
		if (!quickspawning)
		{
			int.TryParse(SpawnMonsterLevelDropdown.options[SpawnMonsterLevelDropdown.value].text, out result);
		}
		else
		{
			result = quickSpawnMonsterLevel;
		}
		try
		{
			if (quickspawning || SpawnMonsterDropdown.IsSelectionValid)
			{
				CActor.EType eType;
				if (!quickspawning && SpawnMonsterAllyToggle.isOn)
				{
					eType = CActor.EType.Ally;
				}
				else
				{
					eType = ((!quickspawning && SpawnMonsterNeutralToggle.isOn) ? CActor.EType.Neutral : ((quickspawning || !SpawnMonsterEnemy2Toggle.isOn) ? CActor.EType.Enemy : CActor.EType.Enemy2));
				}
				SyncExec($"spawn {arg} {eType} {result}", tile);
			}
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
			quickspawning = false;
			CActor cActor = tile?.m_Tile?.LivingActor;
			if (cActor != null)
			{
				ScenarioManager.CurrentScenarioState?.PerformOnSpawnedModifiers(cActor, ScenarioManager.CurrentScenarioState.Players.Count);
			}
		}
		catch (Exception ex)
		{
			Log.GUIError(ex.Message + "\n" + ex.StackTrace);
			SpawnSelectLocation();
		}
	}

	public void QuickSpawnMonster()
	{
		quickspawning = true;
		SpawnSelectLocation();
	}

	public void SpawnSelectLocationObject()
	{
		MenuTitle.text = "Select an empty tile";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnObject;
		IsMenuOpen = false;
	}

	public void SpawnObjectAtLocation(CClientTile tile)
	{
		string text = SpawnObjectDropdown.Text;
		if (SpawnObjectDropdown.IsSelectionValid && int.TryParse(SpawnObjectLevelDropdown.options[SpawnObjectLevelDropdown.value].text, out var result) && LevelEditorController.SpawnObjectAtLocation(tile, text, result))
		{
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void SpawnPropAtLocation(CClientTile tile)
	{
		string text = SpawnPropDropDown.options[SpawnPropDropDown.value].text;
		SyncExec("place " + text, tile);
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		IsMenuOpen = true;
		MenuTitle.text = "Debug Menu";
		Frame.SetActive(value: true);
		ToggleMenu();
	}

	public void SetLOSSourceTile(CClientTile tile)
	{
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		LOSSourceTile = tile;
		ToggleSelectLOSTargetTile();
	}

	public void SetLOSTargetTile(CClientTile tile)
	{
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		LOSTargetTile = tile;
		IsMenuOpen = true;
		MenuTitle.text = "Debug Menu";
		Frame.SetActive(value: true);
		ToggleMenu();
		ClientScenarioManager.s_ClientScenarioManager.SetSpecificSourceAndTargetTile(LOSSourceTile, LOSTargetTile);
	}

	public void SelectActorPosition()
	{
		if (SelectedCharacter != null)
		{
			MenuTitle.text = "Select an empty tile";
			Frame.SetActive(value: false);
			CaptureTile = true;
			CaptureTileFunction = CaptureTileFunctions.SetPosition;
			IsMenuOpen = false;
		}
	}

	public void SelectActorFacing()
	{
		if (SelectedCharacter != null)
		{
			MenuTitle.text = "Select a tile";
			Frame.SetActive(value: false);
			CaptureTile = true;
			CaptureTileFunction = CaptureTileFunctions.SetFacing;
			IsMenuOpen = false;
		}
	}

	public static void SetActorPosition(CActor actor, CClientTile clientTile, bool tryReveal = false)
	{
		if (actor != null && clientTile != null)
		{
			CActor a = actor;
			CTile t = clientTile.m_Tile;
			ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
			{
				GameState.TeleportActor(a, t, tryReveal, null, invokeOnMovedListeners: true, useAnim: false);
			}, "CHEAT_TELEPORT");
		}
	}

	public static void SetActorFacing(CActor actor, CClientTile clientTile, bool tryReveal = false)
	{
		if (actor != null && clientTile != null)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actor);
			if (gameObject != null)
			{
				gameObject.transform.LookAt(clientTile.m_GameObject.transform.position);
			}
		}
	}

	public void SetActorPosition(CClientTile clientTile)
	{
		SyncExec("teleport", clientTile);
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		IsMenuOpen = true;
		MenuTitle.text = "Debug Menu";
		Frame.SetActive(value: true);
		ToggleMenu();
	}

	public void SetActorFacing(CClientTile clientTile)
	{
		SyncExec("face", clientTile);
		CaptureTile = false;
		CaptureTileFunction = CaptureTileFunctions.None;
		IsMenuOpen = true;
		MenuTitle.text = "Debug Menu";
		Frame.SetActive(value: true);
		ToggleMenu();
	}

	public void SetBiome()
	{
		LevelEditorController.SetBiome(((ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))).Single((ScenarioPossibleRoom.EBiome s) => s.ToString() == SetBiomeDropDown.options[SetBiomeDropDown.value].text));
	}

	public void SetSubBiome()
	{
		LevelEditorController.SetSubBiome(((ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))).Single((ScenarioPossibleRoom.ESubBiome s) => s.ToString() == SetSubBiomeDropDown.options[SetSubBiomeDropDown.value].text));
	}

	public void SetTheme()
	{
		LevelEditorController.SetTheme(((ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))).Single((ScenarioPossibleRoom.ETheme s) => s.ToString() == SetThemeDropDown.options[SetThemeDropDown.value].text));
	}

	public void SetSubTheme()
	{
		LevelEditorController.SetSubTheme(((ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))).Single((ScenarioPossibleRoom.ESubTheme s) => s.ToString() == SetSubThemeDropDown.options[SetSubThemeDropDown.value].text));
	}

	public void SetTone()
	{
		LevelEditorController.SetTone(((ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))).Single((ScenarioPossibleRoom.ETone s) => s.ToString() == SetToneDropDown.options[SetToneDropDown.value].text));
	}

	public void SetAttackStrength()
	{
		if (int.TryParse(SetAttackStrengthText.text, out var result))
		{
			CAbilityAttack.SetNextAttackValueOverride(result);
			AttackValueOverride = result;
			Log.GUIInfo("The next attack will have " + result + " set as the strength");
		}
	}

	public void AddElement()
	{
		AddElement(InfuseElementDropdown.options[InfuseElementDropdown.value].text);
	}

	public static void AddElement(string elementName)
	{
		if (!s_InProxyExec)
		{
			SyncExec("infuse " + elementName);
			return;
		}
		try
		{
			ElementInfusionBoardManager.EElement element = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString().Contains(elementName, StringComparison.OrdinalIgnoreCase));
			ElementInfusionBoardManager.Infuse(element, null);
			ElementInfusionBoardManager.ProcessEndTurnInfusions();
			InfusionBoardUI.Instance?.UpdateBoard();
			Log.GUIInfo("Infused " + element);
		}
		catch (Exception exception)
		{
			Log.GUIException(exception, "Failed to infuse element.  An exception occurred.");
		}
	}

	public static void InfuseAll()
	{
		if (!s_InProxyExec)
		{
			SyncExec("infuse all");
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in ElementInfusionBoardManager.Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
		{
			try
			{
				ElementInfusionBoardManager.Infuse(item, null);
				Log.GUIInfo("Infused " + item);
			}
			catch (Exception exception)
			{
				Log.GUIException(exception, "Failed to infuse element.  An exception occurred.");
			}
		}
		ElementInfusionBoardManager.ProcessEndTurnInfusions();
		InfusionBoardUI.Instance?.UpdateBoard();
	}

	public void SelectCharacter()
	{
		MenuTitle.text = "Select a character";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SelectCharacter;
		IsMenuOpen = false;
	}

	public void SelectPropLocation()
	{
		MenuTitle.text = "Select a tile to spawn the prop";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnProp;
		IsMenuOpen = false;
	}

	public void SelectSpawnerLocation()
	{
		MenuTitle.text = "Select a tile to spawn the Spawner";
		Frame.SetActive(value: false);
		CaptureTile = true;
		CaptureTileFunction = CaptureTileFunctions.SpawnSpawner;
		IsMenuOpen = false;
	}

	public void SelectActorParam(CActor target)
	{
		if (target != null)
		{
			SelectedCharacter = target;
		}
	}

	public void SelectActorParam(CClientTile tile)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if (cActor != null)
		{
			SelectedCharacter = cActor;
		}
	}

	public void CharacterSelected(CClientTile tile)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex);
		if (cActor != null)
		{
			SelectedCharacter = cActor;
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			Log.GUIInfo("Selected " + cActor.GetPrefabName());
		}
		else
		{
			Log.GUIError("Unable to find target at selected location");
			SelectCharacter();
		}
	}

	private void RefreshPlayerCardDropdowns(CCharacterClass characterClass)
	{
		MoveCardsDropdown.ClearOptions();
		MoveCardsDropdown.AddOptions(characterClass.HandAbilityCards.Select(DisplayName).ToList());
		MoveCardsDropdown.AddOptions(characterClass.DiscardedAbilityCards.Select(DisplayName).ToList());
		MoveCardsDropdown.AddOptions(characterClass.LostAbilityCards.Select(DisplayName).ToList());
		MoveCardsDropdown.AddOptions(characterClass.PermanentlyLostAbilityCards.Select(DisplayName).ToList());
		SwapCardsUnselectedDropdown.ClearOptions();
		SwapCardsUnselectedDropdown.AddOptions(characterClass.UnselectedAbilityCards.Select(DisplayName).ToList());
		SwapCardsCurrentDropdown.ClearOptions();
		SwapCardsCurrentDropdown.AddOptions(characterClass.HandAbilityCards.Select(DisplayName).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.DiscardedAbilityCards.Select(DisplayName).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.LostAbilityCards.Select(DisplayName).ToList());
		SwapCardsCurrentDropdown.AddOptions(characterClass.PermanentlyLostAbilityCards.Select(DisplayName).ToList());
		static string DisplayName(CAbilityCard card)
		{
			if (!card.Name.StartsWith("FH/"))
			{
				return string.Format("{0} - {1} ({2})", card.ID, card.Name.Replace("ABILITY_CARD_", ""), card.Initiative) + " GH!";
			}
			return LevelEditorUI.FormatFHName(card.Name);
		}
	}

	public void SwapInUnselectedCard()
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !SwapCardsUnselectedDropdown.options[SwapCardsUnselectedDropdown.value].text.IsNullOrEmpty() && !SwapCardsCurrentDropdown.options[SwapCardsCurrentDropdown.value].text.IsNullOrEmpty() && SelectedCharacter is CPlayerActor cPlayerActor && Singleton<CardsHandManager>.Instance.IsActive())
		{
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, SwapCardsCurrentDropdown.options[SwapCardsCurrentDropdown.value].text, "Unselected", updateUI: false);
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, SwapCardsUnselectedDropdown.options[SwapCardsUnselectedDropdown.value].text, "Available");
			RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
		}
	}

	public void MoveAbilityCardParam(CActor actor, string cardName, string destinationPile = "Available")
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && actor is CPlayerActor cPlayerActor && Singleton<CardsHandManager>.Instance.IsActive())
		{
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, cardName, destinationPile);
			RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
		}
	}

	public void MoveAbilityCard(string destinationPile)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !MoveCardsDropdown.options[MoveCardsDropdown.value].text.IsNullOrEmpty() && SelectedCharacter is CPlayerActor cPlayerActor && Singleton<CardsHandManager>.Instance.IsActive())
		{
			MoveAbilityCardToPile(cPlayerActor.CharacterClass, MoveCardsDropdown.options[MoveCardsDropdown.value].text, destinationPile);
			RefreshPlayerCardDropdowns(cPlayerActor.CharacterClass);
		}
	}

	private void MoveAbilityCardToPile(CCharacterClass characterClass, string cardName, string destinationPile, bool updateUI = true)
	{
		bool isLegacyName = cardName.EndsWith("GH!");
		CAbilityCard cAbilityCard = characterClass.HandAbilityCards.SingleOrDefault(NameMatches);
		List<CAbilityCard> fromAbilityCardList = characterClass.HandAbilityCards;
		string fromCardPileName = "HandAbilityCards";
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.DiscardedAbilityCards;
			cAbilityCard = characterClass.DiscardedAbilityCards.SingleOrDefault(NameMatches);
			fromCardPileName = "DiscardedAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.LostAbilityCards;
			cAbilityCard = characterClass.LostAbilityCards.SingleOrDefault(NameMatches);
			fromCardPileName = "LostAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.PermanentlyLostAbilityCards;
			cAbilityCard = characterClass.PermanentlyLostAbilityCards.SingleOrDefault(NameMatches);
			fromCardPileName = "PermanentlyLostAbilityCards";
		}
		if (cAbilityCard == null)
		{
			fromAbilityCardList = characterClass.UnselectedAbilityCards;
			cAbilityCard = characterClass.UnselectedAbilityCards.SingleOrDefault(NameMatches);
			fromCardPileName = "UnselectedAbilityCards";
		}
		if (cAbilityCard != null)
		{
			switch (destinationPile)
			{
			case "Available":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.HandAbilityCards, fromCardPileName, "HandAbilityCards");
				break;
			case "Discard":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.DiscardedAbilityCards, fromCardPileName, "DiscardedAbilityCards");
				break;
			case "Lost":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.LostAbilityCards, fromCardPileName, "LostAbilityCards");
				break;
			case "Unselected":
				characterClass.MoveAbilityCard(cAbilityCard, fromAbilityCardList, characterClass.UnselectedAbilityCards, fromCardPileName, "UnselectedAbilityCards");
				break;
			}
			if (updateUI)
			{
				CardsHandManager.ShowCardsSelection();
			}
		}
		bool NameMatches(CAbilityCard card)
		{
			if (isLegacyName)
			{
				return card.Name == "ABILITY_CARD_" + Regex.Replace(Regex.Replace(cardName, " \\(.*?\\)", ""), "^.*? - ", "");
			}
			return LevelEditorUI.FormatFHName(card.Name) == cardName;
		}
	}

	private void ToggleSelectedCharacterMenus(bool toggle)
	{
		PostiveConditionDropdown.enabled = toggle;
		NegativeConditionDropdown.enabled = toggle;
		EditInventoryButton.enabled = toggle;
		EditCardsDropdown.enabled = toggle;
		SetAttackModButton.enabled = toggle;
		AddPositiveConditionButton.enabled = toggle;
		AddNegativeConditionButton.enabled = toggle;
		KillButton.enabled = toggle;
		AddAttackModifierCardDropdown.enabled = toggle;
		AddAttackModifierButton.enabled = toggle;
	}

	private void KillActor(CActor actor)
	{
		try
		{
			CMessageData lastMessage = Choreographer.s_Choreographer.LastMessage;
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actor);
			if ((bool)gameObject)
			{
				ActorBehaviour.UpdateHealth(gameObject);
			}
			actor.Health = 0;
			GameState.KillActor(actor, actor, CActor.ECauseOfDeath.DebugMenu, out var _);
			if (actor.Type == CActor.EType.Player)
			{
				(actor.Class as CCharacterClass).CheckForFinishedActiveBonuses(actor);
			}
			else if (actor.Type == CActor.EType.HeroSummon)
			{
				(actor as CHeroSummonActor).Summoner.CharacterClass.CheckForFinishedActiveBonuses((actor as CHeroSummonActor).Summoner);
			}
			Choreographer.s_Choreographer.LastMessage = lastMessage;
		}
		catch (Exception ex)
		{
			Log.GUIError(ex.Message + "\n" + ex.StackTrace);
		}
		MoveCardsDropdown.options.Clear();
	}

	public void Kill()
	{
		if (SelectedCharacter != null)
		{
			SyncExec("set_hp 0", SelectedCharacter, SelectedCharacter.ArrayIndex);
			Log.GUIInfo("Character " + SelectedCharacter.GetPrefabName() + " has been killed");
			SelectedCharacter = null;
			ToggleMenu();
		}
	}

	public void KillAllEnemies()
	{
		SyncExec("kill_all_monsters");
	}

	public void ShowAllScenarios()
	{
	}

	public void DrawCityEvent()
	{
	}

	public void ApplyAutoCompleteYML()
	{
	}

	public void ChangeDifficulty()
	{
	}

	public void UnlockAllCharacters()
	{
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			RewardGroup item = new RewardGroup(new List<Reward>
			{
				new Reward(@class.CharacterID)
			});
			AdventureState.MapState.ApplyRewards(new List<RewardGroup> { item });
		}
	}

	public void EditInventory()
	{
		DebugEditInventory.ShowEditInventory(SelectedCharacter.Inventory);
	}

	public void EditCards()
	{
		if (SelectedCharacter != null)
		{
			string text = EditCardsDropdown.options[EditCardsDropdown.value].text;
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = Regex.Replace(text, " \\(.*?\\)", "");
				SyncExec("set_monster_next_card " + text2, SelectedCharacter, SelectedCharacter.ArrayIndex);
			}
		}
	}

	public void SetAttackMod()
	{
		if (SelectedCharacter == null)
		{
			return;
		}
		try
		{
			if (SelectedCharacter.Class is CMonsterClass)
			{
				if (SelectedCharacter is CEnemyActor enemyActor)
				{
					DisplayDebugText(2, MonsterClassManager.DebugSetAttackMod(enemyActor));
				}
				return;
			}
			CCharacterClass cCharacterClass = null;
			cCharacterClass = ((!(SelectedCharacter is CHeroSummonActor cHeroSummonActor)) ? ((CCharacterClass)SelectedCharacter.Class) : ((CCharacterClass)cHeroSummonActor.Summoner.Class));
			if (cCharacterClass.DiscardedAttackModifierCards.Count > 0)
			{
				cCharacterClass.ResetAttackModifierDeck();
			}
			AttackModifierYMLData item = cCharacterClass.AttackModifierCards[0];
			cCharacterClass.AttackModifierCards.Remove(item);
			cCharacterClass.AttackModifierCards.Add(item);
			DisplayDebugText(2, "Next: " + cCharacterClass.AttackModifierCards[0].Name);
		}
		catch (Exception ex)
		{
			Log.GUIError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void AddAttackModifier()
	{
		if (SelectedCharacter == null)
		{
			return;
		}
		try
		{
			if (!(SelectedCharacter is CEnemyActor enemyActor))
			{
				CCharacterClass cCharacterClass = null;
				cCharacterClass = ((!(SelectedCharacter is CHeroSummonActor cHeroSummonActor)) ? ((CCharacterClass)SelectedCharacter.Class) : ((CCharacterClass)cHeroSummonActor.Summoner.Class));
				List<string> cardNames = new List<string> { AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text };
				cCharacterClass.AddAdditionalModifierCards(cardNames);
				DisplayDebugText(2, "Added attack modifier card : " + AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text);
			}
			else
			{
				List<string> cardNames2 = new List<string> { AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text };
				MonsterClassManager.AddAdditionalModifierCards(enemyActor, cardNames2);
				DisplayDebugText(2, "Added attack modifier card : " + AddAttackModifierCardDropdown.options[AddAttackModifierCardDropdown.value].text);
			}
		}
		catch (Exception ex)
		{
			Log.GUIError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void AddPositiveCondition()
	{
		if (SelectedCharacter != null)
		{
			string text = PostiveConditionDropdown.options[PostiveConditionDropdown.value].text;
			SyncExec("apply_condition " + text, SelectedCharacter, SelectedCharacter.ArrayIndex);
			_selectedCharacterChanged = true;
		}
	}

	public void AddNegativeCondition()
	{
		if (SelectedCharacter != null)
		{
			string text = NegativeConditionDropdown.options[NegativeConditionDropdown.value].text;
			SyncExec("apply_condition " + text, SelectedCharacter, SelectedCharacter.ArrayIndex);
			_selectedCharacterChanged = true;
		}
	}

	public void AutoCompletePersonalQuests()
	{
		AdventureState.MapState.AutoCompletePersonalQuests();
		Log.GUIInfo("Complete all Personal Quests");
	}

	public void Win()
	{
		Win(skipMenuToggle: false);
	}

	public void Win(bool skipMenuToggle)
	{
		if (FFSNetwork.IsOnline)
		{
			Synchronizer.SendSideAction(GameActionType.DebugWinScenario, null, canBeUnreliable: false, sendToHostOnly: false, 0, -1);
		}
		Choreographer.s_Choreographer.WinScenario();
		if (!skipMenuToggle)
		{
			ToggleMenu();
		}
	}

	private static bool IsInScenario()
	{
		bool result = false;
		int i = 0;
		for (int sceneCount = SceneManager.sceneCount; i < sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == "Game")
			{
				result = true;
				break;
			}
		}
		return result;
	}

	[ConsoleCommand(Command = "win", Description = " - wins the current scenario")]
	public static void WinScenarioCommand(IConsole console, int conclusionIndex = -1)
	{
		if (!IsInScenario())
		{
			throw new ConsoleCommandException("You are not in a scenario");
		}
		if (FFSNetwork.IsOnline)
		{
			Synchronizer.SendSideAction(GameActionType.DebugWinScenario, null, canBeUnreliable: false, sendToHostOnly: false, 0, conclusionIndex);
		}
		Choreographer.s_Choreographer.WinScenario(forceUI: true, conclusionIndex);
	}

	public void Lose()
	{
		if (!IsInScenario())
		{
			throw new ConsoleCommandException("You are not in a scenario");
		}
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.DebugLoseScenario);
		}
		Choreographer.s_Choreographer.LoseScenario();
		ToggleMenu();
	}

	[ConsoleCommand(Command = "lose", Description = " - loses the current scenario")]
	public static void LoseScenarioCommand(IConsole console)
	{
		if (FFSNetwork.IsHost)
		{
			Synchronizer.SendGameAction(GameActionType.DebugLoseScenario);
		}
		Choreographer.s_Choreographer.LoseScenario();
	}

	public void Restart()
	{
		ToggleMenu();
		(Singleton<ESCMenu>.Instance as UIScenarioEscMenu).RestartScenario();
	}

	public void GainXP()
	{
		if (SelectedCharacter != null && int.TryParse(GainXPText.text, out var result))
		{
			SelectedCharacter.GainXP(result);
		}
	}

	public void SpeedUp()
	{
		SaveData.Instance.Global.DebugSpeedMode = true;
		if (TimeManager.DefaultTimeScale < 10f)
		{
			TimeManager.DefaultTimeScale += 0.02f;
		}
		else
		{
			TimeManager.DefaultTimeScale = 10f;
		}
		DisplayTimeScaleText();
	}

	public void SlowDown()
	{
		SaveData.Instance.Global.DebugSpeedMode = true;
		if (TimeManager.DefaultTimeScale >= 0.02f)
		{
			TimeManager.DefaultTimeScale -= 0.02f;
		}
		else
		{
			TimeManager.DefaultTimeScale = 0f;
		}
		DisplayTimeScaleText();
	}

	public void ForceDesync()
	{
		GameState.s_DesyncVar += new System.Random().Next();
		SyncExec("check_for_desync");
	}

	public void CompareScenarioStates()
	{
		if (FFSNetwork.IsOnline)
		{
			Choreographer.s_Choreographer.StartMPEndOfRoundCompare();
		}
	}

	public void GainMoneyTokens()
	{
		if (SelectedCharacter != null)
		{
			SelectedCharacter.StartLooting();
			SelectedCharacter.GainLoot();
			if (Singleton<ScenarioInfoModule>.Instance != null)
			{
				Singleton<ScenarioInfoModule>.Instance.OnGoldValueChanged();
			}
		}
	}

	public void SetHealth()
	{
		if (int.TryParse(SetHealthText.text, out var result))
		{
			SetHealthParam(SelectedCharacter, result);
		}
	}

	public void SetHealthParam(int health)
	{
		SetHealthParam(SelectedCharacter, health);
	}

	public void SetHealthParam(CActor actor, int health)
	{
		if (actor == null)
		{
			return;
		}
		if (!s_InProxyExec)
		{
			SyncExec($"set_hp {health}", actor, actor.ArrayIndex);
		}
		else if (health < actor.Health)
		{
			if (health <= 0)
			{
				KillActor(actor);
			}
			else
			{
				actor.Damaged(actor.Health - health, fromAttackAbility: false, null, null);
			}
		}
		else
		{
			actor.Healed(health - actor.Health);
			CActorBeenHealed_MessageData message = new CActorBeenHealed_MessageData(actor, actor, 0, 0, poisonTokenRemoved: false, woundTokenRemoved: false, baneTokenRemoved: false, brittleTokenRemoved: false);
			Choreographer.s_Choreographer.PostMessageFromUI(message);
		}
	}

	public void SetMaxHealthParam(int newMaxHealth)
	{
		SetMaxHealthParam(SelectedCharacter, newMaxHealth);
	}

	public void SetMaxHealthParam(CActor actor, int newMaxHealth)
	{
		if (actor == null)
		{
			return;
		}
		if (!s_InProxyExec)
		{
			SyncExec($"set_max_hp {newMaxHealth}", actor, actor.ArrayIndex);
		}
		else if (newMaxHealth > 0)
		{
			actor.MaxHealth = newMaxHealth;
			if (actor.Health > newMaxHealth)
			{
				actor.Damaged(actor.Health - newMaxHealth, fromAttackAbility: false, null, null);
				return;
			}
			CRefreshActorHealth_MessageData message = new CRefreshActorHealth_MessageData(actor, actor, newMaxHealth);
			Choreographer.s_Choreographer.PostMessageFromUI(message);
		}
	}

	public void DestroyAllObetacles()
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile == null)
			{
				continue;
			}
			CTile propTile = cTile;
			CObjectObstacle cObjectObstacle = cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(cTile.m_ArrayIndex, ref propTile);
			}
			if (cObjectObstacle != null)
			{
				ScenarioManager.CurrentScenarioState.Props.Remove(cObjectObstacle);
			}
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(cTile.m_ArrayIndex, ref propTile);
			}
			if (cObjectObstacle != null)
			{
				if (cObjectObstacle.PropHealthDetails != null && cObjectObstacle.PropHealthDetails.HasHealth && cObjectObstacle.RuntimeAttachedActor != null)
				{
					GameState.KillActor(null, cObjectObstacle.RuntimeAttachedActor, CActor.ECauseOfDeath.KillAbility, out var _);
				}
				cObjectObstacle.DestroyProp(0f, sendMessageToClient: false);
			}
			else
			{
				foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(cTile.m_ArrayIndex).FindAll((CActor a) => a is CObjectActor))
				{
					GameState.KillActor(null, item, CActor.ECauseOfDeath.KillAbility, out var _);
				}
			}
			CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(string.Empty, null)
			{
				m_ActorDestroyingObstacle = null,
				m_Tiles = new List<CTile> { cTile },
				m_DestroyedProps = new List<CObjectProp> { cObjectObstacle },
				m_DestroyDelay = 0f,
				m_DestroyObstacleAbility = null
			};
			Choreographer.s_Choreographer.PostMessageFromUI(message);
		}
	}

	public void OnSendErrorReport()
	{
		SceneController.Instance.GlobalErrorMessage.ShowSendErrorReport("Debug Error Report");
		ToggleMenu();
	}

	public void ToggleDebugKeys()
	{
		isDebugKeysShowing = !isDebugKeysShowing;
		if (isDebugKeysShowing)
		{
			ScenarioOptions.SetActive(value: false);
			DebugKeys.SetActive(value: true);
			ShowDebugKeysText.text = "Show Debug Menu";
		}
		else if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			ScenarioOptions.SetActive(value: false);
			DebugKeys.SetActive(value: false);
			ShowDebugKeysText.text = "Show Debug Keys";
		}
		else
		{
			ScenarioOptions.SetActive(value: true);
			DebugKeys.SetActive(value: false);
			ShowDebugKeysText.text = "Show Debug Keys";
		}
	}

	public void TogglePause()
	{
		if (!FFSNetwork.IsOnline)
		{
			if (TimeManager.IsPaused)
			{
				TimeManager.UnpauseTime();
				Time.timeScale = 1f;
			}
			else
			{
				TimeManager.PauseTime();
				Time.timeScale = 0f;
			}
			DisplayTimeScaleText();
		}
		else
		{
			Log.NetworkWarning("Tried using debug menu pause time. Feature disabled in multiplayer.");
		}
	}

	public void ToggleGameSpeedText(bool value)
	{
		ShowSpeedUp = value;
	}

	public void ToggleGameSpeedUp()
	{
		SaveData.Instance.Global.RegularSpeedMultiplier = ((SaveData.Instance.Global.RegularSpeedMultiplier == 1f) ? 3f : 1f);
	}

	private void DisplayTimeScaleText()
	{
		displayTextList.Add(1, "Time Scale: " + TimeManager.TimeScale.ToString("0.00"));
	}

	public void ChangeWallFadeMode()
	{
		Log.Info("Wall fade changed");
		Shader.SetGlobalInt("_WallFadeMode", (Shader.GetGlobalInt("_WallFadeMode") + 1) % 3);
	}

	public void DisplayDebugText(int id, string text)
	{
		displayTextList.Add(id, text);
	}

	public void SpawnSpawnerAtLocation(CClientTile tile)
	{
		List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile.m_Tile);
		allAdjacentTiles.Add(tile.m_Tile);
		List<CClientTile> list = new List<CClientTile>();
		foreach (CTile item in allAdjacentTiles)
		{
			list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
		}
		if (LevelEditorController.SpawnSpawnerAtLocation(list))
		{
			CaptureTile = false;
			CaptureTileFunction = CaptureTileFunctions.None;
			IsMenuOpen = true;
			MenuTitle.text = "Debug Menu";
			Frame.SetActive(value: true);
			ToggleMenu();
		}
	}

	public void ClearAllAchievements()
	{
		PlatformLayer.Instance.NewPlatformLayer.ClearAllAchievements();
	}

	public void ProxyDebugAddItem(GameAction action)
	{
		CItem cItem = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData x) => x.ID == action.SupplementaryDataIDMax)?.GetItem;
		if (cItem != null)
		{
			CInventory cInventory = Choreographer.s_Choreographer.FindPlayerActor(action.ActorID)?.Inventory;
			if (cInventory != null)
			{
				Log.NetworkInfo("PROXY: Adding item (ID: " + cItem.ID + ", Name: " + cItem.Name + ")");
				cInventory.AddItem(cItem);
				return;
			}
			throw new Exception("Error adding item (ID: " + cItem.ID + ", Name: " + cItem.Name + "). Cannot find Actor with ID: " + action.ActorID);
		}
		throw new Exception("Error adding item. Cannot find item with ID: " + action.SupplementaryDataIDMax);
	}

	public static TileBehaviour GetTileBehaviourUnderCursor()
	{
		if (s_InProxyExec)
		{
			if (s_ProxyPickedPoint.X < 0 || s_ProxyPickedPoint.Y < 0)
			{
				return null;
			}
			return ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s_ProxyPickedPoint.X, s_ProxyPickedPoint.Y]?.m_TileBehaviour;
		}
		Camera main = Camera.main;
		if (main == null)
		{
			return null;
		}
		Ray ray = main.ScreenPointToRay(Input.mousePosition);
		float maxDistance = 50f;
		int layerMask = 1 << LayerMask.NameToLayer("Hex");
		if (Physics.Raycast(ray, out var hitInfo, maxDistance, layerMask))
		{
			return hitInfo.transform.GetComponentInChildren<TileBehaviour>();
		}
		return null;
	}

	public static object ResolveContext(out CActor selectedActor, out CActor pickedActor, out CTile pickedTile)
	{
		selectedActor = s_SelectedActor;
		pickedTile = GetTileBehaviourUnderCursor()?.m_ClientTile?.m_Tile;
		pickedActor = ((pickedTile == null) ? null : ScenarioManager.Scenario.FindActorAt(pickedTile.m_ArrayIndex));
		return pickedActor ?? selectedActor;
	}

	public static void ProxyExecCommand(string cmd, CActor selectedActor, Point pickedPoint)
	{
		Log.Info("ProxyExecCommand: \"" + cmd + "\"");
		GameConsole.Engine.Print(">> " + cmd);
		CActor cActor = s_SelectedActor;
		s_SelectedActor = selectedActor;
		bool flag = s_InProxyExec;
		s_InProxyExec = true;
		s_ProxyPickedPoint = pickedPoint;
		GameConsole.Engine.ExecuteCommandLine(cmd);
		s_SelectedActor = cActor;
		s_InProxyExec = flag;
	}

	public static void ProxyExecCommand(GameAction action)
	{
		IProtocolToken supplementaryDataToken = action.SupplementaryDataToken;
		ConsoleCommandToken token = supplementaryDataToken as ConsoleCommandToken;
		if (token == null)
		{
			Log.Error($"ProxyExecCommand: action token is not ConsoleCommandToken: {action.SupplementaryDataToken}");
			return;
		}
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null && player != PlayerRegistry.MyPlayer)
		{
			player.LastActionID++;
		}
		DesyncDetector.DelaySyncChecksOnHost();
		if (ActionProcessor.CurrentState.StateType == ActionProcessorStateType.NONE)
		{
			ActionProcessor.QueueUpAction(action);
			return;
		}
		CActor selectedActor = ScenarioManager.Scenario.AllActors.FirstOrDefault((CActor x) => x.ActorGuid == token.selectedActorGUID);
		ProxyExecCommand(pickedPoint: new Point(token.pickedX, token.pickedY), cmd: token.cmd, selectedActor: selectedActor);
	}

	public static void SyncExec(string cmd, CActor selectedActor, Point pickedPoint, bool executeLocally = true)
	{
		if (executeLocally)
		{
			ReproStepsLogger.LogConsoleCommand(cmd, selectedActor, pickedPoint);
		}
		if (FFSNetwork.IsOnline)
		{
			PlayerRegistry.MyPlayer.LastActionID++;
			DesyncDetector.DelaySyncChecksOnHost();
			Synchronizer.SendSideAction(GameActionType.ConsoleCommand, new ConsoleCommandToken(cmd, selectedActor?.ActorGuid, pickedPoint.X, pickedPoint.Y));
		}
		if (executeLocally)
		{
			ProxyExecCommand(cmd, selectedActor, pickedPoint);
		}
	}

	public static void SyncExec(string cmd, CClientTile pickedTile = null, bool executeLocally = true)
	{
		Point pickedPoint = pickedTile?.m_Tile.m_ArrayIndex ?? new Point(-1, -1);
		SyncExec(cmd, s_SelectedActor, pickedPoint, executeLocally);
	}

	private void PostProcessConsoleCommand(IConsole console, ICommand command)
	{
		if (s_InProxyExec)
		{
			return;
		}
		string text = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		Point pickedPoint = (tileBehaviourUnderCursor?.m_ClientTile)?.m_Tile.m_ArrayIndex ?? new Point(-1, -1);
		if (command is CommandCallCommand commandCallCommand)
		{
			text = commandCallCommand.Command.Name;
			if (!string.IsNullOrEmpty(commandCallCommand.AllArguments))
			{
				text = text + " " + commandCallCommand.AllArguments;
			}
			else if (commandCallCommand.Arguments != null)
			{
				foreach (string argument in commandCallCommand.Arguments)
				{
					text = text + " " + argument;
				}
			}
			ReproStepsLogger.LogConsoleCommand(text, s_SelectedActor, pickedPoint);
			if (!commandCallCommand.Command.Synchronize)
			{
				return;
			}
		}
		if (command is AssignVariableCommand assignVariableCommand)
		{
			text = assignVariableCommand.Variable.Name + " " + assignVariableCommand.Value;
			ReproStepsLogger.LogConsoleCommand(text, s_SelectedActor, pickedPoint);
			if (assignVariableCommand.Variable.Name == "desync_var")
			{
				return;
			}
		}
		if (FFSNetwork.IsOnline && !string.IsNullOrEmpty(text))
		{
			SyncExec(text, tileBehaviourUnderCursor?.m_ClientTile, executeLocally: false);
		}
	}

	[ConsoleCommand(Command = "scmd", Description = "(command) - Synchronously execute a console command on all machines", PassAllArguments = true)]
	public static void Cmd_SyncExecCommand(IConsole console, string args)
	{
		SyncExec(args, GetTileBehaviourUnderCursor()?.m_ClientTile);
	}

	[ConsoleCommand(Command = "spawn", Synchronize = true, Description = "(id) [faction=enemy] - Spawns a monster at the tile under the cursor")]
	public static void Cmd_Spawn(IConsole console, params string[] args)
	{
		TileBehaviour tileBehaviour = GetTileBehaviourUnderCursor();
		if (tileBehaviour != null)
		{
			if (args.Length == 0)
			{
				console.WriteLine("No id provided");
			}
			else
			{
				TrySpawnMonster();
			}
		}
		bool TrySpawnMonster()
		{
			string text = null;
			List<string> list = MonsterClassManager.Classes.Select((CMonsterClass x) => x.ID).ToList();
			list.Sort();
			int num = list.FindIndex(0, (string item) => item.Equals(args[0], StringComparison.InvariantCultureIgnoreCase));
			if (num < 0)
			{
				console.WriteLine("Could not find monsterId " + args[0] + ", searching for a partial match instead");
				num = list.FindIndex(0, (string item) => item.IndexOf(args[0], StringComparison.InvariantCultureIgnoreCase) >= 0);
			}
			if (num < 0)
			{
				console.WriteLine("Invalid monster id");
				return false;
			}
			text = list[num];
			CActor.EType type = CActor.EType.Enemy;
			if (args.Length > 1)
			{
				int num2 = Enum.GetNames(typeof(CActor.EType)).ToList().FindIndex(0, (string t) => t.ToUpper().Contains(args[1].ToUpper()));
				if (num2 >= 0)
				{
					type = (CActor.EType)Enum.GetValues(typeof(CActor.EType)).GetValue(num2);
				}
				else
				{
					console.WriteLine("Invalid monster faction. Fallback to default (faction = enemy)");
				}
			}
			int level = -1;
			CActor cActor = LevelEditorController.SpawnMonsterAtLocation(tileBehaviour.m_ClientTile, text, level, type);
			if (cActor != null)
			{
				ScenarioManager.CurrentScenarioState.PerformOnSpawnedModifiers(cActor, ScenarioManager.CurrentScenarioState.Players.Count);
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
			return true;
		}
	}

	[ConsoleCommand(Command = "increasePressure", Synchronize = true, Description = "Increases the pressure of Metal Mosaic by 1")]
	public static void Cmd_IncreasePressure(IConsole console, params string[] args)
	{
		if (s_SelectedActor == null || s_SelectedActor.Class.ID != "MetalMosaicID")
		{
			console.WriteLine("Select the Metal Mosaic Actor");
			return;
		}
		SyncExec("calc CustomAbility.PressureChange(obj, 1)", s_SelectedActor, s_SelectedActor.ArrayIndex);
		CPlayerActor cPlayerActor = s_SelectedActor as CPlayerActor;
		CPlayerActor.EPressure ePressure = (CPlayerActor.EPressure)Math.Clamp((int)(cPlayerActor.Pressure + cPlayerActor.PressureChangeValue), 1, 4);
		console.WriteLine($"Metal Mosaic's Pressure will be set to {ePressure} at the end of the next action");
	}

	[ConsoleCommand(Command = "decreasePressure", Synchronize = true, Description = "Decreases the pressure of Metal Mosaic by 1")]
	public static void Cmd_DecreasePressure(IConsole console, params string[] args)
	{
		if (s_SelectedActor == null || s_SelectedActor.Class.ID != "MetalMosaicID")
		{
			console.WriteLine("Select the Metal Mosaic Actor");
			return;
		}
		SyncExec("calc CustomAbility.PressureChange(obj, -1)", s_SelectedActor, s_SelectedActor.ArrayIndex);
		CPlayerActor cPlayerActor = s_SelectedActor as CPlayerActor;
		CPlayerActor.EPressure ePressure = (CPlayerActor.EPressure)Math.Clamp((int)(cPlayerActor.Pressure + cPlayerActor.PressureChangeValue), 1, 4);
		console.WriteLine($"Metal Mosaic's Pressure will be set to {ePressure} at the end of the next action");
	}

	[ConsoleCommand(Command = "place", Synchronize = true, Description = "(id) - Places a prop at the tile under the cursor")]
	public static void Cmd_Place(IConsole console, params string[] args)
	{
		TileBehaviour tileBehaviour = GetTileBehaviourUnderCursor();
		if (tileBehaviour != null)
		{
			if (args.Length == 0)
			{
				console.WriteLine("No id provided");
			}
			else
			{
				TrySpawnProp();
			}
		}
		bool TrySpawnProp()
		{
			IEnumerable<string> collection = from s in CObjectProp.PropTypes
				where s != EPropType.None
				select s.ToString();
			IEnumerable<string> collection2 = from s in CObjectProp.SpecificPropTypes
				where s != ESpecificPropType.None
				select s.ToString();
			List<string> list = new List<string>();
			list.AddRange(collection);
			list.AddRange(collection2);
			list.Sort();
			int num = list.FindIndex(0, (string x) => x.ToUpper() == args[0].ToUpper());
			if (num < 0)
			{
				num = list.FindIndex(0, (string p) => p.ToUpper().Contains(args[0].ToUpper()));
			}
			if (num < 0)
			{
				console.WriteLine("Invalid prop name");
				return false;
			}
			string propName = list[num];
			LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { tileBehaviour.m_ClientTile }, propName);
			return true;
		}
	}

	[ConsoleCommand(Command = "set_negative", Synchronize = true, Description = "(is_negative) - Sets the hex under the cursor to be negative")]
	public static void Cmd_SetNegative(IConsole console, params string[] args)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			if (args.Length == 0)
			{
				console.WriteLine("No is_negative provided");
				return;
			}
			int num = int.Parse(args[0]);
			Point arrayIndex = tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex;
			ScenarioManager.PathFinder.GetNode(arrayIndex).IsNegativeHex = num != 0;
		}
	}

	[ConsoleCommand(Command = "infuse", Synchronize = true, Description = "(element_name | all) - Infuse an element (or all elements)")]
	public static void Cmd_Infuse(IConsole console, params string[] args)
	{
		foreach (string text in args)
		{
			if (text == "all")
			{
				InfuseAll();
				break;
			}
			AddElement(text);
		}
	}

	[ConsoleCommand(Command = "trap", Synchronize = true, Description = " [damage|heal [+|-] amount] [[+|-] condition] [...]", PassAllArguments = true)]
	public static void Cmd_Trap(IConsole console, string args)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor == null)
		{
			CError(console, "No picked tile");
			return;
		}
		CObjectTrap cObjectTrap = tileBehaviourUnderCursor.m_ClientTile.m_Tile.FindPropOfType<CObjectTrap>();
		if (cObjectTrap == null)
		{
			if (!tileBehaviourUnderCursor.m_ClientTile.m_Tile.IsEmpty)
			{
				CError(console, "Tile not empty");
				return;
			}
			LevelEditorController.SpawnPropAtLocation(new List<CClientTile> { tileBehaviourUnderCursor.m_ClientTile }, "Trap");
			cObjectTrap = tileBehaviourUnderCursor.m_ClientTile.m_Tile.FindPropOfType<CObjectTrap>();
			if (cObjectTrap == null)
			{
				CError(console, $"Could not spawn trap at {tileBehaviourUnderCursor.m_ClientTile.m_Tile}");
				return;
			}
		}
		CExpression.Parser.Stream stream = new CExpression.Parser.Stream
		{
			text = args,
			curOfs = 0,
			endOfs = args.Length
		};
		while (true)
		{
			CExpression.Parser.SkipBlanks(ref stream);
			if (stream.curOfs >= stream.endOfs)
			{
				break;
			}
			char c = ReadOp(ref stream);
			CExpression.Parser.SkipBlanks(ref stream);
			if (!CExpression.Parser.ReadIdentifier(ref stream, out var val))
			{
				CError(console, "Identifier expected at " + stream.remainingText);
				break;
			}
			string text = val.GetString().ToLowerInvariant();
			CExpression.Parser.SkipBlanks(ref stream);
			if (text == "damage" || text == "heal")
			{
				if (c == '\0')
				{
					c = ReadOp(ref stream);
					CExpression.Parser.SkipBlanks(ref stream);
				}
				if (!CExpression.Parser.ReadInt(ref stream, out var val2))
				{
					CError(console, "Integer expected at " + stream.remainingText);
					break;
				}
				int num = val2.GetInt();
				CExpression.Parser.SkipBlanks(ref stream);
				if (text == "damage")
				{
					int num2 = 0;
					switch (c)
					{
					case '+':
						num2 = cObjectTrap.GetTrapDamage() + num;
						break;
					case '-':
						num2 = cObjectTrap.GetTrapDamage() - num;
						if (num2 < 0)
						{
							num2 = 0;
						}
						break;
					default:
						num2 = num;
						break;
					}
					cObjectTrap.Damage = num2 > 0;
					if (cObjectTrap.Damage)
					{
						cObjectTrap.SetCustomDamageValue(num2);
					}
					continue;
				}
				switch (c)
				{
				case '+':
					cObjectTrap.HealValue += num;
					break;
				case '-':
					cObjectTrap.HealValue -= num;
					if (cObjectTrap.HealValue < 0)
					{
						cObjectTrap.HealValue = 0;
					}
					break;
				default:
					cObjectTrap.HealValue = num;
					break;
				}
				continue;
			}
			bool flag = false;
			CCondition.ENegativeCondition[] negativeConditions = CCondition.NegativeConditions;
			for (int i = 0; i < negativeConditions.Length; i++)
			{
				CCondition.ENegativeCondition item = negativeConditions[i];
				if (!item.ToString().ToLowerInvariant().Contains(text))
				{
					continue;
				}
				flag = true;
				bool flag2 = cObjectTrap.Conditions != null && cObjectTrap.Conditions.Contains(item);
				switch (c)
				{
				case '+':
					if (!flag2)
					{
						if (cObjectTrap.Conditions == null)
						{
							cObjectTrap.Conditions = new List<CCondition.ENegativeCondition>();
						}
						cObjectTrap.Conditions.Add(item);
					}
					break;
				case '-':
					if (flag2)
					{
						cObjectTrap.Conditions.Remove(item);
					}
					break;
				default:
					if (flag2)
					{
						cObjectTrap.Conditions.Remove(item);
						break;
					}
					if (cObjectTrap.Conditions == null)
					{
						cObjectTrap.Conditions = new List<CCondition.ENegativeCondition>();
					}
					cObjectTrap.Conditions.Add(item);
					break;
				}
				break;
			}
			if (flag)
			{
				continue;
			}
			CCondition.EPositiveCondition[] positiveConditions = CCondition.PositiveConditions;
			for (int i = 0; i < positiveConditions.Length; i++)
			{
				CCondition.EPositiveCondition item2 = positiveConditions[i];
				if (!item2.ToString().ToLowerInvariant().Contains(text))
				{
					continue;
				}
				flag = true;
				bool flag3 = cObjectTrap.PositiveConditions != null && cObjectTrap.PositiveConditions.Contains(item2);
				switch (c)
				{
				case '+':
					if (!flag3)
					{
						if (cObjectTrap.PositiveConditions == null)
						{
							cObjectTrap.PositiveConditions = new List<CCondition.EPositiveCondition>();
						}
						cObjectTrap.PositiveConditions.Add(item2);
					}
					break;
				case '-':
					if (flag3)
					{
						cObjectTrap.PositiveConditions.Remove(item2);
					}
					break;
				default:
					if (flag3)
					{
						cObjectTrap.PositiveConditions.Remove(item2);
						break;
					}
					if (cObjectTrap.PositiveConditions == null)
					{
						cObjectTrap.PositiveConditions = new List<CCondition.EPositiveCondition>();
					}
					cObjectTrap.PositiveConditions.Add(item2);
					break;
				}
				break;
			}
			if (!flag)
			{
				CError(console, "Unknown identifier: '" + text + "'");
			}
		}
		static char ReadOp(ref CExpression.Parser.Stream reference)
		{
			if (CExpression.Parser.IsAt(ref reference, "+"))
			{
				reference.curOfs++;
				return '+';
			}
			if (CExpression.Parser.IsAt(ref reference, "-"))
			{
				reference.curOfs++;
				return '-';
			}
			return '\0';
		}
	}

	[ConsoleCommand(Command = "showMonsterDecks", Description = "Shows all monster decks")]
	public static void Cmd_ShowMonsterDecks(IConsole console)
	{
		foreach (CMonsterClass item in ScenarioManager.Scenario.AllAliveMonsters.Select((CEnemyActor monster) => monster.Class).Distinct())
		{
			console.WriteLine("-----------------------------------------------------------------------------------");
			console.WriteLine(item.ID);
			console.WriteLine("");
			console.Write(item.ID + " Round Ability Card: ");
			console.Write($"{item.RoundAbilityCard?.ID} ");
			console.WriteLine("");
			console.Write(item.ID + " Hand: ");
			foreach (CMonsterAbilityCard abilityCard in item.AbilityCards)
			{
				console.Write($"{abilityCard.ID} ");
			}
			console.WriteLine("");
			console.Write(item.ID + " Discard: ");
			foreach (CMonsterAbilityCard discardedAbilityCard in item.DiscardedAbilityCards)
			{
				console.Write($"{discardedAbilityCard.ID} ");
			}
			console.WriteLine("");
		}
	}

	[ConsoleCommand(Command = "select", Description = "Selects the actor under the cursor")]
	public static void Cmd_SelectActor(IConsole console)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			Instance.SelectedCharacter = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		}
	}

	[ConsoleCommand(Command = "teleport", Synchronize = true, Description = "Teleports the selected actor to the position under the cursor")]
	public static void Cmd_TeleportActor(IConsole console)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			SetActorPosition(s_SelectedActor, tileBehaviourUnderCursor.m_ClientTile);
		}
	}

	[ConsoleCommand(Command = "face", Synchronize = true, Description = "The selected actor faces the position under the cursor")]
	public static void Cmd_FaceActor(IConsole console)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			SetActorFacing(s_SelectedActor, tileBehaviourUnderCursor.m_ClientTile);
		}
	}

	public static void SetupContextVars(CVars vars, object obj, CActor selectedActor, CActor pickedActor, CTile pickedTile)
	{
		vars.Set("obj", new TValue(obj));
		vars.Set("SelectedActor", new TValue(selectedActor));
		vars.Set("PickedActor", new TValue(pickedActor));
		vars.Set("PickedTile", new TValue(pickedTile));
		vars.Set("css", new TValue(ScenarioManager.CurrentScenarioState));
		vars.Set("CurrentScenarioState", new TValue(ScenarioManager.CurrentScenarioState));
		vars.Set("ssc", new TValue(ScenarioScript.current));
		vars.Set("scenario", new TValue(ScenarioManager.Scenario));
		vars.Set("Scenario", new TValue(ScenarioManager.Scenario));
		vars.Set("s_Choreographer", new TValue((object)Choreographer.s_Choreographer));
		vars.Set("choreographer", new TValue((object)Choreographer.s_Choreographer));
		vars.Set("outpost", new TValue((object)Singleton<FHMapChoreographer>.InstanceFast));
	}

	[ConsoleCommand(Command = "calc", Synchronize = true, Description = "(expr) - Calculate an expression", PassAllArguments = true)]
	public static void Cmd_CalcExpression(IConsole console, string args)
	{
		CExpression cExpression = CExpression.Parse(args);
		if (cExpression != null)
		{
			string text = cExpression.ToString();
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			object obj = ResolveContext(out var selectedActor, out var pickedActor, out var pickedTile);
			SetupContextVars(cVars, obj, selectedActor, pickedActor, pickedTile);
			TValue tValue = cExpression.Calc(context);
			string text2 = text + " -> " + TValue.Dump(tValue);
			console.WriteLine("{0}", text2);
			Log.Info(text2);
		}
	}

	[ConsoleCommand(Command = "lcalc", Synchronize = false, Description = "(expr) - Calculate an expression locally", PassAllArguments = true)]
	public static void Cmd_CalcExpressionLocally(IConsole console, string args)
	{
		CExpression cExpression = CExpression.Parse(args);
		if (cExpression != null)
		{
			string text = cExpression.ToString();
			CVars cVars = new CVars();
			CVarsContext context = new CVarsContext(cVars);
			object obj = ResolveContext(out var selectedActor, out var pickedActor, out var pickedTile);
			SetupContextVars(cVars, obj, selectedActor, pickedActor, pickedTile);
			TValue tValue = cExpression.Calc(context);
			string text2 = text + " -> " + TValue.Dump(tValue);
			console.WriteLine("{0}", text2);
			Log.Info(text2);
		}
	}

	[ConsoleCommand(Command = "watch", Description = "(expr) - Add a watch", PassAllArguments = true)]
	public static void Cmd_AddWatch(IConsole console, string args)
	{
		if (string.IsNullOrEmpty(args))
		{
			WatchesWindow.Show();
		}
		else
		{
			WatchesWindow.Add(ResolveContext(out var _, out var _, out var _), args);
		}
	}

	[ConsoleCommand(Command = "swatch", Synchronize = true, Description = "(expr) - Add a watch on all machines", PassAllArguments = true)]
	public static void Cmd_AddWatchSync(IConsole console, string args)
	{
		if (string.IsNullOrEmpty(args))
		{
			WatchesWindow.Show();
		}
		else
		{
			WatchesWindow.Add(ResolveContext(out var _, out var _, out var _), args);
		}
	}

	[ConsoleCommand(Command = "t_scale", Synchronize = true, Description = "(time scale) - Changes the time scale")]
	public static void Cmd_TimeScale(IConsole console, float scale)
	{
		Time.timeScale = scale;
	}

	[ConsoleCommand(Command = "t_toggle10x", Synchronize = true, Description = " - Toggles the time scale between 1x and 10x")]
	public static void Cmd_TimeScaleToggle10x(IConsole console)
	{
		if (Time.timeScale == 1f)
		{
			Time.timeScale = 10f;
		}
		else
		{
			Time.timeScale = 1f;
		}
		Timekeeper.instance.m_GlobalClock.localTimeScale = Time.timeScale;
		TimeManager.DefaultTimeScale = Time.timeScale;
		SaveData.Instance.Global.DebugSpeedMode = Time.timeScale != 1f;
	}

	[ConsoleCommand(Command = "fps_limit", Description = "(fps) - Caps the frame rate; 0 or negative removes the cap")]
	public static void Cmd_FpsLimit(IConsole console, int fps)
	{
		if (fps <= 0)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = -1;
			console.Write("FPS limit removed");
		}
		else
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = fps;
			console.Write($"FPS limit set to {fps}");
		}
	}

	[ConsoleCommand(Command = "log_bloat", Description = "(megabytes) - Writes at least this many MB of random filler to the player log (for testing large log transfers)")]
	public static void Cmd_LogBloat(IConsole console, int megabytes)
	{
		if (megabytes <= 0)
		{
			throw new ConsoleCommandException("Megabytes must be positive");
		}
		(console as CommandExecution)?.OpenStream();
		console.Write($"Bloating the log by {megabytes} MB...");
		PlatformLayer.Instance.StartCoroutine(LogBloatCrt(console, megabytes));
	}

	private static IEnumerator LogBloatCrt(IConsole console, int megabytes)
	{
		long targetChars = (long)megabytes * 1024L * 1024;
		long writtenChars = 0L;
		int lineIndex = 0;
		System.Random random = new System.Random();
		byte[] buffer = new byte[6000];
		while (writtenChars < targetChars)
		{
			for (int i = 0; i < 64; i++)
			{
				if (writtenChars >= targetChars)
				{
					break;
				}
				random.NextBytes(buffer);
				string text = $"[LOG_BLOAT {lineIndex++}] {Convert.ToBase64String(buffer)}";
				Log.Info(text);
				writtenChars += text.Length;
			}
			yield return null;
		}
		console.Write($"Log bloat done: wrote {writtenChars / 1048576} MB in {lineIndex} lines");
		(console as CommandExecution)?.CloseStream();
	}

	[ConsoleCommand(Command = "set_hp", Synchronize = true, Description = "(amount) - Sets the health of the actor under the cursor")]
	public static void Cmd_SetHealth(IConsole console, int health)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			Instance.SelectActorParam(tileBehaviourUnderCursor.m_ClientTile);
			Instance.SetHealthParam(health);
		}
	}

	[ConsoleCommand(Command = "set_max_hp", Synchronize = true, Description = "(amount) - Sets the health of the actor under the cursor")]
	public static void Cmd_SetMaxHealth(IConsole console, int health)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			Instance.SelectActorParam(tileBehaviourUnderCursor.m_ClientTile);
			Instance.SetMaxHealthParam(health);
			Instance.SetHealthParam(health);
		}
	}

	[ConsoleCommand(Command = "break_all_objects", Synchronize = false, Description = "Break all obstacle objects")]
	public static void Cmd_BreakAllObjects(IConsole console)
	{
		Instance.DestroyAllObetacles();
	}

	[ConsoleCommand(Command = "kill_all_monsters", Synchronize = true, Description = "Kill all enemies, except bosses")]
	public static void Cmd_KillAllEnemies(IConsole console)
	{
		foreach (CEnemyActor allEnemyMonster in ScenarioManager.Scenario.AllEnemyMonsters)
		{
			if (!allEnemyMonster.MonsterClass.Boss && !allEnemyMonster.IsImmuneToDamage && !allEnemyMonster.ForcedUntargetable)
			{
				Instance.SelectActorParam(allEnemyMonster);
				Instance.SetHealthParam(0);
			}
		}
	}

	[ConsoleCommand(Command = "add_loot_card", Synchronize = true, Description = "Add loot card to top of deck, or just move it if its already in the deck")]
	public static void Cmd_AddLootCard(IConsole console, int id)
	{
		LootCard lootCard = CLootDeck.AllCards.SingleOrDefault((LootCard x) => x.ID == id);
		if (lootCard != default(LootCard))
		{
			int num = CLootDeck.AllCards.IndexOf(lootCard);
			if (num != -1)
			{
				ScenarioManager.CurrentScenarioState.LootDeck.Remove((byte)num);
				ScenarioManager.CurrentScenarioState.LootDeck.Insert(0, (byte)num);
			}
		}
	}

	[ConsoleCommand(Command = "loot", Synchronize = true, Description = "(itemId/itemStringId) - The actor under the cursor loots an item")]
	public static void Cmd_Loot(IConsole console, params string[] args)
	{
		int id = 0;
		string nameSubStr = null;
		if (args.Length != 0 && !int.TryParse(args[0], out id))
		{
			nameSubStr = args[0];
		}
		List<ItemCardYMLData> list = new List<ItemCardYMLData>();
		foreach (ItemCardYMLData itemCard in ScenarioRuleClient.SRLYML.ItemCards)
		{
			if (MatchCard(itemCard))
			{
				list.Add(itemCard);
			}
		}
		if (list.Count == 0)
		{
			console.WriteLine("No matching item(s) found");
			return;
		}
		if (list.Count > 1)
		{
			console.WriteLine($"Found {list.Count} matching items:");
			{
				foreach (ItemCardYMLData item in list)
				{
					console.WriteLine($"  {item}");
				}
				return;
			}
		}
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
			if (cActor == null)
			{
				console.WriteLine("No actor under the cursor");
			}
			else
			{
				cActor.TryLootItem(list[0].ID);
			}
		}
		bool MatchCard(ItemCardYMLData card)
		{
			if (id > 0)
			{
				return card.ID == id;
			}
			if (!string.IsNullOrEmpty(nameSubStr))
			{
				return card.StringID.Contains(nameSubStr, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
	}

	[ConsoleCommand(Command = "perk", Synchronize = true, Description = "(PerkID | PerkNumber) - Add perk (to the player under cursor if PerkNumber is used)")]
	public static void Cmd_Perk(IConsole console, params string[] args)
	{
		int result = 0;
		string id = null;
		CPlayerActor cPlayerActor = null;
		if (args.Length == 0)
		{
			console.WriteLine("Missing argument");
			return;
		}
		if (int.TryParse(args[0], out result))
		{
			TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
			if (tileBehaviourUnderCursor != null)
			{
				cPlayerActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex) as CPlayerActor;
			}
			if (cPlayerActor == null)
			{
				console.WriteLine("No player under the cursor");
				return;
			}
			string text = cPlayerActor.CharacterClass.ID;
			if (text.EndsWith("ID"))
			{
				text = text.Substring(0, text.Length - 2);
			}
			id = $"{text}{result:D2}";
		}
		else
		{
			id = args[0];
		}
		PerksYMLData perk = ScenarioRuleClient.SRLYML.Perks.FirstOrDefault((PerksYMLData x) => x.ID.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (perk == null)
		{
			console.WriteLine("Unknown perk: " + id);
			return;
		}
		if (cPlayerActor == null)
		{
			cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor x) => x.CharacterClass.ID == perk.CharacterID);
			if (cPlayerActor == null)
			{
				console.WriteLine("Player not present: " + perk.CharacterID);
				return;
			}
		}
		cPlayerActor.CharacterClass.ApplyPerk(perk);
		if (!perk.Flags.HasFlags(ScenarioRuleLibrary.YML.EPerkFlag.ApplyActiveBonusesAtScenarioStart))
		{
			return;
		}
		foreach (CAbility ability in perk.Abilities)
		{
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(ability, cPlayerActor));
		}
		if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
		{
			PhaseManager.StartActiveBonusOrScenarioModifierAbility();
		}
	}

	[ConsoleCommand(Command = "damage", Synchronize = true, Description = "(amount) - Apply damage to the actor under the cursor")]
	public static void Cmd_Damage(IConsole console, int damage)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (!(tileBehaviourUnderCursor == null))
		{
			Instance.SelectActorParam(tileBehaviourUnderCursor.m_ClientTile);
			CActor selectedCharacter = Instance.SelectedCharacter;
			if (selectedCharacter != null)
			{
				int health = selectedCharacter.Health;
				int healthParam = Mathf.Max(0, health - damage);
				Instance.SetHealthParam(healthParam);
			}
		}
	}

	[ConsoleCommand(Command = "add_shield", Synchronize = true, Description = "(amount) - Adds shield to the actor under the cursor")]
	public static void Cmd_AddShield(IConsole console, int amount)
	{
		CActor cActor = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
			if (cActor == null)
			{
				console.WriteLine("No actor under the cursor");
				return;
			}
		}
		CAbility cAbility = CAbilityShield.CreateDefaultShield(amount, 1, 1, isMonster: false, new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		cAbility.ActiveBonusData.AbilityData = cAbility;
		cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		cAbility.Start(cActor, cActor);
		cAbility.ApplyToActor(cActor);
	}

	[ConsoleCommand(Command = "shadow_buff", Synchronize = true, Description = "Adds shadow buff active bonus to the actor under the cursor")]
	public static void Cmd_AddShadowBuff(IConsole console)
	{
		CActor cActor = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
			if (cActor == null)
			{
				console.WriteLine("No actor under the cursor");
				return;
			}
		}
		CAbility cAbility = CAbilityMove.CreateDefaultMove(0, isMonster: false, 0, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false);
		cAbility.ActiveBonusData.AbilityData = cAbility;
		cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		cAbility.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.ShadowBuff;
		cAbility.Start(cActor, cActor);
		cAbility.ApplyToActor(cActor);
	}

	[ConsoleCommand(Command = "add_retaliate", Synchronize = true, Description = "(amount) - Adds shield to the actor under the cursor")]
	public static void Cmd_AddRetaliate(IConsole console, int amount)
	{
		CActor cActor = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
			if (cActor == null)
			{
				console.WriteLine("No actor under the cursor");
				return;
			}
		}
		CAbility cAbility = CAbilityRetaliate.CreateDefaultRetaliate(amount, 1, 1, isMonster: false, new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		cAbility.ActiveBonusData.AbilityData = cAbility;
		cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		cAbility.Start(cActor, cActor);
		cAbility.ApplyToActor(cActor);
	}

	[ConsoleCommand(Command = "apply_condition", Synchronize = true, Description = "(condition) - Applies a condition to the actor under the cursor")]
	public static void Cmd_ApplyCondition(IConsole console, string condition)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (!(tileBehaviourUnderCursor != null))
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		if (cActor == null)
		{
			console.WriteLine("No actor under the cursor");
			return;
		}
		List<CCondition.EPositiveCondition> list = ((CCondition.EPositiveCondition[])Enum.GetValues(typeof(CCondition.EPositiveCondition))).ToList();
		int num = list.FindIndex(0, (CCondition.EPositiveCondition c) => c.ToString().ToUpper().Contains(condition.ToUpper()));
		if (num >= 0)
		{
			if (Enum.TryParse<CAbility.EAbilityType>(list[num].ToString(), ignoreCase: false, out var result))
			{
				CAbility cAbility = CAbility.CreateAbility(result, CAbilityFilterContainer.CreateDefaultFilter(), cActor.Type == CActor.EType.Enemy, isTargetedAbility: true);
				cAbility.Start(cActor, cActor);
				cAbility.ApplyToActor(cActor);
			}
			return;
		}
		List<CCondition.ENegativeCondition> list2 = ((CCondition.ENegativeCondition[])Enum.GetValues(typeof(CCondition.ENegativeCondition))).ToList();
		num = list2.FindIndex(0, (CCondition.ENegativeCondition c) => c.ToString().ToUpper().Contains(condition.ToUpper()));
		if (num >= 0)
		{
			if (Enum.TryParse<CAbility.EAbilityType>(list2[num].ToString(), ignoreCase: false, out var result2))
			{
				CAbility cAbility2 = CAbility.CreateAbility(result2, CAbilityFilterContainer.CreateDefaultFilter(), cActor.Type == CActor.EType.Enemy, isTargetedAbility: true);
				cAbility2.Start(cActor, cActor);
				cAbility2.ApplyToActor(cActor);
			}
		}
		else
		{
			console.WriteLine("Invalid condition");
		}
	}

	[ConsoleCommand(Command = "play_anim", Description = "[animation=Idle] - Force play animation on the current actor")]
	public static void Cmd_PlayAnim(IConsole console, params string[] args)
	{
		CActor cActor = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		}
		if (cActor == null)
		{
			cActor = GameState.CurrentActor;
		}
		if (cActor != null)
		{
			string state = "Idle-Run";
			if (args.Length != 0)
			{
				state = args[0];
			}
			MF.AnimatorPlay(MF.GetGameObjectAnimator(Choreographer.s_Choreographer.FindClientActor(cActor)), state);
		}
	}

	[ConsoleCommand(Command = "god", Synchronize = true, Description = "Make all actors invincible.")]
	public static void Cmd_God(IConsole console, params string[] args)
	{
		CActor.GodMode = !CActor.GodMode;
		console.WriteLine("God mode " + (CActor.GodMode ? "ON" : "OFF"));
	}

	[ConsoleCommand(Command = "audio_log", Synchronize = true, Description = "Log audio playing in the console.")]
	public static void Cmd_Audio_Log(IConsole console, params string[] args)
	{
		AudioController.LogToGameConsole = !AudioController.LogToGameConsole;
		console.WriteLine("Audio log " + (AudioController.LogToGameConsole ? "ON" : "OFF"));
	}

	[ConsoleCommand(Command = "repeat", Description = "Repeat last command. Use with key binding.")]
	public static void Cmd_Repeat(IConsole console, params string[] args)
	{
		if (console.ExecEngine.CommandHistory.Count > 0)
		{
			string text = console.ExecEngine.CommandHistory.FirstOrDefault((string x) => x != "repeat");
			if (text != null)
			{
				console.ExecEngine.ExecuteCommandLine(text);
			}
		}
	}

	[ConsoleCommand(Command = "action_repeat", Synchronize = true, Description = "Repeat last card action.")]
	public static void Cmd_ActionRepeat(IConsole console, params string[] args)
	{
		CAction cAction = (CPhaseAction.LastPlayedAbility?.m_BaseCard as CAbilityCard)?.LastSelectedAction;
		if (cAction != null)
		{
			PhaseManager.StartAbilities(cAction.Abilities, CPhaseAction.LastPlayedAbility.m_BaseCard);
		}
	}

	[ConsoleCommand(Command = "pass", Description = " - perform a no-effect action")]
	public static void Cmd_Pass(IConsole console, params string[] args)
	{
		Choreographer s_Choreographer = Choreographer.s_Choreographer;
		CardsHandManager instanceFast = Singleton<CardsHandManager>.InstanceFast;
		TakeDamagePanel instanceFast2 = Singleton<TakeDamagePanel>.InstanceFast;
		ScenarioPartyColumnModule partyModule = Singleton<FHScenarioViewController>.InstanceFast.Modules.PartyModule;
		FullAbilityCard[] fullAbilityCardUIs = instanceFast.CurrentHand.GetComponentsInChildren<FullAbilityCard>();
		MiniAbilityCard[] componentsInChildren = instanceFast.CurrentHand.GetComponentsInChildren<MiniAbilityCard>();
		UnityEngine.UI.Button skipButton = s_Choreographer.m_SkipButton.GetComponent<UnityEngine.UI.Button>();
		UnityEngine.UI.Button component = s_Choreographer.readyButton.GetComponent<UnityEngine.UI.Button>();
		FHButton takeDamageButton = instanceFast2.TakeDamageButton;
		FHToggle burnAvailableCardsToggle = instanceFast2.BurnAvailableCardsToggle;
		UnityEngine.UI.Toggle component2 = Singleton<UIReadyToggle>.InstanceFast.GetComponent<UnityEngine.UI.Toggle>();
		if (Singleton<StoryController>.Instance != null && Singleton<StoryController>.Instance.IsVisible)
		{
			Singleton<StoryController>.Instance.Clear();
		}
		UnityEngine.UI.Button button = GameObject.Find("FH_Button_Proceed_Hotkey")?.GetComponent<UnityEngine.UI.Button>();
		if (button != null)
		{
			TextMeshProUGUI textMeshProUGUI = button.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
			if (textMeshProUGUI == null || !textMeshProUGUI.text.StartsWith("<sprite name=LostIcon>"))
			{
				button = null;
			}
		}
		if (skipButton != null && skipButton.isActiveAndEnabled && skipButton.interactable)
		{
			AutoTestController.LogButtonClick(skipButton.gameObject, skipButton, "Cmd_Pass");
			skipButton.onClick?.Invoke();
			return;
		}
		if (component != null && component.isActiveAndEnabled && component.interactable)
		{
			AutoTestController.LogButtonClick(component.gameObject, component, "Cmd_Pass");
			component.onClick?.Invoke();
			return;
		}
		if (component2 != null && component2.isActiveAndEnabled && component2.interactable && !component2.isOn)
		{
			AutoTestController.LogButtonClick(component2.gameObject, component2, "Cmd_Pass");
			component2.isOn = true;
			return;
		}
		if (button != null && button.isActiveAndEnabled && button.interactable)
		{
			AutoTestController.LogButtonClick(button.gameObject, button, "Cmd_Pass");
			button.onClick?.Invoke();
			return;
		}
		if (takeDamageButton != null && takeDamageButton.isActiveAndEnabled && takeDamageButton.interactable)
		{
			TextMeshProUGUI textMeshProUGUI2 = takeDamageButton.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
			if (textMeshProUGUI2 == null || textMeshProUGUI2.color != new Color32(227, 31, 31, byte.MaxValue))
			{
				AutoTestController.LogButtonClick(takeDamageButton.gameObject, takeDamageButton, "Cmd_Pass");
				takeDamageButton.onClick?.Invoke();
				return;
			}
			if (burnAvailableCardsToggle != null)
			{
				AutoTestController.LogButtonClick(burnAvailableCardsToggle.gameObject, burnAvailableCardsToggle, "Cmd_Pass");
				burnAvailableCardsToggle.isOn = true;
			}
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection && fullAbilityCardUIs.Length != 0)
		{
			fullAbilityCardUIs[0].StartCoroutine(coro());
			return;
		}
		if (PhaseManager.IsInCardsSelectionPhase)
		{
			if (component != null && component.isActiveAndEnabled && component.interactable)
			{
				AutoTestController.LogButtonClick(component.gameObject, component, "Cmd_Pass");
				component.onClick?.Invoke();
				return;
			}
			List<AbilityCardUI> source = componentsInChildren.Select((MiniAbilityCard x) => x.GetComponentInParent<AbilityCardUI>()).ToList();
			List<AbilityCardUI> list = source.Where((AbilityCardUI x) => x.CardType == CardPileType.Hand && x.IsValid && x.IsSelectable && !x.IsSelected && x.AbilityCard != null).ToList();
			list.Sort((AbilityCardUI a, AbilityCardUI b) => b.AbilityCard.Initiative.CompareTo(a.AbilityCard.Initiative));
			foreach (AbilityCardUI item in list)
			{
				item.OnClick();
				if (!item.IsSelected)
				{
					break;
				}
			}
			if (source.Count((AbilityCardUI x) => x.IsSelected) < 2)
			{
				ShortRest shortRest = instanceFast?.CurrentHand?.ShortRestUI;
				if (shortRest != null && shortRest.IsValid && !shortRest.IsSelected)
				{
					shortRest.MouseClick();
					if (shortRest.IsSelected)
					{
						return;
					}
				}
				AbilityCardUI abilityCardUI = source.FirstOrDefault((AbilityCardUI x) => x.IsLongRest);
				if (abilityCardUI != null)
				{
					abilityCardUI.OnClick();
				}
			}
			List<UIPartySlot> list2 = partyModule.Elements.Select((ScenarioPartySlotDetailsElement e) => e.GetComponentInChildren<UIPartySlot>()).ToList();
			for (int num = 0; num < list2.Count; num++)
			{
				if (list2[num].CurrentData.IsSelected)
				{
					int index = (num + 1) % list2.Count;
					list2[index].GetComponentInChildren<UIPartySlotHints>()?.OnClick();
					return;
				}
			}
		}
		if (componentsInChildren.Length != 0)
		{
			AbilityCardUI abilityCardUI2 = componentsInChildren.Select((MiniAbilityCard x) => (!(x == null)) ? x.GetComponentInParent<AbilityCardUI>() : null).FirstOrDefault((AbilityCardUI x) => x != null && x.IsSelectable && !x.IsSelected);
			if (abilityCardUI2 != null)
			{
				abilityCardUI2.ToggleSelect(active: true);
			}
		}
		IEnumerator coro()
		{
			while (true)
			{
				bool flag = false;
				FullAbilityCard[] array = fullAbilityCardUIs;
				foreach (FullAbilityCard fullAbilityCard in array)
				{
					if (fullAbilityCard.DefaultAttackButton == null)
					{
						AutoTestController.LogButtonClick(fullAbilityCard.bottomActionButton.actionButton.gameObject, fullAbilityCard.bottomActionButton.actionButton, "Cmd_Pass");
						fullAbilityCard.bottomActionButton.actionButton.onClick?.Invoke();
						yield break;
					}
					if (fullAbilityCard.DefaultAttackButton.interactable)
					{
						AutoTestController.LogButtonClick(fullAbilityCard.DefaultAttackButton.gameObject, fullAbilityCard.DefaultAttackButton, "Cmd_Pass");
						fullAbilityCard.DefaultAttackButton.onClick?.Invoke();
						flag = true;
						break;
					}
					if (fullAbilityCard.DefaultMoveButton.interactable)
					{
						AutoTestController.LogButtonClick(fullAbilityCard.DefaultMoveButton.gameObject, fullAbilityCard.DefaultMoveButton, "Cmd_Pass");
						fullAbilityCard.DefaultMoveButton.onClick?.Invoke();
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
				int numFramesToSkip = 7;
				int i;
				for (int i2 = 0; i2 < numFramesToSkip; i2 = i)
				{
					yield return null;
					i = i2 + 1;
				}
				skipButton = GameObject.Find("Skip Button")?.GetComponent<UnityEngine.UI.Button>();
				if (!(skipButton != null) || !skipButton.isActiveAndEnabled || !skipButton.interactable)
				{
					break;
				}
				AutoTestController.LogButtonClick(skipButton.gameObject, skipButton, "Cmd_Pass");
				skipButton.onClick?.Invoke();
				for (int i2 = 0; i2 < numFramesToSkip; i2 = i)
				{
					yield return null;
					i = i2 + 1;
				}
			}
		}
	}

	[ConsoleCommand(Command = "start_ability_move", Synchronize = true, Description = "[range=2] [jump=false] [fly=false] - Start move ability for the actor in turn")]
	public static void Cmd_StartMoveAbility(IConsole console, params string[] args)
	{
		int moveAmount = 2;
		if (args.Length != 0)
		{
			moveAmount = int.Parse(args[0]);
		}
		bool jump = false;
		if (args.Length > 1)
		{
			jump = bool.Parse(args[1]);
		}
		bool fly = false;
		if (args.Length > 2)
		{
			fly = bool.Parse(args[2]);
		}
		CAbility cAbility = CAbilityMove.CreateDefaultMove(moveAmount, isMonster: false, 0, jump, fly, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false);
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_loot", Synchronize = true, Description = "[range=1] - Start loot ability for the actor in turn")]
	public static void Cmd_StartLootAbility(IConsole console, params string[] args)
	{
		int range = 1;
		if (args.Length != 0)
		{
			range = int.Parse(args[0]);
		}
		CAbility cAbility = CAbilityLoot.CreateDefaultLoot(range);
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_attack", Synchronize = true, Description = "[strength=2] [range=1] [targets=1] [multipass=false] [xp=0] - Start attack ability for the actor in turn")]
	public static void Cmd_StartAttackAbility(IConsole console, params string[] args)
	{
		int num = 2;
		if (args.Length != 0)
		{
			num = int.Parse(args[0]);
		}
		int num2 = 1;
		if (args.Length > 1)
		{
			num2 = int.Parse(args[1]);
		}
		int num3 = 1;
		if (args.Length > 2)
		{
			num3 = int.Parse(args[2]);
		}
		bool flag = false;
		if (args.Length > 3)
		{
			flag = bool.Parse(args[3]);
		}
		int num4 = 0;
		if (args.Length > 4)
		{
			num4 = int.Parse(args[4]);
		}
		CAbility cAbility = CAbilityAttack.CreateDefaultAttack(num, num2, num3, isMonster: false, xp: num4, isMultipass: flag, abilityfilter: new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self), animationOverload: (num2 > 1) ? "RangedAttack" : "Attack");
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_heal", Synchronize = true, Description = "[strength=2] [range=3] [targets=1] - Start heal ability for the actor in turn")]
	public static void Cmd_StartHealAbility(IConsole console, params string[] args)
	{
		int strength = 2;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 3;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		CAbility cAbility = CAbilityHeal.CreateDefaultHeal(strength, range, numberOfTargets, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All));
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_damage", Synchronize = true, Description = "[strength=2] [range=1] [targets=1] - Start damage ability for the actor in turn")]
	public static void Cmd_StartDamageAbility(IConsole console, params string[] args)
	{
		int strength = 2;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 1;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		CAbility cAbility = CAbilityDamage.CreateDefaultDamage(strength, range, numberOfTargets, isMonster: false, "", isMultipass: false, new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_push", Synchronize = true, Description = "[strength=2] [range=1] [targets=1] - Start push ability for the actor in turn")]
	public static void Cmd_StartPushAbility(IConsole console, params string[] args)
	{
		int strength = 2;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 1;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		CAbility cAbility = CAbilityPush.CreateDefaultPush(strength, range, numberOfTargets, isMonster: false, "", new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_pull", Synchronize = true, Description = "[strength=2] [range=3] [targets=1] - Start pull ability for the actor in turn")]
	public static void Cmd_StartPullAbility(IConsole console, params string[] args)
	{
		int strength = 2;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 3;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		CAbility cAbility = CAbilityPull.CreateDefaultPull(strength, range, numberOfTargets, isMonster: false, "", new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_teleport2", Synchronize = true, Description = "[strength=10] [range=5] [targets=1] - Start teleport2 ability for the actor in turn")]
	public static void Cmd_StartTeleport2Ability(IConsole console, params string[] args)
	{
		int strength = 10;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 5;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		CAbility cAbility = CAbilityTeleport2.CreateDefaultTeleport2(strength, range, numberOfTargets, isMonster: false, "", "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All));
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_select_and_play_card", Synchronize = true, Description = "[num_cards_to_select=1] - Start select and play card ability for the actor in turn")]
	public static void Cmd_StartSelectAndPlayCardAbility(IConsole console, params string[] args)
	{
		int numCardsToSelect = 1;
		if (args.Length != 0)
		{
			numCardsToSelect = int.Parse(args[0]);
		}
		CAbility cAbility = CAbilitySelectAndPlayCard.CreateDefaultSelectAndPlayCard(numCardsToSelect, new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Hand });
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbility, cAbility.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_grant_move", Synchronize = true, Description = "[move_amount=3] [move_jump=false] [move_fly=false] [control_targets=1] - Start control actor ability for the actor in turn that grants move ability")]
	public static void Cmd_StartGrantMove(IConsole console, params string[] args)
	{
		int moveAmount = 3;
		if (args.Length != 0)
		{
			moveAmount = int.Parse(args[0]);
		}
		bool jump = false;
		if (args.Length > 1)
		{
			jump = bool.Parse(args[1]);
		}
		bool fly = false;
		if (args.Length > 2)
		{
			fly = bool.Parse(args[2]);
		}
		int numberOfTargets = 1;
		if (args.Length > 3)
		{
			numberOfTargets = int.Parse(args[3]);
		}
		CAbilityControlActor cAbilityControlActor = (CAbilityControlActor)CAbilityControlActor.CreateDefaultControl(99, numberOfTargets, isMonster: false, "", new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		CAbility cAbility = CAbilityMove.CreateDefaultMove(moveAmount, isMonster: false, 0, jump, fly, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false);
		cAbility.StrengthIsBase = true;
		cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
		cAbilityControlActor.ControlActorData.ControlAbilities.Add(cAbility);
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbilityControlActor, cAbilityControlActor.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_grant_attack", Synchronize = true, Description = "[attack_strength=2] [attack_range=1] [attack_targets=1] [control_targets=1] - Start control actor ability for the actor in turn that grants attack ability")]
	public static void Cmd_StartGrantAttack(IConsole console, params string[] args)
	{
		int num = 2;
		if (args.Length != 0)
		{
			num = int.Parse(args[0]);
		}
		int num2 = 1;
		if (args.Length > 1)
		{
			num2 = int.Parse(args[1]);
		}
		int num3 = 1;
		if (args.Length > 2)
		{
			num3 = int.Parse(args[2]);
		}
		int numberOfTargets = 1;
		if (args.Length > 3)
		{
			numberOfTargets = int.Parse(args[3]);
		}
		CAbilityControlActor cAbilityControlActor = (CAbilityControlActor)CAbilityControlActor.CreateDefaultControl(99, numberOfTargets, isMonster: false, "", new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		CAbility cAbility = CAbilityAttack.CreateDefaultAttack(num, num2, num3, isMonster: false, abilityfilter: new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self), animationOverload: (num2 > 1) ? "RangedAttack" : "Attack", isMultipass: false, activeBonuseData: null, isDefault: false);
		cAbility.StrengthIsBase = true;
		cAbility.RangeIsBase = true;
		cAbility.TargetIsBase = true;
		cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
		cAbilityControlActor.ControlActorData.ControlAbilities.Add(cAbility);
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbilityControlActor, cAbilityControlActor.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "start_ability_grant_heal", Synchronize = true, Description = "[heal_strength=2] [heal_range=1] [heal_targets=1] [control_targets=1] - Start control actor ability for the actor in turn that grants heal ability")]
	public static void Cmd_StartGrantHeal(IConsole console, params string[] args)
	{
		int strength = 2;
		if (args.Length != 0)
		{
			strength = int.Parse(args[0]);
		}
		int range = 1;
		if (args.Length > 1)
		{
			range = int.Parse(args[1]);
		}
		int numberOfTargets = 1;
		if (args.Length > 2)
		{
			numberOfTargets = int.Parse(args[2]);
		}
		int numberOfTargets2 = 1;
		if (args.Length > 3)
		{
			numberOfTargets2 = int.Parse(args[3]);
		}
		CAbilityControlActor cAbilityControlActor = (CAbilityControlActor)CAbilityControlActor.CreateDefaultControl(99, numberOfTargets2, isMonster: false, "", new CAbilityFilterContainer(~CAbilityFilter.EFilterTargetType.Self));
		CAbility cAbility = CAbilityHeal.CreateDefaultHeal(strength, range, numberOfTargets, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.All));
		cAbility.StrengthIsBase = true;
		cAbility.RangeIsBase = true;
		cAbility.TargetIsBase = true;
		cAbilityControlActor.ControlActorData = new CAbilityControlActor.ControlActorAbilityData();
		cAbilityControlActor.ControlActorData.ControlAbilities.Add(cAbility);
		if (PhaseManager.CurrentPhase is CPhaseActionSelection)
		{
			PhaseManager.StartAbility(cAbilityControlActor, cAbilityControlActor.GetAbilityBaseCard());
		}
	}

	[ConsoleCommand(Command = "set_monster_next_card", Synchronize = true, Description = "(ID) - Reshuffle monster's ability cards deck and put the given card on top")]
	public static void Cmd_SetMonstersNextCard(IConsole console, params string[] args)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor == null)
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		if (cActor != null && cActor.Class is CMonsterClass { CardsPoolClass: var cardsPoolClass })
		{
			cardsPoolClass.AbilityCards.Clear();
			cardsPoolClass.ResetAbilityDeck();
			string cardID = args[0];
			CMonsterAbilityCard cMonsterAbilityCard = cardsPoolClass.AbilityCards.FirstOrDefault((CMonsterAbilityCard x) => x.ID.ToString() == cardID);
			if (cMonsterAbilityCard != null)
			{
				cardsPoolClass.AbilityCards.Remove(cMonsterAbilityCard);
				cardsPoolClass.AbilityCards.Insert(0, cMonsterAbilityCard);
				Instance.DisplayDebugText(2, "Next monster card ID: " + cMonsterAbilityCard.ID + " (Init: " + cMonsterAbilityCard.Initiative + ")");
			}
		}
	}

	[ConsoleCommand(Command = "attack_modifier_cards", Synchronize = true, Description = "[reset | name1 name2 ...] - View / Reset / Set attack modifier deck")]
	public static void Cmd_AttackModifierCards(IConsole console, params string[] args)
	{
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor == null)
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
		if (cActor == null)
		{
			return;
		}
		if (args.Length == 1 && args[0] == "reset")
		{
			if (cActor.Class is CCharacterClass cCharacterClass)
			{
				cCharacterClass.ResetAttackModifierDeck();
			}
			else if (cActor is CEnemyActor enemyActor)
			{
				CMonsterAttackModifierDeck attackModifiersDeck = MonsterClassManager.GetAttackModifiersDeck(enemyActor);
				attackModifiersDeck.AttackModifierCards.AddRange(attackModifiersDeck.DiscardedAttackModifierCards);
				attackModifiersDeck.DiscardedAttackModifierCards.Clear();
			}
			Instance._selectedCharacterChanged = true;
			return;
		}
		List<AttackModifierYMLData> attackModifierCards = GetAttackModifierCards(cActor);
		if (args.Length == 0)
		{
			string text = $"Attack Modifiers for {cActor}: {TValue.Dump(attackModifierCards)}";
			console.WriteLine("{0}", text);
			Log.Info(text);
			return;
		}
		List<AttackModifierYMLData> source = attackModifierCards.ToList();
		attackModifierCards.Clear();
		foreach (string cardName in args)
		{
			AttackModifierYMLData item = source.FirstOrDefault((AttackModifierYMLData c) => c.Name == cardName);
			attackModifierCards.Add(item);
		}
		Instance._selectedCharacterChanged = true;
	}

	[ConsoleCommand(Command = "restore_player_cards", Synchronize = true, Description = "Restore all player discarded/lost cards")]
	public static void Cmd_RestorePlayerCards(IConsole console)
	{
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			for (int num = allPlayer.CharacterClass.LostAbilityCards.Count - 1; num >= 0; num--)
			{
				CAbilityCard cAbilityCard = allPlayer.CharacterClass.LostAbilityCards[num];
				Cmd_GiveAbilityCard(console, cAbilityCard.Name);
			}
			for (int num2 = allPlayer.CharacterClass.DiscardedAbilityCards.Count - 1; num2 >= 0; num2--)
			{
				CAbilityCard cAbilityCard2 = allPlayer.CharacterClass.DiscardedAbilityCards[num2];
				Cmd_GiveAbilityCard(console, cAbilityCard2.Name);
			}
		}
	}

	[ConsoleCommand(Command = "discard", Synchronize = false, Description = "[ID | part of name | all | any] - Discard an ability card")]
	public static void Cmd_DiscardAbilityCard(IConsole console, params string[] args)
	{
		int result = 0;
		string text = null;
		CPlayerActor cPlayerActor = null;
		if (args.Length > 1)
		{
			if (int.TryParse(args[1], out var playerID))
			{
				cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor p) => p.ID == playerID);
			}
		}
		else
		{
			cPlayerActor = Singleton<CardsHandManager>.Instance?.CurrentHand?.PlayerActor;
		}
		if (cPlayerActor == null)
		{
			CError(console, "Not selected player");
			return;
		}
		if (args.Length != 0 && !int.TryParse(args[0], out result))
		{
			text = args[0];
		}
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (CAbilityCard handAbilityCard in cPlayerActor.CharacterClass.HandAbilityCards)
		{
			if (!(text != "all") || !(text != "any") || MatchCard(handAbilityCard, result, text))
			{
				list.Add(handAbilityCard);
			}
		}
		if (list.Count == 0)
		{
			CError(console, "No matching card(s) found");
			return;
		}
		if (list.Count > 1 && text != "all" && text != "any")
		{
			CLog(console, $"Found {list.Count} matching cards:");
			{
				foreach (CAbilityCard item in list)
				{
					CLog(console, "    " + item.Name);
				}
				return;
			}
		}
		if (!s_InProxyExec && text == "all")
		{
			SyncExec($"discard all {cPlayerActor.ID}", null, new Point(-1, -1), executeLocally: false);
		}
		while (list.Count > 0)
		{
			CAbilityCard cAbilityCard = list[0];
			CAbilityCard abilityCard = FindCardInstance(cPlayerActor.CharacterClass.HandAbilityCards, cAbilityCard);
			cPlayerActor.CharacterClass.MoveAbilityCard(abilityCard, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
			if (text != "all")
			{
				if (!s_InProxyExec)
				{
					SyncExec($"discard {cAbilityCard.ID} {cPlayerActor.ID}", null, new Point(-1, -1), executeLocally: false);
				}
				break;
			}
			list.RemoveAt(0);
		}
		Singleton<CardsHandManager>.Instance.UpdateElements(cPlayerActor);
		Singleton<CardsHandManager>.Instance.CurrentHand?.UpdateShortRest();
		if (Singleton<CardsHandManager>.Instance.IsActive())
		{
			CardsHandManager.ShowCardsSelection(cPlayerActor);
		}
	}

	[ConsoleCommand(Command = "burn", Synchronize = false, Description = "[ID | part of name | all | any] - Burn an ability card")]
	public static void Cmd_BurnAbilityCard(IConsole console, params string[] args)
	{
		int result = 0;
		string text = null;
		CPlayerActor cPlayerActor = null;
		if (args.Length > 1)
		{
			if (int.TryParse(args[1], out var playerID))
			{
				cPlayerActor = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor p) => p.ID == playerID);
			}
		}
		else
		{
			cPlayerActor = Singleton<CardsHandManager>.Instance?.CurrentHand?.PlayerActor;
		}
		if (cPlayerActor == null)
		{
			CError(console, "Not selected player");
			return;
		}
		if (args.Length != 0 && !int.TryParse(args[0], out result))
		{
			text = args[0];
		}
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (CAbilityCard handAbilityCard in cPlayerActor.CharacterClass.HandAbilityCards)
		{
			if (!(text != "all") || !(text != "any") || MatchCard(handAbilityCard, result, text))
			{
				list.Add(handAbilityCard);
			}
		}
		if (list.Count == 0)
		{
			CError(console, "No matching card(s) found");
			return;
		}
		if (list.Count > 1 && text != "all" && text != "any")
		{
			CLog(console, $"Found {list.Count} matching cards:");
			{
				foreach (CAbilityCard item in list)
				{
					CLog(console, "    " + item.Name);
				}
				return;
			}
		}
		if (!s_InProxyExec && text == "all")
		{
			SyncExec($"burn all {cPlayerActor.ID}", null, new Point(-1, -1), executeLocally: false);
		}
		while (list.Count > 0)
		{
			CAbilityCard cAbilityCard = list[0];
			CAbilityCard abilityCard = FindCardInstance(cPlayerActor.CharacterClass.HandAbilityCards, cAbilityCard);
			cPlayerActor.CharacterClass.MoveAbilityCard(abilityCard, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.LostAbilityCards, "HandAbilityCards", "LostAbilityCards");
			if (text != "all")
			{
				if (!s_InProxyExec)
				{
					SyncExec($"burn {cAbilityCard.ID} {cPlayerActor.ID}", null, new Point(-1, -1), executeLocally: false);
				}
				break;
			}
			list.RemoveAt(0);
		}
		Singleton<CardsHandManager>.Instance.UpdateElements(cPlayerActor);
		Singleton<CardsHandManager>.Instance.CurrentHand?.UpdateShortRest();
		if (Singleton<CardsHandManager>.Instance.IsActive())
		{
			CardsHandManager.ShowCardsSelection(cPlayerActor);
		}
	}

	[ConsoleCommand(Command = "give_card", Synchronize = true, Description = "[ID | part of name] - Give an ability card to an existing character")]
	public static void Cmd_GiveAbilityCard(IConsole console, params string[] args)
	{
		int result = 0;
		string text = null;
		if (args.Length != 0 && !int.TryParse(args[0], out result))
		{
			text = args[0];
		}
		bool flag = text == "all";
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (CAbilityCard allAbilityCard in CharacterClassManager.AllAbilityCards)
		{
			if (flag || MatchCard(allAbilityCard, result, text))
			{
				CPlayerActor cPlayerActor = FindPlayer(allAbilityCard);
				if (cPlayerActor != null && !ContainsCard(cPlayerActor.CharacterClass.HandAbilityCards, allAbilityCard) && !ContainsCard(cPlayerActor.CharacterClass.ActivatedAbilityCards, allAbilityCard))
				{
					list.Add(allAbilityCard);
				}
			}
		}
		if (list.Count == 0)
		{
			CError(console, "No matching card(s) found");
			return;
		}
		if (list.Count > 1 && !flag)
		{
			CLog(console, $"Found {list.Count} matching cards:");
			{
				foreach (CAbilityCard item in list)
				{
					CLog(console, "    " + item.Name);
				}
				return;
			}
		}
		GiveCard(console, list);
	}

	public static void Cmd_GiveAbilityCards(IConsole console, int[] ids)
	{
		List<CAbilityCard> list = new List<CAbilityCard>();
		foreach (CAbilityCard allAbilityCard in CharacterClassManager.AllAbilityCards)
		{
			if (ids.Contains(allAbilityCard.ID))
			{
				CPlayerActor cPlayerActor = FindPlayer(allAbilityCard);
				if (cPlayerActor != null && !ContainsCard(cPlayerActor.CharacterClass.HandAbilityCards, allAbilityCard) && !ContainsCard(cPlayerActor.CharacterClass.ActivatedAbilityCards, allAbilityCard))
				{
					list.Add(allAbilityCard);
				}
			}
		}
		if (list.Count == 0)
		{
			CError(console, "No matching card(s) found");
		}
		else
		{
			GiveCard(console, list, logChoreographer: false);
		}
	}

	public static void GiveCard(IConsole console, List<CAbilityCard> options, bool logChoreographer = true)
	{
		string text = null;
		CPlayerActor cPlayerActor = null;
		foreach (CAbilityCard option in options)
		{
			cPlayerActor = FindPlayer(option);
			if (ContainsCard(cPlayerActor.CharacterClass.HandAbilityCards, option))
			{
				CError(console, $"{cPlayerActor} already has {option.Name}");
				return;
			}
			if (ContainsCard(cPlayerActor.CharacterClass.ActivatedAbilityCards, option))
			{
				CError(console, $"{cPlayerActor} already has activated {option.Name}");
				return;
			}
			CLog(console, $"Adding [{option.Initiative}]'{option.Name}' to {cPlayerActor}");
			CAbilityCard cardInstance = null;
			List<CAbilityCard> outDeck = null;
			if (FindCardInstance(cPlayerActor.CharacterClass.DiscardedAbilityCards, option, out outDeck, out cardInstance))
			{
				text = "DiscardedAbilityCards";
			}
			else if (FindCardInstance(cPlayerActor.CharacterClass.LostAbilityCards, option, out outDeck, out cardInstance))
			{
				text = "LostAbilityCards";
			}
			else if (FindCardInstance(cPlayerActor.CharacterClass.OwnedUnusedCards, option, out outDeck, out cardInstance))
			{
				text = "OwnedUnusedCards";
				if (!ContainsCard(cPlayerActor.CharacterClass.SelectedAbilityCards, option))
				{
					cPlayerActor.CharacterClass.SelectedAbilityCards.Add(cardInstance);
				}
			}
			else
			{
				cardInstance = option;
				if (!CharacterClassManager.AllAbilityCardInstances.Contains(cardInstance))
				{
					CharacterClassManager.AllAbilityCardInstances.Add(cardInstance);
				}
				cPlayerActor.CharacterClass.HandAbilityCards.Add(cardInstance);
				if (!ContainsCard(cPlayerActor.CharacterClass.AbilityCardsPool, option))
				{
					cPlayerActor.CharacterClass.AbilityCardsPool.Add(cardInstance);
				}
				if (!ContainsCard(cPlayerActor.CharacterClass.SelectedAbilityCards, option))
				{
					cPlayerActor.CharacterClass.SelectedAbilityCards.Add(cardInstance);
				}
				Singleton<CardsHandManager>.Instance.GetHand(cPlayerActor).SpawnCardUIFromCheat(cardInstance);
				cPlayerActor.CharacterClass.MoveAbilityCard(cardInstance, cPlayerActor.CharacterClass.HandAbilityCards, cPlayerActor.CharacterClass.DiscardedAbilityCards, "HandAbilityCards", "DiscardedAbilityCards");
				outDeck = cPlayerActor.CharacterClass.DiscardedAbilityCards;
				text = "DiscardedAbilityCards";
			}
			cPlayerActor.CharacterClass.MoveAbilityCard(cardInstance, outDeck, cPlayerActor.CharacterClass.HandAbilityCards, text, "HandAbilityCards");
			ScenarioManager.CurrentScenarioState.TryCreateCardScripts(cPlayerActor);
			Singleton<CardsHandManager>.Instance.UpdateElements(cPlayerActor);
			if (Singleton<CardsHandManager>.Instance.IsActive())
			{
				CardsHandManager.ShowCardsSelection(cPlayerActor);
			}
		}
		if (logChoreographer)
		{
			if (text == "DiscardedAbilityCards")
			{
				CRecoverDiscardedCards_MessageData cRecoverDiscardedCards_MessageData = new CRecoverDiscardedCards_MessageData(null, cPlayerActor);
				cRecoverDiscardedCards_MessageData.m_ActorRecoveringLostCards = cPlayerActor;
				cRecoverDiscardedCards_MessageData.m_Ability = null;
				Choreographer.s_Choreographer.PostMessageFromUI(cRecoverDiscardedCards_MessageData);
			}
			else if (text == "LostAbilityCards")
			{
				CRecoverLostCards_MessageData cRecoverLostCards_MessageData = new CRecoverLostCards_MessageData(null, cPlayerActor);
				cRecoverLostCards_MessageData.m_ActorRecoveringLostCards = cPlayerActor;
				cRecoverLostCards_MessageData.m_Ability = null;
				Choreographer.s_Choreographer.PostMessageFromUI(cRecoverLostCards_MessageData);
			}
		}
	}

	private static bool MatchCard(CAbilityCard card, int ID, string nameSubStr)
	{
		if (ID > 0)
		{
			return card.ID == ID;
		}
		if (!string.IsNullOrEmpty(nameSubStr))
		{
			return card.Name.Contains(nameSubStr, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static CPlayerActor FindPlayer(CAbilityCard card)
	{
		return ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor x) => x.CharacterClass.ID == card.ClassID);
	}

	private static bool ContainsCard(List<CAbilityCard> deck, CAbilityCard card)
	{
		return deck.Any((CAbilityCard x) => x.ID == card.ID);
	}

	private static CAbilityCard FindCardInstance(List<CAbilityCard> deck, CAbilityCard card)
	{
		return deck.FirstOrDefault((CAbilityCard x) => x.ID == card.ID);
	}

	private static bool FindCardInstance(List<CAbilityCard> deck, CAbilityCard card, out List<CAbilityCard> outDeck, out CAbilityCard cardInstance)
	{
		outDeck = deck;
		cardInstance = deck.FirstOrDefault((CAbilityCard x) => x.ID == card.ID);
		return cardInstance != null;
	}

	private static void CLog(IConsole console, string msg)
	{
		console?.WriteLine("{0}", msg);
	}

	private static void CError(IConsole console, string msg)
	{
		console?.WriteLine("{0}", msg);
	}

	[ConsoleCommand(Command = "enhance", Synchronize = true, Description = "[CardID] [AbilityName[.EnhancementLine]] [Enhancement] - Enhance an ability card")]
	public static void Cmd_EnhanceAbilityCard(IConsole console, int cardID = 0, string abilityName = null, string enhancementName = null)
	{
		string line = ParseAbilityNameAndEnhancementLine(ref abilityName);
		if (cardID <= 0)
		{
			DumpEnhancements();
			return;
		}
		CAbilityCard cAbilityCard = FindAbilityCard(cardID);
		if (cAbilityCard == null)
		{
			Error($"No character has ability card with ID {cardID}");
			cardID = 0;
			DumpEnhancements();
			return;
		}
		if (string.IsNullOrEmpty(abilityName))
		{
			DumpEnhancements();
			return;
		}
		CAbility cAbility = cAbilityCard.FindAbilityOnCard(abilityName);
		if (cAbility == null)
		{
			Error($"No '{abilityName}' ability found in {cAbilityCard}");
			abilityName = null;
			line = null;
			DumpEnhancements();
			return;
		}
		CEnhancement cEnhancement = null;
		foreach (CEnhancement abilityEnhancement in cAbility.AbilityEnhancements)
		{
			if (string.IsNullOrEmpty(line) || !(abilityEnhancement.EnhancementLine.ToString() != line))
			{
				cEnhancement = abilityEnhancement;
				break;
			}
		}
		EEnhancement result;
		if (cEnhancement == null)
		{
			Error("No mathcing enhancement found for ability '" + abilityName + "' of card " + cAbilityCard.Name);
			abilityName = null;
			line = null;
			DumpEnhancements();
		}
		else if (string.IsNullOrEmpty(enhancementName))
		{
			DumpEnhancements();
		}
		else if (!Enum.TryParse<EEnhancement>(enhancementName, out result))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Invalid enhancement type: " + enhancementName);
			stringBuilder.AppendLine("Possible enhancement types:");
			EEnhancement[] enhancements = CEnhancement.Enhancements;
			foreach (EEnhancement eEnhancement in enhancements)
			{
				stringBuilder.AppendLine($"    {eEnhancement}");
			}
			Error(stringBuilder.ToString());
		}
		else
		{
			cEnhancement.Enhancement = result;
		}
		void DumpEnhancements()
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				bool flag = false;
				foreach (CAbilityCard item in allPlayer.CharacterClass.AbilityCardsPool)
				{
					bool flag2 = false;
					if (cardID <= 0 || item.ID == cardID)
					{
						foreach (CEnhancement allEnhancement in item.GetAllEnhancements())
						{
							if ((string.IsNullOrEmpty(abilityName) || !(allEnhancement.AbilityName != abilityName)) && (string.IsNullOrEmpty(line) || !(allEnhancement.EnhancementLine.ToString() != line)))
							{
								if (!flag)
								{
									flag = true;
									stringBuilder2.AppendLine(allPlayer.CharacterClass.ID + ":");
								}
								if (!flag2)
								{
									flag2 = true;
									stringBuilder2.AppendLine($"    {item.ID}: {item.Name}:");
								}
								stringBuilder2.AppendLine($"        {allEnhancement.AbilityName}.{allEnhancement.EnhancementLine} ({allEnhancement.MarkType}): {allEnhancement.Enhancement}");
							}
						}
					}
				}
			}
			Log(stringBuilder2.ToString());
		}
		void Error(string msg)
		{
			if (console != null)
			{
				console.WriteLine("{0}", msg);
			}
		}
		static CAbilityCard FindAbilityCard(int num)
		{
			foreach (CPlayerActor allPlayer2 in ScenarioManager.Scenario.AllPlayers)
			{
				CAbilityCard cAbilityCard2 = allPlayer2.CharacterClass.AbilityCardsPool.Find((CAbilityCard x) => x.ID == num);
				if (cAbilityCard2 != null)
				{
					return cAbilityCard2;
				}
			}
			return null;
		}
		void Log(string msg)
		{
			if (console != null)
			{
				console.WriteLine("{0}", msg);
			}
		}
		static string ParseAbilityNameAndEnhancementLine(ref string reference)
		{
			if (string.IsNullOrEmpty(reference))
			{
				return null;
			}
			int num = reference.IndexOf('.');
			if (num >= 0)
			{
				string result2 = reference.Substring(num + 1);
				reference = reference.Substring(0, num);
				return result2;
			}
			return null;
		}
	}

	[ConsoleCommand(Command = "reveal_all_rooms", Synchronize = true)]
	public static void Cmd_RevealAllRooms(IConsole console)
	{
		RevealAllRooms();
	}

	[ConsoleCommand(Command = "open_all_doors", Synchronize = true)]
	public static void Cmd_OpenAllDoors(IConsole console)
	{
		RevealAllRooms();
	}

	private static void RevealAllRooms()
	{
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState == null)
		{
			return;
		}
		foreach (CObjectProp doorProp in currentScenarioState.DoorProps)
		{
			if (doorProp is CObjectDoor cObjectDoor)
			{
				cObjectDoor.ForceActivate(null);
			}
		}
		foreach (CMap map in currentScenarioState.Maps)
		{
			if (!map.Revealed)
			{
				map.Reveal(initial: false);
				CRevealRooms_MessageData message = new CRevealRooms_MessageData();
				Choreographer.s_Choreographer.PostMessageFromUI(message);
			}
		}
	}

	[ConsoleCommand(Command = "serializable_check", Synchronize = false, Description = "Check for automatically serialized types")]
	public static void Cmd_SerializableCheck(IConsole console, params string[] args)
	{
		HashSet<object> alreadySerializedObjects = new HashSet<object>(new RefComparer());
		Dictionary<Type, string> dictionary = new Dictionary<Type, string>();
		CheckSerializable(null, "SaveData.Instance.Global.CurrentAdventureData", SaveData.Instance?.Global?.CurrentAdventureData, alreadySerializedObjects, dictionary);
		CheckSerializable(null, "ScenarioManager.CurrentScenarioState", ScenarioManager.CurrentScenarioState, alreadySerializedObjects, dictionary);
		if (dictionary.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<Type, string> item in dictionary)
		{
			Type key = item.Key;
			string value = item.Value;
			stringBuilder.AppendLine(TValue.FriendlyTypeName(key) + ": " + value);
		}
		string arg = stringBuilder.ToString();
		Log.Error($"Non-serializable types: {dictionary.Count}\n{arg}");
	}

	private static void CheckSerializable(Type type, string path, object obj, HashSet<object> alreadySerializedObjects, Dictionary<Type, string> nonSerializableTypes)
	{
		if (type == null)
		{
			type = obj?.GetType();
		}
		if (obj == null)
		{
			if (!(type == typeof(object)))
			{
				CheckTypeSerializable(type, path, nonSerializableTypes);
			}
		}
		else
		{
			if (alreadySerializedObjects.Contains(obj))
			{
				return;
			}
			alreadySerializedObjects.Add(obj);
			CheckTypeSerializable(type, path, nonSerializableTypes);
			if (obj is IDictionary dictionary)
			{
				int num = 0;
				foreach (object key in dictionary.Keys)
				{
					string path2 = $"{path}.Keys[{num}]";
					CheckSerializable(null, path2, key, alreadySerializedObjects, nonSerializableTypes);
					num++;
				}
				num = 0;
				{
					foreach (object value2 in dictionary.Values)
					{
						string path3 = $"{path}.Values[{num}]";
						CheckSerializable(null, path3, value2, alreadySerializedObjects, nonSerializableTypes);
						num++;
					}
					return;
				}
			}
			if (obj is ICollection collection)
			{
				int num2 = 0;
				{
					foreach (object item in collection)
					{
						string path4 = $"{path}[{num2}]";
						CheckSerializable(null, path4, item, alreadySerializedObjects, nonSerializableTypes);
						num2++;
					}
					return;
				}
			}
			if (obj is ITuple { Length: var length } tuple)
			{
				for (int i = 0; i < length; i++)
				{
					object obj2 = tuple[i];
					string path5 = $"{path}[{i}]";
					CheckSerializable(null, path5, obj2, alreadySerializedObjects, nonSerializableTypes);
				}
			}
			else if (obj is ISerializable serializable)
			{
				SerializationInfo serializationInfo = new SerializationInfo(type, new FormatterConverter());
				serializable.GetObjectData(serializationInfo, default);
				foreach (SerializationEntry item2 in serializationInfo)
				{
					Type objectType = item2.ObjectType;
					string text = item2.Name;
					object value = item2.Value;
					string path6 = path + "." + text;
					CheckSerializable(objectType, path6, value, alreadySerializedObjects, nonSerializableTypes);
				}
			}
		}
	}

	private static Type[] ResolveContainerType(Type type)
	{
		if (type == null)
		{
			return null;
		}
		if (type.HasElementType)
		{
			Type elementType = type.GetElementType();
			return new Type[1] { elementType };
		}
		if (type.IsGenericType && !type.IsGenericTypeDefinition)
		{
			return type.GetGenericArguments();
		}
		return null;
	}

	private static void CheckTypeSerializable(Type type, string path, Dictionary<Type, string> nonSerializableTypes)
	{
		if (type == null || type.IsPrimitive || type == typeof(string) || type.IsEnum)
		{
			return;
		}
		Type[] array = ResolveContainerType(type);
		if (array != null)
		{
			Type[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				CheckTypeSerializable(array2[i], path, nonSerializableTypes);
			}
		}
		else if (!typeof(ISerializable).IsAssignableFrom(type))
		{
			nonSerializableTypes.TryAdd(type, path);
		}
	}

	[ConsoleCommand(Command = "dump_game_state", Synchronize = false, Description = "[file_name] - Dump Game State")]
	public static void Cmd_DumpGameState(IConsole console, params string[] args)
	{
		string fileName;
		if (args.Length != 0)
		{
			fileName = args[0];
		}
		else if (FFSNetwork.IsOnline)
		{
			fileName = ((!FFSNetwork.IsHost) ? $"client_{PlayerRegistry.MyPlayer?.PlayerID}_state" : "host_state");
		}
		else
		{
			fileName = "game_state";
		}
		StateDumper.SaveStateDump(fileName);
	}

	[ConsoleCommand(Command = "dump_stats", Synchronize = false, Description = "Dump ScenarioState.Stats to file")]
	public static void Cmd_DumpDtats(IConsole console, params string[] args)
	{
		StatsState statsState = ScenarioManager.CurrentScenarioState?.Stats;
		if (statsState == null)
		{
			return;
		}
		Dictionary<string, int> countsPerType = new Dictionary<string, int>();
		int totalCount = 0;
		string stateDump = StateDump.Dump(statsState, "Stats", null, (string key, object obj) =>
		{
			if (obj is CPlayerStatsBase)
			{
				int num = totalCount + 1;
				totalCount = num;
				string key2 = obj.GetType().Name;
				if (countsPerType.TryGetValue(key2, out var value))
				{
					countsPerType[key2] = value + 1;
				}
				else
				{
					countsPerType.Add(key2, 1);
				}
			}
		});
		List<(string, int)> list = new List<(string, int)>(countsPerType.Count);
		foreach (KeyValuePair<string, int> item in countsPerType)
		{
			list.Add((item.Key, item.Value));
		}
		list.Sort(((string type, int count) a, (string type, int count) b) => b.count.CompareTo(a.count));
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var item2 in list)
		{
			stringBuilder.AppendLine($"{item2.Item1}: {item2.Item2}");
		}
		stringBuilder.Append($"Total: {totalCount}");
		string text = stringBuilder.ToString();
		Log.Info(text);
		console.WriteLine(text);
		StateDumper.SaveStateDump(stateDump, "Stats");
	}

	[ConsoleCommand(Command = "fast_save", Synchronize = true, Description = "Fast Save")]
	public static void Cmd_FastSave(IConsole console)
	{
		FastReload.QuickSave(saveToDisk: false);
	}

	[ConsoleCommand(Command = "fast_load", Synchronize = true, Description = "[h[istory]|<checkpoint_index>|<checkpoint_name>] Fast Load")]
	public static void Cmd_FastLoad(IConsole console, params string[] args)
	{
		if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
		{
			FastReload.QuickLoad(loadFromDisk: false);
			return;
		}
		string text = args[0];
		if (text == "history" || text == "h")
		{
			for (int i = 0; i < FastReload.History.Count; i++)
			{
				FastReload.Checkpoint checkpoint = FastReload.History[i];
				string arg = ((i == FastReload.IndexInHistory) ? " (current)" : "");
				string text2 = $"{i}: {checkpoint.name}{arg}";
				console.WriteLine("{0}", text2);
			}
			return;
		}
		if (int.TryParse(text, out var result))
		{
			FastReload.Load(result);
			return;
		}
		for (int j = 0; j < FastReload.History.Count; j++)
		{
			if (FastReload.History[j].name.Contains(text, StringComparison.OrdinalIgnoreCase))
			{
				FastReload.Load(j);
				return;
			}
		}
		Log.Error("Unknown checkpoint: '" + text + "'");
	}

	[ConsoleCommand(Command = "quick_save", Synchronize = true, Description = "Quick Save")]
	public static void Cmd_QuickSave(IConsole console)
	{
		FastReload.QuickSave(saveToDisk: true);
	}

	[ConsoleCommand(Command = "quick_load", Synchronize = true, Description = "Quick Load")]
	public static void Cmd_QuickLoad(IConsole console)
	{
		FastReload.QuickLoad(loadFromDisk: true);
	}

	[ConsoleCommand(Command = "quick_history_save", Synchronize = true, Description = "Save Undo History")]
	public static void Cmd_QuickHistorySave(IConsole console)
	{
		FastReload.SaveAllHistory();
	}

	[ConsoleCommand(Command = "quick_history_load", Synchronize = true, Description = "Load Undo History")]
	public static void Cmd_QuickHistoryLoad(IConsole console)
	{
		FastReload.LoadAllHistory();
	}

	[ConsoleCommand(Command = "fast_undo", Synchronize = true, Description = "Fast Undo")]
	public static void Cmd_FastUndo(IConsole console)
	{
		FastReload.Undo(force: true);
	}

	[ConsoleCommand(Command = "fast_redo", Synchronize = true, Description = "Fast Redo")]
	public static void Cmd_FastRedo(IConsole console)
	{
		FastReload.Redo(force: true);
	}

	[ConsoleCommand(Command = "fast_restart_round", Synchronize = true, Description = "Fast Restart Round")]
	public static void Cmd_FastRestartRound(IConsole console)
	{
		FastReload.RestartRound();
	}

	[ConsoleCommand(Command = "fast_auto_undo_redo", Synchronize = true, Description = "Fast Auto Undo / Redo")]
	public static void Cmd_FastAutoUndoRedo(IConsole console, params string[] args)
	{
		int undoCount = 1;
		int redoCount = -2;
		if (args.Length != 0)
		{
			string text = args[0];
			switch (text)
			{
			case "on":
				FastReload.AutoUnodRedoOnEveryAutoSave = true;
				Log.Info("Auto Undo / Redo on every action Enabled");
				return;
			case "off":
				FastReload.AutoUnodRedoOnEveryAutoSave = false;
				Log.Info("Auto Undo / Redo on every action Diabled");
				return;
			case "all":
				undoCount = -1;
				break;
			case "once":
				undoCount = 1;
				break;
			default:
			{
				if (int.TryParse(text, out var result))
				{
					undoCount = result;
					break;
				}
				Log.Error("Invalid first argument: '" + text + "'");
				return;
			}
			}
		}
		if (args.Length > 1)
		{
			string text2 = args[1];
			if (text2 == "all")
			{
				redoCount = -1;
			}
			else if (text2 == "once")
			{
				redoCount = 1;
			}
			else
			{
				if (!int.TryParse(text2, out var result2))
				{
					Log.Error("Invalid second argument: '" + text2 + "'");
					return;
				}
				redoCount = result2;
			}
		}
		FastReload.RequestAutoUndoRedo(undoCount, redoCount);
	}

	[ConsoleCommand(Command = "check_for_desync", Synchronize = true, Description = "Perform desync check in multiplayer")]
	public static void Cmd_CheckForDesync(IConsole console)
	{
		if (FFSNetwork.IsOnline && FFSNetwork.IsHost)
		{
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				DesyncDetector.RequestDelayedCheckOnHost();
			}
			else
			{
				DesyncDetector.CheckForDesync();
			}
		}
	}

	[ConsoleCommand(Command = "flood_fill", Synchronize = true, Description = "[range=-1] [free_water_tiles=0] [ignore_blocked=false] - Visualizes flood fill for the actor under the cursor")]
	public static void Cmd_FloodFill(IConsole console, params string[] args)
	{
		int num = 999;
		int num2 = num;
		if (args.Length != 0)
		{
			num2 = int.Parse(args[0]);
			if (num2 < 0)
			{
				num2 = num;
			}
		}
		int waterTilesThatCostZero = 0;
		if (args.Length > 1)
		{
			waterTilesThatCostZero = int.Parse(args[1]);
		}
		bool ignoreBlocked = false;
		if (args.Length > 2)
		{
			ignoreBlocked = bool.Parse(args[2]);
		}
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex);
			if (cActor == null)
			{
				console.WriteLine("No actor under the cursor");
				return;
			}
			CAbilityMove cAbilityMove = (CAbilityMove)CAbilityMove.CreateDefaultMove(0, isMonster: false, 0, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, isDefault: false);
			cAbilityMove.TargetingActor = cActor;
			cAbilityMove.RefreshShadowNavLinks();
			_floodedPoints = new List<Point>();
			_pfContext = ScenarioManager.PathFinder.FloodFill_ToRange(cActor, cActor.ArrayIndex, num2, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, waterTilesThatCostZero, null, null, _floodedPoints, null, ignoreDoorLocks: false);
			cAbilityMove.ClearShadowNavLinks();
		}
		else
		{
			console.WriteLine("No tile under the cursor");
		}
	}

	[ConsoleCommand(Command = "clear_flood_fill", Synchronize = true, Description = "Clears the last flood fill if any")]
	public static void Cmd_ClearFloodFill(IConsole console)
	{
		_pfContext = null;
		_floodedPoints = null;
	}

	[ConsoleCommand(Command = "summon", Synchronize = true)]
	public static void Cmd_Summon(IConsole console, params string[] argv)
	{
		if (argv.Length < 2)
		{
			printPossibilities();
			console.WriteLine("Usage: summon <summon partial class/ID> <character partial model/ID> [x] [y]");
			return;
		}
		CHeroSummonClass summonClass = null;
		CMonsterClass monsterClass = null;
		string value = argv[0].ToLowerInvariant();
		foreach (CHeroSummonClass heroSummonClass in CharacterClassManager.HeroSummonClasses)
		{
			if (heroSummonClass.ID.ToLowerInvariant().Contains(value))
			{
				summonClass = heroSummonClass;
				break;
			}
		}
		_ = summonClass;
		if (monsterClass == null && summonClass == null)
		{
			printPossibilities();
			console.WriteLine("Can't find summon class " + argv[0]);
			return;
		}
		if (!ScenarioScriptFrontend.GetFirstMatchingActor(argv[1], out var character))
		{
			foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
			{
				console.WriteLine(allPlayer.Class.ID + " " + allPlayer.ActorGuid);
			}
			console.WriteLine("Can't find character " + argv[1]);
			return;
		}
		CClientTile tile = null;
		if (argv.Length >= 5)
		{
			int.TryParse(argv[2], out var result);
			int.TryParse(argv[3], out var result2);
			CClientTile[,] clientTileArray = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray;
			result = Mathf.Clamp(result, 0, clientTileArray.GetLength(0));
			result2 = Mathf.Clamp(result2, 0, clientTileArray.GetLength(1));
			tile = clientTileArray[result, result2];
			if (tile == null)
			{
				console.WriteLine($"Tile {result} {result2} doesn't have a proper CClientTile");
				return;
			}
		}
		if (tile == null && !ScenarioScriptFrontend.GetTileUnderCursor(out tile))
		{
			console.WriteLine("No tile under cursor.");
			return;
		}
		ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
		{
			if (!CAbilitySummon.TrySpawnSummon(character, summonClass?.ID, monsterClass?.ID, new List<CTile> { tile.m_Tile }, out var error))
			{
				string text = ((summonClass != null) ? summonClass.ID : monsterClass.ID);
				console.WriteLine("Failed to spawn summon " + text + ". " + error);
			}
			else
			{
				console.WriteLine("Spawned " + summonClass.ID + ".");
			}
		}, "CHEAT_SUMMON");
		void printPossibilities()
		{
			console.WriteLine("Possible summons:");
			foreach (CHeroSummonClass heroSummonClass2 in CharacterClassManager.HeroSummonClasses)
			{
				console.WriteLine("  " + heroSummonClass2.ID);
			}
		}
	}

	[ConsoleCommand(Command = "give_resonance", Synchronize = true)]
	public static void Cmd_GiveResonance(IConsole console, int amount)
	{
		CPlayerActor cPlayerActor = null;
		TileBehaviour tileBehaviourUnderCursor = GetTileBehaviourUnderCursor();
		if (tileBehaviourUnderCursor != null)
		{
			cPlayerActor = ScenarioManager.Scenario.FindActorAt(tileBehaviourUnderCursor.m_ClientTile.m_Tile.m_ArrayIndex) as CPlayerActor;
			if (cPlayerActor == null)
			{
				console.WriteLine("No actor under the cursor");
				return;
			}
		}
		cPlayerActor.SetResonanceTokens(cPlayerActor.GetResonanceTokens() + amount);
	}
}

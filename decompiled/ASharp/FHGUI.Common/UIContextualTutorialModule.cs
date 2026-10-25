using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common.Tooltips;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.Scenario;
using FHGUI.Services;
using FHGUI.States;
using FHGUI.Tweening;
using FHRL;
using FHRL.Buildings;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Achievments;
using UnityEngine;
using UnityEngine.UI;
using WorldspaceUI;

namespace FHGUI.Common;

public class UIContextualTutorialModule : MonoBehaviour
{
	public delegate void InfoModuleEventHandler(IGeneralInfoData currentData);

	[SerializeField]
	private AudioMapViewModuleProfile _audioProfile;

	[Header("Screens")]
	[SerializeField]
	private UIContextualTutorialHighlightAndFocusCamera _HexObject;

	[Header("Other")]
	[SerializeField]
	private GraphicRaycaster _graphicRaycaster;

	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[SerializeField]
	private Image _blocker;

	private UIGeneralInfoBase[] _modules;

	private UIGeneralInfoBase _currentModule;

	private readonly List<ContextualData> _totalData = new List<ContextualData>();

	private readonly List<ContextualData> _currentlyShownData = new List<ContextualData>();

	public static Action OnTutorialsEnabledChanged;

	public static bool AreTutorialsEnabled = true;

	private AudioMapViewModuleProfile AudioProfile
	{
		get
		{
			if (!(_audioProfile != null))
			{
				if (!(UIInfoTools.Instance != null))
				{
					return null;
				}
				return UIInfoTools.Instance.defaultAudioMapViewModuleProfile;
			}
			return _audioProfile;
		}
	}

	public UIGeneralInfoBase CurrentModule => _currentModule;

	public event InfoModuleEventHandler OnShow;

	private void OnEnable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnShow);
		}
	}

	public static void Cache()
	{
		AreTutorialsEnabled = PlayerPrefs.GetInt("TutorialsEnabled", 1) == 1;
	}

	public static void SetTutorialsEnabled(bool enabled)
	{
		PlayerPrefs.SetInt("TutorialsEnabled", enabled ? 1 : 0);
		Cache();
		OnTutorialsEnabledChanged?.Invoke();
		PlayerPrefs.Save();
	}

	private void Awake()
	{
		if (_blocker != null)
		{
			_blocker.enabled = false;
		}
		Canvas component = GetComponent<Canvas>();
		if (component != null)
		{
			component.sortingLayerName = "Default";
			component.sortingOrder = 1010;
		}
		_modules = GetComponentsInChildren<UIGeneralInfoBase>(includeInactive: true);
		UIGeneralInfoBase[] modules = _modules;
		foreach (UIGeneralInfoBase module in modules)
		{
			module.Hide();
			UIGeneralInfoBase uIGeneralInfoBase = module;
			uIGeneralInfoBase.OnHide = (Action)Delegate.Combine(uIGeneralInfoBase.OnHide, (Action)(() =>
			{
				OnInfoHide(module);
			}));
		}
	}

	private void OnDisable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnHide);
		}
		_currentModule?.Hide();
		_totalData.Clear();
	}

	private void OnInfoHide(UIGeneralInfoBase module)
	{
		if (_blocker != null)
		{
			_blocker.enabled = false;
		}
		if (_currentModule == module)
		{
			_currentModule = null;
		}
	}

	private bool CanShowRightNow()
	{
		CameraController s_CameraController = CameraController.s_CameraController;
		if ((object)s_CameraController != null && s_CameraController.IsMoving())
		{
			return false;
		}
		ScenarioRuleLibrary.ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		if (currentScenarioState != null && currentScenarioState.LoadingScenarioState)
		{
			return false;
		}
		SceneController instance = SceneController.Instance;
		if ((object)instance != null && instance.IsLoading)
		{
			return false;
		}
		StoryController instanceFast = Singleton<StoryController>.InstanceFast;
		if ((object)instanceFast != null && instanceFast.IsBusy)
		{
			return false;
		}
		LevelEventsController instanceFast2 = Singleton<LevelEventsController>.InstanceFast;
		if ((object)instanceFast2 != null && instanceFast2.IsBusy)
		{
			return false;
		}
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			return false;
		}
		return true;
	}

	private bool CanShowAtAll()
	{
		if (!AreTutorialsEnabled)
		{
			return false;
		}
		if (Choreographer.IsInNoPartyCampaign())
		{
			return false;
		}
		return true;
	}

	private bool TryShow(object data, out UIGeneralInfoBase info, bool onlyCalc)
	{
		info = null;
		if (!CanShowRightNow())
		{
			return false;
		}
		if (data is ContextualData data2)
		{
			info = _HexObject;
			if (!onlyCalc)
			{
				return _HexObject.Show(data2);
			}
			return true;
		}
		return false;
	}

	private void OnInfoShow(ContextualData data, UIGeneralInfoBase info)
	{
		_currentModule?.Hide();
		if (_blocker != null)
		{
			_blocker.enabled = data.BlockInput;
		}
		OnShow?.Invoke(data);
		_currentModule = info;
		TryShow(data, out var _, onlyCalc: false);
	}

	private void OnShow_Internal(ContextualData data)
	{
		if (TryShow(data, out var info, onlyCalc: true))
		{
			OnInfoShow(data, info);
			RefreshNavigator();
		}
	}

	private bool HasShownTutorial(string key)
	{
		return GlobalStatRepository.Instance.ShownContextualTutorials.HasValue(key);
	}

	private void PushData(ContextualData data)
	{
		if (CanShowAtAll())
		{
			string key = GetDataKey(data);
			if (!_totalData.Any((ContextualData x) => GetDataKey(x) == key) && !HasShownTutorial(key) && IsDataValid(data))
			{
				data.OrderAdded = _totalData.Count;
				_totalData.Add(data);
			}
		}
	}

	public static string GetDataKey(ContextualData data)
	{
		return data.ID;
	}

	private void RefreshNavigator()
	{
		if (!(_currentModule == null))
		{
			List<IGeneralInfoData> datas = ((IEnumerable<ContextualData>)_currentlyShownData).Select((Func<ContextualData, IGeneralInfoData>)((ContextualData x) => x)).ToList();
			_currentModule.RefreshNavigator(datas);
		}
	}

	public static void OnEvent(string eventKey, params object[] parameters)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (Singleton<FHScenarioViewController>.InstanceFast != null)
			{
				Singleton<FHScenarioViewController>.InstanceFast.OnEvent(eventKey, parameters);
				Singleton<FHScenarioViewController>.InstanceFast.ContextualTutorial?._OnEvent(eventKey, parameters);
			}
			else if (Singleton<FHMapChoreographer>.InstanceFast != null)
			{
				Singleton<FHMapChoreographer>.InstanceFast?.View?.Modules.ContextualTutorial._OnEvent(eventKey, parameters);
			}
		}
	}

	public static void OnEvent(SEvent @event)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (@event is SEventActorActiveBonuses sEventActorActiveBonuses)
			{
				OnEvent("ActiveBonusesChanged", sEventActorActiveBonuses);
				TryShowRetaliate();
			}
			else if (@event is SEventAbility sEventAbility)
			{
				if (sEventAbility.AbilitySubType == ESESubTypeAbility.AbilityStart)
				{
					CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
					if (cAbility != null)
					{
						OnEvent("AbilityStarted", cAbility);
					}
				}
				if (sEventAbility.AbilitySubType == ESESubTypeAbility.AbilityTileSelected)
				{
					CAbility cAbility2 = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
					if (cAbility2 != null)
					{
						OnEvent("AbilityTileSelected", cAbility2);
					}
				}
			}
			else if (@event is SEventLoseCard sEventLoseCard)
			{
				OnEvent("LoseCard", sEventLoseCard.CardPile);
			}
			else if (@event is SEventActor { ActorSubType: ESESubTypeActor.ActorInitialized } sEventActor)
			{
				CActor cActor = ScenarioManager.FindActor(sEventActor.MainActorGuid);
				OnEvent("ActorSpawned", cActor);
				TryShowRetaliate();
			}
			else if (@event is SEventPhase { SubTypePhase: ESESubTypePhase.PhaseStart, Type: ESEType.Phase } sEventPhase)
			{
				if (sEventPhase.SubType == CPhase.PhaseType.SelectAbilityCardsOrLongRest && ScenarioManager.CurrentScenarioState.RoundNumber == 1)
				{
					TryShowNewRoomContents();
					RecreateObjectiveActivatedOnScenarioStart();
				}
				else if (sEventPhase.SubType == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
				{
					OnEvent("MonsterCardSelection");
				}
			}
			else if (@event is SEventObjectProp { ObjectPropSubType: ESESubTypeObjectProp.Spawned } sEventObjectProp)
			{
				if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.Obstacle)
				{
					TryShowObstacle();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.Spawner)
				{
					TryShowSpawner();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble)
				{
					TryShowDifficultTerrain();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainWater)
				{
					TryShowWater();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainIce)
				{
					TryShowIcyTerrain();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns)
				{
					TryShowHazardousTerrain();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.Trap)
				{
					TryShowTrap();
				}
				else if (sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.Chest || sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.GoalChest || sEventObjectProp.ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
				{
					TryShowChestAndLoot();
				}
			}
			else if (@event is SEventCampaignSectionTriggered sEventCampaignSectionTriggered)
			{
				OnEvent("CampaignSectionTriggered", sEventCampaignSectionTriggered.SectionID);
			}
			if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityDiscardCards)
			{
				TryShowDiscard();
			}
			if (PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest)
			{
				TryShowRest();
			}
			if (@event is SEventPhase { SubTypePhase: ESESubTypePhase.PhaseStart, Type: ESEType.Phase } sEventPhase2)
			{
				OnEvent("RoundStarted", sEventPhase2.Round);
			}
		}
	}

	private static void RecreateObjectiveActivatedOnScenarioStart()
	{
		foreach (CObjective allObjective in ScenarioManager.CurrentScenarioState.AllObjectives)
		{
			if (allObjective.IsActive)
			{
				OnEvent("ObjectiveActivated", allObjective.EventIdentifier);
			}
		}
	}

	private static void TryShowNewRoomContents()
	{
		TryShowDoor();
		TryShowObstacle();
		TryShowSpawner();
		TryShowObjectives();
		TryShowDifficultTerrain();
		TryShowWater();
		TryShowChestAndLoot();
		TryShowHazardousTerrain();
		TryShowTrap();
		TryShowIcyTerrain();
		TryShowPressurePlate();
		TryShowCombatLog();
	}

	private static void TryShowObjectives()
	{
		TryShowEscapeHex();
		TryShowStartingTiles();
		TryShowSurviveXRoundsObjective();
		TryShowLootObjective();
		TryShowKillObjective();
		TryShowAllyMustSurviveObjective();
	}

	public static void OnEvent(UIEvent @event)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (@event.EventType == UIEvent.EUIEventType.RoomRevealed)
			{
				string mapID = @event.ContextID;
				CMap cMap = ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap x) => x.MapGuid == mapID);
				if (cMap?.MapTiles != null)
				{
					IEnumerable<CTile> mapTiles = from x in cMap.MapTiles
						where x != null
						select ScenarioManager.GetTile(x.ArrayIndex.ToPoint());
					if (!ScenarioManager.StartingTiles.Any((CTile x) => mapTiles.Contains(x)))
					{
						TryShowNewRoomContents();
						OnEvent("RoomRevealed", @event.ContextID);
					}
				}
			}
			else if (@event.EventType == UIEvent.EUIEventType.ObjectiveActivated)
			{
				TryShowObjectives();
				OnEvent("ObjectiveActivated", @event.ContextID);
			}
			else if (@event.EventType == UIEvent.EUIEventType.ElementsChanged)
			{
				TryShowElementsInfused();
			}
			else if (@event.EventType == UIEvent.EUIEventType.ActorConditionsChanged)
			{
				CActor cActor = ScenarioManager.FindActor(@event.ContextActorGUID);
				if (cActor != null && !cActor.IsDead)
				{
					OnEvent("Condition", cActor, @event.ContextID);
				}
			}
			else if (@event.EventType == UIEvent.EUIEventType.DamageChoiceDialogAppeared)
			{
				TryShowPreventDamage();
			}
			else if (@event.EventType == UIEvent.EUIEventType.WindowOpened && @event.ContextID.Contains("GeneralInfo", StringComparison.InvariantCultureIgnoreCase))
			{
				OnEvent("GeneralInfoOpened");
			}
			else if (@event.EventType == UIEvent.EUIEventType.ScenarioModifierHiddenStateUpdated)
			{
				OnEvent("ScenarioModifierHiddenStateUpdated");
			}
			else if (@event.EventType == UIEvent.EUIEventType.UpdateScenarioModifierDescriptions)
			{
				OnEvent("UpdateScenarioModifierDescriptions");
			}
			else if (@event.EventType == UIEvent.EUIEventType.ScenarioLoaded)
			{
				OnEvent("ScenarioLoaded");
			}
			else if (@event.EventType == UIEvent.EUIEventType.ObjectiveCompleted)
			{
				OnEvent("ObjectiveCompleted", @event.ContextID);
			}
			else if (@event.EventType == UIEvent.EUIEventType.DoorUnlocked)
			{
				OnEvent("DoorUnlocked", @event.ContextID);
			}
		}
	}

	private static void TryShowPreventDamage()
	{
		if (Singleton<TakeDamagePanel>.InstanceFast != null && Singleton<TakeDamagePanel>.InstanceFast.ThisPlayerHasTakeDamageControl)
		{
			OnEvent("TakeDamagePanelShown");
		}
	}

	private static void TryShowDoor()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.DoorProps.Where((CObjectProp x) => x is CObjectDoor cObjectDoor && (cObjectDoor.PropTile.m_HexMap.Revealed || (cObjectDoor.PropTile.m_Hex2Map != null && cObjectDoor.PropTile.m_Hex2Map.Revealed))).ToList();
		if (list.Count > 0)
		{
			OnEvent("Door", list[0]);
		}
	}

	private static void TryShowObstacle()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectObstacle cObjectObstacle && cObjectObstacle.PropTile.m_HexMap.Revealed && cObjectObstacle.RuntimeAttachedActor == null).ToList();
		if (list.Count > 0)
		{
			OnEvent("Obstacle", list);
		}
	}

	private static void TryShowDifficultTerrain()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectDifficultTerrain cObjectDifficultTerrain && cObjectDifficultTerrain.PropTile.m_HexMap.Revealed && cObjectDifficultTerrain.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble).ToList();
		if (list.Count > 0)
		{
			OnEvent("DifficultTerrain", list);
		}
	}

	private static void TryShowWater()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectDifficultTerrain cObjectDifficultTerrain && cObjectDifficultTerrain.PropTile.m_HexMap.Revealed && cObjectDifficultTerrain.ObjectType == ScenarioManager.ObjectImportType.TerrainWater).ToList();
		if (list.Count > 0)
		{
			OnEvent("Water", list);
		}
	}

	private static void TryShowHazardousTerrain()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectHazardousTerrain cObjectHazardousTerrain && cObjectHazardousTerrain.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("HazardousTerrain", list);
		}
	}

	private static void TryShowTrap()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectTrap cObjectTrap && cObjectTrap.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("Trap", list);
		}
	}

	private static void TryShowIcyTerrain()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectIceTerrain cObjectIceTerrain && cObjectIceTerrain.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("IcyTerrain", list);
		}
	}

	private static void TryShowChestAndLoot()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => (x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest) && x.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("Chest", list);
		}
		List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken && x.PropTile.m_HexMap.Revealed).ToList();
		if (list2.Count > 0)
		{
			OnEvent("Loot", list2);
		}
	}

	private static void TryShowSpawner()
	{
		List<CSpawner> list = ScenarioManager.CurrentScenarioState.Spawners.Where((CSpawner x) => x.Prop != null && x.Prop.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("Spawner", list);
		}
	}

	private static void TryShowPressurePlate()
	{
		List<CObjectProp> list = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => x is CObjectPressurePlate cObjectPressurePlate && cObjectPressurePlate.PropTile.m_HexMap.Revealed).ToList();
		if (list.Count > 0)
		{
			OnEvent("PressurePlate", list);
		}
	}

	private static void TryShowEscapeHex()
	{
		CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective x) => x is CObjective_ActorsEscaped);
		if (cObjective != null && cObjective.IsActive)
		{
			List<TileIndex> list = (from x in ScenarioManager.CurrentScenarioState.GetEscapeZones()
				where ScenarioManager.GetTile(x.ToPoint()).m_HexMap.Revealed
				select x).ToList();
			if (list.Count > 0)
			{
				OnEvent("EscapeHex", cObjective, list);
			}
		}
	}

	private static void TryShowStartingTiles()
	{
		if (SaveData.Instance?.Global?.CurrentCustomLevelData != null && ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.RoundNumber == 1 && PhaseManager.CurrentPhase is CPhaseSelectAbilityCardsOrLongRest)
		{
			List<TileIndex> startingTileIndexes = SaveData.Instance.Global.CurrentCustomLevelData.StartingTileIndexes;
			if (startingTileIndexes.Count > 0)
			{
				OnEvent("StartingTiles", startingTileIndexes);
			}
		}
	}

	private static void TryShowSurviveXRoundsObjective()
	{
		CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective x) => x is CObjective_ReachRound);
		if (cObjective != null && cObjective.IsActive)
		{
			OnEvent("Objective", cObjective);
		}
	}

	private static void TryShowLootObjective()
	{
		CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective x) => x is CObjective_LootX);
		if (cObjective != null && cObjective.IsActive)
		{
			OnEvent("Objective", cObjective);
		}
	}

	private static void TryShowKillObjective()
	{
		CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective x) => x is CObjective_KillAllEnemies);
		if (cObjective != null && cObjective.IsActive)
		{
			OnEvent("Objective", cObjective);
		}
	}

	private static void TryShowAllyMustSurviveObjective()
	{
		CObjective cObjective = ScenarioManager.CurrentScenarioState.AllObjectives.FirstOrDefault((CObjective x) => (x is CObjective_XCharactersDie cObjective_XCharactersDie && cObjective_XCharactersDie.ObjectiveFilter.FilterActorType.HasFlag(CAbilityFilter.EFilterActorType.Ally)) || (x is CObjective_Scripted cObjective_Scripted && cObjective_Scripted.ValidActors != null));
		if (cObjective != null && cObjective.IsActive)
		{
			OnEvent("Objective", cObjective);
		}
	}

	private void ShowProtectObjective()
	{
		if (HasShownTutorial("FH_Hint_Objectives_Protect"))
		{
			return;
		}
		IEnumerable<CObjective> protectObjectives = ScenarioManager.CurrentScenarioState.LoseObjectives.Where((CObjective x) => x is CObjective_DestroyXObjects || (x is CObjective_XCharactersDie cObjective_XCharactersDie && cObjective_XCharactersDie.ObjectiveFilter.FilterActorType.HasFlag(CAbilityFilter.EFilterActorType.Ally)) || (x is CObjective_Scripted cObjective_Scripted2 && cObjective_Scripted2.ValidActors != null)).Concat(ScenarioManager.CurrentScenarioState.WinObjectives.Where((CObjective x) => x is CObjective_ActorsEscaped cObjective_ActorsEscaped && cObjective_ActorsEscaped.ObjectiveFilter.FilterActorType.HasFlag(CAbilityFilter.EFilterActorType.Ally)));
		IEnumerable<GameObject> source = Singleton<ScenarioInfoModule>.InstanceFast?.MissionObjectiveContainer?.ObjectiveInstances?.Where((MissionObjectiveUI x) => protectObjectives.Contains(x.m_Objective)).Select((MissionObjectiveUI x) => x.gameObject);
		List<CActor> list = new List<CActor>();
		foreach (CObjective obj in protectObjectives)
		{
			if (obj is CObjective_Scripted cObjective_Scripted)
			{
				list.AddRange(cObjective_Scripted.ValidActors());
				continue;
			}
			foreach (ActorState item in ScenarioManager.CurrentScenarioState.ActorStates.Where((ActorState x) => obj.ObjectiveFilter.IsValidTarget(x)))
			{
				CActor actor = item.Actor;
				if (actor != null)
				{
					list.Add(actor);
				}
			}
		}
		if (protectObjectives.Any())
		{
			PushData(new ContextualData
			{
				Actors = list.Distinct().ToList(),
				ID = "FH_Hint_Objectives_Protect",
				Caption = "FH_Hint_Name_Objectives_Protect",
				UIObjects = source.ToList(),
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
		}
	}

	private void ShowDestroyPropObjective()
	{
		if (HasShownTutorial("FH_Hint_Objectives_Health"))
		{
			return;
		}
		IEnumerable<CObjective> destroyPropObjectives = ScenarioManager.CurrentScenarioState.WinObjectives.Where((CObjective x) => x is CObjective_DestroyXObjects || (x is CObjective_XCharactersDie cObjective_XCharactersDie && cObjective_XCharactersDie.ObjectiveFilter.FilterEnemyType.HasFlag(CAbilityFilter.EFilterEnemy.Object)));
		IEnumerable<GameObject> enumerable = Singleton<ScenarioInfoModule>.InstanceFast?.MissionObjectiveContainer?.ObjectiveInstances?.Where((MissionObjectiveUI x) => destroyPropObjectives.Contains(x.m_Objective))?.Select((MissionObjectiveUI x) => x.gameObject);
		List<CActor> list = new List<CActor>();
		List<CObjectProp> list2 = new List<CObjectProp>();
		foreach (CObjective obj in destroyPropObjectives)
		{
			foreach (ActorState item in ScenarioManager.CurrentScenarioState.ActorStates.Where((ActorState x) => obj.ObjectiveFilter.IsValidTarget(x)))
			{
				CActor actor = item.Actor;
				if (actor != null && actor.Tile.m_HexMap.Revealed)
				{
					list.Add(actor);
				}
			}
			foreach (CObjectProp item2 in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.Where((CObjectProp x) => obj.ObjectiveFilter.IsValidProp(x)))
			{
				if (item2.PropTile.m_HexMap.Revealed)
				{
					list2.Add(item2);
				}
			}
		}
		if (destroyPropObjectives.Any() && (list.Count > 0 || list2.Count > 0))
		{
			PushData(new ContextualData
			{
				Actors = list,
				Props = list2,
				UIObjects = enumerable?.ToList(),
				ID = "FH_Hint_Objectives_Health",
				Caption = "FH_Hint_Name_Objectives_Health",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
		}
	}

	private static void TryShowElementsInfused()
	{
		if (ElementInfusionBoardManager.GetElementsToInfuse().Count > 0 || ElementInfusionBoardManager.GetAvailableElements().Count > 0)
		{
			OnEvent("Elements");
		}
	}

	private static void TryShowRetaliate()
	{
		List<CActor> list = ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x.CachedHasRetaliate).ToList();
		if (list.Count > 0)
		{
			OnEvent("Retaliate", list);
		}
	}

	private static void TryShowRest()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			playerActor.CountCardsForExhausted(null, out var cardsInHand);
			if (cardsInHand < 2)
			{
				OnEvent("Rest");
				break;
			}
		}
		UISimpleActionBox uISimpleActionBox = Singleton<FHScenarioViewController>.InstanceFast?.Modules?.SimpleActionBox;
		if (!(uISimpleActionBox == null))
		{
			if (uISimpleActionBox.LongRest != null)
			{
				uISimpleActionBox.LongRest.onValueChanged.RemoveListener(OnClickRestButton);
				uISimpleActionBox.LongRest.onValueChanged.AddListener(OnClickRestButton);
			}
			if (uISimpleActionBox.ShortRest != null)
			{
				uISimpleActionBox.ShortRest.onValueChanged.RemoveListener(OnClickRestButton);
				uISimpleActionBox.ShortRest.onValueChanged.AddListener(OnClickRestButton);
			}
		}
	}

	private static void OnClickRestButton(bool x)
	{
		OnEvent("Rest");
	}

	private static void TryShowDiscard()
	{
		if (Singleton<CardsHandManager>.InstanceFast?.CurrentHand != null && Singleton<CardsHandManager>.Instance.CurrentHand.currentMode == CardHandMode.DiscardCard)
		{
			OnEvent("Discard");
		}
	}

	private static void TryShowCombatLog()
	{
		OnEvent("CombatLog");
	}

	public static void OnEvent(CampaignEvent campaignEvent)
	{
		if (campaignEvent is CampaignEvent.CharacterLevelUp characterLevelUp && characterLevelUp.Character.CreationFlowPassed)
		{
			OnEvent("LevelUp", characterLevelUp.Character);
		}
	}

	public static void OnFHMapViewStateEnter(FHMapViewState state)
	{
		if (state is EventViewState { EventState: not null } eventViewState)
		{
			OnEvent("OutpostEvent", eventViewState.EventState.Def);
		}
		OnEvent("ViewStateChanged", state);
		TryShowDistribution();
		TryShowPartyResources();
		TryShowPartyColumn();
		TryShowPersonalQuestSelection();
		TryShowQuickAccessBar();
		TryShowBuildButton();
		TryShowWorldMapButton();
		TryShowLegacyPerks();
		TryShowWreckedBuildings();
		TryShowScenarioSelection();
	}

	public static void TryShowLegacyPerks()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null && Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is NewCharacterViewState && !Singleton<FHMapChoreographer>.InstanceFast.GameState.IsInPrologue)
		{
			OnEvent("LegacyPerks");
		}
	}

	public static void TryShowDistribution()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			DistributionModule distributionModule = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.DistributionModule;
			if (distributionModule.isActiveAndEnabled)
			{
				OnEvent("Distribution", distributionModule);
			}
		}
	}

	public static void TryShowPartyResources()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			MainHeaderModule mainHeaderModule = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.MainHeaderModule;
			if (mainHeaderModule.isActiveAndEnabled)
			{
				OnEvent("Resources", mainHeaderModule);
			}
		}
	}

	public static void TryShowPartyColumn()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			PartyColumnModule partyColumn = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.Characters.PartyColumn;
			if (partyColumn.isActiveAndEnabled)
			{
				OnEvent("Party", partyColumn);
			}
		}
	}

	public static void TryShowPersonalQuestSelection()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null && Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is PersonalQuestSelectionViewState)
		{
			PersonalQuestSelectionModule personalQuestSelection = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.Characters.PersonalQuestSelection;
			OnEvent("PersonalQuest", personalQuestSelection);
		}
	}

	public static void TryShowQuickAccessBar()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null && Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is OutpostVillageMapState)
		{
			OutpostQuickAccessBarModule quickAccessBar = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.QuickAccessBar;
			if (quickAccessBar.isActiveAndEnabled)
			{
				OnEvent("QuickAccess", quickAccessBar);
			}
		}
	}

	public static void TryShowBuildButton()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			OutpostQuickAccessBarModule quickAccessBar = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.QuickAccessBar;
			if (quickAccessBar.BuildButton != null && quickAccessBar.BuildButton.isActiveAndEnabled)
			{
				OnEvent("BuildButton", quickAccessBar.BuildButton);
				quickAccessBar.BuildButton.onValueChanged.RemoveListener(OnBuildButtonClicked);
				quickAccessBar.BuildButton.onValueChanged.AddListener(OnBuildButtonClicked);
			}
		}
	}

	public static void TryShowWreckedBuildings()
	{
		if (IsValidDuringOutpostView())
		{
			List<BuildingState> list = Singleton<FHMapChoreographer>.InstanceFast.GameState.Outpost.Buildings.Where((BuildingState x) => x.Wrecked).ToList();
			if (list.Count > 0)
			{
				OnEvent("WreckedBuildings", list);
			}
		}
	}

	private static void OnBuildButtonClicked(bool x)
	{
		OnEvent("BuildButtonClicked");
	}

	public static void TryShowWorldMapButton()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			OutpostQuickAccessBarModule quickAccessBar = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.QuickAccessBar;
			if (quickAccessBar.WorldMapButton != null && quickAccessBar.WorldMapButton.isActiveAndEnabled)
			{
				OnEvent("WorldMap", quickAccessBar.WorldMapButton);
			}
		}
	}

	public static void TryShowScenarioSelection()
	{
		if (IsValidDuringScenarioSelectionView())
		{
			OnEvent("Outpost - Quests");
		}
	}

	public static bool IsValidWhileNoOutpostEvent()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentState is EventViewState)
		{
			return false;
		}
		return true;
	}

	public bool IsValidWhileNotTravelling()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentState is WorldTravelViewState || Singleton<FHMapChoreographer>.InstanceFast?.GameState?.IsPartyTravelling == true)
		{
			return false;
		}
		return true;
	}

	public bool IsValidWhileNoDistribution()
	{
		if (Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentModalState is DistributionViewState || Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentModalState is EventDistributionViewState)
		{
			return false;
		}
		if (Singleton<FHMapChoreographer>.InstanceFast?.GameState != null && (Singleton<FHMapChoreographer>.InstanceFast.GameState.HasUnresolvedAction(CampaignInteractionType.GainItem) || Singleton<FHMapChoreographer>.InstanceFast.GameState.HasUnresolvedAction(CampaignInteractionType.GainMaterials) || Singleton<FHMapChoreographer>.InstanceFast.GameState.HasUnresolvedAction(CampaignInteractionType.GainGold) || Singleton<FHMapChoreographer>.InstanceFast.GameState.HasUnresolvedAction(CampaignInteractionType.GainAnyHerbs)))
		{
			return false;
		}
		return true;
	}

	public bool IsValidDuringDowntime()
	{
		FHMapChoreographer instanceFast = Singleton<FHMapChoreographer>.InstanceFast;
		if ((object)instanceFast == null || instanceFast.GameState?.Outpost?.Phase != OutpostPhase.Downtime)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidDuringOutpostView()
	{
		return Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentState is OutpostVillageMapState;
	}

	public static bool IsValidDuringScenarioSelectionView()
	{
		return Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentState is ScenarioSelectionViewState;
	}

	public static bool IsValidDuringActiveScenario()
	{
		if (ScenarioManager.CurrentScenarioState != null)
		{
			return !ScenarioManager.CurrentScenarioState.ScenarioEnded;
		}
		return false;
	}

	public bool IsShowingHint(string hintID)
	{
		return _currentlyShownData.Any((ContextualData x) => x.ID.Equals(hintID, StringComparison.InvariantCultureIgnoreCase));
	}

	private void _OnEvent(string eventKey, params object[] parameters)
	{
		if (!CanShowAtAll())
		{
			return;
		}
		switch (eventKey)
		{
		case "GeneralInfoOpened":
			if (GlobalStatRepository.Instance.CompletedScenarios.HasValue("201"))
			{
				break;
			}
			PushData(new ContextualData
			{
				ID = "FH_Hint_UI_Inspection",
				Caption = "FH_Hint_UI_Name_Inspection",
				FixedPosition = true,
				OnShown = () =>
				{
					UIGeneralInfoModule uIGeneralInfoModule = Singleton<FHScenarioViewController>.InstanceFast?.GeneralInfo ?? Singleton<FHMapChoreographer>.InstanceFast?.View?.Modules.GeneralInfo;
					if (uIGeneralInfoModule != null)
					{
						ClickTrackerScopeElement[] componentsInChildren = uIGeneralInfoModule.GetComponentsInChildren<ClickTrackerScopeElement>(includeInactive: true);
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].area.Add(CurrentModule.transform as RectTransform);
						}
					}
				},
				OnClosed = () =>
				{
					UIGeneralInfoModule uIGeneralInfoModule = Singleton<FHScenarioViewController>.InstanceFast?.GeneralInfo ?? Singleton<FHMapChoreographer>.InstanceFast?.View?.Modules.GeneralInfo;
					if (uIGeneralInfoModule != null)
					{
						ClickTrackerScopeElement[] componentsInChildren = uIGeneralInfoModule.GetComponentsInChildren<ClickTrackerScopeElement>(includeInactive: true);
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].area.Remove(CurrentModule.transform as RectTransform);
						}
					}
				},
				AdditionalValidation = () =>
				{
					if (IsShowingHint("FH_Hint_UI_Inspection"))
					{
						UIGeneralInfoModule uIGeneralInfoModule = Singleton<FHScenarioViewController>.InstanceFast?.GeneralInfo ?? Singleton<FHMapChoreographer>.InstanceFast?.View?.Modules.GeneralInfo;
						if (uIGeneralInfoModule != null)
						{
							return uIGeneralInfoModule.CurrentModule != null;
						}
					}
					return true;
				}
			});
			break;
		case "Codex":
		{
			Button codexButton = parameters[0] as Button;
			if (codexButton != null)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { codexButton.gameObject },
					ID = "FH_Hint_UI_Codex",
					Caption = "GUI_Wiki",
					FixedPosition = false,
					AdditionalValidation = () => !IsShowingHint("FH_Hint_UI_Codex") || codexButton.gameObject.activeInHierarchy,
					NonRemovingValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution()
				});
			}
			break;
		}
		case "ItemShown":
		{
			FHItemTooltip.ItemTooltipParams obj = parameters[0] as FHItemTooltip.ItemTooltipParams;
			RectTransform anchorTarget = obj.AnchorTarget;
			if (obj.ItemData.FlipScenarioItemID.HasValue)
			{
				List<GameObject> uIObjects4 = null;
				if (anchorTarget != null)
				{
					uIObjects4 = new List<GameObject> { anchorTarget.gameObject };
				}
				PushData(new ContextualData
				{
					UIObjects = uIObjects4,
					ID = "FH_Hint_Items_Flipping",
					Caption = "FH_Hint_Items_Name_Flipping",
					FixedPosition = true
				});
			}
			break;
		}
		case "AbilityStarted":
			if (parameters[0] as CAbility is CAbilityAttack { State: CAbilityAttack.EAttackState.SelectAttackFocus, AttackSummary: not null } cAbilityAttack && cAbilityAttack.AttackSummary.Targets.Any((CAttackSummary.TargetSummary x) => x.Pierce > 0))
			{
				PushData(new ContextualData
				{
					ID = ConditionIdToGlossaryKey(EffectsBar.FEffect.Pierce.ToString()),
					Caption = ConditionIdToCaptionKey(EffectsBar.FEffect.Pierce.ToString()),
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		case "AbilityTileSelected":
		{
			if (!(parameters[0] as CAbility is CAbilityAttack { AttackSummary: not null, State: CAbilityAttack.EAttackState.SelectAttackFocus } cAbilityAttack2))
			{
				break;
			}
			string conditionId2 = EffectsBar.FEffect.Advantage.ToString();
			string text3 = ConditionIdToGlossaryKey(conditionId2);
			if (!HasShownTutorial(text3))
			{
				List<CAttackSummary.TargetSummary> list5 = cAbilityAttack2.AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.OverallAdvantage == EAdvantageStatuses.Advantage).ToList();
				if (list5.Count > 0)
				{
					List<CActor> list6 = new List<CActor>();
					List<GameObject> list7 = new List<GameObject>();
					foreach (CAttackSummary.TargetSummary item6 in list5)
					{
						EffectsBar.EffectRef effectUI3 = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActor(item6.ActorToAttack)).m_WorldspacePanelUI.PreviewBar.GetEffectUI(EffectsBar.FEffect.Advantage);
						list7.Add(effectUI3.m_EffectObject);
						list6.Add(item6.ActorToAttack);
					}
					PushData(new ContextualData
					{
						ID = text3,
						Caption = ConditionIdToCaptionKey(conditionId2),
						Actors = list6,
						UIObjects = list7,
						FixedPosition = true,
						Group = "Tactical",
						AdditionalValidation = () => IsValidDuringActiveScenario()
					});
				}
			}
			string conditionId3 = EffectsBar.FEffect.Disadvantage.ToString();
			string text4 = ConditionIdToGlossaryKey(conditionId3);
			if (HasShownTutorial(text4))
			{
				break;
			}
			List<CAttackSummary.TargetSummary> list8 = cAbilityAttack2.AttackSummary.Targets.Where((CAttackSummary.TargetSummary x) => x.OverallAdvantage == EAdvantageStatuses.Disadvantage).ToList();
			if (list8.Count <= 0)
			{
				break;
			}
			List<CActor> list9 = new List<CActor>();
			List<GameObject> list10 = new List<GameObject>();
			foreach (CAttackSummary.TargetSummary item7 in list8)
			{
				EffectsBar.EffectRef effectUI4 = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActor(item7.ActorToAttack)).m_WorldspacePanelUI.PreviewBar.GetEffectUI(EffectsBar.FEffect.Disadvantage);
				list10.Add(effectUI4.m_EffectObject);
				list9.Add(item7.ActorToAttack);
			}
			PushData(new ContextualData
			{
				ID = text4,
				Caption = ConditionIdToCaptionKey(conditionId3),
				Actors = list9,
				UIObjects = list10,
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "MonsterCardSelection":
			PushData(new ContextualData
			{
				ID = "FH_Hint_Combat_Monsters",
				Caption = "FH_Hint_Combat_Name_Monsters",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		case "HighlightActorDuringAbility":
		{
			string text = "FH_Hint_Combat_LineOfSight";
			if (HasShownTutorial(text))
			{
				break;
			}
			CActor cActor3 = parameters[0] as CActor;
			CAbility cAbility = parameters[1] as CAbility;
			if (cAbility.IsCurrentlyTargetingActors() && cAbility.AbilityFilter != null && cAbility.AbilityFilter.IsValidTarget(cActor3, cAbility.TargetingActor, cAbility.IsTargetedAbility, useTargetOriginalType: false, false) && !cAbility.ValidActorsInRange.Contains(cActor3) && cAbility.Targeting != CAbility.EAbilityTargeting.Custom && (cAbility.Range <= -1 || cAbility.Range >= 99 || cActor3.GetDistanceTo(cAbility.TargetingActor, viaPathfinding: true) <= cAbility.Range))
			{
				PushData(new ContextualData
				{
					Actor = cActor3,
					ID = text,
					Caption = "GUI_HOUSE_RULES_LINE_OF_SIGHT",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "ActiveBonusesChanged":
		{
			SEventActorActiveBonuses sEventActorActiveBonuses = parameters[0] as SEventActorActiveBonuses;
			string actorGUID = sEventActorActiveBonuses.MainActorGuid;
			if (HasShownTutorial("FH_Hint_Card_PersistentBonus") && HasShownTutorial("FH_Hint_Card_RoundBonus"))
			{
				break;
			}
			CActor cActor = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actorGUID)?.Actor;
			if (cActor == null || !(cActor is CPlayerActor))
			{
				break;
			}
			foreach (CActiveBonus activeBonuse in cActor.ActiveBonuses)
			{
				if (activeBonuse.Ability.ActiveBonusData.Hidden)
				{
					continue;
				}
				if (activeBonuse.Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent)
				{
					PushData(new ContextualData
					{
						ID = "FH_Hint_Card_PersistentBonus",
						Caption = "FH_Hint_Card_Name_PersistentBonus",
						FixedPosition = true,
						Group = "Tactical",
						AdditionalValidation = () => IsValidDuringActiveScenario()
					});
				}
				else if (activeBonuse.Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Round)
				{
					PushData(new ContextualData
					{
						ID = "FH_Hint_Card_RoundBonus",
						Caption = "FH_Hint_Card_Name_RoundBonus",
						FixedPosition = true,
						Group = "Tactical",
						AdditionalValidation = () => IsValidDuringActiveScenario()
					});
				}
			}
			break;
		}
		case "LoseCard":
			if ((string)parameters[0] == "PermanentlyLostAbilityCards")
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Cards_PermaLost",
					Caption = "FH_Hint_Name_Cards_PermaLost",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			else
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Cards_Lost",
					Caption = "CODEX_DiscardedAndLostCards_Title",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		case "CombatLog":
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl2 = InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable.SingleOrDefault((InteractabilityIsolatedUIControl x) => x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.CombatLog);
			if (interactabilityIsolatedUIControl2 != null)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { interactabilityIsolatedUIControl2.gameObject },
					ID = "FH_Hint_UI_BattleLog",
					Caption = "FH_Hint_UI_Name_BattleLog",
					FixedPosition = false,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "Discard":
		{
			if (Singleton<CardsHandManager>.InstanceFast?.CurrentHand?.AllCards == null)
			{
				return;
			}
			List<GameObject> list13 = (from x in Singleton<CardsHandManager>.Instance.CurrentHand.AllCards
				where x?.AbilityCard != null && x.AbilityCard.CurrentCardPile == CBaseCard.ECardPile.Hand && x.miniAbilityCard != null
				select x.miniAbilityCard.gameObject).ToList();
			if (list13.Count > 0)
			{
				PushData(new ContextualData
				{
					UIObjects = list13,
					ID = "FH_Hint_Cards_Discard",
					Caption = "FH_Hint_Name_Cards_Discard_Rest",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "Rest":
		{
			UISimpleActionBox uISimpleActionBox = Singleton<FHScenarioViewController>.InstanceFast?.Modules?.SimpleActionBox;
			if (uISimpleActionBox == null)
			{
				return;
			}
			FHToggle longRest = uISimpleActionBox.LongRest;
			FHToggle shortRest = uISimpleActionBox.ShortRest;
			bool flag = shortRest != null && shortRest.gameObject != null && shortRest.IsInteractable();
			bool flag2 = longRest != null && longRest.gameObject != null && longRest.IsInteractable();
			if (!(flag | flag2))
			{
				break;
			}
			PushData(new ContextualData
			{
				ID = "FH_Hint_Cards_Rest",
				Caption = "FH_Hint_Name_Cards_Rest",
				FixedPosition = true,
				Group = "Tactical",
				Priority = 10,
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			if (flag)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { shortRest.gameObject },
					ID = "FH_Hint_Cards_ShortRest",
					Caption = "GUI_SHORT_REST",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 9,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (flag2)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { longRest.gameObject },
					ID = "FH_Hint_Cards_LongRest",
					Caption = "GUI_LONG_REST",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 8,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "Retaliate":
		{
			List<CActor> list4 = parameters[0] as List<CActor>;
			EffectsBar.FEffect eff = EffectsBar.FEffect.Retaliate;
			string conditionId = eff.ToString();
			List<GameObject> uIObjects2 = null;
			foreach (CActor item8 in list4)
			{
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActor(item8);
				if (gameObject == null)
				{
					continue;
				}
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
				if (actorBehaviour == null)
				{
					continue;
				}
				EffectsBar effectBar = actorBehaviour.m_WorldspacePanelUI.EffectBar;
				if (!(effectBar == null))
				{
					EffectsBar.EffectRef effectUI2 = effectBar.GetEffectUI(eff);
					if (effectUI2 != null)
					{
						uIObjects2 = new List<GameObject> { effectUI2.m_EffectObject };
						break;
					}
				}
			}
			PushData(new ContextualData
			{
				Actors = list4,
				UIObjects = uIObjects2,
				ID = ConditionIdToGlossaryKey(conditionId),
				Caption = ConditionIdToCaptionKey(conditionId),
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Condition":
		{
			CActor actor = parameters[0] as CActor;
			string text2 = parameters[1] as string;
			if (!Enum.TryParse<EffectsBar.FEffect>(text2, out var result))
			{
				break;
			}
			EffectsBar.EffectRef effectUI = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActor(actor)).m_WorldspacePanelUI.EffectBar.GetEffectUI(result);
			List<GameObject> uIObjects = null;
			if (effectUI != null)
			{
				uIObjects = new List<GameObject> { effectUI.m_EffectObject };
			}
			if (effectUI != null)
			{
				PushData(new ContextualData
				{
					Actor = actor,
					UIObjects = uIObjects,
					ID = ConditionIdToGlossaryKey(text2),
					Caption = ConditionIdToCaptionKey(text2),
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "ChooseActorToBurnCardsFor":
			_ = Singleton<TakeDamagePanel>.InstanceFast;
			PushData(new ContextualData
			{
				ID = "FH_Hint_Combat_AllyDamagePrevention",
				Caption = "FH_Hint_Combat_Name_AllyDamagePrevention",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		case "TakeDamagePanelShown":
			if (!GlobalStatRepository.Instance.CompletedScenarios.HasValue("202"))
			{
				TakeDamagePanel instanceFast = Singleton<TakeDamagePanel>.InstanceFast;
				PushData(new ContextualData
				{
					UIObjects = ((instanceFast?.BurnAvailableCardsToggle?.gameObject != null && instanceFast?.BurnDiscardedCardsToggle?.gameObject != null) ? new List<GameObject>
					{
						instanceFast.BurnAvailableCardsToggle.gameObject,
						instanceFast.BurnDiscardedCardsToggle.gameObject
					} : null),
					ID = "FH_Hint_Combat_DamageReduction",
					Caption = "FH_Hint_Combat_Name_DamageReduction",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		case "ModifierDrawn":
		{
			if (!GlobalStatRepository.Instance.CompletedScenarios.HasValue("202"))
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Combat_Modifiers",
					Caption = "COMPENDIUM_AttackModifiers_Title",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			CAttackSummary.TargetSummary targetSummary = parameters[0] as CAttackSummary.TargetSummary;
			if (targetSummary?.UsedAttackMods != null && targetSummary.UsedAttackMods.Any((AttackModifierYMLData x) => x.MathModifier == "*0"))
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Combat_Modifier0",
					Caption = "FH_Hint_Combat_Name_Modifier0",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "Elements":
		{
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = InteractabilityManager.s_Instance.ControlsCurrentlyIsolatable.SingleOrDefault((InteractabilityIsolatedUIControl x) => x.ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ElementInfusionBoard);
			PushData(new ContextualData
			{
				UIObjects = ((interactabilityIsolatedUIControl?.gameObject != null) ? new List<GameObject> { interactabilityIsolatedUIControl.gameObject } : null),
				ID = "FH_Hint_Combat_Infusions",
				Caption = "FH_Hint_Combat_Name_Infusions",
				FixedPosition = true,
				Group = "Tactical"
			});
			break;
		}
		case "Objective":
		{
			CObjective objective = parameters[0] as CObjective;
			MissionObjectiveUI missionObjectiveUI = Singleton<ScenarioInfoModule>.InstanceFast?.MissionObjectiveContainer?.ObjectiveInstances?.SingleOrDefault((MissionObjectiveUI x) => x.m_Objective == objective);
			List<GameObject> list = new List<GameObject>();
			if (missionObjectiveUI != null && missionObjectiveUI.gameObject != null)
			{
				list.Add(missionObjectiveUI.gameObject);
			}
			if (objective is CObjective_ReachRound)
			{
				PushData(new ContextualData
				{
					UIObjects = list,
					ID = "FH_Hint_Objectives_Survive",
					Caption = "FH_Hint_Name_Objectives_Survive",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			else if (objective is CObjective_KillAllEnemies)
			{
				PushData(new ContextualData
				{
					UIObjects = list,
					ID = "FH_Hint_Objectives_GeneralKill",
					Caption = "FH_Hint_Name_Objectives_GeneralKill",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 10,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			else if (objective is CObjective_LootX)
			{
				PushData(new ContextualData
				{
					UIObjects = list,
					ID = "FH_Hint_Objectives_Collect",
					Caption = "FH_Hint_Name_Objectives_Collect",
					FixedPosition = true,
					Group = "Tactical",
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (objective is CObjective_DestroyXObjects || objective is CObjective_XCharactersDie || objective is CObjective_Scripted || objective is CObjective_ActorsEscaped)
			{
				ShowProtectObjective();
				ShowDestroyPropObjective();
			}
			break;
		}
		case "EscapeHex":
		{
			CObjective objective2 = parameters[0] as CObjective;
			MissionObjectiveUI missionObjectiveUI2 = Singleton<ScenarioInfoModule>.InstanceFast?.MissionObjectiveContainer?.ObjectiveInstances?.SingleOrDefault((MissionObjectiveUI x) => x.m_Objective == objective2);
			if (missionObjectiveUI2 == null)
			{
				return;
			}
			List<TileIndex> source3 = parameters[1] as List<TileIndex>;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { missionObjectiveUI2.gameObject },
				ID = "FH_Hint_Objectives_Escape",
				Caption = "FH_Hint_Name_Objectives_Escape",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			PushData(new ContextualData
			{
				Tiles = source3.Select((TileIndex tile) => ScenarioManager.GetTile(tile.ToPoint())).ToArray(),
				ID = "FH_Hint_Hex_EscapeZone",
				Caption = "FH_Hint_Name_Hex_EscapeZone",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "StartingTiles":
		{
			List<TileIndex> source2 = parameters[0] as List<TileIndex>;
			PushData(new ContextualData
			{
				Tiles = source2.Select((TileIndex tile) => ScenarioManager.GetTile(tile.ToPoint())).ToArray(),
				ID = "FH_Hint_Hex_Starting",
				Caption = "FH_Hint_Name_Hex_Starting",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Spawner":
		{
			List<CSpawner> source = parameters[0] as List<CSpawner>;
			PushData(new ContextualData
			{
				Props = source.Select((CSpawner x) => x.Prop).ToList(),
				ID = "FH_Hint_Hex_Spawner",
				Caption = "FH_Hint_Name_Hex_Spawner",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Obstacle":
		{
			List<CObjectProp> props2 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props2,
				ID = "FH_Hint_Hex_Obstacle",
				Caption = "CODEX_Obstacle_Title",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "PressurePlate":
		{
			List<CObjectProp> props9 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props9,
				ID = "FH_Hint_Hex_PressurePlate",
				Caption = "PRESSURE_PLATE_TOOLTIP",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "DifficultTerrain":
		{
			List<CObjectProp> props6 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props6,
				ID = "FH_Hint_Hex_DifficultTerrain",
				Caption = "CODEX_DifficultTerrain_Title",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Water":
		{
			List<CObjectProp> props7 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props7,
				ID = "FH_Hint_Hex_Water",
				Caption = "FH_Hint_Name_Hex_Water",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "HazardousTerrain":
		{
			List<CObjectProp> props5 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props5,
				ID = "FH_Hint_Hex_Hazardous",
				Caption = "FH_Hint_Name_Hex_Hazardous",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Trap":
		{
			List<CObjectProp> props4 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props4,
				ID = "FH_Hint_Hex_Trap",
				Caption = "PoisonMine",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "IcyTerrain":
		{
			List<CObjectProp> props8 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props8,
				ID = "FH_Hint_Hex_Icy",
				Caption = "FH_Hint_Name_Hex_Icy",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Chest":
		{
			List<CObjectProp> props3 = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props3,
				ID = "FH_Hint_Hex_Chest",
				Caption = "Chest",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Loot":
		{
			List<CObjectProp> props = parameters[0] as List<CObjectProp>;
			PushData(new ContextualData
			{
				Props = props,
				ID = "FH_Hint_Hex_Loot",
				Caption = "Loot",
				FixedPosition = true,
				Group = "Tactical",
				AdditionalValidation = () => IsValidDuringActiveScenario()
			});
			break;
		}
		case "Door":
		{
			CObjectDoor cObjectDoor = parameters[0] as CObjectDoor;
			if (!GlobalStatRepository.Instance.CompletedScenarios.HasValue("201"))
			{
				PushData(new ContextualData
				{
					Prop = cObjectDoor,
					ID = "FH_Hint_Door_General",
					Caption = "Door",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 10,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (cObjectDoor.DoorIsLocked)
			{
				PushData(new ContextualData
				{
					Prop = cObjectDoor,
					ID = "FH_Hint_Door_Locked",
					Caption = "LOCKED_DOOR_TOOLTIP",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 1,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			break;
		}
		case "ActorSpawned":
		{
			CActor cActor2 = parameters[0] as CActor;
			if (cActor2 is CObjectActor || cActor2.Type == CActor.EType.Ally)
			{
				ShowDestroyPropObjective();
				ShowProtectObjective();
			}
			if (cActor2.Type == CActor.EType.Enemy2 && !HasShownTutorial("FH_Hint_Combat_EnemyFactions"))
			{
				List<CActor> actors = ((IEnumerable<CEnemyActor>)ScenarioManager.Scenario.Enemy2Monsters).Select((Func<CEnemyActor, CActor>)((CEnemyActor x) => x)).ToList();
				PushData(new ContextualData
				{
					Actors = actors,
					ID = "FH_Hint_Combat_EnemyFactions",
					Caption = "FH_Hint_Combat_Name_EnemyFactions",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 1,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (cActor2.IsBoss && !HasShownTutorial("FH_Hint_Combat_Bosses"))
			{
				List<CActor> actors2 = ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor x) => x.IsBoss).Select((Func<CEnemyActor, CActor>)((CEnemyActor x) => x)).ToList();
				PushData(new ContextualData
				{
					Actors = actors2,
					ID = "FH_Hint_Combat_Bosses",
					Caption = "GUI_CODEX_BOSSES",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 1,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (cActor2 is CHeroSummonActor)
			{
				PushData(new ContextualData
				{
					Actor = cActor2,
					ID = "FH_Hint_Combat_Summon",
					Caption = "FH_Hint_Combat_Name_Summon",
					FixedPosition = true,
					Group = "Tactical",
					Priority = 1,
					AdditionalValidation = () => IsValidDuringActiveScenario()
				});
			}
			if (cActor2 is CObjectActor { AttachedProp: { SpecificPropType: ESpecificPropType.WarBanner } })
			{
				PushData(new ContextualData
				{
					Actor = cActor2,
					ID = "RFH_Scenario_Campaign_006_SCENARIO_MODIFIER_1",
					FixedPosition = true,
					Priority = 10,
					BlockInput = true,
					AdditionalValidation = () => IsValidDuringActiveScenario(),
					OverrideFallbackPosition = "BottomMiddle"
				});
			}
			break;
		}
		case "ScenarioResultsShown":
		{
			ScenarioDebriefingModule debriefingModule = (Singleton<UIResultsManager>.InstanceFast as UINewAdventureResultsManager)?.DebriefingModule;
			if (debriefingModule != null)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { debriefingModule.OpenBattleStats.gameObject },
					ID = "FH_Hint_UI_Endscreen",
					Caption = "FH_Hint_UI_Name_Endscreen",
					FixedPosition = false,
					BlockInput = true,
					Group = "Tactical",
					AdditionalValidation = () => !debriefingModule.AreBattleStatsShown
				});
			}
			break;
		}
		case "LevelUpDebriefing":
		{
			GameObject item2 = parameters[0] as GameObject;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { item2 },
				ID = "FH_Hint_Hero_LevelUpOutpost",
				Caption = "FH_Hint_Hero_Name_LevelUpOutpost",
				FixedPosition = true,
				BlockInput = false,
				Group = "Tactical"
			});
			break;
		}
		case "LevelUp":
		{
			CharacterState character = parameters[0] as CharacterState;
			IEnumerable<UIPartySlot> source5 = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.Characters.PartyColumn.Slots();
			List<GameObject> uiElements = new List<GameObject>();
			UIPartySlot uIPartySlot = source5.FirstOrDefault((UIPartySlot x) => x.CurrentData.ClassID == character.ClassID.ID);
			if (uIPartySlot != null)
			{
				uiElements.Add(uIPartySlot.gameObject);
			}
			PushData(new ContextualData
			{
				UIObjects = uiElements,
				ID = "FH_Hint_Hero_Leveling",
				Caption = "FH_Hint_Hero_Name_Leveling",
				FixedPosition = false,
				AdditionalValidation = () =>
				{
					if (IsShowingHint("FH_Hint_Hero_Leveling"))
					{
						if (!(Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is OutpostVillageMapState))
						{
							return false;
						}
						if (uiElements.Any((GameObject x) => x.gameObject == null || !x.gameObject.activeInHierarchy))
						{
							return false;
						}
					}
					return true;
				},
				NonRemovingValidation = () =>
				{
					if (_totalData.Any((ContextualData x) => !x.ID.Equals("FH_Hint_Hero_Leveling", StringComparison.InvariantCultureIgnoreCase) && x.Priority > 5))
					{
						return false;
					}
					return IsValidDuringDowntime() ? true : false;
				},
				Priority = 5
			});
			break;
		}
		case "ScenarioCompleted":
		{
			List<CampaignEffect> source4 = parameters[0] as List<CampaignEffect>;
			if (source4.Any((CampaignEffect x) => x.Type == CampaignEffectType.UnlockMandatoryScenario))
			{
				ScenarioRewardsModule scenarioRewards = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.WorldMap.ScenarioRewards;
				List<CampaignEffect> list11 = source4.Where((CampaignEffect effect) => !OutpostEventsService.IsHiddenEffect(effect)).ToList();
				CampaignEffect item3 = list11.FirstOrDefault((CampaignEffect x) => x.Type == CampaignEffectType.UnlockMandatoryScenario);
				int num2 = list11.IndexOf(item3);
				if (scenarioRewards.Effects.Count > num2)
				{
					GameObject item4 = scenarioRewards.Effects[num2];
					PushData(new ContextualData
					{
						UIObjects = new List<GameObject> { item4 },
						ID = "FH_Hint_UI_MandatoryQuest",
						Caption = "FH_Hint_UI_Name_MandatoryQuest",
						FixedPosition = true
					});
				}
			}
			break;
		}
		case "QuestBriefingShown":
		{
			if (!(Singleton<FHMapChoreographer>.InstanceFast?.View.CurrentState is BattleGoalSelectionViewState))
			{
				break;
			}
			ScenarioBriefingPartyModule scenarioBriefingParty = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.Characters.ScenarioBriefingParty;
			List<GameObject> list2 = new List<GameObject>();
			List<GameObject> list3 = new List<GameObject>();
			foreach (UIScenarioBriefingPartySlot element in scenarioBriefingParty.Elements)
			{
				list2.Add(element.m_PartySlot.gameObject);
				if (element.m_PartySlot.CurrentData.Owned && element.m_GoalListContainer.Elements.Any())
				{
					list3.Add(element.m_GoalListContainer.gameObject);
				}
			}
			PushData(new ContextualData
			{
				UIObjects = list2,
				ID = "FH_Hint_UI_EditCharacter",
				Caption = "FH_Hint_UI_Name_EditCharacter",
				FixedPosition = false,
				BlockInput = true,
				Priority = 10
			});
			if (list3.Count > 0)
			{
				PushData(new ContextualData
				{
					UIObjects = list3,
					ID = "FH_Hint_UI_BattleGoals",
					Caption = "GUI_BATTLE_GOALS",
					FixedPosition = false,
					BlockInput = true,
					Priority = 9
				});
			}
			break;
		}
		case "OutpostEvent":
		{
			OutpostEventDef outpostEventDef = parameters[0] as OutpostEventDef;
			if (outpostEventDef.Type == OutpostEventType.Outpost)
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Events_Outpost",
					Caption = "FH_Hint_Events_Name_Outpost",
					FixedPosition = true,
					Priority = 1,
					AdditionalValidation = () => !IsValidWhileNoOutpostEvent()
				});
			}
			else if (outpostEventDef.Type == OutpostEventType.Road)
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Events_Road",
					Caption = "FH_Hint_Events_Name_Road",
					FixedPosition = true,
					Priority = 1,
					AdditionalValidation = () => !IsValidWhileNoOutpostEvent()
				});
			}
			else if (outpostEventDef.Type == OutpostEventType.Boat)
			{
				PushData(new ContextualData
				{
					ID = "FH_Hint_Events_Boat",
					Caption = "FH_Hint_Events_Name_Boat",
					FixedPosition = true,
					Priority = 1,
					AdditionalValidation = () => !IsValidWhileNoOutpostEvent()
				});
			}
			break;
		}
		case "LegacyPerks":
			PushData(new ContextualData
			{
				ID = "FH_Hint_Hero_LegacyPerks",
				Caption = "FH_Hint_Hero_Name_LegacyPerks",
				FixedPosition = true,
				AdditionalValidation = () => Singleton<FHMapChoreographer>.InstanceFast?.View?.CurrentState is NewCharacterViewState
			});
			break;
		case "Distribution":
		{
			DistributionModule distributionModule = parameters[0] as DistributionModule;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { (distributionModule.HighlightRect != null) ? distributionModule.HighlightRect.gameObject : distributionModule.gameObject },
				ID = "FH_Hint_UI_ResourceAllocation",
				Caption = "GUI_RESOURCE_ALLOCATION_CAPTION",
				FixedPosition = true,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent(),
				Priority = 1
			});
			break;
		}
		case "Resources":
		{
			MainHeaderModule mainHeaderModule = parameters[0] as MainHeaderModule;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject>
				{
					mainHeaderModule.CharacterResources.gameObject,
					mainHeaderModule.OutpostResources.gameObject
				},
				ID = "FH_Hint_UI_ResourceBar",
				Caption = "FH_Hint_UI_Name_ResourceBar",
				FixedPosition = false,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime(),
				Group = "OutpostHUD",
				BlockInput = true,
				Priority = 10
			});
			break;
		}
		case "Party":
		{
			PartyColumnModule partyColumnModule = parameters[0] as PartyColumnModule;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { partyColumnModule.gameObject },
				ID = "FH_Hint_UI_PartyColumn",
				Caption = "FH_Hint_UI_Name_PartyColumn",
				FixedPosition = false,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime(),
				Group = "OutpostHUD",
				BlockInput = true,
				Priority = 9
			});
			break;
		}
		case "Masteries":
		{
			CharacterGoalsModule masteriesModule = parameters[0] as CharacterGoalsModule;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { masteriesModule.ContainerMasteries.gameObject },
				ID = "FH_Hint_Hero_Mastery",
				Caption = "FH_Hint_Hero_Name_Mastery",
				FixedPosition = false,
				AdditionalValidation = () => !IsShowingHint("FH_Hint_Hero_Mastery") || (masteriesModule != null && masteriesModule.isActiveAndEnabled)
			});
			break;
		}
		case "PersonalQuest":
		{
			PersonalQuestSelectionModule personalQuestSelectionModule = parameters[0] as PersonalQuestSelectionModule;
			List<GameObject> list12 = new List<GameObject>();
			if (personalQuestSelectionModule.QuestLeft != null && personalQuestSelectionModule.QuestLeft.gameObject.activeSelf)
			{
				list12.Add(personalQuestSelectionModule.QuestLeft.gameObject);
			}
			if (personalQuestSelectionModule.QuestRight != null && personalQuestSelectionModule.QuestRight.gameObject.activeSelf)
			{
				list12.Add(personalQuestSelectionModule.QuestRight.gameObject);
			}
			PushData(new ContextualData
			{
				UIObjects = list12,
				ID = "FH_Hint_Hero_PersonalQuest",
				Caption = "GUI_CREATE_CHARACTER_PERSONAL_QUEST_TAB",
				FixedPosition = false,
				AdditionalValidation = () => !IsShowingHint("FH_Hint_Hero_PersonalQuest") || (personalQuestSelectionModule != null && personalQuestSelectionModule.isActiveAndEnabled),
				NonRemovingValidation = () => personalQuestSelectionModule != null && personalQuestSelectionModule.isActiveAndEnabled
			});
			break;
		}
		case "QuickAccess":
		{
			OutpostQuickAccessBarModule quickAccessModule = parameters[0] as OutpostQuickAccessBarModule;
			GameObject item5 = quickAccessModule?.HighlightRect.gameObject ?? quickAccessModule.gameObject;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { item5 },
				ID = "FH_Hint_UI_QuickAccessBar",
				Caption = "FH_Hint_UI_Name_QuickAccessBar",
				FixedPosition = false,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime() && quickAccessModule != null && quickAccessModule.GameObject != null && quickAccessModule.isActiveAndEnabled,
				Group = "OutpostHUD",
				BlockInput = true,
				Priority = 7
			});
			break;
		}
		case "BuildButton":
		{
			FHToggle fHToggle = parameters[0] as FHToggle;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { fHToggle.gameObject },
				ID = "FH_Hint_UI_BuildingBar",
				Caption = "FH_Hint_UI_Name_BuildingBar",
				FixedPosition = false,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime(),
				Group = "OutpostHUD",
				BlockInput = true,
				Priority = 8
			});
			break;
		}
		case "WreckedBuildings":
			PushData(new ContextualData
			{
				ID = "FH_Hint_Outpost_WreckedBuildings",
				Caption = "GUI_WreckedBuildings",
				FixedPosition = true,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime() && IsValidDuringOutpostView()
			});
			break;
		case "BuildingConstructionOptionsShown":
		{
			GameObject module = parameters[0] as GameObject;
			if ((bool)parameters[1])
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { module },
					ID = "FH_Hint_Outpost_Donations",
					Caption = "FH_Hint_Outpost_Name_Donations",
					FixedPosition = false,
					AdditionalValidation = () => IsValidDuringOutpostView() && module.activeInHierarchy
				});
			}
			break;
		}
		case "BuildButtonClicked":
			PushData(new ContextualData
			{
				ID = "FH_Hint_Outpost_FunctionalityBuilding",
				Caption = "GUI_CODEX_BUILDINGS",
				FixedPosition = true,
				AdditionalValidation = () => IsValidDuringOutpostView()
			});
			break;
		case "WorldMap":
		{
			FHButton fHButton = parameters[0] as FHButton;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { fHButton.gameObject },
				ID = "FH_Hint_UI_WorldMap",
				Caption = "GUI_WorldMap",
				FixedPosition = false,
				AdditionalValidation = () => IsValidWhileNoOutpostEvent() && IsValidWhileNotTravelling() && IsValidWhileNoDistribution() && IsValidDuringDowntime(),
				Group = "OutpostHUD",
				BlockInput = true,
				Priority = 6
			});
			break;
		}
		case "Outpost - Quests":
		{
			ScenarioListModule scenarioList = Singleton<FHMapChoreographer>.InstanceFast.View.Modules.WorldMap.ScenarioList;
			PushData(new ContextualData
			{
				UIObjects = new List<GameObject> { scenarioList.gameObject },
				ID = "FH_Hint_Outpost_Quests",
				Caption = "GUI_Footer_Quests",
				FixedPosition = true,
				AdditionalValidation = () => IsValidDuringScenarioSelectionView()
			});
			break;
		}
		case "BuildingSelected":
		{
			BuildingInfoModule buildingModule = parameters[0] as BuildingInfoModule;
			BuildingTypeDef buildingDef = ((BuildingInfoModule.BuildingInfoParams)parameters[1]).Building.Def;
			string hintID2 = null;
			string caption2 = $"BUILDING_Name_{buildingDef.ID.ID}";
			if (buildingDef is TownHall_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_TownHall";
			}
			else if (buildingDef is Stables_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_StablePets";
			}
			else if (buildingDef is Enhancer_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Enhancer";
			}
			else if (buildingDef is Garden_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Garden";
			}
			else if (buildingDef is TempleOfTheGreatOak_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Temple";
			}
			else if (buildingDef is HallOfRevelry_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Hall";
			}
			else if (buildingDef is Craftsman_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Craftsman";
			}
			else if (buildingDef is Alchemist_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Alchemist";
			}
			else if (buildingDef is Walls_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Wall";
			}
			else if (buildingDef is Workshop_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Workshop";
			}
			else if (buildingDef is LoggingCamp_BuildingTypeDef || buildingDef is MiningCamp_BuildingTypeDef || buildingDef is HuntingLodge_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_ResourceCamp";
				caption2 = "FH_Hint_Outpost_Name_ResourceCamp";
			}
			else if (buildingDef is LumberDepot_BuildingTypeDef || buildingDef is MetalDepot_BuildingTypeDef || buildingDef is HideDepot_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_ResourceDepo";
				caption2 = "FH_Hint_Outpost_Name_ResourceDepo";
			}
			else if (buildingDef is Barrack_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_BarracksDefense";
			}
			else if (buildingDef is Inn_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Inn";
			}
			else if (buildingDef is Tavern_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Tavern";
			}
			else if (buildingDef is Jeweler_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Jeweler";
			}
			else if (buildingDef is Library_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Library";
			}
			else if (buildingDef is Carpenter_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_Carpenter";
			}
			else if (buildingDef is TradingPost_BuildingTypeDef)
			{
				hintID2 = "FH_Hint_Outpost_TradingPost";
			}
			if (!string.IsNullOrEmpty(hintID2))
			{
				UITweenEffects tween = null;
				_ = Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState;
				List<GameObject> uIObjects3 = null;
				if (buildingModule?.Container != null)
				{
					uIObjects3 = new List<GameObject> { buildingModule.Container.gameObject };
					tween = buildingModule?.GetComponentInChildren<UITweenEffects>(includeInactive: true);
				}
				PushData(new ContextualData
				{
					UIObjects = uIObjects3,
					ID = hintID2,
					Caption = caption2,
					FixedPosition = false,
					AdditionalValidation = () => !IsShowingHint(hintID2) || (buildingModule.Container.gameObject.activeInHierarchy && buildingModule.data.Building.Def == buildingDef),
					NonRemovingValidation = () => tween == null || (!tween.IsAboutToPlay() && !tween.IsActive() && buildingModule.Container.gameObject.activeInHierarchy)
				});
			}
			break;
		}
		case "ViewStateChanged":
		{
			string hintID = null;
			FHMapViewState curState = parameters[0] as FHMapViewState;
			string caption = null;
			if (curState is ShopViewState && !curState.GameState.IsInPrologue)
			{
				hintID = "FH_Hint_Outpost_Merchant";
				caption = "GUI_MERCHANT";
			}
			else if (curState is CraftingViewState)
			{
				hintID = "FH_Hint_Outpost_Craftsman";
				caption = "BUILDING_Name_34";
			}
			else if (curState is AlchemistViewState)
			{
				hintID = "FH_Hint_Outpost_Alchemist";
				caption = "BUILDING_Name_35";
			}
			else if (curState is EditCharacterViewState)
			{
				hintID = "FH_Hint_UI_CharacterScreen";
				caption = "FH_Hint_UI_Name_CharacterScreen";
			}
			else if (curState is HandManagementViewState)
			{
				hintID = "FH_Hint_UI_HandManagement";
				caption = "GUI_Footer_HandManagement";
			}
			else if (curState is UnlockCardsViewState)
			{
				hintID = "FH_Hint_UI_NewCards";
				caption = "GUI_Footer_NewCards";
			}
			else if (curState is CharacterPerksViewState)
			{
				hintID = "FH_Hint_Hero_Perks";
				caption = "GUI_PERKS_INVENTORY_CAPTION";
			}
			else if (curState is ManageRosterViewState)
			{
				hintID = "FH_Hint_Hero_PartyManagement";
				caption = "FH_Hint_Hero_Name_PartyManagement";
			}
			else if (curState is TownPerksViewState)
			{
				hintID = "FH_Hint_Outpost_TownHallTownGuard";
				caption = "FH_Hint_Outpost_Name_TownHallTownGuard";
			}
			if (!string.IsNullOrEmpty(hintID))
			{
				PushData(new ContextualData
				{
					ID = hintID,
					Caption = caption,
					FixedPosition = true,
					AdditionalValidation = () => !IsShowingHint(hintID) || Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState == curState
				});
			}
			if (curState is ShopViewState || curState is SpendStartingGoldViewState)
			{
				string itemHintKey = "FH_Hint_Items_General";
				PushData(new ContextualData
				{
					ID = itemHintKey,
					Caption = "GUI_OPT_COMBAT_LOG_ITEMS",
					FixedPosition = true,
					AdditionalValidation = () => !IsShowingHint(itemHintKey) || Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState == curState,
					Priority = 9
				});
			}
			if (curState is AlchemistViewState && curState.GameController.HasActiveEffect(CampaignEffectType.CanBrewAndDistill))
			{
				string distillHintID = "FH_Hint_Outpost_Alchemist_Destill";
				PushData(new ContextualData
				{
					ID = distillHintID,
					Caption = "FH_Hint_Outpost_Name_Alchemist_Destill",
					FixedPosition = true,
					AdditionalValidation = () => !IsShowingHint(distillHintID) || Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState == curState,
					Priority = 9
				});
			}
			break;
		}
		case "ActivatedCampaignSection":
			switch (parameters[0] as string)
			{
			case "126.1":
			case "155.3":
			case "175.2":
				PushData(new ContextualData
				{
					ID = "FH_Hint_MoraleSection",
					Caption = "Event_155.3_Name",
					FixedPosition = true,
					BlockInput = true,
					Priority = 1
				});
				break;
			}
			break;
		case "ProsperityLevelChanged":
		{
			uint num = (uint)parameters[0];
			GameObject item = parameters[2] as GameObject;
			if (num >= 2)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { item },
					ID = "FH_Hint_Outpost_Town_Level2",
					Caption = "FH_Hint_Starting_Hero_Perks"
				});
			}
			if (num >= 3)
			{
				PushData(new ContextualData
				{
					UIObjects = new List<GameObject> { item },
					ID = "FH_Hint_Outpost_Town_Level3",
					Caption = "FH_Hint_Starting_Hero_Level"
				});
			}
			break;
		}
		}
		RefreshNavigator();
	}

	private void LateUpdate()
	{
		TryProcessPendingData();
	}

	private bool IsDataValid(IGeneralInfoData data)
	{
		bool result = true;
		if (data is ContextualData contextualData)
		{
			result = contextualData.Validate();
		}
		return result;
	}

	private bool IsDataValidToShow(IGeneralInfoData data)
	{
		bool result = true;
		if (data is ContextualData contextualData)
		{
			result = contextualData.NonRemovingValidation == null || contextualData.NonRemovingValidation();
		}
		return result;
	}

	private void TryProcessPendingData()
	{
		if (!CanShowRightNow())
		{
			return;
		}
		for (int num = _totalData.Count - 1; num >= 0; num--)
		{
			ContextualData contextualData = _totalData[num];
			ContextualData contextualData2 = default;
			if (CurrentModule is UIContextualTutorialHighlightAndFocusCamera uIContextualTutorialHighlightAndFocusCamera)
			{
				contextualData2 = uIContextualTutorialHighlightAndFocusCamera.Data;
			}
			if (!IsDataValid(contextualData))
			{
				RemoveData(contextualData);
				if (CurrentModule != null)
				{
					RefreshNavigator();
				}
			}
			else if (CurrentModule != null && _currentlyShownData.Count > 0 && !_currentlyShownData.Contains(contextualData) && contextualData.Group != null && contextualData.Group == contextualData2.Group && IsDataValidToShow(contextualData))
			{
				AddDataAsShown(contextualData);
				RefreshNavigator();
			}
		}
		if (_currentlyShownData.Count == 0 && _totalData.Count > 0)
		{
			List<ContextualData> list = _totalData.ToList();
			list.RemoveAll((ContextualData x) => !IsDataValidToShow(x));
			if (list.Count > 0)
			{
				ContextualData mostImportantTut = list.OrderByDescending((ContextualData x) => x.Priority).First();
				foreach (ContextualData item in from x in list
					where x.ID.Equals(mostImportantTut.ID) || (x.Group != null && x.Group == mostImportantTut.Group)
					orderby x.Priority descending, x.OrderAdded
					select x)
				{
					AddDataAsShown(item);
				}
				OnShow_Internal(_currentlyShownData[0]);
			}
		}
		else if (_currentlyShownData.Count > 0 && (CurrentModule == null || (CurrentModule is UIContextualTutorialBase uIContextualTutorialBase && !uIContextualTutorialBase.Data.ID.Equals(_currentlyShownData[0].ID))))
		{
			OnShow_Internal(_currentlyShownData[0]);
		}
		if (CurrentModule != null && _currentlyShownData.Count == 0)
		{
			CurrentModule?.Hide();
			_currentModule = null;
		}
	}

	public void Close()
	{
		if (_currentlyShownData.Count == 0)
		{
			if (!CanShowRightNow() && CurrentModule != null)
			{
				CurrentModule?.Hide();
				_currentModule = null;
			}
			return;
		}
		ContextualData data = _currentlyShownData[0];
		RemoveData(data);
		if (!AreTutorialsEnabled)
		{
			_totalData.Clear();
			_currentlyShownData.Clear();
		}
		TryProcessPendingData();
	}

	private void RemoveData(ContextualData data)
	{
		_currentlyShownData.Remove(data);
		_totalData.Remove(data);
	}

	private void AddDataAsShown(ContextualData data)
	{
		_currentlyShownData.Add(data);
		GlobalStatRepository.Instance.ShownContextualTutorials.NewValue(GetDataKey(data));
	}

	private static string ConditionIdToGlossaryKey(string conditionId)
	{
		return conditionId switch
		{
			"Retaliate" => "FH_Hint_Condition_Retaliate", 
			"Blessed" => "Glossary_Bless", 
			"Cursed" => "Glossary_Curse", 
			"Disarmed" => "Glossary_Disarm", 
			"Immobilized" => "Glossary_Immobilize", 
			"Invisible" => "Glossary_Invisible", 
			"Muddled" => "Glossary_Muddle", 
			"Pierce" => "Glossary_Pierce", 
			"Poisoned" => "Glossary_Poison", 
			"Strengthened" => "Glossary_Strengthen", 
			"Stunned" => "Glossary_Stun", 
			"Wounded" => "Glossary_Wound", 
			"Advantage" => "Glossary_Advantage", 
			"Disadvantage" => "Glossary_Disadvantage", 
			"Baned" => "Glossary_Bane", 
			"Regenerate" => "Glossary_Regenerate", 
			"Ward" => "Glossary_Ward", 
			"Brittle" => "Glossary_Brittle", 
			"Impaired" => "Glossary_Impair", 
			_ => string.Empty, 
		};
	}

	private static string ConditionIdToCaptionKey(string conditionId)
	{
		return conditionId switch
		{
			"Retaliate" => "Retaliate", 
			"Blessed" => "ENHANCEMENT_Bless", 
			"Cursed" => "ENHANCEMENT_Curse", 
			"Disarmed" => "ENHANCEMENT_Disarm", 
			"Immobilized" => "ENHANCEMENT_Immobilize", 
			"Invisible" => "FH_Hint_Conditions_Name_Invisible", 
			"Muddled" => "ENHANCEMENT_Muddle", 
			"Pierce" => "FH_Hint_Conditions_Name_Pierce", 
			"Poisoned" => "ENHANCEMENT_Poison", 
			"Strengthened" => "ENHANCEMENT_Strengthen", 
			"Stunned" => "FH_Hint_Conditions_Name_Stun", 
			"Wounded" => "ENHANCEMENT_Wound", 
			"Advantage" => "Advantage", 
			"Disadvantage" => "Disadvantage", 
			"Baned" => "FH_Hint_Conditions_Name_Bane", 
			"Regenerate" => "FH_Hint_Conditions_Name_Regenerate", 
			"Ward" => "FH_Hint_Conditions_Name_Ward", 
			"Brittle" => "FH_Hint_Conditions_Name_Brittle", 
			"Impaired" => "FH_Hint_Conditions_Name_Impair", 
			_ => string.Empty, 
		};
	}
}

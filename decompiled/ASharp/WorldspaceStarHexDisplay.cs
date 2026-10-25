using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AStar;
using Chronos;
using DevLocker.GFrame.Input;
using EPOOutline;
using FH.ApparanceBaking;
using FHGUI;
using FHGUI.Common;
using FHGUI.Scenario.Services;
using GLOOM;
using Libs.GameConsole;
using Libs.Threading;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

public class WorldspaceStarHexDisplay : MonoBehaviour
{
	private class TileTemplate
	{
		public IHexControl.HexType m_Type;

		public IHexControl.HexMode m_Mode;

		public bool m_Hover;

		public TileTemplate(IHexControl.HexType type, IHexControl.HexMode mode, bool hover)
		{
			m_Type = type;
			m_Mode = mode;
			m_Hover = hover;
		}
	}

	public enum EAbilityDisplayType
	{
		None,
		Normal,
		AreaOfEffect,
		EnemyAreaOfEffect,
		SelectObjectPosition,
		SelectObjectPositionNegative,
		TargetingAbility,
		NegativeAbility,
		SelectPath,
		SelectObjectPositionAreaOfEffect,
		RedistributeDamageAbility,
		ObjectiveAbility
	}

	public enum EMoveDisplayType
	{
		None,
		Normal,
		Pull,
		Push,
		Teleport2
	}

	public enum WorldSpaceStarDisplayState
	{
		ShowNone,
		CharacterPlacement,
		MovementSelection,
		TargetSelection,
		LongResting,
		LevelEditorSpawning
	}

	private class TileLOSResult
	{
		public CTile Tile;

		public CTile CasterTile;

		public bool HasLOS;

		public static void Job(TileLOSResult a)
		{
			a.HasLOS = CActor.HaveLOS(a.CasterTile, a.Tile);
		}
	}

	[ConsoleVariable(Alias = "show_path_cost", Persistent = false)]
	public static bool ConsoleVar_ShowPathCost = false;

	public static List<Tuple<Point, int>> s_DebugPathPoints = new List<Tuple<Point, int>>();

	public static WorldspaceStarHexDisplay Instance;

	private readonly Dictionary<CClientTile, IHexControl> m_PossibleMoveStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_PossibleMoveStarsRemainingInDirection = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_PathMoveStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_PossibleAttackPathStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_PathAttackStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_AttackStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_AbilityStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_PlacementStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_DungeonExitStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_GenericStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_ImpossibleMoveChestDoorStars = new Dictionary<CClientTile, IHexControl>();

	private readonly Dictionary<CClientTile, IHexControl> m_HighlightAllStars = new Dictionary<CClientTile, IHexControl>();

	private CClientTile m_SelectedPlacementStars;

	private CClientTile m_CursorHighlightedTile;

	private IHexControl m_CursorHighlightedStar;

	private List<Coroutine> SynchronizedCoroutines = new List<Coroutine>();

	private readonly Dictionary<CClientTile, Dictionary<CClientTile, IHexControl>> m_CurrentlyActiveStars = new Dictionary<CClientTile, Dictionary<CClientTile, IHexControl>>();

	private readonly List<CClientTile> m_TilesExpectedToDisplay = new List<CClientTile>();

	private readonly List<CClientTile> m_CachedMoveStars = new List<CClientTile>();

	private readonly List<CClientTile> m_CachedPushStars = new List<CClientTile>();

	private readonly List<CClientTile> m_CachedPullStars = new List<CClientTile>();

	private readonly List<CClientTile> m_CachedTeleport2Stars = new List<CClientTile>();

	private readonly List<CClientTile> m_CachedPullStarsTotal = new List<CClientTile>();

	[ConsoleVariable(Alias = "dont_highlight_empty_tiles_when_selecting_actors")]
	public bool s_DontHighlightEmptyTilesWhenSelectingActors;

	private int m_AreaEffectAngle;

	private int m_LastAreaEffectAngle;

	private bool m_AreaEffectFlipped;

	private float m_LastRecievedRotateDirection;

	private bool m_TurningRight;

	private CInteractable m_LastPointedAtInteractable;

	private Vector3 m_LastPointedAtGroundPosition;

	private bool m_NewTilePointedAtThisFrame;

	private bool m_LastPointedOverUI;

	private readonly List<CClientTile> m_CachedAbilityRangeTiles = new List<CClientTile>();

	private readonly List<CClientTile> m_ChestTiles = new List<CClientTile>();

	private readonly List<CClientTile> m_DoorTiles = new List<CClientTile>();

	private List<CTile> m_LeftLineTiles = new List<CTile>();

	private List<CTile> m_RightLineTiles = new List<CTile>();

	private List<CTile> m_TopLeftLineTiles = new List<CTile>();

	private List<CTile> m_TopRightLineTiles = new List<CTile>();

	private List<CTile> m_BottomLeftLineTiles = new List<CTile>();

	private List<CTile> m_BottomRightLineTiles = new List<CTile>();

	private readonly List<CTile> m_AllLineTiles = new List<CTile>();

	private bool m_EndingMovement;

	private bool m_OpeningDoor;

	private bool m_SelectingADoor;

	private bool m_AOELocked;

	private CAbility m_SavedAbility;

	private ScenarioManager.ObjectImportType m_ObjectSpawnType = ScenarioManager.ObjectImportType.None;

	private List<CAbilityFilter.EFilterTile> m_TileFilters;

	private CShowRedistributeDamageUI_MessageData m_SavedRedistributeDamageUIMessage;

	private DistributeDamageService m_SavedDistributeDamageService;

	private bool m_RefreshedCurrentState;

	private bool m_AllHexesHighlighted;

	private bool m_FoundAllChestAndDoorTiles;

	private readonly List<ActorBehaviour> m_EnemiesToRetaliate = new List<ActorBehaviour>();

	private GameObject m_TargetTooltip;

	private Action m_OnRemoveTargetTooltip;

	private readonly TileTemplate m_MovePossiblityTemplate = new TileTemplate(IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false);

	private readonly TileTemplate m_MovePossiblityTemplateAllHexes = new TileTemplate(IHexControl.HexType.Move, IHexControl.HexMode.ReachAllHexes, hover: false);

	private readonly TileTemplate m_MoveHazardPossibilityTemplate = new TileTemplate(IHexControl.HexType.NegativeEffect, IHexControl.HexMode.PossibleTarget, hover: false);

	private readonly TileTemplate m_MoveChestDoorPossibilityTemplate = new TileTemplate(IHexControl.HexType.PositiveEffect, IHexControl.HexMode.PossibleTarget, hover: false);

	private readonly TileTemplate m_DifficultTerrainPossibilityTemplate = new TileTemplate(IHexControl.HexType.DifficultTerrain, IHexControl.HexMode.PossibleTarget, hover: false);

	private readonly TileTemplate m_IceTerrainPossibilityTemplate = new TileTemplate(IHexControl.HexType.IcyTerrain, IHexControl.HexMode.PossibleTarget, hover: false);

	private readonly TileTemplate m_ElevatedTerrainPossibilityTemplate = new TileTemplate(IHexControl.HexType.ElevatedTerrain, IHexControl.HexMode.PossibleTarget, hover: false);

	public EMoveDisplayType CurrentMoveDisplayType;

	private WorldSpaceStarDisplayState m_currentDisplayState;

	private WorldSpaceStarDisplayState m_savedDisplayState;

	private bool m_HexDisplayToggledOff;

	public ScenarioManager.EAdjacentPosition CurrentLineDirection;

	private List<GameObject> previewedCharacterPlacements = new List<GameObject>();

	private List<GameObject> _links = new List<GameObject>();

	public static object DisplayLock = new object();

	private List<TileLOSResult> _finalTiles = new List<TileLOSResult>();

	public static CAbility CurrentAbility => Instance?.m_SavedAbility;

	public CAbility SavedAbility => m_SavedAbility;

	public int AreaEffectAngle => m_AreaEffectAngle;

	public bool AreaEffectFlipped => m_AreaEffectFlipped;

	public bool LockView { get; set; }

	public EAbilityDisplayType CurrentAbilityDisplayType { get; private set; }

	public WorldSpaceStarDisplayState CurrentDisplayState
	{
		get
		{
			return m_currentDisplayState;
		}
		set
		{
			if (m_currentDisplayState != value && (m_SavedAbility?.Vars?.Any(((string, TValue) x) => x.Item1.StartsWith("ForcePreviewTiles")) != true || m_currentDisplayState != WorldSpaceStarDisplayState.TargetSelection || value != WorldSpaceStarDisplayState.ShowNone))
			{
				if (value == WorldSpaceStarDisplayState.ShowNone)
				{
					m_CachedAbilityRangeTiles?.Clear();
				}
				m_currentDisplayState = value;
				m_savedDisplayState = value;
				OnStarDisplayStateChanged();
			}
		}
	}

	private bool IsPushPossible { get; set; }

	private bool IsPullPossible { get; set; }

	private bool IsTeleportPossible { get; set; }

	public IEnumerable<IHexControl> GetAllActiveStars()
	{
		return m_CurrentlyActiveStars.Select((KeyValuePair<CClientTile, Dictionary<CClientTile, IHexControl>> entry) => entry.Value).SelectMany((Dictionary<CClientTile, IHexControl> entryValue) => entryValue.Values).Distinct();
	}

	public IEnumerable<(CClientTile, IHexControl)> GetAllActiveStarsAndTiles()
	{
		return m_CurrentlyActiveStars.Select((KeyValuePair<CClientTile, Dictionary<CClientTile, IHexControl>> entry) => entry.Value).SelectMany((Dictionary<CClientTile, IHexControl> entryValue) => entryValue.Select((KeyValuePair<CClientTile, IHexControl> x) => (Key: x.Key, Value: x.Value))).Distinct();
	}

	private void Awake()
	{
		Instance = this;
		m_currentDisplayState = WorldSpaceStarDisplayState.ShowNone;
		m_CachedAbilityRangeTiles.Clear();
		m_CursorHighlightedStar = null;
	}

	private void OnDestroy()
	{
		Instance = null;
		if ((bool)Singleton<InputFHManager>.InstanceFast)
		{
			InputFHManager.Controls.DisableAll(this);
		}
		m_PossibleMoveStars.Clear();
		m_PossibleMoveStarsRemainingInDirection.Clear();
		m_PathMoveStars.Clear();
		m_PossibleAttackPathStars.Clear();
		m_PathAttackStars.Clear();
		m_AttackStars.Clear();
		m_AbilityStars.Clear();
		m_PlacementStars.Clear();
		m_DungeonExitStars.Clear();
		m_GenericStars.Clear();
		m_ImpossibleMoveChestDoorStars.Clear();
		m_SelectedPlacementStars = null;
		m_CursorHighlightedTile = null;
		m_CursorHighlightedStar = null;
		m_HighlightAllStars.Clear();
		m_CurrentlyActiveStars.Clear();
		m_TilesExpectedToDisplay.Clear();
		m_CachedMoveStars.Clear();
		m_CachedPushStars.Clear();
		m_CachedPullStars.Clear();
		m_CachedTeleport2Stars.Clear();
		m_LastPointedAtInteractable = null;
		m_LastPointedAtGroundPosition = Vector3.zero;
		m_CachedAbilityRangeTiles.Clear();
		m_DoorTiles.Clear();
		m_ChestTiles.Clear();
		m_LeftLineTiles.Clear();
		m_RightLineTiles.Clear();
		m_TopLeftLineTiles.Clear();
		m_TopRightLineTiles.Clear();
		m_BottomLeftLineTiles.Clear();
		m_BottomRightLineTiles.Clear();
		m_AllLineTiles.Clear();
		m_EnemiesToRetaliate.Clear();
	}

	public void ToggleHexDisplay(bool enabled)
	{
		if (enabled)
		{
			m_currentDisplayState = m_savedDisplayState;
			m_savedDisplayState = WorldSpaceStarDisplayState.ShowNone;
			m_HexDisplayToggledOff = false;
		}
		else
		{
			m_savedDisplayState = CurrentDisplayState;
			m_currentDisplayState = WorldSpaceStarDisplayState.ShowNone;
			m_HexDisplayToggledOff = true;
			if (m_CursorHighlightedStar != null)
			{
				ObjectPool.Recycle(m_CursorHighlightedStar.GameObject, GlobalSettings.Instance.m_GenericHexStar);
			}
			m_CursorHighlightedStar = null;
		}
		OnStarDisplayStateChanged();
	}

	private void OnStarDisplayStateChanged()
	{
		UpdateRetaliateDamage(0, forceUpdate: true);
		SetInitialState();
		RefreshCurrentState();
	}

	public void SetDisplayAbility(CAbility displayAbility, EAbilityDisplayType attackType = EAbilityDisplayType.None, ScenarioManager.ObjectImportType objectSpawnType = ScenarioManager.ObjectImportType.None, List<CAbilityFilter.EFilterTile> tileFilters = null)
	{
		HidePreviewedCharacterPlacements();
		if (displayAbility != null && displayAbility != m_SavedAbility && attackType == CurrentAbilityDisplayType)
		{
			ClearStars();
		}
		m_SavedAbility = displayAbility;
		CurrentAbilityDisplayType = attackType;
		m_ObjectSpawnType = objectSpawnType;
		m_TileFilters = tileFilters?.ToList();
		m_CachedAbilityRangeTiles.Clear();
		EAbilityDisplayType currentAbilityDisplayType = CurrentAbilityDisplayType;
		if (currentAbilityDisplayType == EAbilityDisplayType.AreaOfEffect || currentAbilityDisplayType == EAbilityDisplayType.SelectObjectPositionAreaOfEffect)
		{
			InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft);
			InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight);
		}
		else
		{
			InputFHManager.Controls.Disable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft);
			InputFHManager.Controls.Disable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight);
		}
	}

	private void SetInitialState()
	{
		if (Choreographer.s_Choreographer != null && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && m_currentDisplayState == WorldSpaceStarDisplayState.TargetSelection && CurrentAbilityDisplayType == EAbilityDisplayType.RedistributeDamageAbility)
		{
			SetDisplayRedistributeDamageInitialState();
		}
	}

	public void RefreshCurrentState()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForTileSelected)
			{
				return;
			}
			RefreshPreviewedCharacterPlacements();
			switch (m_currentDisplayState)
			{
			case WorldSpaceStarDisplayState.CharacterPlacement:
				ShowPossiblePlacementHexes();
				break;
			case WorldSpaceStarDisplayState.MovementSelection:
				ShowPossibleMovementSelectionHexes();
				break;
			case WorldSpaceStarDisplayState.TargetSelection:
				if (CurrentAbilityDisplayType == EAbilityDisplayType.SelectPath)
				{
					ShowPossibleAttackPathStars();
				}
				break;
			case WorldSpaceStarDisplayState.LongResting:
				ShowLongRestHex();
				break;
			case WorldSpaceStarDisplayState.ShowNone:
				if (ControllerInputPointer.CursorType > ECursorType.Default)
				{
					ControllerInputPointer.CursorType = ECursorType.Default;
				}
				StopSynchronizedCoroutines();
				ClearStars();
				break;
			}
			ShowPossibleExitHexes();
			ShowGenericHexes();
			m_RefreshedCurrentState = true;
		}
	}

	private void RefreshPreviewedCharacterPlacements()
	{
		if (m_currentDisplayState != WorldSpaceStarDisplayState.CharacterPlacement)
		{
			HidePreviewedCharacterPlacements();
		}
	}

	private void HidePreviewedCharacterPlacements()
	{
		foreach (GameObject previewedCharacterPlacement in previewedCharacterPlacements)
		{
			if (previewedCharacterPlacement != null)
			{
				MF.EnableRenderers(previewedCharacterPlacement, enable: false);
			}
		}
		previewedCharacterPlacements.Clear();
	}

	public string GetHighlightType(CClientTile tile)
	{
		if (tile == null || !m_CurrentlyActiveStars.TryGetValue(tile, out var value))
		{
			return "none";
		}
		if (!value.TryGetValue(tile, out var value2))
		{
			return "unknown";
		}
		return $"{value2.Type} / {value2.Mode}";
	}

	public void Update()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!(Choreographer.s_Choreographer != null) || m_HexDisplayToggledOff)
			{
				return;
			}
			if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
			{
				m_NewTilePointedAtThisFrame = PointingAtANewTile();
				if (m_NewTilePointedAtThisFrame || m_RefreshedCurrentState)
				{
					if (m_currentDisplayState == WorldSpaceStarDisplayState.LevelEditorSpawning)
					{
						ListenForTargetingInputEvents();
						DisplayLevelEditorObstaclePlacementStars();
					}
					ShowLevelEditorStartingTiles();
				}
			}
			else
			{
				if (SaveData.Instance.Global.CurrentGameState != EGameState.Scenario || TimeManager.IsPaused || Choreographer.s_Choreographer.m_WaitState.m_State == Choreographer.ChoreographerStateType.WaitingForTileSelected)
				{
					return;
				}
				m_NewTilePointedAtThisFrame = PointingAtANewTile();
				if (!m_AllHexesHighlighted && !LockView)
				{
					if (m_NewTilePointedAtThisFrame || m_RefreshedCurrentState)
					{
						ClearTargetTooltip();
						switch (m_currentDisplayState)
						{
						case WorldSpaceStarDisplayState.CharacterPlacement:
							HighlightSelectedPlacementHex();
							break;
						case WorldSpaceStarDisplayState.MovementSelection:
							ShowMovePathStars();
							break;
						case WorldSpaceStarDisplayState.TargetSelection:
							if (Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
							{
								if (ListenForTargetingInputEvents())
								{
									switch (CurrentAbilityDisplayType)
									{
									case EAbilityDisplayType.AreaOfEffect:
										DisplayAOEStars();
										break;
									case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
										DisplaySelectObjectPositionAOEStars();
										break;
									}
								}
								else
								{
									ShowPossibleTargetSelectionHexes();
								}
							}
							else
							{
								ShowPossibleTargetSelectionHexes(null, forceUseOriginTile: true);
							}
							break;
						}
						m_RefreshedCurrentState = false;
					}
					else if (m_currentDisplayState == WorldSpaceStarDisplayState.TargetSelection && Choreographer.s_Choreographer.ThisPlayerHasTurnControl && ListenForTargetingInputEvents())
					{
						switch (CurrentAbilityDisplayType)
						{
						case EAbilityDisplayType.AreaOfEffect:
							DisplayAOEStars();
							break;
						case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
							DisplaySelectObjectPositionAOEStars();
							break;
						}
					}
				}
				ControlPlayerEndMovement();
				DisplayCursorHoverStar();
			}
		}
	}

	public void ShowPossiblePlacementHexes()
	{
		ClearStars();
		ControllerInputPointer.CursorType = ((InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			if (!m_PlacementStars.ContainsKey(allPossibleStartingTile) && allPossibleStartingTile.m_Tile.m_HexMap != null && allPossibleStartingTile.m_Tile.m_HexMap.Revealed)
			{
				if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
				{
					ModifyStar(allPossibleStartingTile, IHexControl.HexType.InvalidPlacement, IHexControl.HexMode.Selected, hover: false, m_PlacementStars);
				}
				else
				{
					ModifyStar(allPossibleStartingTile, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false, m_PlacementStars);
				}
			}
		}
		SetHexBorders(m_PlacementStars);
	}

	public void ShowPossibleExitHexes()
	{
		ClearStars(possibleMove: false, pathMove: false, attack: false, attackPath: false, possibleAttackPath: false, ability: false, placement: false, placementSelection: false, allHighlightedTiles: false, impossibleMoveChestDoor: false, possibleMoveDirection: false, cursorHighlightedTiles: false, exitDungeon: true);
		foreach (CClientTile tile in ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Concat(ClientScenarioManager.s_ClientScenarioManager.ReachHexTiles))
		{
			bool flag = false;
			foreach (CObjective winObjective in ScenarioManager.CurrentScenarioState.WinObjectives)
			{
				if (!winObjective.IsActive)
				{
					continue;
				}
				if (winObjective is CObjective_ActorReachPosition { ActorTargetPositions: var actorTargetPositions })
				{
					if (actorTargetPositions.SingleOrDefault((TileIndex t) => t.X == tile.m_Tile.m_ArrayIndex.X && t.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
					{
						flag = true;
						break;
					}
				}
				else if (winObjective is CObjective_ActorsEscaped { EscapePositions: var escapePositions } && escapePositions.SingleOrDefault((TileIndex t) => t.X == tile.m_Tile.m_ArrayIndex.X && t.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
				{
					flag = true;
					break;
				}
			}
			if (flag && !m_CurrentlyActiveStars.ContainsKey(tile) && !m_DungeonExitStars.ContainsKey(tile) && tile.m_Tile.m_HexMap != null)
			{
				bool flag2 = (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_HexMap.Revealed) || (tile.m_Tile.m_Hex2Map != null && tile.m_Tile.m_Hex2Map.Revealed);
				ModifyStar(tile, flag2 ? IHexControl.HexType.ExitDungeonRevealed : IHexControl.HexType.ExitDungeonUnrevealed, IHexControl.HexMode.Reach, hover: false, m_DungeonExitStars);
			}
		}
		SetHexBorders(m_DungeonExitStars);
	}

	public void ShowGenericHexes()
	{
		ClearStars(possibleMove: false, pathMove: false, attack: false, attackPath: false, possibleAttackPath: false, ability: false, placement: false, placementSelection: false, allHighlightedTiles: false, impossibleMoveChestDoor: false, possibleMoveDirection: false, cursorHighlightedTiles: false, exitDungeon: false, genericTiles: true);
		foreach (IGrouping<Color?, Tuple<CClientTile, Color?>> item3 in from x in ClientScenarioManager.s_ClientScenarioManager.GenericHighlightedTiles
			group x by x.Item2)
		{
			Dictionary<CClientTile, IHexControl> dictionary = new Dictionary<CClientTile, IHexControl>();
			foreach (Tuple<CClientTile, Color?> item4 in item3)
			{
				CClientTile item = item4.Item1;
				if (item != null && !m_CurrentlyActiveStars.ContainsKey(item) && !m_GenericStars.ContainsKey(item) && item.m_Tile.m_HexMap != null)
				{
					bool flag = (item.m_Tile.m_HexMap != null && item.m_Tile.m_HexMap.Revealed) || (item.m_Tile.m_Hex2Map != null && item.m_Tile.m_Hex2Map.Revealed);
					Color? item2 = item4.Item2;
					ModifyStar(item, flag ? IHexControl.HexType.ExitDungeonRevealed : IHexControl.HexType.ExitDungeonUnrevealed, IHexControl.HexMode.Reach, hover: false, m_GenericStars, item2);
					dictionary[item] = m_GenericStars[item];
				}
			}
			SetHexBorders(dictionary);
			foreach (KeyValuePair<CClientTile, IHexControl> item5 in dictionary)
			{
				item5.Value.HexUpdate();
			}
		}
	}

	public void ShowGenericLinks()
	{
		foreach (GameObject link in _links)
		{
			ObjectPool.Recycle(link, GlobalSettings.Instance.m_NavLinkLineObject);
		}
		_links.Clear();
		foreach (var genericHighlightedLink in ClientScenarioManager.s_ClientScenarioManager.GenericHighlightedLinks)
		{
			CClientTile item = genericHighlightedLink.Item1;
			CClientTile item2 = genericHighlightedLink.Item2;
			GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_NavLinkLineObject, item.m_GameObject.transform);
			gameObject.GetComponent<ParableTesselator>().Tesselate(item.m_GameObject.transform.position, item2.m_GameObject.transform.position);
			_links.Add(gameObject);
		}
	}

	public void HighlightSelectedPlacementHex()
	{
		Waypoint.s_PlacementTile = null;
		ClearStars(possibleMove: true, pathMove: true, attack: true, attackPath: true, possibleAttackPath: true, ability: true, placement: false);
		if (InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile == null)
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
		}
		if (Interactable() == null)
		{
			HidePreviewedCharacterPlacements();
			return;
		}
		TileBehaviour component = Interactable().GetComponent<TileBehaviour>();
		if (component == null || component.m_ClientTile == null)
		{
			HidePreviewedCharacterPlacements();
			return;
		}
		CClientTile clientTile = component.m_ClientTile;
		foreach (CClientTile allPossibleStartingTile in ClientScenarioManager.s_ClientScenarioManager.AllPossibleStartingTiles)
		{
			if (allPossibleStartingTile != clientTile && allPossibleStartingTile.m_Tile.m_HexMap != null && allPossibleStartingTile.m_Tile.m_HexMap.Revealed)
			{
				if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
				{
					ModifyStar(allPossibleStartingTile, IHexControl.HexType.InvalidPlacement, IHexControl.HexMode.Reach, hover: false, m_PlacementStars);
				}
				else
				{
					ModifyStar(allPossibleStartingTile, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false, m_PlacementStars);
				}
				m_PlacementStars[allPossibleStartingTile].RefreshHexUI();
			}
		}
		SetHexBorders(m_PlacementStars);
		if (!m_PlacementStars.ContainsKey(clientTile))
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
			HidePreviewedCharacterPlacements();
			return;
		}
		if (ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex) != null)
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
			HidePreviewedCharacterPlacements();
			return;
		}
		if (m_PlacementStars.ContainsKey(clientTile))
		{
			CActor cActor = InitiativeTrack.Instance.SelectedActor()?.Actor;
			if (cActor is CPlayerActor && cActor.IsUnderMyControl())
			{
				int bodyIndex = -1;
				CPlayerActor cPlayerActor = cActor as CPlayerActor;
				if (cPlayerActor.PendingForm > CPlayerActor.EForm.Choose)
				{
					bodyIndex = ((cPlayerActor.PendingForm != CPlayerActor.EForm.Melee) ? 1 : 0);
				}
				PreviewPlacement(cActor.Class.ID, clientTile, Choreographer.s_Choreographer.FindClientPlayer(cActor).transform.rotation, bodyIndex);
			}
			else
			{
				HidePreviewedCharacterPlacements();
			}
			ControllerInputPointer.CursorType = ECursorType.Targeted;
			m_PlacementStars[clientTile].Hover = true;
			m_SelectedPlacementStars = clientTile;
			bool flag = false;
			if (Choreographer.s_Choreographer.WillMovingPlayerToTileMakeStartingPositionsValid(clientTile))
			{
				flag = true;
			}
			if (flag)
			{
				ModifyStar(clientTile, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: true, m_PlacementStars);
			}
			else
			{
				ModifyStar(clientTile, IHexControl.HexType.InvalidPlacement, IHexControl.HexMode.Reach, hover: true, m_PlacementStars);
			}
			m_PlacementStars[clientTile].RefreshHexUI();
		}
		else
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
		}
		Waypoint.s_PlacementTile = clientTile;
	}

	public void ShowLongRestHex()
	{
		ClearStars();
		if (Choreographer.s_Choreographer.m_CurrentActor != null)
		{
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y];
			if (!m_PlacementStars.ContainsKey(cClientTile) && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_HexMap.Revealed)
			{
				ModifyStar(cClientTile, IHexControl.HexType.PositiveEffect, IHexControl.HexMode.Selected, hover: true, m_PlacementStars);
				SetHexBorders(m_PlacementStars);
			}
		}
	}

	public bool IsMovementPossible()
	{
		switch (CurrentMoveDisplayType)
		{
		case EMoveDisplayType.Normal:
		{
			Waypoint getLastWaypoint = Waypoint.GetLastWaypoint;
			if (getLastWaypoint != null)
			{
				return getLastWaypoint.CanEndMovementHere;
			}
			break;
		}
		case EMoveDisplayType.Push:
			return IsPushPossible;
		case EMoveDisplayType.Pull:
			return IsPullPossible;
		case EMoveDisplayType.Teleport2:
			return IsTeleportPossible;
		}
		return false;
	}

	private void ShowPossibleMovementSelectionHexes()
	{
		switch (CurrentMoveDisplayType)
		{
		case EMoveDisplayType.Normal:
			ShowPossibleMoveStars();
			break;
		case EMoveDisplayType.Push:
			ShowPossiblePushStars();
			break;
		case EMoveDisplayType.Pull:
			ShowPossiblePullStars();
			break;
		case EMoveDisplayType.Teleport2:
			ShowPossibleTeleport2Stars();
			break;
		}
	}

	private void ShowPossibleMoveStars()
	{
		try
		{
			ClearStars();
			Waypoint.ClearValidSelectionTiles();
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove) || m_SelectingADoor)
			{
				return;
			}
			CAbilityMove cAbilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
			bool s_HasWaypoints = Waypoint.s_HasWaypoints;
			bool ignoreBlocked = cAbilityMove.Jump || cAbilityMove.Fly;
			bool ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
			bool ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
			int remainingWaterTilesThatCostZero = cAbilityMove.RemainingWaterTilesThatCostZero;
			Waypoint getLastWaypoint = Waypoint.GetLastWaypoint;
			CActor s_MovingActor = Waypoint.s_MovingActor;
			int num = (s_HasWaypoints ? getLastWaypoint.MovesRemaining : cAbilityMove.RemainingMovesAfterLimit);
			Point point = (s_HasWaypoints ? getLastWaypoint.ClientTile.m_Tile.m_ArrayIndex : Waypoint.s_MovingActor.ArrayIndex);
			int num2 = (s_HasWaypoints ? getLastWaypoint.RemainingWaterTilesThatCostZero : remainingWaterTilesThatCostZero);
			Point? freeTileAwayFromCaster = cAbilityMove?.m_FreeTileAwayFromCaster;
			CActor.EType type = Waypoint.s_MovingActor.Type;
			if (!m_FoundAllChestAndDoorTiles)
			{
				FindAllChestAndDoorTiles();
			}
			List<CClientTile> list = new List<CClientTile>();
			if (cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
			{
				if (Waypoint.s_HasWaypoints)
				{
					list = ((CurrentLineDirection != ScenarioManager.EAdjacentPosition.ECenter) ? GetRemainingTilesInDirection(s_MovingActor, num, CurrentLineDirection) : FindStraightLineDirection(s_MovingActor, num));
					list.AddRange(m_DoorTiles);
					PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, num, num2, freeTileAwayFromCaster, point, type, m_PossibleMoveStarsRemainingInDirection, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint, LOS: false, () =>
					{
						SetHexBorders(m_PossibleMoveStarsRemainingInDirection);
					});
				}
				else
				{
					list = ((CurrentLineDirection != ScenarioManager.EAdjacentPosition.ECenter) ? GetRemainingTilesInDirection(s_MovingActor, num, CurrentLineDirection) : FindAllStraightLineTiles(s_MovingActor, num));
					list.AddRange(m_DoorTiles);
					PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, num, num2, freeTileAwayFromCaster, point, type, m_PossibleMoveStars, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint, LOS: false, () =>
					{
						SetHexBorders(m_PossibleMoveStars);
					});
				}
			}
			else
			{
				if (cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.MustEndNextToEnemy))
				{
					foreach (CTile mustEndNextToEnemyTile in cAbilityMove.GetMustEndNextToEnemyTiles(s_MovingActor, num, (Waypoint.s_Waypoints.Count == 0) ? Waypoint.s_MovingActor.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex, ignoreBlocked, ignoreDifficultTerrain, ignoreHazardousTerrain, num2))
					{
						CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[mustEndNextToEnemyTile.m_ArrayIndex.X, mustEndNextToEnemyTile.m_ArrayIndex.Y];
						if (cClientTile != null)
						{
							list.Add(cClientTile);
						}
					}
				}
				else if (cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.MustEndNextToObstacle))
				{
					foreach (CTile mustEndNextToObstacleTile in cAbilityMove.GetMustEndNextToObstacleTiles(s_MovingActor, num, (Waypoint.s_Waypoints.Count == 0) ? Waypoint.s_MovingActor.ArrayIndex : Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex, ignoreBlocked, ignoreDifficultTerrain, ignoreHazardousTerrain, num2))
					{
						CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[mustEndNextToObstacleTile.m_ArrayIndex.X, mustEndNextToObstacleTile.m_ArrayIndex.Y];
						if (cClientTile2 != null)
						{
							list.Add(cClientTile2);
						}
					}
				}
				else
				{
					List<Point> list2 = new List<Point>();
					List<Point> blockedList = ScenarioManager.PathFinder.CalcBlockedForWaypointFloodFill(s_MovingActor, cAbilityMove);
					ScenarioManager.PathFinder.FloodFill_ToRange(s_MovingActor, point, num, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreHazardousTerrain, num2, blockedList, null, list2, null, cAbilityMove?.IgnoreDoorLocks ?? false);
					foreach (Point item in list2)
					{
						CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.X, item.Y];
						if (cClientTile3 != null)
						{
							list.Add(cClientTile3);
						}
					}
				}
				PossibleMoveTileStarCreation(list, ignoreBlocked, s_MovingActor, num, num2, freeTileAwayFromCaster, point, type, m_PossibleMoveStars, cAbilityMove.RestrictRange, cAbilityMove.RestrictPoint, LOS: false, () =>
				{
					SetHexBorders(m_PossibleMoveStars);
				});
			}
			SetHexBorders(m_ImpossibleMoveChestDoorStars);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.PossibleMoveStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowPossiblePushStars()
	{
		try
		{
			if (!(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush))
			{
				return;
			}
			Waypoint.ClearValidSelectionTiles();
			ClearStars();
			CAbilityPush pushAbility = (CAbilityPush)Choreographer.s_Choreographer.m_CurrentAbility;
			CActor currentTarget = pushAbility.CurrentTarget;
			if (currentTarget == null)
			{
				return;
			}
			bool ignoreBlocked = currentTarget.FlyingAfterConditions || pushAbility.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles);
			int remainingWaterTilesThatCostZero = 0;
			int remainingMoves = (Waypoint.s_HasWaypoints ? Waypoint.GetLastWaypoint.MovesRemaining : pushAbility.RemainingPushes);
			Point point = (Waypoint.s_HasWaypoints ? Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex : pushAbility.CurrentTarget.ArrayIndex);
			CActor.EType type = pushAbility.CurrentTarget.Type;
			List<CClientTile> checkTiles = new List<CClientTile>();
			foreach (CTile pushTile in CAbilityPush.GetPushTiles(pushAbility.CurrentTarget, type, point, pushAbility.PushFromPoint, remainingMoves, pushAbility.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles), pushAbility))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[pushTile.m_ArrayIndex.X, pushTile.m_ArrayIndex.Y];
				if (cClientTile != null)
				{
					checkTiles.Add(cClientTile);
				}
			}
			bool firstTileBlocked = false;
			if (pushAbility.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
			{
				CTile cTile = ScenarioManager.Tiles[pushAbility.CurrentTarget.ArrayIndex.X, pushAbility.CurrentTarget.ArrayIndex.Y];
				if (CAbilityPush.IsPushTileAdjacentToBlockedPushTile(cTile, pushAbility.CurrentTarget, type, point, pushAbility.PushFromPoint, pushAbility.RemainingPushes, intoBlocked: true))
				{
					checkTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y]);
					firstTileBlocked = true;
				}
			}
			PossibleMoveTileStarCreation(checkTiles, ignoreBlocked, currentTarget, remainingMoves, remainingWaterTilesThatCostZero, null, point, type, m_PossibleMoveStars, null, null, LOS: false, () =>
			{
				SetHexBorders(m_PossibleMoveStars);
				SetHexBorders(m_ImpossibleMoveChestDoorStars);
				m_CachedPushStars.Clear();
				m_CachedPushStars.AddRange(m_PossibleMoveStars.Keys.Where((CClientTile x) => checkTiles.Contains(x)));
				bool flag = remainingMoves < pushAbility.ModifiedStrength();
				if (Waypoint.GetLastWaypoint != null && !Waypoint.GetLastWaypoint.CanEndMovementHere)
				{
					flag = false;
				}
				IsPushPossible = flag | firstTileBlocked;
				if (CurrentDisplayState != WorldSpaceStarDisplayState.ShowNone)
				{
					Choreographer.s_Choreographer.readyButton.SetInteractable(IsPushPossible);
				}
			});
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.PossiblePushStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowPossiblePullStars()
	{
		try
		{
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull))
			{
				return;
			}
			Waypoint.ClearValidSelectionTiles();
			ClearStars();
			CAbilityPull cAbilityPull = (CAbilityPull)Choreographer.s_Choreographer.m_CurrentAbility;
			CActor currentTarget = cAbilityPull.CurrentTarget;
			if (currentTarget == null)
			{
				return;
			}
			bool flyingAfterConditions = currentTarget.FlyingAfterConditions;
			bool s_HasWaypoints = Waypoint.s_HasWaypoints;
			int remainingWaterTilesThatCostZero = 0;
			int remainingMoves = (s_HasWaypoints ? Waypoint.GetLastWaypoint.MovesRemaining : cAbilityPull.RemainingPulls);
			Point point = (s_HasWaypoints ? Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex : cAbilityPull.CurrentTarget.ArrayIndex);
			CActor.EType type = cAbilityPull.CurrentTarget.Type;
			List<CClientTile> list = new List<CClientTile>();
			foreach (CTile pullTile in CAbilityPull.GetPullTiles(cAbilityPull.CurrentTarget, type, point, cAbilityPull.PullToPoint, cAbilityPull.PullType, cAbilityPull.RemainingPulls, cAbilityPull))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[pullTile.m_ArrayIndex.X, pullTile.m_ArrayIndex.Y];
				if (cClientTile != null)
				{
					list.Add(cClientTile);
				}
			}
			List<CTile> pullTiles = CAbilityPull.GetPullTiles(cAbilityPull.CurrentTarget, type, cAbilityPull.CurrentTarget.ArrayIndex, cAbilityPull.PullToPoint, cAbilityPull.PullType, cAbilityPull.RemainingPulls, cAbilityPull);
			m_CachedPullStarsTotal.Clear();
			foreach (CTile item in pullTiles)
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (cClientTile2 != null)
				{
					m_CachedPullStarsTotal.Add(cClientTile2);
				}
			}
			PossibleMoveTileStarCreation(list, flyingAfterConditions, currentTarget, remainingMoves, remainingWaterTilesThatCostZero, null, point, type, m_PossibleMoveStars, null, null, LOS: false, () =>
			{
				SetHexBorders(m_PossibleMoveStars);
				SetHexBorders(m_ImpossibleMoveChestDoorStars);
				m_CachedPullStars.Clear();
				m_CachedPullStars.AddRange(m_PossibleMoveStars.Keys);
				bool isPullPossible = Waypoint.s_Waypoints.Count > 0;
				Waypoint getLastWaypoint = Waypoint.GetLastWaypoint;
				if (getLastWaypoint != null && !getLastWaypoint.CanEndMovementHere)
				{
					isPullPossible = false;
				}
				IsPullPossible = isPullPossible;
				Choreographer.s_Choreographer.readyButton.SetInteractable(IsPullPossible);
			});
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.PossiblePullStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void ShowPossibleTeleport2Stars()
	{
		try
		{
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2))
			{
				return;
			}
			if (!m_FoundAllChestAndDoorTiles)
			{
				FindAllChestAndDoorTiles();
			}
			Waypoint.ClearValidSelectionTiles();
			ClearStars();
			CAbilityTeleport2 cAbilityTeleport = (CAbilityTeleport2)Choreographer.s_Choreographer.m_CurrentAbility;
			CActor actorToTeleport = cAbilityTeleport.ActorToTeleport;
			int remainingWaterTilesThatCostZero = 0;
			int remainingMoves = 0;
			Point arrayIndex = cAbilityTeleport.ActorToTeleport.ArrayIndex;
			CActor.EType type = cAbilityTeleport.ActorToTeleport.Type;
			List<CClientTile> list = new List<CClientTile>();
			foreach (CTile teleportTile in cAbilityTeleport.GetTeleportTiles(actorToTeleport.ArrayIndex, cAbilityTeleport.Strength))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[teleportTile.m_ArrayIndex.X, teleportTile.m_ArrayIndex.Y];
				if (cClientTile != null)
				{
					list.Add(cClientTile);
				}
			}
			PossibleMoveTileStarCreation(list, ignoreBlocked: true, actorToTeleport, remainingMoves, remainingWaterTilesThatCostZero, null, arrayIndex, type, m_PossibleMoveStars, null, null, LOS: false, () =>
			{
				if (PhaseManager.CurrentPhaseAction == null)
				{
					Clear();
				}
				else
				{
					SetHexBorders(m_PossibleMoveStars);
					SetHexBorders(m_ImpossibleMoveChestDoorStars);
					m_CachedTeleport2Stars.Clear();
					m_CachedTeleport2Stars.AddRange(m_PossibleMoveStars.Keys);
					bool s_HasWaypoints = Waypoint.s_HasWaypoints;
					IsTeleportPossible = s_HasWaypoints;
					Choreographer.s_Choreographer.readyButton.SetInteractable(IsTeleportPossible);
				}
			});
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.PossibleTeleport2Stars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PossibleMoveTileStarCreation(List<CClientTile> tiles, bool ignoreBlocked, CActor movingActor, int remainingMoves, int remainingWaterTilesThatCostZero, Point? freeTileAwayFromCaster, Point startingPoint, CActor.EType actorType, Dictionary<CClientTile, IHexControl> outStoreTiles, int? restrictRange = null, Point? restrictPoint = null, bool LOS = false, Action whenDone = null)
	{
		bool jump = ignoreBlocked;
		bool fly = ignoreBlocked;
		bool ignoreDifficultTerrain = false;
		bool flag = false;
		bool carryOther = false;
		CActor.EType carryType = CActor.EType.Unknown;
		CAbilityMove abilityMove = null;
		if (Choreographer.s_Choreographer.m_CurrentAbility != null)
		{
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
			{
				abilityMove = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityMove;
				jump = abilityMove.Jump;
				fly = abilityMove.Fly;
				ignoreDifficultTerrain = abilityMove.IgnoreDifficultTerrain;
				flag = abilityMove.IgnoreHazardousTerrain;
				carryOther = abilityMove.CarryOtherActorsOnHex;
				if (abilityMove.ActorsToCarry.Count > 0)
				{
					int strength = 0;
					carryType = abilityMove.ActorsToCarry[0].Type;
					actorType = carryType;
					CAbilityMove.GetMoveBonuses(abilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out flag, ref strength);
				}
				ignoreBlocked = jump | fly;
			}
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull || Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush || Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2)
			{
				ignoreDifficultTerrain = true;
			}
		}
		List<CObjectTrap> unactivatedTraps = (from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
			where !w.Activated
			select w).ToList();
		CActor.EType movingType = actorType;
		if (actorType == CActor.EType.Player && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Count > 0 && GameState.OverridenActionActorStack.Peek().OriginalType == CActor.EType.Enemy)
		{
			movingType = CActor.EType.Enemy;
		}
		List<CActiveBonus> addConditionActiveBonuses = CActiveBonus.FindApplicableActiveBonuses(movingActor, CAbility.EAbilityType.AddCondition).ToList();
		IList addDesignatedTiles = GetDesignatedTilesForCurrentAbility();
		List<Action> mainThreadActionQueue = new List<Action>();
		_003C_003Ec__DisplayClass135_0 CS_0024_003C_003E8__locals2;
		AsyncCreateTiles(() =>
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				foreach (CClientTile tile in tiles)
				{
					CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
					if (currentAbility == null)
					{
						break;
					}
					if (IsMoveTileValid(tile, ignoreBlocked, movingActor, movingType, abilityMove))
					{
						CTile targetTile = ScenarioManager.Tiles[movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y];
						if (!LOS || CActor.HaveLOS(tile.m_Tile, targetTile))
						{
							if (tile.m_Tile.m_ArrayIndex != startingPoint)
							{
								if (!(currentAbility is CAbilityTeleport2))
								{
									List<Point> list = CActor.FindCharacterPath(movingActor, startingPoint, tile.m_Tile.m_ArrayIndex, ignoreBlocked, CurrentMoveDisplayType != EMoveDisplayType.Normal, out var foundPath, ignoreDifficultTerrain, ignoreNegativeTiles: false, remainingWaterTilesThatCostZero, carryOther, carryType, logFailure: false, remainingMoves, ignoreCannotStop: true, jump, fly, currentAbility.IgnoreDoorLocks);
									int num = CAbilityMove.CalculateMoveCost(movingActor, startingPoint, list, fly || ((CurrentMoveDisplayType == EMoveDisplayType.Normal) & jump), ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, remainingWaterTilesThatCostZero, freeTileAwayFromCaster);
									if (!foundPath || num > remainingMoves)
									{
										list = CActor.FindCharacterPath(movingActor, startingPoint, tile.m_Tile.m_ArrayIndex, ignoreBlocked, CurrentMoveDisplayType != EMoveDisplayType.Normal, out foundPath, ignoreDifficultTerrain, ignoreNegativeTiles: true, remainingWaterTilesThatCostZero, carryOther, carryType, logFailure: false, remainingMoves, ignoreCannotStop: true, jump, fly, currentAbility.IgnoreDoorLocks);
										num = CAbilityMove.CalculateMoveCost(movingActor, startingPoint, list, fly || ((CurrentMoveDisplayType == EMoveDisplayType.Normal) & jump), ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, remainingWaterTilesThatCostZero, freeTileAwayFromCaster);
										if (!foundPath || num > remainingMoves)
										{
											if (IsPossibleMoveTileAsDoorOrChest_NonBlocking(tile))
											{
												CClientTile copy_tile = tile;
												IHexControl.HexType copy_type = m_MoveChestDoorPossibilityTemplate.m_Type;
												IHexControl.HexMode copy_mode = m_MoveChestDoorPossibilityTemplate.m_Mode;
												bool copy_hover = m_MoveChestDoorPossibilityTemplate.m_Hover;
												Dictionary<CClientTile, IHexControl> copy_container = m_ImpossibleMoveChestDoorStars;
												mainThreadActionQueue.Add(() =>
												{
													ModifyStar(copy_tile, copy_type, copy_mode, copy_hover, copy_container);
												});
											}
											continue;
										}
									}
									if (restrictRange.HasValue && restrictPoint.HasValue)
									{
										List<Point> path = CActor.FindCharacterPath(movingActor, restrictPoint.Value, tile.m_Tile.m_ArrayIndex, ignoreBlocked, CurrentMoveDisplayType != EMoveDisplayType.Normal, out var foundPath2, ignoreDifficultTerrain, ignoreNegativeTiles: true, remainingWaterTilesThatCostZero, carryOther, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump, fly, currentAbility.IgnoreDoorLocks);
										if (!foundPath2 || CAbilityMove.CalculateMoveCost(movingActor, restrictPoint.Value, path, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, remainingWaterTilesThatCostZero, freeTileAwayFromCaster) > restrictRange.Value)
										{
											continue;
										}
									}
									foreach (Point item in list)
									{
										CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.X, item.Y];
										CClientTile copy_pathTile = cClientTile;
										Dictionary<CClientTile, IHexControl> copy_outStoreTiles = outStoreTiles;
										List<CObjectTrap> copy_unactivatedTraps = unactivatedTraps;
										List<CActiveBonus> copy_addConditionActiveBonuses = addConditionActiveBonuses;
										IList copy_addDesignatedTiles = addDesignatedTiles;
										mainThreadActionQueue.Add(() =>
										{
											AddPossibleMoveTileStar(copy_pathTile, copy_outStoreTiles, copy_unactivatedTraps, copy_addConditionActiveBonuses, copy_addDesignatedTiles);
										});
									}
								}
								else if (((actorType == CActor.EType.Player && GameState.OverridingCurrentActor && GameState.OverridenActionActorStack.Count > 0 && (GameState.OverridenActionActorStack.Peek().PreviousActor.Type != CActor.EType.Player || movingActor.OriginalType != CActor.EType.Player)) || (movingActor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)) && ProcessClosedDoorsForControlledAI_NonBlocking(tile))
								{
									CClientTile copy_tile2 = tile;
									IHexControl.HexType copy_type2 = m_MoveChestDoorPossibilityTemplate.m_Type;
									IHexControl.HexMode copy_mode2 = m_MoveChestDoorPossibilityTemplate.m_Mode;
									bool copy_hover2 = m_MoveChestDoorPossibilityTemplate.m_Hover;
									Dictionary<CClientTile, IHexControl> copy_container2 = m_ImpossibleMoveChestDoorStars;
									mainThreadActionQueue.Add(() =>
									{
										ModifyStar(copy_tile2, copy_type2, copy_mode2, copy_hover2, copy_container2);
									});
									continue;
								}
							}
							CClientTile copy_pathTile2 = tile;
							Dictionary<CClientTile, IHexControl> copy_outStoreTiles2 = outStoreTiles;
							mainThreadActionQueue.Add(() =>
							{
								AddPossibleMoveTileStar(copy_pathTile2, copy_outStoreTiles2, (List<CObjectTrap>)(object)CS_0024_003C_003E8__locals2, (List<CActiveBonus>)(object)CS_0024_003C_003E8__locals2, (IList)(object)CS_0024_003C_003E8__locals2);
							});
						}
					}
				}
			}
			return 0;
		}, (int _) =>
		{
			QueueSynchronizedCoroutine(StartCoroutine(RunTimeSlicedQueueCrt(() =>
			{
				whenDone?.Invoke();
			}, mainThreadActionQueue)));
		});
	}

	private void AsyncCreateTiles(Func<int> doAsync, Action<int> whenFinished)
	{
		QueueSynchronizedCoroutine(this.Synchronize(doAsync, whenFinished));
	}

	private void StopSynchronizedCoroutines()
	{
		foreach (Coroutine synchronizedCoroutine in SynchronizedCoroutines)
		{
			StopCoroutine(synchronizedCoroutine);
		}
		SynchronizedCoroutines.Clear();
	}

	private void QueueSynchronizedCoroutine(Coroutine coro)
	{
		if (coro != null)
		{
			StopSynchronizedCoroutines();
			SynchronizedCoroutines.Add(coro);
		}
	}

	private static IEnumerator RunTimeSlicedQueueCrt(Action whenDone, List<Action> mainThreadActionQueue)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		foreach (Action item in mainThreadActionQueue)
		{
			item();
			if (stopwatch.ElapsedMilliseconds > 5)
			{
				yield return null;
				stopwatch.Restart();
			}
		}
		whenDone?.Invoke();
	}

	private IList GetDesignatedTilesForCurrentAbility()
	{
		IList list = null;
		if (Choreographer.s_Choreographer.m_CurrentAbility != null)
		{
			list = Choreographer.s_Choreographer.m_CurrentAbility.GetVar("AddDesignatedTilesHighlight").GetObject<IList>();
			if (list == null)
			{
				list = Choreographer.s_Choreographer.m_CurrentAbility.GetMiscVar("AddDesignatedTilesHighlight").GetObject<IList>();
			}
		}
		return list;
	}

	private bool IsDesignatedTileForCurrentAbility(CClientTile clientTile, IList designatedTiles)
	{
		if (designatedTiles == null)
		{
			return false;
		}
		if (clientTile == null)
		{
			return false;
		}
		return designatedTiles.Contains(clientTile.m_Tile);
	}

	private bool IsDesignatedTileForCurrentAbility(CClientTile clientTile)
	{
		IList designatedTilesForCurrentAbility = GetDesignatedTilesForCurrentAbility();
		return IsDesignatedTileForCurrentAbility(clientTile, designatedTilesForCurrentAbility);
	}

	private void AddPossibleMoveTileStar(CClientTile tile, Dictionary<CClientTile, IHexControl> storeTiles, List<CObjectTrap> unactivatedTraps, List<CActiveBonus> addConditionActiveBonuses, IList addDesignatedTiles)
	{
		if (tile?.m_Tile == null)
		{
			return;
		}
		if (IsPossibleMoveTileAsDoorOrChest_NonBlocking(tile))
		{
			SynchronizedModifyStar(tile, m_MoveChestDoorPossibilityTemplate.m_Type, m_MoveChestDoorPossibilityTemplate.m_Mode, m_MoveChestDoorPossibilityTemplate.m_Hover, storeTiles);
			return;
		}
		bool flag = ShowAsHazardous(tile, unactivatedTraps);
		bool flag2 = ShowAsDifficulTerrain(tile?.m_Tile);
		bool flag3 = IsDesignatedTileForCurrentAbility(tile, addDesignatedTiles);
		if (!flag3)
		{
			foreach (CActiveBonus addConditionActiveBonuse in addConditionActiveBonuses)
			{
				if (addConditionActiveBonuse.ActiveBonusIsActivatedByTile(tile.m_Tile))
				{
					flag3 = true;
					break;
				}
			}
		}
		if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles } cAbilityMove && cAbilityMove.CheckMustEndOnSpecificTiles(tile.m_Tile))
		{
			flag3 = true;
		}
		bool isIceTerrain = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsIceTerrain;
		if (flag3)
		{
			SynchronizedModifyStar(tile, IHexControl.HexType.ActiveBonusCondition, IHexControl.HexMode.PossibleTarget, hover: false, storeTiles);
		}
		else if (flag & flag2)
		{
			SynchronizedModifyStar(tile, m_MoveHazardPossibilityTemplate.m_Type, m_MoveHazardPossibilityTemplate.m_Mode, m_MoveHazardPossibilityTemplate.m_Hover, storeTiles);
		}
		else if (flag2)
		{
			SynchronizedModifyStar(tile, m_DifficultTerrainPossibilityTemplate.m_Type, m_DifficultTerrainPossibilityTemplate.m_Mode, m_DifficultTerrainPossibilityTemplate.m_Hover, storeTiles);
		}
		else if (flag)
		{
			SynchronizedModifyStar(tile, m_MoveHazardPossibilityTemplate.m_Type, m_MoveHazardPossibilityTemplate.m_Mode, m_MoveHazardPossibilityTemplate.m_Hover, storeTiles);
		}
		else if (isIceTerrain)
		{
			SynchronizedModifyStar(tile, m_IceTerrainPossibilityTemplate.m_Type, m_IceTerrainPossibilityTemplate.m_Mode, m_IceTerrainPossibilityTemplate.m_Hover, storeTiles);
		}
		else
		{
			SynchronizedModifyStar(tile, m_MovePossiblityTemplate.m_Type, m_MovePossiblityTemplate.m_Mode, m_MovePossiblityTemplate.m_Hover, storeTiles);
		}
	}

	private bool IsPossibleMoveTileAsDoorOrChest_NonBlocking(CClientTile tile, bool checkChests = true, bool checkDoors = true)
	{
		if (checkChests && m_ChestTiles.Contains(tile))
		{
			if (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
			{
				m_ChestTiles.Remove(tile);
				return false;
			}
			return true;
		}
		if (checkDoors && m_DoorTiles.Contains(tile))
		{
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor == null || cObjectDoor.IsDungeonEntrance || cObjectDoor.IsDungeonExit || cObjectDoor.DoorIsOpen)
			{
				m_DoorTiles.Remove(tile);
				return false;
			}
			return true;
		}
		return false;
	}

	private bool ProcessClosedDoorsForControlledAI_NonBlocking(CClientTile tile)
	{
		if (m_DoorTiles.Contains(tile))
		{
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor == null || cObjectDoor.IsDungeonEntrance || cObjectDoor.IsDungeonExit || cObjectDoor.DoorIsOpen)
			{
				m_DoorTiles.Remove(tile);
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool IsMoveTileValid(CClientTile tile, bool ignoreBlocked, CActor movingActor, CActor.EType movingType, CAbility ability)
	{
		try
		{
			if (tile == null)
			{
				return false;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode == null)
			{
				return false;
			}
			if (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_Hex2Map != null)
			{
				if (!tile.m_Tile.m_HexMap.RevealedAndNotDestroyed && !tile.m_Tile.m_Hex2Map.RevealedAndNotDestroyed)
				{
					return false;
				}
			}
			else
			{
				if (tile.m_Tile.m_HexMap != null && !tile.m_Tile.m_HexMap.RevealedAndNotDestroyed)
				{
					return false;
				}
				if (tile.m_Tile.m_Hex2Map != null && !tile.m_Tile.m_Hex2Map.RevealedAndNotDestroyed)
				{
					return false;
				}
			}
			if (!cNode.Walkable || (cNode.Blocked && !ignoreBlocked))
			{
				return false;
			}
			CObjectDoor cObjectDoor = (CObjectDoor)tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
			if (cObjectDoor != null && ((cObjectDoor.DoorIsLocked && !movingActor.CanUnlockDoors) || (cObjectDoor.PropHealthDetails != null && cObjectDoor.PropHealthDetails.HasHealth && cObjectDoor.PropHealthDetails.CurrentHealth > 0)))
			{
				return false;
			}
			foreach (CActor item in ScenarioManager.Scenario.FindActorsAt(tile.m_Tile.m_ArrayIndex))
			{
				if (item != null && !ignoreBlocked && !item.DoesNotBlock && (!CActor.AreActorsAllied(item.Type, movingType, item, movingActor) || item.IsDowned) && !item.Deactivated)
				{
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.IsMoveTileValid().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private void ShowMovePathStars()
	{
		try
		{
			m_TilesExpectedToDisplay.Clear();
			m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
			m_TilesExpectedToDisplay.AddRange(m_PossibleMoveStars.Keys);
			m_TilesExpectedToDisplay.AddRange(m_PossibleMoveStarsRemainingInDirection.Keys);
			m_TilesExpectedToDisplay.AddRange(m_ImpossibleMoveChestDoorStars.Keys);
			m_CachedMoveStars.Clear();
			m_CachedMoveStars.AddRange(m_PathMoveStars.Keys);
			m_PathMoveStars.Clear();
			while (WaypointLine.s_Instance.m_Transform.childCount > 0)
			{
				ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
			}
			WaypointLine.s_Instance.m_UpdateFlag = true;
			Waypoint.ClearValidSelectionTiles();
			CActor cActor = Waypoint.s_MovingActor;
			if (cActor == null)
			{
				return;
			}
			bool jump = false;
			bool fly = false;
			bool ignoreDifficultTerrain = false;
			bool ignoreHazardousTerrain = false;
			bool ignoreBlockedTileMoveCost = false;
			int num = 0;
			Point? freeTileAwayFromCaster = null;
			bool ignoreDoorLocks = false;
			CAbilityMove cAbilityMove = null;
			CAbilityPull cAbilityPull = null;
			CAbilityPush cAbilityPush = null;
			CAbilityTeleport2 cAbilityTeleport = null;
			bool carryOtherActors = false;
			CActor.EType carryType = CActor.EType.Unknown;
			if (Choreographer.s_Choreographer.m_CurrentAbility != null)
			{
				ignoreDoorLocks = Choreographer.s_Choreographer.m_CurrentAbility.IgnoreDoorLocks;
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
				{
					cAbilityMove = (CAbilityMove)Choreographer.s_Choreographer.m_CurrentAbility;
					jump = cAbilityMove.Jump;
					fly = cAbilityMove.Fly;
					ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
					ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
					ignoreBlockedTileMoveCost = cAbilityMove.IgnoreBlockedTileMoveCost;
					num = cAbilityMove.RemainingWaterTilesThatCostZero;
					freeTileAwayFromCaster = cAbilityMove.m_FreeTileAwayFromCaster;
					carryOtherActors = cAbilityMove.CarryOtherActorsOnHex;
					if (cAbilityMove.ActorsToCarry.Count > 0)
					{
						int strength = 0;
						carryType = cAbilityMove.ActorsToCarry[0].Type;
						CAbilityMove.GetMoveBonuses(cAbilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, ref strength);
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull)
				{
					cAbilityPull = (CAbilityPull)Choreographer.s_Choreographer.m_CurrentAbility;
					cActor = cAbilityPull.CurrentTarget;
					fly = cActor?.FlyingAfterConditions ?? false;
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush)
				{
					cAbilityPush = (CAbilityPush)Choreographer.s_Choreographer.m_CurrentAbility;
					cActor = cAbilityPush.CurrentTarget;
					fly = (cActor != null && cActor.FlyingAfterConditions) || cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles);
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2)
				{
					cAbilityTeleport = (CAbilityTeleport2)Choreographer.s_Choreographer.m_CurrentAbility;
					cActor = cAbilityTeleport.ActorToTeleport;
					fly = true;
				}
			}
			if (cActor == null || !ScenarioManager.Scenario.HasActor(cActor))
			{
				return;
			}
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
			if (gameObject == null)
			{
				return;
			}
			CClientTile clientTile = ((!Choreographer.s_Choreographer.ThisPlayerHasTurnControl) ? null : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			ControllerInputPointer.CursorType = ((InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile != null) ? ECursorType.Default : ECursorType.Invalid);
			List<Point> list = new List<Point>();
			List<int> list2 = new List<int>();
			list.Add(cActor.ArrayIndex);
			list2.Add(num);
			if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush && cAbilityPush != null && cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked))
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				if ((clientTile == null || !m_PossibleMoveStars.ContainsKey(clientTile) || clientTile == cClientTile) && CAbilityPush.IsPushTileAdjacentToBlockedPushTile(cClientTile.m_Tile, cAbilityPush.CurrentTarget, cAbilityPush.CurrentTarget.Type, cClientTile.m_Tile.m_ArrayIndex, cAbilityPush.PushFromPoint, cAbilityPush.Strength, intoBlocked: true))
				{
					Waypoint.AddValidSelectionTiles(cActor.ArrayIndex);
					if (m_PossibleMoveStars.TryGetValue(cClientTile, out var value))
					{
						value = m_PossibleMoveStars[cClientTile];
						value.Type = IHexControl.HexType.NegativeEffect;
						value.Mode = IHexControl.HexMode.Selected;
						value.Hover = cClientTile == clientTile;
						value.RefreshHexUI();
						if (!m_PathMoveStars.ContainsKey(cClientTile))
						{
							m_PathMoveStars.Add(cClientTile, value);
						}
					}
				}
			}
			list.AddRange(Waypoint.s_Waypoints.Select((GameObject w) => w.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex));
			list2.AddRange(Waypoint.s_Waypoints.Select((GameObject w) => w.GetComponent<Waypoint>().RemainingWaterTilesThatCostZero));
			if (Waypoint.GetLastWaypoint == null || Waypoint.GetLastWaypoint.MovesRemaining > 0)
			{
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (list.Count < 2)
					{
						if (clientTile != null && m_PossibleMoveStars.ContainsKey(clientTile) && !m_SelectingADoor)
						{
							list.Add(clientTile.m_Tile.m_ArrayIndex);
							if (Waypoint.s_HasWaypoints)
							{
								int valueOrDefault = (Waypoint.s_Waypoints.Find((GameObject x) => x.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex == clientTile.m_Tile.m_ArrayIndex)?.GetComponent<Waypoint>()?.RemainingWaterTilesThatCostZero).GetValueOrDefault();
								list2.Add(valueOrDefault);
							}
						}
					}
					else if (clientTile != null && m_PossibleMoveStarsRemainingInDirection.ContainsKey(clientTile) && !m_SelectingADoor)
					{
						list.Add(clientTile.m_Tile.m_ArrayIndex);
						if (Waypoint.s_HasWaypoints)
						{
							int valueOrDefault2 = (Waypoint.s_Waypoints.Find((GameObject x) => x.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex == clientTile.m_Tile.m_ArrayIndex)?.GetComponent<Waypoint>()?.RemainingWaterTilesThatCostZero).GetValueOrDefault();
							list2.Add(valueOrDefault2);
						}
					}
				}
				else if (cAbilityPush != null)
				{
					if (clientTile != null && m_PossibleMoveStars.ContainsKey(clientTile) && m_CachedPushStars.Contains(clientTile) && !m_SelectingADoor)
					{
						list.Add(clientTile.m_Tile.m_ArrayIndex);
						list2.Add(0);
					}
				}
				else if (cAbilityPull != null)
				{
					if (clientTile != null && m_PossibleMoveStars.ContainsKey(clientTile) && m_CachedPullStars.Contains(clientTile) && !m_SelectingADoor)
					{
						list.Add(clientTile.m_Tile.m_ArrayIndex);
						list2.Add(0);
					}
				}
				else if (cAbilityTeleport != null)
				{
					if (clientTile != null && m_PossibleMoveStars.ContainsKey(clientTile) && m_CachedTeleport2Stars.Contains(clientTile) && !m_SelectingADoor)
					{
						list.Add(clientTile.m_Tile.m_ArrayIndex);
						list2.Add(0);
					}
				}
				else if (clientTile != null && m_PossibleMoveStars.ContainsKey(clientTile))
				{
					GameObject gameObject2 = Waypoint.s_Waypoints.Find((GameObject x) => x.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex == clientTile.m_Tile.m_ArrayIndex);
					if (gameObject2 != null)
					{
						list2.Add(gameObject2.GetComponent<Waypoint>().RemainingWaterTilesThatCostZero);
					}
					else if (num > 0)
					{
						int num2 = CActor.FindCharacterPath(cActor, list[list.Count - 1], clientTile.m_Tile.m_ArrayIndex, jump | fly, ignoreMoveCost: false, out var _, ignoreDifficultTerrain, ignoreHazardousTerrain, list2[list2.Count - 1], carryOtherActors, carryType, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump, fly, ignoreDoorLocks).Count((Point x) => ScenarioManager.GetTile(x).IsWater);
						int item = Mathf.Max(list2[list2.Count - 1] - num2, 0);
						list2.Add(item);
					}
					else
					{
						list2.Add(0);
					}
					list.Add(clientTile.m_Tile.m_ArrayIndex);
				}
			}
			if (list.Count < 2)
			{
				foreach (CClientTile cachedMoveStar in m_CachedMoveStars)
				{
					if (m_PossibleMoveStars.ContainsKey(cachedMoveStar))
					{
						ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar, m_PossibleMoveStars[cachedMoveStar]);
					}
					else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && m_PossibleMoveStarsRemainingInDirection.ContainsKey(cachedMoveStar))
					{
						ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar, m_PossibleMoveStarsRemainingInDirection[cachedMoveStar]);
					}
				}
				return;
			}
			bool flag = cAbilityTeleport != null;
			List<Point> list3 = new List<Point>();
			bool foundPath2;
			if (flag)
			{
				list3.Add(list[list.Count - 1]);
			}
			else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
			{
				list3.AddRange(CActor.FindStraightPath(cActor, list[0], list[list.Count - 1], jump | fly, out foundPath2));
			}
			else if (cAbilityMove != null)
			{
				bool flag2 = false;
				for (int num3 = 0; num3 < list.Count - 1; num3++)
				{
					list3.AddRange(CActor.FindCharacterPath(cActor, list[num3], list[num3 + 1], jump | fly, ignoreMoveCost: false, out foundPath2, ignoreDifficultTerrain, ignoreHazardousTerrain, list2[num3], carryOtherActors, carryType, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump, fly, ignoreDoorLocks));
					if (!foundPath2)
					{
						flag2 = true;
					}
				}
				if (flag2 || CAbilityMove.CalculateMoveCost(cActor, list[0], list3, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num, freeTileAwayFromCaster) > cAbilityMove.RemainingMovesAfterLimit)
				{
					list3.Clear();
					flag2 = false;
					for (int num4 = 0; num4 < list.Count - 1; num4++)
					{
						list3.AddRange(CActor.FindCharacterPath(cActor, list[num4], list[num4 + 1], jump | fly, ignoreMoveCost: false, out foundPath2, ignoreDifficultTerrain, ignoreNegativeTiles: true, list2[num4], carryOtherActors, carryType, logFailure: false, cAbilityMove.RemainingMovesAfterLimit, ignoreCannotStop: false, jump, fly, ignoreDoorLocks));
					}
					if (flag2 || CAbilityMove.CalculateMoveCost(cActor, list[0], list3, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num, freeTileAwayFromCaster) > cAbilityMove.RemainingMovesAfterLimit)
					{
						list3.Clear();
					}
				}
			}
			else
			{
				for (int num5 = 0; num5 < list.Count - 1; num5++)
				{
					list3.AddRange(CActor.FindCharacterPath(cActor, list[num5], list[num5 + 1], jump | fly, ignoreMoveCost: true, out foundPath2, ignoreDifficultTerrain, ignoreHazardousTerrain, list2[num5], carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: true, jump, fly, ignoreDoorLocks));
				}
			}
			bool flag3 = true;
			if (list3.Count == 0)
			{
				flag3 = false;
			}
			else if (!flag)
			{
				Point arrayIndex = list3[list3.Count - 1];
				if (ScenarioManager.PathFinder.GetNode(arrayIndex).IsIceTerrain)
				{
					List<Point> waypoints = Waypoint.s_Waypoints.Select((GameObject wp) => wp.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex).ToList();
					flag3 = (fly | jump) || CActorStatic.IsIceTerrainSlideInterrupted(cActor, list3, waypoints);
				}
				if (flag3 && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull cAbilityPull2)
				{
					if (list.Count > 0)
					{
						flag3 = list3.Contains(list[list.Count - 1]);
					}
					if (flag3)
					{
						for (int num6 = 0; num6 < list3.Count; num6++)
						{
							Point point = list3[num6];
							bool flag4 = m_CachedPullStarsTotal.Any((CClientTile t) => t.m_Tile.m_ArrayIndex == point);
							if (!flag4 && num6 < list3.Count - 1)
							{
								bool isIceTerrain = ScenarioManager.PathFinder.GetNode(point).IsIceTerrain;
								CActor cActor2 = ScenarioManager.GetTile(point)?.LivingActor;
								if (isIceTerrain || (cActor2 != null && (!cActor2.IsTransientBlocker || cActor2.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) || CustomAbility.IsAlly(cActor2, cAbilityPull2.CurrentTarget))))
								{
									flag4 = true;
								}
							}
							if (!flag4)
							{
								flag3 = false;
								break;
							}
						}
					}
				}
			}
			List<Point> list4 = new List<Point>();
			if (Waypoint.s_HasWaypoints)
			{
				foreach (Point item2 in list3)
				{
					list4.Add(item2);
					if (item2 == Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex)
					{
						break;
					}
				}
			}
			if (flag3 || list4.Count > 0)
			{
				Vector3 position = gameObject.transform.position;
				ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>()?.EnableType(WaypointHolder.EWaypointType.Start);
			}
			IEnumerable<CObjectTrap> source = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			foreach (Point item3 in list3)
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item3.X, item3.Y];
				if (cClientTile2 == null || cClientTile2.m_GameObject == null)
				{
					continue;
				}
				bool flag5 = false;
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (!m_PossibleMoveStars.ContainsKey(cClientTile2) && !m_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile2))
					{
						if (!m_AllLineTiles.Contains(cClientTile2.m_Tile))
						{
							{
								foreach (CClientTile cachedMoveStar2 in m_CachedMoveStars)
								{
									if (m_PossibleMoveStars.ContainsKey(cachedMoveStar2))
									{
										ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar2, m_PossibleMoveStars[cachedMoveStar2]);
									}
									else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && m_PossibleMoveStarsRemainingInDirection.ContainsKey(cachedMoveStar2))
									{
										ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar2, m_PossibleMoveStarsRemainingInDirection[cachedMoveStar2]);
									}
								}
								return;
							}
						}
						flag5 = true;
					}
				}
				else
				{
					if (cAbilityPull != null && !flag3)
					{
						continue;
					}
					if (!m_PossibleMoveStars.ContainsKey(cClientTile2))
					{
						flag5 = true;
					}
				}
				if (flag5)
				{
					if (m_ChestTiles.Contains(cClientTile2) || m_DoorTiles.Contains(cClientTile2))
					{
						if (m_ChestTiles.Contains(cClientTile2) && cClientTile2.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
						{
							m_ChestTiles.Remove(cClientTile2);
						}
						ModifyStar(cClientTile2, m_MoveChestDoorPossibilityTemplate.m_Type, IHexControl.HexMode.Reach, m_MoveChestDoorPossibilityTemplate.m_Hover, m_PossibleMoveStars);
						m_TilesExpectedToDisplay.Add(cClientTile2);
						m_PossibleMoveStars[cClientTile2].RefreshHexUI();
					}
					else
					{
						ModifyStar(cClientTile2, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false, m_PossibleMoveStars);
						m_TilesExpectedToDisplay.Add(cClientTile2);
						m_PossibleMoveStars[cClientTile2].RefreshHexUI();
					}
				}
				bool flag6 = IsDesignatedTileForCurrentAbility(cClientTile2);
				if (!flag6)
				{
					foreach (CActiveBonus item4 in CActiveBonus.FindApplicableActiveBonuses(cActor, CAbility.EAbilityType.AddCondition).ToList())
					{
						if (item4.ActiveBonusIsActivatedByTile(cClientTile2.m_Tile))
						{
							flag6 = true;
							if (item4.BaseCard is CItem cItem)
							{
								ShowHoveredTileTooltip(cClientTile2, LocalizationManager.GetTranslation(cItem.Name), LocalizationManager.GetTranslation("Item_" + cItem.YMLData.Art.RemoveSpaces()));
							}
						}
					}
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles } cAbilityMove2 && cAbilityMove2.CheckMustEndOnSpecificTiles(cClientTile2.m_Tile))
				{
					flag6 = true;
				}
				bool flag7 = false;
				bool flag8 = ShowAsDifficulTerrain(cClientTile2.m_Tile);
				bool flag9 = Waypoint.GetLastWaypoint != null && Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex == item3;
				CObjectProp cObjectProp = cClientTile2.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
				if (cObjectProp != null && source.Contains(cObjectProp))
				{
					flag7 = true;
				}
				if (!flag7 && (cClientTile2.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null || cClientTile2.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null))
				{
					flag7 = true;
				}
				if (clientTile != null && Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect } cAbilityPush2 && additionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked) && item3 == list3.Last())
				{
					int tileDistance = ScenarioManager.GetTileDistance(cAbilityPush2.CurrentTarget.ArrayIndex.X, cAbilityPush2.CurrentTarget.ArrayIndex.Y, clientTile.m_Tile.m_ArrayIndex.X, clientTile.m_Tile.m_ArrayIndex.Y);
					int num7 = cAbilityPush2.Strength - tileDistance;
					if (num7 > 0 && CAbilityPush.IsPushTileAdjacentToBlockedPushTile(clientTile.m_Tile, cAbilityPush2.CurrentTarget, cAbilityPush2.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPush2.PushFromPoint, num7, intoBlocked: true))
					{
						flag7 = true;
					}
				}
				IHexControl hexControl = null;
				if (m_PossibleMoveStars.ContainsKey(cClientTile2))
				{
					hexControl = m_PossibleMoveStars[cClientTile2];
				}
				else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && m_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile2))
				{
					hexControl = m_PossibleMoveStarsRemainingInDirection[cClientTile2];
				}
				if (hexControl == null)
				{
					return;
				}
				if (m_ChestTiles.Contains(cClientTile2) || m_DoorTiles.Contains(cClientTile2))
				{
					hexControl.Type = m_MoveChestDoorPossibilityTemplate.m_Type;
					hexControl.Mode = IHexControl.HexMode.Selected;
				}
				else if (flag6)
				{
					hexControl.Type = IHexControl.HexType.ActiveBonusCondition;
					hexControl.Mode = IHexControl.HexMode.PossibleTarget;
				}
				else if (ScenarioManager.PathFinder.Nodes[item3.X, item3.Y].IsIceTerrain)
				{
					hexControl.Type = m_IceTerrainPossibilityTemplate.m_Type;
					hexControl.Mode = ((!flag9) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
				}
				else if (flag7)
				{
					hexControl.Type = m_MoveHazardPossibilityTemplate.m_Type;
					hexControl.Mode = ((!flag9) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
				}
				else if (flag8)
				{
					hexControl.Type = m_DifficultTerrainPossibilityTemplate.m_Type;
					hexControl.Mode = ((!flag9) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
				}
				else
				{
					hexControl.Type = m_MovePossiblityTemplate.m_Type;
					if (flag3)
					{
						hexControl.Mode = ((!flag9) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
					}
					else
					{
						hexControl.Mode = IHexControl.HexMode.Reach;
					}
				}
				hexControl.Hover = flag3 && cClientTile2 == clientTile;
				hexControl.RefreshHexUI();
				if (hexControl.Hover)
				{
					ControllerInputPointer.CursorType = ECursorType.Targeted;
				}
				if (!m_PathMoveStars.ContainsKey(cClientTile2))
				{
					m_PathMoveStars.Add(cClientTile2, hexControl);
				}
				if (flag3 || list4.Contains(cClientTile2.m_Tile.m_ArrayIndex))
				{
					WaypointHolder component = ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, cClientTile2.m_GameObject.transform.position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>();
					if (item3 == cActor.ArrayIndex)
					{
						component?.EnableType(WaypointHolder.EWaypointType.Start);
					}
					else if (list.Contains(item3))
					{
						component?.EnableType(WaypointHolder.EWaypointType.Node);
					}
					else
					{
						component?.EnableType(WaypointHolder.EWaypointType.Small);
					}
				}
				if (cClientTile2.m_Tile.m_ArrayIndex == list3[list3.Count - 1] && !flag3)
				{
					continue;
				}
				if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly))
				{
					if (Waypoint.s_HasWaypoints)
					{
						if (m_PossibleMoveStarsRemainingInDirection.ContainsKey(cClientTile2))
						{
							Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
						}
					}
					else
					{
						Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPush != null)
				{
					if (m_CachedPushStars.Contains(cClientTile2))
					{
						Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityPull != null)
				{
					if (m_CachedPullStars.Contains(cClientTile2))
					{
						Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
					}
				}
				else if (cAbilityTeleport != null)
				{
					if (m_CachedTeleport2Stars.Contains(cClientTile2))
					{
						Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
					}
				}
				else
				{
					Waypoint.AddValidSelectionTiles(cClientTile2.m_Tile.m_ArrayIndex);
				}
			}
			if (ConsoleVar_ShowPathCost)
			{
				int num8 = 0;
				s_DebugPathPoints.Clear();
				for (int num9 = 0; num9 < list3.Count; num9++)
				{
					num8 += CAbilityMove.CalculateMoveUsed(cActor, list[0], list3, num9, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num, freeTileAwayFromCaster);
					s_DebugPathPoints.Add(new Tuple<Point, int>(list3[num9], num8));
				}
			}
			foreach (CClientTile cachedMoveStar3 in m_CachedMoveStars)
			{
				if (!m_PathMoveStars.ContainsKey(cachedMoveStar3))
				{
					if (m_PossibleMoveStars.ContainsKey(cachedMoveStar3))
					{
						ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar3, m_PossibleMoveStars[cachedMoveStar3]);
					}
					else if (cAbilityMove != null && cAbilityMove.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && m_PossibleMoveStarsRemainingInDirection.ContainsKey(cachedMoveStar3))
					{
						ResetTileStarFromPathMoveToPossiblePath(cachedMoveStar3, m_PossibleMoveStarsRemainingInDirection[cachedMoveStar3]);
					}
				}
			}
			ClearUnusedStars(m_TilesExpectedToDisplay);
			WaypointLine.s_Instance.m_UpdateFlag = true;
			if (Waypoint.GetLastWaypoint?.ClientTile == clientTile)
			{
				Choreographer.s_Choreographer.readyButton.ButtonComponent.Select();
			}
			else if (EventSystem.current.currentSelectedGameObject == Choreographer.s_Choreographer.readyButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.MovePathStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private static bool ShowAsHazardous(CClientTile tile, IEnumerable<CObjectTrap> unactivatedTraps)
	{
		if (tile?.m_Tile == null)
		{
			return false;
		}
		bool flag = false;
		CObjectProp cObjectProp = tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
		if (cObjectProp != null && unactivatedTraps.Contains(cObjectProp))
		{
			flag = true;
		}
		CNode cNode = ScenarioManager.PathFinder?.GetNode(tile.m_Tile.m_ArrayIndex);
		if (!flag && (tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainThorns) != null || tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainHotCoals) != null || (cNode != null && cNode.IsNegativeHex) || (cNode != null && cNode.IsSuperNegativeHex) || tile.m_Tile.TreatAsHazardous || ScenarioManager.CurrentScenarioState.GetNegativeHexes(GameState.CurrentActor).Contains(tile.m_Tile.m_ArrayIndex)))
		{
			flag = true;
		}
		return flag;
	}

	public static bool ShowAsDifficulTerrain(CTile tile)
	{
		if (tile == null)
		{
			return false;
		}
		if (ScenarioManager.PathFinder?.GetNode(tile.m_ArrayIndex)?.IsDifficultTerrain != true)
		{
			return false;
		}
		if (!tile.TreatAsDifficult && tile.FindProp(ScenarioManager.ObjectImportType.TerrainRubble) == null && !tile.IsWater)
		{
			if (tile.FindPropOfTypes(EPropType.TerrainVisualEffect) == null)
			{
				return tile.FindProp(ScenarioManager.ObjectImportType.VFXProp) != null;
			}
			return true;
		}
		return true;
	}

	public void ResetTileStarFromPathMoveToPossiblePath(CClientTile tile, IHexControl hexControl)
	{
		if (!m_PossibleMoveStars.ContainsKey(tile) && !m_PossibleMoveStarsRemainingInDirection.ContainsKey(tile))
		{
			return;
		}
		hexControl.Hover = false;
		if (m_ChestTiles.Contains(tile) || m_DoorTiles.Contains(tile))
		{
			if (m_ChestTiles.Contains(tile) && tile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
			{
				m_ChestTiles.Remove(tile);
				bool flag = hexControl.Type == IHexControl.HexType.NegativeEffect;
				hexControl.Mode = (flag ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Reach);
			}
			hexControl.Mode = m_MoveChestDoorPossibilityTemplate.m_Mode;
		}
		else
		{
			IEnumerable<CObjectTrap> unactivatedTraps = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			bool flag2 = ShowAsHazardous(tile, unactivatedTraps);
			bool flag3 = ShowAsDifficulTerrain(tile?.m_Tile);
			bool flag4 = IsDesignatedTileForCurrentAbility(tile);
			if (!flag4)
			{
				foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(Waypoint.s_MovingActor, CAbility.EAbilityType.AddCondition).ToList())
				{
					if (item.ActiveBonusIsActivatedByTile(tile.m_Tile))
					{
						flag4 = true;
						break;
					}
				}
			}
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles } cAbilityMove && cAbilityMove.CheckMustEndOnSpecificTiles(tile.m_Tile))
			{
				flag4 = true;
			}
			if (flag4)
			{
				hexControl.Mode = IHexControl.HexMode.PossibleTarget;
				hexControl.Type = IHexControl.HexType.ActiveBonusCondition;
			}
			else if (flag2)
			{
				hexControl.Mode = m_MoveHazardPossibilityTemplate.m_Mode;
				hexControl.Type = m_MoveHazardPossibilityTemplate.m_Type;
			}
			else if (flag3)
			{
				hexControl.Mode = m_DifficultTerrainPossibilityTemplate.m_Mode;
				hexControl.Type = m_DifficultTerrainPossibilityTemplate.m_Type;
			}
			else if (ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y].IsIceTerrain)
			{
				hexControl.Mode = m_IceTerrainPossibilityTemplate.m_Mode;
				hexControl.Type = m_IceTerrainPossibilityTemplate.m_Type;
			}
			else
			{
				hexControl.Mode = m_MovePossiblityTemplate.m_Mode;
				hexControl.Type = m_MovePossiblityTemplate.m_Type;
			}
		}
		hexControl.HexUpdate();
	}

	private void ResetTileStarFromAttackPathToPossibleAttackPath(CClientTile tile, IHexControl hexControl)
	{
		if (m_PossibleAttackPathStars.ContainsKey(tile))
		{
			hexControl.Hover = false;
			hexControl.Type = IHexControl.HexType.Move;
			hexControl.Mode = IHexControl.HexMode.Reach;
			hexControl.HexUpdate();
		}
	}

	private void ShowPossibleTargetSelectionHexes(CClientTile originTile = null, bool forceUseOriginTile = false)
	{
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.Normal:
			DisplayStandardAttackStars(originTile);
			break;
		case EAbilityDisplayType.AreaOfEffect:
			DisplayAOEStars(originTile, forceUseOriginTile);
			break;
		case EAbilityDisplayType.EnemyAreaOfEffect:
			DisplayEnemyAOEStars();
			break;
		case EAbilityDisplayType.ObjectiveAbility:
			DisplayTargetingAbilityStars(originTile, isObjective: true);
			break;
		case EAbilityDisplayType.TargetingAbility:
			DisplayTargetingAbilityStars(originTile);
			break;
		case EAbilityDisplayType.NegativeAbility:
			DisplayTargetingAbilityStars(originTile, isObjective: false, isPositive: false);
			break;
		case EAbilityDisplayType.SelectObjectPosition:
			DisplaySelectObjectPositionStars(originTile, m_SavedAbility?.IsPositive() ?? true);
			break;
		case EAbilityDisplayType.SelectObjectPositionNegative:
			DisplaySelectObjectPositionStars(originTile, isPositive: false);
			break;
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			DisplaySelectObjectPositionAOEStars(originTile, forceUseOriginTile);
			break;
		case EAbilityDisplayType.SelectPath:
			DisplayAttackPathStars(originTile);
			break;
		case EAbilityDisplayType.RedistributeDamageAbility:
			DisplayRedistributeDamageStars();
			break;
		}
	}

	private void GetAbilityValues(CAbility ability, ref int range, ref bool rangedAttack, ref bool includeCasterTile)
	{
		try
		{
			if (ability.IncludeCasterTile() || ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || ability.TargetingOverrideArrayIndex.X != 0 || ability.TargetingOverrideArrayIndex.Y != 0)
			{
				includeCasterTile = true;
			}
			else
			{
				includeCasterTile = false;
			}
			rangedAttack = ability.Range > 1;
			if (ability is CAbilityAttack)
			{
				rangedAttack = !(ability as CAbilityAttack).IsMeleeAttack;
			}
			else if (CurrentAbilityDisplayType == EAbilityDisplayType.AreaOfEffect && ability.AreaEffect != null && ability.AreaEffect.Melee)
			{
				rangedAttack = false;
			}
			range = ((ability.Range <= 1) ? 1 : ability.Range);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.GetAbilityValues().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public bool AlreadySelected(CClientTile clientTile)
	{
		if (clientTile?.m_Tile == null)
		{
			return false;
		}
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.AreaOfEffect:
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			if (m_SavedAbility != null)
			{
				CActor item = ScenarioManager.Scenario.FindActorAt(clientTile.m_Tile.m_ArrayIndex);
				if (!m_SavedAbility.ValidTilesInAreaEffectedIncludingBlocked.Contains(clientTile.m_Tile))
				{
					return m_SavedAbility.ActorsToTarget.Contains(item);
				}
				return true;
			}
			break;
		case EAbilityDisplayType.Normal:
		case EAbilityDisplayType.TargetingAbility:
		case EAbilityDisplayType.NegativeAbility:
		case EAbilityDisplayType.ObjectiveAbility:
			if (m_SavedAbility != null)
			{
				CActor actorOnTile = GameState.GetActorOnTile(clientTile.m_Tile, m_SavedAbility.TargetingActor, m_SavedAbility.AbilityFilter, m_SavedAbility.ActorsToIgnore, m_SavedAbility.IsTargetedAbility, false);
				return m_SavedAbility.ActorsToTarget.Contains(actorOnTile);
			}
			break;
		case EAbilityDisplayType.SelectObjectPosition:
		case EAbilityDisplayType.SelectObjectPositionNegative:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.TilesSelected.Contains(clientTile.m_Tile);
			}
			break;
		}
		return false;
	}

	public int CurrentNumberSelectedTargets()
	{
		switch (CurrentAbilityDisplayType)
		{
		case EAbilityDisplayType.Normal:
		case EAbilityDisplayType.AreaOfEffect:
		case EAbilityDisplayType.TargetingAbility:
		case EAbilityDisplayType.NegativeAbility:
		case EAbilityDisplayType.SelectPath:
		case EAbilityDisplayType.ObjectiveAbility:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.ActorsToTarget.Count;
			}
			break;
		case EAbilityDisplayType.SelectObjectPosition:
		case EAbilityDisplayType.SelectObjectPositionNegative:
		case EAbilityDisplayType.SelectObjectPositionAreaOfEffect:
			if (m_SavedAbility != null)
			{
				return m_SavedAbility.TilesSelected.Count;
			}
			break;
		}
		return 0;
	}

	private void DisplayStandardAttackStars(CClientTile originTile = null)
	{
		try
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				m_TilesExpectedToDisplay.Clear();
				m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
				for (int i = 0; i < m_EnemiesToRetaliate.Count; i++)
				{
					m_EnemiesToRetaliate[i].m_WorldspacePanelUI?.RetaliateWarnEffect(active: false, 0);
				}
				m_EnemiesToRetaliate.Clear();
				UpdateRetaliateDamage(0);
				if (Choreographer.s_Choreographer.m_CurrentActor == null)
				{
					return;
				}
				CAbility savedAbility = m_SavedAbility;
				bool flag = m_SavedAbility.AbilityType == CAbility.EAbilityType.Damage;
				bool flag2 = m_SavedAbility.AbilityType == CAbility.EAbilityType.Attack;
				if (savedAbility == null || savedAbility.TargetingActor.Type == CActor.EType.Enemy || savedAbility.TargetingActor.Type == CActor.EType.HeroSummon)
				{
					return;
				}
				int range = 1;
				bool includeCasterTile = false;
				bool rangedAttack = false;
				GetAbilityValues(savedAbility, ref range, ref rangedAttack, ref includeCasterTile);
				List<CClientTile> clientTiles;
				if (savedAbility.AllTargetsOnMovePath)
				{
					clientTiles = savedAbility.ValidActorsInRange.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
				}
				else if (savedAbility.AreaEffectBackup != null)
				{
					clientTiles = savedAbility.ActorsToTarget.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
					clientTiles.AddRange(from w in CurrentAbilityRangeTiles(savedAbility, range, includeCasterTile, rangedAttack)
						where !clientTiles.Contains(w)
						select w);
				}
				else
				{
					AbilityData.MiscAbilityData miscAbilityData = savedAbility.MiscAbilityData;
					if (miscAbilityData != null && miscAbilityData.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData2 = savedAbility.MiscAbilityData;
						if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.Value)
						{
							clientTiles = savedAbility.ValidActorsInRange.Select((CActor s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.ArrayIndex.X, s.ArrayIndex.Y]).ToList();
							clientTiles.AddRange(savedAbility.TilesInRange.Select((CTile s) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[s.m_ArrayIndex.X, s.m_ArrayIndex.Y]).ToList());
							goto IL_025b;
						}
					}
					clientTiles = CurrentAbilityRangeTiles(savedAbility, range, includeCasterTile, rangedAttack).ToList();
				}
				goto IL_025b;
				IL_025b:
				if (savedAbility.TargetingOverrideArrayIndex.X != 0 && savedAbility.TargetingOverrideArrayIndex.X != 0)
				{
					clientTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[savedAbility.TargetingOverrideArrayIndex.X, savedAbility.TargetingOverrideArrayIndex.Y]);
				}
				if (savedAbility.ActorsToTarget != null)
				{
					foreach (CTile item in savedAbility.ActorsToTarget.Select((CActor x) => x.Tile))
					{
						CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.GetClientTile(item.m_ArrayIndex);
						if (clientTile != null && !clientTiles.Contains(clientTile))
						{
							clientTiles.Add(clientTile);
						}
					}
				}
				List<Tuple<CActor, bool>> list = new List<Tuple<CActor, bool>>();
				foreach (CClientTile tile in clientTiles)
				{
					CActor cActor = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
					CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) =>
					{
						bool flag5 = false;
						if (e is CObjectActor)
						{
							CTile propTile = new CTile();
							CObjectProp attachedProp = ((CObjectActor)e).AttachedProp;
							if (attachedProp != null)
							{
								flag5 = CObjectProp.FindPropWithPathingBlocker(new Point(attachedProp.ArrayIndex.X, attachedProp.ArrayIndex.Y), ref propTile)?.PathingBlockers.Any((TileIndex p) => new Point(p.X, p.Y) == tile.m_Tile.m_ArrayIndex) ?? false;
							}
						}
						return (e.ArrayIndex == tile.m_Tile.m_ArrayIndex || (e.MultiHexArrayIndexes != null && e.MultiHexArrayIndexes.Contains(tile.m_Tile.m_ArrayIndex))) | flag5;
					});
					bool flag3 = savedAbility.ActorsToTarget.Contains(cEnemyActor) || savedAbility.ActorsToTarget.Contains(cActor);
					bool flag4 = tile == originTile;
					if (!flag4)
					{
						flag4 = GetOtherLinkedTiles(tile).Contains(originTile);
					}
					if (savedAbility.NumberTargets != 0 && savedAbility.MaxTargetsSelected() && !flag3 && tile != originTile)
					{
						continue;
					}
					IHexControl.HexMode hexMode;
					IHexControl.HexType hexType;
					if (savedAbility.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !flag)
					{
						hexMode = IHexControl.HexMode.PossibleTarget;
						hexType = IHexControl.HexType.PositiveEffect;
						if (flag3)
						{
							hexMode = IHexControl.HexMode.Selected;
							if (cEnemyActor != null)
							{
								list.Add(new Tuple<CActor, bool>(cEnemyActor, item2: true));
							}
							else if (cActor != null)
							{
								list.Add(new Tuple<CActor, bool>(cActor, item2: true));
							}
						}
					}
					else if (cEnemyActor != null || cActor != null)
					{
						hexMode = IHexControl.HexMode.PossibleTarget;
						hexType = IHexControl.HexType.NegativeEffect;
						if (flag3)
						{
							hexMode = IHexControl.HexMode.Selected;
							if (cEnemyActor != null)
							{
								list.Add(new Tuple<CActor, bool>(cEnemyActor, item2: false));
							}
							else if (cActor != null)
							{
								list.Add(new Tuple<CActor, bool>(cActor, item2: false));
							}
						}
					}
					else if (savedAbility.MaxTargetsSelected() & flag4)
					{
						hexMode = IHexControl.HexMode.Cursor;
						hexType = IHexControl.HexType.Move;
					}
					else
					{
						hexMode = IHexControl.HexMode.Reach;
						hexType = IHexControl.HexType.NegativeEffect;
					}
					m_TilesExpectedToDisplay.Add(tile);
					CClientTile tile2 = tile;
					IHexControl.HexType type = hexType;
					IHexControl.HexMode mode = hexMode;
					bool hover = flag4;
					Dictionary<CClientTile, IHexControl> attackStars = m_AttackStars;
					List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
					ModifyStar(tile2, type, mode, hover, attackStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
				}
				WorldspacePanelUIController.OnActorHexTargeted(null);
				DisplayStandardAttackHoverStars(out var totalRetaliate, originTile, flag);
				WorldspacePanelUIController.OnActorsHexTargeted(list, untargetOthers: false);
				WorldspacePanelUIController.OnActorsHexSelected(list.Select((Tuple<CActor, bool> it) => it.Item1).ToList());
				if (flag2)
				{
					for (int num = 0; num < list.Count; num++)
					{
						CActor actor = list[num].Item1;
						if (m_EnemiesToRetaliate.Exists((ActorBehaviour it) => it.Actor == actor))
						{
							continue;
						}
						List<Point> list2 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor, Choreographer.s_Choreographer.m_CurrentActor.RealArrayIndex, actor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
						if (!foundPath)
						{
							continue;
						}
						int num2 = actor.CalculateRetaliate(savedAbility, list2.Count);
						if (num2 > 0)
						{
							ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor));
							if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
							{
								m_EnemiesToRetaliate.Add(actorBehaviour);
								actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true, list2.Count);
							}
							totalRetaliate += num2;
						}
					}
				}
				if (flag2 && totalRetaliate > 0)
				{
					UpdateRetaliateDamage(totalRetaliate);
				}
				ClearUnusedStars(m_TilesExpectedToDisplay);
				SetHexBorders(m_AttackStars);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayStandardAttackHoverStars(out int totalRetaliate, CClientTile originTile = null, bool damageAbility = false)
	{
		totalRetaliate = 0;
		try
		{
			for (int i = 0; i < m_EnemiesToRetaliate.Count; i++)
			{
				m_EnemiesToRetaliate[i].m_WorldspacePanelUI?.RetaliateWarnEffect(active: false, 0);
			}
			m_EnemiesToRetaliate.Clear();
			CAbility savedAbility = m_SavedAbility;
			bool flag = savedAbility != null && savedAbility.AbilityType == CAbility.EAbilityType.Attack;
			TileBehaviour tileBehaviour = Interactable()?.GetComponent<TileBehaviour>();
			ControllerInputPointer.CursorType = ((!(InteractableUnderMouse()?.GetComponent<TileBehaviour>() == null)) ? ECursorType.Default : ECursorType.Invalid);
			CClientTile tile;
			if (FFSNetwork.IsClient)
			{
				if (originTile == null)
				{
					return;
				}
				tile = originTile;
			}
			else
			{
				if (tileBehaviour == null)
				{
					return;
				}
				tile = tileBehaviour.m_ClientTile;
			}
			if (tile == null || !m_AttackStars.ContainsKey(tile))
			{
				return;
			}
			IHexControl hexControl = m_AttackStars[tile];
			hexControl.Hover = true;
			CAbility savedAbility2 = m_SavedAbility;
			if (savedAbility2 == null)
			{
				return;
			}
			if (!savedAbility2.AllTargetsOnAttackPath)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
				WorldspacePanelUIController.OnActorHexTargeted(cEnemyActor, hexControl.Type == IHexControl.HexType.PositiveEffect);
				if (cEnemyActor == null)
				{
					return;
				}
				List<Point> list = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor, Choreographer.s_Choreographer.m_CurrentActor.RealArrayIndex, cEnemyActor.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
				if (!foundPath)
				{
					return;
				}
				ControllerInputPointer.CursorType = ECursorType.Targeted;
				int num = cEnemyActor.CalculateRetaliate(savedAbility2, list.Count);
				if (flag && num > 0)
				{
					ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor));
					if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
					{
						m_EnemiesToRetaliate.Add(actorBehaviour);
						actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true, list.Count);
					}
					UpdateRetaliateDamage(num);
				}
				totalRetaliate += num;
				if (m_SavedAbility is CAbilityAttack cAbilityAttack && cAbilityAttack.IsTargetOutsideOfInitialRange(cEnemyActor) && cAbilityAttack.AttackSummary.AddRangeActiveBonuses.FirstOrDefault((CActiveBonus it) => it.Ability.IsItemAbility)?.BaseCard is CItem cItem)
				{
					ShowHoveredActorTooltip(cEnemyActor, LocalizationManager.GetTranslation(cItem.Name), LocalizationManager.GetTranslation("Item_" + cItem.YMLData.Art.RemoveSpaces()));
				}
				return;
			}
			CEnemyActor cEnemyActor2 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == tile.m_Tile.m_ArrayIndex);
			if (cEnemyActor2 == null)
			{
				WorldspacePanelUIController.OnActorHexTargeted(cEnemyActor2);
				return;
			}
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor, Choreographer.s_Choreographer.m_CurrentActor.RealArrayIndex, cEnemyActor2.ArrayIndex, !damageAbility && ((CAbilityAttack)m_SavedAbility).IsMeleeAttack, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath2, m_SavedAbility?.IgnoreDoorLocks ?? false);
			if (!foundPath2)
			{
				return;
			}
			ControllerInputPointer.CursorType = ECursorType.Targeted;
			int num2 = 1;
			List<CEnemyActor> list3 = new List<CEnemyActor>();
			foreach (Point point in list2)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[point.X, point.Y];
				if (cClientTile == null)
				{
					return;
				}
				CEnemyActor cEnemyActor3 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor x) => x.ArrayIndex == point);
				m_TilesExpectedToDisplay.Add(cClientTile);
				int mode = ((cEnemyActor3 != null) ? 2 : 0);
				Dictionary<CClientTile, IHexControl> attackStars = m_AttackStars;
				List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
				ModifyStar(cClientTile, IHexControl.HexType.NegativeEffect, (IHexControl.HexMode)mode, hover: false, attackStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
				if (cEnemyActor3 != null)
				{
					list3.Add(cEnemyActor2);
					int num3 = cEnemyActor3.CalculateRetaliate(m_SavedAbility, num2);
					if (flag && num3 > 0)
					{
						ActorBehaviour actorBehaviour2 = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cEnemyActor3));
						if (actorBehaviour2 != null && actorBehaviour2.m_WorldspacePanelUI != null)
						{
							m_EnemiesToRetaliate.Add(actorBehaviour2);
							actorBehaviour2.m_WorldspacePanelUI.RetaliateWarnEffect(active: true, num2);
						}
					}
					totalRetaliate += num3;
				}
				num2++;
			}
			WorldspacePanelUIController.OnActorsHexTargeted(list3);
			if (flag && totalRetaliate > 0)
			{
				UpdateRetaliateDamage(totalRetaliate);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackHoverStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SetTargetTooltip(GameObject obj, Action onRemovedTarget = null)
	{
		if (m_TargetTooltip != obj)
		{
			m_OnRemoveTargetTooltip?.Invoke();
		}
		m_TargetTooltip = obj;
		m_OnRemoveTargetTooltip = onRemovedTarget;
	}

	private void ClearTargetTooltip()
	{
		SetTargetTooltip(null);
	}

	private void ShowHoveredActorTooltip(CActor actor, string tooltipTitle, string tooltipText)
	{
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(actor);
		if (m_TargetTooltip != gameObject)
		{
			ClearTargetTooltip();
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
		if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
		{
			actorBehaviour.m_WorldspacePanelUI.ShowInfoTooltip(tooltipTitle, tooltipText);
			SetTargetTooltip(gameObject, actorBehaviour.m_WorldspacePanelUI.HideInfoTooltip);
		}
	}

	private void ShowHoveredTileTooltip(CClientTile tile, string tooltipTitle, string tooltipText)
	{
		if (m_TargetTooltip != tile.m_TileBehaviour.gameObject)
		{
			ClearTargetTooltip();
		}
		tile.m_TileBehaviour.ShowTooltip(tooltipTitle, tooltipText);
		SetTargetTooltip(tile.m_TileBehaviour.gameObject, tile.m_TileBehaviour.HideTooltip);
	}

	public void SetAOELocked(bool locked)
	{
		m_AOELocked = locked;
		Log.NetworkInfo("AOE LOCK: " + locked);
	}

	public bool IsAOELocked()
	{
		return m_AOELocked;
	}

	private void DisplayEnemyAOEStars()
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			ClearStars();
			foreach (CTile item in m_SavedAbility.ValidTilesInAreaAffected)
			{
				CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (clientTile != null && !(clientTile.m_Tile.m_ArrayIndex == Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex))
				{
					bool flag = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor e) => e.ArrayIndex == clientTile.m_Tile.m_ArrayIndex) != null;
					CreateStar(clientTile, IHexControl.HexType.NegativeEffect, (!flag) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected, hover: false, m_AttackStars);
				}
			}
			SetHexBorders(m_AttackStars);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayEnemyAOEStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DisplayAOEStars(CClientTile originTile = null, bool forceUseOriginTile = false)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			m_TilesExpectedToDisplay.Clear();
			m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
			m_TilesExpectedToDisplay.AddRange(m_GenericStars.Keys);
			int range = 1;
			bool includeCasterTile = false;
			bool rangedAttack = false;
			CAbility savedAbility = m_SavedAbility;
			bool flag = savedAbility != null && savedAbility.AbilityType == CAbility.EAbilityType.Attack;
			GetAbilityValues(m_SavedAbility, ref range, ref rangedAttack, ref includeCasterTile);
			List<CClientTile> list = new List<CClientTile>();
			if (!m_SavedAbility.MaxTargetsSelected())
			{
				list.AddRange(CurrentAbilityRangeTiles(m_SavedAbility, range, rangedAttack, includeCasterTile));
				foreach (CActor item3 in m_SavedAbility.ValidActorsInRange.ToList())
				{
					CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item3.ArrayIndex.X, item3.ArrayIndex.Y];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			for (int i = 0; i < m_EnemiesToRetaliate.Count; i++)
			{
				m_EnemiesToRetaliate[i].m_WorldspacePanelUI?.RetaliateWarnEffect(active: false, 0);
			}
			m_EnemiesToRetaliate.Clear();
			UpdateRetaliateDamage(0);
			CClientTile cClientTile = ((originTile != null || forceUseOriginTile) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
				{
					cClientTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
				}
			}
			else
			{
				AutoTestController.LogAOETileHover(cClientTile);
			}
			if (m_SavedAbility.AreaEffect != null && !m_AOELocked)
			{
				m_SavedAbility.UpdateAreaEffect(cClientTile?.m_Tile, m_AreaEffectAngle, m_AreaEffectFlipped);
				if (m_SavedAbility is CAbilityHeal { TargetingActor: not null } cAbilityHeal && cAbilityHeal.TargetingActor.Type == CActor.EType.Player)
				{
					WorldspacePanelUIController.RefreshHealPreviews(cAbilityHeal, cAbilityHeal.TargetingActor);
				}
				else if (m_SavedAbility is CAbilityAttack cAbilityAttack)
				{
					WorldspacePanelUIController.PreviewAttack(cAbilityAttack, cAbilityAttack.TargetingActor);
				}
				else
				{
					WorldspacePanelUIController.PreviewEffects(m_SavedAbility);
				}
			}
			int num = 0;
			List<CTile> list2 = new List<CTile>();
			list2.AddRange(m_SavedAbility.ValidTilesInAreaEffectedIncludingBlocked);
			if (m_SavedAbility.ActorsToTarget != null)
			{
				foreach (CActor item4 in m_SavedAbility.ActorsToTarget.ToList())
				{
					CTile item2 = ScenarioManager.Tiles[item4.ArrayIndex.X, item4.ArrayIndex.Y];
					if (!list2.Contains(item2))
					{
						list2.Add(item2);
					}
				}
			}
			foreach (CTile item5 in list2)
			{
				CClientTile clientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item5.m_ArrayIndex.X, item5.m_ArrayIndex.Y];
				if (clientTile == null)
				{
					continue;
				}
				if (m_SavedAbility.AreaEffectLocked && m_SavedAbility.FlagsOfValidTiles.TryGetValue(clientTile.m_Tile, out var value) && value.HasFlag(CAreaEffect.EHexFlag.Space))
				{
					list.RemoveAll((CClientTile x) => x == clientTile);
				}
				else if (!list.Contains(clientTile))
				{
					list.Add(clientTile);
				}
			}
			List<CActor> list3 = new List<CActor>();
			List<CActor> list4 = new List<CActor>();
			List<CActor> list5 = ScenarioManager.Scenario.AllAliveActorsThreadSafe;
			if (m_SavedAbility?.AbilityFilter != null)
			{
				list5 = list5.Where((CActor enemyActor) => m_SavedAbility.AbilityFilter.IsValidTarget(enemyActor, m_SavedAbility.TargetingActor, m_SavedAbility.IsTargetedAbility, useTargetOriginalType: false, false)).ToList();
			}
			foreach (CClientTile clientTile2 in list)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(clientTile2.m_Tile.m_ArrayIndex);
				CActor cActor2 = list5.Find((CActor e) =>
				{
					bool flag5 = false;
					if (e is CObjectActor)
					{
						CObjectProp attachedProp = ((CObjectActor)e).AttachedProp;
						if (attachedProp != null)
						{
							CTile propTile = new CTile();
							flag5 = CObjectProp.FindPropWithPathingBlocker(new Point(attachedProp.ArrayIndex.X, attachedProp.ArrayIndex.Y), ref propTile)?.PathingBlockers.Any((TileIndex p) => new Point(p.X, p.Y) == clientTile2.m_Tile.m_ArrayIndex) ?? false;
						}
					}
					return (e.ArrayIndex == clientTile2.m_Tile.m_ArrayIndex || (e.MultiHexArrayIndexes != null && e.MultiHexArrayIndexes.Contains(clientTile2.m_Tile.m_ArrayIndex))) | flag5;
				});
				bool flag2 = cActor2 != null;
				if (cActor != null && !m_SavedAbility.IsValidSingleTargetActiveBonusTarget(cActor))
				{
					continue;
				}
				m_TilesExpectedToDisplay.Add(clientTile2);
				bool flag3 = m_AOELocked && clientTile2 == cClientTile;
				IHexControl.HexType hexType = IHexControl.HexType.NegativeEffect;
				bool flag4 = false;
				if (m_SavedAbility.FlagsOfValidTiles.ContainsKey(clientTile2.m_Tile))
				{
					CAreaEffect.EHexFlag eHexFlag = m_SavedAbility.FlagsOfValidTiles[clientTile2.m_Tile];
					if ((eHexFlag & CAreaEffect.EHexFlag.Space) != 0)
					{
						cActor = (cActor2 = null);
						flag2 = false;
						flag4 = true;
					}
					hexType = (((eHexFlag & CAreaEffect.EHexFlag.Ally) != 0) ? IHexControl.HexType.PositiveEffect : IHexControl.HexType.NegativeEffect);
				}
				IHexControl.HexMode hexMode;
				if (flag2)
				{
					hexMode = IHexControl.HexMode.PossibleTarget;
					if (m_AOELocked && list2.Contains(clientTile2.m_Tile) && m_SavedAbility.ActorsToTarget.Contains(cActor2))
					{
						hexMode = IHexControl.HexMode.Selected;
						list3.Add(cActor2);
						list4.Add(cActor2);
					}
					else if (flag3)
					{
						list3.Add(cActor2);
					}
				}
				else if (cActor != null)
				{
					hexMode = IHexControl.HexMode.PossibleTarget;
					if (m_AOELocked && list2.Contains(clientTile2.m_Tile) && m_SavedAbility.ActorsToTarget.Contains(cActor))
					{
						hexMode = IHexControl.HexMode.Selected;
						list3.Add(cActor);
						list4.Add(cActor);
					}
					else if (flag3)
					{
						list3.Add(cActor);
					}
				}
				else
				{
					hexMode = ((!flag4 && list2.Contains(clientTile2.m_Tile)) ? IHexControl.HexMode.SelectedAOEWithoutTarget : IHexControl.HexMode.Reach);
					if (m_AOELocked)
					{
						if (list2.Contains(clientTile2.m_Tile))
						{
							hexMode = IHexControl.HexMode.SelectedAOEWithoutTarget;
						}
						else if (clientTile2 == cClientTile)
						{
							hexType = IHexControl.HexType.Move;
							hexMode = IHexControl.HexMode.Cursor;
						}
					}
				}
				CClientTile tile = clientTile2;
				IHexControl.HexType type = hexType;
				IHexControl.HexMode mode = hexMode;
				bool hover = flag3;
				Dictionary<CClientTile, IHexControl> attackStars = m_AttackStars;
				List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
				ModifyStar(tile, type, mode, hover, attackStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
				if (!flag2)
				{
					continue;
				}
				List<Point> list6 = ScenarioManager.PathFinder.FindPath(Choreographer.s_Choreographer.m_CurrentActor, Choreographer.s_Choreographer.m_CurrentActor.RealArrayIndex, cActor2.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, m_SavedAbility?.IgnoreDoorLocks ?? false);
				if ((foundPath & flag) && m_SavedAbility.IsActorTargetedByAreaEffect(cActor2))
				{
					int num2 = cActor2.CalculateRetaliate(m_SavedAbility, list6.Count);
					if (num2 > 0)
					{
						ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(cActor2));
						if (actorBehaviour != null && actorBehaviour.m_WorldspacePanelUI != null)
						{
							m_EnemiesToRetaliate.Add(actorBehaviour);
							actorBehaviour.m_WorldspacePanelUI.RetaliateWarnEffect(active: true, list6.Count);
						}
					}
					num += num2;
				}
				if (num > 0)
				{
					UpdateRetaliateDamage(num);
				}
			}
			if (m_SavedAbility.AreaEffect?.Hexes?.Find((CAreaEffect.CAreaEffectHex h) =>
			{
				Point arrayIndex = h.ArrayIndex;
				return arrayIndex.X == 0 && arrayIndex.Y == 0 && h.Flags.HasFlag(CAreaEffect.EHexFlag.Ally);
			}) != null && m_SavedAbility.AbilityFilter.FilterAllyType(CAbilityFilter.EFilterAllyType.OwnSummon))
			{
				list3 = list3.FindAll((CActor a) => a is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == m_SavedAbility.TargetingActor);
			}
			List<CActor> validActors = m_SavedAbility.GetValidActorsInArea(m_SavedAbility.AllPossibleTilesInAreaEffect);
			WorldspacePanelUIController.OnActorsHexTargeted(list3.Where((CActor a) => validActors.Contains(a)).ToList(), m_SavedAbility.IsPositive());
			WorldspacePanelUIController.OnActorsHexSelected(list4);
			ClearUnusedStars(m_TilesExpectedToDisplay);
			List<CClientTile> hoveredTiles = (from s in m_AttackStars
				where s.Value.Hover
				select s.Key).ToList();
			SetHexBorders(m_AttackStars, hoveredTiles);
			ShowPossibleExitHexes();
			ShowGenericHexes();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void DisplaySelectObjectPositionAOEStars(CClientTile originTile = null, bool forceUseOriginTile = false)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			m_TilesExpectedToDisplay.Clear();
			m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
			int range = 1;
			bool includeCasterTile = false;
			bool rangedAttack = false;
			GetAbilityValues(m_SavedAbility, ref range, ref rangedAttack, ref includeCasterTile);
			List<CClientTile> list = CurrentAbilityRangeTiles(m_SavedAbility, range, rangedAttack, includeCasterTile).ToList();
			for (int i = 0; i < m_EnemiesToRetaliate.Count; i++)
			{
				m_EnemiesToRetaliate[i].m_WorldspacePanelUI?.RetaliateWarnEffect(active: false, 0);
			}
			m_EnemiesToRetaliate.Clear();
			UpdateRetaliateDamage(0);
			CClientTile cClientTile = ((originTile != null || forceUseOriginTile) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
				{
					cClientTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
				}
			}
			else
			{
				AutoTestController.LogAOETileHover(cClientTile);
			}
			if (m_SavedAbility.AreaEffect != null && !m_AOELocked)
			{
				m_SavedAbility.UpdateAreaEffect(cClientTile?.m_Tile, m_AreaEffectAngle, m_AreaEffectFlipped);
			}
			List<CTile> list2 = new List<CTile>();
			IHexControl.HexMode mode = ((!m_AOELocked) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
			foreach (CTile item in m_SavedAbility.ValidTilesInAreaAffected)
			{
				if (m_SavedAbility.SubAbilities.Count > 0)
				{
					foreach (CAbility subAbility in m_SavedAbility.SubAbilities)
					{
						if (subAbility is CAbilityAttack cAbilityAttack)
						{
							list2.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, cAbilityAttack.Range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false));
						}
					}
				}
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (cClientTile2 != null)
				{
					m_TilesExpectedToDisplay.Add(cClientTile2);
					Dictionary<CClientTile, IHexControl> attackStars = m_AttackStars;
					List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
					ModifyStar(cClientTile2, IHexControl.HexType.PositiveEffect, mode, hover: true, attackStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
				}
			}
			foreach (CClientTile item2 in list)
			{
				if (!m_TilesExpectedToDisplay.Contains(item2) && !m_SavedAbility.ValidTilesInAreaAffected.Contains(item2.m_Tile))
				{
					m_TilesExpectedToDisplay.Add(item2);
					bool flag = false;
					if (list2.Count > 0 && list2.Contains(item2.m_Tile) && ScenarioManager.Scenario.FindActorAt(item2.m_Tile.m_ArrayIndex) != null)
					{
						Dictionary<CClientTile, IHexControl> attackStars2 = m_AttackStars;
						List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
						ModifyStar(item2, IHexControl.HexType.NegativeEffect, mode, hover: false, attackStars2, null, checkLinkedTiles: true, tilesExpectedToDisplay);
						flag = true;
					}
					if (!flag)
					{
						Dictionary<CClientTile, IHexControl> attackStars3 = m_AttackStars;
						List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
						ModifyStar(item2, IHexControl.HexType.PositiveEffect, IHexControl.HexMode.Reach, hover: false, attackStars3, null, checkLinkedTiles: true, tilesExpectedToDisplay);
					}
				}
			}
			ClearUnusedStars(m_TilesExpectedToDisplay);
			SetHexBorders(m_AttackStars);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private bool ListenForTargetingInputEvents()
	{
		bool result = false;
		EAbilityDisplayType currentAbilityDisplayType = CurrentAbilityDisplayType;
		if ((currentAbilityDisplayType == EAbilityDisplayType.AreaOfEffect || currentAbilityDisplayType == EAbilityDisplayType.SelectObjectPositionAreaOfEffect) && m_SavedAbility != null)
		{
			result = ((m_SavedAbility.Range <= 1 && (m_SavedAbility.MiscAbilityData == null || !m_SavedAbility.MiscAbilityData.TreatAsRange.HasValue || !m_SavedAbility.MiscAbilityData.TreatAsRange.Value)) ? (RotateAOEWithMouse() || FlipAOE()) : RotateAOEWithHotkeys());
		}
		if (CurrentDisplayState == WorldSpaceStarDisplayState.LevelEditorSpawning)
		{
			result = RotateAOEWithHotkeys();
		}
		return result;
	}

	private void GetActorMousePositionOnGround(out Vector3 mousePos, out CClientTile actorTile, out CInteractable interactable, out bool overUI)
	{
		mousePos = Vector3.zero;
		actorTile = null;
		interactable = null;
		if (Choreographer.s_Choreographer?.m_CurrentActor != null)
		{
			actorTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y];
		}
		interactable = Interactable();
		overUI = UIManager.IsPointerOverUI;
		CClientTile cClientTile = interactable?.GetComponent<TileBehaviour>()?.m_ClientTile;
		if (AutoTestController.s_AutoLogPlaybackInProgress && AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOETileHover)
		{
			cClientTile = CAutoTileClick.TileIndexToClientTile((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoAOETileHover).PlacementTile);
		}
		if (cClientTile != null)
		{
			mousePos = cClientTile.m_GameObject.transform.position;
		}
		else if (actorTile != null && actorTile.m_GameObject != null)
		{
			Plane groundPlane = new Plane(Vector3.up, actorTile.m_GameObject.transform.position);
			mousePos = MF.FindGroundPointAtMousePosition(groundPlane);
			mousePos = MF.SnapVector3ToHexes(mousePos);
		}
	}

	private bool RotateAOEWithMouse()
	{
		try
		{
			if (!m_NewTilePointedAtThisFrame && !AutoTestController.s_AutoLogPlaybackInProgress)
			{
				return false;
			}
			GetActorMousePositionOnGround(out var mousePos, out var actorTile, out var _, out var _);
			if (actorTile == null || actorTile.m_GameObject == null)
			{
				return false;
			}
			Vector3 forward = mousePos - actorTile.m_GameObject.transform.position;
			forward.y = 0f;
			m_AreaEffectAngle = (int)Quaternion.LookRotation(forward).eulerAngles.y;
			m_AreaEffectAngle = 360 - m_AreaEffectAngle - 270;
			if (m_AreaEffectAngle < 0)
			{
				m_AreaEffectAngle = 360 + m_AreaEffectAngle;
			}
			m_AreaEffectAngle = m_AreaEffectAngle / 60 * 60;
			if (m_AreaEffectAngle != m_LastAreaEffectAngle)
			{
				Log.GUIInfo("New area Effect Angle:" + m_AreaEffectAngle);
				m_LastAreaEffectAngle = m_AreaEffectAngle;
				return true;
			}
			Log.GUIInfo("Same area Effect Angle:" + m_AreaEffectAngle);
			return false;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.RotateAOEWithMouse().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private bool RotateAOEWithHotkeys()
	{
		bool result = false;
		if (InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft.WasPressedThisFrame())
		{
			RotateAOEClockwise(turnRight: false);
			result = true;
		}
		if (InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight.WasPressedThisFrame())
		{
			RotateAOEClockwise(turnRight: true);
			result = true;
		}
		return result;
	}

	public void RotateAOEClockwise(bool turnRight)
	{
		try
		{
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				if (AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent is CAutoAOERotate cAutoAOERotate)
				{
					turnRight = cAutoAOERotate.IsClockWise;
				}
			}
			else
			{
				AutoTestController.LogAOERotate(turnRight);
			}
			if (m_LastRecievedRotateDirection + 0.3f < Timekeeper.instance.m_GlobalClock.time)
			{
				m_TurningRight = turnRight;
				m_LastRecievedRotateDirection = Timekeeper.instance.m_GlobalClock.time;
			}
			if (m_TurningRight)
			{
				m_AreaEffectAngle -= 60;
				if (m_AreaEffectAngle < 0)
				{
					if (CanFlipArea())
					{
						m_AreaEffectFlipped = !m_AreaEffectFlipped;
					}
					m_AreaEffectAngle += 360;
				}
				return;
			}
			m_AreaEffectAngle += 60;
			if (m_AreaEffectAngle >= 360)
			{
				if (CanFlipArea())
				{
					m_AreaEffectFlipped = !m_AreaEffectFlipped;
				}
				m_AreaEffectAngle -= 360;
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.RotateAOEClockwise().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public bool CanFlipArea()
	{
		if (LevelEditorController.s_Instance != null && LevelEditorController.s_Instance.CurrentState == LevelEditorController.ELevelEditorState.Editing && LevelEditorController.s_Instance.CurrentAreaEffectSelection != null)
		{
			return false;
		}
		return m_SavedAbility?.AreaEffect?.CanFlip ?? true;
	}

	private bool FlipAOE()
	{
		if (!CanFlipArea())
		{
			return false;
		}
		if (InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft.WasPressedThisFrame())
		{
			AutoTestController.LogAOERotate(clockwise: false);
			m_AreaEffectFlipped = !m_AreaEffectFlipped;
			return true;
		}
		if (InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight.WasPressedThisFrame())
		{
			AutoTestController.LogAOERotate(clockwise: true);
			m_AreaEffectFlipped = !m_AreaEffectFlipped;
			return true;
		}
		return false;
	}

	public void Clear()
	{
		try
		{
			ClearStars();
			ResetPlayerAttackType();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.Clear().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayTargetingAbilityStars(CClientTile originTile = null, bool isObjective = false, bool isPositive = true)
	{
		try
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				ClearStars();
				CAbility targetingAbility = m_SavedAbility;
				if (targetingAbility == null)
				{
					return;
				}
				List<CActor> list = new List<CActor>();
				List<CActor> list2 = new List<CActor>();
				CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
				List<CActor> list3 = new List<CActor>();
				if (!(targetingAbility is CAbilityTargeting { OneTargetAtATime: not false }) || targetingAbility.ActorsToTarget.Count <= 0)
				{
					list3.AddRange(targetingAbility.ValidActorsInRange);
				}
				list3.AddRange(targetingAbility.ActorsToTarget);
				list3 = list3.Distinct().ToList();
				list3.RemoveAll((CActor x) => targetingAbility.ActorsToIgnore.Contains(x));
				List<CActor> list4 = new List<CActor>();
				list4.AddRange(targetingAbility.ActorsToTarget);
				ControllerInputPointer.CursorType = ECursorType.Default;
				foreach (CActor item in list3)
				{
					CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.RealArrayIndex.X, item.RealArrayIndex.Y];
					bool flag = list4.Contains(item);
					bool flag2 = cClientTile != null && cClientTile2 == cClientTile;
					if (!flag2)
					{
						flag2 = GetOtherLinkedTiles(cClientTile2).Contains(cClientTile);
					}
					ModifyStar(cClientTile2, isObjective ? IHexControl.HexType.Objective : (isPositive ? IHexControl.HexType.PositiveEffect : IHexControl.HexType.NegativeEffect), (!flag) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected, flag2, m_AbilityStars);
					if (flag2 | flag)
					{
						list2.Add(item);
					}
					if (flag)
					{
						list.Add(item);
					}
					if (flag2 && targetingAbility is CAbilityAddDoom && item is CEnemyActor)
					{
						int count = item.CachedDoomActiveBonuses.Count;
						List<CActiveBonus> cachedAddDoomSlotActiveBonuses = Choreographer.s_Choreographer.m_CurrentActor.CachedAddDoomSlotActiveBonuses;
						if (Choreographer.s_Choreographer.m_CurrentActor.DoomSlots <= count)
						{
							string tooltipTitle = string.Join("\n\n", cachedAddDoomSlotActiveBonuses.Select((CActiveBonus it) => "<color=#" + UIInfoTools.DefInstance.mainColor.ToHex() + ">" + LocalizationManager.GetTranslation(it.BaseCard.Name) + "</color>\n" + ((it.Layout == null) ? string.Empty : ActivePropertyLookup(it.Ability, LocalisationAndPropertyLookup(it.Ability, it.Layout.ListLayouts.FirstOrDefault())))));
							ShowHoveredActorTooltip(item, tooltipTitle, null);
						}
					}
					if (flag2)
					{
						ControllerInputPointer.CursorType = ECursorType.Targeted;
					}
				}
				if (!targetingAbility.MaxTargetsSelected())
				{
					int range = ((targetingAbility.Targeting == CAbility.EAbilityTargeting.Range) ? targetingAbility.Range : 0);
					bool rangedAttack = targetingAbility.Targeting == CAbility.EAbilityTargeting.Range && targetingAbility.Range > 1;
					foreach (CClientTile item2 in CurrentAbilityRangeTiles(targetingAbility, range, includeCasterTile: true, rangedAttack).ToList())
					{
						if (!m_AbilityStars.ContainsKey(item2))
						{
							bool hover = cClientTile != null && item2 == cClientTile;
							ModifyStar(item2, isObjective ? IHexControl.HexType.Objective : (isPositive ? IHexControl.HexType.PositiveEffect : IHexControl.HexType.NegativeEffect), IHexControl.HexMode.Reach, hover, m_AbilityStars);
						}
					}
				}
				WorldspacePanelUIController.OnActorsHexTargeted(list2, isPositive);
				WorldspacePanelUIController.OnActorsHexSelected(list);
				SetHexBorders(m_AbilityStars);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayTargetingAbilityStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00014", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private string ActivePropertyLookup(CAbility ability, string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == ability.Name)
			{
				num += ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	private string LocalisationAndPropertyLookup(CAbility ability, string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', ability.Strength.ToString()) : desc);
		return desc;
	}

	private void AutoPreviewTiles(List<CTile> tiles, List<CClientTile> selectedTiles, bool isPositive, CClientTile mouseTile, bool forceHovered = true, IHexControl.HexMode mode = IHexControl.HexMode.Selected)
	{
		List<CActor> list = new List<CActor>();
		foreach (CTile tile in tiles)
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
			if (cClientTile != null && (m_SavedAbility == null || !m_SavedAbility.TilesSelected.Contains(tile)))
			{
				bool hover = forceHovered || mouseTile == cClientTile;
				ModifyStar(cClientTile, isPositive ? IHexControl.HexType.PositiveEffect : IHexControl.HexType.NegativeEffect, mode, hover, m_AbilityStars);
				selectedTiles.Add(cClientTile);
				CActor livingActor = tile.LivingActor;
				list.Add(livingActor);
			}
		}
		WorldspacePanelUIController.OnActorsHexTargeted(list);
		WorldspacePanelUIController.OnActorsHexSelected(list);
	}

	private void RemoveTileFilterTiles(List<CClientTile> tiles)
	{
		if (m_SavedAbility == null || tiles == null)
		{
			return;
		}
		TValue miscVar = m_SavedAbility.GetMiscVar("SpawnPreviewFilter");
		if (miscVar.isUnknown)
		{
			return;
		}
		if (Enum.TryParse<CAbilityFilter.EFilterTile>(miscVar.GetString(), out var spawnPreviewFilter))
		{
			tiles.RemoveAll((CClientTile x) => !CAbilityFilter.IsValidTile(x.m_Tile, spawnPreviewFilter));
		}
	}

	private void CheckForcePreviewTiles(IHexControl.HexMode mode, List<CClientTile> selectedTiles, CClientTile mouseTile, bool isPositive = true)
	{
		string text = "ForcePreviewTiles";
		if (mode != IHexControl.HexMode.Selected)
		{
			text += mode;
		}
		TValue var = m_SavedAbility.GetVar(text);
		if (var.isUnknown)
		{
			return;
		}
		List<object> list = var.GetObject<List<object>>();
		if (list != null)
		{
			AutoPreviewTiles(list.Select((object x) => x as CTile).ToList(), selectedTiles, isPositive, mouseTile, forceHovered: false, mode);
			return;
		}
		List<CTile> list2 = var.GetObject<List<CTile>>();
		if (list2 != null)
		{
			AutoPreviewTiles(list2, selectedTiles, isPositive, mouseTile, forceHovered: false, mode);
		}
	}

	private void DisplaySelectObjectPositionStars(CClientTile originTile = null, bool isPositive = true)
	{
		try
		{
			if (m_SavedAbility == null)
			{
				return;
			}
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				ClearStars();
				if (m_CachedAbilityRangeTiles.Count <= 0)
				{
					bool includeCasterTile = m_SavedAbility.IncludeCasterTile();
					CurrentAbilityRangeTiles(m_SavedAbility, m_SavedAbility.Range, includeCasterTile, rangedAttack: false, m_SavedAbility.TilesSelected);
				}
				CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
				List<CClientTile> list = new List<CClientTile>();
				foreach (CClientTile cachedAbilityRangeTile in m_CachedAbilityRangeTiles)
				{
					if (m_TileFilters != null)
					{
						bool flag = false;
						foreach (CAbilityFilter.EFilterTile tileFilter in m_TileFilters)
						{
							if (CAbilityFilter.IsValidTile(cachedAbilityRangeTile.m_Tile, tileFilter))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					CObjectProp cObjectProp = cachedAbilityRangeTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectProp != null && cObjectProp is CObjectDoor && (cObjectProp as CObjectDoor).IsDungeonEntrance)
					{
						continue;
					}
					bool flag2 = m_SavedAbility.TilesSelected?.Contains(cachedAbilityRangeTile.m_Tile) ?? false;
					if (!flag2)
					{
						lock (DisplayLock)
						{
							flag2 = GetOtherLinkedTiles(cachedAbilityRangeTile).Any((CClientTile x) => m_SavedAbility.TilesSelected?.Any((CTile y) => y == x.m_Tile) ?? false);
						}
					}
					bool flag3 = cClientTile != null && cClientTile == cachedAbilityRangeTile;
					if (!flag3)
					{
						lock (DisplayLock)
						{
							flag3 = GetOtherLinkedTiles(cachedAbilityRangeTile).Contains(cClientTile);
						}
					}
					if (flag3)
					{
						ControllerInputPointer.CursorType = ECursorType.Targeted;
					}
					bool flag4 = m_SavedAbility is CAbilityCreateLimitedHazardousTerrain { m_State: CAbilityCreateLimitedHazardousTerrain.CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions } || (m_SavedAbility is CAbilityCreateShadow cAbilityCreateShadow && cAbilityCreateShadow.m_State == CAbilityCreateShadow.CreateShadowState.RemoveShadowPositions);
					if (!m_SavedAbility.AllTargets || flag4 || flag2)
					{
						if (flag2)
						{
							list.Add(cachedAbilityRangeTile);
						}
						ModifyStar(cachedAbilityRangeTile, isPositive ? IHexControl.HexType.PositiveEffect : IHexControl.HexType.NegativeEffect, flag2 ? IHexControl.HexMode.Selected : IHexControl.HexMode.Reach, flag3, m_AbilityStars);
					}
				}
				TValue miscVar = m_SavedAbility.GetMiscVar("PreviewPath");
				if (miscVar != TValue.Unknown)
				{
					CVars cVars = new CVars();
					if (cClientTile?.m_Tile != null && m_CachedAbilityRangeTiles.Contains(cClientTile))
					{
						cVars.Set("MouseoverTile", new TValue(cClientTile.m_Tile));
						cVars.Set("SelectedTiles", new TValue(m_SavedAbility.TilesSelected));
						cVars.Set("Ability", new TValue(m_SavedAbility));
						CVarsContext context = new CVarsContext(cVars, m_SavedAbility.VarsContext);
						List<CTile> list2 = miscVar.GetObject<List<CTile>>(context);
						if (list2 != null)
						{
							AutoPreviewTiles(list2, list, isPositive, cClientTile);
						}
					}
				}
				foreach (IHexControl.HexMode value in Enum.GetValues(typeof(IHexControl.HexMode)))
				{
					CheckForcePreviewTiles(value, list, cClientTile, isPositive);
				}
				if (m_ObjectSpawnType == ScenarioManager.ObjectImportType.Shadow && m_SavedAbility is CAbilityMoveShadow { HightlightOriginTile: not false, SelectedShadowTile: not null } cAbilityMoveShadow)
				{
					CClientTile tile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cAbilityMoveShadow.SelectedShadowTile.m_ArrayIndex.X, cAbilityMoveShadow.SelectedShadowTile.m_ArrayIndex.Y];
					ModifyStar(tile, IHexControl.HexType.Move, IHexControl.HexMode.Selected, hover: false, m_AbilityStars);
				}
				if (m_ObjectSpawnType == ScenarioManager.ObjectImportType.HeroSummons)
				{
					CAbility savedAbility = m_SavedAbility;
					CAbilitySummon summonAbility = savedAbility as CAbilitySummon;
					if (summonAbility != null && (summonAbility.SelectedSummonYMLData != null || summonAbility.SelectedMonsterYMLData != null) && ((cClientTile != null && m_CachedAbilityRangeTiles.Contains(cClientTile)) || !list.IsNullOrEmpty()))
					{
						if (cClientTile != null && m_CachedAbilityRangeTiles.Contains(cClientTile) && list.Count < summonAbility.NumberTargets && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
						{
							list.Add(cClientTile);
						}
						RemoveTileFilterTiles(list);
						if (Choreographer.s_Choreographer.m_ClientPlacementPreviews.Count((KeyValuePair<string, GameObject> x) => x.Key.StartsWith(summonAbility.SelectedSummonID + "_")) > list.Count)
						{
							HidePreviewedCharacterPlacements();
						}
						int num = 0;
						foreach (CClientTile item in list)
						{
							string text = summonAbility.SelectedSummonID + "_" + num;
							num++;
							if (!Choreographer.s_Choreographer.m_ClientPlacementPreviews.ContainsKey(text))
							{
								Choreographer.s_Choreographer.m_ClientPlacementPreviews[text] = Choreographer.s_Choreographer.CreatePreviewPlayer(item, CActor.EType.HeroSummon, (summonAbility.SelectedSummonYMLData != null) ? summonAbility.SelectedSummonYMLData.Model : summonAbility.SelectedMonsterYMLData.Models[0]);
							}
							PreviewPlacement(text, item, Quaternion.identity);
						}
						goto IL_0b2c;
					}
				}
				if (m_ObjectSpawnType == ScenarioManager.ObjectImportType.Shadow && m_SavedAbility is CAbilityCreateShadow cAbilityCreateShadow2 && ((cClientTile != null && m_CurrentlyActiveStars.ContainsKey(cClientTile)) || !list.IsNullOrEmpty()))
				{
					if (cClientTile != null && m_CurrentlyActiveStars.ContainsKey(cClientTile) && list.Count < cAbilityCreateShadow2.NumberTargets && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
					{
						list.Add(cClientTile);
					}
					RemoveTileFilterTiles(list);
					int num2 = 0;
					foreach (CClientTile item2 in list)
					{
						string text2 = "Shadow_" + num2;
						num2++;
						if (!Choreographer.s_Choreographer.m_ClientPlacementPreviews.ContainsKey(text2))
						{
							GameObject propPrefab = GlobalSettings.GetPropPrefab(m_SavedAbility.SpawnPropPreview);
							if (propPrefab == null)
							{
								Log.Error("Unable to find prop: " + EPropType.Shadow);
								break;
							}
							GameObject gameObject = UnityEngine.Object.Instantiate(propPrefab, item2.m_GameObject.transform.parent);
							UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
							if (component == null)
							{
								Log.Error("No UnityGameEditorObject script found on Trap");
								UnityEngine.Object.Destroy(gameObject);
								break;
							}
							component.m_ForPreview = true;
							gameObject.name = text2;
							gameObject.transform.position = item2.m_GameObject.transform.position;
							Choreographer.s_Choreographer.m_ClientPlacementPreviews[text2] = gameObject;
						}
						PreviewPlacement(text2, item2, Quaternion.identity);
					}
				}
				else if (m_ObjectSpawnType != ScenarioManager.ObjectImportType.None)
				{
					int num3 = 0;
					GameObject gameObject2 = GlobalSettings.GetPropPrefab(m_SavedAbility.SpawnPropPreview);
					if (gameObject2 != null)
					{
						BakedProp component2 = gameObject2.GetComponent<BakedProp>();
						if (component2 != null)
						{
							gameObject2 = component2.bakedProp;
						}
						if (cClientTile != null && m_CurrentlyActiveStars.ContainsKey(cClientTile) && list.Count < m_SavedAbility.NumberTargets && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
						{
							list.Add(cClientTile);
						}
						RemoveTileFilterTiles(list);
						string propKey = m_ObjectSpawnType.ToString();
						if (Choreographer.s_Choreographer.m_ClientPlacementPreviews.Count((KeyValuePair<string, GameObject> x) => x.Key.Contains(propKey)) > list.Count)
						{
							HidePreviewedCharacterPlacements();
						}
						foreach (CClientTile item3 in list)
						{
							string text3 = $"{propKey}_{gameObject2.name}_{num3}";
							num3++;
							if (!Choreographer.s_Choreographer.m_ClientPlacementPreviews.ContainsKey(text3))
							{
								GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject2, item3.m_GameObject.transform.parent);
								if (component2 == null)
								{
									UnityGameEditorObject component3 = gameObject3.GetComponent<UnityGameEditorObject>();
									if (component3 == null)
									{
										Log.Error("No UnityGameEditorObject script found on " + gameObject3.name);
										UnityEngine.Object.Destroy(gameObject3);
										break;
									}
									component3.m_ForPreview = true;
								}
								Collider component4 = gameObject3.GetComponent<Collider>();
								if (component4 != null)
								{
									component4.enabled = false;
								}
								gameObject3.name = text3;
								gameObject3.transform.position = item3.m_GameObject.transform.position;
								Choreographer.s_Choreographer.m_ClientPlacementPreviews[text3] = gameObject3;
							}
							PreviewPlacement(text3, item3, Quaternion.identity);
						}
					}
					else
					{
						HidePreviewedCharacterPlacements();
					}
				}
				else if (m_SavedAbility.GetMiscVar("SpawnActorPreviewID") != TValue.Unknown && ((cClientTile != null && m_CachedAbilityRangeTiles.Contains(cClientTile)) || !list.IsNullOrEmpty()))
				{
					string spawnActorID = m_SavedAbility.GetMiscVar("SpawnActorPreviewID").GetString();
					MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == spawnActorID);
					if (cClientTile != null && m_CachedAbilityRangeTiles.Contains(cClientTile) && list.Count < m_SavedAbility.NumberTargets && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
					{
						list.Add(cClientTile);
					}
					RemoveTileFilterTiles(list);
					if (Choreographer.s_Choreographer.m_ClientPlacementPreviews.Count((KeyValuePair<string, GameObject> x) => x.Key.StartsWith(spawnActorID + "_")) > list.Count)
					{
						HidePreviewedCharacterPlacements();
					}
					if (monsterYMLData != null)
					{
						int num4 = 0;
						foreach (CClientTile item4 in list)
						{
							string text4 = spawnActorID + "_" + num4;
							num4++;
							if (!Choreographer.s_Choreographer.m_ClientPlacementPreviews.ContainsKey(text4))
							{
								Choreographer.s_Choreographer.m_ClientPlacementPreviews[text4] = Choreographer.s_Choreographer.CreatePreviewPlayer(item4, CActor.EType.Ally, monsterYMLData.Models[0]);
							}
							PreviewPlacement(text4, item4, Quaternion.identity);
						}
					}
				}
				else
				{
					HidePreviewedCharacterPlacements();
				}
				goto IL_0b2c;
				IL_0b2c:
				SetHexBorders(m_AbilityStars);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplaySelectObjectPositionStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00015", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void PreviewPlacement(string actor, CClientTile tile, Quaternion rotation, int bodyIndex = -1)
	{
		if (string.IsNullOrEmpty(actor) || tile == null || tile.m_GameObject == null)
		{
			return;
		}
		Choreographer.s_Choreographer.m_ClientPlacementPreviews.TryGetValue(actor, out var value);
		if (value != null)
		{
			MF.EnableRenderers(value, enable: true);
			value.SetActive(value: true);
			CharacterManager.GetCharacterManager(value)?.OnBeforeForcedMovement();
			value.transform.position = tile.m_GameObject.transform.position;
			value.transform.rotation = rotation;
			previewedCharacterPlacements.Add(value);
			CharacterManager.GetCharacterManager(value)?.OnAfterForcedMovement();
			if (bodyIndex == -1)
			{
				return;
			}
			GameObject[] array = ActorBehaviour.TryFindMutliBodies(value);
			if (array != null && array.Length > 1)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(i == bodyIndex);
				}
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Trying to display preview for \"" + actor + "\", but no preview found for it.");
		}
	}

	private void ShowPossibleAttackPathStars()
	{
		try
		{
			ClearStars();
			Waypoint.ClearValidSelectionTiles();
			if (Choreographer.s_Choreographer.m_CurrentAbility == null || !(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack))
			{
				return;
			}
			CAbilityAttack cAbilityAttack = Choreographer.s_Choreographer.m_CurrentAbility as CAbilityAttack;
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			int remainingWaterTilesThatCostZero = 0;
			int remainingMoves = (Waypoint.s_HasWaypoints ? Waypoint.GetLastWaypoint.MovesRemaining : cAbilityAttack.Range);
			Point startingPoint = (Waypoint.s_HasWaypoints ? Waypoint.GetLastWaypoint.ClientTile.m_Tile.m_ArrayIndex : Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex);
			CActor.EType type = Choreographer.s_Choreographer.m_CurrentActor.Type;
			List<CClientTile> list = new List<CClientTile>();
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
					if (cClientTile != null)
					{
						list.Add(cClientTile);
					}
				}
			}
			PossibleMoveTileStarCreation(list, ignoreBlocked: true, currentActor, remainingMoves, remainingWaterTilesThatCostZero, null, startingPoint, type, m_PossibleAttackPathStars, null, null, LOS: true, () =>
			{
				SetHexBorders(m_PossibleAttackPathStars);
			});
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.ShowPossibleAttackPathStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00016", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void DisplayAttackPathStars(CClientTile originTile = null)
	{
		try
		{
			if (!(Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack cAbilityAttack))
			{
				return;
			}
			m_TilesExpectedToDisplay.Clear();
			m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
			m_TilesExpectedToDisplay.AddRange(m_PossibleAttackPathStars.Keys);
			m_CachedMoveStars.Clear();
			m_CachedMoveStars.AddRange(m_PathAttackStars.Keys);
			m_PathAttackStars.Clear();
			while (WaypointLine.s_Instance.m_Transform.childCount > 0)
			{
				ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
			}
			WaypointLine.s_Instance.m_UpdateFlag = true;
			Waypoint.ClearValidSelectionTiles();
			CActor currentActor = Choreographer.s_Choreographer.m_CurrentActor;
			if (Choreographer.s_Choreographer.FindClientActorGameObject(currentActor) == null)
			{
				return;
			}
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			List<Point> list = new List<Point>();
			list.Add(currentActor.ArrayIndex);
			list.AddRange(Waypoint.s_Waypoints.Select((GameObject w) => w.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex).ToList());
			int num = ((Waypoint.s_Waypoints.Count > 0) ? Waypoint.s_Waypoints[Waypoint.s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : cAbilityAttack.Range);
			if (cClientTile != null && m_PossibleAttackPathStars.ContainsKey(cClientTile) && num > 0)
			{
				list.Add(cClientTile.m_Tile.m_ArrayIndex);
			}
			if (list.Count < 2)
			{
				foreach (CClientTile cachedMoveStar in m_CachedMoveStars)
				{
					if (m_PossibleAttackPathStars.ContainsKey(cachedMoveStar))
					{
						ResetTileStarFromAttackPathToPossibleAttackPath(cachedMoveStar, m_PossibleAttackPathStars[cachedMoveStar]);
					}
				}
				return;
			}
			List<Point> list2 = new List<Point>();
			for (int num2 = 0; num2 < list.Count - 1; num2++)
			{
				list2.AddRange(CActor.FindCharacterPath(currentActor, list[num2], list[num2 + 1], ignoreBlocked: true, ignoreMoveCost: true, out var _));
			}
			Vector3 position = Choreographer.s_Choreographer.FindClientActorGameObject(currentActor).transform.position;
			ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>()?.EnableType(WaypointHolder.EWaypointType.Start);
			foreach (Point item in list2)
			{
				CClientTile waypointTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.X, item.Y];
				bool flag = false;
				if (!m_PossibleAttackPathStars.ContainsKey(waypointTile))
				{
					flag = true;
				}
				if (flag)
				{
					if (m_ChestTiles.Contains(waypointTile) || m_DoorTiles.Contains(waypointTile))
					{
						if (m_ChestTiles.Contains(waypointTile) && waypointTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) == null)
						{
							m_ChestTiles.Remove(waypointTile);
						}
						CClientTile tile = waypointTile;
						IHexControl.HexType type = m_MoveChestDoorPossibilityTemplate.m_Type;
						bool hover = m_MoveChestDoorPossibilityTemplate.m_Hover;
						Dictionary<CClientTile, IHexControl> possibleAttackPathStars = m_PossibleAttackPathStars;
						List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
						ModifyStar(tile, type, IHexControl.HexMode.Reach, hover, possibleAttackPathStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
						m_TilesExpectedToDisplay.Add(waypointTile);
						m_PossibleAttackPathStars[waypointTile].RefreshHexUI();
					}
					else
					{
						CClientTile tile2 = waypointTile;
						Dictionary<CClientTile, IHexControl> possibleAttackPathStars2 = m_PossibleAttackPathStars;
						List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
						ModifyStar(tile2, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false, possibleAttackPathStars2, null, checkLinkedTiles: true, tilesExpectedToDisplay);
						m_TilesExpectedToDisplay.Add(waypointTile);
						m_PossibleAttackPathStars[waypointTile].RefreshHexUI();
					}
				}
				IHexControl hexControl = null;
				if (m_PossibleAttackPathStars.ContainsKey(waypointTile))
				{
					hexControl = m_PossibleAttackPathStars[waypointTile];
				}
				if (hexControl == null)
				{
					return;
				}
				bool flag2 = ScenarioManager.Scenario.AllEnemyMonstersAndObjects.Find((CEnemyActor e) => e.ArrayIndex == waypointTile.m_Tile.m_ArrayIndex) != null;
				hexControl.Type = (flag2 ? IHexControl.HexType.NegativeEffect : IHexControl.HexType.NegativeEffect);
				hexControl.Mode = ((!flag2) ? IHexControl.HexMode.PossibleTarget : IHexControl.HexMode.Selected);
				hexControl.Hover = waypointTile == cClientTile;
				hexControl.RefreshHexUI();
				if (!m_PathAttackStars.ContainsKey(waypointTile))
				{
					m_PathAttackStars.Add(waypointTile, hexControl);
				}
				WaypointHolder component = ObjectPool.Spawn(GlobalSettings.Instance.m_WaypointHolder, WaypointLine.s_Instance.m_Transform, waypointTile.m_GameObject.transform.position + WaypointHolder.YOffset, Quaternion.identity).GetComponent<WaypointHolder>();
				if (item == currentActor.ArrayIndex)
				{
					component?.EnableType(WaypointHolder.EWaypointType.Start);
				}
				else if (list.Contains(item))
				{
					component?.EnableType(WaypointHolder.EWaypointType.Node);
				}
				else
				{
					component?.EnableType(WaypointHolder.EWaypointType.Small);
				}
				Waypoint.AddValidSelectionTiles(waypointTile.m_Tile.m_ArrayIndex);
			}
			foreach (CClientTile cachedMoveStar2 in m_CachedMoveStars)
			{
				if (!m_PathAttackStars.ContainsKey(cachedMoveStar2) && m_PossibleAttackPathStars.ContainsKey(cachedMoveStar2))
				{
					ResetTileStarFromAttackPathToPossibleAttackPath(cachedMoveStar2, m_PossibleAttackPathStars[cachedMoveStar2]);
				}
			}
			ClearUnusedStars(m_TilesExpectedToDisplay);
			WaypointLine.s_Instance.m_UpdateFlag = true;
			if (Waypoint.GetLastWaypoint?.ClientTile == cClientTile)
			{
				Choreographer.s_Choreographer.readyButton.ButtonComponent.Select();
			}
			else if (EventSystem.current.currentSelectedGameObject == Choreographer.s_Choreographer.readyButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayAttackPathStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00017", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void StoreDistributeDamageService(DistributeDamageService distributeDamageService)
	{
		m_SavedDistributeDamageService = distributeDamageService;
	}

	private void SetDisplayRedistributeDamageInitialState()
	{
		CShowRedistributeDamageUI_MessageData cShowRedistributeDamageUI_MessageData = (Choreographer.s_Choreographer.LastMessage as CShowRedistributeDamageUI_MessageData) ?? m_SavedRedistributeDamageUIMessage;
		if (cShowRedistributeDamageUI_MessageData != null)
		{
			m_SavedRedistributeDamageUIMessage = cShowRedistributeDamageUI_MessageData;
		}
	}

	private void DisplayRedistributeDamageStars()
	{
		try
		{
			m_TilesExpectedToDisplay.Clear();
			m_TilesExpectedToDisplay.AddRange(m_DungeonExitStars.Keys);
			if (Choreographer.s_Choreographer.m_CurrentActor == null)
			{
				return;
			}
			SetDisplayRedistributeDamageInitialState();
			CAbility redistributeDamageAbility = m_SavedRedistributeDamageUIMessage.m_RedistributeDamageAbility;
			if (redistributeDamageAbility == null || redistributeDamageAbility.TargetingActor.Type == CActor.EType.Enemy || redistributeDamageAbility.TargetingActor.Type == CActor.EType.HeroSummon)
			{
				return;
			}
			List<Tuple<CActor, bool>> list = new List<Tuple<CActor, bool>>();
			foreach (CActor item in m_SavedRedistributeDamageUIMessage.m_ActorsToRedistributeBetween)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.ArrayIndex.X, item.ArrayIndex.Y];
				IHexControl.HexMode hexMode = IHexControl.HexMode.PossibleTarget;
				IHexControl.HexType hexType = IHexControl.HexType.Move;
				int assignedPoints = m_SavedDistributeDamageService.GetAssignedPoints(item);
				if (assignedPoints < 0)
				{
					hexMode = IHexControl.HexMode.Selected;
					hexType = IHexControl.HexType.NegativeEffect;
					list.Add(new Tuple<CActor, bool>(item, item2: false));
				}
				else if (assignedPoints > 0)
				{
					hexMode = IHexControl.HexMode.Selected;
					hexType = IHexControl.HexType.PositiveEffect;
					list.Add(new Tuple<CActor, bool>(item, item2: true));
				}
				m_TilesExpectedToDisplay.Add(cClientTile);
				IHexControl.HexType type = hexType;
				IHexControl.HexMode mode = hexMode;
				Dictionary<CClientTile, IHexControl> attackStars = m_AttackStars;
				List<CClientTile> tilesExpectedToDisplay = m_TilesExpectedToDisplay;
				ModifyStar(cClientTile, type, mode, hover: false, attackStars, null, checkLinkedTiles: true, tilesExpectedToDisplay);
			}
			ClearUnusedStars(m_TilesExpectedToDisplay);
			SetHexBorders(m_AttackStars);
			WorldspacePanelUIController.OnActorsHexTargeted(list, true);
			WorldspacePanelUIController.OnActorsHexSelected(list.Select((Tuple<CActor, bool> it) => it.Item1).ToList());
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayStandardAttackStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private GameObject CreateStar(CClientTile tile, IHexControl.HexType type, IHexControl.HexMode mode, bool hover, Dictionary<CClientTile, IHexControl> container, Color? color = null)
	{
		try
		{
			if (tile == null || tile.m_GameObject == null)
			{
				return null;
			}
			GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_GenericHexStar, tile.m_GameObject.transform);
			if (gameObject != null)
			{
				gameObject.name = $"{GlobalSettings.Instance.m_GenericHexStar.name}_{tile}";
				IHexControl component = gameObject.GetComponent<IHexControl>();
				component.Type = type;
				component.Mode = mode;
				component.Hover = hover;
				component.ColorOverride = color;
				if (!container.ContainsKey(tile))
				{
					container.Add(tile, component);
				}
				container[tile] = component;
				if (!m_CurrentlyActiveStars.ContainsKey(tile))
				{
					m_CurrentlyActiveStars.Add(tile, container);
				}
				SetStarPos(gameObject, tile);
				return gameObject;
			}
			throw new Exception("Generic hex star was not spawned");
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.CreateStar().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00018", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			throw;
		}
	}

	private IEnumerable<CTile> GetLinkedTiles(CTile tile)
	{
		if (m_SavedAbility == null)
		{
			yield break;
		}
		foreach (CTile linkedTile in m_SavedAbility.GetLinkedTiles(tile))
		{
			yield return linkedTile;
		}
	}

	private IEnumerable<CClientTile> GetOtherLinkedTiles(CClientTile tile)
	{
		return (from x in GetLinkedTiles(tile.m_Tile)
			select ClientScenarioManager.s_ClientScenarioManager.GetClientTile(x.m_ArrayIndex)).Distinct();
	}

	private GameObject SynchronizedModifyStar(CClientTile tile, IHexControl.HexType type, IHexControl.HexMode mode, bool hover, Dictionary<CClientTile, IHexControl> container, Color? color = null, bool checkLinkedTiles = true, List<CClientTile> additionalModifiedStars = null)
	{
		return ModifyStar(tile, type, mode, hover, container, color, checkLinkedTiles, additionalModifiedStars, startInactive: true);
	}

	private GameObject ModifyStar(CClientTile tile, IHexControl.HexType type, IHexControl.HexMode mode, bool hover, Dictionary<CClientTile, IHexControl> container, Color? color = null, bool checkLinkedTiles = true, List<CClientTile> additionalModifiedStars = null, bool startInactive = false)
	{
		try
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				if (checkLinkedTiles)
				{
					foreach (CClientTile otherLinkedTile in GetOtherLinkedTiles(tile))
					{
						ModifyStar(otherLinkedTile, type, mode, hover, container, color, checkLinkedTiles: false, null, startInactive);
						additionalModifiedStars?.Add(otherLinkedTile);
					}
				}
				GameObject gameObject;
				if (m_CurrentlyActiveStars.ContainsKey(tile))
				{
					Dictionary<CClientTile, IHexControl> dictionary = m_CurrentlyActiveStars[tile];
					if (dictionary != null && dictionary == container)
					{
						IHexControl hexControl = container[tile];
						hexControl.Type = type;
						hexControl.Mode = mode;
						hexControl.Hover = hover;
						hexControl.ColorOverride = color;
						gameObject = hexControl.GameObject;
					}
					else
					{
						GameObject objInstance = dictionary[tile].GameObject;
						dictionary.Remove(tile);
						m_CurrentlyActiveStars.Remove(tile);
						ObjectPool.Recycle(objInstance, GlobalSettings.Instance.m_GenericHexStar);
						gameObject = ModifyStar(tile, type, mode, hover, container, color, checkLinkedTiles: true, null, startInactive);
					}
				}
				else
				{
					gameObject = CreateStar(tile, type, mode, hover, container, color);
				}
				if ((gameObject != null) & startInactive)
				{
					gameObject.SetActive(value: false);
				}
				return gameObject;
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.ModifyStar().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00019", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return null;
	}

	private void SetHexBorders(Dictionary<CClientTile, IHexControl> tileStars, List<CClientTile> hoveredTiles = null)
	{
		try
		{
			HashSet<CClientTile> activeTiles = new HashSet<CClientTile>(tileStars.Keys);
			foreach (KeyValuePair<CClientTile, IHexControl> tileStar in tileStars)
			{
				IHexControl value = tileStar.Value;
				value.GameObject.SetActive(value: true);
				value.SetNeighbors(tileStar.Key, activeTiles, hoveredTiles);
				value.RefreshHexUI(forceUpdate: false);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.SetHexBorders().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00020", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RefreshHexesWithAllBorders(Dictionary<CClientTile, IHexControl> tileStars)
	{
		try
		{
			new HashSet<CClientTile>();
			foreach (KeyValuePair<CClientTile, IHexControl> tileStar in tileStars)
			{
				IHexControl value = tileStar.Value;
				value.SetAllNeighborsOn();
				value.RefreshHexUI(forceUpdate: false);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.RefreshHexesWithAllBorders().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00021", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void SetStarPos(GameObject starGameObject, CClientTile tile)
	{
		starGameObject.transform.SetParent(ClientScenarioManager.s_ClientScenarioManager.m_Board.transform);
		Vector3 position = new Vector3(tile.m_GameObject.transform.position.x, tile.m_GameObject.transform.position.y, tile.m_GameObject.transform.position.z);
		starGameObject.transform.position = position;
	}

	public void ClearStars(bool possibleMove = true, bool pathMove = true, bool attack = true, bool attackPath = true, bool possibleAttackPath = true, bool ability = true, bool placement = true, bool placementSelection = true, bool allHighlightedTiles = true, bool impossibleMoveChestDoor = true, bool possibleMoveDirection = true, bool cursorHighlightedTiles = true, bool exitDungeon = false, bool genericTiles = false)
	{
		try
		{
			s_DebugPathPoints.Clear();
			if (m_SavedAbility == null || m_SavedAbility.IsCurrentlyTargetingActors())
			{
				WorldspacePanelUIController.OnActorsHexSelected();
				WorldspacePanelUIController.OnActorHexTargeted(null);
			}
			if (pathMove)
			{
				foreach (KeyValuePair<CClientTile, IHexControl> pathMoveStar in m_PathMoveStars)
				{
					if (pathMoveStar.Value != null)
					{
						ResetTileStarFromPathMoveToPossiblePath(pathMoveStar.Key, pathMoveStar.Value);
					}
				}
				m_PathMoveStars.Clear();
				while (WaypointLine.s_Instance.m_Transform.childCount > 0)
				{
					ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
				}
				WaypointLine.s_Instance.m_UpdateFlag = true;
			}
			if (possibleMove)
			{
				RemoveSubsetFromAllActiveList(m_PossibleMoveStars);
				foreach (KeyValuePair<CClientTile, IHexControl> possibleMoveStar in m_PossibleMoveStars)
				{
					if (possibleMoveStar.Value != null)
					{
						ObjectPool.Recycle(possibleMoveStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_PossibleMoveStars.Clear();
			}
			if (possibleMoveDirection)
			{
				RemoveSubsetFromAllActiveList(m_PossibleMoveStarsRemainingInDirection);
				foreach (KeyValuePair<CClientTile, IHexControl> item in m_PossibleMoveStarsRemainingInDirection)
				{
					if (item.Value != null)
					{
						ObjectPool.Recycle(item.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_PossibleMoveStarsRemainingInDirection.Clear();
			}
			if (attack)
			{
				RemoveSubsetFromAllActiveList(m_AttackStars);
				foreach (KeyValuePair<CClientTile, IHexControl> attackStar in m_AttackStars)
				{
					if (attackStar.Value != null)
					{
						ObjectPool.Recycle(attackStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_AttackStars.Clear();
			}
			if (attackPath)
			{
				RemoveSubsetFromAllActiveList(m_PathAttackStars);
				foreach (KeyValuePair<CClientTile, IHexControl> pathAttackStar in m_PathAttackStars)
				{
					if (pathAttackStar.Value != null)
					{
						ResetTileStarFromAttackPathToPossibleAttackPath(pathAttackStar.Key, pathAttackStar.Value);
					}
				}
				m_PathAttackStars.Clear();
				while (WaypointLine.s_Instance.m_Transform.childCount > 0)
				{
					ObjectPool.Recycle(WaypointLine.s_Instance.m_Transform.GetChild(0).gameObject, GlobalSettings.Instance.m_WaypointHolder);
				}
				WaypointLine.s_Instance.m_UpdateFlag = true;
			}
			if (possibleAttackPath)
			{
				RemoveSubsetFromAllActiveList(m_PossibleAttackPathStars);
				foreach (KeyValuePair<CClientTile, IHexControl> possibleAttackPathStar in m_PossibleAttackPathStars)
				{
					if (possibleAttackPathStar.Value != null)
					{
						ObjectPool.Recycle(possibleAttackPathStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_PossibleAttackPathStars.Clear();
			}
			if (ability)
			{
				RemoveSubsetFromAllActiveList(m_AbilityStars);
				foreach (KeyValuePair<CClientTile, IHexControl> abilityStar in m_AbilityStars)
				{
					if (abilityStar.Value != null)
					{
						ObjectPool.Recycle(abilityStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_AbilityStars.Clear();
			}
			if (placement)
			{
				RemoveSubsetFromAllActiveList(m_PlacementStars);
				foreach (KeyValuePair<CClientTile, IHexControl> placementStar in m_PlacementStars)
				{
					if (placementStar.Value != null)
					{
						ObjectPool.Recycle(placementStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_PlacementStars.Clear();
			}
			if (placementSelection)
			{
				if (m_SelectedPlacementStars != null && m_PlacementStars.ContainsKey(m_SelectedPlacementStars))
				{
					m_PlacementStars[m_SelectedPlacementStars].Hover = false;
				}
				m_SelectedPlacementStars = null;
			}
			if (allHighlightedTiles)
			{
				RemoveSubsetFromAllActiveList(m_HighlightAllStars);
				foreach (KeyValuePair<CClientTile, IHexControl> highlightAllStar in m_HighlightAllStars)
				{
					if (highlightAllStar.Value != null)
					{
						ObjectPool.Recycle(highlightAllStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_HighlightAllStars.Clear();
			}
			if (impossibleMoveChestDoor)
			{
				RemoveSubsetFromAllActiveList(m_ImpossibleMoveChestDoorStars);
				foreach (KeyValuePair<CClientTile, IHexControl> impossibleMoveChestDoorStar in m_ImpossibleMoveChestDoorStars)
				{
					if (impossibleMoveChestDoorStar.Value != null)
					{
						ObjectPool.Recycle(impossibleMoveChestDoorStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_ImpossibleMoveChestDoorStars.Clear();
			}
			if (exitDungeon)
			{
				RemoveSubsetFromAllActiveList(m_DungeonExitStars);
				foreach (KeyValuePair<CClientTile, IHexControl> dungeonExitStar in m_DungeonExitStars)
				{
					if (dungeonExitStar.Value != null)
					{
						ObjectPool.Recycle(dungeonExitStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
					}
				}
				m_DungeonExitStars.Clear();
			}
			if (!genericTiles)
			{
				return;
			}
			RemoveSubsetFromAllActiveList(m_GenericStars);
			foreach (KeyValuePair<CClientTile, IHexControl> genericStar in m_GenericStars)
			{
				if (genericStar.Value != null)
				{
					ObjectPool.Recycle(genericStar.Value.GameObject, GlobalSettings.Instance.m_GenericHexStar);
				}
			}
			m_GenericStars.Clear();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.ClearStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00013", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void RemoveSubsetFromAllActiveList(Dictionary<CClientTile, IHexControl> dictionaryToRemove)
	{
		foreach (CClientTile key in dictionaryToRemove.Keys)
		{
			m_CurrentlyActiveStars.Remove(key);
		}
	}

	private void ClearUnusedStars(List<CClientTile> tilesThatShouldRemain)
	{
		List<CClientTile> list = new List<CClientTile>();
		foreach (CClientTile key2 in m_CurrentlyActiveStars.Keys)
		{
			if (!tilesThatShouldRemain.Contains(key2) && !m_GenericStars.ContainsKey(key2))
			{
				list.Add(key2);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			CClientTile key = list[i];
			Dictionary<CClientTile, IHexControl> dictionary = m_CurrentlyActiveStars[key];
			GameObject objInstance = dictionary[key].GameObject;
			dictionary.Remove(key);
			m_CurrentlyActiveStars.Remove(key);
			ObjectPool.Recycle(objInstance, GlobalSettings.Instance.m_GenericHexStar);
		}
	}

	public void DisplayCursorHoverStar()
	{
		CInteractable cInteractable = Interactable();
		if (InteractableUnderMouse()?.GetComponent<TileBehaviour>()?.m_ClientTile == null)
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
		}
		if (cInteractable == null)
		{
			WorldspaceUITools.Instance.DisableAllHoveredOutlines();
			return;
		}
		TileBehaviour component = cInteractable.GetComponent<TileBehaviour>();
		if (component == null || component.m_ClientTile == null)
		{
			WorldspaceUITools.Instance.DisableAllHoveredOutlines();
			return;
		}
		CClientTile clientTile = component.m_ClientTile;
		if (clientTile.m_Tile.m_HexMap != null && !clientTile.m_Tile.m_HexMap.RevealedAndNotDestroyed && (clientTile.m_Tile.m_Hex2Map == null || !clientTile.m_Tile.m_Hex2Map.RevealedAndNotDestroyed))
		{
			ControllerInputPointer.CursorType = ECursorType.Invalid;
			if (!CanCursorHighlightUnrevealedTile(clientTile))
			{
				m_CursorHighlightedTile = null;
				Singleton<UIHexInfoPanel>.Instance?.Hide();
				Singleton<ActorStatPanel>.Instance?.HideForActor();
				Singleton<UIPropInfoPanel>.Instance?.Hide();
				Singleton<UITextInfoPanel>.Instance?.Hide();
				if (m_CursorHighlightedStar != null)
				{
					m_CursorHighlightedStar.GameObject.SetActive(value: false);
				}
			}
			else if (m_CursorHighlightedTile != clientTile)
			{
				m_CursorHighlightedTile = clientTile;
				ShowTooltipForTile(clientTile);
			}
			return;
		}
		bool flag = CanCursorHighlightTile(clientTile);
		if (m_CursorHighlightedStar == null)
		{
			IHexControl component2 = ObjectPool.Spawn(GlobalSettings.Instance.m_GenericHexStar, clientTile.m_GameObject.transform).GetComponent<IHexControl>();
			component2.Type = IHexControl.HexType.Move;
			component2.Mode = IHexControl.HexMode.Cursor;
			component2.RefreshHexUI();
			component2.Hover = false;
			m_CursorHighlightedStar = component2;
		}
		if (m_CursorHighlightedTile != clientTile || m_CursorHighlightedStar.GameObject.activeSelf != flag)
		{
			SetStarPos(m_CursorHighlightedStar.GameObject, clientTile);
			m_CursorHighlightedStar.Type = IHexControl.HexType.Move;
			m_CursorHighlightedStar.Mode = IHexControl.HexMode.Cursor;
			m_CursorHighlightedStar.RefreshHexUI();
			m_CursorHighlightedStar.Hover = false;
			m_CursorHighlightedStar.ColorOverride = null;
			m_CursorHighlightedTile = clientTile;
			m_CursorHighlightedStar.GameObject.SetActive(flag);
			ShowTooltipForTile(clientTile);
			foreach (GameObject link in _links)
			{
				ObjectPool.Recycle(link, GlobalSettings.Instance.m_NavLinkLineObject);
			}
			_links.Clear();
			foreach (var (cClientTile, cClientTile2) in ClientScenarioManager.s_ClientScenarioManager.GenericHighlightedLinks)
			{
				if (cClientTile == clientTile || cClientTile2 == clientTile)
				{
					GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_NavLinkLineObject, cClientTile.m_GameObject.transform);
					gameObject.GetComponent<ParableTesselator>().Tesselate(cClientTile.m_GameObject.transform.position, cClientTile2.m_GameObject.transform.position);
					_links.Add(gameObject);
				}
			}
		}
		if (ControllerInputPointer.CursorType == ECursorType.Invalid)
		{
			ControllerInputPointer.CursorType = ECursorType.Default;
		}
	}

	private bool CanCursorHighlightTile(CClientTile tile)
	{
		if (CameraController.s_CameraController.m_DragStarted)
		{
			return false;
		}
		if (m_CurrentlyActiveStars.ContainsKey(tile))
		{
			return false;
		}
		CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
		if (cNode == null)
		{
			return false;
		}
		if (!cNode.Walkable)
		{
			return false;
		}
		return true;
	}

	private bool CanCursorHighlightUnrevealedTile(CClientTile tile)
	{
		return ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Contains(tile);
	}

	private void ShowTooltipForTile(CClientTile tile)
	{
		WorldspaceUITools.Instance.DisableAllHoveredOutlines();
		HexViewService.HexInfoParams hexInfoParams = HexViewService.GenerateHexInfoParams(tile.m_Tile);
		CActor actorOnTile = hexInfoParams.ActorOnTile;
		string text = string.Empty;
		string text2 = null;
		string text3 = null;
		ScenarioManager.ObjectImportType objectImportType = ScenarioManager.ObjectImportType.None;
		int num = 0;
		List<CObjectProp> list = tile.m_Tile.m_Props;
		CSpawner cSpawner = null;
		foreach (CObjectProp item in list)
		{
			if (item.PropHealthDetails != null && !string.IsNullOrEmpty(item.PropHealthDetails.CustomLocKey) && !(item is CObjectObstacle))
			{
				text = LocalizationManager.GetTranslation(item.PropHealthDetails.CustomLocKey);
			}
			if (item.CanLootLocKey.IsNOTNullOrEmpty())
			{
				text3 = string.Format(LocalizationManager.GetTranslation("GUI_PROP_CAN_BE_LOOTED_BY"), LocalizationManager.GetTranslation(item.CanLootLocKey));
			}
			if (item.PropHealthDetails != null && item.PropHealthDetails.CurrentHealth > 0 && item.PropActorHasBeenAssigned)
			{
				EnableObjectPropOutline(item);
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.MoneyToken)
			{
				num++;
				continue;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Chest)
			{
				text = LocalizationManager.GetTranslation("TREASURE_CHEST_TOOLTIP");
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.GoalChest)
			{
				string text4 = ((!string.IsNullOrEmpty(item.PropCustomLocKey)) ? item.PropCustomLocKey : item.PropHealthDetails?.CustomLocKey);
				text = LocalizationManager.GetTranslation((!text4.IsNullOrEmpty()) ? text4 : "GOAL_CHEST_TOOLTIP");
				objectImportType = item.ObjectType;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Obstacle)
			{
				text = HexViewService.GetObstacleTooltipTitle(item);
				text2 = (item.OverrideDisallowDestroyAndMove ? LocalizationManager.GetTranslation("IMMOVABLE_OBSTACLE_DESCR_TOOLTIP") : null);
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Door)
			{
				CObjectDoor cObjectDoor = (CObjectDoor)item;
				if (cObjectDoor.DoorIsOpen)
				{
					text = LocalizationManager.GetTranslation("DOORWAY_TOOLTIP");
				}
				else
				{
					text = (cObjectDoor.DoorIsLocked ? LocalizationManager.GetTranslation("LOCKED_DOOR_TOOLTIP") : LocalizationManager.GetTranslation("CLOSED_DOOR_TOOLTIP"));
				}
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.PressurePlate)
			{
				objectImportType = item.ObjectType;
				text = LocalizationManager.GetTranslation("PRESSURE_PLATE_TOOLTIP");
				text2 = null;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.TerrainIce)
			{
				objectImportType = item.ObjectType;
				text = LocalizationManager.GetTranslation("ICE_TERRAIN_TOOLTIP");
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Portal)
			{
				objectImportType = item.ObjectType;
				text = LocalizationManager.GetTranslation("PORTAL_TOOLTIP");
				text2 = LocalizationManager.GetTranslation("PORTAL_DESCR_TOOLTIP");
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				objectImportType = item.ObjectType;
				text = LocalizationManager.GetTranslation(FHUILocFormatting.GetScenarioQuestItemName(item));
				text2 = LocalizationManager.GetTranslation(FHUILocFormatting.GetScenarioQuestItemDesc(item));
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Resource)
			{
				CObjectResource cObjectResource = (CObjectResource)item;
				objectImportType = item.ObjectType;
				text = LocalizationManager.GetTranslation(cObjectResource.ResourceData.ID);
				foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
				{
					string text5 = scenarioScript.PropKeyOverride(item, out var formatParameters);
					if (!string.IsNullOrEmpty(text5))
					{
						text = LocalizationManager.GetTranslation(text5);
						if (formatParameters != null)
						{
							string format = text;
							object[] args = formatParameters;
							text = string.Format(format, args);
						}
					}
				}
				text2 = LocalizationManager.GetTranslation(cObjectResource.ResourceData.ID + "_DESCR_TOOLTIP");
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.Shadow)
			{
				text = LocalizationManager.GetTranslation("SHADOW_TOOLTIP");
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.VFXProp && !string.IsNullOrEmpty(item.PropCustomLocKey))
			{
				text = LocalizationManager.GetTranslation(item.PropCustomLocKey);
				if (item.CustomLocKeyArgs != null)
				{
					text = string.Format(text, item.CustomLocKeyArgs.ToArray());
				}
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType == ScenarioManager.ObjectImportType.TerrainVisualEffect && !string.IsNullOrEmpty(item.PropCustomLocKey))
			{
				text = (string.IsNullOrEmpty(item.PropCustomLocKey) ? LocalizationManager.GetTranslation("ICE_TERRAIN_TOOLTIP") : LocalizationManager.GetTranslation(item.PropCustomLocKey));
				if (item.CustomLocKeyArgs != null)
				{
					text = string.Format(text, item.CustomLocKeyArgs.ToArray());
				}
				if (!string.IsNullOrEmpty(item.PropCustomDescriptionLocKey))
				{
					text2 = LocalizationManager.GetTranslation(item.PropCustomDescriptionLocKey);
				}
				objectImportType = item.ObjectType;
				break;
			}
			if (item.ObjectType != ScenarioManager.ObjectImportType.TerrainRubble || !ShowAsDifficulTerrain(tile.m_Tile))
			{
				continue;
			}
			string text6 = ((!string.IsNullOrEmpty(item.PropCustomLocKey)) ? item.PropCustomLocKey : item.PropHealthDetails?.CustomLocKey);
			text = LocalizationManager.GetTranslation((!text6.IsNullOrEmpty()) ? text6 : "DIFFICULT_TERRAIN_TOOLTIP");
			objectImportType = ScenarioManager.ObjectImportType.TerrainRubble;
			break;
		}
		if (objectImportType == ScenarioManager.ObjectImportType.None && ClientScenarioManager.s_ClientScenarioManager.DungeonExitTiles.Contains(tile))
		{
			text = LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP");
			text2 = LocalizationManager.GetTranslation("GUI_Exit_Tile");
			objectImportType = ScenarioManager.ObjectImportType.Door;
		}
		List<CObjective> list2 = ScenarioManager.CurrentScenarioState.AllObjectives.Where((CObjective x) =>
		{
			if (x.CustomTileHoverLocKey.IsNOTNullOrEmpty())
			{
				EObjectiveType objectiveType = x.ObjectiveType;
				return objectiveType == EObjectiveType.ActorReachPosition || objectiveType == EObjectiveType.BringItemsToArea || objectiveType == EObjectiveType.ActorsEscaped || objectiveType == EObjectiveType.AnyActorReachPosition;
			}
			return false;
		}).ToList();
		if (list2.Any())
		{
			foreach (CObjective item2 in list2)
			{
				if (!item2.CustomTileHoverLocKey.IsNullOrEmpty())
				{
					List<TileIndex> list3 = null;
					if (item2 is CObjective_ActorReachPosition cObjective_ActorReachPosition)
					{
						list3 = cObjective_ActorReachPosition.ActorTargetPositions;
					}
					if (item2 is CObjective_ActorsEscaped cObjective_ActorsEscaped)
					{
						list3 = cObjective_ActorsEscaped.EscapePositions;
					}
					if (item2 is CObjective_BringItemsToArea cObjective_BringItemsToArea)
					{
						list3 = cObjective_BringItemsToArea.AreaPositions;
					}
					if (item2 is CObjective_AnyActorReachPosition cObjective_AnyActorReachPosition)
					{
						list3 = cObjective_AnyActorReachPosition.ActorTargetPositions;
					}
					if (list3 != null && list3.FirstOrDefault((TileIndex x) => x.X == tile.m_Tile.m_ArrayIndex.X && x.Y == tile.m_Tile.m_ArrayIndex.Y) != null)
					{
						text = LocalizationManager.GetTranslation("DUNGEON_EXIT_TOOLTIP");
						text2 = LocalizationManager.GetTranslation(item2.CustomTileHoverLocKey);
						objectImportType = ScenarioManager.ObjectImportType.Door;
					}
				}
			}
		}
		if (objectImportType == ScenarioManager.ObjectImportType.None)
		{
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode != null && cNode.Blocked)
			{
				List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile.m_Tile);
				for (int num2 = 0; num2 < allAdjacentTiles.Count; num2++)
				{
					if (allAdjacentTiles[num2].m_Props.Count > 0 && allAdjacentTiles[num2].FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle cObjectObstacle)
					{
						text = HexViewService.GetObstacleTooltipTitle(cObjectObstacle);
						objectImportType = ScenarioManager.ObjectImportType.Obstacle;
						if (cObjectObstacle.PathingBlockers.Any((TileIndex b) => b.X == tile.m_Tile.m_ArrayIndex.X && b.Y == tile.m_Tile.m_ArrayIndex.Y))
						{
							list = new List<CObjectProp>(list);
							list.Add(cObjectObstacle);
						}
					}
				}
			}
			foreach (CSpawner spawner in tile.m_Tile.m_Spawners)
			{
				if (spawner.SpawnerData.SpawnerHoverNameLoc.IsNOTNullOrEmpty())
				{
					string text7 = spawner.SpawnerData.SpawnerHoverNameLoc;
					string text8 = string.Empty;
					if (text7.Contains('|'))
					{
						string[] array = text7.Split('|');
						if (array.Length == 2)
						{
							text7 = array[0];
							text8 = array[1];
						}
					}
					if (text7.IsNOTNullOrEmpty())
					{
						text = LocalizationManager.GetTranslation(text7);
					}
					if (text8.IsNOTNullOrEmpty())
					{
						text2 = LocalizationManager.GetTranslation(text8);
					}
					objectImportType = ((spawner.Prop == null) ? ScenarioManager.ObjectImportType.Spawner : spawner.Prop.ObjectType);
				}
				else if (spawner.Prop != null)
				{
					cSpawner = spawner;
				}
			}
		}
		if (num > 0)
		{
			string text9 = LocalizationManager.GetTranslation("LOOT_TOKEN_TOOLTIP");
			if (num > 1)
			{
				text9 = text9 + " x" + num;
			}
			text = ((objectImportType == ScenarioManager.ObjectImportType.None) ? text9 : (text + " & " + text9));
		}
		SaveData instance = SaveData.Instance;
		if ((object)instance != null && instance.Global?.GameMode == EGameMode.LevelEditor)
		{
			return;
		}
		Singleton<UIHexInfoPanel>.Instance?.Hide();
		Singleton<ActorStatPanel>.Instance?.HideForActor();
		Singleton<UIPropInfoPanel>.Instance?.Hide(UIPropInfoPanel.EPropType.QuestItem, UIPropInfoPanel.EPropType.DifficultTerrain);
		Singleton<UITextInfoPanel>.Instance?.Hide();
		foreach (CObjectProp item3 in list)
		{
			EnableObjectPropOutline(item3);
		}
		Singleton<UIHexInfoPanel>.Instance?.Show(hexInfoParams.PropInfos);
		if (actorOnTile != null && !actorOnTile.Deactivated && !actorOnTile.SkipVisualsAndGUI)
		{
			Singleton<ActorStatPanel>.Instance?.Show(actorOnTile);
			WorldspaceUITools.Instance.EnableHoveredOutline(actorOnTile);
			if (m_SavedAbility != null && m_savedDisplayState == WorldSpaceStarDisplayState.TargetSelection)
			{
				UIContextualTutorialModule.OnEvent("HighlightActorDuringAbility", actorOnTile, m_SavedAbility);
			}
		}
		else if (cSpawner != null)
		{
			Singleton<ActorStatPanel>.Instance?.Show(cSpawner);
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (text3.IsNOTNullOrEmpty())
			{
				text2 = ((!text2.IsNullOrEmpty()) ? (text2 + "\n" + text3) : text3);
			}
			switch (objectImportType)
			{
			case ScenarioManager.ObjectImportType.TerrainRubble:
				Singleton<UIPropInfoPanel>.Instance?.ShowDifficultTerrain(text);
				break;
			case ScenarioManager.ObjectImportType.CarryableQuestItem:
				Singleton<UIPropInfoPanel>.Instance?.ShowQuestItem(text, text2);
				break;
			default:
				Singleton<UITextInfoPanel>.Instance?.Show(text, text2);
				break;
			}
		}
	}

	public static void EnableObjectPropOutline(CObjectProp prop, bool ProcessLinkedProps = true)
	{
		if (prop == null)
		{
			return;
		}
		GameObject gameObject = Choreographer.FindPropGameObject(prop);
		if (gameObject != null)
		{
			List<Outlinable> list = gameObject.GetComponentsInChildren<Outlinable>().ToList();
			if (list != null)
			{
				foreach (Outlinable item in list)
				{
					if (!OutlineWrapper.OutlinableIsObstacle(item))
					{
						WorldspaceUITools.Instance.EnableHoveredOutline(item);
					}
				}
			}
		}
		if (!ProcessLinkedProps)
		{
			return;
		}
		if (prop is CObjectDoor { TwinDoors: var twinDoors })
		{
			for (int i = 0; i < twinDoors.Length; i++)
			{
				EnableObjectPropOutline(twinDoors[i], ProcessLinkedProps: false);
			}
		}
		if (!prop.HasLinkedProps)
		{
			return;
		}
		foreach (CObjectProp item2 in prop.GetLinkedPropsTotal())
		{
			EnableObjectPropOutline(item2, ProcessLinkedProps: false);
		}
	}

	public void ActorIsSelectingDoor(bool active)
	{
		m_SelectingADoor = active;
	}

	public void ActorIsOpeningDoor(bool active)
	{
		m_OpeningDoor = active;
	}

	public void ClearNonTargetHexHighlights(List<CActor> actors = null, List<CTile> tiles = null, List<CClientTile> clientTiles = null, bool removeHover = true)
	{
		List<CClientTile> list = null;
		if (list == null && actors != null)
		{
			list = actors.Select((CActor a) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[a.RealArrayIndex.X, a.RealArrayIndex.Y]).ToList();
			foreach (CActor actor in actors)
			{
				if (actor is CObjectActor)
				{
					CObjectActor cObjectActor = (CObjectActor)actor;
					if (cObjectActor.AttachedProp is CObjectObstacle)
					{
						list.AddRange(((CObjectObstacle)cObjectActor.AttachedProp).PathingBlockers.Select((TileIndex p) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[p.X, p.Y]).ToList());
					}
				}
				if (actor.MultiHexArrayIndexes != null)
				{
					list.AddRange(actor.MultiHexArrayIndexes.Select((Point p) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[p.X, p.Y]).ToList());
				}
			}
		}
		if (list == null && tiles != null)
		{
			list = tiles.Select((CTile t) => ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[t.m_ArrayIndex.X, t.m_ArrayIndex.Y]).ToList();
		}
		list = list ?? clientTiles.ToList();
		if (list == null)
		{
			return;
		}
		LockView = true;
		list.AddRange(m_DungeonExitStars.Keys);
		ClearUnusedStars(list);
		foreach (KeyValuePair<CClientTile, Dictionary<CClientTile, IHexControl>> currentlyActiveStar in m_CurrentlyActiveStars)
		{
			IHexControl hexControl = currentlyActiveStar.Value[currentlyActiveStar.Key];
			hexControl.Hover = false;
			hexControl.HexUpdate();
		}
	}

	public void ResetPlayerAttackType()
	{
		if (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.InvalidAttack && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.ActorSelectedAttackFocus && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.PlayerSelectedTile && CurrentAbilityDisplayType != EAbilityDisplayType.None)
		{
			if (InputFHManager.Controls.IsEnabledBy(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft))
			{
				InputFHManager.Controls.Disable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetLeft);
				InputFHManager.Controls.Disable(this, InputFHManager.Controls.ScenarioWorldspace.RotateTargetRight);
			}
			CurrentAbilityDisplayType = EAbilityDisplayType.None;
			LockView = false;
			m_SavedAbility = null;
			m_CachedAbilityRangeTiles.Clear();
			m_SavedRedistributeDamageUIMessage = null;
			m_SavedDistributeDamageService = null;
			HidePreviewedCharacterPlacements();
		}
	}

	private void ControlPlayerEndMovement()
	{
		if (Choreographer.s_Choreographer.m_CurrentActor != null && Choreographer.s_Choreographer.m_CurrentActor.Type == CActor.EType.Player)
		{
			if (Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActorHasMoved)
			{
				m_EndingMovement = true;
				m_SelectingADoor = false;
			}
			if (Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActionSelection)
			{
				m_EndingMovement = false;
				m_OpeningDoor = false;
				m_SelectingADoor = false;
			}
			if (m_EndingMovement && !m_OpeningDoor && Choreographer.s_Choreographer.LastMessage.m_Type == CMessageData.MessageType.ActorIsSelectingMoveTile)
			{
				m_EndingMovement = false;
				m_OpeningDoor = false;
			}
		}
	}

	private List<CClientTile> CurrentAbilityRangeTiles(CAbility ability, int range = 1, bool includeCasterTile = false, bool rangedAttack = false, List<CTile> selectedTiles = null)
	{
		try
		{
			if (m_CachedAbilityRangeTiles.Count > 0)
			{
				return m_CachedAbilityRangeTiles;
			}
			m_CachedAbilityRangeTiles.Clear();
			if (ability.TargetingActor == null)
			{
				return m_CachedAbilityRangeTiles;
			}
			new List<CTile>();
			List<string> list = new List<string> { "PreviewTiles" };
			foreach (IHexControl.HexMode value in Enum.GetValues(typeof(IHexControl.HexMode)))
			{
				if (value == IHexControl.HexMode.Selected)
				{
					list.Add("ForcePreviewTiles");
				}
				else
				{
					list.Add("ForcePreviewTiles" + value);
				}
			}
			bool flag = false;
			foreach (string item in list)
			{
				TValue var = ability.GetVar(item);
				if (var.isUnknown)
				{
					continue;
				}
				flag = true;
				List<object> list2 = var.GetObject<List<object>>();
				if (list2 != null)
				{
					foreach (CTile item2 in list2)
					{
						if (item2 != null)
						{
							CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
							if (cClientTile != null)
							{
								m_CachedAbilityRangeTiles.Add(cClientTile);
							}
						}
					}
					continue;
				}
				List<CTile> list3 = var.GetObject<List<CTile>>();
				if (list3 == null)
				{
					continue;
				}
				foreach (CTile item3 in list3)
				{
					if (item3 != null)
					{
						CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item3.m_ArrayIndex.X, item3.m_ArrayIndex.Y];
						if (cClientTile2 != null)
						{
							m_CachedAbilityRangeTiles.Add(cClientTile2);
						}
					}
				}
			}
			if (flag)
			{
				return m_CachedAbilityRangeTiles;
			}
			if (ability.IsCurrentlyTargetingActors() && Choreographer.s_Choreographer?.m_CurrentActor != null)
			{
				if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
				{
					m_CachedAbilityRangeTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.X, Choreographer.s_Choreographer.m_CurrentActor.ArrayIndex.Y]);
					return m_CachedAbilityRangeTiles;
				}
				if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion, exclusive: true) && Choreographer.s_Choreographer.m_CurrentActor is CPlayerActor { CompanionSummon: not null } cPlayerActor)
				{
					m_CachedAbilityRangeTiles.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cPlayerActor.CompanionSummon.ArrayIndex.X, cPlayerActor.CompanionSummon.ArrayIndex.Y]);
					return m_CachedAbilityRangeTiles;
				}
			}
			CTile tile = ScenarioManager.GetTile(ability.TargetingActor.ArrayIndex);
			List<CTile> list4 = new List<CTile>();
			if (ability.AllTargetsOnMovePath)
			{
				list4.AddRange(ability.TilesSelected);
			}
			else if (!(ability is CAbilityTargeting { OneTargetAtATime: not false, AreaEffectSelected: not false }))
			{
				if (ability is CAbilityPull { State: CAbilityPull.EPullState.SelectTileToPullTowards, PullType: CAbilityPull.EPullType.PullTowardsAlly })
				{
					list4.AddRange(ability.TilesInRange);
				}
				else if (ability.TilesInRange != null)
				{
					list4.AddRange(ability.TilesInRange);
					if (ability.IsCurrentlyTargetingActors())
					{
						for (int num = list4.Count - 1; num >= 0; num--)
						{
							CActor livingActor = list4[num].LivingActor;
							if (livingActor != null && !ability.ValidActorsInRange.Contains(livingActor))
							{
								list4.RemoveAt(num);
							}
						}
					}
				}
				else
				{
					Log.GUIError("Unable to find CurrentAbilityRangeTiles to display");
				}
			}
			if (ability.ValidActorsInRange != null && ability.ValidActorsInRange.Count > 0)
			{
				foreach (CObjectActor objectActor in ability.ValidActorsInRange.Where((CActor x) => x is CObjectActor))
				{
					if (!list4.Any((CTile x) => x.m_ArrayIndex == objectActor.ArrayIndex) && objectActor.IsAttachedToProp && objectActor.AttachedProp.ObjectType == ScenarioManager.ObjectImportType.Door && objectActor.AttachedProp.PropHealthDetails != null && objectActor.AttachedProp.PropHealthDetails.HasHealth && objectActor.AttachedProp.PropHealthDetails.CurrentHealth > 0)
					{
						CTile adjacentTile = ScenarioManager.GetAdjacentTile(objectActor.ArrayIndex.X, objectActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter);
						if (adjacentTile != null)
						{
							list4.Add(adjacentTile);
						}
					}
				}
			}
			if (ability is CAbilityDestroyObstacle && ability.AllTargets)
			{
				foreach (CTile item4 in ability.TilesSelected)
				{
					if (!list4.Contains(item4))
					{
						list4.Add(item4);
					}
				}
			}
			bool flag2 = m_SavedAbility is CAbilityNullHex { m_ExternalParams: var externalParams } && externalParams?.TilesInRange != null;
			_finalTiles.Clear();
			int num2 = -1;
			foreach (CTile item5 in list4)
			{
				num2++;
				bool flag3 = true;
				if (!flag2)
				{
					if (!includeCasterTile && item5 == tile && m_ObjectSpawnType != ScenarioManager.ObjectImportType.Shadow)
					{
						continue;
					}
					CActor cActor = ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndex);
					if (cActor != null && cActor.Deactivated && !cActor.SkipTurnWhenDeactivated)
					{
						cActor = null;
					}
					if (s_DontHighlightEmptyTilesWhenSelectingActors && cActor == null && ability.IsCurrentlyTargetingActors())
					{
						continue;
					}
					if (cActor != null && m_ObjectSpawnType != ScenarioManager.ObjectImportType.Shadow)
					{
						CAbilityFilter.CurrentVarsContext = ability.VarsContext;
						flag3 = ability.AbilityFilter.IsValidTarget(cActor, ability.FilterActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible, ability.CanSkipUntergetableCheck());
						CAbilityFilter.CurrentVarsContext = null;
						if (!flag3 || !ability.IsValidSingleTargetActiveBonusTarget(cActor))
						{
							continue;
						}
					}
					foreach (CObjectObstacle item6 in ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.OfType<CObjectObstacle>().ToList())
					{
						foreach (TileIndex pathingBlocker in item6.PathingBlockers)
						{
							if (item6.IgnoresFlyAndJump && item5.m_Hex.ArrayIndex.Equals(pathingBlocker))
							{
								flag3 = false;
								break;
							}
						}
						if (!flag3)
						{
							break;
						}
					}
					if (!flag3)
					{
						continue;
					}
				}
				_finalTiles.Add(new TileLOSResult
				{
					Tile = item5,
					CasterTile = tile,
					HasLOS = true
				});
			}
			if (_finalTiles.Any())
			{
				JobSystem.ExecuteFastJobs(_finalTiles, TileLOSResult.Job);
			}
			bool flag4 = ability.IsCurrentlyTargetingActors() && (ability.MiscAbilityData == null || !ability.MiscAbilityData.IgnoreLOS);
			List<CActor> list5 = ability.TargetFromAnywhereActors();
			foreach (TileLOSResult finalTile in _finalTiles)
			{
				CTile tile2 = finalTile.Tile;
				bool flag5 = flag4 && (tile2.LivingActor == null || !list5.Contains(tile2.LivingActor));
				if (!(!finalTile.HasLOS & flag5))
				{
					CClientTile cClientTile3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tile2.m_ArrayIndex.X, tile2.m_ArrayIndex.Y];
					if (cClientTile3 != null)
					{
						m_CachedAbilityRangeTiles.Add(cClientTile3);
					}
				}
			}
			return m_CachedAbilityRangeTiles;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.CurrentAbilityRangeTiles().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
		return null;
	}

	private bool PointingAtANewTile()
	{
		GetActorMousePositionOnGround(out var mousePos, out var _, out var interactable, out var overUI);
		if (mousePos != m_LastPointedAtGroundPosition || overUI != m_LastPointedOverUI)
		{
			m_LastPointedAtInteractable = interactable;
			m_LastPointedAtGroundPosition = mousePos;
			m_LastPointedOverUI = overUI;
			return true;
		}
		return false;
	}

	private static void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		if (Choreographer.s_Choreographer.m_CurrentActor != null)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(Choreographer.s_Choreographer.m_CurrentActor);
			if (gameObject != null)
			{
				ActorBehaviour.GetActorBehaviour(gameObject).UpdateRetaliateDamage(retaliateDamage, forceUpdate);
			}
		}
	}

	private CInteractable InteractableNearestToPosition(Vector3 position)
	{
		if (Singleton<WorldspacePicker>.InstanceFast == null)
		{
			return null;
		}
		return MF.FindNearestInteractableToPosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.InstanceFast.m_HexSelectionRaycastLayer, position);
	}

	private CInteractable Interactable()
	{
		if (AutoTestController.s_AutoLogPlaybackInProgress || UIManager.IsPointerOverUI)
		{
			return null;
		}
		return InteractableUnderMouse();
	}

	public static CInteractable InteractableUnderMouse()
	{
		if (Singleton<WorldspacePicker>.InstanceFast == null)
		{
			return null;
		}
		if (!(Singleton<WorldspacePicker>.Instance != null))
		{
			return null;
		}
		return MF.FindInteractableAtMousePosition(ignoreinteractableviaguiflag: false, Singleton<WorldspacePicker>.InstanceFast.m_HexSelectionRaycastLayer);
	}

	private Vector3 GetCentralPoint(List<CClientTile> validAOETiles)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (CClientTile validAOETile in validAOETiles)
		{
			foreach (CClientTile validAOETile2 in validAOETiles)
			{
				if (validAOETile != validAOETile2)
				{
					float x = (validAOETile.m_GameObject.transform.position.x + validAOETile2.m_GameObject.transform.position.x) / 2f;
					float y = (validAOETile.m_GameObject.transform.position.y + validAOETile2.m_GameObject.transform.position.y) / 2f;
					float z = (validAOETile.m_GameObject.transform.position.z + validAOETile2.m_GameObject.transform.position.z) / 2f;
					list.Add(new Vector3(x, y, z));
				}
			}
		}
		return GetMeanVector(list);
	}

	private Vector3 GetMeanVector(List<Vector3> positions)
	{
		if (positions.Count == 0)
		{
			return Vector3.zero;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (Vector3 position in positions)
		{
			num += position.x;
			num2 += position.y;
			num3 += position.z;
		}
		return new Vector3(num / (float)positions.Count, num2 / (float)positions.Count, num3 / (float)positions.Count);
	}

	public void DisplayLevelEditorObstaclePlacementStars(CClientTile originTile = null)
	{
		try
		{
			m_TilesExpectedToDisplay.Clear();
			CClientTile cClientTile = ((originTile != null) ? originTile : Interactable()?.GetComponent<TileBehaviour>()?.m_ClientTile);
			if (cClientTile == null)
			{
				return;
			}
			List<CTile> list = new List<CTile>();
			int length = ScenarioManager.Tiles.GetLength(1);
			int length2 = ScenarioManager.Tiles.GetLength(0);
			for (int i = 0; i < length2; i++)
			{
				for (int j = 0; j < length; j++)
				{
					list.Add(ScenarioManager.Tiles[i, j]);
				}
			}
			List<CTile> list2 = new List<CTile> { cClientTile.m_Tile };
			List<CTile> validTilesIncludingBlocked = null;
			Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = null;
			list2.AddRange(CAreaEffect.GetValidTiles(cClientTile.m_Tile, cClientTile.m_Tile, LevelEditorController.s_Instance.CurrentAreaEffectSelection, m_AreaEffectAngle, m_AreaEffectFlipped, null, list, ref validTilesIncludingBlocked, ref flagsOfValidTiles, ignoreLos: false, ignoreDoorLocks: false));
			foreach (CTile item in list2)
			{
				CClientTile cClientTile2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				m_TilesExpectedToDisplay.Add(cClientTile2);
				IHexControl.HexMode mode = IHexControl.HexMode.PossibleTarget;
				IHexControl.HexType type = IHexControl.HexType.PositiveEffect;
				ModifyStar(cClientTile2, type, mode, hover: true, m_AttackStars);
			}
			ClearUnusedStars(m_TilesExpectedToDisplay);
			List<CClientTile> hoveredTiles = (from s in m_AttackStars
				where s.Value.Hover
				select s.Key).ToList();
			SetHexBorders(m_AttackStars, hoveredTiles);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.DisplayAOEStars().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void ShowLevelEditorStartingTiles()
	{
		m_TilesExpectedToDisplay.Clear();
		if (!ClientScenarioManager.s_ClientScenarioManager || ClientScenarioManager.s_ClientScenarioManager.ClientTileArray == null)
		{
			return;
		}
		if (SaveData.Instance.Global.CurrentEditorLevelData != null && SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes.Count > 0)
		{
			foreach (TileIndex startingTileIndex in SaveData.Instance.Global.CurrentEditorLevelData.StartingTileIndexes)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[startingTileIndex.X, startingTileIndex.Y];
				if (cClientTile != null && !m_PlacementStars.ContainsKey(cClientTile) && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_HexMap.Revealed)
				{
					ModifyStar(cClientTile, IHexControl.HexType.Move, IHexControl.HexMode.Reach, hover: false, m_PlacementStars);
				}
			}
		}
		SetHexBorders(m_PlacementStars);
	}

	public void HighlightAllHexes(bool show)
	{
		if (show)
		{
			m_AllHexesHighlighted = true;
			ClearStars();
			List<CClientTile> list = new List<CClientTile>();
			for (int i = 0; i < ScenarioManager.Height; i++)
			{
				for (int j = 0; j < ScenarioManager.Width; j++)
				{
					CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
					if (cClientTile != null)
					{
						list.Add(cClientTile);
					}
				}
			}
			IEnumerable<CObjectTrap> source = from w in ScenarioManager.CurrentScenarioState.Maps.SelectMany((CMap sm) => sm.Props).OfType<CObjectTrap>()
				where !w.Activated
				select w;
			foreach (CClientTile item in list)
			{
				if (!IsHighlightTileValid(item))
				{
					continue;
				}
				TileTemplate tileTemplate = m_MovePossiblityTemplateAllHexes;
				CObjectProp cObjectProp = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap);
				if (cObjectProp != null)
				{
					if (source.Contains(cObjectProp))
					{
						tileTemplate = m_MoveHazardPossibilityTemplate;
					}
				}
				else if (ShowAsHazardous(item, null))
				{
					tileTemplate = m_MoveHazardPossibilityTemplate;
				}
				else if (ShowAsDifficulTerrain(item.m_Tile))
				{
					tileTemplate = m_DifficultTerrainPossibilityTemplate;
				}
				else if (item.m_Tile.FindProp(ScenarioManager.ObjectImportType.TerrainIce) != null || item.m_Tile.IsIce)
				{
					tileTemplate = m_IceTerrainPossibilityTemplate;
				}
				else
				{
					ScenarioScript current2 = ScenarioScript.current;
					if (current2 != null && current2.IsTileEleveated(item.m_Tile))
					{
						tileTemplate = m_ElevatedTerrainPossibilityTemplate;
					}
					else
					{
						CObjectProp cObjectProp2 = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest);
						CObjectProp cObjectProp3 = item.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
						if (cObjectProp2 != null || (cObjectProp3 != null && !(cObjectProp3 as CObjectDoor).IsDungeonEntrance))
						{
							tileTemplate = m_MoveChestDoorPossibilityTemplate;
						}
					}
				}
				ModifyStar(item, tileTemplate.m_Type, tileTemplate.m_Mode, tileTemplate.m_Hover, m_HighlightAllStars);
			}
			RefreshHexesWithAllBorders(m_HighlightAllStars);
		}
		else
		{
			m_AllHexesHighlighted = false;
			ClearStars();
			RefreshCurrentState();
		}
	}

	private bool IsHighlightTileValid(CClientTile tile)
	{
		try
		{
			if (tile == null)
			{
				return false;
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_Tile.m_ArrayIndex.X, tile.m_Tile.m_ArrayIndex.Y];
			if (cNode == null)
			{
				return false;
			}
			if (tile.m_Tile.m_HexMap != null && tile.m_Tile.m_Hex2Map != null)
			{
				if (!tile.m_Tile.m_HexMap.Revealed && !tile.m_Tile.m_Hex2Map.RevealedAndNotDestroyed)
				{
					return false;
				}
			}
			else
			{
				if (tile.m_Tile.m_HexMap != null && !tile.m_Tile.m_HexMap.RevealedAndNotDestroyed)
				{
					return false;
				}
				if (tile.m_Tile.m_Hex2Map != null && !tile.m_Tile.m_Hex2Map.RevealedAndNotDestroyed)
				{
					return false;
				}
			}
			if (!cNode.Walkable)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the WorldspaceStarHexDisplay.IsHighlightTileValid().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_HEXDISPLAY_00023", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return false;
		}
	}

	private void FindAllChestAndDoorTiles()
	{
		m_ChestTiles.Clear();
		m_DoorTiles.Clear();
		new List<CClientTile>();
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[j, i];
				if (cClientTile == null)
				{
					continue;
				}
				if (cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Chest) != null)
				{
					m_ChestTiles.Add(cClientTile);
					continue;
				}
				CObjectDoor cObjectDoor = (CObjectDoor)cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door);
				if (cObjectDoor != null && cClientTile.m_Tile.m_HexMap != null && cClientTile.m_Tile.m_Hex2Map != null && !cObjectDoor.DoorIsOpen)
				{
					m_DoorTiles.Add(cClientTile);
				}
			}
		}
		m_FoundAllChestAndDoorTiles = true;
	}

	public void ClearCachedAbilityTiles()
	{
		m_CachedAbilityRangeTiles.Clear();
	}

	private List<CClientTile> FindStraightLineDirection(CActor movingActor, int remainingMoves)
	{
		List<CClientTile> result = new List<CClientTile>();
		if (Waypoint.s_Waypoints.Count > 0)
		{
			Waypoint waypointComponent = Waypoint.GetWaypointComponent(Waypoint.s_Waypoints.Last());
			if (waypointComponent != null && waypointComponent.ClientTile.m_Tile != null)
			{
				if (m_LeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ELeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ELeft;
				}
				else if (m_RightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ERight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ERight;
				}
				else if (m_TopLeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ETopLeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ETopLeft;
				}
				else if (m_TopRightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.ETopRight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.ETopRight;
				}
				else if (m_BottomLeftLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomLeft);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.EBottomLeft;
				}
				else if (m_BottomRightLineTiles.Contains(waypointComponent.ClientTile.m_Tile))
				{
					result = GetRemainingTilesInDirection(movingActor, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomRight);
					CurrentLineDirection = ScenarioManager.EAdjacentPosition.EBottomRight;
				}
			}
		}
		return result;
	}

	private List<CClientTile> GetRemainingTilesInDirection(CActor movingActor, int remainingMoves, ScenarioManager.EAdjacentPosition position)
	{
		List<CClientTile> list = new List<CClientTile>();
		List<CTile> list2 = null;
		switch (position)
		{
		case ScenarioManager.EAdjacentPosition.ELeft:
			list2 = m_LeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ERight:
			list2 = m_RightLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ETopLeft:
			list2 = m_TopLeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.ETopRight:
			list2 = m_TopRightLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomLeft:
			list2 = m_BottomLeftLineTiles;
			break;
		case ScenarioManager.EAdjacentPosition.EBottomRight:
			list2 = m_BottomRightLineTiles;
			break;
		}
		if (Waypoint.s_Waypoints.Count > 0)
		{
			Waypoint waypointComponent = Waypoint.GetWaypointComponent(Waypoint.s_Waypoints.Last());
			if (list2.Contains(waypointComponent.ClientTile.m_Tile))
			{
				for (int i = list2.IndexOf(waypointComponent.ClientTile.m_Tile) + 1; i < list2.Count; i++)
				{
					CTile cTile = list2[i];
					CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y];
					list.Add(item);
				}
			}
		}
		else
		{
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y];
			if (cClientTile != null && list2.Contains(cClientTile.m_Tile))
			{
				for (int j = list2.IndexOf(cClientTile.m_Tile) + 1; j < list2.Count; j++)
				{
					CTile cTile2 = list2[j];
					CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y];
					list.Add(item2);
				}
			}
		}
		return list;
	}

	private List<CClientTile> FindAllStraightLineTiles(CActor movingActor, int remainingMoves)
	{
		List<CClientTile> list = new List<CClientTile>();
		m_LeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ELeft);
		m_RightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ERight);
		m_TopLeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ETopLeft);
		m_TopRightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.ETopRight);
		m_BottomLeftLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomLeft);
		m_BottomRightLineTiles = ScenarioManager.GetTilesInLine(movingActor.ArrayIndex.X, movingActor.ArrayIndex.Y, remainingMoves, ScenarioManager.EAdjacentPosition.EBottomRight);
		m_AllLineTiles.Clear();
		m_AllLineTiles.AddRange(m_LeftLineTiles);
		m_AllLineTiles.AddRange(m_RightLineTiles);
		m_AllLineTiles.AddRange(m_TopLeftLineTiles);
		m_AllLineTiles.AddRange(m_TopRightLineTiles);
		m_AllLineTiles.AddRange(m_BottomLeftLineTiles);
		m_AllLineTiles.AddRange(m_BottomRightLineTiles);
		foreach (CTile leftLineTile in m_LeftLineTiles)
		{
			CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[leftLineTile.m_ArrayIndex.X, leftLineTile.m_ArrayIndex.Y];
			list.Add(item);
		}
		foreach (CTile rightLineTile in m_RightLineTiles)
		{
			CClientTile item2 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[rightLineTile.m_ArrayIndex.X, rightLineTile.m_ArrayIndex.Y];
			list.Add(item2);
		}
		foreach (CTile topLeftLineTile in m_TopLeftLineTiles)
		{
			CClientTile item3 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[topLeftLineTile.m_ArrayIndex.X, topLeftLineTile.m_ArrayIndex.Y];
			list.Add(item3);
		}
		foreach (CTile topRightLineTile in m_TopRightLineTiles)
		{
			CClientTile item4 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[topRightLineTile.m_ArrayIndex.X, topRightLineTile.m_ArrayIndex.Y];
			list.Add(item4);
		}
		foreach (CTile bottomLeftLineTile in m_BottomLeftLineTiles)
		{
			CClientTile item5 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[bottomLeftLineTile.m_ArrayIndex.X, bottomLeftLineTile.m_ArrayIndex.Y];
			list.Add(item5);
		}
		foreach (CTile bottomRightLineTile in m_BottomRightLineTiles)
		{
			CClientTile item6 = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[bottomRightLineTile.m_ArrayIndex.X, bottomRightLineTile.m_ArrayIndex.Y];
			list.Add(item6);
		}
		return list;
	}

	public void ResetLineDirection()
	{
		CurrentLineDirection = ScenarioManager.EAdjacentPosition.ECenter;
	}

	public void ProxyUpdateSelectionHexes(CClientTile originTile, int targetingAngle, bool targetingFlipped, bool aoeLocked)
	{
		Log.NetworkInfo($"PROXY: Updating ability selection hexes. Targeting angle: {targetingAngle}, flipped: {targetingFlipped}");
		m_AreaEffectAngle = targetingAngle;
		m_AreaEffectFlipped = targetingFlipped;
		SetAOELocked(aoeLocked);
		ShowPossibleTargetSelectionHexes(originTile);
	}
}

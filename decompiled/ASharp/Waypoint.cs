using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FFSNet;
using FHGUI.Scenario.Services;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public enum EWaypointType
	{
		None,
		Move,
		Push,
		Pull,
		Attack,
		Teleport2
	}

	public static CActor s_MovingActor;

	public bool IsTheLastWaypoint;

	public bool CanEndMovementHere = true;

	public bool WaypointIsADoor;

	[HideInInspector]
	public bool DoorIsOpen;

	public GameObject ParentWaypointObject;

	public GameObject EndMovementButton;

	public GameObject DoorYesButton;

	public GameObject DeleteButton;

	public static List<GameObject> s_Waypoints = new List<GameObject>();

	public static List<GameObject> s_CachedWaypoints = new List<GameObject>();

	private static List<Point> s_ClearValidSelectionTiles = new List<Point>();

	public static CClientTile s_PlacementTile;

	public static bool s_LockWaypoints = false;

	public static bool s_CachedLockWaypoints = false;

	private CClientTile m_ClientTile;

	private int m_Order;

	private int m_MovesRemaining;

	private int m_MoveCost;

	private int m_MovesAvailable;

	private int m_RemainingWaterTilesThatCostZero;

	private EWaypointType m_WaypointType;

	private bool m_TileIsBlocked;

	private bool m_ActorFlying;

	public static bool IsLastWaypointADoor
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return false;
			}
			return s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().WaypointIsADoor;
		}
	}

	public static Waypoint GetLastWaypoint
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return null;
			}
			return s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>();
		}
	}

	public static bool SingleAction
	{
		get
		{
			if (s_Waypoints.Count == 0)
			{
				return false;
			}
			Waypoint getLastWaypoint = GetLastWaypoint;
			if (getLastWaypoint.m_WaypointType != EWaypointType.Push && getLastWaypoint.m_WaypointType != EWaypointType.Pull)
			{
				return getLastWaypoint.m_WaypointType == EWaypointType.Attack;
			}
			return true;
		}
	}

	public CClientTile ClientTile => m_ClientTile;

	public int MovesRemaining => m_MovesRemaining;

	public int MoveCost => m_MoveCost;

	public int Order => m_Order;

	public int RemainingWaterTilesThatCostZero => m_RemainingWaterTilesThatCostZero;

	public static bool s_HasWaypoints => s_Waypoints.Count > 0;

	public EWaypointType Type => m_WaypointType;

	public static void RefreshWaypointMovesRemaining(CAbilityMove moveAbility)
	{
		int num = moveAbility.MoveCountAfterLimit;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint component = s_Waypoint.GetComponent<Waypoint>();
			component.m_MovesRemaining = num - component.MoveCost;
			num -= component.MoveCost;
		}
	}

	public static bool IsPathGoingThroughADoor()
	{
		if (s_ClearValidSelectionTiles == null || s_ClearValidSelectionTiles.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < s_ClearValidSelectionTiles.Count - 1; i++)
		{
			Point point = s_ClearValidSelectionTiles[i];
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[point.X, point.Y];
			if (cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door) != null && !cClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Door).Activated)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		EndMovementButton.SetActive(value: false);
		DoorYesButton.SetActive(value: false);
		DeleteButton.SetActive(value: false);
	}

	private void OnDestroy()
	{
		s_Waypoints.Remove(gameObject);
	}

	public void SetOrder(int order, List<Point> path = null)
	{
		try
		{
			m_Order = order;
			CheckIfPlayerCanEndMovementHere(path);
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
			CheckForDoors();
			SetupWaypoint();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.SetOrder().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CheckIfPlayerCanEndMovementHere(List<Point> path = null)
	{
		try
		{
			CanEndMovementHere = true;
			if (m_WaypointType == EWaypointType.Attack || m_WaypointType == EWaypointType.Teleport2)
			{
				return;
			}
			CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
			CAbilityMove shadowMoveAbility = currentAbility as CAbilityMove;
			if (shadowMoveAbility != null && shadowMoveAbility.MoveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.MustEndOnShadow))
			{
				CanEndMovementHere = ClientTile.m_Tile.FindProps(ScenarioManager.ObjectImportType.Shadow).Count > 0 && ClientTile.m_Tile.LivingActor == null;
				if (ClientTile.m_Tile.FindDoor() == null)
				{
					return;
				}
				int num = 0;
				if (s_Waypoints != null)
				{
					foreach (GameObject s_Waypoint in s_Waypoints)
					{
						Waypoint component = s_Waypoint.GetComponent<Waypoint>();
						if (!(component == null))
						{
							num += component.MoveCost;
						}
					}
				}
				int range = shadowMoveAbility.ModifiedStrength() - num;
				if (GameState.GetTilesInRange(ClientTile.m_Tile.m_ArrayIndex, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, shadowMoveAbility.Jump, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: true).ToList().Any((CTile tile) => tile.FindProp(ScenarioManager.ObjectImportType.Shadow) != null && (tile.LivingActor == null || tile.LivingActor == shadowMoveAbility.TargetingActor)))
				{
					CanEndMovementHere = true;
				}
				return;
			}
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: var moveRestrictionType } cAbilityMove && moveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles) && !cAbilityMove.CheckMustEndOnSpecificTiles(ClientTile.m_Tile))
			{
				CanEndMovementHere = false;
				return;
			}
			bool flag = false;
			if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: var moveRestrictionType2 } cAbilityMove2)
			{
				if (moveRestrictionType2.Equals(CAbilityMove.EMoveRestrictionType.MustEndNextToEnemy) && GameState.GetAllEnemiesInRange(cAbilityMove2.TargetingActor, ClientTile.m_Tile.m_ArrayIndex, 1).Count == 0)
				{
					CanEndMovementHere = false;
					return;
				}
				if (cAbilityMove2.CarryOtherActorsOnHex)
				{
					if (cAbilityMove2.ActorsToCarry.Count > 0)
					{
						flag = true;
					}
					else if (path != null && path.Count > 1)
					{
						for (int num2 = 0; num2 < path.Count - 1; num2++)
						{
							List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[path[num2].X, path[num2].Y], Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), null, isTargetedAbility: false, false);
							if (actorsOnTile != null && actorsOnTile.Count > 0)
							{
								flag = true;
							}
						}
					}
				}
			}
			if (!s_MovingActor.IgnoreActorCollision | flag)
			{
				CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
				foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
				{
					if (!allAliveMonster.BlocksPathing)
					{
						continue;
					}
					Point realArrayIndex = allAliveMonster.RealArrayIndex;
					if (s_MovingActor != allAliveMonster || s_Waypoints.Count <= 0)
					{
						if (mirrorAbilityBehaviour != null && (s_MovingActor == mirrorAbilityBehaviour.ActiveBonus.Actor || s_MovingActor == mirrorAbilityBehaviour.MirrorTarget) && !mirrorAbilityBehaviour.IsStopTileValidForAbilityMirrored(ClientTile.m_Tile.m_ArrayIndex, allAliveMonster))
						{
							CanEndMovementHere = false;
							return;
						}
						if (realArrayIndex == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
				foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
				{
					if (heroSummon.PhasedOut || heroSummon.IgnoreActorCollision)
					{
						continue;
					}
					Point realArrayIndex2 = heroSummon.RealArrayIndex;
					if ((s_MovingActor != heroSummon || s_Waypoints.Count <= 0) && realArrayIndex2 == ClientTile.m_Tile.m_ArrayIndex)
					{
						HeroSummonYMLData heroSummonYMLData = (s_MovingActor as CHeroSummonActor)?.SummonData;
						if (heroSummonYMLData == null || heroSummonYMLData.CanMoveOnActorClassID == null || heroSummon.Class?.ID != heroSummonYMLData.CanMoveOnActorClassID)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
				foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
				{
					Point realArrayIndex3 = @object.RealArrayIndex;
					if (@object.IsSuperTransientBlocker && realArrayIndex3 == ClientTile.m_Tile.m_ArrayIndex)
					{
						CanEndMovementHere = false;
						return;
					}
					if (!@object.PhasedOut && !@object.IgnoreActorCollision && (s_MovingActor != @object || s_Waypoints.Count <= 0) && realArrayIndex3 == ClientTile.m_Tile.m_ArrayIndex)
					{
						CanEndMovementHere = false;
						return;
					}
				}
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					if (!playerActor.PhasedOut && !playerActor.IgnoreActorCollision)
					{
						Point realArrayIndex4 = playerActor.RealArrayIndex;
						if ((s_MovingActor != playerActor || s_Waypoints.Count <= 0) && realArrayIndex4 == ClientTile.m_Tile.m_ArrayIndex)
						{
							CanEndMovementHere = false;
							return;
						}
					}
				}
			}
			CNode cNode = ScenarioManager.PathFinder.Nodes[ClientTile.m_Tile.m_ArrayIndex.X, ClientTile.m_Tile.m_ArrayIndex.Y];
			if (cNode.IsIceTerrain)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(s_MovingActor, out var jump, out var fly, out var _, out var _, ref strength);
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove cAbilityMove3)
				{
					jump = jump || cAbilityMove3.Jump;
					fly = fly || cAbilityMove3.Fly;
				}
				List<Point> waypoints = s_Waypoints.Select((GameObject wp) => wp.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex).ToList();
				CanEndMovementHere = (fly | jump) || CActorStatic.IsIceTerrainSlideInterrupted(s_MovingActor, path, waypoints);
			}
			else if (!m_ActorFlying || (m_ActorFlying && Choreographer.s_Choreographer.m_CurrentAbility.TargetingActor.IgnoreActorCollision))
			{
				CanEndMovementHere = cNode != null && cNode.Walkable && !cNode.Blocked;
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.CheckIfPlayerCanEndMovementHere().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00002", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void CheckForDoors()
	{
		try
		{
			WaypointIsADoor = Choreographer.s_Choreographer.CheckForDoors(ClientTile);
			if (m_WaypointType == EWaypointType.Move || m_WaypointType == EWaypointType.Teleport2)
			{
				if (WaypointIsADoor && !DoorIsOpen)
				{
					SetupWaypointForDoor(active: true);
				}
				else
				{
					SetupWaypointForDoor(active: false);
				}
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.CheckForDoors().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00003", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void EndMovement()
	{
		try
		{
			if (CanEndMovementHere)
			{
				EndMovementButton.SetActive(value: false);
				DeleteButton.SetActive(value: false);
				WorldspaceStarHexDisplay.Instance.ActorIsOpeningDoor(active: false);
				OnPress(networkActionIfOnline: true);
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.EndMovement().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00004", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OpenDoor()
	{
		try
		{
			DoorYesButton.SetActive(value: false);
			Choreographer.s_Choreographer.DisableTileSelection(active: false);
			DoorIsOpen = true;
			WorldspaceStarHexDisplay.Instance.ActorIsOpeningDoor(active: true);
			OnPress();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.OpenDoor().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00005", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	private void LateUpdate()
	{
		if (m_ClientTile == null || !(m_ClientTile.m_GameObject != null) || !(WorldspaceUITools.Instance != null) || !(WorldspaceUITools.Instance.WorldspaceCanvas != null) || !(WorldspaceUITools.Instance.WorldspaceCamera != null))
		{
			return;
		}
		RectTransform component = WorldspaceUITools.Instance.WorldspaceCanvas.GetComponent<RectTransform>();
		if (component != null)
		{
			Vector2 b = new Vector2(component.rect.width / (float)Screen.width, component.rect.height / (float)Screen.height);
			RectTransform component2 = GetComponent<RectTransform>();
			if (component2 != null)
			{
				component2.anchoredPosition = Vector2.Scale(WorldspaceUITools.Instance.WorldspaceCamera.WorldToScreenPoint(m_ClientTile.m_GameObject.transform.position), b);
			}
		}
	}

	public void OnPress(bool networkActionIfOnline = false)
	{
		try
		{
			GameObject gameObject = (SingleAction ? s_Waypoints[s_Waypoints.Count - 1] : s_Waypoints[0]);
			CClientTile clientTile = GetWaypointComponent(gameObject).ClientTile;
			CAbilityMove cAbilityMove = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMove;
			if (FFSNetwork.IsOnline && (Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl() & networkActionIfOnline))
			{
				Synchronizer.SendGameAction(GameActionType.PressWaypoint, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new TileToken(clientTile.m_Tile.m_ArrayIndex));
			}
			if (SingleAction)
			{
				Choreographer.s_Choreographer.m_UndoButton.Toggle(active: false);
			}
			Choreographer.s_Choreographer.AutoTestIgnoreTileClick = true;
			Choreographer.s_Choreographer.TileHandler(clientTile, GetCTileList());
			Choreographer.s_Choreographer.AutoTestIgnoreTileClick = false;
			CAbilityMove cAbilityMove2 = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMove;
			if (WaypointIsADoor && cAbilityMove2 == cAbilityMove)
			{
				Choreographer.s_Choreographer.ContinueTileSelection();
			}
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
			Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
			DestroyWaypoint(gameObject);
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.OnPress().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00006", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void OnDelete()
	{
		try
		{
			WorldspaceStarHexDisplay.Instance.ActorIsSelectingDoor(active: false);
			s_Waypoints.Remove(ParentWaypointObject);
			Choreographer.s_Choreographer.DisableTileSelection(active: false);
			CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
			if (s_Waypoints.Count == 0)
			{
				string term = string.Empty;
				ReadyButton.EButtonState state = ReadyButton.EButtonState.EREADYBUTTONCONTINUE;
				switch (m_WaypointType)
				{
				case EWaypointType.Move:
					term = "GUI_CONFIRM_MOVEMENT";
					state = ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED;
					break;
				case EWaypointType.Push:
					term = "GUI_END_PUSH";
					break;
				case EWaypointType.Pull:
					term = "GUI_END_PULL";
					break;
				case EWaypointType.Teleport2:
					term = "GUI_END_TELEPORT2";
					break;
				case EWaypointType.Attack:
					term = "GUI_CONFIRM_ATTACK";
					if (PhaseManager.Phase is CPhaseAction)
					{
						ScenarioRuleClient.ClearTargets();
					}
					break;
				}
				Choreographer.s_Choreographer.readyButton.ResetAlternativeAction(LocalizationManager.GetTranslation(term), state);
				string text = ((currentAbility is CAbilityMove) ? LocalizationManager.GetTranslation("GUI_SKIP_MOVEMENT") : LocalizationManager.GetTranslation("GUI_SKIP_ABILITY"));
				if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability == currentAbility && currentAbility != null && !currentAbility.AbilityHasBeenCancelled)
				{
					SkipButton skipButton = Choreographer.s_Choreographer.m_SkipButton;
					bool? interactable = Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility.CanSkip;
					skipButton.Toggle(active: true, text, hideOnClick: true, interactable);
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility?.MiscAbilityData != null && Choreographer.s_Choreographer.m_CurrentAbility.MiscAbilityData.CanSkip.HasValue && !Choreographer.s_Choreographer.m_CurrentAbility.MiscAbilityData.CanSkip.Value && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { MoveRestrictionType: CAbilityMove.EMoveRestrictionType.MustEndOnSpecificTiles, HasMoved: false } cAbilityMove && !cAbilityMove.CheckMustEndOnSpecificTiles(cAbilityMove.CurrentMovingActor.Tile))
				{
					Choreographer.s_Choreographer.m_SkipButton.SetInteractable(active: false);
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove { CarryOtherActorsOnHex: not false })
				{
					ScenarioRuleClient.UpdateMoveCarry(GetCTileList(), removeWaypoint: true);
				}
			}
			else if (m_WaypointType == EWaypointType.Attack)
			{
				GetLastWaypoint.SetupWaypoint();
			}
			else
			{
				Choreographer.s_Choreographer.readyButton.SetInteractable(GetLastWaypoint.CanEndMovementHere);
			}
			UnityEngine.Object.Destroy(ParentWaypointObject);
			if (currentAbility is CAbilityMove { MoveRestrictionType: var moveRestrictionType } cAbilityMove3 && moveRestrictionType.Equals(CAbilityMove.EMoveRestrictionType.StraightLineOnly) && !cAbilityMove3.HasMoved && s_Waypoints.Count <= 0)
			{
				WorldspaceStarHexDisplay.Instance.ResetLineDirection();
			}
			WorldspaceStarHexDisplay.Instance.RefreshCurrentState();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.OnDelete().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00007", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static GameObject GetNextWaypoint()
	{
		try
		{
			if (s_Waypoints.Count == 0)
			{
				return null;
			}
			return s_Waypoints[0];
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.GetNextWaypoint().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00008", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static List<CTile> GetCTileList()
	{
		try
		{
			List<CTile> list = new List<CTile>();
			foreach (GameObject s_Waypoint in s_Waypoints)
			{
				Waypoint component = s_Waypoint.GetComponent<Waypoint>();
				list.Add(component.m_ClientTile.m_Tile);
			}
			return list;
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.GetCTileList().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static void CacheWaypoints()
	{
		s_CachedWaypoints.Clear();
		s_CachedWaypoints.AddRange(s_Waypoints);
		s_CachedLockWaypoints = s_LockWaypoints;
	}

	public static void RestoreWaypoints()
	{
		if (s_CachedWaypoints.Count > 0)
		{
			s_Waypoints.Clear();
			s_Waypoints.AddRange(s_CachedWaypoints);
			s_CachedWaypoints.Clear();
			s_LockWaypoints = s_CachedLockWaypoints;
			s_CachedLockWaypoints = false;
		}
	}

	private void SetupWaypoint()
	{
		try
		{
			IsTheLastWaypoint = s_Waypoints.FindIndex((GameObject x) => x == gameObject) == s_Waypoints.Count - 1;
			ReadyButton.EButtonState state = (WaypointIsADoor ? ReadyButton.EButtonState.EREADYBUTTONOPENDOOR : ReadyButton.EButtonState.EREADYBUTTONCONFIRMMOVEMENT);
			ActionsViewService.TryGetMovementButtonParam(Choreographer.s_Choreographer.CurrentActor, out var resultButtonParam);
			if (!WaypointIsADoor && m_WaypointType == EWaypointType.Attack && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack)
			{
				ScenarioRuleClient.UpdateAttackpath(GetCTileList(), processImmediately: true);
			}
			if (resultButtonParam.LabelKey != string.Empty)
			{
				Choreographer.s_Choreographer.readyButton.AlternativeAction(resultButtonParam.Callback, state, LocalizationManager.GetTranslation(resultButtonParam.LabelKey));
			}
			Choreographer.s_Choreographer.readyButton.SetInteractable(resultButtonParam.Interactable);
			Choreographer.s_Choreographer.RefreshViewController();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.SetupWaypoint().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00010", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static void Create(CClientTile clientTile, int movesRemaining, int moveCost, int movesAvailable, int remainingWaterTilesThatCostZero, EWaypointType waypointType, bool actorFlying, bool tileIsBlocked, List<Point> path = null)
	{
		try
		{
			Log.SimpleInfo("Creating Waypoint at " + clientTile.m_Tile.m_ArrayIndex.ToString());
			GameObject gameObject = UnityEngine.Object.Instantiate(GlobalSettings.Instance.m_WaypointPrefab);
			s_Waypoints.Add(gameObject);
			Waypoint component = gameObject.GetComponent<Waypoint>();
			component.m_ClientTile = clientTile;
			component.m_MovesRemaining = movesRemaining;
			component.m_MoveCost = moveCost;
			component.m_MovesAvailable = movesAvailable;
			component.m_RemainingWaterTilesThatCostZero = remainingWaterTilesThatCostZero;
			component.m_WaypointType = waypointType;
			component.m_ActorFlying = actorFlying;
			component.m_TileIsBlocked = tileIsBlocked;
			component.SetOrder(GetHighestOrder() + 1, path);
			gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceCanvas.transform, worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one;
			if (s_LockWaypoints)
			{
				return;
			}
			Choreographer.s_Choreographer.m_UndoButton.SetOnClickOverrider(() =>
			{
				if (s_Waypoints.Count != 0)
				{
					GetLastWaypoint.OnDelete();
					if (s_Waypoints.Count == 0 && waypointType == EWaypointType.Move)
					{
						Choreographer.s_Choreographer.readyButton.Toggle(active: true, ReadyButton.EButtonState.EREADYBUTTONCONFIRMDISABLED, null, hideOnClick: true, glowingEffect: true);
					}
				}
			}, UndoButton.EButtonState.EUNDOBUTTONUNDOWAYPOINT, LocalizationManager.GetTranslation("GUI_UNDO_WAYPOINT"));
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.Create().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00011", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static Waypoint GetWaypointComponent(GameObject waypointGO)
	{
		return waypointGO.GetComponent<Waypoint>();
	}

	private static int GetHighestOrder()
	{
		int num = 0;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint component = s_Waypoint.GetComponent<Waypoint>();
			if (component.m_Order > num)
			{
				num = component.m_Order;
			}
		}
		return num;
	}

	public static void AddValidSelectionTiles(Point point)
	{
		s_ClearValidSelectionTiles.Add(point);
	}

	public static void ClearValidSelectionTiles()
	{
		s_ClearValidSelectionTiles.Clear();
	}

	public static void DestroyWaypoint(GameObject waypoint)
	{
		Waypoint component = waypoint.GetComponent<Waypoint>();
		Log.SimpleInfo("Destroying Waypoint at " + component.m_ClientTile.m_Tile.m_ArrayIndex.ToString());
		component.OnDelete();
		UnityEngine.Object.Destroy(waypoint);
	}

	public static void Clear()
	{
		if (!s_LockWaypoints)
		{
			for (int num = s_Waypoints.Count - 1; num >= 0; num--)
			{
				if (!s_CachedWaypoints.Contains(s_Waypoints[num]))
				{
					DestroyWaypoint(s_Waypoints[num]);
				}
			}
		}
		ClearValidSelectionTiles();
	}

	private void SetupWaypointForDoor(bool active)
	{
		WorldspaceStarHexDisplay.Instance.ActorIsSelectingDoor(active);
	}

	private static bool IsLastWaypointClicked(CClientTile clientTile, bool actingPlayerHasSecondClickConfirmationEnabled = false)
	{
		if (s_Waypoints.Count <= 0)
		{
			return false;
		}
		Waypoint component = s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>();
		if (clientTile != component.ClientTile)
		{
			return false;
		}
		if (actingPlayerHasSecondClickConfirmationEnabled)
		{
			if (!Choreographer.s_Choreographer.readyButton.IsInteractable)
			{
				return false;
			}
			if (component.m_WaypointType != EWaypointType.Attack && !component.CanEndMovementHere)
			{
				return false;
			}
			if (Choreographer.s_Choreographer.readyButton.ActionsQueue.Count == 0)
			{
				Choreographer.s_Choreographer.readyButton.ActionsQueue.Add(() =>
				{
					GetLastWaypoint.OnPress();
				});
			}
			Choreographer.s_Choreographer.readyButton.OnClick();
		}
		else if (Choreographer.s_Choreographer.m_CurrentAbility.CanClearTargets())
		{
			Choreographer.s_Choreographer.m_UndoButton.OnClick();
		}
		return true;
	}

	private static void DeleteLastWaypoint()
	{
		GetLastWaypoint?.OnDelete();
		if (s_Waypoints.Count <= 0)
		{
			Choreographer.s_Choreographer.m_UndoButton.ClearOnClickOverriders();
		}
	}

	public static void TileHandler(CClientTile clientTile, List<CTile> optionalTileList, bool networkActionIfOnline = true, bool isUserClick = false, bool actingPlayerHasSecondClickConfirmationEnabled = false, CInteractable.EInputType inputType = CInteractable.EInputType.None)
	{
		if (isUserClick && inputType != CInteractable.EInputType.Primary)
		{
			return;
		}
		try
		{
			if (LevelEditorController.InterceptTileAction(clientTile) || DebugMenu.InterceptTileAction(clientTile))
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Exiting early, action was intercepted");
				return;
			}
			if (Choreographer.s_Choreographer.m_TileSelectionDisabled & isUserClick)
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Exiting early, tile selection is disabled");
				return;
			}
			if (isUserClick)
			{
				AutoTestController.LogTileClick(clientTile, optionalTileList, s_ClearValidSelectionTiles, null, inputType);
			}
			CAbility currentAbility = Choreographer.s_Choreographer.m_CurrentAbility;
			if (currentAbility != null && (currentAbility.IsWaitingForSingleTargetItem() || currentAbility.IsWaitingForSingleTargetActiveBonus()))
			{
				CActor cActor = currentAbility.ActorsToTarget.SingleOrDefault((CActor s) => s.GetTilesCanAttackFrom().Contains(clientTile.m_Tile));
				if (!currentAbility.IsValidSingleTargetActiveBonusTarget(cActor))
				{
					cActor = null;
				}
				if (cActor != null)
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					ScenarioRuleClient.ApplySingleTarget(cActor);
					if (FFSNetwork.IsOnline & networkActionIfOnline)
					{
						Synchronizer.SendGameActionClassID(GameActionType.ApplySingleTargetEffect, ActionProcessor.CurrentPhase, cActor.ID, (int)cActor.Type, supplementaryDataBool: true, cActor.Class.ID);
					}
				}
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Exiting early, single target item/active bonus was active with no target selected");
				return;
			}
			if (IsLastWaypointClicked(clientTile, actingPlayerHasSecondClickConfirmationEnabled))
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Clicked previous waypoint twice. SecondClickConfirmationEnabled: " + actingPlayerHasSecondClickConfirmationEnabled);
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileCancelClickAudioItem);
				return;
			}
			if (IsLastWaypointADoor)
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Exiting early, last waypoint is a door");
				return;
			}
			bool flag = IsPathGoingThroughADoor();
			if (AutoTestController.s_AutoLogPlaybackInProgress)
			{
				s_ClearValidSelectionTiles = CAutoTileClick.TileIndexToAStarList((AutoTestController.s_Instance.CurrentAutoLogPlayback.CurrentEvent as CAutoTileClick)?.Waypoints);
				if (s_ClearValidSelectionTiles == null)
				{
					Log.GUIError("Invalid tile selection.  No valid waypoints were found.");
					AutoTestController.s_Instance.CurrentAutoLogPlayback.TestEnded = true;
					return;
				}
			}
			if (currentAbility is CAbilityPull || currentAbility is CAbilityPush || currentAbility is CAbilityTeleport2)
			{
				foreach (GameObject s_Waypoint in s_Waypoints)
				{
					if (s_Waypoint.GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex == clientTile.m_Tile.m_ArrayIndex)
					{
						Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Exiting early, push/pull/teleport2 cannot select same waypoint twice");
						return;
					}
				}
			}
			if (s_ClearValidSelectionTiles.Any((Point it) => it.Equals(clientTile.m_Tile.m_ArrayIndex)))
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Tile (" + clientTile.m_Tile.m_ArrayIndex.ToString() + ") is in valid selection tiles");
				Point point = ((s_Waypoints.Count == 0) ? s_MovingActor.ArrayIndex : s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.m_ArrayIndex);
				bool fly = false;
				bool jump = false;
				bool ignoreDifficultTerrain = false;
				bool ignoreHazardousTerrain = false;
				int num = 0;
				bool ignoreDoorLocks = false;
				Point? freeTileAwayFromCaster = null;
				int num2 = 0;
				bool ignoreMoveCost = false;
				bool ignoreBlockedTileMoveCost = false;
				EWaypointType eWaypointType = EWaypointType.None;
				bool carryOtherActors = false;
				CActor.EType carryType = CActor.EType.Unknown;
				CAbilityMove cAbilityMove = null;
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Current ability is a move ability");
					cAbilityMove = (CAbilityMove)Choreographer.s_Choreographer.m_CurrentAbility;
					fly = cAbilityMove.Fly;
					jump = cAbilityMove.Jump;
					ignoreDoorLocks = cAbilityMove.IgnoreDoorLocks;
					ignoreDifficultTerrain = cAbilityMove.IgnoreDifficultTerrain;
					ignoreHazardousTerrain = cAbilityMove.IgnoreHazardousTerrain;
					ignoreBlockedTileMoveCost = cAbilityMove.IgnoreBlockedTileMoveCost;
					num = ((s_Waypoints.Count > 0) ? s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().RemainingWaterTilesThatCostZero : cAbilityMove.RemainingWaterTilesThatCostZero);
					freeTileAwayFromCaster = cAbilityMove.m_FreeTileAwayFromCaster;
					num2 = cAbilityMove.RemainingMovesAfterLimit;
					eWaypointType = EWaypointType.Move;
					carryOtherActors = cAbilityMove.CarryOtherActorsOnHex;
					if (cAbilityMove.ActorsToCarry.Count > 0)
					{
						int strength = 0;
						carryType = cAbilityMove.ActorsToCarry[0].Type;
						CAbilityMove.GetMoveBonuses(cAbilityMove.ActorsToCarry[0], out jump, out fly, out ignoreDifficultTerrain, out ignoreHazardousTerrain, ref strength);
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Current ability is a teleport2 ability");
					num2 = 0;
					eWaypointType = EWaypointType.Teleport2;
					fly = true;
					ignoreDifficultTerrain = true;
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull cAbilityPull)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Current ability is a pull ability");
					num2 = cAbilityPull.RemainingPulls;
					eWaypointType = EWaypointType.Pull;
					fly = cAbilityPull.CurrentTarget.FlyingAfterConditions;
					ignoreDifficultTerrain = true;
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush cAbilityPush)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Current ability is a push ability");
					num2 = cAbilityPush.RemainingPushes;
					eWaypointType = EWaypointType.Push;
					fly = cAbilityPush.CurrentTarget.FlyingAfterConditions || cAbilityPush.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles);
					ignoreDifficultTerrain = true;
				}
				else if (WorldspaceStarHexDisplay.Instance.CurrentAbilityDisplayType == WorldspaceStarHexDisplay.EAbilityDisplayType.SelectPath && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack cAbilityAttack)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Current ability is an attack ability with path selection");
					num2 = cAbilityAttack.Range;
					eWaypointType = EWaypointType.Attack;
					fly = true;
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2 cAbilityTeleport)
				{
					if ((FFSNetwork.IsOnline & networkActionIfOnline) && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl())
					{
						Synchronizer.SendGameAction(GameActionType.CreateWaypoint, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, SaveData.Instance.Global.EnableSecondClickHexToConfirm, null, new TilesToken(s_ClearValidSelectionTiles));
					}
					if (cAbilityTeleport.GetTeleportTiles(cAbilityTeleport.ActorToTeleport.ArrayIndex, cAbilityTeleport.Range).Count == 1)
					{
						s_LockWaypoints = true;
					}
					Create(clientTile, 0, 0, 0, 0, eWaypointType, fly, tileIsBlocked: false);
					return;
				}
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Checking paths to new potential waypoint (" + clientTile.m_Tile.m_ArrayIndex.ToString() + ")");
				int num3 = ((s_Waypoints.Count > 0) ? s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().MovesRemaining : num2);
				List<Point> list = CActor.FindCharacterPath(s_MovingActor, point, clientTile.m_Tile.m_ArrayIndex, jump | fly, ignoreMoveCost, out var foundPath, ignoreDifficultTerrain, ignoreHazardousTerrain, num, carryOtherActors, carryType, logFailure: false, num3, ignoreCannotStop: true, jump, fly, ignoreDoorLocks);
				int num4 = CAbilityMove.CalculateMoveCost(s_MovingActor, point, list, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num, freeTileAwayFromCaster);
				if (!foundPath || num4 > num3)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: No path found avoiding traps, trying to find a path including traps");
					list = CActor.FindCharacterPath(s_MovingActor, point, clientTile.m_Tile.m_ArrayIndex, jump | fly, ignoreMoveCost, out foundPath, ignoreDifficultTerrain, ignoreNegativeTiles: true, num, carryOtherActors, carryType, logFailure: false, num3, ignoreCannotStop: false, jump, fly, ignoreDoorLocks);
				}
				if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { AdditionalPushEffect: var additionalPushEffect } cAbilityPush2 && additionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.TrackBlocked) && clientTile.m_Tile.m_ArrayIndex == cAbilityPush2.CurrentTarget.ArrayIndex)
				{
					foundPath = true;
				}
				int num5 = CAbilityMove.CalculateMoveCost(s_MovingActor, point, list, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, num, freeTileAwayFromCaster);
				int num6 = Mathf.Max(num3 - num5, 0);
				int remainingWaterTilesThatCostZero = Mathf.Max(num - list.Count((Point x) => ScenarioManager.GetTile(x).IsWater), 0);
				int movesAvailable = 0;
				if (num3 == 0 && s_Waypoints.Count > 0 && !s_Waypoints[s_Waypoints.Count - 1].GetComponent<Waypoint>().ClientTile.m_Tile.IsIce)
				{
					return;
				}
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Found path: " + foundPath + ", AbilityMovesRemaining: " + num3 + ", MoveCost: " + num5);
				if (foundPath && num3 >= num5)
				{
					Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Found path and moves remaining is >= to move cost so we can create the waypoint");
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
					bool tileIsBlocked = false;
					if (eWaypointType == EWaypointType.Pull || eWaypointType == EWaypointType.Push)
					{
						if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPull cAbilityPull2)
						{
							movesAvailable = CAbilityPull.GetPullTiles(cAbilityPull2.CurrentTarget, cAbilityPull2.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPull2.PullToPoint, cAbilityPull2.PullType, num6, cAbilityPull2).Count;
						}
						else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush { CurrentTarget: var currentTarget } cAbilityPush3)
						{
							movesAvailable = CAbilityPush.GetPushTiles(currentTarget, cAbilityPush3.CurrentTarget.Type, clientTile.m_Tile.m_ArrayIndex, cAbilityPush3.PushFromPoint, num6, cAbilityPush3.AdditionalPushEffect.Equals(CAbilityPush.EAdditionalPushEffect.IntoObstacles), cAbilityPush3).Count;
						}
						else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityTeleport2 cAbilityTeleport2)
						{
							movesAvailable = cAbilityTeleport2.GetTeleportTiles(cAbilityTeleport2.ActorToTeleport.ArrayIndex, cAbilityTeleport2.Range).Count;
						}
					}
					if ((FFSNetwork.IsOnline & networkActionIfOnline) && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl())
					{
						Synchronizer.SendGameAction(GameActionType.CreateWaypoint, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, SaveData.Instance.Global.EnableSecondClickHexToConfirm, null, new TilesToken(s_ClearValidSelectionTiles, clientTile.m_Tile));
					}
					Create(clientTile, num6, num5, movesAvailable, remainingWaterTilesThatCostZero, eWaypointType, fly, tileIsBlocked, list);
					if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityAttack)
					{
						ScenarioRuleClient.UpdateAttackpath(GetCTileList());
					}
					if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityMove cAbilityMove2)
					{
						if (flag)
						{
							cAbilityMove.InvalidTileReason = "GUI_TOOLTIP_SELECT_PATH_THROUGH_DOOR";
							Choreographer.s_Choreographer.readyButton.SetInteractable(interactable: false);
							Singleton<HelpBox>.Instance.ShowTranslated(null);
						}
						else
						{
							cAbilityMove.InvalidTileReason = null;
							Singleton<HelpBox>.Instance.ShowTranslated(null);
						}
						if (cAbilityMove2.CarryOtherActorsOnHex)
						{
							ScenarioRuleClient.UpdateMoveCarry(GetCTileList());
						}
					}
				}
				else if (Choreographer.s_Choreographer.m_CurrentAbility != null && Choreographer.s_Choreographer.m_CurrentAbility is CAbilityPush)
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.tileClickAudioItem);
				}
				else
				{
					AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				}
			}
			else
			{
				Log.SimpleInfo("[WAYPOINT TILE HANDLER]: Selected tile " + clientTile.m_Tile.m_ArrayIndex.ToString() + " was not found in the valid selection tile list");
			}
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the Waypoint.TileHandler().");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_WAYPOINT_00012", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public static void ProxySelectTileToCreateMovementPath(GameAction action)
	{
		Log.NetworkInfo("PROXY: Creating movement path.", true);
		TilesToken tilesToken = (TilesToken)action.SupplementaryDataToken;
		ClearValidSelectionTiles();
		for (int i = 0; i < tilesToken.Tiles.GetLength(0); i++)
		{
			int x = tilesToken.Tiles[i, 0];
			int y = tilesToken.Tiles[i, 1];
			AddValidSelectionTiles(new Point(x, y));
			Log.NetworkInfo("X: " + x + ", Y: " + y, true);
		}
		int num;
		int num2;
		if (tilesToken.HasClickedTile)
		{
			num = tilesToken.ClickedTileX;
			num2 = tilesToken.ClickedTileY;
		}
		else
		{
			int num3 = tilesToken.Tiles.GetLength(0) - 1;
			num = tilesToken.Tiles[num3, 0];
			num2 = tilesToken.Tiles[num3, 1];
		}
		TileHandler(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[num, num2], null, networkActionIfOnline: false, isUserClick: false, action.SupplementaryDataBoolean);
	}

	public static void ProxyDeleteLastWaypoint()
	{
		Log.NetworkInfo("PROXY: About to delete the last waypoint.");
		DeleteLastWaypoint();
	}

	public static void ProxyPressWaypoint(GameAction action)
	{
		Log.NetworkInfo("PROXY: About to press a waypoint.");
		TileToken tileToken = (TileToken)action.SupplementaryDataToken;
		foreach (GameObject s_Waypoint in s_Waypoints)
		{
			Waypoint waypointComponent = GetWaypointComponent(s_Waypoint);
			if (waypointComponent != null)
			{
				if (waypointComponent.m_ClientTile.m_Tile.m_ArrayIndex.X == tileToken.TileX && waypointComponent.m_ClientTile.m_Tile.m_ArrayIndex.Y == tileToken.TileY)
				{
					Log.NetworkInfo("PROXY: Waypoint found. Pressing waypoint.");
					waypointComponent.OnPress();
					return;
				}
				continue;
			}
			throw new Exception("Error pressing waypoint for proxy playerActor. Waypoint returns null.");
		}
		throw new Exception("Error selecting waypoint for proxy playerActor. No such waypoint exists (X = " + tileToken.TileX + ", Y = " + tileToken.TileY + ")");
	}
}

using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMoveShadow : CAbility
{
	public enum EMoveShadowState
	{
		SelectShadowToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingShadow,
		MovingShadow,
		MoveShadow,
		MovedShadow,
		MoveShadowDone
	}

	public bool HightlightOriginTile;

	private EMoveShadowState m_State;

	private List<CTile> m_SelectedShadowTiles;

	private CTile m_SelectedShadowTile;

	private CTile m_TileToMoveTo;

	private int m_MovingShadowIndex;

	private int m_ShadowsToMove;

	private Dictionary<string, int> m_MovedShadowDictionary;

	private List<CObjectProp> m_movedShadows;

	private List<CTile> m_FinalShadowPositions;

	public List<CTile> FinalShadowPositions => m_FinalShadowPositions;

	public List<CTile> InitialShadowPositions => m_SelectedShadowTiles;

	public CTile SelectedShadowTile => m_SelectedShadowTile;

	public CTile TileToMoveTo => m_TileToMoveTo;

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.Shadow;
		}
	}

	public override string SpawnPropPreview
	{
		get
		{
			string spawnPropPreview = base.SpawnPropPreview;
			if (!string.IsNullOrEmpty(spawnPropPreview))
			{
				return spawnPropPreview;
			}
			return EPropType.Shadow.ToString();
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EMoveShadowState.SelectShadowToMove;
		if (NumberTargets == -1)
		{
			List<CTile> list = (from shadow in ScenarioManager.CurrentScenarioState.Props
				where shadow.ObjectType == ScenarioManager.ObjectImportType.Shadow
				select shadow.PropTile).ToList();
			SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
			NumberTargets = list.Count();
		}
		m_ShadowsToMove = NumberTargets;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_MovingShadowIndex = 0;
		m_SelectedShadowTiles = new List<CTile>();
		m_MovedShadowDictionary = new Dictionary<string, int>();
		m_movedShadows = new List<CObjectProp>();
		m_FinalShadowPositions = new List<CTile>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		switch (m_State)
		{
		case EMoveShadowState.SelectShadowToMove:
		{
			HightlightOriginTile = false;
			DontMoveAbilityState = false;
			m_SelectedShadowTile = null;
			if (TargetingActor.Type != CActor.EType.Player)
			{
				break;
			}
			HelpBoxTooltipLocKey = "FH/GUI_SELECT_SHADOW_MOVE_HELPBOX";
			TilesInRange = GameState.GetTilesInRange(TargetingActor.ArrayIndex, 99).ToList();
			List<CTile> list2 = new List<CTile>();
			foreach (CTile item2 in TilesInRange)
			{
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
				CObjectProp cObjectProp = item2.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
				if (item2.m_HexMap.Revealed && cObjectProp != null && cNode2.Walkable && !m_movedShadows.Contains(cObjectProp))
				{
					list2.Add(item2);
				}
			}
			SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list2, this);
			TilesInRange = list2.ToList();
			m_TilesSelected.Clear();
			NumberTargets = 1;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Shadow },
				m_IsPositive = IsPositive(),
				m_SpawnType = ScenarioManager.ObjectImportType.None
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case EMoveShadowState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = EMoveShadowState.SelectMoveToTile;
			Perform();
			break;
		case EMoveShadowState.SelectMoveToTile:
		{
			m_CanUndo = false;
			HightlightOriginTile = true;
			AbilityHasHappened = true;
			m_SelectedShadowTiles.AddRange(m_TilesSelected);
			m_SelectedShadowTile = m_TilesSelected[0];
			m_TilesSelected.Clear();
			NumberTargets = 1;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			TilesInRange = GameState.GetTilesInRange(m_SelectedShadowTile.m_ArrayIndex, Range, EAbilityTargeting.AllConnectedRooms, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: true, ignoreMoveCost: false, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: false, checkTeleport: true).ToList();
			List<CTile> list = new List<CTile>();
			foreach (CTile item3 in TilesInRange)
			{
				if (ScenarioScript.current?.TilesCannotStopOnTopOf()?.Contains(item3) == true || !item3.IsWalkable)
				{
					continue;
				}
				CNode cNode = ScenarioManager.PathFinder.Nodes[item3.m_ArrayIndex.X, item3.m_ArrayIndex.Y];
				if (!item3.m_HexMap.Revealed || item3.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow) || !cNode.Walkable)
				{
					continue;
				}
				if (GetVar("TargetUnoccupiedTiles").GetBool())
				{
					if (ScenarioManager.Scenario.FindActorsAt(item3.m_ArrayIndex).Count == 0 && !cNode.Blocked)
					{
						list.Add(item3);
					}
				}
				else
				{
					list.Add(item3);
				}
			}
			SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
			m_TileToMoveTo = m_SelectedShadowTile;
			TilesInRange = list.ToList();
			if (TargetingActor.Type == CActor.EType.Player)
			{
				HelpBoxTooltipLocKey = "FH/GUI_SELECT_HEX_SHADOW_MOVE_HELPBOX";
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		}
		case EMoveShadowState.PreMovingShadow:
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CObjectShadow item = m_SelectedShadowTile.FindProp(ScenarioManager.ObjectImportType.Shadow) as CObjectShadow;
			m_movedShadows.Add(item);
			m_State = EMoveShadowState.MovingShadow;
			Perform();
			break;
		}
		case EMoveShadowState.MovingShadow:
			if (m_TilesSelected.Count != 0)
			{
				CPlayCharacterAnimation_MessageData cPlayCharacterAnimation_MessageData = new CPlayCharacterAnimation_MessageData("Shadow Manipulation", TargetingActor);
				cPlayCharacterAnimation_MessageData.m_TileToLookAt = m_TilesSelected[0];
				cPlayCharacterAnimation_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayCharacterAnimation_MessageData);
			}
			else
			{
				m_State = EMoveShadowState.MoveShadow;
				Perform();
			}
			break;
		case EMoveShadowState.MoveShadow:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingShadowIndex < m_ShadowsToMove)
				{
					m_TileToMoveTo = m_TilesSelected[0];
					CObjectShadow cObjectShadow = m_SelectedShadowTile.FindProp(ScenarioManager.ObjectImportType.Shadow) as CObjectShadow;
					MoveShadow(cObjectShadow, m_TileToMoveTo);
					m_MovedShadowDictionary.TryGetValue(cObjectShadow.PrefabName, out var value);
					m_MovedShadowDictionary[cObjectShadow.PrefabName] = value + 1;
					m_State = EMoveShadowState.MovedShadow;
					m_FinalShadowPositions.Add(m_TilesSelected[0]);
					CCombatLogMessage_MessageData message2 = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "COMBAT_LOG_MOVE_PROP",
						m_Actors = new List<CActor> { TargetingActor },
						m_Params = new List<string> { "Actor_0", "<sprite name=DWLShadow>" }
					};
					ScenarioRuleClient.MessageHandler(message2);
					Perform();
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EMoveShadowState.MovedShadow:
			m_MovingShadowIndex++;
			CActiveBonus.RefreshAllAuraActiveBonuses(this);
			if (m_MovingShadowIndex <= m_ShadowsToMove - 1)
			{
				m_State = EMoveShadowState.SelectShadowToMove;
				Perform();
			}
			else
			{
				PhaseManager.StepComplete();
			}
			break;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public static void MoveShadow(CObjectShadow shadow, CTile newTile)
	{
		shadow.PropTile.DelProp(shadow);
		newTile.AddProp(shadow);
		shadow.SetNewStartingMapGuid(newTile.m_HexMap.MapGuid);
		CMoveProp_MessageData message = new CMoveProp_MessageData
		{
			m_MoveProp = shadow,
			m_MoveToTile = newTile,
			m_MoveSpeed = 3.4f
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override void AbilityPassStep()
	{
		if (m_State == EMoveShadowState.SelectShadowToMove)
		{
			DontMoveAbilityState = true;
			m_State = EMoveShadowState.MoveShadowDone;
		}
		if (m_State == EMoveShadowState.SelectMoveToTile)
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CObjectShadow item = m_SelectedShadowTile.FindProp(ScenarioManager.ObjectImportType.Shadow) as CObjectShadow;
			m_movedShadows.Add(item);
			if (m_movedShadows.Count < m_ShadowsToMove)
			{
				DontMoveAbilityState = true;
			}
			m_State = EMoveShadowState.MovedShadow;
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!TilesSelected.Contains(selectedTile))
		{
			if (m_State == EMoveShadowState.SelectShadowToMove && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Shadow) is CObjectShadow cObjectShadow)
				{
					CTile cTile = ScenarioManager.Tiles[cObjectShadow.ArrayIndex.X, cObjectShadow.ArrayIndex.Y];
					if (cTile != null)
					{
						m_TilesSelected.Add(cTile);
						m_NumberTargetsRemaining--;
					}
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			else if (m_State == EMoveShadowState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
					m_NumberTargetsRemaining--;
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message2 = new CPlayerSelectedTile_MessageData(TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
				else
				{
					ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(TargetingActor));
				}
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EMoveShadowState.SelectShadowToMove)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != EMoveShadowState.SelectShadowToMove)
		{
			return m_State == EMoveShadowState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveShadowState.SelectShadowToMove)
			{
				return m_State == EMoveShadowState.SelectMoveToTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EMoveShadowState.MoveShadowDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = EMoveShadowState.SelectShadowToMove;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TilesSelected.Clear();
		if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_SelectedShadowTiles = new List<CTile>();
		m_MovedShadowDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == TargetingActor.ActorGuid)?.IsSummon;
		}
	}

	public override string GetDescription()
	{
		return "MoveShadow";
	}

	public bool HasPassedState(EMoveShadowState moveShadowState)
	{
		return m_State > moveShadowState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}

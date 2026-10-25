using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMoveTrap : CAbility
{
	public enum EMoveTrapState
	{
		SelectTrapToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingTrap,
		RemoveTrap,
		PlaceTrap,
		MovedTrap,
		MoveTrapDone
	}

	private EMoveTrapState m_State;

	private List<CTile> m_OriginalTilesInRange = new List<CTile>();

	private List<CTile> m_SelectedTrapTiles;

	private int m_MovingTrapIndex;

	private string m_MovingPropName;

	private Dictionary<string, int> m_MovedTrapDictionary;

	private List<CTile> m_FinalTrapPositions;

	private int m_OriginalNumberOfTargets;

	public bool OneTargetAtATime = true;

	private AbilityData.TrapData m_MovingTrapData;

	[JsonProperty]
	public int MoveTrapRange { get; set; }

	public List<CTile> FinalTrapPositions => m_FinalTrapPositions;

	public List<CTile> InitialTrapPositions => m_SelectedTrapTiles;

	public IEnumerable<CTile> OriginalTilesWithTrapsInRange => m_OriginalTilesInRange.Where((CTile x) => x.Trap != null);

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.Trap;
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
			return EPropType.Trap.ToString();
		}
	}

	public CAbilityMoveTrap(int moveTrapRange)
	{
		MoveTrapRange = moveTrapRange;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EMoveTrapState.SelectTrapToMove;
		if (Targeting == EAbilityTargeting.Custom)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else if (AreaEffect == null)
		{
			TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, skipPathingCheck: false, IncludeCasterTile());
		}
		List<CTile> list = new List<CTile>();
		foreach (CTile item in TilesInRange)
		{
			if (CAbilityFilter.IsValidTile(item, CAbilityFilter.EFilterTile.Trap, includeInitial: false, null, null, VarsContext))
			{
				list.Add(item);
			}
		}
		TilesInRange = list.ToList();
		m_OriginalTilesInRange = TilesInRange.ToList();
		if (NumberTargets < 0)
		{
			NumberTargets = TilesInRange.Count;
		}
		if (TilesInRange.Count <= NumberTargets && TilesSelected.Count == 0 && !OneTargetAtATime)
		{
			TilesSelected.AddRange(TilesInRange);
		}
		m_NumberTargetsRemaining = NumberTargets - TilesSelected.Count;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_OriginalNumberOfTargets = NumberTargets;
		m_SelectedTrapTiles = new List<CTile>();
		m_MovedTrapDictionary = new Dictionary<string, int>();
		m_FinalTrapPositions = new List<CTile>();
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
		case EMoveTrapState.SelectTrapToMove:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Trap },
					m_IsPositive = IsPositive(),
					m_SpawnType = ScenarioManager.ObjectImportType.None
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		case EMoveTrapState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = EMoveTrapState.SelectMoveToTile;
			Perform();
			break;
		case EMoveTrapState.SelectMoveToTile:
		{
			m_CanUndo = false;
			m_SelectedTrapTiles.AddRange(m_TilesSelected);
			m_TilesSelected.Clear();
			NumberTargets = m_SelectedTrapTiles.Count;
			m_NumberTargetsRemaining = NumberTargets;
			Targeting = EAbilityTargeting.Range;
			Range = MoveTrapRange;
			CTile cTile = GetVar("MoveAroundTile").GetObject<CTile>();
			if (cTile == null)
			{
				cTile = TargetingActor.Tile;
			}
			TilesInRange = GameState.GetTilesInRange(cTile.m_ArrayIndex, Range, Targeting, emptyTilesOnly: true, ignoreBlocked: true);
			List<CTile> list = new List<CTile>();
			foreach (CTile item3 in TilesInRange)
			{
				new List<CTile>();
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext))
				{
					list.Add(item3);
				}
			}
			TilesInRange = list.ToList();
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			break;
		}
		case EMoveTrapState.PreMovingTrap:
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			AbilityHasHappened = true;
			m_MovingTrapIndex = 0;
			foreach (CTile selectedTrapTile in m_SelectedTrapTiles)
			{
				if (selectedTrapTile.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap item2)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(item2);
				}
			}
			if (IsMergedAbility)
			{
				PhaseManager.StepComplete();
				break;
			}
			CMovingTrap_MessageData message2 = new CMovingTrap_MessageData(AnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_From = m_SelectedTrapTiles,
				m_To = TilesSelected
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case EMoveTrapState.RemoveTrap:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingTrapIndex < m_SelectedTrapTiles.Count)
				{
					CObjectTrap cObjectTrap = m_SelectedTrapTiles[m_MovingTrapIndex].FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) as CObjectTrap;
					m_MovingTrapData = new AbilityData.TrapData
					{
						Damage = (cObjectTrap.Damage ? cObjectTrap.GetTrapDamage() : 0),
						Heal = cObjectTrap.HealValue,
						Conditions = cObjectTrap.Conditions?.ToList(),
						PositiveConditions = cObjectTrap.PositiveConditions?.ToList(),
						AdjacentRange = cObjectTrap.AdjacentRange,
						AdjacentDamage = cObjectTrap.AdjacentDamageValue,
						AdjacentConditions = cObjectTrap.AdjacentConditions?.ToList(),
						AdjacentFilter = cObjectTrap.AdjacentFilter?.Copy(),
						TriggeredXP = cObjectTrap.TriggeredXP,
						TrapName = cObjectTrap.PrefabName
					};
					m_MovingPropName = cObjectTrap.PrefabName;
					cObjectTrap.DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, this);
					m_MovedTrapDictionary.TryGetValue(cObjectTrap.PrefabName, out var value);
					m_MovedTrapDictionary[cObjectTrap.PrefabName] = value + 1;
				}
				m_State = EMoveTrapState.PlaceTrap;
				Perform();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EMoveTrapState.PlaceTrap:
		{
			CObjectTrap prop = new CObjectTrap(m_MovingPropName, ScenarioManager.ObjectImportType.Trap, new TileIndex(m_TilesSelected[m_MovingTrapIndex].m_ArrayIndex), null, null, TargetingActor, m_TilesSelected[0].m_HexMap.MapGuid, m_MovingTrapData);
			m_TilesSelected[m_MovingTrapIndex].SpawnProp(prop, notifyClient: true, SpawnDelay, this);
			m_FinalTrapPositions.Add(m_TilesSelected[m_MovingTrapIndex]);
			AbilityData.TrapData movingTrapData = m_MovingTrapData;
			CLogMovedTrapMessage_MessageData message4 = new CLogMovedTrapMessage_MessageData(TargetingActor)
			{
				m_Event = new SEventAbilityTrap(new SEventAbility.SEventAbilityBaseData(TargetingActor, ESESubTypeAbility.AbilityPerform, this, defaultAction: false, movingTrapData.Damage), CAbilityTrap.TrapState.TrapPlaced, movingTrapData.TrapName, movingTrapData.TriggeredXP, movingTrapData.Heal, movingTrapData.Conditions?.ToList(), movingTrapData.PositiveConditions?.ToList())
			};
			ScenarioRuleClient.MessageHandler(message4);
			m_State = EMoveTrapState.MovedTrap;
			Perform();
			break;
		}
		case EMoveTrapState.MovedTrap:
		{
			CTile item = InitialTrapPositions[m_MovingTrapIndex];
			m_MovingTrapIndex++;
			if (m_MovingTrapIndex <= m_TilesSelected.Count - 1)
			{
				m_State = EMoveTrapState.RemoveTrap;
				Perform();
				break;
			}
			m_OriginalTilesInRange.Remove(item);
			NumberTargetsRemaining = m_OriginalNumberOfTargets - 1;
			m_OriginalNumberOfTargets = NumberTargetsRemaining;
			if (OriginalTilesWithTrapsInRange.Any() && NumberTargetsRemaining > 0)
			{
				m_MovingTrapIndex = 0;
				m_State = EMoveTrapState.SelectTrapToMove;
				m_SelectedTrapTiles.Clear();
				m_TilesSelected.Clear();
				TilesInRange = m_OriginalTilesInRange.ToList();
				Perform();
			}
			else
			{
				PhaseManager.StepComplete();
			}
			break;
		}
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message5 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
			else
			{
				CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!TilesSelected.Contains(selectedTile) && (!OneTargetAtATime || TilesSelected.Count == 0))
		{
			if (m_State == EMoveTrapState.SelectTrapToMove && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap cObjectTrap)
				{
					CTile cTile = ScenarioManager.Tiles[cObjectTrap.ArrayIndex.X, cObjectTrap.ArrayIndex.Y];
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
			else if (m_State == EMoveTrapState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				new List<CTile>();
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
		if (m_State == EMoveTrapState.SelectMoveToTile)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != EMoveTrapState.SelectTrapToMove)
		{
			return m_State == EMoveTrapState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveTrapState.SelectTrapToMove)
			{
				return m_State == EMoveTrapState.SelectMoveToTile;
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
		return m_State == EMoveTrapState.MoveTrapDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = EMoveTrapState.SelectTrapToMove;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TilesSelected.Clear();
		if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_SelectedTrapTiles = new List<CTile>();
		m_MovedTrapDictionary = new Dictionary<string, int>();
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
		return "MoveTrap";
	}

	public bool HasPassedState(EMoveTrapState moveTrapState)
	{
		return m_State > moveTrapState;
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

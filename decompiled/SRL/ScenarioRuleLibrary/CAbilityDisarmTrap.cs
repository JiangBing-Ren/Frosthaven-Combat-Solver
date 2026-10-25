using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityDisarmTrap : CAbility
{
	public enum DisarmTrapState
	{
		SelectTrapPosition,
		DisarmingTrap,
		DisarmTrap,
		SelectActorToTargetWithTrapEffects,
		AffectingActorWithTrapEffects,
		DisarmTrapDone
	}

	private DisarmTrapState m_State;

	private Dictionary<string, int> m_disarmedTrapsDictionary;

	private CObjectTrap m_lastTrapDisarmed;

	[JsonProperty]
	public bool TargetActorWithTrapEffects { get; set; }

	[JsonProperty]
	public int TargetActorWithTrapEffectRange { get; set; }

	public CAbilityDisarmTrap(bool targetActorWithTrapEffects, int targetActorWithTrapEffectRange)
	{
		TargetActorWithTrapEffects = targetActorWithTrapEffects;
		TargetActorWithTrapEffectRange = targetActorWithTrapEffectRange;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = DisarmTrapState.SelectTrapPosition;
		if (AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
			{
				if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						TilesInRange.Add(cTile);
					}
				}
			}
		}
		else if (Targeting == EAbilityTargeting.Custom)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else if (AreaEffect == null)
		{
			bool includeStartingTile = IncludeCasterTile();
			TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, skipPathingCheck: false, includeStartingTile);
			if (Range > 1)
			{
				TilesInRange.Add(ScenarioManager.GetAdjacentTile(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter));
			}
		}
		if (NumberTargets == -1 || Targeting == EAbilityTargeting.All || Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		for (int num = TilesInRange.Count - 1; num >= 0; num--)
		{
			if (!(TilesInRange[num].FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap { Activated: false }))
			{
				TilesInRange.RemoveAt(num);
			}
		}
		if (m_AllTargets)
		{
			NumberTargets = TilesInRange.Count;
		}
		if (TilesInRange.Count <= NumberTargets && TilesSelected.Count == 0)
		{
			TilesSelected.AddRange(TilesInRange);
		}
		m_NumberTargetsRemaining = NumberTargets - TilesSelected.Count;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_disarmedTrapsDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
		AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
		if (miscAbilityData != null && miscAbilityData.AutotriggerAbility == true)
		{
			m_State = DisarmTrapState.DisarmingTrap;
		}
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
		case DisarmTrapState.SelectTrapPosition:
			if (UseSubAbilityTargeting)
			{
				if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
					{
						TileSelected(inlineSubAbilityTile, null);
					}
					break;
				}
				if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
				{
					foreach (CTile item in ParentAbility.TilesSelected)
					{
						TileSelected(item, null);
					}
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
					m_TilesSelected.Clear();
					foreach (CTile item2 in TilesInRange)
					{
						if (item2.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap { Activated: false } && !m_TilesSelected.Contains(item2))
						{
							m_TilesSelected.Add(item2);
						}
					}
					if (m_TilesSelected.Count <= 0)
					{
						SetCancelAbilityFlag(cancel: true);
						if (IsMergedAbility)
						{
							PhaseManager.StepComplete();
						}
						else
						{
							PhaseManager.NextStep();
						}
					}
					NumberTargets = m_TilesSelected.Count;
				}
				m_NumberTargetsRemaining = NumberTargets - m_TilesSelected.Count;
				m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
				if (!AbilityHasBeenCancelled)
				{
					CPlayerSelectingObjectPosition_MessageData message4 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
					{
						m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Trap },
						m_IsPositive = IsPositive(),
						m_SpawnType = ScenarioManager.ObjectImportType.None
					};
					ScenarioRuleClient.MessageHandler(message4);
				}
				break;
			}
			foreach (CTile item3 in TilesInRange)
			{
				if (item3.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap { Activated: false })
				{
					if (!m_TilesSelected.Contains(item3))
					{
						m_TilesSelected.Add(item3);
					}
					m_State = DisarmTrapState.DisarmingTrap;
					Perform();
					break;
				}
			}
			break;
		case DisarmTrapState.DisarmingTrap:
			if (m_TilesSelected.Count > 0)
			{
				CDisarmTrap_MessageData message3 = new CDisarmTrap_MessageData(AnimOverload, TargetingActor)
				{
					m_ActorDisarmingTrap = TargetingActor,
					m_Tiles = m_TilesSelected.ToList(),
					m_DisarmedTraps = m_TilesSelected.Select((CTile t) => t.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) as CObjectTrap).ToList(),
					m_DisarmTrapAbility = this
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				m_State = DisarmTrapState.DisarmTrap;
				Perform();
			}
			break;
		case DisarmTrapState.DisarmTrap:
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				foreach (CTile item4 in m_TilesSelected)
				{
					if (!(item4.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap { Activated: false } cObjectTrap))
					{
						continue;
					}
					AbilityHasHappened = true;
					cObjectTrap.DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, this);
					string characterID = "";
					if (TargetingActor != null)
					{
						if (TargetingActor is CPlayerActor cPlayerActor)
						{
							characterID = cPlayerActor.CharacterClass.ID;
						}
						if (TargetingActor is CHeroSummonActor { Summoner: var summoner })
						{
							characterID = summoner?.CharacterClass.ID;
						}
						SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropTrap(0, characterID, "Disarm", ESESubTypeObjectProp.Activated, cObjectTrap.ObjectType, cObjectTrap.PropGuid, cObjectTrap.PrefabName, cObjectTrap.Owner?.ActorGuid, "", null, cObjectTrap.ArrayIndex.ToPoint()));
					}
					m_disarmedTrapsDictionary.TryGetValue(cObjectTrap.PrefabName, out var value);
					m_disarmedTrapsDictionary[cObjectTrap.PrefabName] = value + 1;
					if (TargetActorWithTrapEffects)
					{
						m_lastTrapDisarmed = cObjectTrap;
					}
				}
			}
			m_State = DisarmTrapState.SelectActorToTargetWithTrapEffects;
			Perform();
			break;
		case DisarmTrapState.SelectActorToTargetWithTrapEffects:
			m_TilesSelected.Clear();
			if (TargetActorWithTrapEffects && TargetingActor.Type == CActor.EType.Player)
			{
				Range = TargetActorWithTrapEffectRange;
				ValidActorsInRange = GameState.GetActorsInRange(TargetingActor, FilterActor, TargetActorWithTrapEffectRange, m_ActorsToIgnore, AbilityFilter, null, AllPossibleTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
				m_ActorsToTarget.Clear();
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = false;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			else
			{
				m_State = DisarmTrapState.AffectingActorWithTrapEffects;
				Perform();
			}
			break;
		case DisarmTrapState.AffectingActorWithTrapEffects:
			if (TargetActorWithTrapEffects && TargetingActor.Type == CActor.EType.Player)
			{
				foreach (CActor item5 in m_ActorsToTarget)
				{
					if (!ScenarioManager.Scenario.HasActor(item5))
					{
						continue;
					}
					if (GameState.ActorHealthCheck(item5, item5) && m_lastTrapDisarmed.Conditions.Count > 0)
					{
						foreach (CCondition.ENegativeCondition condition in m_lastTrapDisarmed.Conditions)
						{
							EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((EAbilityType s) => s.ToString() == condition.ToString());
							if (condition != CCondition.ENegativeCondition.NA && eAbilityType != EAbilityType.None)
							{
								CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
								cAbility.Start(TargetingActor, TargetingActor);
								((CAbilityTargeting)cAbility).ApplyToActor(item5);
								if (eAbilityType == EAbilityType.Stun || eAbilityType == EAbilityType.Immobilize || eAbilityType == EAbilityType.Sleep)
								{
									CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
									ScenarioRuleClient.MessageHandler(message);
								}
							}
							else
							{
								Log.ErrorWithReport("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
							}
						}
					}
					int health = item5.Health;
					bool actorWasAsleep = item5.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					if (m_lastTrapDisarmed.Damage)
					{
						GameState.ActorBeenDamaged(item5, m_lastTrapDisarmed.GetTrapDamage(), checkIfPlayerCanAvoidDamage: true, TargetingActor, null, EAbilityType.DisarmTrap, 0, isTrapDamage: true);
					}
					if ((item5.Type != CActor.EType.Player || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None || !m_lastTrapDisarmed.Damage) && GameState.ActorHealthCheck(item5, item5, isTrap: false, isTerrain: false, actorWasAsleep))
					{
						CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(item5)
						{
							m_ActorBeingDamaged = item5,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
			}
			if (IsMergedAbility)
			{
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
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
		if (!AllTargets && !TilesSelected.Contains(selectedTile) && TilesSelected.Count < NumberTargets)
		{
			if (m_State == DisarmTrapState.SelectTrapPosition)
			{
				if (TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Trap, excludeHealth: true) is CObjectTrap { Activated: false })
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
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
			else if (TargetActorWithTrapEffects && m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				if (m_ValidActorsInRange.Contains(actorOnTile))
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						TilesSelected.Add(selectedTile);
						m_NumberTargetsRemaining--;
					}
					m_ActorsToTarget.Add(actorOnTile);
					CActorSelectedAttackFocus_MessageData message2 = new CActorSelectedAttackFocus_MessageData(TargetingActor)
					{
						m_AttackingActor = TargetingActor,
						m_Ability = this,
						m_AttackFocus = actorOnTile,
						m_Adding = false
					};
					ScenarioRuleClient.MessageHandler(message2);
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
		if (!AllTargets && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (m_State == DisarmTrapState.SelectTrapPosition)
			{
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
					{
						m_Ability = this
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else if (TargetActorWithTrapEffects && m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				m_ActorsToTarget.Remove(actorOnTile);
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
		if (m_State == DisarmTrapState.SelectTrapPosition || m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != DisarmTrapState.SelectTrapPosition)
		{
			return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != DisarmTrapState.SelectTrapPosition)
			{
				return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
			}
			return true;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == DisarmTrapState.DisarmTrapDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDisarmTrap(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_disarmedTrapsDictionary));
	}

	public override string GetDescription()
	{
		return "DisarmTrap";
	}

	public bool HasPassedState(DisarmTrapState disarmState)
	{
		return m_State > disarmState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return m_State == DisarmTrapState.SelectActorToTargetWithTrapEffects;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		int num = GetMiscVar("MinTargets").GetInt(null, 1);
		return m_TilesSelected.Count >= num;
	}
}

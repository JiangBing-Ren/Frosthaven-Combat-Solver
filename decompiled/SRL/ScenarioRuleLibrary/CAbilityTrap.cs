using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityTrap : CAbility
{
	public enum TrapState
	{
		CheckForDelayedDrops,
		SelectTrapPosition,
		SpawningTrap,
		SpawnTrap,
		TrapPlaced,
		TrapDone,
		NoTilesSelected
	}

	private TrapState m_State;

	private bool m_TrapPlaced;

	[JsonProperty]
	public AbilityData.TrapData TrapData { get; set; }

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
			if (!string.IsNullOrEmpty(TrapData.TrapName))
			{
				return TrapData.TrapName;
			}
			return EPropType.Trap.ToString();
		}
	}

	public CAbilityTrap(AbilityData.TrapData trapData)
	{
		TrapData = trapData;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = TrapState.CheckForDelayedDrops;
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		else
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		m_TrapPlaced = false;
		if (GetMiscVar("ScenarioTrap").GetBool())
		{
			TrapData.Damage = ScenarioManager.TrapDamage;
		}
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
		case TrapState.CheckForDelayedDrops:
		{
			CWaitForDelayedDrops_MessageData message4 = new CWaitForDelayedDrops_MessageData();
			ScenarioRuleClient.MessageHandler(message4);
			return true;
		}
		case TrapState.SelectTrapPosition:
		{
			if (Targeting == EAbilityTargeting.Custom || TargetingActor is CHeroSummonActor)
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
				if (TrapData.PlacementTileFilter != CAbilityFilter.EFilterTile.None)
				{
					TilesInRange.RemoveAll((CTile tile) => !CAbilityFilter.IsValidTile(tile, TrapData.PlacementTileFilter, includeInitial: false, null, null, VarsContext) || tile.FindProp(ScenarioManager.ObjectImportType.Trap) != null);
					TilesSelected.RemoveAll((CTile tile) => !CAbilityFilter.IsValidTile(tile, TrapData.PlacementTileFilter, includeInitial: false, null, null, VarsContext) || tile.FindProp(ScenarioManager.ObjectImportType.Trap) != null);
				}
				if ((MiscAbilityData == null || !MiscAbilityData.AutotriggerAbility.HasValue) ? UseSubAbilityTargeting : MiscAbilityData.AutotriggerAbility.Value)
				{
					if (m_TilesSelected.Count > 0)
					{
						m_State = TrapState.SpawningTrap;
						Perform();
					}
					else
					{
						SetCancelAbilityFlag(cancel: true);
						PhaseManager.StepComplete();
					}
					break;
				}
				CActor actorSpawningMessage = TargetingActor;
				if (TargetingActor is CHeroSummonActor cHeroSummonActor)
				{
					actorSpawningMessage = cHeroSummonActor.Summoner;
				}
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, actorSpawningMessage)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { TrapData.PlacementTileFilter },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message);
				break;
			}
			if (UseSubAbilityTargeting)
			{
				if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					m_TilesSelected.AddRange(InlineSubAbilityTiles);
				}
				else
				{
					if (ParentAbility == null || ParentAbility.TilesSelected == null || ParentAbility.TilesSelected.Count <= 0)
					{
						PhaseManager.NextStep();
						return true;
					}
					foreach (CTile item in ParentAbility.TilesSelected)
					{
						if (!m_TilesSelected.Contains(item))
						{
							m_TilesSelected.Add(item);
						}
					}
				}
				m_State = TrapState.SpawningTrap;
				Perform();
				break;
			}
			if (AreaEffect == null)
			{
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				List<CTile> list = new List<CTile>();
				foreach (CTile item2 in TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item2, TrapData.PlacementTileFilter, includeInitial: false, null, null, VarsContext) && item2.FindProp(ScenarioManager.ObjectImportType.Trap) == null)
					{
						list.Add(item2);
					}
				}
				TilesInRange = list.ToList();
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { TrapData.PlacementTileFilter },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message2);
				break;
			}
			CAIMoveNew.GetPossibleEmptyTiles(TargetingActor, TilesInRange, out var failTile, out var failReason, out var possibleTiles);
			if (failTile.Count > 0)
			{
				Log.SimpleInfo(TargetingActor?.Class?.ID + " Failed to place trap adjacent to " + TargetingActor?.Class?.ID);
				for (int num = 0; num < failTile.Count; num++)
				{
					Log.SimpleInfo("Tile at x:" + failTile[num].m_ArrayIndex.X + " y:" + failTile[num].m_ArrayIndex.Y + ", fail reason=" + failReason[num]);
				}
			}
			CAIMoveNew.SelectTilesBasedOnTargetingFocus(TargetingActor, Range, possibleTiles, Targeting, TilesSelected, NumberTargetsRemaining, AllTargets);
			if (m_TilesSelected.Count > 0)
			{
				m_State = TrapState.SpawningTrap;
				Perform();
				break;
			}
			PhaseManager.NextStep();
			return true;
		}
		case TrapState.SpawningTrap:
			if (m_TilesSelected.Count != 0)
			{
				CPlacingTrap_MessageData message3 = new CPlacingTrap_MessageData(AnimOverload, TargetingActor)
				{
					m_ActorPlacingTrap = TargetingActor,
					m_Tiles = m_TilesSelected,
					m_TrapAbility = this
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				m_State = TrapState.SpawnTrap;
				Perform();
			}
			break;
		case TrapState.SpawnTrap:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			foreach (CTile item3 in m_TilesSelected)
			{
				AbilityHasHappened = true;
				m_TrapPlaced = true;
				CObjectTrap prop = new CObjectTrap(TrapData.TrapName, ScenarioManager.ObjectImportType.Trap, new TileIndex(item3.m_ArrayIndex), null, null, TargetingActor, item3.m_HexMap.MapGuid, TrapData);
				item3.SpawnProp(prop, notifyClient: true, SpawnDelay, this);
			}
			m_State = TrapState.TrapPlaced;
			Perform();
			break;
		case TrapState.TrapPlaced:
			PhaseManager.NextStep();
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
		bool flag = false;
		if (!TilesSelected.Contains(selectedTile) && !AllTargets && m_State == TrapState.SelectTrapPosition)
		{
			if (TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < NumberTargets)
				{
					if (TargetingActor.PlayerActor != null)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					}
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
						m_NumberTargetsRemaining--;
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(TargetingActor));
				m_State = TrapState.SelectTrapPosition;
				flag = true;
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_TilesSelected.Contains(selectedTile) && !AllTargets)
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (TargetingActor.PlayerActor != null)
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

	public override bool CanClearTargets()
	{
		return m_State == TrapState.SelectTrapPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == TrapState.SelectTrapPosition;
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
			if (m_State == TrapState.TrapDone)
			{
				Perform();
			}
		}
		return m_State == TrapState.TrapDone;
	}

	public override string GetDescription()
	{
		return "Trap";
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		TrapState trapState = m_State;
		if (trapState == TrapState.TrapDone && !m_TrapPlaced)
		{
			trapState = TrapState.NoTilesSelected;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTrap(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, defaultAction: false, TrapData.Damage), trapState, TrapData.TrapName, TrapData.TriggeredXP, TrapData.Heal, TrapData.Conditions?.ToList(), TrapData.PositiveConditions?.ToList()));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}
}

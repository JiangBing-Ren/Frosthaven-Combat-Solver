using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveShadow : CAbility
{
	public enum RemoveShadowState
	{
		SelectShadowsPosition,
		PreRemoveShadows,
		AnimateCharacter,
		RemoveShadows,
		RemoveShadowsDone
	}

	public float m_DestroyDelay;

	private RemoveShadowState m_State;

	private List<CObjectProp> m_RemovedShadows;

	public bool m_CanClearTargets = true;

	public RemoveShadowState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public List<CObjectProp> RemovedShadows => m_RemovedShadows;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = RemoveShadowState.SelectShadowsPosition;
		m_RemovedShadows = new List<CObjectProp>();
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (UseSubAbilityTargeting)
		{
			if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
			{
				m_ValidTilesInAreaEffect.AddRange(ParentAbility.InlineSubAbilityTiles);
			}
			else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
			{
				if (ParentAbility.AreaEffect != null)
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
				}
				else
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.TilesSelected);
				}
			}
		}
		else
		{
			if (Range == 0)
			{
				m_ValidTilesInAreaEffect = new List<CTile>();
				m_ValidTilesInAreaEffect.Add(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
			}
			else
			{
				m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			if (Targeting != EAbilityTargeting.Custom)
			{
				m_ValidTilesInAreaEffect.Add(TargetingActor.Tile);
			}
			List<CTile> list = new List<CTile>();
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (item.m_HexMap.Revealed && item.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow))
				{
					list.Add(item);
				}
			}
			m_ValidTilesInAreaEffect = list.ToList();
			TilesInRange = m_ValidTilesInAreaEffect.ToList();
		}
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
			foreach (CTile item2 in m_ValidTilesInAreaEffect)
			{
				if (!m_TilesSelected.Contains(item2))
				{
					m_TilesSelected.Add(item2);
				}
			}
		}
		if (VarsContext.GetVar("CanClearTargets").isValid)
		{
			m_CanClearTargets = VarsContext.GetVar("CanClearTargets").GetBool();
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
		case RemoveShadowState.SelectShadowsPosition:
		{
			if (AreaEffect == null)
			{
				List<CTile> list2 = new List<CTile>();
				if (Targeting == EAbilityTargeting.Custom)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				else
				{
					bool includeStartingTile = IncludeCasterTile();
					TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true, skipPathingCheck: false, includeStartingTile);
					TilesInRange.Add(TargetingActor.Tile);
				}
				foreach (CTile item in TilesInRange)
				{
					if (item.m_HexMap.Revealed && item.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow))
					{
						list2.Add(item);
					}
				}
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list2, this);
				CAbilityPerformFromShadow.RemoveTilesReserevdByPerformFromShadow(list2);
				TilesInRange = list2.ToList();
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.AlsoTargetSelf == true && !m_TilesSelected.Contains(TargetingActor.Tile))
				{
					m_TilesSelected.Add(TargetingActor.Tile);
				}
				AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
				if (miscAbilityData3 != null && miscAbilityData3.AutotriggerAbility == true)
				{
					m_State = RemoveShadowState.PreRemoveShadows;
					Perform();
					break;
				}
			}
			CPlayerSelectingObjectPosition_MessageData message4 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Shadow },
				m_ShowActiveBonusBar = true,
				m_IsPositive = IsPositive(),
				m_SpawnType = ScenarioManager.ObjectImportType.None
			};
			ScenarioRuleClient.MessageHandler(message4);
			break;
		}
		case RemoveShadowState.PreRemoveShadows:
			m_CanUndo = false;
			m_CanSkip = false;
			m_State = RemoveShadowState.AnimateCharacter;
			Perform();
			return true;
		case RemoveShadowState.AnimateCharacter:
			AbilityHasHappened = true;
			if (m_TilesSelected.Count != 0)
			{
				bool faceObstacle = true;
				bool flag = GetVar("SendBuffProjectiles").GetBool();
				foreach (CTile item2 in m_TilesSelected)
				{
					CObjectProp cObjectProp = item2.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
					if (cObjectProp == null)
					{
						PhaseManager.NextStep();
						break;
					}
					m_RemovedShadows.Add(cObjectProp);
				}
				if (flag)
				{
					List<CObjectProp> list = m_RemovedShadows.ToList();
					list.RemoveAll((CObjectProp t) => t.PropTile == TargetingActor.Tile);
					CSpawnProjectiles_MessageData message2 = new CSpawnProjectiles_MessageData
					{
						projectileSettingsName = "BuffProjectileSettings",
						m_TargetActor = TargetingActor,
						m_FromProps = list
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				CDestroyingObstacle_MessageData message3 = new CDestroyingObstacle_MessageData(AnimOverload, TargetingActor, faceObstacle)
				{
					m_ActorDestroyingObstacle = TargetingActor,
					m_Tiles = m_TilesSelected.ToList(),
					m_DestroyedProps = m_RemovedShadows.ToList(),
					m_DestroyObstacleAbility = this
				};
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case RemoveShadowState.RemoveShadows:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				AbilityHasHappened = true;
				foreach (CObjectProp removedShadow in m_RemovedShadows)
				{
					removedShadow.DestroyProp(0f, sendMessageToClient: false, forceDestroy: true, this);
				}
				CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(AnimOverload, TargetingActor)
				{
					m_ActorDestroyingObstacle = TargetingActor,
					m_Tiles = m_TilesSelected.ToList(),
					m_DestroyedProps = m_RemovedShadows.ToList(),
					m_DestroyDelay = m_DestroyDelay,
					m_DestroyObstacleAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
				ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				CActiveBonus.RefreshAllAuraActiveBonuses(this);
				PhaseManager.StepComplete();
			}
			else if (!IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			return true;
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
		if (!AllTargets && !TilesSelected.Contains(selectedTile) && m_State == RemoveShadowState.SelectShadowsPosition && m_NumberTargetsRemaining > 0 && TilesInRange.Contains(selectedTile))
		{
			bool flag2 = false;
			if (!m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag = true;
				flag2 = true;
			}
			if (flag2)
			{
				m_NumberTargetsRemaining--;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
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
		bool flag = false;
		if (!AllTargets)
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Remove(selectedTile);
				m_NumberTargetsRemaining++;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
				flag = true;
			}
			if (flag)
			{
				Perform();
			}
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == RemoveShadowState.SelectShadowsPosition)
		{
			Restart();
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_CanClearTargets)
		{
			return m_State == RemoveShadowState.SelectShadowsPosition;
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == RemoveShadowState.SelectShadowsPosition;
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
		return m_State == RemoveShadowState.RemoveShadowsDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = RemoveShadowState.SelectShadowsPosition;
		m_TilesSelected.Clear();
		m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
		TilesInRange = m_ValidTilesInAreaEffect;
		if (NumberTargets == OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (!m_TilesSelected.Contains(item))
				{
					m_TilesSelected.Add(item);
				}
			}
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		GameState.UpdateObstaclesDestroyedThisTurn(m_RemovedShadows.Count());
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityRemoveShadow(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, (m_TilesSelected != null) ? m_TilesSelected.Count : 0));
	}

	public override string GetDescription()
	{
		return "RemoveShadow";
	}

	public bool HasPassedState(RemoveShadowState state)
	{
		return m_State > state;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		if (GetVar("TargetsUpTo") != TValue.Unknown && GetVar("TargetsUpTo").GetBool())
		{
			return m_TilesSelected.Count > 0;
		}
		return m_TilesSelected.Count == NumberTargets;
	}
}

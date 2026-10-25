using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityNullHex : CAbility
{
	public enum ENullHexState
	{
		SelectHexPositions,
		AnimateCharacter,
		Perform,
		NullHexDone
	}

	public delegate List<CTile> CalcValidTilesInRangeFunc(CAbilityNullHex ability);

	public delegate void CallbackFunc(CAbilityNullHex ability, List<CTile> selectedTiles);

	public class ExternalParams
	{
		public CActor Actor;

		public CalcValidTilesInRangeFunc CalcValidTilesInRange;

		public List<CTile> TilesInRange;

		public CallbackFunc Callback;

		public int numTilesToPick;

		public bool AnimateActor;

		public bool RecalculateValidTilesOnSelect;
	}

	private ENullHexState m_State;

	public ExternalParams m_ExternalParams;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (AbilityHasBeenCancelled)
		{
			return;
		}
		m_State = ENullHexState.SelectHexPositions;
		m_ExternalParams = VarsContext.GetVar("ExternalParams").GetObject<ExternalParams>();
		if (m_ExternalParams != null)
		{
			NumberTargets = m_ExternalParams.numTilesToPick;
			if (m_ExternalParams.CalcValidTilesInRange != null)
			{
				m_ExternalParams.TilesInRange = m_ExternalParams.CalcValidTilesInRange(this);
				if (m_ExternalParams.TilesInRange == null)
				{
					m_ExternalParams.TilesInRange = new List<CTile>();
				}
			}
		}
		CalcValidTilesInRange();
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		else
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (TilesInRange.Count == 0)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		else if (TilesInRange.Count > 0 && TilesInRange.Count <= NumberTargets && VarsContext.GetVar("PreSelectValidTiles").GetBool())
		{
			TilesSelected.AddRange(TilesInRange);
		}
		m_NumberTargetsRemaining = NumberTargets - m_TilesSelected.Count;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	private void CalcValidTilesInRange()
	{
		if (m_ExternalParams?.TilesInRange != null)
		{
			TilesInRange = m_ExternalParams.TilesInRange;
		}
		else
		{
			if (Targeting == EAbilityTargeting.Custom)
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			else
			{
				bool flag = IncludeCasterTile();
				bool flag2 = GetMiscVar("AllowClosedDoorTiles").GetBool();
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, includeStartingTile: flag, allowClosedDoorTiles: flag2, ignoreLOS: MiscAbilityData?.IgnoreLOS ?? false, emptyOpenDoorTiles: true);
			}
			List<CTile> list = new List<CTile>();
			foreach (CTile item in TilesInRange)
			{
				if (Targeting != EAbilityTargeting.Range || Range <= 0 || TargetingActor.GetDistanceTo(item) <= Range)
				{
					if (CAbilityFilter.IsValidTile(item, TileFilter, includeInitial: false, null, MiscVars, VarsContext) && !CheckIfWillBeBlocking(item))
					{
						list.Add(item);
					}
					else
					{
						m_TilesSelected.Remove(item);
					}
				}
			}
			TilesInRange = list.ToList();
		}
		ExpandTileListWithLinkedProps(TilesInRange);
		SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(TilesInRange, this);
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
		case ENullHexState.SelectHexPositions:
		{
			if (AllTargets)
			{
				TilesSelected.Clear();
				TilesSelected.AddRange(TilesInRange);
			}
			if (AutoTrigger)
			{
				PhaseManager.StepComplete();
				break;
			}
			CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { TileFilter },
				m_IsPositive = IsPositive(),
				m_ShowActiveBonusBar = true
			};
			if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
			{
				cPlayerSelectingObjectPosition_MessageData.m_ShowConsumeBar = cAbilityCard.GetAction(this)?.Augmentations?.Any() == true;
			}
			ScenarioRuleClient.MessageHandler(cPlayerSelectingObjectPosition_MessageData);
			return true;
		}
		case ENullHexState.AnimateCharacter:
			AbilityHasHappened = true;
			ScenarioRuleClient.FirstAbilityStarted();
			if (m_TilesSelected.Count != 0)
			{
				CActorSelectedNullHexes_MessageData message = new CActorSelectedNullHexes_MessageData(AnimOverload, TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
				if (StackInlineSubAbilities(null, m_TilesSelected[0]))
				{
					m_State = ENullHexState.NullHexDone;
				}
			}
			else if (!IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			break;
		case ENullHexState.Perform:
			PhaseManager.StepComplete();
			return true;
		case ENullHexState.NullHexDone:
			PhaseManager.NextStep();
			return true;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		InvalidTileReason = null;
		bool flag = false;
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if ((!TilesSelected.Contains(selectedTile) || (AreaEffect != null && !AreaEffectLocked)) && m_State == ENullHexState.SelectHexPositions && !AllTargets && !UseSubAbilityTargeting)
			{
				if (AreaEffect != null)
				{
					if (!m_AreaEffectLocked && m_ValidTilesInAreaEffect.Count == ValidTilesInAreaEffectedIncludingBlocked.Count && m_ValidTilesInAreaEffect.Count == AreaEffect.EnabledHexes.Count())
					{
						foreach (CTile item in m_ValidTilesInAreaEffect)
						{
							if (!CAbilityFilter.IsValidTile(item, TileFilter, includeInitial: false, null, MiscVars, VarsContext) && !CheckIfWillBeBlocking(item, warn: true))
							{
								if (flag)
								{
									Perform();
								}
								base.TileSelected(selectedTile, optionalTileList);
								LogEvent(ESESubTypeAbility.AbilityTileSelected);
								return;
							}
						}
						m_TilesSelected.Clear();
						m_TilesSelected.AddRange(m_ValidTilesInAreaEffect);
						m_AreaEffectLocked = true;
						if (TargetingActor.Type == CActor.EType.Player)
						{
							CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
							cPlayerSelectedTile_MessageData.m_Ability = this;
							ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
						}
					}
				}
				else if (TilesInRange.Contains(selectedTile) && m_TilesSelected.Count < NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						m_TilesSelected.Add(selectedTile);
						m_TilesSelected.AddRange(GetLinkedTiles(selectedTile));
						flag = true;
						RecalculateValidTilesInRange();
					}
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData2.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
					}
				}
			}
			m_NumberTargetsRemaining = NumberTargets - m_TilesSelected.Count;
		}
		if (flag)
		{
			SetMiscVar("SkipPlayerFocus", true);
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		InvalidTileReason = null;
		bool flag = false;
		bool flag2 = false;
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (AreaEffect != null && m_AreaEffectLocked)
			{
				m_AreaEffectLocked = false;
				m_TilesSelected.Clear();
				flag2 = true;
			}
			else if (m_TilesSelected.Contains(selectedTile) && !AllTargets)
			{
				m_TilesSelected.Remove(selectedTile);
				foreach (CTile linkedTile in GetLinkedTiles(selectedTile))
				{
					m_TilesSelected.Remove(linkedTile);
				}
				bool flag3 = true;
				foreach (CTile item in m_TilesSelected)
				{
					if (item != selectedTile && CheckIfWillBeBlocking(item, warn: true))
					{
						flag3 = false;
						break;
					}
				}
				if (!flag3)
				{
					m_TilesSelected.Add(selectedTile);
					foreach (CTile linkedTile2 in GetLinkedTiles(selectedTile))
					{
						m_TilesSelected.Add(linkedTile2);
					}
				}
				else
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
			PerformStateVars("OnTileDeselected");
			RecalculateValidTilesInRange();
			flag = true;
		}
		m_NumberTargetsRemaining = NumberTargets - m_TilesSelected.Count;
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	private void RecalculateValidTilesInRange()
	{
		if (m_ExternalParams != null && m_ExternalParams.RecalculateValidTilesOnSelect)
		{
			if (m_ExternalParams.CalcValidTilesInRange != null)
			{
				m_ExternalParams.TilesInRange = m_ExternalParams.CalcValidTilesInRange(this);
				if (m_ExternalParams.TilesInRange == null)
				{
					m_ExternalParams.TilesInRange = new List<CTile>();
				}
			}
			if (m_ExternalParams?.TilesInRange != null)
			{
				TilesInRange = m_ExternalParams.TilesInRange;
			}
		}
		CalcValidTilesInRange();
	}

	public override void ClearTargets()
	{
		int count = TilesSelected.Count;
		RecalculateValidTilesInRange();
		base.ClearTargets();
		if (m_State == ENullHexState.SelectHexPositions)
		{
			if (count != TilesSelected.Count)
			{
				PerformStateVars("OnTileDeselected");
			}
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (GetMiscVar("CantClearTargets").GetBool())
		{
			return false;
		}
		return m_State == ENullHexState.SelectHexPositions;
	}

	public override bool CanReceiveTileSelection()
	{
		if (GetMiscVar("CantReceiveTileSelection").GetBool())
		{
			return false;
		}
		if (base.CanReceiveTileSelection())
		{
			return m_State == ENullHexState.SelectHexPositions;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		bool flag = m_ExternalParams == null || m_ExternalParams.AnimateActor;
		if (!dontMoveState)
		{
			if (flag)
			{
				m_State++;
				if (m_State == ENullHexState.NullHexDone)
				{
					m_ExternalParams?.Callback?.Invoke(this, m_TilesSelected);
				}
			}
			else
			{
				m_State = ENullHexState.NullHexDone;
				m_ExternalParams?.Callback?.Invoke(this, m_TilesSelected);
			}
		}
		return m_State == ENullHexState.NullHexDone;
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

	public override bool IsPositive()
	{
		return !GetMiscVar("IsNegative").GetBool();
	}

	public override bool EnoughTargetsSelected()
	{
		TValue miscVar = GetMiscVar("MinTilesSelected");
		if (miscVar.isValid && TilesSelected.Count < miscVar.GetInt())
		{
			return false;
		}
		return TilesSelected.Count > 0;
	}

	public override bool CanSkipUntergetableCheck()
	{
		return true;
	}

	protected override void OnFindBlockingTiles()
	{
		base.OnFindBlockingTiles();
		if (TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { TileFilter },
				m_IsPositive = IsPositive(),
				m_ShowActiveBonusBar = true
			};
			if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
			{
				cPlayerSelectingObjectPosition_MessageData.m_ShowConsumeBar = cAbilityCard.GetAction(this)?.Augmentations?.Any() == true;
			}
			ScenarioRuleClient.MessageHandler(cPlayerSelectingObjectPosition_MessageData);
		}
	}
}

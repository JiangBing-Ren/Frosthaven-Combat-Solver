using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityShiftForm : CAbility
{
	public enum EShiftFormState
	{
		ShiftBegin,
		Shift,
		CheckForPlayerIdle,
		ShiftDone
	}

	private EShiftFormState m_State;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EShiftFormState.ShiftBegin;
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
				goto IL_0083;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0083;
				}
			}
		}
		switch (m_State)
		{
		case EShiftFormState.ShiftBegin:
		{
			CShiftingForm_MessageData message3 = new CShiftingForm_MessageData
			{
				m_Tile = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y],
				m_Actor = (CEnemyActor)TargetingActor
			};
			ScenarioRuleClient.MessageHandler(message3);
			return false;
		}
		case EShiftFormState.Shift:
		{
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (cPhaseAction != null)
			{
				if (TargetingActor.Type != CActor.EType.Player && !m_AbilityHasHappened)
				{
					AbilityHasHappened = true;
					TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				CEnemyActor cEnemyActor = (CEnemyActor)TargetingActor;
				cEnemyActor.ShiftEliteForm();
				if (PhaseManager.Phase is CPhaseAction cPhaseAction2)
				{
					for (int i = 0; i < cPhaseAction2.RemainingPhaseAbilities.Count; i++)
					{
						CAbility ability = (cPhaseAction2.RemainingPhaseAbilities[i].m_BaseCard as CMonsterAbilityCard).FindAbilityOnCard(cPhaseAction2.RemainingPhaseAbilities[i].m_Ability.Name);
						cPhaseAction2.RemainingPhaseAbilities[i].m_Ability = cEnemyActor.CloneAbilityAndApplyBaseStats(ability);
						cPhaseAction2.RemainingPhaseAbilities[i].m_Ability.TargetingActor = cEnemyActor;
					}
				}
				CShiftForm_MessageData message = new CShiftForm_MessageData
				{
					m_Tile = ScenarioManager.Tiles[cEnemyActor.ArrayIndex.X, cEnemyActor.ArrayIndex.Y],
					m_Actor = cEnemyActor
				};
				ScenarioRuleClient.MessageHandler(message);
				CRefreshActorHealth_MessageData message2 = new CRefreshActorHealth_MessageData(cEnemyActor, cEnemyActor, cEnemyActor.Health);
				ScenarioRuleClient.MessageHandler(message2);
				if (cPhaseAction.ActionAugmentationsAvailableForCurrentAbility().Count <= 0)
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
					return true;
				}
				if (cPhaseAction.HasAnyUntoggleableActionAugments() || cPhaseAction.AnyActionAugmentationsConsumed() || cPhaseAction.PreviousPhaseAbilities.Count > 0)
				{
					m_CanUndo = false;
				}
			}
			return true;
		}
		case EShiftFormState.CheckForPlayerIdle:
		{
			CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(TargetingActor);
			cPlayerWaitForIdle_MessageData.m_Actor = TargetingActor;
			ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
			return true;
		}
		default:
			return false;
		}
		IL_0083:
		PhaseManager.NextStep();
		return true;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EShiftFormState.ShiftDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityShiftForm(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State));
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		return false;
	}
}

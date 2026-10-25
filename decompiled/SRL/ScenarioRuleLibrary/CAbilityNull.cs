using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityNull : CAbility
{
	public enum ENullState
	{
		NullBegin,
		NullAnimateActor,
		NullPerform,
		NullDone
	}

	private ENullState m_State;

	public CMessageData customWaitMessage;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ENullState.NullBegin;
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
		if (IsControlAbility)
		{
			AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.AutotriggerAbility == true)
			{
				PhaseManager.NextStep();
				return true;
			}
		}
		switch (m_State)
		{
		case ENullState.NullBegin:
		{
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (cPhaseAction != null && TargetingActor.Type == CActor.EType.Player && customWaitMessage == null)
			{
				if (TargetingActor is CPlayerActor cPlayerActor && cPlayerActor.Class.ID == "GeminateID" && cPlayerActor.PendingForm == CPlayerActor.EForm.None)
				{
					if (Flags.HasFlag(EAbilityFlag.SwitchFormMeleeToRange))
					{
						cPlayerActor.PendingForm = CPlayerActor.EForm.Range;
					}
					else if (Flags.HasFlag(EAbilityFlag.SwitchFormRangeToMelee))
					{
						cPlayerActor.PendingForm = CPlayerActor.EForm.Melee;
					}
				}
				if (TargetingActor is CPlayerActor cPlayerActor2 && cPlayerActor2.Class.ID == "GeminateID" && Flags.HasFlag(EAbilityFlag.IgnoreSwitchForm))
				{
					cPlayerActor2.IgnoreSwitchForm = true;
					cPlayerActor2.PendingForm = CPlayerActor.EForm.None;
				}
				if (cPhaseAction.ActionAugmentationsAvailableForCurrentAbility().Count <= 0)
				{
					m_State++;
					Perform();
					return true;
				}
				if (MiscAbilityData != null && MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
				{
					m_State++;
					Perform();
					return true;
				}
				if (cPhaseAction.HasAnyUntoggleableActionAugments() || cPhaseAction.AnyActionAugmentationsConsumed() || cPhaseAction.PreviousPhaseAbilities.Count > 0)
				{
					m_CanUndo = false;
				}
				CActorWantsAnActionConfirmation_MessageData cActorWantsAnActionConfirmation_MessageData = new CActorWantsAnActionConfirmation_MessageData(TargetingActor);
				cActorWantsAnActionConfirmation_MessageData.m_Ability = this;
				cActorWantsAnActionConfirmation_MessageData.m_Confirm = true;
				cActorWantsAnActionConfirmation_MessageData.m_CanUndo = MiscAbilityData.CanUndo ?? true;
				ScenarioRuleClient.MessageHandler(cActorWantsAnActionConfirmation_MessageData);
			}
			else if (customWaitMessage != null)
			{
				ScenarioRuleClient.MessageHandler(customWaitMessage);
			}
			else
			{
				CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(TargetingActor);
				cPlayerWaitForIdle_MessageData.m_Actor = TargetingActor;
				ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
			}
			return true;
		}
		case ENullState.NullAnimateActor:
		{
			AbilityHasHappened = true;
			if (TargetingActor.Type == CActor.EType.Player && !m_AbilityHasHappened)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			CPlayCharacterAnimation_MessageData message = new CPlayCharacterAnimation_MessageData(AnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_TargetedTilesForVFX = (ScenarioScript.current?.NullAbilityTargetedTiles(this) ?? GetVar("TargetedTilesForVFX").GetObject<List<object>>()?.OfType<CTile>()?.ToList()),
				m_AreaTilesForVFX = (ScenarioScript.current?.NullAbilityAreaTiles(this) ?? GetVar("AreaTilesForVFX").GetObject<List<object>>()?.OfType<CTile>()?.ToList()),
				m_IsFromItem = IsItemAbility
			};
			ScenarioRuleClient.MessageHandler(message);
			break;
		}
		case ENullState.NullPerform:
			PhaseManager.NextStep();
			return true;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player && CanUndo && CPhaseAction.IsFirstAbility(this))
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

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ENullState.NullDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityNull(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State));
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

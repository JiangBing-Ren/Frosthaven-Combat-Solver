using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityTransferDooms : CAbilityTargeting
{
	public CAbilityTransferDooms()
		: base(EAbilityType.TransferDooms)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (Strength == int.MaxValue)
			{
				TargetingActor.TransferAllDooms(actor);
			}
			else
			{
				CTransferDoomChoice_MessageData message = new CTransferDoomChoice_MessageData(TargetingActor)
				{
					m_NewDoomTargetActor = actor,
					m_TransferDoomAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
				GameState.WaitingForMercenarySpecialMechanicSlotChoice = true;
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		GameState.WaitingForMercenarySpecialMechanicSlotChoice = false;
		TargetingActor.ClearCharacterSpecialMechanicsCache(clearAugments: false, clearSongs: false, clearDooms: true);
	}
}

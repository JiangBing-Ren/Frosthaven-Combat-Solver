using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityAddDoom : CAbilityTargeting
{
	[JsonProperty]
	public CDoom Doom { get; set; }

	public CAbilityAddDoom(CDoom doom)
		: base(EAbilityType.AddDoom)
	{
		Doom = doom;
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
			if (Doom != null)
			{
				TargetingActor.AddDoom(Doom, actor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find Song for Song Ability " + Name);
			}
			if (!GameState.WaitingForMercenarySpecialMechanicSlotChoice)
			{
				CWaitForProgressChoreographer_MessageData message = new CWaitForProgressChoreographer_MessageData(TargetingActor)
				{
					WaitActor = TargetingActor,
					WaitTickFrame = 10000,
					ClearEvents = SkipAnim
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			CActorBeenDoomed_MessageData message2 = new CActorBeenDoomed_MessageData(TargetingActor)
			{
				m_ActorBeingDoomed = actor,
				m_DoomAbility = this
			};
			ScenarioRuleClient.MessageHandler(message2);
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
}

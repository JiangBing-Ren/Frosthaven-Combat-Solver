using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CAbilityAddSong : CAbilityTargeting
{
	public CAbilityAddSong()
		: base(EAbilityType.AddSong)
	{
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingAddSong_MessageData message = new CActorIsApplyingAddSong_MessageData(AnimOverload, actorApplying)
		{
			m_AddSongAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CApplyToActorAddSong_MessageData message = new CApplyToActorAddSong_MessageData(TargetingActor)
			{
				m_AddSongAbility = this,
				m_Target = actor
			};
			ScenarioRuleClient.MessageHandler(message);
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
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
		return true;
	}
}

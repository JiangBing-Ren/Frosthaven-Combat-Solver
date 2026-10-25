using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityAddActiveBonus : CAbilityTargeting
{
	[JsonIgnore]
	[StateDump(Skip = true)]
	public List<CAbility> CanBeToggledDuringAbilities = new List<CAbility>();

	[JsonProperty]
	public CAbility AddAbility { get; set; }

	public CAbilityAddActiveBonus(CAbility addAbility)
		: base(EAbilityType.AddActiveBonus)
	{
		AddAbility = addAbility;
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
		if (PerkID != null && AddAbility != null && AddAbility.PerkID == null)
		{
			AddAbility.PerkID = PerkID;
		}
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			CAddActiveBonus_MessageData message = new CAddActiveBonus_MessageData(AnimOverload, TargetingActor)
			{
				m_AddActiveBonus = this,
				m_ActorAppliedTo = actor
			};
			ScenarioRuleClient.MessageHandler(message);
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Error: Unable to find base ability card for ability " + Name);
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

	public bool CanBeToggledDuringAbility(CAbility ability)
	{
		if (CanBeToggledDuringAbilities.Any((CAbility x) => x.Name == ability.Name))
		{
			return true;
		}
		return ability.ID == AddAbility.ID;
	}
}

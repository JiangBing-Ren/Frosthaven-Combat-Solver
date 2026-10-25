using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityDisableCardAction : CAbilityTargeting
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DisableCardActionData
	{
		[JsonProperty]
		public CBaseCard.ActionType DisableActionType;

		[JsonProperty]
		public string CardName;

		public DisableCardActionData()
		{
			DisableActionType = CBaseCard.ActionType.NA;
		}

		public DisableCardActionData Copy()
		{
			return new DisableCardActionData
			{
				DisableActionType = DisableActionType,
				CardName = CardName
			};
		}

		public DisableCardActionData Merge(DisableCardActionData dataToMerge)
		{
			return new DisableCardActionData
			{
				DisableActionType = dataToMerge.DisableActionType,
				CardName = dataToMerge.CardName
			};
		}

		public static DisableCardActionData DefaultDisableCardActionData()
		{
			return new DisableCardActionData();
		}
	}

	[JsonProperty]
	public DisableCardActionData DisableCardActionAbilityData { get; set; }

	public CAbilityDisableCardAction(DisableCardActionData disableData)
		: base(EAbilityType.DisableCardAction)
	{
		DisableCardActionAbilityData = disableData;
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
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
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

	public override bool CanApplyActiveBonusTogglesTo()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return false;
	}
}

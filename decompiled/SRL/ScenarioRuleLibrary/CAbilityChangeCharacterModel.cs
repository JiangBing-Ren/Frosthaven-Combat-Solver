using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityChangeCharacterModel : CAbilityTargeting
{
	[JsonProperty]
	public ECharacter CharacterModel { get; set; }

	[JsonProperty]
	public CActor.EType? OverrideType { get; set; }

	public CAbilityChangeCharacterModel(ECharacter characterModel)
		: base(EAbilityType.ChangeCharacterModel)
	{
		CharacterModel = characterModel;
		OverrideType = null;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		return true;
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
			CChangeCharacterModel_MessageData cChangeCharacterModel_MessageData = new CChangeCharacterModel_MessageData(actor);
			cChangeCharacterModel_MessageData.m_ChangeCharacterAbility = this;
			ScenarioRuleClient.MessageHandler(cChangeCharacterModel_MessageData);
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
		return true;
	}
}

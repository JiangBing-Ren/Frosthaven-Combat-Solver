using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityShowNextAttackModifier : CAbilityTargeting
{
	public CAbilityShowNextAttackModifier()
		: base(EAbilityType.ShowNextAttackModifier)
	{
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		ActiveBonusData.HiddenInLog = true;
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
			if (GetMiscVar("DontAddActiveBonus").GetBool())
			{
				return true;
			}
			CActiveBonus cActiveBonus = null;
			if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				cActiveBonus = GetAbilityBaseCard().AddActiveBonus(this, actor, TargetingActor);
			}
			if (cActiveBonus != null && cActiveBonus.Duration != CActiveBonus.EActiveBonusDurationType.NA)
			{
				CLogActiveBonus_MessageData message = new CLogActiveBonus_MessageData(AnimOverload, TargetingActor)
				{
					m_ActiveBonus = cActiveBonus,
					m_ActorAppliedTo = actor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (cActiveBonus == null || !cActiveBonus.IsAura)
			{
				ProcessPositiveStatusEffects(actor);
				ProcessNegativeStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return !GetMiscVar("IsNegative").GetBool();
	}

	public override void AbilityEnded()
	{
		base.AbilityEnded();
		if (!AbilityHasHappened)
		{
			return;
		}
		Dictionary<string, List<AttackModifierYMLData>> dictionary = new Dictionary<string, List<AttackModifierYMLData>>();
		foreach (CActor item in ActorsTargeted)
		{
			List<AttackModifierYMLData> attackModifierDeck = CustomAbility.GetAttackModifierDeck(item, out var deckID);
			dictionary[deckID] = attackModifierDeck;
		}
		foreach (KeyValuePair<string, List<AttackModifierYMLData>> item2 in dictionary)
		{
			AttackModifierCard attackModifierCard = item2.Value?.FirstOrDefault()?.Card;
			string text = item2.Key;
			switch (text)
			{
			case "Enemy":
				text = "GUI_ENEMY";
				break;
			case "Enemy2":
				text = "GUI_ENEMY2";
				break;
			case "Ally":
				text = "GUI_ALLY";
				break;
			case "Neutral":
				text = "GUI_NEUTRAL";
				break;
			}
			CCombatLogMessage_MessageData cCombatLogMessage_MessageData = new CCombatLogMessage_MessageData();
			cCombatLogMessage_MessageData.m_MessageKey = "$COMBAT_LOG_NEXT_MODIFIER$";
			cCombatLogMessage_MessageData.m_Params = new List<string>
			{
				text,
				attackModifierCard?.AttackModifierLogString() ?? "GUI_NextAttackModifier_Fallback"
			};
			ScenarioRuleClient.MessageHandler(cCombatLogMessage_MessageData);
		}
	}
}

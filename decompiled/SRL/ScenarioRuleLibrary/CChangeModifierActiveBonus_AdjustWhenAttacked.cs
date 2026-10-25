using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChangeModifierActiveBonus_AdjustWhenAttacked : CBespokeBehaviour
{
	public CBaseCard BaseCard;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.AdjustWhenAttacked;

	public CChangeModifierActiveBonus_AdjustWhenAttacked(CActor actor, CBaseCard baseCard, CAbilityChangeModifier ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnDrawModifier(EAdvantageStatuses advantageStatus, ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target)
	{
		if (target != m_Actor || !m_ActiveBonus.SetupTriggerContext(this, null, m_Ability, null, target, ("AdvantageStatus", new TValue(advantageStatus))))
		{
			return;
		}
		CAbilityChangeModifier cAbilityChangeModifier = m_Ability as CAbilityChangeModifier;
		if ((cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers != null || cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.HasValue) && cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifier != null)
		{
			for (int i = 0; i < modifiers.Count; i++)
			{
				AttackModifierYMLData attackModifierYMLData = modifiers[i];
				if (cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifier == null || cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers == null || (!cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers.Contains(attackModifierYMLData.MathModifier) && (!cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.HasValue || !cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.Value || !attackModifierYMLData.MathModifier.StartsWith('-'))))
				{
					continue;
				}
				string replaceWithModifierName = cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifier;
				AttackModifierYMLData attackModifierYMLData2 = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == replaceWithModifierName);
				char operatorSymbol;
				int mathVal;
				if (attackModifierYMLData2 != null)
				{
					AttackModifierYMLData attackModifierYMLData3 = attackModifierYMLData2.Copy(createCard: true);
					modifiers[i] = attackModifierYMLData3;
					attackModifierYMLData3.OriginalModifier = attackModifierYMLData.MathModifier;
					attackModifierYMLData3.MathModifier = attackModifierYMLData2.MathModifier;
					attackModifierYMLData3.ModifiedFromModifier = attackModifierYMLData.Card;
					m_ActiveBonus.RestrictActiveBonus(actor);
					CChangeModifier_MessageData message = new CChangeModifier_MessageData(GameState.InternalCurrentActor)
					{
						m_ActiveBonus = ActiveBonus,
						m_OriginalModifier = attackModifierYMLData3.OriginalModifier
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else if (AttackModifierYMLData.TryParseMathModifier(replaceWithModifierName, out operatorSymbol, out mathVal))
				{
					AttackModifierYMLData attackModifierYMLData4 = attackModifierYMLData.Copy(createCard: true);
					modifiers[i] = attackModifierYMLData4;
					attackModifierYMLData4.OriginalModifier = attackModifierYMLData.MathModifier;
					attackModifierYMLData4.MathModifier = replaceWithModifierName;
					attackModifierYMLData4.ModifiedFromModifier = attackModifierYMLData.Card;
					m_ActiveBonus.RestrictActiveBonus(actor);
					CChangeModifier_MessageData message2 = new CChangeModifier_MessageData(GameState.InternalCurrentActor)
					{
						m_ActiveBonus = ActiveBonus,
						m_OriginalModifier = attackModifierYMLData4.OriginalModifier
					};
					ScenarioRuleClient.MessageHandler(message2);
					if (m_ActiveBonus.HasTracker)
					{
						m_ActiveBonus.UpdateXPTracker();
						m_ActiveBonus.FinishIfNoRemaining();
					}
				}
			}
		}
		else
		{
			if (!cAbilityChangeModifier.GetMiscVar("DoubleMathModifierValue").GetBool())
			{
				return;
			}
			for (int num = 0; num < modifiers.Count; num++)
			{
				AttackModifierYMLData attackModifierYMLData5 = modifiers[num];
				if (attackModifierYMLData5.MathModifier.Length == 2)
				{
					char c = attackModifierYMLData5.MathModifier[0];
					char c2 = attackModifierYMLData5.MathModifier[1];
					int num2 = int.Parse(new string(new char[1] { c2 }));
					num2 *= 2;
					c2 = num2.ToString()[0];
					string mathModifier = c.ToString() + num2;
					AttackModifierYMLData attackModifierYMLData6 = attackModifierYMLData5.Copy(createCard: true);
					modifiers[num] = attackModifierYMLData6;
					attackModifierYMLData6.OriginalModifier = attackModifierYMLData5.MathModifier;
					attackModifierYMLData6.MathModifier = mathModifier;
					attackModifierYMLData6.ModifiedFromModifier = attackModifierYMLData5.Card;
					m_ActiveBonus.RestrictActiveBonus(actor);
					CChangeModifier_MessageData message3 = new CChangeModifier_MessageData(GameState.InternalCurrentActor)
					{
						m_ActiveBonus = ActiveBonus,
						m_OriginalModifier = attackModifierYMLData6.OriginalModifier
					};
					ScenarioRuleClient.MessageHandler(message3);
				}
			}
		}
	}
}

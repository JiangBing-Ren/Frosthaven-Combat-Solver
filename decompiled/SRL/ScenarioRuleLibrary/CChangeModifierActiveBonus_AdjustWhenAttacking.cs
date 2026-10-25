using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CChangeModifierActiveBonus_AdjustWhenAttacking : CBespokeBehaviour
{
	public CBaseCard BaseCard;

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.AdjustWhenAttacking;

	public CChangeModifierActiveBonus_AdjustWhenAttacking(CActor actor, CBaseCard baseCard, CAbilityChangeModifier ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnAttackAbilityFinished(CAbilityAttack attackAbility)
	{
		base.OnAttackAbilityFinished(attackAbility);
		if (attackAbility.TargetingActor == m_Actor && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(m_Actor) && m_Ability.GetMiscVar("EndAfterAttackAbilityFinishIfToggled").GetBool())
		{
			m_ActiveBonus.Finish();
		}
	}

	public override void OnDrawModifier(EAdvantageStatuses advantageStatus, ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target)
	{
		if (actor == m_Actor || (actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == m_Actor))
		{
			for (int i = 0; i < modifiers.Count; i++)
			{
				AttackModifierYMLData modifierToChange = modifiers[i];
				HandleModifier(advantageStatus, ref modifierToChange, actor, target);
				modifiers[i] = modifierToChange;
			}
		}
	}

	public void HandleModifier(EAdvantageStatuses advantageStatus, ref AttackModifierYMLData modifierToChange, CActor actor, CActor target)
	{
		AttackModifierYMLData attackModifierYMLData = modifierToChange;
		CAbilityChangeModifier cAbilityChangeModifier = m_Ability as CAbilityChangeModifier;
		if ((cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers != null || cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.HasValue) && cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifiers != null && cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers.Count == cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifiers.Count)
		{
			if (cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifiers == null || cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers == null || (!cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers.Contains(attackModifierYMLData.MathModifier) && (!cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.HasValue || !cAbilityChangeModifier.MiscAbilityData.ReplaceNegativeModifiers.Value || !attackModifierYMLData.MathModifier.StartsWith('-'))))
			{
				return;
			}
			int num = cAbilityChangeModifier.MiscAbilityData.ReplaceModifiers.IndexOf(attackModifierYMLData.MathModifier);
			if (num < 0 || num >= cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifiers.Count)
			{
				num = 0;
			}
			string replaceWithModifierName = cAbilityChangeModifier.MiscAbilityData.ReplaceWithModifiers[num];
			AttackModifierYMLData attackModifierYMLData2 = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == replaceWithModifierName);
			char operatorSymbol;
			int mathVal;
			if (attackModifierYMLData2 != null)
			{
				if (CheckTrigger(advantageStatus, ref modifierToChange, actor, target))
				{
					AttackModifierYMLData attackModifierYMLData3 = (modifierToChange = attackModifierYMLData2.Copy(createCard: true));
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
					if (m_ActiveBonus.HasTracker)
					{
						m_ActiveBonus.UpdateXPTracker();
						m_ActiveBonus.FinishIfNoRemaining();
					}
				}
			}
			else if (AttackModifierYMLData.TryParseMathModifier(replaceWithModifierName, out operatorSymbol, out mathVal) && CheckTrigger(advantageStatus, ref modifierToChange, actor, target))
			{
				AttackModifierYMLData attackModifierYMLData4 = (modifierToChange = attackModifierYMLData.Copy(createCard: true));
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
		else
		{
			if (!cAbilityChangeModifier.GetMiscVar("DoubleMathModifierValue").GetBool())
			{
				return;
			}
			cAbilityChangeModifier.GetMiscVar("DoubleMathModifierIgnoreValues").GetString(out var val, null, "");
			string[] source = val.Split(',');
			if (attackModifierYMLData.MathModifier.Length == 2 && !source.Contains(attackModifierYMLData.MathModifier) && CheckTrigger(advantageStatus, ref modifierToChange, actor, target))
			{
				char c = attackModifierYMLData.MathModifier[0];
				char c2 = attackModifierYMLData.MathModifier[1];
				int num2 = int.Parse(new string(new char[1] { c2 }));
				num2 *= 2;
				c2 = num2.ToString()[0];
				string mathModifier = c.ToString() + num2;
				AttackModifierYMLData attackModifierYMLData5 = (modifierToChange = attackModifierYMLData.Copy(createCard: true));
				attackModifierYMLData5.OriginalModifier = attackModifierYMLData.MathModifier;
				attackModifierYMLData5.MathModifier = mathModifier;
				attackModifierYMLData5.ModifiedFromModifier = attackModifierYMLData.Card;
				m_ActiveBonus.RestrictActiveBonus(actor);
				CChangeModifier_MessageData message3 = new CChangeModifier_MessageData(GameState.InternalCurrentActor)
				{
					m_ActiveBonus = ActiveBonus,
					m_OriginalModifier = attackModifierYMLData5.OriginalModifier
				};
				ScenarioRuleClient.MessageHandler(message3);
				if (m_ActiveBonus.HasTracker)
				{
					m_ActiveBonus.UpdateXPTracker();
					m_ActiveBonus.FinishIfNoRemaining();
				}
			}
		}
	}

	private bool CheckTrigger(EAdvantageStatuses advantageStatus, ref AttackModifierYMLData modifierToChange, CActor actor, CActor target)
	{
		GameState.ShowToggleActiveBonusSelection(m_Actor as CPlayerActor, target, null, new List<CActiveBonus> { ActiveBonus }, BehaviourType);
		if ((actor == m_Actor || (actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner == m_Actor)) && m_ActiveBonus.IsActiveBonusToggledAndNotRestricted(m_Actor) && m_ActiveBonus.SetupTriggerContext(this, null, m_Ability, null, target, ("AdvantageStatus", new TValue(advantageStatus))))
		{
			return true;
		}
		return false;
	}
}

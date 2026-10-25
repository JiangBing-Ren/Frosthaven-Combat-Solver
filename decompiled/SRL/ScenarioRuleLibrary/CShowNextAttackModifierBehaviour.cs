using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CShowNextAttackModifierBehaviour : CBespokeBehaviour
{
	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.ShowNextAttackModifierBehaviour;

	public CShowNextAttackModifierBehaviour(CActor actor, CAbilityShowNextAttackModifier ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public override void OnDrawModifier(EAdvantageStatuses advantageStatus, ref List<AttackModifierYMLData> modifiers, CActor actor, CActor target)
	{
		base.OnDrawModifier(advantageStatus, ref modifiers, actor, target);
		foreach (CShowNextAttackModifierActiveBonus item in ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => from cActiveBonus in x.FindApplicableActiveBonuses(CAbility.EAbilityType.ShowNextAttackModifier)
			where CustomAbility.SameModifierDeck(cActiveBonus.Actor, actor)
			select cActiveBonus).Distinct().ToList())
		{
			OnModifierDrawnFromDeck(item, ref modifiers);
		}
	}

	private static void OnModifierDrawnFromDeck(CShowNextAttackModifierActiveBonus bonus, ref List<AttackModifierYMLData> modifiers)
	{
		if (modifiers.Any((AttackModifierYMLData x) => x.Shuffle))
		{
			bonus.Finish();
			return;
		}
		if (modifiers != null)
		{
			bool flag = false;
			foreach (AttackModifierYMLData modifier in modifiers)
			{
				AttackModifierYMLData attackModifierYMLData = bonus.NextAttackModifiers.FirstOrDefault((AttackModifierYMLData x) => modifier.Name == x.Name);
				if (attackModifierYMLData != null)
				{
					bonus.NextAttackModifiers.Remove(attackModifierYMLData);
					flag = true;
				}
			}
			if (!flag && bonus.NextAttackModifiers.Count > 0 && bonus.NextAttackModifiers[0] == null)
			{
				bonus.NextAttackModifiers.Clear();
			}
		}
		ValidateModifierDeck(bonus);
	}

	private static void ValidateModifierDeck(CShowNextAttackModifierActiveBonus bonus)
	{
		if (bonus.NextAttackModifiers.Count == 0)
		{
			bonus.Finish();
			return;
		}
		List<AttackModifierYMLData> nextAttackModifiers = bonus.NextAttackModifiers;
		List<AttackModifierYMLData> nextModifiers = bonus.GetNextModifiers(nextAttackModifiers.Count);
		for (int i = 0; i < nextAttackModifiers.Count; i++)
		{
			if (nextAttackModifiers[i] != nextModifiers[i])
			{
				bonus.Finish();
				break;
			}
		}
	}

	public static void ValidateAllShowNextModifierBonuses(CActor actor)
	{
		foreach (CShowNextAttackModifierActiveBonus item in ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => from cActiveBonus in x.FindApplicableActiveBonuses(CAbility.EAbilityType.ShowNextAttackModifier)
			where CustomAbility.SameModifierDeck(cActiveBonus.Actor, actor)
			select cActiveBonus).Distinct().ToList())
		{
			ValidateModifierDeck(item);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CShowNextAttackModifierActiveBonus : CActiveBonus
{
	public List<AttackModifierYMLData> NextAttackModifiers;

	public CShowNextAttackModifierActiveBonus(CBaseCard baseCard, CAbility ability, CActor actor, CActor caster, int activeBonusStartRound, int? id, int? remaining = null)
		: base(baseCard, ability, actor, caster, activeBonusStartRound, id, remaining)
	{
		AddBespokeBehaviour(new CShowNextAttackModifierBehaviour(Actor, ability as CAbilityShowNextAttackModifier, this));
		RefreshNextModifiers(ability.Strength);
	}

	public void RefreshNextModifiers(int strength)
	{
		NextAttackModifiers = GetNextModifiers(strength);
	}

	public List<AttackModifierYMLData> GetNextModifiers(int cnt)
	{
		List<AttackModifierYMLData> list = new List<AttackModifierYMLData>();
		List<AttackModifierYMLData> attackModifierDeck = CustomAbility.GetAttackModifierDeck(Actor);
		for (int i = 0; i < Math.Min(attackModifierDeck.Count, cnt); i++)
		{
			list.Add(attackModifierDeck[i]);
		}
		if (list.Count == 0)
		{
			list.Add(null);
		}
		return list;
	}

	public override void Save(List<Tuple<string, object>> toSave)
	{
		base.Save(toSave);
		List<string> value = NextAttackModifiers.Select((AttackModifierYMLData m) => m?.Name ?? string.Empty).ToList();
		toSave.Add(new Tuple<string, object>("additionalSerialization", JsonConvert.SerializeObject(value)));
	}

	public override void Load(List<Tuple<string, object>> toLoad)
	{
		base.Load(toLoad);
		string text = (string)toLoad.SingleOrDefault((Tuple<string, object> x) => x.Item1.Equals("additionalSerialization", StringComparison.InvariantCultureIgnoreCase)).Item2;
		if (int.TryParse(text, out var result))
		{
			RefreshNextModifiers(result);
			return;
		}
		List<string> source = JsonConvert.DeserializeObject<List<string>>(text);
		List<AttackModifierYMLData> pool = GetAttackModifierPool();
		NextAttackModifiers = source.Select((string n) => (!string.IsNullOrEmpty(n)) ? pool?.FirstOrDefault((AttackModifierYMLData m) => m.Name == n) : null).ToList();
	}

	private List<AttackModifierYMLData> GetAttackModifierPool()
	{
		if (Actor is CPlayerActor cPlayerActor)
		{
			return cPlayerActor.CharacterClass.AttackModifierCardsPool;
		}
		return CustomAbility.GetAttackModifierDeck(Actor);
	}

	public string NextAttackModifierString()
	{
		return NextAttackModifierString(NextAttackModifiers);
	}

	public static string NextAttackModifierString(List<AttackModifierYMLData> NextAttackModifiers)
	{
		string text = string.Empty;
		foreach (AttackModifierYMLData NextAttackModifier in NextAttackModifiers)
		{
			if (NextAttackModifier == null)
			{
				return "$GUI_NextAttackModifier_Fallback$";
			}
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += ((!string.IsNullOrEmpty(NextAttackModifier.CustomCombatLogDescriptionLocKey)) ? ("$" + NextAttackModifier.Card.AttackModifierLogString() + "$") : NextAttackModifier.Card.AttackModifierLogString());
		}
		return text;
	}

	public static void RefreshAllShowNextModifierBonuses(CActor actor)
	{
		foreach (CShowNextAttackModifierActiveBonus item in ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => from cActiveBonus in x.FindApplicableActiveBonuses(CAbility.EAbilityType.ShowNextAttackModifier)
			where CustomAbility.SameModifierDeck(cActiveBonus.Actor, actor)
			select cActiveBonus).Distinct().ToList())
		{
			item.RefreshNextModifiers(item.NextAttackModifiers.Count);
		}
	}
}

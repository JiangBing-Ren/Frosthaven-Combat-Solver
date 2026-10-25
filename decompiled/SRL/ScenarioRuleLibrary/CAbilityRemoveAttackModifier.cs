using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityRemoveAttackModifier : CAbilityTargeting
{
	[JsonProperty]
	public List<string> ModifierCardNames { get; private set; }

	public CAbilityRemoveAttackModifier(List<string> modifierCardNames)
		: base(EAbilityType.RemoveAttackModifier)
	{
		ModifierCardNames = new List<string>(modifierCardNames);
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			AbilityHasHappened = true;
			if (!(actor is CEnemyActor enemyActor))
			{
				CCharacterClass cCharacterClass = null;
				cCharacterClass = ((!(actor is CHeroSummonActor cHeroSummonActor)) ? (actor.Class as CCharacterClass) : (cHeroSummonActor.Summoner.Class as CCharacterClass));
				cCharacterClass.RemoveModifierCards(ModifierCardNames);
			}
			else
			{
				MonsterClassManager.RemoveModifierCards(enemyActor, ModifierCardNames);
			}
		}
		return true;
	}
}

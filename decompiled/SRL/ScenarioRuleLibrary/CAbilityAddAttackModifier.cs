using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityAddAttackModifier : CAbilityTargeting
{
	[JsonProperty]
	public List<string> ModifierCardNames { get; private set; }

	[JsonProperty]
	public bool ToDiscardPile { get; private set; }

	public CAbilityAddAttackModifier(List<string> modifierCardNames, bool toDiscard)
		: base(EAbilityType.AddAttackModifier)
	{
		ModifierCardNames = new List<string>(modifierCardNames);
		ToDiscardPile = toDiscard;
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
				cCharacterClass.AddAdditionalModifierCards(ModifierCardNames, ToDiscardPile);
			}
			else
			{
				MonsterClassManager.AddAdditionalModifierCards(enemyActor, ModifierCardNames);
			}
		}
		return true;
	}
}

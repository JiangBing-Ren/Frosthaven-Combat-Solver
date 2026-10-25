using ScenarioRuleLibrary;

public class MonsterCardChoice : IAbilityCardOption
{
	public CMonsterClass MonsterClass;

	public CBaseCard AbilityCard { get; private set; }

	public CBaseCard.ActionType ActionTypeDisabled { get; private set; }

	public CAbilityChooseAbility ChooseAbility { get; private set; }

	public MonsterCardChoice(CAbilityChooseAbility chooseAbility, CMonsterAbilityCard abilityCard, CMonsterClass monsterClass)
	{
		ChooseAbility = chooseAbility;
		AbilityCard = abilityCard;
		ActionTypeDisabled = CBaseCard.ActionType.NA;
		MonsterClass = monsterClass;
	}
}

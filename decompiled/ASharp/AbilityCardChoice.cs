using ScenarioRuleLibrary;

public class AbilityCardChoice : IAbilityCardOption
{
	public CCharacterClass CharacterClass;

	public CBaseCard AbilityCard { get; private set; }

	public CBaseCard.ActionType ActionTypeDisabled { get; private set; }

	public AbilityCardChoice(CAbilityCard abilityCard, CCharacterClass characterClass, CBaseCard.ActionType actionTypeDisabled = CBaseCard.ActionType.NA)
	{
		AbilityCard = abilityCard;
		ActionTypeDisabled = actionTypeDisabled;
		CharacterClass = characterClass;
	}
}

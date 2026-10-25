using ScenarioRuleLibrary;

public interface IAbilityCardOption
{
	CBaseCard AbilityCard { get; }

	CBaseCard.ActionType ActionTypeDisabled { get; }
}

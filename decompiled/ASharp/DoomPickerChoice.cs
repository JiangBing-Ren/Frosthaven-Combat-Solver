using ScenarioRuleLibrary;

public class DoomPickerChoice : IAbilityCardOption
{
	public CBaseCard AbilityCard { get; private set; }

	public CBaseCard.ActionType ActionTypeDisabled { get; private set; }

	public CDoom Doom { get; private set; }

	public DoomPickerChoice(CDoom doom, CAbilityCard abilityCard)
	{
		Doom = doom;
		AbilityCard = abilityCard;
		ActionTypeDisabled = ((abilityCard.GetAbilityActionType(doom.DoomAbilities[0]) == CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction);
	}

	public DoomPickerChoice(CActiveBonus bonus)
	{
		Doom = (bonus.Ability as CAbilityAddDoom).Doom;
		AbilityCard = bonus.BaseCard as CAbilityCard;
		ActionTypeDisabled = ((AbilityCard.GetAbilityActionType(bonus.Ability) == CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.TopAction : CBaseCard.ActionType.BottomAction);
	}
}

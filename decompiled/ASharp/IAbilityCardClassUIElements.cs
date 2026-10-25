using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public interface IAbilityCardClassUIElements
{
	void SetupCharacterClassElements(string cardName, AbilityCardYMLData cardData);

	void UpdateCharacterClassElements(CPlayerActor cPlayerActor);
}

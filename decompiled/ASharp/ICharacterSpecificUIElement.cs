using ScenarioRuleLibrary;

public interface ICharacterSpecificUIElement
{
	bool IsCharacterElement(CPlayerActor playerActor);

	void Init(CPlayerActor cPlayer);
}

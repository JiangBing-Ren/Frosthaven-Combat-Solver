using ScenarioRuleLibrary.YML;

public interface ICharacter
{
	string CharacterID { get; }

	string CharacterName { get; }

	CharacterYMLData Class { get; }

	bool IsMapCharacterUnderMyControl { get; }
}

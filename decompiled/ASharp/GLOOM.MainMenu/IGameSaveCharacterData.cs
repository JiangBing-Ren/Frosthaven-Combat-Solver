using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

public interface IGameSaveCharacterData
{
	ECharacter CharacterModel { get; }

	string CharacterName { get; }

	int Level { get; }

	int? Gold { get; }

	DLCRegistry.EDLCKey BelongsToDLC(List<DLCRegistry.EDLCKey> dlcs);
}

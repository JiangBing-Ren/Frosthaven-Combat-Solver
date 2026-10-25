using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace GLOOM.MainMenu;

internal class GameSaveCharacterData : IGameSaveCharacterData
{
	public ECharacter CharacterModel { get; private set; }

	public string CharacterName { get; private set; }

	public int Level { get; private set; }

	public int? Gold { get; private set; }

	public DLCRegistry.EDLCKey BelongsToDLC(List<DLCRegistry.EDLCKey> dlcs)
	{
		foreach (DLCRegistry.EDLCKey dlc in dlcs)
		{
			if (DLCRegistry.AllDLCCharacters.ContainsKey(dlc) && DLCRegistry.AllDLCCharacters[dlc].Contains(CharacterModel))
			{
				return dlc;
			}
		}
		return DLCRegistry.EDLCKey.None;
	}

	public GameSaveCharacterData(ECharacter model, string name, int level, int? gold)
	{
		Level = level;
		CharacterModel = model;
		CharacterName = name;
		Gold = gold;
	}
}

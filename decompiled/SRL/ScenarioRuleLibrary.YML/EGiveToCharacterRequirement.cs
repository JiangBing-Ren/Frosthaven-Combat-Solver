using System;
using FH.Tools;

namespace ScenarioRuleLibrary.YML;

[Serializable]
[KronosEnumPolice(0)]
public enum EGiveToCharacterRequirement
{
	None,
	CharacterHasPersonalQuest
}

using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(1)]
public enum EScenarioModifierActivationType
{
	None,
	QuestCompleted,
	PersonalQuestOwnerInParty
}

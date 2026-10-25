using System;
using FH.Tools;

namespace ScenarioRuleLibrary.YML;

[Serializable]
[KronosEnumPolice(0)]
public enum EGiveToCharacterType
{
	None,
	Give,
	Equip
}

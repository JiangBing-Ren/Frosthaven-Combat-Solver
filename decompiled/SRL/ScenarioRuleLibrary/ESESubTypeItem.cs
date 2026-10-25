using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

[Serializable]
[KronosEnumPolice(0)]
public enum ESESubTypeItem
{
	None,
	ItemAdded,
	ItemSpent,
	ItemConsumed,
	ItemUnrestrictedUsed,
	ItemRefreshed,
	ItemSelected,
	ItemDeSelected,
	ItemUsable,
	ItemNoLongerUsable,
	ItemActive,
	ItemFlipped,
	ItemRemoved,
	ItemLocked
}

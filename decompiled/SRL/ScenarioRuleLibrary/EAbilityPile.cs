using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EAbilityPile
{
	None,
	Hand,
	Round,
	Activated,
	Discarded,
	Lost,
	PermaLost
}

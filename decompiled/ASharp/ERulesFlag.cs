using System;

[Serializable]
[Flags]
public enum ERulesFlag
{
	None = 0,
	ReducedRandomness = 1,
	OptionalRetirement = 2,
	Ironman = 4,
	MergedPartyResources = 8,
	AutoOutpost = 0x10,
	AutoCardSelect = 0x20,
	CanRespecCharacters = 0x40
}

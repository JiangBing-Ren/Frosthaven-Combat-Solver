using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class CConditionalOverride
{
	[JsonProperty]
	public string Name { get; private set; }

	[JsonProperty]
	public CAbilityFilterContainer Filter { get; private set; }

	[JsonProperty]
	public CAbilityRequirements Requirements { get; private set; }

	[JsonProperty]
	public int XP { get; private set; }

	[JsonProperty]
	public List<CAbilityOverride> AbilityOverrides { get; private set; }

	[JsonProperty]
	public bool Self { get; private set; }

	[JsonProperty]
	public CAbilityRequirements TargetRequirements { get; private set; }

	[JsonConstructor]
	public CConditionalOverride(string name, CAbilityFilterContainer filter, CAbilityRequirements requirements, int xp, List<CAbilityOverride> abilityOverrides, bool self, CAbilityRequirements targetRequirements)
	{
		Name = name;
		Filter = filter;
		Requirements = requirements;
		XP = xp;
		AbilityOverrides = abilityOverrides;
		foreach (CAbilityOverride abilityOverride in abilityOverrides)
		{
			abilityOverride.OriginalConditionalOverride = this;
		}
		Self = self;
		TargetRequirements = targetRequirements;
	}

	public CConditionalOverride Copy()
	{
		return new CConditionalOverride(Name, Filter.Copy(), Requirements.Copy(), XP, AbilityOverrides.Select((CAbilityOverride x) => CAbilityOverride.CopyAbilityOverride(x)).ToList(), Self, TargetRequirements?.Copy());
	}
}

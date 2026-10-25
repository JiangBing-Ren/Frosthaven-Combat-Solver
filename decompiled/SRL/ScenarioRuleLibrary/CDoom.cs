using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CDoom
{
	public Guid ID;

	[JsonProperty]
	public List<CAbility> DoomAbilities;

	public string ActiveBonusLayout;

	public CDoom(List<CAbility> doomAbilities)
	{
		ID = Guid.NewGuid();
		DoomAbilities = doomAbilities;
	}
}

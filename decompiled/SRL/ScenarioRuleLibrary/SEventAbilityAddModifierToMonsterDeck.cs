using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityAddModifierToMonsterDeck : SEventAbility
{
	public CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState AddModifierToMonsterDeckState { get; private set; }

	public List<string> AddModifierStrings { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AddModifierToMonsterDeckState", AddModifierToMonsterDeckState);
		info.AddValue("AddModifierStrings", AddModifierStrings);
	}

	public SEventAbilityAddModifierToMonsterDeck(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			string name = item.Name;
			if (!(name == "AddModifierToMonsterDeckState"))
			{
				if (name == "AddModifierStrings")
				{
					AddModifierStrings = (List<string>)info.GetValue("AddModifierStrings", typeof(List<string>));
				}
			}
			else
			{
				AddModifierToMonsterDeckState = (CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState)info.GetValue("AddModifierToMonsterDeckState", typeof(CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState));
			}
		}
	}

	public SEventAbilityAddModifierToMonsterDeck(SEventAbilityBaseData baseData, CAbilityAddModifierToMonsterDeck.EAddModifierToMonsterDeckState addModifierToMonsterDeckState, List<string> addModifierStrings)
		: base(baseData)
	{
		AddModifierToMonsterDeckState = addModifierToMonsterDeckState;
		AddModifierStrings = addModifierStrings;
	}
}

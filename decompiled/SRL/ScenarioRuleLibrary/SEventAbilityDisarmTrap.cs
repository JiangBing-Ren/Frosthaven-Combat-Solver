using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDisarmTrap : SEventAbility
{
	public CAbilityDisarmTrap.DisarmTrapState DisarmTrapState { get; private set; }

	public Dictionary<string, int> TrapsDisarmedDictionary { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DisarmTrapState", DisarmTrapState);
		info.AddValue("TrapsDisarmedDictionary", TrapsDisarmedDictionary);
	}

	public SEventAbilityDisarmTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "DisarmTrapState"))
				{
					if (name == "TrapsDisarmedDictionary")
					{
						TrapsDisarmedDictionary = (Dictionary<string, int>)info.GetValue("TrapsDisarmedDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					DisarmTrapState = (CAbilityDisarmTrap.DisarmTrapState)info.GetValue("DisarmTrapState", typeof(CAbilityDisarmTrap.DisarmTrapState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDisarmTrap entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDisarmTrap(SEventAbilityBaseData baseData, CAbilityDisarmTrap.DisarmTrapState disarmTrapState, Dictionary<string, int> trapsDisarmedDictionary)
		: base(baseData)
	{
		DisarmTrapState = disarmTrapState;
		TrapsDisarmedDictionary = trapsDisarmedDictionary;
	}
}

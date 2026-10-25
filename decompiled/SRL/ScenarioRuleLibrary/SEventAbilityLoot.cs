using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityLoot : SEventAbility
{
	public CAbilityLoot.ELootState LootState { get; private set; }

	public List<string> PropsLooted { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootState", LootState);
		info.AddValue("PropsLooted", PropsLooted);
	}

	public SEventAbilityLoot(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "LootState"))
				{
					if (name == "PropsLooted")
					{
						PropsLooted = (List<string>)info.GetValue("PropsLooted", typeof(List<string>));
					}
				}
				else
				{
					LootState = (CAbilityLoot.ELootState)info.GetValue("LootState", typeof(CAbilityLoot.ELootState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityLoot entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityLoot(SEventAbilityBaseData baseData, CAbilityLoot.ELootState lootState, List<string> propsLooted)
		: base(baseData)
	{
		LootState = lootState;
		PropsLooted = propsLooted;
	}
}

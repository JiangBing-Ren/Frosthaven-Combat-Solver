using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilitySummon : SEventAbility
{
	public CAbilitySummon.SummonState SummonState { get; private set; }

	public string SummonName { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SummonState", SummonState);
		info.AddValue("SummonName", SummonName);
	}

	public SEventAbilitySummon(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "SummonState"))
				{
					if (name == "SummonName")
					{
						SummonName = info.GetString("SummonName");
					}
				}
				else
				{
					SummonState = (CAbilitySummon.SummonState)info.GetValue("SummonState", typeof(CAbilitySummon.SummonState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilitySummon entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilitySummon(SEventAbilityBaseData baseData, CAbilitySummon.SummonState summonState, string summonName)
		: base(baseData)
	{
		SummonState = summonState;
		SummonName = summonName;
	}
}

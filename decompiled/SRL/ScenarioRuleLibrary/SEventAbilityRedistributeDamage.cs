using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityRedistributeDamage : SEventAbility
{
	public CAbilityRedistributeDamage.ERedistributeState RedistributeState { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SwapState", RedistributeState);
	}

	public SEventAbilityRedistributeDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			if (item.Name == "RedistributeState")
			{
				RedistributeState = (CAbilityRedistributeDamage.ERedistributeState)info.GetValue("RedistributeState", typeof(CAbilityRedistributeDamage.ERedistributeState));
			}
		}
	}

	public SEventAbilityRedistributeDamage(SEventAbilityBaseData baseData, CAbilityRedistributeDamage.ERedistributeState redistributeState)
		: base(baseData)
	{
		RedistributeState = redistributeState;
	}
}

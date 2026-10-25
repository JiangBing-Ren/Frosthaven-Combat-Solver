using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDamage : SEventAbility
{
	public CAbilityDamage.EDamageState DamageState { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DamageState", DamageState);
	}

	public SEventAbilityDamage(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "DamageState")
				{
					DamageState = (CAbilityDamage.EDamageState)info.GetValue("DamageState", typeof(CAbilityDamage.EDamageState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDamage entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDamage(SEventAbilityBaseData baseData, CAbilityDamage.EDamageState damageState)
		: base(baseData)
	{
		DamageState = damageState;
	}
}

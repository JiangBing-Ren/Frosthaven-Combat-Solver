using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityNull : SEventAbility
{
	public CAbilityNull.ENullState NullState { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("NullState", NullState);
	}

	public SEventAbilityNull(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "NullState")
				{
					NullState = (CAbilityNull.ENullState)info.GetValue("NullState", typeof(CAbilityNull.ENullState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityNull entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityNull(SEventAbilityBaseData baseData, CAbilityNull.ENullState nullState)
		: base(baseData)
	{
		NullState = nullState;
	}
}

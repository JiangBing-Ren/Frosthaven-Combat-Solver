using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDestroyProp : SEventAbility
{
	public EPropType PropType;

	public ESpecificPropType SpecificPropType;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PropType", PropType);
		info.AddValue("SpecificPropType", SpecificPropType);
	}

	public SEventAbilityDestroyProp(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PropType"))
				{
					if (name == "SpecificPropType")
					{
						SpecificPropType = (ESpecificPropType)info.GetValue("SpecificPropType", typeof(ESpecificPropType));
					}
				}
				else
				{
					PropType = (EPropType)info.GetValue("PropType", typeof(EPropType));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDestroyProp entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDestroyProp(EPropType PropType, ESpecificPropType SpecificPropType, SEventAbilityBaseData baseData)
		: base(baseData)
	{
		this.PropType = PropType;
		this.SpecificPropType = SpecificPropType;
	}
}

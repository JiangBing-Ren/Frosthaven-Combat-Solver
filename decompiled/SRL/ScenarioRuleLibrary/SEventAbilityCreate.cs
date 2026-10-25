using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityCreate : SEventAbility
{
	public CAbilityCreate.CreateState CreateState { get; private set; }

	public string PropName { get; private set; }

	public int PropsSpawned { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CreateState", CreateState);
		info.AddValue("PropName", PropName);
		info.AddValue("PropsSpawned", PropsSpawned);
	}

	public SEventAbilityCreate(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CreateState":
					CreateState = (CAbilityCreate.CreateState)info.GetValue("CreateState", typeof(CAbilityCreate.CreateState));
					break;
				case "PropName":
					PropName = info.GetString("PropName");
					break;
				case "PropsSpawned":
					PropsSpawned = info.GetInt32("PropsSpawned");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityCreate entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityCreate(SEventAbilityBaseData baseData, CAbilityCreate.CreateState createState, string propName, int propsSpawned)
		: base(baseData)
	{
		CreateState = createState;
		PropName = propName;
		PropsSpawned = propsSpawned;
	}
}

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDeactivateSpawner : SEventAbility
{
	public CAbilityDeactivateSpawner.EDeactivateSpawnerState DeactivateSpawnerState { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DeactivateSpawnerState", DeactivateSpawnerState);
	}

	public SEventAbilityDeactivateSpawner(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "DeactivateSpawnerState")
				{
					DeactivateSpawnerState = (CAbilityDeactivateSpawner.EDeactivateSpawnerState)info.GetValue("DeactivateSpawnerState", typeof(CAbilityDeactivateSpawner.EDeactivateSpawnerState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDeactivateSpawner entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDeactivateSpawner(SEventAbilityBaseData baseData, CAbilityDeactivateSpawner.EDeactivateSpawnerState deactivateSpawnerState)
		: base(baseData)
	{
		DeactivateSpawnerState = deactivateSpawnerState;
	}
}

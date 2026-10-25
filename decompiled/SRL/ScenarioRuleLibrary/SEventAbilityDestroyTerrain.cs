using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityDestroyTerrain : SEventAbility
{
	public CAbilityDestroyTerrain.DestroyObstacleState DestroyObstacleState { get; private set; }

	public Dictionary<string, int> DestroyedPropsDictionary { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DestroyObstacleState", DestroyObstacleState);
		info.AddValue("DestroyedPropNames", DestroyedPropsDictionary);
	}

	public SEventAbilityDestroyTerrain(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "DestroyObstacleState"))
				{
					if (name == "DestroyedPropsDictionary")
					{
						DestroyedPropsDictionary = (Dictionary<string, int>)info.GetValue("DestroyedPropsDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					DestroyObstacleState = (CAbilityDestroyTerrain.DestroyObstacleState)info.GetValue("DestroyObstacleState", typeof(CAbilityDestroyTerrain.DestroyObstacleState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDestroyObstacle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityDestroyTerrain(SEventAbilityBaseData baseData, CAbilityDestroyTerrain.DestroyObstacleState destroyObstacleState, Dictionary<string, int> destroyedPropsDictionary)
		: base(baseData)
	{
		DestroyObstacleState = destroyObstacleState;
		DestroyedPropsDictionary = destroyedPropsDictionary;
	}
}

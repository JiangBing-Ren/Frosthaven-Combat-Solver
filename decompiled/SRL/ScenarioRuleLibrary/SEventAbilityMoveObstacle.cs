using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityMoveObstacle : SEventAbility
{
	public CAbilityMoveObstacle.MoveObstacleState MovedObstacleState { get; private set; }

	public Dictionary<string, int> MovedPropsDictionary { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MovedObstacleState", MovedObstacleState);
		info.AddValue("MovedPropsDictionary", MovedPropsDictionary);
	}

	public SEventAbilityMoveObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "MovedObstacleState"))
				{
					if (name == "MovedPropsDictionary")
					{
						MovedPropsDictionary = (Dictionary<string, int>)info.GetValue("MovedPropsDictionary", typeof(Dictionary<string, int>));
					}
				}
				else
				{
					MovedObstacleState = (CAbilityMoveObstacle.MoveObstacleState)info.GetValue("MovedObstacleState", typeof(CAbilityMoveObstacle.MoveObstacleState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityMoveObstacle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityMoveObstacle(SEventAbilityBaseData baseData, CAbilityMoveObstacle.MoveObstacleState moveObstacleState, Dictionary<string, int> movedPropsDictionary)
		: base(baseData)
	{
		MovedObstacleState = moveObstacleState;
		MovedPropsDictionary = movedPropsDictionary;
	}
}

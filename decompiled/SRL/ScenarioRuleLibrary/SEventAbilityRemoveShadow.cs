using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityRemoveShadow : SEventAbility
{
	public CAbilityRemoveShadow.RemoveShadowState RemoveShadowState { get; private set; }

	public int Targets { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("RemoveShadowState", RemoveShadowState);
		info.AddValue("Targets", Targets);
	}

	public SEventAbilityRemoveShadow(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "RemoveShadowState"))
				{
					if (name == "Targets")
					{
						Targets = info.GetInt32("Targets");
					}
				}
				else
				{
					RemoveShadowState = (CAbilityRemoveShadow.RemoveShadowState)info.GetValue("RemoveShadowState", typeof(CAbilityRemoveShadow.RemoveShadowState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityDestroyObstacle entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityRemoveShadow(SEventAbilityBaseData baseData, CAbilityRemoveShadow.RemoveShadowState destroyObstacleState, int targets)
		: base(baseData)
	{
		RemoveShadowState = destroyObstacleState;
		Targets = targets;
	}
}

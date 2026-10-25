using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityPush : SEventAbility
{
	public CAbilityPush.EPushState PushState { get; private set; }

	public List<CAbilityPush.PushedActorStats> ActorsDistanceMoved { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PushState", PushState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityPush(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PushState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityPush.PushedActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityPush.PushedActorStats>));
					}
				}
				else
				{
					PushState = (CAbilityPush.EPushState)info.GetValue("PushState", typeof(CAbilityPush.EPushState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityPush entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityPush(SEventAbilityBaseData baseData, CAbilityPush.EPushState pushState, List<CAbilityPush.PushedActorStats> actorsDistanceMoved)
		: base(baseData)
	{
		PushState = pushState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}

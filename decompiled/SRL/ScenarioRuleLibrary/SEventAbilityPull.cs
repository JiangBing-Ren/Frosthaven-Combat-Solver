using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityPull : SEventAbility
{
	public CAbilityPull.EPullState PullState { get; private set; }

	public List<CAbilityPull.PulledActorStats> ActorsDistanceMoved { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PullState", PullState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityPull(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "PullState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityPull.PulledActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityPull.PulledActorStats>));
					}
				}
				else
				{
					PullState = (CAbilityPull.EPullState)info.GetValue("PullState", typeof(CAbilityPull.EPullState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityPull entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityPull(SEventAbilityBaseData baseData, CAbilityPull.EPullState pullState, List<CAbilityPull.PulledActorStats> actorsDistanceMoved)
		: base(baseData)
	{
		PullState = pullState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}

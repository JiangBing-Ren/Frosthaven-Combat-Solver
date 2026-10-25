using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityFear : SEventAbility
{
	public CAbilityFear.EFearState FearState { get; private set; }

	public List<CAbilityFear.FearedActorStats> ActorsDistanceMoved { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("FearState", FearState);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityFear(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "FearState"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityFear.FearedActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityFear.FearedActorStats>));
					}
				}
				else
				{
					FearState = (CAbilityFear.EFearState)info.GetValue("FearState", typeof(CAbilityFear.EFearState));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityPush entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityFear(SEventAbilityBaseData baseData, CAbilityFear.EFearState fearState, List<CAbilityFear.FearedActorStats> actorsDistanceMoved)
		: base(baseData)
	{
		FearState = fearState;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}

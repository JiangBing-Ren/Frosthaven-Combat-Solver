using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTeleport2 : SEventAbility
{
	public CAbilityTeleport2.ETeleport2State Teleport2State { get; private set; }

	public List<CAbilityTeleport2.Teleported2ActorStats> ActorsDistanceMoved { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Teleport2State", Teleport2State);
		info.AddValue("ActorsDistanceMoved", ActorsDistanceMoved);
	}

	public SEventAbilityTeleport2(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "Teleport2State"))
				{
					if (name == "ActorsDistanceMoved")
					{
						ActorsDistanceMoved = (List<CAbilityTeleport2.Teleported2ActorStats>)info.GetValue("ActorsDistanceMoved", typeof(List<CAbilityTeleport2.Teleported2ActorStats>));
					}
				}
				else
				{
					Teleport2State = (CAbilityTeleport2.ETeleport2State)info.GetValue("Teleport2State", typeof(CAbilityTeleport2.ETeleport2State));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityTeleport2 entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTeleport2(SEventAbilityBaseData baseData, CAbilityTeleport2.ETeleport2State teleport2State, List<CAbilityTeleport2.Teleported2ActorStats> actorsDistanceMoved)
		: base(baseData)
	{
		Teleport2State = teleport2State;
		ActorsDistanceMoved = actorsDistanceMoved;
	}
}

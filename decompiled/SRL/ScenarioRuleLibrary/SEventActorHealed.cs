using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorHealed : SEventActor
{
	public int HealAmount { get; private set; }

	public bool PoisonRemoved { get; private set; }

	public bool WoundRemoved { get; private set; }

	public bool BaneRemoved { get; private set; }

	public bool BrittleRemoved { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("HealAmount", HealAmount);
		info.AddValue("PoisonRemoved", PoisonRemoved);
		info.AddValue("WoundRemoved", WoundRemoved);
	}

	public SEventActorHealed(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "HealAmount":
					HealAmount = info.GetInt32("HealAmount");
					break;
				case "PoisonRemoved":
					PoisonRemoved = info.GetBoolean("PoisonRemoved");
					break;
				case "WoundRemoved":
					WoundRemoved = info.GetBoolean("WoundRemoved");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorHealed entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorHealed(bool poisonRemoved, bool woundRemoved, bool baneRemoved, bool brittleRemoved, int healAmount, SEventActorBaseData baseData)
		: base(baseData)
	{
		PoisonRemoved = poisonRemoved;
		WoundRemoved = woundRemoved;
		BaneRemoved = baneRemoved;
		HealAmount = healAmount;
		BrittleRemoved = brittleRemoved;
	}
}

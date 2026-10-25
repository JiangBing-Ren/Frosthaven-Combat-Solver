using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorLootedRandomItemBlueprint : SEventActor
{
	public string ActorDroppingLoot { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorDroppingLoot", ActorDroppingLoot);
	}

	public SEventActorLootedRandomItemBlueprint(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ActorDroppingLoot")
				{
					ActorDroppingLoot = info.GetString("ActorDroppingLoot");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorLooted entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorLootedRandomItemBlueprint(string actorActivated, SEventActorBaseData baseData)
		: base(baseData)
	{
		ActorDroppingLoot = actorActivated;
	}
}

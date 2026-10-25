using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActorLooted : SEventActor
{
	public ScenarioManager.ObjectImportType LootedType { get; private set; }

	public string ActorDroppingLoot { get; private set; }

	public bool EndOfTurn { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("LootedType", LootedType);
		info.AddValue("ActorDroppingLoot", ActorDroppingLoot);
		info.AddValue("EndOfTurn", EndOfTurn);
	}

	public SEventActorLooted(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "LootedType":
					LootedType = (ScenarioManager.ObjectImportType)info.GetValue("LootedType", typeof(ScenarioManager.ObjectImportType));
					break;
				case "ActorDroppingLoot":
					ActorDroppingLoot = info.GetString("ActorDroppingLoot");
					break;
				case "EndOfTurn":
					EndOfTurn = info.GetBoolean("EndOfTurn");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventActorLooted entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventActorLooted(ScenarioManager.ObjectImportType lootedType, string actorActivated, bool endOfTurn, SEventActorBaseData baseData)
		: base(baseData)
	{
		LootedType = lootedType;
		ActorDroppingLoot = actorActivated;
		EndOfTurn = endOfTurn;
	}
}

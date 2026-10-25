using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventHand : SEvent
{
	public string CharacterID { get; private set; }

	public int HandSize { get; private set; }

	public int DiscardSize { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("HandSize", HandSize);
		info.AddValue("DiscardSize", DiscardSize);
	}

	public SEventHand(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "HandSize":
					HandSize = info.GetInt32("HandSize");
					break;
				case "DiscardSize":
					DiscardSize = info.GetInt32("DiscardSize");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventHand entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventHand(string characterID, int handSize, int discardSize, string text = "")
		: base(ESEType.Donate, text)
	{
		CharacterID = characterID;
		HandSize = handSize;
		DiscardSize = discardSize;
	}
}

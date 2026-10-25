using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventPersonalQuest : SEvent
{
	public string CharacterID { get; private set; }

	public int Amount { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Amount", Amount);
	}

	public SEventPersonalQuest(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CharacterID"))
				{
					if (name == "Amount")
					{
						Amount = info.GetInt32("Amount");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventPersonalQuest entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventPersonalQuest(string characterID, int amount, string text = "")
		: base(ESEType.PersonalQuest, text)
	{
		CharacterID = characterID;
		Amount = amount;
	}
}

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventRecoverCard : SEvent
{
	public string CharacterID { get; private set; }

	public bool WasDiscarded { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("WasDiscarded", WasDiscarded);
	}

	public SEventRecoverCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CharacterID"))
				{
					if (name == "WasDiscarded")
					{
						WasDiscarded = info.GetBoolean("WasDiscarded");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventEnhancement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventRecoverCard(string characterID, bool wasDiscarded, string text = "")
		: base(ESEType.LoseCard, text)
	{
		CharacterID = characterID;
		WasDiscarded = wasDiscarded;
	}
}

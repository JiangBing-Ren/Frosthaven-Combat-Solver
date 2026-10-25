using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLostAdjacency : SEvent
{
	public string CharacterID { get; private set; }

	public bool WasForced { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("WasForced", WasForced);
	}

	public SEventLostAdjacency(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CharacterID"))
				{
					if (name == "WasForced")
					{
						WasForced = info.GetBoolean("WasForced");
					}
				}
				else
				{
					CharacterID = info.GetString("CharacterID");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventLostAdjacency entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventLostAdjacency(string characterID, bool wasForced, string text = "")
		: base(ESEType.None, text)
	{
		CharacterID = characterID;
		WasForced = wasForced;
	}
}

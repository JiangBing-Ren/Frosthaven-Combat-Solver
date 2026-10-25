using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary.YML;

[Serializable]
public class CharacterPerk : ISerializable
{
	public string PerkID;

	public string CharacterID;

	public bool IsActive;

	public PerksYMLData Perk => ScenarioRuleClient.SRLYML.Perks.SingleOrDefault((PerksYMLData s) => s.ID == PerkID);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PerkID", PerkID);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("IsActive", IsActive);
	}

	private CharacterPerk(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PerkID":
					PerkID = info.GetString("PerkID");
					break;
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "IsActive":
					IsActive = info.GetBoolean("IsActive");
					break;
				case "PerkName":
				{
					string perkName = info.GetString("PerkName");
					PerksYMLData perksYMLData = ScenarioRuleClient.SRLYML.Perks.SingleOrDefault((PerksYMLData s) => s.Name == perkName);
					PerkID = perksYMLData.ID;
					break;
				}
				case "Character":
					CharacterID = ((ECharacter)info.GetValue("Character", typeof(ECharacter))/*cast due to constrained. prefix*/).ToString() + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CharacterPerk entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CharacterPerk(string perkID, string characterID)
	{
		PerkID = perkID;
		CharacterID = characterID;
		IsActive = false;
	}
}

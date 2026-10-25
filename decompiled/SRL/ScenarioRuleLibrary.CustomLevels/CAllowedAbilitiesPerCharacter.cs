using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CAllowedAbilitiesPerCharacter : ISerializable
{
	public string CharacterID { get; private set; }

	public List<string> Abilities { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Abilities", Abilities);
	}

	public CAllowedAbilitiesPerCharacter(SerializationInfo info, StreamingContext context)
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
				case "Abilities":
					Abilities = (List<string>)info.GetValue("Abilities", typeof(List<string>));
					break;
				case "Character":
					CharacterID = info.GetString("Character").Replace(" ", string.Empty) + "ID";
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CAllowedAbilitiesPerCharacter entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAllowedAbilitiesPerCharacter(string characterID, List<string> abilities)
	{
		CharacterID = characterID;
		Abilities = abilities;
	}
}

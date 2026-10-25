using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerRecords : ISerializable
{
	public string CharacterID { get; private set; }

	public string CharacterName { get; private set; }

	public CharacterRecordStats ScenarioBest { get; private set; }

	public CharacterRecordStats LifetimeTotal { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("CharacterName", CharacterName);
		info.AddValue("ScenarioBest", ScenarioBest);
		info.AddValue("LifetimeTotal", LifetimeTotal);
	}

	public CPlayerRecords(SerializationInfo info, StreamingContext context)
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
				case "CharacterName":
					CharacterName = info.GetString("CharacterName");
					break;
				case "ScenarioBest":
					ScenarioBest = (CharacterRecordStats)info.GetValue("ScenarioBest", typeof(CharacterRecordStats));
					break;
				case "LifetimeTotal":
					LifetimeTotal = (CharacterRecordStats)info.GetValue("LifetimeTotal", typeof(CharacterRecordStats));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerRecords entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (ScenarioBest == null)
		{
			ScenarioBest = new CharacterRecordStats();
		}
		if (LifetimeTotal == null)
		{
			LifetimeTotal = new CharacterRecordStats();
		}
	}

	public CPlayerRecords(string characterID, string characterName)
	{
		CharacterID = characterID;
		CharacterName = characterName;
		ScenarioBest = new CharacterRecordStats();
		LifetimeTotal = new CharacterRecordStats();
	}

	public void UpdatePlayerRecord(int kills, int damageDone, int damageTaken, int healingDone, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed, int rounds, bool exhausted, bool won)
	{
		ScenarioBest.UpdateCharacterRecordStats(kills, damageDone, damageTaken, healingDone, goldLooted, chestsLooted, xpEarned, itemsUsed, rounds);
		LifetimeTotal.UpdateCharacterTotalStats(kills, damageDone, damageTaken, healingDone, goldLooted, chestsLooted, xpEarned, itemsUsed, rounds, exhausted, won);
	}

	public void ScenarioReset()
	{
		ScenarioBest.ResetRecord();
	}

	public void LifetimeReset()
	{
		LifetimeTotal.ResetRecord();
	}

	public void Reset()
	{
		ScenarioReset();
		LifetimeReset();
	}
}

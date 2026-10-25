using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CharacterRecordStats : ISerializable
{
	public int Kills { get; private set; }

	public int DamageDone { get; private set; }

	public int DamageTaken { get; private set; }

	public int HealingDone { get; private set; }

	public int GoldLooted { get; private set; }

	public int ChestsLooted { get; private set; }

	public int XPEarned { get; private set; }

	public int ItemsUsed { get; private set; }

	public int RoundsPlayed { get; private set; }

	public int Exhaustions { get; private set; }

	public int ScenariosPlayed { get; private set; }

	public int ScenariosWon { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Kills", Kills);
		info.AddValue("DamageDone", DamageDone);
		info.AddValue("DamageTaken", DamageTaken);
		info.AddValue("HealingDone", HealingDone);
		info.AddValue("GoldLooted", GoldLooted);
		info.AddValue("ChestsLooted", ChestsLooted);
		info.AddValue("XPEarned", XPEarned);
		info.AddValue("ItemsUsed", ItemsUsed);
		info.AddValue("RoundsPlayed", RoundsPlayed);
		info.AddValue("Exhaustions", Exhaustions);
		info.AddValue("ScenariosPlayed", ScenariosPlayed);
		info.AddValue("ScenariosWon", ScenariosWon);
	}

	public CharacterRecordStats(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Kills":
					Kills = info.GetInt32("Kills");
					break;
				case "DamageDone":
					DamageDone = info.GetInt32("DamageDone");
					break;
				case "DamageTaken":
					DamageTaken = info.GetInt32("DamageTaken");
					break;
				case "HealingDone":
					HealingDone = info.GetInt32("HealingDone");
					break;
				case "GoldLooted":
					GoldLooted = info.GetInt32("GoldLooted");
					break;
				case "ChestsLooted":
					ChestsLooted = info.GetInt32("ChestsLooted");
					break;
				case "XPEarned":
					XPEarned = info.GetInt32("XPEarned");
					break;
				case "ItemsUsed":
					ItemsUsed = info.GetInt32("ItemsUsed");
					break;
				case "RoundsPlayed":
					RoundsPlayed = info.GetInt32("RoundsPlayed");
					break;
				case "Exhaustions":
					Exhaustions = info.GetInt32("Exhaustions");
					break;
				case "ScenariosPlayed":
					ScenariosPlayed = info.GetInt32("ScenariosPlayed");
					break;
				case "ScenariosWon":
					ScenariosWon = info.GetInt32("ScenariosWon");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CharacterRecordStats entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (RoundsPlayed == ItemsUsed && RoundsPlayed == Exhaustions)
		{
			RoundsPlayed = 0;
			Exhaustions = 0;
			ScenariosPlayed = 0;
			ScenariosWon = 0;
		}
	}

	public CharacterRecordStats()
	{
	}

	public CharacterRecordStats(int kills, int damageDone, int damageTaken, int healingDone, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed)
	{
		Kills = kills;
		DamageDone = damageDone;
		DamageTaken = damageTaken;
		HealingDone = healingDone;
		GoldLooted = goldLooted;
		ChestsLooted = chestsLooted;
		XPEarned = xpEarned;
		ItemsUsed = itemsUsed;
	}

	public void UpdateCharacterRecordStats(int kills, int damageDone, int damageTaken, int healingDone, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed, int rounds)
	{
		Kills = Math.Max(kills, Kills);
		DamageDone = Math.Max(damageDone, DamageDone);
		DamageTaken = Math.Max(damageTaken, DamageTaken);
		HealingDone = Math.Max(healingDone, HealingDone);
		GoldLooted = Math.Max(goldLooted, GoldLooted);
		ChestsLooted = Math.Max(chestsLooted, ChestsLooted);
		XPEarned = Math.Max(xpEarned, XPEarned);
		ItemsUsed = Math.Max(itemsUsed, ItemsUsed);
		RoundsPlayed = Math.Max(rounds, RoundsPlayed);
	}

	public void UpdateCharacterTotalStats(int kills, int damageDone, int damageTaken, int healingDone, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed, int rounds, bool exhausted, bool won)
	{
		Kills += kills;
		DamageDone += damageDone;
		DamageTaken += damageTaken;
		HealingDone += healingDone;
		GoldLooted += goldLooted;
		ChestsLooted += chestsLooted;
		XPEarned += xpEarned;
		ItemsUsed += itemsUsed;
		RoundsPlayed += rounds;
		ScenariosPlayed++;
		ScenariosWon += (won ? 1 : 0);
		Exhaustions += (exhausted ? 1 : 0);
	}

	public void ResetRecord()
	{
		Kills = 0;
		DamageDone = 0;
		DamageTaken = 0;
		HealingDone = 0;
		GoldLooted = 0;
		ChestsLooted = 0;
		XPEarned = 0;
		ItemsUsed = 0;
		RoundsPlayed = 0;
		ScenariosPlayed = 0;
		ScenariosWon = 0;
		Exhaustions = 0;
	}
}

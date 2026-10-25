using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsHeroSummon : CPlayerStatsBase, ISerializable
{
	public string GUID;

	public string ClassID;

	public int RangeToSummoner;

	public bool IsDead;

	public CActor.ECauseOfDeath SummonerCauseOfDeath;

	public int RoundSpawned;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("GUID", GUID);
		info.AddValue("ClassID", ClassID);
		info.AddValue("RangeToSummoner", RangeToSummoner);
		info.AddValue("IsDead", IsDead);
		info.AddValue("SummonerCauseOfDeath", SummonerCauseOfDeath);
		info.AddValue("RoundSpawned", RoundSpawned);
	}

	public CPlayerStatsHeroSummon(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "SummonerCauseOfDeath":
					SummonerCauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("SummonerCauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "GUID":
					GUID = info.GetString("GUID");
					break;
				case "ClassID":
					ClassID = info.GetString("ClassID");
					break;
				case "RangeToSummoner":
					RangeToSummoner = info.GetInt32("RangeToSummoner");
					break;
				case "IsDead":
					IsDead = info.GetBoolean("IsDead");
					break;
				case "RoundSpawned":
					RoundSpawned = info.GetInt32("RoundSpawned");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsHeal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsHeroSummon(string guid, string classID, int rangeToSummoner, bool isDead, CActor.ECauseOfDeath summonerCauseOfDeath, int roundSpawned, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		GUID = guid;
		ClassID = classID;
		RangeToSummoner = rangeToSummoner;
		IsDead = isDead;
		SummonerCauseOfDeath = summonerCauseOfDeath;
		RoundSpawned = roundSpawned;
	}
}

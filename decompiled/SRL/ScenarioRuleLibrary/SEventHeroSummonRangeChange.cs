using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventHeroSummonRangeChange : SEventActor
{
	public int RangeToSummoner;

	public CActor.ECauseOfDeath SummonerCauseOfDeath;

	public int RoundSpawned;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("RangeToSummoner", RangeToSummoner);
		info.AddValue("SummonerCauseOfDeath", SummonerCauseOfDeath);
		info.AddValue("RoundSpawned", RoundSpawned);
	}

	public SEventHeroSummonRangeChange(SerializationInfo info, StreamingContext context)
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
				case "RangeToSummoner":
					RangeToSummoner = info.GetInt32("RangeToSummoner");
					break;
				case "RoundSpawned":
					RoundSpawned = info.GetInt32("RoundSpawned");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventHeroSummonRangeChange entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventHeroSummonRangeChange(int rangeToSummoner, CActor.ECauseOfDeath summonerCauseOfDeath, int roundSpawned, SEventActorBaseData baseData)
		: base(baseData)
	{
		RangeToSummoner = rangeToSummoner;
		SummonerCauseOfDeath = summonerCauseOfDeath;
		RoundSpawned = roundSpawned;
	}
}

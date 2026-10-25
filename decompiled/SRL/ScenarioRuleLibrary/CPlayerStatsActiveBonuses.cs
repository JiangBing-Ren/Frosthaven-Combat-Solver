using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsActiveBonuses : CPlayerStatsBase, ISerializable
{
	public List<SEventActiveBonus.SEventActiveBonusData> ActiveBonuses = new List<SEventActiveBonus.SEventActiveBonusData>();

	public CActor.ECauseOfDeath CauseOfDeath { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActiveBonuses", ActiveBonuses);
		info.AddValue("CauseOfDeath", CauseOfDeath);
	}

	public CPlayerStatsActiveBonuses(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "ActiveBonuses"))
				{
					if (name == "CauseOfDeath")
					{
						CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					}
				}
				else
				{
					ActiveBonuses = (List<SEventActiveBonus.SEventActiveBonusData>)info.GetValue("ActiveBonuses", typeof(List<SEventActiveBonus.SEventActiveBonusData>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsHeal entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (ActiveBonuses == null)
		{
			ActiveBonuses = new List<SEventActiveBonus.SEventActiveBonusData>();
		}
	}

	public CPlayerStatsActiveBonuses(List<SEventActiveBonus.SEventActiveBonusData> activeBonuses, CActor.ECauseOfDeath causeOfDeath, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		ActiveBonuses = activeBonuses;
		CauseOfDeath = causeOfDeath;
	}
}

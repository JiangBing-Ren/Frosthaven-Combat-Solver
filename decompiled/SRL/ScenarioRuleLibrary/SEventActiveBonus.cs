using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventActiveBonus : SEventActor
{
	[Serializable]
	public class SEventActiveBonusData
	{
		public int EXPTrackerIndex { get; private set; }

		public int EXPTrackerCount { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("EXPTrackerIndex", EXPTrackerIndex);
			info.AddValue("EXPTrackerCount", EXPTrackerCount);
		}

		public SEventActiveBonusData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					string name = item.Name;
					if (!(name == "EXPTrackerIndex"))
					{
						if (name == "EXPTrackerCount")
						{
							EXPTrackerCount = info.GetInt32("EXPTrackerCount");
						}
					}
					else
					{
						EXPTrackerIndex = info.GetInt32("EXPTrackerIndex");
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize SEventActiveBonusData entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventActiveBonusData(int eXPTrackerIndex, int eXPTrackerCount)
		{
			EXPTrackerIndex = eXPTrackerIndex;
			EXPTrackerCount = eXPTrackerCount;
		}
	}

	public List<SEventActiveBonusData> ActiveBonuses = new List<SEventActiveBonusData>();

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActiveBonuses", ActiveBonuses);
	}

	public SEventActiveBonus(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "ActiveBonuses")
				{
					ActiveBonuses = (List<SEventActiveBonusData>)info.GetValue("ActiveBonuses", typeof(List<SEventActiveBonusData>));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventEnhancement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (ActiveBonuses == null)
		{
			ActiveBonuses = new List<SEventActiveBonusData>();
		}
	}

	public SEventActiveBonus(CPlayerActor player, SEventActorBaseData baseData, List<CActiveBonus> cachedActiveBonuses)
		: base(baseData)
	{
		foreach (CActiveBonus item in cachedActiveBonuses ?? player.ActiveBonuses.Where((CActiveBonus x) => x.BaseCard.CardType == CBaseCard.ECardType.CharacterAbility))
		{
			List<int> tracker = item.Ability.ActiveBonusData.Tracker;
			if (tracker != null && tracker.Count != 0)
			{
				ActiveBonuses.Add(new SEventActiveBonusData(item.TrackerIndex, tracker.Count));
			}
		}
	}
}

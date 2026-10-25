using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsBase : ISerializable, IComparable<CPlayerStatsBase>
{
	[Serializable]
	public class CPlayerSatsBaseData : ISerializable
	{
		public string QuestType { get; private set; } = "";

		public int Round { get; private set; }

		public int EventOrder { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (QuestType != "")
			{
				info.AddValue("QuestType", QuestType);
			}
			info.AddValue("Round", Round);
			info.AddValue("EventOrder", EventOrder);
		}

		public CPlayerSatsBaseData(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "QuestType":
						QuestType = info.GetString("QuestType");
						break;
					case "Round":
						Round = info.GetInt32("Round");
						break;
					case "EventOrder":
						EventOrder = info.GetInt32("EventOrder");
						break;
					}
				}
				catch (Exception ex)
				{
					Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsBase entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public CPlayerSatsBaseData(string advGuid, string sceGuid, string questType, int round, int eventOrder)
		{
			QuestType = questType;
			Round = round;
			EventOrder = eventOrder;
		}
	}

	public CPlayerSatsBaseData BaseData { get; private set; }

	public string QuestType => BaseData.QuestType;

	public int Round => BaseData.Round;

	private int EventOrder => BaseData.EventOrder;

	public int CompareTo(CPlayerStatsBase other)
	{
		int num = CompareTiming(other);
		if (num != 0)
		{
			return num;
		}
		return GetHashCode().CompareTo(other.GetHashCode());
	}

	public int CompareTiming(CPlayerStatsBase other)
	{
		if (other == null)
		{
			return 0;
		}
		if (Round != other.Round)
		{
			return Round.CompareTo(other.Round);
		}
		return EventOrder.CompareTo(other.EventOrder);
	}

	public bool HappenedBefore(CPlayerStatsBase other)
	{
		return CompareTiming(other) < 0;
	}

	public bool HappenedSameTime(CPlayerStatsBase other)
	{
		return CompareTiming(other) == 0;
	}

	public bool HappenedAfter(CPlayerStatsBase other)
	{
		return CompareTiming(other) > 0;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("BaseData", BaseData);
	}

	public CPlayerStatsBase(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				if (item.Name == "BaseData")
				{
					BaseData = (CPlayerSatsBaseData)info.GetValue("BaseData", typeof(CPlayerSatsBaseData));
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsBase entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsBase(CPlayerSatsBaseData baseData)
	{
		BaseData = baseData;
	}
}

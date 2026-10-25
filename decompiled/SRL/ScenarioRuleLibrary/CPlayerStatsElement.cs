using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsElement : CPlayerStatsBase, ISerializable
{
	[Serializable]
	public enum EStatsElementType
	{
		None,
		Infused,
		Consumed
	}

	public string ActorClassID { get; private set; }

	public EStatsElementType StatsElementType { get; private set; }

	public ElementInfusionBoardManager.EElement Element { get; private set; }

	public string ActingType { get; private set; }

	public List<ElementInfusionBoardManager.EElement> Infused { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("StatsElementType", StatsElementType);
		info.AddValue("Element", Element);
		info.AddValue("ActingType", ActingType);
		info.AddValue("Infused", Infused);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
	}

	public CPlayerStatsElement(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "StatsElementType":
					StatsElementType = (EStatsElementType)info.GetValue("StatsElementType", typeof(EStatsElementType));
					break;
				case "Element":
					Element = (ElementInfusionBoardManager.EElement)info.GetValue("Element", typeof(ElementInfusionBoardManager.EElement));
					break;
				case "ActingType":
					ActingType = info.GetString("ActingType");
					break;
				case "Infused":
					Infused = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Infused", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActorClass":
				{
					string text = info.GetString("ActorClass").Replace(" ", string.Empty);
					ActorClassID = text + "ID";
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CPlayerStatsElement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsElement(string actorClassID, EStatsElementType statsElementType, ElementInfusionBoardManager.EElement element, string actingType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, CPlayerSatsBaseData baseData)
		: base(baseData)
	{
		ActorClassID = actorClassID;
		StatsElementType = statsElementType;
		Element = element;
		ActingType = actingType;
		Infused = infused;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
	}

	public static CPlayerStatsElement CreateElementsStat(SEventElement sevent, string advGuid, string sceGuid, string currentQuestType, EStatsElementType statElementType, string actingType = null)
	{
		return new CPlayerStatsElement(sevent.ActorClassID, statElementType, sevent.Element, (actingType != null) ? actingType : sevent.ActorType.ToString(), sevent.Elements, sevent.PositiveConditions, sevent.NegativeConditions, new CPlayerSatsBaseData(advGuid, sceGuid, currentQuestType, sevent.Round, sevent.EventOrder));
	}
}

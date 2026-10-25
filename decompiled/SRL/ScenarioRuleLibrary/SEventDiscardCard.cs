using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventDiscardCard : SEvent
{
	public string CharacterID { get; private set; }

	public int CardID { get; private set; }

	public CBaseCard.ECardPile Pile { get; private set; }

	public DateTime TimeStamp { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("CardID", CardID);
		info.AddValue("Pile", Pile);
		info.AddValue("TimeStamp", TimeStamp);
	}

	public SEventDiscardCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
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
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "Pile":
					Pile = (CBaseCard.ECardPile)info.GetValue("Pile", typeof(CBaseCard.ECardPile));
					break;
				case "TimeStamp":
					TimeStamp = (DateTime)info.GetValue("TimeStamp", typeof(DateTime));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventDiscardCard entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventDiscardCard(string characterID, int card, CBaseCard.ECardPile pile, string text = "")
		: base(ESEType.DiscardCard, text)
	{
		CharacterID = characterID;
		CardID = card;
		Pile = pile;
		TimeStamp = DateTime.Now;
	}
}

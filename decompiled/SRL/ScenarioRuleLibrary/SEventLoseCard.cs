using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventLoseCard : SEvent
{
	public string CharacterID { get; private set; }

	public int Amount { get; private set; }

	public CAbility.EAbilityType AbilityType { get; private set; }

	public int CardID { get; private set; }

	public string CardPile { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterID", CharacterID);
		info.AddValue("Amount", Amount);
		info.AddValue("AbilityType", AbilityType);
		info.AddValue("CardID", CardID);
		info.AddValue("CardPile", CardPile);
	}

	public SEventLoseCard(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "AbilityType":
					AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
					break;
				case "CharacterID":
					CharacterID = info.GetString("CharacterID");
					break;
				case "Amount":
					Amount = info.GetInt32("Amount");
					break;
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "CardPile":
					CardPile = info.GetString("CardPile");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventEnhancement entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventLoseCard(string characterID, int amount, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, int cardID = -1, string cardPile = null, string text = "")
		: base(ESEType.LoseCard, text)
	{
		CharacterID = characterID;
		Amount = amount;
		AbilityType = abilityType;
		CardID = cardID;
		CardPile = cardPile;
	}
}

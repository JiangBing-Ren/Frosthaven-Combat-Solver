using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class CardsToken : StatePropertyToken
{
	private int cardCount;

	public int[] CardIDs { get; set; }

	public int CardType { get; set; }

	public CBaseCard.ECardType CardTypeEnum => (CBaseCard.ECardType)CardType;

	public CardsToken(List<int> cardIDs)
	{
		if (cardIDs.IsNullOrEmpty())
		{
			CardIDs = new int[0];
		}
		else
		{
			CardIDs = new int[cardIDs.Count];
			for (int i = 0; i < CardIDs.Length; i++)
			{
				CardIDs[i] = cardIDs[i];
			}
		}
		cardCount = CardIDs.Length;
		CardType = 1;
	}

	public CardsToken(List<AbilityCardUI> cards)
	{
		if (cards.IsNullOrEmpty())
		{
			CardIDs = new int[0];
		}
		else
		{
			CardIDs = new int[cards.Count];
			for (int i = 0; i < CardIDs.Length; i++)
			{
				CardIDs[i] = ((!cards[i].IsLongRest) ? cards[i].AbilityCard.CardInstanceID : 0);
			}
		}
		cardCount = CardIDs.Length;
		CardType = 1;
	}

	public CardsToken(List<CAbilityCard> cards)
	{
		if (cards.IsNullOrEmpty())
		{
			CardIDs = new int[0];
		}
		else
		{
			CardIDs = new int[cards.Count];
			for (int i = 0; i < CardIDs.Length; i++)
			{
				CardIDs[i] = cards[i].CardInstanceID;
			}
		}
		cardCount = CardIDs.Length;
		CardType = 1;
	}

	public CardsToken(List<int> cardIDs, CBaseCard.ECardType cardType)
	{
		if (cardIDs.IsNullOrEmpty())
		{
			CardIDs = new int[0];
		}
		else
		{
			CardIDs = cardIDs.ToArray();
		}
		cardCount = CardIDs.Length;
		CardType = (int)cardType;
	}

	public CardsToken(CBaseCard baseCard)
	{
		cardCount = 1;
		CardIDs = new int[cardCount];
		CardIDs[0] = baseCard.ID;
		CardType = (int)baseCard.CardType;
	}

	public CardsToken(CAbilityCard card)
	{
		cardCount = 1;
		CardIDs = new int[cardCount];
		CardIDs[0] = card.CardInstanceID;
		CardType = 1;
	}

	public CardsToken()
	{
		cardCount = 0;
		CardIDs = new int[cardCount];
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(cardCount);
		for (int i = 0; i < cardCount; i++)
		{
			packet.WriteInt(CardIDs[i]);
		}
		packet.WriteInt(CardType);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		cardCount = packet.ReadInt();
		CardIDs = new int[cardCount];
		for (int i = 0; i < cardCount; i++)
		{
			CardIDs[i] = packet.ReadInt();
		}
		CardType = packet.ReadInt();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Card IDs: " + CardIDs.ToStringPretty());
	}
}

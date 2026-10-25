using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class StartRoundCardsToken : StatePropertyToken
{
	private int roundCardCount;

	private int handCardCount;

	private int discardedCardCount;

	private int lostCardCount;

	private int permaLostCardCount;

	private int takenCardCount;

	private int ownedUnusedCardCount;

	private int activatedCardCount;

	private int excludedFromShortRestAbilityCardCount;

	public string FastReloadID;

	public int[] RoundCardIDs { get; set; }

	public int[] HandCardIDs { get; set; }

	public int[] DiscardedCardIDs { get; set; }

	public int[] LostCardIDs { get; set; }

	public int[] PermaLostCardIDs { get; set; }

	public int[] TakenCardIDs { get; set; }

	public int[] OwnedUnusedCardIDs { get; set; }

	public int[] ActivatedCardIDs { get; set; }

	public int[] ExcludedFromShortRestAbilityCardIDs { get; set; }

	public int InitiativeCardID { get; set; }

	public int SubInitiativeCardID { get; set; }

	public bool HasShortRested { get; set; }

	public bool HasImprovedShortRested { get; set; }

	public int CardBurnedID { get; set; }

	public bool ShortRestCardRedrawn { get; set; }

	public bool ExcludedCardsFromShortRest { get; set; }

	public bool ImprovedShortRest { get; set; }

	public bool LongRestSelected { get; set; }

	public bool ShortRestSelected { get; set; }

	public StartRoundCardsToken(StartRoundCardState startRoundCardState)
	{
		roundCardCount = startRoundCardState.RoundAbilityCardInstanceIDs.Count;
		handCardCount = startRoundCardState.HandAbilityCardInstanceIDs.Count;
		discardedCardCount = startRoundCardState.DiscardedAbilityCardInstanceIDs.Count;
		lostCardCount = startRoundCardState.LostAbilityCardInstanceIDs.Count;
		permaLostCardCount = startRoundCardState.PermaLostAbilityCardInstanceIDs.Count;
		takenCardCount = startRoundCardState.TakenAbilityCardInstanceIDs.Count;
		ownedUnusedCardCount = startRoundCardState.OwnedUnusedAbilityCardInstanceIDs.Count;
		activatedCardCount = startRoundCardState.ActivatedCardIDs.Count;
		excludedFromShortRestAbilityCardCount = startRoundCardState.ExcludedFromShortRestAbilityCardIDs.Count;
		RoundCardIDs = new int[roundCardCount];
		HandCardIDs = new int[handCardCount];
		DiscardedCardIDs = new int[discardedCardCount];
		LostCardIDs = new int[lostCardCount];
		PermaLostCardIDs = new int[permaLostCardCount];
		TakenCardIDs = new int[takenCardCount];
		OwnedUnusedCardIDs = new int[ownedUnusedCardCount];
		ActivatedCardIDs = new int[activatedCardCount];
		ExcludedFromShortRestAbilityCardIDs = new int[excludedFromShortRestAbilityCardCount];
		for (int i = 0; i < roundCardCount; i++)
		{
			RoundCardIDs[i] = startRoundCardState.RoundAbilityCardInstanceIDs[i];
		}
		for (int j = 0; j < handCardCount; j++)
		{
			HandCardIDs[j] = startRoundCardState.HandAbilityCardInstanceIDs[j];
		}
		for (int k = 0; k < discardedCardCount; k++)
		{
			DiscardedCardIDs[k] = startRoundCardState.DiscardedAbilityCardInstanceIDs[k];
		}
		for (int l = 0; l < lostCardCount; l++)
		{
			LostCardIDs[l] = startRoundCardState.LostAbilityCardInstanceIDs[l];
		}
		for (int m = 0; m < permaLostCardCount; m++)
		{
			PermaLostCardIDs[m] = startRoundCardState.PermaLostAbilityCardInstanceIDs[m];
		}
		for (int n = 0; n < takenCardCount; n++)
		{
			TakenCardIDs[n] = startRoundCardState.TakenAbilityCardInstanceIDs[n];
		}
		for (int num = 0; num < ownedUnusedCardCount; num++)
		{
			OwnedUnusedCardIDs[num] = startRoundCardState.OwnedUnusedAbilityCardInstanceIDs[num];
		}
		for (int num2 = 0; num2 < activatedCardCount; num2++)
		{
			ActivatedCardIDs[num2] = startRoundCardState.ActivatedCardIDs[num2];
		}
		for (int num3 = 0; num3 < excludedFromShortRestAbilityCardCount; num3++)
		{
			ExcludedFromShortRestAbilityCardIDs[num3] = startRoundCardState.ExcludedFromShortRestAbilityCardIDs[num3];
		}
		InitiativeCardID = startRoundCardState.InitiativeCardID;
		SubInitiativeCardID = startRoundCardState.SubInitiativeCardID;
		HasShortRested = startRoundCardState.HasShortRested;
		HasImprovedShortRested = startRoundCardState.HasImprovedShortRested;
		CardBurnedID = startRoundCardState.ShortRestCardBurnedID;
		ShortRestCardRedrawn = startRoundCardState.ShortRestCardRedrawn;
		ExcludedCardsFromShortRest = startRoundCardState.ExcludedCardsFromShortRest;
		ImprovedShortRest = startRoundCardState.ImprovedShortRest;
		LongRestSelected = startRoundCardState.LongRestSelected;
		ShortRestSelected = startRoundCardState.ShortRestSelected;
		FastReloadID = startRoundCardState.FastReloadID;
	}

	public StartRoundCardsToken()
	{
		roundCardCount = 0;
		handCardCount = 0;
		discardedCardCount = 0;
		lostCardCount = 0;
		permaLostCardCount = 0;
		takenCardCount = 0;
		ownedUnusedCardCount = 0;
		activatedCardCount = 0;
		excludedFromShortRestAbilityCardCount = 0;
		RoundCardIDs = new int[roundCardCount];
		HandCardIDs = new int[handCardCount];
		DiscardedCardIDs = new int[discardedCardCount];
		LostCardIDs = new int[lostCardCount];
		PermaLostCardIDs = new int[permaLostCardCount];
		TakenCardIDs = new int[takenCardCount];
		OwnedUnusedCardIDs = new int[ownedUnusedCardCount];
		ActivatedCardIDs = new int[activatedCardCount];
		ExcludedFromShortRestAbilityCardIDs = new int[excludedFromShortRestAbilityCardCount];
		InitiativeCardID = -1;
		SubInitiativeCardID = -1;
		HasShortRested = false;
		HasImprovedShortRested = false;
		CardBurnedID = -1;
		ShortRestCardRedrawn = false;
		ExcludedCardsFromShortRest = false;
		ImprovedShortRest = false;
		LongRestSelected = false;
		ShortRestSelected = false;
		FastReloadID = null;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(roundCardCount);
		packet.WriteInt(handCardCount);
		packet.WriteInt(discardedCardCount);
		packet.WriteInt(lostCardCount);
		packet.WriteInt(permaLostCardCount);
		packet.WriteInt(takenCardCount);
		packet.WriteInt(ownedUnusedCardCount);
		packet.WriteInt(activatedCardCount);
		packet.WriteInt(excludedFromShortRestAbilityCardCount);
		packet.WriteInt(permaLostCardCount);
		packet.WriteInt(takenCardCount);
		packet.WriteInt(activatedCardCount);
		packet.WriteInt(excludedFromShortRestAbilityCardCount);
		for (int i = 0; i < roundCardCount; i++)
		{
			packet.WriteInt(RoundCardIDs[i]);
		}
		for (int j = 0; j < handCardCount; j++)
		{
			packet.WriteInt(HandCardIDs[j]);
		}
		for (int k = 0; k < discardedCardCount; k++)
		{
			packet.WriteInt(DiscardedCardIDs[k]);
		}
		for (int l = 0; l < lostCardCount; l++)
		{
			packet.WriteInt(LostCardIDs[l]);
		}
		for (int m = 0; m < permaLostCardCount; m++)
		{
			packet.WriteInt(PermaLostCardIDs[m]);
		}
		for (int n = 0; n < takenCardCount; n++)
		{
			packet.WriteInt(TakenCardIDs[n]);
		}
		for (int num = 0; num < ownedUnusedCardCount; num++)
		{
			packet.WriteInt(OwnedUnusedCardIDs[num]);
		}
		for (int num2 = 0; num2 < activatedCardCount; num2++)
		{
			packet.WriteInt(ActivatedCardIDs[num2]);
		}
		for (int num3 = 0; num3 < excludedFromShortRestAbilityCardCount; num3++)
		{
			packet.WriteInt(ExcludedFromShortRestAbilityCardIDs[num3]);
		}
		packet.WriteInt(InitiativeCardID);
		packet.WriteInt(SubInitiativeCardID);
		packet.WriteBool(HasShortRested);
		packet.WriteBool(HasImprovedShortRested);
		packet.WriteBool(ExcludedCardsFromShortRest);
		packet.WriteInt(CardBurnedID);
		packet.WriteBool(ShortRestCardRedrawn);
		packet.WriteBool(ImprovedShortRest);
		packet.WriteBool(LongRestSelected);
		packet.WriteBool(ShortRestSelected);
		packet.WriteString(FastReloadID);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		roundCardCount = packet.ReadInt();
		handCardCount = packet.ReadInt();
		discardedCardCount = packet.ReadInt();
		lostCardCount = packet.ReadInt();
		permaLostCardCount = packet.ReadInt();
		takenCardCount = packet.ReadInt();
		ownedUnusedCardCount = packet.ReadInt();
		activatedCardCount = packet.ReadInt();
		excludedFromShortRestAbilityCardCount = packet.ReadInt();
		RoundCardIDs = new int[roundCardCount];
		permaLostCardCount = packet.ReadInt();
		takenCardCount = packet.ReadInt();
		activatedCardCount = packet.ReadInt();
		excludedFromShortRestAbilityCardCount = packet.ReadInt();
		RoundCardIDs = new int[roundCardCount];
		for (int i = 0; i < roundCardCount; i++)
		{
			RoundCardIDs[i] = packet.ReadInt();
		}
		HandCardIDs = new int[handCardCount];
		for (int j = 0; j < handCardCount; j++)
		{
			HandCardIDs[j] = packet.ReadInt();
		}
		DiscardedCardIDs = new int[discardedCardCount];
		for (int k = 0; k < discardedCardCount; k++)
		{
			DiscardedCardIDs[k] = packet.ReadInt();
		}
		LostCardIDs = new int[lostCardCount];
		for (int l = 0; l < lostCardCount; l++)
		{
			LostCardIDs[l] = packet.ReadInt();
		}
		PermaLostCardIDs = new int[permaLostCardCount];
		for (int m = 0; m < permaLostCardCount; m++)
		{
			PermaLostCardIDs[m] = packet.ReadInt();
		}
		TakenCardIDs = new int[takenCardCount];
		for (int n = 0; n < takenCardCount; n++)
		{
			TakenCardIDs[n] = packet.ReadInt();
		}
		OwnedUnusedCardIDs = new int[ownedUnusedCardCount];
		for (int num = 0; num < ownedUnusedCardCount; num++)
		{
			OwnedUnusedCardIDs[num] = packet.ReadInt();
		}
		ActivatedCardIDs = new int[activatedCardCount];
		for (int num2 = 0; num2 < activatedCardCount; num2++)
		{
			ActivatedCardIDs[num2] = packet.ReadInt();
		}
		ExcludedFromShortRestAbilityCardIDs = new int[excludedFromShortRestAbilityCardCount];
		for (int num3 = 0; num3 < excludedFromShortRestAbilityCardCount; num3++)
		{
			ExcludedFromShortRestAbilityCardIDs[num3] = packet.ReadInt();
		}
		InitiativeCardID = packet.ReadInt();
		SubInitiativeCardID = packet.ReadInt();
		HasShortRested = packet.ReadBool();
		HasImprovedShortRested = packet.ReadBool();
		ExcludedCardsFromShortRest = packet.ReadBool();
		CardBurnedID = packet.ReadInt();
		ShortRestCardRedrawn = packet.ReadBool();
		ImprovedShortRest = packet.ReadBool();
		LongRestSelected = packet.ReadBool();
		ShortRestSelected = packet.ReadBool();
		FastReloadID = packet.ReadString();
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "Round Card IDs: " + RoundCardIDs.ToStringPretty() + "Hand Card IDs: " + HandCardIDs.ToStringPretty() + "Discarded Card IDs: " + DiscardedCardIDs.ToStringPretty() + "Lost Card IDs: " + LostCardIDs.ToStringPretty() + "Perma Lost Card IDs: " + PermaLostCardIDs.ToStringPretty() + "Taken Card IDs: " + TakenCardIDs.ToStringPretty() + "Own Unused Card IDs" + OwnedUnusedCardIDs.ToStringPretty() + "Activated Card IDs: " + ActivatedCardIDs.ToStringPretty() + "Excluded From Short Rest Card IDs: " + ExcludedFromShortRestAbilityCardIDs.ToStringPretty() + "Initiative Card ID: " + InitiativeCardID + "Sub Initiative Card ID: " + SubInitiativeCardID + "Has Short Rested: " + HasShortRested + "Has Short Rested: " + HasImprovedShortRested + "Card Burned ID: " + CardBurnedID + "Short Rest Card Redrawn: " + ShortRestCardRedrawn + "Excluded Cards From Short Rest: " + ExcludedCardsFromShortRest + "Improved Short Rest: " + ImprovedShortRest + "Long Rest Selected: " + LongRestSelected + "Short Rest Selected: " + ShortRestSelected + "Fast reload id: " + FastReloadID);
	}

	public StartRoundCardState GetStartRoundCardState()
	{
		return new StartRoundCardState(RoundCardIDs.ToList(), HandCardIDs.ToList(), DiscardedCardIDs.ToList(), LostCardIDs.ToList(), PermaLostCardIDs.ToList(), TakenCardIDs.ToList(), OwnedUnusedCardIDs.ToList(), ActivatedCardIDs.ToList(), ExcludedFromShortRestAbilityCardIDs.ToList(), InitiativeCardID, SubInitiativeCardID, HasShortRested, HasImprovedShortRested, CardBurnedID, ImprovedShortRest, ShortRestCardRedrawn, ExcludedCardsFromShortRest, LongRestSelected, ShortRestSelected, FastReload.CurrentHistoryID);
	}
}

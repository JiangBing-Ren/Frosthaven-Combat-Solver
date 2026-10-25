using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class StartRoundCardState
{
	public List<int> RoundAbilityCardInstanceIDs;

	public List<int> HandAbilityCardInstanceIDs;

	public List<int> DiscardedAbilityCardInstanceIDs;

	public List<int> LostAbilityCardInstanceIDs;

	public List<int> PermaLostAbilityCardInstanceIDs;

	public List<int> TakenAbilityCardInstanceIDs;

	public List<int> OwnedUnusedAbilityCardInstanceIDs;

	public List<int> ActivatedCardIDs;

	public List<int> ExcludedFromShortRestAbilityCardIDs;

	public int InitiativeCardID;

	public int SubInitiativeCardID;

	public bool HasShortRested;

	public bool HasImprovedShortRested;

	public int ShortRestCardBurnedID;

	public bool ImprovedShortRest;

	public bool ShortRestCardRedrawn;

	public bool ExcludedCardsFromShortRest;

	public bool LongRestSelected;

	public bool ShortRestSelected;

	public string FastReloadID;

	public StartRoundCardState(List<CAbilityCard> roundAbilityCards, List<CAbilityCard> handAbilityCards, List<CAbilityCard> discardedAbilityCards, List<CAbilityCard> lostAbilityCards, List<CAbilityCard> permaLostAbilityCards, List<CAbilityCard> takenAbilityCards, List<CAbilityCard> ownedUnusedAbilityCards, List<CBaseCard> activatedCards, List<CAbilityCard> excludedFromShortRestAbilityCardIDs, CAbilityCard initiativeCard, CAbilityCard subInitiativeCard, bool hasShortRested, bool hasImprovedShortRested, int shortRestCardBurned, bool improvedShortRest, bool shortRestCardRedrawn, bool excludedCardsFromShortRest, bool longRestSelected, bool shortRestSelected, string fastReloadID)
	{
		RoundAbilityCardInstanceIDs = roundAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		HandAbilityCardInstanceIDs = handAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		DiscardedAbilityCardInstanceIDs = discardedAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		LostAbilityCardInstanceIDs = lostAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		PermaLostAbilityCardInstanceIDs = permaLostAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		TakenAbilityCardInstanceIDs = takenAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		OwnedUnusedAbilityCardInstanceIDs = ownedUnusedAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		ActivatedCardIDs = activatedCards.Select((CBaseCard x) => x.ID).ToList();
		ExcludedFromShortRestAbilityCardIDs = excludedFromShortRestAbilityCardIDs.Select((CAbilityCard x) => x.ID).ToList();
		InitiativeCardID = initiativeCard?.CardInstanceID ?? (-1);
		SubInitiativeCardID = subInitiativeCard?.CardInstanceID ?? (-1);
		HasShortRested = hasShortRested;
		HasImprovedShortRested = hasImprovedShortRested;
		ShortRestCardBurnedID = shortRestCardBurned;
		ImprovedShortRest = improvedShortRest;
		ShortRestCardRedrawn = shortRestCardRedrawn;
		ExcludedCardsFromShortRest = excludedCardsFromShortRest;
		LongRestSelected = longRestSelected;
		ShortRestSelected = shortRestSelected;
		FastReloadID = fastReloadID;
	}

	public StartRoundCardState(List<int> roundAbilityCardInstanceIDs, List<int> handAbilityCardInstanceIDs, List<int> discardedAbilityCardInstanceIDs, List<int> lostAbilityCardInstanceIDs, List<int> permaLostAbilityCardInstanceIDs, List<int> takenAbilityCardInstanceIDs, List<int> ownedUnusedAbilityCardInstanceIDs, List<int> activatedAbilityCardInstanceIDs, List<int> excludedFromShortRestAbilityCardIDs, int initiativeCardID, int subInitiativeCardID, bool hasShortRested, bool hasImprovedShortRested, int shortRestCardBurnedID, bool improvedShortRest, bool shortRestCardRedrawn, bool excludedCardsFromShortRest, bool longRestSelected, bool shortRestSelected, string fastReloadID)
	{
		RoundAbilityCardInstanceIDs = roundAbilityCardInstanceIDs.ToList();
		HandAbilityCardInstanceIDs = handAbilityCardInstanceIDs.ToList();
		DiscardedAbilityCardInstanceIDs = discardedAbilityCardInstanceIDs.ToList();
		LostAbilityCardInstanceIDs = lostAbilityCardInstanceIDs.ToList();
		PermaLostAbilityCardInstanceIDs = permaLostAbilityCardInstanceIDs.ToList();
		TakenAbilityCardInstanceIDs = takenAbilityCardInstanceIDs.ToList();
		OwnedUnusedAbilityCardInstanceIDs = ownedUnusedAbilityCardInstanceIDs.ToList();
		ActivatedCardIDs = activatedAbilityCardInstanceIDs.ToList();
		ExcludedFromShortRestAbilityCardIDs = excludedFromShortRestAbilityCardIDs.ToList();
		InitiativeCardID = initiativeCardID;
		SubInitiativeCardID = subInitiativeCardID;
		HasShortRested = hasShortRested;
		HasImprovedShortRested = hasImprovedShortRested;
		ShortRestCardBurnedID = shortRestCardBurnedID;
		ImprovedShortRest = improvedShortRest;
		ShortRestCardRedrawn = shortRestCardRedrawn;
		ExcludedCardsFromShortRest = excludedCardsFromShortRest;
		LongRestSelected = longRestSelected;
		ShortRestSelected = shortRestSelected;
		FastReloadID = fastReloadID;
	}
}

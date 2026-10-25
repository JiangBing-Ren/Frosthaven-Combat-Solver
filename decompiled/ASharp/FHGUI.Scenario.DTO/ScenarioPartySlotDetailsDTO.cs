using FHGUI.Common;
using FHGUI.DTO;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario.DTO;

public class ScenarioPartySlotDetailsDTO
{
	public CPlayerActor PlayerActor;

	public PartySlotDTO PartySlotDto;

	public int HealthCurrent;

	public int HealthMax;

	public int CardsCurrent;

	public int CardsMax;

	public bool IsShortResting;

	public bool IsLongResting;

	public bool IsInCardsSelectionPhase;

	public bool IsExhausted => PlayerActor?.IsDead ?? false;

	public bool IsAvailable => PartySlotDto?.IsAvailable ?? (!IsExhausted);

	public ECharacter Character => PlayerActor.CharacterClass.CharacterModel;

	public ScenarioPartySlotDetailsDTO(CPlayerActor playerActor, int slotId, bool isInCardsSelectionPhase, bool isSelected)
	{
		PlayerActor = playerActor;
		HealthCurrent = PlayerActor.Health;
		HealthMax = PlayerActor.MaxHealth;
		bool flag = !IsExhausted;
		IsInCardsSelectionPhase = flag & isInCardsSelectionPhase;
		PartySlotDto = new PartySlotDTO
		{
			SlotId = slotId,
			Character = Character,
			ClassID = PlayerActor.CharacterClass.CharacterID,
			Name = PlayerActor.CharacterName,
			Portrait = UIInfoTools.GetHeroPortrait(PlayerActor.CharacterClass.DefaultModel),
			Color = UIInfoTools.Instance.GetCharacterColor(PlayerActor),
			MultiplayerUserData = UIPlayerCharacterUtils.GetPlayerData(PlayerActor),
			Owned = PlayerActor.IsUnderMyControl(),
			IsAvailable = !IsExhausted,
			IsSelected = isSelected
		};
		if (IsInCardsSelectionPhase)
		{
			CardsCurrent = PlayerActor.CharacterClass.RoundAbilityCards.Count;
			CardsMax = PlayerActor.MaxRoundCardsCount;
			IsShortResting = PlayerActor.CharacterClass.ShortRest;
			IsLongResting = PlayerActor.IsLongRestSelected;
		}
	}
}

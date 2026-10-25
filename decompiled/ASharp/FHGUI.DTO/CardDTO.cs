using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using ScenarioRuleLibrary;

namespace FHGUI.DTO;

public class CardDTO
{
	public int Id;

	public int Initiative;

	public int AltInitiative;

	public string NameLocKey;

	public int Level;

	public bool IsDisabled;

	public bool IsInHand;

	public bool IsAltColor;

	public bool IsSelectable;

	public ECharacter CharacterID;

	public CharacterID Character;

	public CEnhancement[] Enhancements;

	public bool HasAltInitiative => AltInitiative != -1;

	public CardDTO()
	{
	}

	public CardDTO(ClassCardDef cardDef, bool isInHand, CharacterID characterID = null, IEnumerable<CEnhancement> enhancements = null, bool isSelectable = true, bool isDisabled = false)
	{
		Id = (int)cardDef.ID.ID;
		Initiative = (int)cardDef.Initiative;
		NameLocKey = cardDef.NameLocKey;
		Level = (int)cardDef.Level;
		IsDisabled = isDisabled;
		IsInHand = isInHand;
		IsAltColor = CrossReference.ClassIdToECharacterModel(cardDef.ClassID.ID) == ECharacter.Geminate && !cardDef.IsGeminateMeleeCard;
		IsSelectable = isSelectable;
		CharacterID = CrossReference.ClassIdToECharacterModel(cardDef.ClassID.ID);
		AltInitiative = ((CharacterID == ECharacter.Blinkblade) ? ((int)(cardDef.Initiative + 30)) : (-1));
		Character = characterID;
		Enhancements = enhancements?.ToArray() ?? Array.Empty<CEnhancement>();
	}

	public CardDTO(CAbilityCard cAbilityCard, bool isInHand = true, CharacterID characterID = null, IEnumerable<CEnhancement> enhancements = null, bool isSelectable = true, bool isDisabled = false)
	{
		Id = cAbilityCard.ID;
		Initiative = cAbilityCard.Initiative;
		NameLocKey = cAbilityCard.Name;
		Level = cAbilityCard.Level;
		IsDisabled = isDisabled;
		IsInHand = isInHand;
		IsAltColor = cAbilityCard.GetAbilityCardYML.RequiredForm == CPlayerActor.EForm.Melee;
		IsSelectable = isSelectable;
		CharacterID = CrossReference.ClassIdToECharacterModel(cAbilityCard.ClassID);
		AltInitiative = ((CharacterID == ECharacter.Blinkblade) ? (cAbilityCard.Initiative + 30) : (-1));
		Character = characterID;
		Enhancements = enhancements?.ToArray() ?? Array.Empty<CEnhancement>();
	}

	public CardDTO Clone()
	{
		return (CardDTO)MemberwiseClone();
	}
}

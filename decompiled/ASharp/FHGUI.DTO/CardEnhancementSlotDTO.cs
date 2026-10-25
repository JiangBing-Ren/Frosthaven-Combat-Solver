using System.Collections.Generic;
using FHRL;

namespace FHGUI.DTO;

public class CardEnhancementSlotDTO
{
	public CardEnhancementMarkType Mark;

	public CardEffectType Effect;

	public string AbilityName;

	public string AbilityType;

	public uint SlotIndex;

	public uint EnhancementLine;

	public bool IsSummonStatEffect;

	public bool InUse;

	public List<CardEnhancementDTO> Enhancements;

	public CardEnhancementSlotDTO(ClassCardDef.EnhacementSlot source)
	{
		Mark = source.Mark;
		Effect = source.Effect;
		AbilityName = source.AbilityName;
		AbilityType = source.AbilityType;
		SlotIndex = source.SlotIndex;
		EnhancementLine = source.EnhancementLine;
		IsSummonStatEffect = source.IsSummonStatEffect;
	}

	public CardEnhancementSlotDTO Clone()
	{
		return (CardEnhancementSlotDTO)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"Slot {SlotIndex}, Mark: {Mark},  Ability: {AbilityName}, Line {EnhancementLine}";
	}
}

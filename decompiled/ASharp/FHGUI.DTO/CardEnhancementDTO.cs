using FHRL;
using GLOOM;

namespace FHGUI.DTO;

public class CardEnhancementDTO
{
	public string ID;

	public string NameKey;

	public string NameLocalized;

	public CardEnhancementMarkType Mark;

	public CardEnhancementEffectType Effect;

	public CardEffectType ForEffect;

	public uint BaseGoldCost;

	public uint CurrentGoldCost;

	public bool Affordable;

	public bool Sloted;

	public CardEnhancementDTO()
	{
	}

	public CardEnhancementDTO(CardEnhancementTypeDef enhancementDef)
	{
		ID = enhancementDef.ID.ID;
		Mark = enhancementDef.Mark;
		Effect = enhancementDef.Effect;
		ForEffect = enhancementDef.ForEffect;
		BaseGoldCost = enhancementDef.Cost;
		NameKey = enhancementDef.Name;
		NameLocalized = LocalizationManager.GetTranslation(enhancementDef.Name);
	}

	public CardEnhancementDTO Clone()
	{
		return (CardEnhancementDTO)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"ID: {ID}, Name {NameKey}, Effect {Effect}";
	}
}

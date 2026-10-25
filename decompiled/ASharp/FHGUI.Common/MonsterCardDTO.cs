using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Common;

public struct MonsterCardDTO
{
	public int ID;

	public string Name;

	public int Initiative;

	public CardPileType CardPile;

	public MonsterCardDTO(CMonsterAbilityCard card, CardPileType cardPile = CardPileType.None, CEnemyActor actor = null, CMonsterClass monster = null)
	{
		ID = card.ID;
		string text = monster?.LocKey;
		if (!string.IsNullOrEmpty(actor?.CustomLocKey))
		{
			text = actor.CustomLocKey;
		}
		Name = ((text == null) ? null : LocalizationManager.GetTranslation(text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true));
		Initiative = card.Initiative;
		CardPile = cardPile;
	}

	public MonsterCardDTO(MonsterCardYMLData card, CardPileType cardPile = CardPileType.None, CEnemyActor actor = null, CMonsterClass monster = null)
	{
		ID = card.ID;
		string text = monster?.LocKey;
		if (!string.IsNullOrEmpty(actor?.CustomLocKey))
		{
			text = actor.CustomLocKey;
		}
		Name = ((text == null) ? null : LocalizationManager.GetTranslation(text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true));
		Initiative = card.Initiative;
		CardPile = cardPile;
	}
}

using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCardUIClassElementsGeminate : MonoBehaviour, IAbilityCardClassUIElements
{
	[SerializeField]
	private Image _cardImage;

	[SerializeField]
	private Sprite _cardImageSpriteMelee;

	[SerializeField]
	private Sprite _cardImageSpriteRanged;

	[SerializeField]
	private Image _miniCardImage;

	[SerializeField]
	private Sprite _miniCardImageSpriteMelee;

	[SerializeField]
	private Sprite _miniCardImageSpriteRanged;

	[SerializeField]
	private Image _actionButtons;

	[SerializeField]
	private Sprite _actionButtonsSpriteMelee;

	[SerializeField]
	private Sprite _actionButtonsSpriteRanged;

	public void SetupCharacterClassElements(string cardName, AbilityCardYMLData cardData)
	{
		_cardImage.sprite = ((cardData.RequiredForm == CPlayerActor.EForm.Melee) ? _cardImageSpriteMelee : _cardImageSpriteRanged);
		_actionButtons.sprite = ((cardData.RequiredForm == CPlayerActor.EForm.Melee) ? _actionButtonsSpriteMelee : _actionButtonsSpriteRanged);
	}

	public void UpdateCharacterClassElements(CPlayerActor cPlayerActor)
	{
	}
}

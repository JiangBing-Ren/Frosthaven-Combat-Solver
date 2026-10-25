using FHGUI.DTO;
using FHGUI.Modules;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UICompactAbilityCard : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _initiativeText;

	[SerializeField]
	private GameObject _altInitiative;

	[SerializeField]
	private TMP_Text _altInitiativeText;

	[SerializeField]
	private Image _header;

	private UICardInteractionEvents _events;

	private CardDTO _card;

	public void Init(CardDTO card, UICardInteractionEvents events)
	{
		name = $"CardSlot_ID[{card.Id}]_{card.Initiative}";
		_name.text = LocalizationManager.GetTranslation(card.NameLocKey);
		_initiativeText.text = card.Initiative.ToString();
		if (_altInitiative != null)
		{
			_altInitiative.SetActive(card.AltInitiative >= 0);
			_altInitiativeText.text = card.AltInitiative.ToString();
		}
		_header.sprite = UIInfoTools.Instance.GetCharacterCardHeader(card.CharacterID, card.Id);
		_events = events;
		_card = card;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_events?.OnHover?.Invoke(_card, isFlagRaised: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_events?.OnHover?.Invoke(_card, isFlagRaised: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_events?.OnClick?.Invoke(_card);
	}
}

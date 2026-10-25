using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;

namespace FHGUI.Modals;

public class CardInfoModal : FHMapViewModule
{
	[SerializeField]
	private TMP_Text _textInfoTitle;

	[SerializeField]
	private TMP_Text _textInfoDescription;

	[SerializeField]
	private UIOutpostCardSlot _cardSlot;

	[SerializeField]
	private ButtonsBarElement _buttonOk;

	[SerializeField]
	private ButtonsBarElement _buttonCancel;

	public void Show(string infoTitleLocalized, string infoDescriptionLocalized, CardDTO cardDto, ButtonsBarParam? okParam = null, ButtonsBarParam? cancelParam = null)
	{
		_textInfoTitle.text = infoTitleLocalized;
		_textInfoDescription.text = infoDescriptionLocalized;
		_cardSlot.Init(cardDto, isMiniCard: false, null, fitInParent: true);
		SetParam(_buttonOk, okParam);
		SetParam(_buttonCancel, cancelParam);
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void SetParam(ButtonsBarElement element, ButtonsBarParam? param)
	{
		element.gameObject.SetActive(element.TrySetParam(param));
	}
}

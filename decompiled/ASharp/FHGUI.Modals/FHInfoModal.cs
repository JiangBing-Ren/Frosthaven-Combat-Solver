using System;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modals;

public class FHInfoModal : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _textInfoTitle;

	[SerializeField]
	private TMP_Text _textInfoBody;

	[SerializeField]
	private ButtonsBarElement _buttonOk;

	[SerializeField]
	private ButtonsBarElement _buttonCancel;

	[SerializeField]
	private Button _buttonClose;

	private Action _onCloseClicked;

	private void Awake()
	{
		_buttonClose.onClick.AddListener(OnClose);
	}

	private void OnClose()
	{
		_onCloseClicked?.Invoke();
		Hide();
	}

	public void Show(string infoTitleLocalized, string infoBodyLocalized, ButtonsBarParam? okParam = null, ButtonsBarParam? cancelParam = null, Action onCloseClicked = null)
	{
		_textInfoTitle.text = infoTitleLocalized;
		_textInfoBody.text = infoBodyLocalized;
		SetParam(_buttonOk, okParam);
		SetParam(_buttonCancel, cancelParam);
		_buttonClose.gameObject.SetActive(_onCloseClicked != null);
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

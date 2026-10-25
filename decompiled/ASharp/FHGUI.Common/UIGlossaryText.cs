using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGlossaryText : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _glossaryKey;

	[SerializeField]
	private Image _glossaryIcon;

	[SerializeField]
	private RectTransform _glossaryLeftSide;

	[SerializeField]
	private TMP_Text _glossaryDescription;

	private GameObject _leftSidePreview;

	public UIGlossaryData Data { get; private set; }

	public void Init(UIGlossaryData glossary)
	{
		Clear();
		Data = glossary;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (Data.Prefab != null)
		{
			_leftSidePreview = Object.Instantiate(Data.Prefab, _glossaryLeftSide);
		}
		else if (Data.TMPKey.IsNOTNullOrEmpty())
		{
			_glossaryKey.gameObject.SetActive(value: true);
			_glossaryKey.text = "<sprite name=\"" + Data.TMPKey + "\">";
		}
		else if (Data.Icon != null)
		{
			_glossaryIcon.gameObject.SetActive(value: true);
			_glossaryIcon.sprite = Data.Icon;
		}
		_glossaryDescription.text = Data.GetDescription();
	}

	private void OnDisable()
	{
		Clear();
	}

	private void Clear()
	{
		_glossaryIcon.gameObject.SetActive(value: false);
		_glossaryKey.gameObject.SetActive(value: false);
		if (_leftSidePreview != null)
		{
			Object.Destroy(_leftSidePreview);
		}
	}
}

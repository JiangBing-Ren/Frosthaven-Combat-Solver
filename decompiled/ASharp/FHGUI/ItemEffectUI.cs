using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class ItemEffectUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Image _icon;

	public void Init(ItemDataDTO.ItemEffectDTO effectDto)
	{
		_icon.sprite = effectDto.Icon;
		_text.enabled = effectDto.Name.IsNOTNullOrEmpty();
		_text.text = effectDto.Name ?? string.Empty;
	}
}

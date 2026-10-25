using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHSimpleIcon : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Image Icon;

	[SerializeField]
	private TMP_Text Value;

	[SerializeField]
	private UITextTooltipTarget Tooltips;

	public void Init(SpriteWithTooltipDTO dto)
	{
		Icon.sprite = dto.Sprite;
		Icon.color = dto.Tint;
		if (Value != null)
		{
			Value.text = dto.TextValue;
		}
		Tooltips?.SetText(dto.Description);
	}
}

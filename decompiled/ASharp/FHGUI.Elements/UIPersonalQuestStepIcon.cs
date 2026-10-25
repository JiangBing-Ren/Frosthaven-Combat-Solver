using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPersonalQuestStepIcon : PersonalQuestStepUI
{
	[SerializeField]
	private TMP_Text _goalDescription;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	public override void Init(PersonalQuestStepDTO dto)
	{
		TMP_Text goalDescription = _goalDescription;
		string text;
		if (dto.IsComplete)
		{
			text = dto.Name;
		}
		else
		{
			text = (dto.ProgressDesc.IsNOTNullOrEmpty() ? (dto.Name + " - " + dto.ProgressDesc) : dto.Name);
		}
		goalDescription.text = text;
		_tooltip.SetText(dto.Tooltip);
		_tooltip.TooltipEnabled = !string.IsNullOrEmpty(dto.Tooltip);
		_icon.sprite = dto.Icon;
	}
}

using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIPersonalQuestStepProgress : PersonalQuestStepUI
{
	[SerializeField]
	private TMP_Text _goalDescription;

	[SerializeField]
	private TMP_Text _parentName;

	[SerializeField]
	private FHImageFillElement _progressBar;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private TMP_Text _progress;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Toggle _checkbox;

	public override void Init(PersonalQuestStepDTO dto)
	{
		_goalDescription.enabled = dto.Name.IsNOTNullOrEmpty();
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
		if ((bool)_parentName)
		{
			_parentName.text = dto.PQName;
		}
		_tooltip.SetText(dto.Tooltip);
		_tooltip.TooltipEnabled = !string.IsNullOrEmpty(dto.Tooltip);
		_progressBar.SetProgress(dto.Progress);
		_canvasGroup.alpha = (dto.IsComplete ? 0.25f : 1f);
		if ((bool)_checkbox)
		{
			_checkbox.isOn = dto.IsComplete;
		}
		if ((bool)_progress)
		{
			_progress.enabled = !dto.IsCheckbox;
			_progress.text = $"{dto.Current}/{dto.Max}";
			_progressBar.gameObject.SetActive(!dto.IsCheckbox);
		}
	}
}

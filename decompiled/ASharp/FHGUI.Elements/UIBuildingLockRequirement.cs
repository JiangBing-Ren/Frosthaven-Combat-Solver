using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

[ExecuteAlways]
public class UIBuildingLockRequirement : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _requirementText;

	[SerializeField]
	private Image _requirementIcon;

	[SerializeField]
	private UITextTooltipTarget _requirementTooltip;

	[SerializeField]
	private uint _level = 1u;

	private void OnEnable()
	{
		SetLevel(_level);
	}

	public void ShowBasedOnLevel(uint currentLevel)
	{
		gameObject.SetActive(_level > currentLevel);
	}

	public void SetLevel(uint level)
	{
		_level = level;
		_requirementText.text = _level.ToString();
		if (_requirementTooltip != null)
		{
			_requirementTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_REQUIRES_LEVEL"), _level));
		}
	}

	public void Show(uint level, uint currentLevel)
	{
		SetLevel(level);
		ShowBasedOnLevel(currentLevel);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}

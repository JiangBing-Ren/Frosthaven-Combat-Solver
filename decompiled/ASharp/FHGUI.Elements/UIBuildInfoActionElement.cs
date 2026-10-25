using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildInfoActionElement : MonoBehaviour
{
	[SerializeField]
	private FHButton _button;

	[SerializeField]
	private UITextTooltipTarget _buttonTooltip;

	[SerializeField]
	private FHStacksCounter _stacks;

	private BuildingFunctionalityActionHandler _onClick;

	public FHButton Button => _button;

	private void Awake()
	{
		_button.onClick.AddListener(() =>
		{
			_onClick?.Invoke();
		});
	}

	public void Init(BuildingFunctionalityAction data)
	{
		_onClick = data.OnClick;
		_button.interactable = data.IsInteractable;
		if (_button.Text != null)
		{
			if (data.Localize)
			{
				UIHelperUtil.SetTextKey(_button.Text, data.ActionText);
			}
			else
			{
				_button.Text.text = data.ActionText;
			}
		}
		_buttonTooltip.SetText(data.TooltipText);
		bool flag = data.MaxCharges > 0;
		_stacks.gameObject.SetActive(flag);
		if (flag)
		{
			_stacks.Init(data.Charges, data.MaxCharges);
		}
	}
}

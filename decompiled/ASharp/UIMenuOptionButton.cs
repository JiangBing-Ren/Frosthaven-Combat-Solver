using UnityEngine;
using UnityEngine.UI;

public class UIMenuOptionButton : UIMenuOption
{
	[SerializeField]
	private Image selectedMask;

	[Space]
	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	protected bool toggleSelectedOnClick = true;

	protected virtual void Awake()
	{
		button.onClick.AddListener(OnClick);
		button.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: false);
		});
	}

	protected void OnClick()
	{
		if (toggleSelectedOnClick)
		{
			ToggleSelection(!isSelected);
		}
		else
		{
			Select();
		}
	}

	public override void SetSelected(bool selected)
	{
		base.SetSelected(selected);
		selectedMask.enabled = selected;
	}
}

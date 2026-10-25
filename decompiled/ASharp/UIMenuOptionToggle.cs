using UnityEngine;

public class UIMenuOptionToggle : UIMenuOption
{
	[SerializeField]
	protected ExtendedToggle toggle;

	[Header("Hover")]
	[SerializeField]
	private RectTransform hoverRect;

	[SerializeField]
	private Vector2 hoverDisplacement;

	private static Vector2 zero = Vector2.zero;

	private void Awake()
	{
		toggle.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		toggle.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: false);
		});
		toggle.onValueChanged.AddListener((bool isOn) =>
		{
			if (isOn)
			{
				base.Select();
			}
			else
			{
				base.Deselect();
			}
		});
	}

	protected override void OnHovered(bool hovered)
	{
		hoverRect.anchoredPosition = (hovered ? hoverDisplacement : zero);
		base.OnHovered(hovered);
	}

	public override void Select()
	{
		toggle.isOn = true;
	}

	public override void Deselect()
	{
		toggle.isOn = false;
	}

	public override void SetSelected(bool isSelected)
	{
		base.SetSelected(isSelected);
		toggle.SetValue(isSelected);
	}
}

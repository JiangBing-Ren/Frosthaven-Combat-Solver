using System;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UIMainMenuSuboption : UIMainMenuOption
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image iconHighlight;

	public void Init(IMenuSuboption option, ToggleGroup toggleGroup, Action onSelected, Action onDeselected, Action<bool> onHovered)
	{
		base.Init(onSelected, onDeselected, onHovered);
		toggle.group = toggleGroup;
		icon.sprite = option.Icon;
		iconHighlight.sprite = ((option.IconHighlight == null) ? option.Icon : option.IconHighlight);
		text.text = LocalizationManager.GetTranslation(option.NameLocKey);
		IsInteractable = option.IsInteractable;
	}

	protected override void RefreshHighlight(bool highlight)
	{
		base.RefreshHighlight(highlight);
		iconHighlight.enabled = highlight;
	}

	public void ToggleSelect(bool select)
	{
		ToggleSelection(select);
	}
}

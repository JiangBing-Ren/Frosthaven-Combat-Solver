using System;

namespace FHGUI.DTO;

public struct QuickAccessTabDTO(Action onClick, bool hasNew, bool isInteractable = true, string tooltipText = null)
{
	public Action OnClick = onClick;

	public bool HasNew = hasNew;

	public string TooltipText = tooltipText;

	public bool IsInteractable = isInteractable;
}

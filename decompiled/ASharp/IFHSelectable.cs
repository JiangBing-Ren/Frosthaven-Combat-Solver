using UnityEngine.Events;

public interface IFHSelectable
{
	public enum SelectionState
	{
		Normal,
		Highlighted,
		Pressed,
		Selected,
		Disabled
	}

	bool IsFaded { get; }

	bool IsActiveAndEnabled { get; }

	event UnityAction HighlightBegin;

	event UnityAction HighlightEnd;

	event UnityAction PointerEntered;

	event UnityAction PointerExited;

	event UnityAction PointerDown;

	event UnityAction PointerUp;

	event UnityAction Selected;

	event UnityAction Deselected;

	event UnityAction<SelectionState> OnSelectionStateChanged;

	bool IsInteractable();

	bool IsHighlighted();

	bool IsSelected();

	void RemoveAllEventListeners();

	void SetAsFaded(bool faded, float fade = 0.3f);
}

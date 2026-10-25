using UnityEngine;

namespace FHGUI.Modules;

public abstract class UIInteractionEvents<T>
{
	public delegate void OnUIEvent(T target);

	public delegate void OnUIFlagEvent(T target, bool isFlagRaised);

	public delegate void OnUIVector2Event(T target, Vector2 delta);

	public OnUIFlagEvent OnHover;

	public OnUIEvent OnClick;

	public OnUIEvent OnDoubleClick;

	public OnUIEvent OnRightClick;

	public OnUIVector2Event OnDrag;
}

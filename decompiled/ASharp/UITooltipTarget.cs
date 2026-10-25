using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UITooltipTarget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	protected UITooltipManager.Corner corner = UITooltipManager.Corner.Auto;

	[SerializeField]
	protected Vector2 anchoredOffset = Vector2.zero;

	[SerializeField]
	protected Vector2 anchoredPositionOffset = Vector2.zero;

	[SerializeField]
	protected bool anchorToExactMouseTargetInstead;

	[SerializeField]
	protected float width = 100f;

	[SerializeField]
	protected bool autoAdjustHeight = true;

	[ConditionalField("autoAdjustHeight", false, true)]
	[SerializeField]
	protected float height = 50f;

	[SerializeField]
	protected bool hideBackground;

	[SerializeField]
	protected bool showOnSelect = true;

	[SerializeField]
	protected bool preserveOnSelect;

	[SerializeField]
	protected bool screenBound;

	[ConditionalField("screenBound", true, true)]
	[SerializeField]
	protected float screenBoundOffset = 20f;

	protected bool tooltipShown;

	protected bool _selected;

	protected PointerEventData _eventData;

	public bool TooltipEnabled { get; set; } = true;

	public void Initialize(UITooltipManager.Corner corner, Vector2 anchoredOffset, bool anchorToExactMouseTargetInstead, float width, float height, bool autoAdjustHeight, bool hideBackground)
	{
		this.corner = corner;
		this.anchoredOffset = anchoredOffset;
		this.anchorToExactMouseTargetInstead = anchorToExactMouseTargetInstead;
		this.width = width;
		this.height = height;
		this.autoAdjustHeight = autoAdjustHeight;
		this.hideBackground = hideBackground;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (TooltipEnabled && !tooltipShown && CanShowTooltip())
		{
			_eventData = eventData;
			PrepareTooltip(eventData);
			ShowTooltip();
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (TooltipEnabled && tooltipShown)
		{
			HideTooltip();
			eventData = null;
		}
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		_selected = true;
		if (showOnSelect)
		{
			OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (showOnSelect)
		{
			OnPointerExit(null);
		}
		_selected = false;
	}

	private void OnDisable()
	{
		if (TooltipEnabled && tooltipShown)
		{
			HideTooltip();
		}
		_selected = false;
	}

	protected virtual bool CanShowTooltip()
	{
		return true;
	}

	protected void PrepareTooltip(PointerEventData eventData)
	{
		UITooltipManager.AnchorToRect(((anchorToExactMouseTargetInstead && eventData != null) ? eventData.pointerEnter.transform : transform) as RectTransform, corner);
		UITooltipManager.SetAnchoredOffset(anchoredOffset);
		UITooltipManager.SetAnchoredPositionOffset(anchoredPositionOffset);
		UITooltipManager.ShowBackground(!hideBackground);
		UITooltipManager.SetScreenBound(screenBound, screenBoundOffset);
		SetControls();
		if (autoAdjustHeight)
		{
			UITooltipManager.SetWidth(width);
		}
		else
		{
			UITooltipManager.SetSize(width, height);
		}
	}

	protected virtual void SetControls()
	{
		UITooltipManager.SetVerticalControls(autoAdjustHeight);
	}

	protected virtual void ShowTooltip(float delay = -1f)
	{
		UITooltipManager.Show(delay);
		tooltipShown = true;
	}

	public virtual void HideTooltip(float delay = -1f)
	{
		UITooltipManager.Hide(delay);
		tooltipShown = false;
	}
}

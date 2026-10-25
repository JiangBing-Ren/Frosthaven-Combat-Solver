using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIConditionEffectElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private UIPrefabTooltipTarget _tooltip;

	[SerializeField]
	private UITextTooltipTarget _tooltipText;

	private StatusEffectDTO _status;

	private void OnHover(bool hovered)
	{
		_highlight.enabled = hovered;
	}

	private void OnEnable()
	{
		OnHover(hovered: false);
	}

	public void Init(StatusEffectDTO effect, UIPrefabTooltipTarget.TooltipInitParams tooltipInitParams = null)
	{
		_status = effect;
		_icon.enabled = true;
		_icon.sprite = effect.Icon;
		if (effect.Description.IsNullOrEmpty())
		{
			_tooltip.enabled = false;
			if (_tooltipText != null)
			{
				_tooltipText.enabled = _status.Name.IsNOTNullOrEmpty();
				_tooltipText.SetText(_status.Name);
			}
		}
		else if (tooltipInitParams != null)
		{
			_tooltip.enabled = true;
			if (_tooltipText != null)
			{
				_tooltipText.enabled = false;
			}
			_tooltip.Initialize(tooltipInitParams.Prefab, tooltipInitParams.OnObjectCreatedCallback);
		}
	}

	public void SetEmpty()
	{
		_icon.enabled = false;
		_tooltip.enabled = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover(hovered: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHover(hovered: false);
	}
}

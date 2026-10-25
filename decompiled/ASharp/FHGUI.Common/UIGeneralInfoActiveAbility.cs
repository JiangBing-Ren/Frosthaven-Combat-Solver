using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoActiveAbility : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private UIPrefabTooltipTarget _tooltip;

	private StatusEffectDTO _status;

	private void OnHover(bool hovered)
	{
		_highlight.enabled = hovered;
	}

	private void OnEnable()
	{
		OnHover(hovered: false);
	}

	public void Init(ActiveAbilityDTO activeAbility, UIPrefabTooltipTarget.TooltipInitParams tooltipInitParams = null)
	{
		_icon.sprite = activeAbility.Icon;
		if (tooltipInitParams != null)
		{
			_tooltip.Initialize(tooltipInitParams.Prefab, tooltipInitParams.OnObjectCreatedCallback);
		}
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

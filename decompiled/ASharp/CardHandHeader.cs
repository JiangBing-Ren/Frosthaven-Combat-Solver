using UnityEngine;

public class CardHandHeader : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup tooltip;

	[SerializeField]
	private bool highlightTooltip;

	[SerializeField]
	private Vector2 tooltipOffset;

	[SerializeField]
	private TextLocalizedListener headerName;

	[SerializeField]
	private TextLocalizedListener tooltipDescription;

	[SerializeField]
	private TextLocalizedListener tooltipWarning;

	private void Awake()
	{
		tooltip.alpha = 0f;
	}

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		OnHovered(hovered: false);
		gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		tooltip.alpha = 0f;
	}

	public void OnHovered(bool hovered)
	{
		if (hovered)
		{
			tooltip.alpha = 1f;
			if (highlightTooltip)
			{
				RectTransform component = tooltip.GetComponent<RectTransform>();
				if (((Vector2)component.DeltaPositionToFitTheScreen(UIManager.Instance.UICamera, 10f)).x != 0f)
				{
					component.pivot = new Vector2(1f, component.pivot.y);
					component.anchorMin = new Vector2(0f, component.anchorMin.y);
					component.anchorMax = new Vector2(0f, component.anchorMax.y);
					component.anchoredPosition = -tooltipOffset;
				}
				UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			}
		}
		else
		{
			tooltip.alpha = 0f;
			if (highlightTooltip)
			{
				UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
				RectTransform component2 = tooltip.GetComponent<RectTransform>();
				component2.pivot = new Vector2(0f, component2.pivot.y);
				component2.anchorMin = new Vector2(1f, component2.anchorMin.y);
				component2.anchorMax = new Vector2(1f, component2.anchorMax.y);
				component2.anchoredPosition = tooltipOffset;
			}
		}
	}

	public void SetKeys(string headerNameKey, string tooltipDescriptionKey, string tooltipWarningKey)
	{
		headerName.SetTextKey(headerNameKey);
		tooltipDescription.SetTextKey(tooltipDescriptionKey);
		tooltipWarning.SetTextKey(tooltipWarningKey);
	}
}

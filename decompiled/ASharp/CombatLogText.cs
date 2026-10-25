using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatLogText : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TextMeshProUGUI textLog;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Sprite highlight;

	[SerializeField]
	private TextMeshProTooltip tooltip;

	private Sprite backgroundSprite;

	public void Init(string text, Sprite backgroundSprite = null)
	{
		textLog.text = text;
		this.backgroundSprite = backgroundSprite;
		Highlight(enable: false);
		tooltip.enabled = text.Contains("<link=");
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Highlight(enable: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Highlight(enable: false);
	}

	private void Highlight(bool enable)
	{
		if (enable)
		{
			background.sprite = highlight;
			background.enabled = true;
		}
		else
		{
			background.sprite = backgroundSprite;
			background.enabled = backgroundSprite != null;
		}
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class FHUICodexTableCell : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Image Background;

	[SerializeField]
	private TextMeshProUGUI Text;

	[SerializeField]
	private Image Icon;

	[SerializeField]
	private UITextTooltipTarget Tooltip;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private float m_TextHorizontalPadding = 8f;

	[SerializeField]
	private HorizontalLayoutGroup m_LayoutGroup;

	[SerializeField]
	private FHButton m_Button;

	public void Init(CellData data, ColumnDefinition column, RowData row)
	{
		if (data == null)
		{
			return;
		}
		if (data.Merged)
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		if (Text != null)
		{
			bool flag = data.TextValue.IsNOTNullOrEmpty();
			if (flag)
			{
				Text.text = data.TextValue;
			}
			Text.gameObject.SetActive(flag);
			Text.enabled = !data.Hidden;
			if (data.Style != null)
			{
				Text.fontSizeMin = data.Style.MinFontSize;
				Text.fontSizeMax = data.Style.MaxFontSize;
				m_LayoutGroup.childAlignment = data.Style.Alignment;
			}
		}
		if (Icon != null)
		{
			Icon.sprite = data.Icon;
			Icon.gameObject.SetActive(data.Icon != null);
			Icon.enabled = !data.Hidden;
		}
		if ((bool)Tooltip)
		{
			Tooltip.SetText(data.Tooltip);
		}
		if ((bool)m_LayoutElement)
		{
			m_LayoutElement.minWidth = data.Width;
			m_LayoutElement.minHeight = row.Height;
			m_LayoutElement.preferredWidth = data.Width;
			m_LayoutElement.preferredHeight = row.Height;
		}
		if ((bool)Background)
		{
			Background.enabled = !data.Hidden;
		}
		if ((bool)m_LayoutGroup)
		{
			m_LayoutGroup.childForceExpandWidth = !(data.Icon != null) || !data.TextValue.IsNOTNullOrEmpty();
		}
		if (!m_Button)
		{
			return;
		}
		m_Button.RemoveAllEventListeners();
		bool flag2 = data.OnClickAction != null;
		m_Button.interactable = flag2;
		if (flag2)
		{
			m_Button.onClick.AddListener(() =>
			{
				data.OnClickAction();
			});
		}
	}

	public float CalcDesiredWidth(CellData cell)
	{
		return Text.GetPreferredValues(cell.TextValue).x + m_TextHorizontalPadding + (float)((cell.Icon != null) ? 36 : 0) + 8f;
	}
}

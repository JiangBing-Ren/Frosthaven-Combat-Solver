using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDebriefingTableCell : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text Value;

	[SerializeField]
	private Image Icon;

	[SerializeField]
	private GameObject Highlight;

	[Header("Settings")]
	[SerializeField]
	private Color TextColorNormal;

	[SerializeField]
	private Color TextColorHighlighted;

	public void Init(DebriefingTableDTO.Cell cellData)
	{
		if (Value != null)
		{
			Value.text = cellData.Text;
			Value.color = (cellData.IsHighlighted ? TextColorHighlighted : TextColorNormal);
		}
		if (Icon != null)
		{
			Icon.sprite = cellData.Icon;
			Icon.enabled = cellData.Icon != null;
		}
		if (Highlight != null)
		{
			Highlight.gameObject.SetActive(cellData.IsHighlighted);
		}
	}
}

using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDebriefingTableHeader : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TMP_Text Label;

	[SerializeField]
	private Image Icon;

	public void Init(DebriefingTableDTO.Header tableDataCell)
	{
		if (Label != null)
		{
			Label.text = tableDataCell.Text;
		}
		if (Icon != null)
		{
			Icon.sprite = tableDataCell.Icon;
		}
	}
}

using FHGUI.DTO.Outpost;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class CalendarEventElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	public void Init(CampaignSectionEventDTO data)
	{
		_icon.sprite = data.Icon;
	}
}

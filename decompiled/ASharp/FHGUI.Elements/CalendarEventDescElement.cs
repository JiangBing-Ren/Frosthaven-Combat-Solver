using FHGUI.DTO.Outpost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class CalendarEventDescElement : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _description;

	public void Init(CampaignSectionEventDTO dto)
	{
		_icon.sprite = dto.Icon;
		_name.text = dto.Name;
		_description.text = dto.Desc;
	}
}

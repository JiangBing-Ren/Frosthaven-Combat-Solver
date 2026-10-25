using FHGUI.DTO.Outpost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.Notifications;

public class UIOutpostEventLogRow : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Image _dot;

	[SerializeField]
	private Image _divider;

	public EventLogDTO Data { get; private set; }

	public void Init(EventLogDTO data)
	{
		Data = data;
		_text.text = data.Text;
		_dot.gameObject.SetActive(!data.IsNewWeek);
		_divider.enabled = data.IsNewWeek;
		_text.margin = new Vector4(data.IsNewWeek ? 5 : 30, _text.margin.y, _text.margin.z, _text.margin.w);
	}
}

using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO.Outpost;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class CalendarEventsListContainer : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	protected CalendarEventElement m_Prototype;

	[SerializeField]
	private TMP_Text _moreElements;

	private List<CalendarEventElement> m_ListElements = new List<CalendarEventElement>();

	private int m_ActiveCount;

	private const int MAX_EVENTS = 3;

	public IEnumerable<CalendarEventElement> Elements => m_ListElements.Take(m_ActiveCount);

	private void InitCache()
	{
		if (m_ListElements.Count <= 0)
		{
			m_Container.GetComponentsInChildren(m_ListElements);
		}
	}

	public void Init(IList<CampaignSectionEventDTO> dtos)
	{
		InitCache();
		int num = dtos?.Count ?? 0;
		int num2 = (m_ActiveCount = Mathf.Min(num, 3));
		HelperTools.NormalizePool(ref m_ListElements, m_Prototype.gameObject, m_Container, num2);
		for (int i = 0; i < num2; i++)
		{
			m_ListElements[i].Init(dtos[i]);
		}
		_moreElements.text = ((num2 < num) ? $"+{num - num2}" : "");
	}
}

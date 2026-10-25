using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIAnchorLinkNavbar : MonoBehaviour
{
	public struct Params
	{
		public string LinkID;

		public string Tooltip;

		public Sprite Icon;
	}

	[Header("Base Elements")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private GameObject m_Prototype;

	[SerializeField]
	private ScrollRect m_TargetScrollRect;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorNormal;

	[SerializeField]
	private Color m_ColorCurrent;

	private List<GameObject> m_Icons = new List<GameObject>();

	private List<Params> m_Links = new List<Params>();

	private List<UIAnchorLink> m_TrackedObjects = new List<UIAnchorLink>();

	private bool m_Initialized;

	public int SelectedIndex { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((object)m_Prototype != null)
			{
				m_Icons.Add(m_Prototype);
			}
		}
	}

	public void Init(List<Params> links)
	{
		InitCache();
		m_Links = links;
		Populate();
		ExtractTrackedObjects();
	}

	private void ExtractTrackedObjects()
	{
		m_TrackedObjects.Clear();
		if (!(m_TargetScrollRect == null))
		{
			m_TargetScrollRect.GetComponentsInChildren(includeInactive: true, m_TrackedObjects);
		}
	}

	private void Populate()
	{
		HelperTools.NormalizePool(ref m_Icons, m_Prototype, m_Container, m_Links.Count);
		for (int i = 0; i < m_Icons.Count; i++)
		{
			bool flag = m_Links != null && m_Links.Count > i;
			GameObject gameObject = m_Icons[i];
			gameObject.SetActive(flag);
			if (!flag)
			{
				continue;
			}
			Params obj = m_Links[i];
			Image componentInChildren = gameObject.GetComponentInChildren<Image>();
			if ((object)componentInChildren != null)
			{
				componentInChildren.sprite = obj.Icon;
			}
			gameObject.GetComponent<UITextTooltipTarget>()?.SetText(obj.Tooltip);
			FHButton component = gameObject.GetComponent<FHButton>();
			int ci = i;
			if ((object)component != null)
			{
				component.onClick.RemoveAllListeners();
				component.onClick.AddListener(() =>
				{
					FocusOn(ci);
				});
			}
		}
	}

	private void FocusOn(int index)
	{
		m_TargetScrollRect.ScrollLayoutGroupVerticallyToTop(m_TrackedObjects[index].Content);
	}

	private void LateUpdate()
	{
		if (m_TrackedObjects.Count == 0)
		{
			return;
		}
		UIAnchorLink focusedObject = CalculateFocusedObject();
		if (focusedObject == null)
		{
			return;
		}
		int num = m_Links.FindIndex((Params l) => l.LinkID == focusedObject.ID);
		for (int num2 = 0; num2 < m_Icons.Count; num2++)
		{
			Image componentInChildren = m_Icons[num2].GetComponentInChildren<Image>();
			if ((object)componentInChildren != null)
			{
				componentInChildren.color = ((num == num2) ? m_ColorCurrent : m_ColorNormal);
			}
		}
	}

	private UIAnchorLink CalculateFocusedObject()
	{
		float num = (m_TargetScrollRect.horizontal ? m_TargetScrollRect.normalizedPosition.x : (1f - m_TargetScrollRect.normalizedPosition.y));
		float num2 = 0f;
		foreach (UIAnchorLink trackedObject in m_TrackedObjects)
		{
			if (trackedObject.gameObject.activeInHierarchy)
			{
				RectTransform content = trackedObject.Content;
				num2 += (m_TargetScrollRect.horizontal ? content.rect.width : content.rect.height);
			}
		}
		float num3 = 0f;
		foreach (UIAnchorLink trackedObject2 in m_TrackedObjects)
		{
			if (trackedObject2.gameObject.activeInHierarchy)
			{
				RectTransform content2 = trackedObject2.Content;
				float num4 = (m_TargetScrollRect.horizontal ? content2.rect.width : content2.rect.height);
				num3 += num4;
				if (num <= num3 / num2)
				{
					return trackedObject2;
				}
			}
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIBreadcrumbs : MonoBehaviour
{
	public struct Params
	{
		public byte FlowTag;

		public string Text;

		public Sprite Icon;

		public Action onClick;
	}

	[Header("Base Elements")]
	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private GameObject m_CrumPrototype;

	[SerializeField]
	private Image m_Progress;

	[Header("Settings")]
	[SerializeField]
	private Sprite m_IconInactive;

	[SerializeField]
	private Sprite m_IconCurrent;

	[SerializeField]
	private Sprite m_IconPassed;

	[Header("Colors")]
	[SerializeField]
	private Color m_ColorLabelInactive;

	[SerializeField]
	private Color m_ColorLabelCurrent;

	[SerializeField]
	private Color m_ColorLabelPassed;

	private List<GameObject> m_Icons = new List<GameObject>();

	private List<Params> m_Crumbs = new List<Params>();

	private HorizontalOrVerticalLayoutGroup layoutGroup;

	private bool m_Initialized;

	public int SelectedIndex { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			if ((object)m_CrumPrototype != null)
			{
				m_Icons.Add(m_CrumPrototype);
			}
			layoutGroup = m_Container.GetComponent<HorizontalOrVerticalLayoutGroup>();
		}
	}

	public void Init(List<Params> crumbs, byte selectedTag)
	{
		InitCache();
		m_Crumbs = crumbs;
		SelectedIndex = GetSelectedIndex(selectedTag);
		Populate();
		UpdateProgress();
	}

	private int GetSelectedIndex(byte selectedTag)
	{
		int num = m_Crumbs.FindIndex((Params c) => c.FlowTag == selectedTag);
		if (num >= 0)
		{
			return num;
		}
		return 0;
	}

	private void Populate()
	{
		HelperTools.NormalizePool(ref m_Icons, m_CrumPrototype, m_Container, m_Crumbs.Count);
		for (int i = 0; i < m_Icons.Count; i++)
		{
			bool flag = m_Crumbs != null && m_Crumbs.Count > i;
			GameObject gameObject = m_Icons[i];
			gameObject.SetActive(flag);
			if (flag)
			{
				Params obj = m_Crumbs[i];
				Image componentInChildren = gameObject.GetComponentInChildren<Image>();
				if ((object)componentInChildren != null)
				{
					componentInChildren.sprite = GetStateIcon(i);
				}
				TMP_Text componentInChildren2 = gameObject.GetComponentInChildren<TMP_Text>();
				if ((object)componentInChildren2 != null)
				{
					componentInChildren2.text = obj.Text;
					componentInChildren2.color = GetLabelColor(i);
				}
			}
		}
		RepositionIcons();
	}

	private void UpdateProgress()
	{
		if ((object)m_Progress != null)
		{
			m_Progress.fillAmount = ((m_Crumbs.Count > 1) ? ((float)SelectedIndex / (float)(m_Crumbs.Count - 1)) : 0f);
		}
	}

	private void RepositionIcons()
	{
		if ((object)layoutGroup == null)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < m_Container.transform.childCount; i++)
		{
			RectTransform rectTransform = (RectTransform)m_Container.transform.GetChild(i);
			if (rectTransform.gameObject.activeSelf)
			{
				num2++;
				num += rectTransform.rect.width;
			}
		}
		if (num2 <= 1)
		{
			layoutGroup.childAlignment = TextAnchor.MiddleCenter;
			layoutGroup.spacing = 0f;
		}
		else
		{
			layoutGroup.spacing = (m_Container.rect.width - (float)layoutGroup.padding.horizontal - num) / (float)(num2 - 1);
			layoutGroup.childAlignment = TextAnchor.MiddleLeft;
		}
	}

	private Sprite GetStateIcon(int index)
	{
		if (index == SelectedIndex)
		{
			return m_IconCurrent;
		}
		if (index < SelectedIndex)
		{
			return m_IconPassed;
		}
		return m_IconInactive;
	}

	private Color GetLabelColor(int index)
	{
		if (index == SelectedIndex)
		{
			return m_ColorLabelCurrent;
		}
		if (index < SelectedIndex)
		{
			return m_ColorLabelPassed;
		}
		return m_ColorLabelInactive;
	}
}

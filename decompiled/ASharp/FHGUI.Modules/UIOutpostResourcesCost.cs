using System.Collections.Generic;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class UIOutpostResourcesCost : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform m_Contianer;

	[SerializeField]
	private GameObject m_IconPrototype;

	[Header("Settings")]
	[SerializeField]
	private Color m_TextColorNormal;

	[SerializeField]
	private Color m_TextColorUnavailable;

	[SerializeField]
	private Color m_ImageColorNormal;

	[SerializeField]
	private Color m_ImageColorUnavailable;

	private List<GameObject> m_ResourceIcons = new List<GameObject>();

	private bool m_Initialzied;

	private void InitCache()
	{
		if (!m_Initialzied)
		{
			m_Initialzied = true;
			if (m_IconPrototype.transform.parent == m_Contianer)
			{
				m_ResourceIcons.Add(m_IconPrototype);
			}
		}
	}

	public void Init(ResourceCostDTO cost)
	{
		InitCache();
		UIHelperUtil.UpdateUIList(cost.Count, m_ResourceIcons, m_IconPrototype, m_Contianer, onShow);
		void onShow(int index, GameObject inst)
		{
			ResourceCostDTO.ItemData itemData = cost[index];
			PopulateIcon(inst, itemData.Icon, itemData.Value, itemData.Available);
		}
	}

	private void PopulateIcon(GameObject inst, Sprite sprite, int value, bool available)
	{
		Image component = inst.GetComponent<Image>();
		if (component != null)
		{
			component.color = (available ? m_ImageColorNormal : m_ImageColorUnavailable);
			component.sprite = sprite;
		}
		TMP_Text componentInChildren = inst.GetComponentInChildren<TMP_Text>();
		if (componentInChildren != null && (componentInChildren.enabled = value > 1))
		{
			componentInChildren.color = (available ? m_TextColorNormal : m_TextColorUnavailable);
			componentInChildren.text = "x" + value;
		}
	}
}

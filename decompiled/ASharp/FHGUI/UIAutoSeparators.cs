using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI;

public class UIAutoSeparators : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_TargetContainer;

	[SerializeField]
	private GameObject m_SeparatorPrototype;

	private List<GameObject> m_Separators = new List<GameObject>();

	private List<RectTransform> m_TargetElement = new List<RectTransform>();

	private bool m_IgnoreTransformChanges;

	private bool m_Invalidate;

	private void Awake()
	{
		if (m_SeparatorPrototype != null && m_SeparatorPrototype.transform.parent == m_TargetContainer)
		{
			m_Separators.Add(m_SeparatorPrototype);
		}
	}

	private void OnEnable()
	{
		m_Invalidate = true;
	}

	private void Rebuild()
	{
		if (m_TargetContainer == null || m_SeparatorPrototype == null)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < m_TargetContainer.childCount; i++)
		{
			GameObject gameObject = m_TargetContainer.GetChild(i).gameObject;
			LayoutElement component = gameObject.GetComponent<LayoutElement>();
			if ((!(component != null) || !component.ignoreLayout) && gameObject.activeInHierarchy && !m_Separators.Contains(gameObject))
			{
				list.Add(gameObject);
			}
		}
		m_IgnoreTransformChanges = true;
		int newLength = Mathf.Max(list.Count - 1, 0);
		HelperTools.NormalizePool(ref m_Separators, m_SeparatorPrototype, m_TargetContainer, newLength);
		for (int j = 0; j < list.Count; j++)
		{
			list[j].transform.SetSiblingIndex(j * 2);
			if (list.Count > j + 1)
			{
				m_Separators[j].transform.SetSiblingIndex(j * 2 + 1);
			}
		}
		m_IgnoreTransformChanges = false;
	}

	private void OnTransformChildrenChanged()
	{
		if (!m_IgnoreTransformChanges)
		{
			m_Invalidate = true;
		}
	}

	private void LateUpdate()
	{
		if (m_Invalidate)
		{
			m_Invalidate = false;
			Rebuild();
		}
	}

	public void Invalidate()
	{
		m_Invalidate = true;
	}

	public void ForceRebuild()
	{
		Rebuild();
	}
}

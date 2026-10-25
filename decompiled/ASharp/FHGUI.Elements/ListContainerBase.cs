using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FHGUI.Elements;

public abstract class ListContainerBase<TListElementComponent, TListParams, TElementParams> : MonoBehaviour where TListElementComponent : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	protected RectTransform m_Container;

	[SerializeField]
	protected TListElementComponent m_Prototype;

	protected List<TListElementComponent> m_ListElements = new List<TListElementComponent>();

	private int m_ActiveCount;

	public IEnumerable<TListElementComponent> Elements => m_ListElements.Take(m_ActiveCount);

	private void InitCache()
	{
		if (m_ListElements.Count <= 0)
		{
			m_Container.GetComponentsInChildren(m_ListElements);
			InitCache_Impl();
		}
	}

	protected virtual void InitCache_Impl()
	{
	}

	public void Init(TListParams listParams, IList<TElementParams> listElementParams)
	{
		InitCache();
		InitList(listParams, listElementParams);
		int num = (m_ActiveCount = listElementParams?.Count ?? 0);
		HelperTools.NormalizePool(ref m_ListElements, m_Prototype.gameObject, m_Container, num);
		for (int i = 0; i < num; i++)
		{
			InitElement(m_ListElements[i], listElementParams[i], listParams);
		}
		AfterInit_Impl(listParams);
	}

	protected virtual void InitList(TListParams listParams, IList<TElementParams> listElementParams)
	{
	}

	protected virtual void AfterInit_Impl(TListParams listParams)
	{
	}

	protected abstract void InitElement(TListElementComponent listElement, TElementParams elementParams, TListParams listParams);
}

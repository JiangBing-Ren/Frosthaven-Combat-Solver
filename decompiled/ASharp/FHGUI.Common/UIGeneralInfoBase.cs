using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public abstract class UIGeneralInfoBase : MonoBehaviour, IUIGeneralInfo
{
	[SerializeField]
	private UIGeneralInfoNavigator m_Navigator;

	[SerializeField]
	private GameObject m_GroupInfoContainer;

	[SerializeField]
	private TextMeshProUGUI m_GroupInfoCount;

	public Action OnHide { get; set; }

	public virtual UIGeneralInfoNavigator Navigator
	{
		get
		{
			if (m_Navigator == null)
			{
				m_Navigator = GetComponentInChildren<UIGeneralInfoNavigator>();
			}
			return m_Navigator;
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		OnHide_Internal();
		OnHide?.Invoke();
	}

	protected virtual void OnHide_Internal()
	{
	}

	public void RefreshNavigator(List<IGeneralInfoData> _datas)
	{
		if (m_GroupInfoContainer != null)
		{
			m_GroupInfoContainer.SetActive(_datas.Count > 1);
		}
		if (m_GroupInfoCount != null)
		{
			m_GroupInfoCount.text = _datas.Count.ToString();
		}
	}
}

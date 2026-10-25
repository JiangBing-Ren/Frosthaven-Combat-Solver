using System;
using System.Collections.Generic;
using GLOOM;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIFilterGroup : MonoBehaviour
{
	public class FilterParam
	{
		public Enum Filter;

		public Sprite Icon;

		public Sprite IconInactive;

		public string TextKey;

		public string TooltipKey;
	}

	[Header("Components")]
	[SerializeField]
	private RectTransform m_Contianer;

	[SerializeField]
	private FHTabButton m_Prototype;

	[Header("Settings")]
	[SerializeField]
	private bool m_FullMachOnly;

	private FilterParam[] m_filters;

	private List<FHTabButton> m_Buttons;

	private bool _initialized;

	public Action<Enum> OnFilter;

	public Action<Enum> OnStateUpdated;

	public Enum ActiveFilter { get; private set; }

	private void InitCache()
	{
		if (!_initialized)
		{
			if (m_Buttons == null)
			{
				m_Buttons = new List<FHTabButton>();
			}
			m_Buttons.AddRange(m_Contianer.GetComponentsInChildren<FHTabButton>());
			_initialized = true;
		}
	}

	public void SetFilters(FilterParam[] filters)
	{
		InitCache();
		m_filters = filters;
		UpdateFilters();
	}

	private void UpdateFilters()
	{
		if (m_filters != null && m_filters.Length != 0)
		{
			UIHelperUtil.UpdateUIList(m_filters.Length, m_Buttons, m_Prototype, m_Contianer, setFilterData);
		}
		void setFilterData(int i, FHTabButton toggle)
		{
			FilterParam filterData = ((i < m_filters.Length) ? m_filters[i] : null);
			if (filterData != null)
			{
				toggle.onClick.RemoveAllListeners();
				toggle.onClick.AddListener(() =>
				{
					HandleOnFilter(toggle, filterData.Filter);
				});
				UITextTooltipTarget component = toggle.GetComponent<UITextTooltipTarget>();
				if (component != null)
				{
					component.SetText(string.IsNullOrEmpty(filterData.TooltipKey) ? string.Empty : LocalizationManager.GetTranslation(filterData.TooltipKey));
				}
				Image image = toggle.transform.FindTransformInChildren("Icon")?.GetComponent<Image>();
				if (image != null)
				{
					image.sprite = filterData.Icon;
				}
				Image image2 = toggle.transform.FindTransformInChildren("IconInactive")?.GetComponent<Image>();
				if (image2 != null)
				{
					image2.sprite = filterData.IconInactive;
				}
				Image image3 = toggle.transform.FindTransformInChildren("IconActive")?.GetComponent<Image>();
				if (image3 != null)
				{
					image3.sprite = filterData.Icon;
				}
				TMP_Text tMP_Text = toggle.transform.FindTransformInChildren("Text")?.GetComponent<TMP_Text>();
				if (tMP_Text != null)
				{
					tMP_Text.text = LocalizationManager.GetTranslation(filterData.TextKey);
				}
				TMP_Text tMP_Text2 = toggle.transform.FindTransformInChildren("TextInactive")?.GetComponent<TMP_Text>();
				if (tMP_Text2 != null)
				{
					tMP_Text2.text = LocalizationManager.GetTranslation(filterData.TextKey);
				}
			}
		}
	}

	public void UpdateState(Enum filter)
	{
		ActiveFilter = filter;
		if (m_filters != null && m_filters.Length != 0)
		{
			for (int i = 0; i < m_filters.Length; i++)
			{
				FHTabButton fHTabButton = m_Buttons[i];
				FilterParam filterParam = m_filters[i];
				fHTabButton.IsFocused = (m_FullMachOnly ? object.Equals(filter, filterParam.Filter) : filter.HasFlag(filterParam.Filter));
			}
			OnStateUpdated?.Invoke(ActiveFilter);
		}
	}

	private void HandleOnFilter(FHTabButton button, Enum filter)
	{
		OnFilter?.Invoke(filter);
	}
}

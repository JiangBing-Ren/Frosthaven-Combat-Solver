using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPagination : MonoBehaviour
{
	[SerializeField]
	private RectTransform _paginationContainer;

	[SerializeField]
	private ToggleGroup _toggleGroup;

	[SerializeField]
	private Toggle _paginationPrefab;

	[SerializeField]
	private bool _showWhenOnlyOnePage = true;

	private List<Toggle> _toggles = new List<Toggle>();

	private List<UnityAction<bool>> _toggleEvents = new List<UnityAction<bool>>();

	private UnityAction<int> _action;

	private int m_ActiveCount;

	[SerializeField]
	private UnityEvent<int> _onPageClick = new UnityEvent<int>();

	public IEnumerable<Toggle> Elements => _toggles.Take(m_ActiveCount);

	public int Current => _toggles.IndexOf(_toggleGroup.GetFirstActiveToggle());

	private void Awake()
	{
		HelperTools.NormalizePool(ref _toggles, _paginationPrefab.gameObject, _paginationContainer, 0);
	}

	private void OnDisable()
	{
		if (_action != null)
		{
			_onPageClick.RemoveListener(_action);
		}
		_action = null;
	}

	private void InitCache()
	{
		if (_toggles.Count <= 0)
		{
			_paginationContainer.GetComponentsInChildren(_toggles);
			_toggleGroup.allowSwitchOff = false;
			HelperTools.NormalizePool(ref _toggles, _paginationPrefab.gameObject, _paginationContainer, 0);
		}
	}

	public void InitPages(int count, Action<int> onPageClick, int current = 0)
	{
		InitCache();
		m_ActiveCount = count;
		if (_action != null)
		{
			_onPageClick.RemoveListener(_action);
		}
		_action = ((onPageClick == null) ? null : new UnityAction<int>(onPageClick.Invoke));
		_onPageClick.AddListener(_action);
		HelperTools.NormalizePool(ref _toggles, _paginationPrefab.gameObject, _paginationContainer, count);
		for (int i = 0; i < _toggles.Count; i++)
		{
			Toggle toggle = _toggles[i];
			if (i < m_ActiveCount)
			{
				toggle.group = _toggleGroup;
				_toggleGroup.RegisterToggle(toggle);
				UnityAction<bool> unityAction = _toggleEvents.ElementAtOrDefault(i);
				if (unityAction == null)
				{
					unityAction = (bool isOn) =>
					{
						OnToggleClick(toggle, isOn);
					};
					_toggleEvents.Add(unityAction);
					toggle.onValueChanged.AddListener(unityAction);
				}
			}
			else
			{
				_toggleGroup.UnregisterToggle(toggle);
			}
		}
		_paginationContainer.gameObject.SetActive(m_ActiveCount > 1 || _showWhenOnlyOnePage);
		SetCurrentPage(current);
	}

	private void OnToggleClick(Toggle toggle, bool selected)
	{
		if (selected)
		{
			_onPageClick?.Invoke(_toggles.IndexOf(toggle));
		}
	}

	public void SetCurrentPage(int current)
	{
		if (Current != current)
		{
			_toggleGroup.SetAllTogglesOff(sendCallback: false);
			int index = Mathf.Clamp(current, 0, m_ActiveCount - 1);
			if (m_ActiveCount > 0)
			{
				_toggles[index].SetIsOnWithoutNotify(value: true);
			}
		}
	}
}

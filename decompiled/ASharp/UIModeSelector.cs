using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIModeSelector<T> : MonoBehaviour
{
	[Serializable]
	protected class ModeConfig
	{
		public Toggle toggle;

		public List<UIOptionEntry> entries;

		public T Value;

		public void RefreshEnabled()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].Enable(toggle.isOn);
			}
		}
	}

	[Serializable]
	public class ModeSelectorEvent : UnityEvent<T>
	{
	}

	[SerializeField]
	protected List<ModeConfig> m_Modes;

	public ModeSelectorEvent OnModeChanged;

	protected ModeConfig selectedMode;

	protected virtual void Awake()
	{
		for (int i = 0; i < m_Modes.Count; i++)
		{
			ModeConfig mode = m_Modes[i];
			if (mode.toggle.isOn)
			{
				selectedMode = mode;
			}
			mode.RefreshEnabled();
			mode.toggle.onValueChanged.AddListener((bool isOn) =>
			{
				OnValueChanged(isOn, mode);
			});
		}
	}

	public void SelectNext()
	{
		if (selectedMode == null)
		{
			m_Modes.First().toggle.isOn = true;
			return;
		}
		int num = m_Modes.IndexOf(selectedMode);
		m_Modes[(num + 1) % m_Modes.Count].toggle.isOn = true;
	}

	public void SelectPrevious()
	{
		if (selectedMode == null)
		{
			m_Modes.First().toggle.isOn = true;
			return;
		}
		int num = m_Modes.IndexOf(selectedMode);
		m_Modes[(num == 0) ? (m_Modes.Count - 1) : (num - 1)].toggle.isOn = true;
	}

	private void OnValueChanged(bool value, ModeConfig mode)
	{
		mode.RefreshEnabled();
		if (value && selectedMode != mode)
		{
			selectedMode = mode;
			OnModeChanged.Invoke(GetSelectedMode());
		}
	}

	public virtual void SetMode(T mode)
	{
		SetMode(m_Modes.First((ModeConfig it) =>
		{
			ref T value = ref it.Value;
			object obj = mode;
			return value.Equals(obj);
		}));
	}

	protected void SetMode(ModeConfig mode)
	{
		selectedMode = mode;
		if (mode.toggle.isOn)
		{
			mode.toggle.onValueChanged.Invoke(arg0: true);
			return;
		}
		mode.toggle.isOn = true;
		foreach (ModeConfig mode2 in m_Modes)
		{
			if (mode != mode2)
			{
				mode2.toggle.isOn = false;
			}
		}
	}

	public T GetSelectedMode()
	{
		return selectedMode.Value;
	}

	public bool HasSelectedMode()
	{
		return selectedMode != null;
	}
}

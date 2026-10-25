using System;
using UnityEngine;

public class CombatLogFilterSetting : MonoBehaviour
{
	[SerializeField]
	private CombatLogFilter filter;

	[SerializeField]
	private ButtonSwitch toggle;

	private Action<CombatLogFilter, bool> onValueChanged;

	private void Awake()
	{
		toggle.OnValueChanged.AddListener((bool enable) =>
		{
			onValueChanged?.Invoke(filter, enable);
		});
	}

	public void Init(Action<CombatLogFilter, bool> onValueChanged)
	{
		this.onValueChanged = onValueChanged;
		toggle.SetValue(!SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter));
	}
}

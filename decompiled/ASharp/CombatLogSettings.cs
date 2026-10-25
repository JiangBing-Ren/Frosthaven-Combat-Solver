using UnityEngine;
using UnityEngine.Events;

public class CombatLogSettings : Singleton<CombatLogSettings>
{
	[SerializeField]
	private ButtonSwitch toggleCombatLog;

	[SerializeField]
	private CombatLogFilterSetting[] filterSettings;

	public UnityEvent OnCombatLogFiltersChanged = new UnityEvent();

	public UnityEvent OnEnableCombatLog = new UnityEvent();

	public UnityEvent OnDisableCombatLog = new UnityEvent();

	protected override void AwakeInit()
	{
		filterSettings = GetComponentsInChildren<CombatLogFilterSetting>();
	}

	private void Start()
	{
		if (SaveData.Instance?.Global != null)
		{
			PostInitialize();
		}
		else
		{
			SceneController.Instance.OnSaveDataLoaded += PostInitialize;
		}
	}

	private void PostInitialize()
	{
		SceneController.Instance.OnSaveDataLoaded -= PostInitialize;
		toggleCombatLog.IsOn = !SaveData.Instance.Global.DisabledCombatLog;
		toggleCombatLog.OnValueChanged.AddListener(ToggleCombatLog);
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].gameObject.SetActive(!SaveData.Instance.Global.DisabledCombatLog);
			filterSettings[i].Init(ToggleFilter);
		}
	}

	public void ToggleFilter(CombatLogFilter filter, bool active)
	{
		bool flag = false;
		if (!active && !SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter))
		{
			flag = true;
			SaveData.Instance.Global.DisabledCombatLogFilters.Add(filter);
		}
		else if (active && SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter))
		{
			flag = true;
			SaveData.Instance.Global.DisabledCombatLogFilters.Remove(filter);
		}
		if (flag)
		{
			SaveData.Instance.SaveGlobalData();
			OnCombatLogFiltersChanged.Invoke();
		}
	}

	public void ToggleCombatLog(bool enable)
	{
		SaveData.Instance.Global.DisabledCombatLog = !enable;
		SaveData.Instance.SaveGlobalData();
		for (int i = 0; i < filterSettings.Length; i++)
		{
			filterSettings[i].gameObject.SetActive(enable);
		}
		if (enable)
		{
			OnEnableCombatLog.Invoke();
		}
		else
		{
			OnDisableCombatLog.Invoke();
		}
	}
}

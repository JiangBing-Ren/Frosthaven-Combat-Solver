using I2.Loc;
using ScenarioRuleLibrary;
using UnityEngine;

public abstract class LocalizedListener : MonoBehaviour
{
	[StateDump(Skip = true)]
	private string lastLanguage;

	protected virtual void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
		if (lastLanguage != null && lastLanguage != LocalizationManager.CurrentLanguage)
		{
			OnLanguageChanged();
		}
	}

	protected virtual void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			lastLanguage = LocalizationManager.CurrentLanguage;
			LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
		}
	}

	protected abstract void OnLanguageChanged();
}

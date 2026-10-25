using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Gloomhaven;

public class LanguageSettings : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown m_LanguageDropdown;

	[SerializeField]
	private List<string> m_IgnoredLanguages;

	[SerializeField]
	private List<string> m_UsedLanguages;

	private SelectorWrapper<string> m_LanguageSelector;

	private void Awake()
	{
		m_LanguageSelector = new SelectorWrapper<string>(m_LanguageDropdown, null);
		m_LanguageSelector.OnValuedChanged.AddListener(SetLanguage);
	}

	private void OnEnable()
	{
		m_LanguageDropdown.interactable = SaveData.Instance?.Global != null && SaveData.Instance.Global.GameMode == EGameMode.MainMenu;
		m_LanguageDropdown.captionText.color = (m_LanguageDropdown.interactable ? UIInfoTools.DefInstance.basicTextColor : UIInfoTools.DefInstance.greyedOutTextColor);
		RefreshDropdown();
	}

	public void RefreshDropdown()
	{
		List<string> allLanguages = I2.Loc.LocalizationManager.GetAllLanguages().ToList();
		List<string> list = m_UsedLanguages.Where((string used) => allLanguages.Any((string l) => used.Equals(l, StringComparison.InvariantCulture))).ToList();
		List<SelectorOptData<string>> options = list.ConvertAll((string it) => new SelectorOptData<string>(it, () => (!GLOOM.LocalizationManager.TryGetTranslation(it, out var translation)) ? it : translation));
		m_LanguageSelector.SetOptions(options);
		m_LanguageDropdown.SetValueWithoutNotify(list.IndexOf(I2.Loc.LocalizationManager.CurrentLanguage));
	}

	public void SetLanguage(string language)
	{
		if (!(I2.Loc.LocalizationManager.CurrentLanguage == language) && I2.Loc.LocalizationManager.HasLanguage(language))
		{
			List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
			{
				new ErrorMessage.LabelAction("GUI_CANCEL", () =>
				{
					m_LanguageSelector.SetValueWithoutNotify(I2.Loc.LocalizationManager.CurrentLanguage);
				}, InputFHManager.Controls.Generic.Back),
				new ErrorMessage.LabelAction("GUI_APPLY_AND_LOAD", () =>
				{
					I2.Loc.LocalizationManager.CurrentLanguage = language;
					SaveData.Instance.RootData.CurrentLanguage = language;
					SaveData.Instance.SaveRootData();
					StartCoroutine(SceneController.Instance.ReloadGlobalCardAssets());
					RefreshDropdown();
				}, InputFHManager.Controls.Generic.AlternativeChoice)
			};
			SceneController.Instance.GlobalErrorMessage.ShowGenericMessage("GUI_OPT_LANG_TITLE", "GUI_OPT_LANG_APPLY", Environment.StackTrace, buttons);
		}
	}

	private void OnDisable()
	{
		if (SceneController.Instance.GlobalErrorMessage.ShowingMessage && SceneController.Instance.GlobalErrorMessage.Currentkey == "GUI_OPT_LANG_TITLE")
		{
			SceneController.Instance.GlobalErrorMessage.CloseErrorMessage();
		}
	}
}

using System;
using System.Collections.Generic;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIRulesetView : MonoBehaviour
{
	[Serializable]
	public class RulesetEvent : UnityEvent<RulesetDataView>
	{
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private List<UIRulesetModSlot> slotPool;

	[SerializeField]
	private ScrollRect modsScroll;

	[SerializeField]
	private TMP_InputField nameInput;

	[SerializeField]
	private GameObject nameContainer;

	[SerializeField]
	private ExtendedButton confirmButton;

	[SerializeField]
	private GameObject validationMask;

	public RulesetEvent OnConfirmedRulesetEvent = new RulesetEvent();

	private Dictionary<IMod, UIRulesetModSlot> assignedMods = new Dictionary<IMod, UIRulesetModSlot>();

	private RulesetDataView rulesetDataView;

	private IRulesetModService moddingService;

	private void Awake()
	{
		confirmButton.onClick.AddListener(ConfirmEdit);
		nameInput.contentType = TMP_InputField.ContentType.Alphanumeric;
		nameInput.onValueChanged.AddListener(OnUpdatedName);
	}

	public void Setup(IRulesetModService moddingService)
	{
		this.moddingService = moddingService;
		CreateMods(moddingService.GetMods());
	}

	private void CreateMods(List<IMod> mods)
	{
		assignedMods.Clear();
		HelperTools.NormalizePool(ref slotPool, slotPool[0].gameObject, modsScroll.content, mods.Count);
		for (int i = 0; i < mods.Count; i++)
		{
			IMod mod = mods[i];
			UIRulesetModSlot uIRulesetModSlot = slotPool[i];
			uIRulesetModSlot.Hide();
			assignedMods[mod] = uIRulesetModSlot;
			uIRulesetModSlot.SetMod(mod, OnToggledMod, ValidateMod);
		}
	}

	private void ValidateMod(IMod mod)
	{
		validationMask.SetActive(value: true);
		moddingService.ValidateMod(mod, () =>
		{
			validationMask.SetActive(value: false);
			RefreshValidSelectedMod(mod);
			if (mod.IsValid)
			{
				UIModdingNotifications.ShowValidatedNotification(mod.Name);
			}
			else
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_MOD_FAIL_CONFIRMATION"), () =>
				{
					moddingService.ViewModErrors(mod);
				}, null, "GUI_MODDING_VALIDATION_VIEW_MOD_ERRORS", "GUI_CLOSE", UIInfoTools.Instance.warningColor);
			}
		});
	}

	public void RefreshValidSelectedMods()
	{
		rulesetDataView.SetUnvalidMods(rulesetDataView.Mods.FindAll((IMod it) => !it.IsValid));
		foreach (IMod mod in rulesetDataView.Mods)
		{
			assignedMods[mod].SetValid(mod.IsValid);
		}
		CheckRulebaseValid();
	}

	public void RefreshValidSelectedMod(IMod mod)
	{
		if (!rulesetDataView.Mods.Contains(mod))
		{
			return;
		}
		if (!mod.IsValid)
		{
			if (rulesetDataView.UnvalidMods.Add(mod))
			{
				assignedMods[mod].SetValid(isValid: false);
				CheckRulebaseValid();
			}
		}
		else if (rulesetDataView.UnvalidMods.Remove(mod))
		{
			assignedMods[mod].SetValid(isValid: true);
			CheckRulebaseValid();
		}
	}

	public void Hide()
	{
		window.Hide();
	}

	public void Show(IRuleset ruleset)
	{
	}

	public void Show(RulesetDataView ruleset)
	{
		rulesetDataView = ruleset;
		modsScroll.verticalNormalizedPosition = 1f;
		nameContainer.SetActive(value: false);
		confirmButton.gameObject.SetActive(value: false);
		foreach (KeyValuePair<IMod, UIRulesetModSlot> mod in assignedMods)
		{
			int num = rulesetDataView.Mods.FindIndex((IMod it) => it.Name == mod.Key.Name);
			if (num >= 0)
			{
				mod.Value.Show(isSelected: true, enableSelection: false, num + 1);
				mod.Value.SetValid(!rulesetDataView.UnvalidMods.Contains(mod.Key));
			}
			else
			{
				mod.Value.Hide();
			}
		}
		window.Show();
	}

	private void OnUpdatedName(string rulesetName)
	{
		rulesetDataView.Name = rulesetName;
		CheckRulebaseValid();
	}

	private void OnToggledMod(IMod mod, bool selected)
	{
		if (selected)
		{
			rulesetDataView.Mods.Add(mod);
			assignedMods[mod].SetOrder(rulesetDataView.Mods.Count);
		}
		else
		{
			if (rulesetDataView.UnvalidMods.Remove(mod))
			{
				assignedMods[mod].SetValid(isValid: true);
			}
			rulesetDataView.Mods.Remove(mod);
			for (int i = 0; i < rulesetDataView.Mods.Count; i++)
			{
				assignedMods[rulesetDataView.Mods[i]].SetOrder(i + 1);
			}
			assignedMods[mod].SetOrder(null);
		}
		CheckRulebaseValid();
	}

	private void CheckRulebaseValid()
	{
		confirmButton.interactable = rulesetDataView.IsValid();
		confirmButton.buttonText.CrossFadeColor(confirmButton.interactable ? UIInfoTools.Instance.White : UIInfoTools.DefInstance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
	}

	public void Edit(RulesetDataView ruleset, List<IMod> compatibleMods, string confirmButtonKey = "GUI_MODDING_FINISH_EDIT_RULESET")
	{
		rulesetDataView = ruleset;
		modsScroll.verticalNormalizedPosition = 1f;
		confirmButton.TextLanguageKey = confirmButtonKey;
		confirmButton.gameObject.SetActive(value: true);
		nameInput.text = rulesetDataView.Name;
		nameContainer.SetActive(value: true);
		CheckRulebaseValid();
		window.Show();
	}

	private void ConfirmEdit()
	{
		if (rulesetDataView.IsValid())
		{
			OnConfirmedRulesetEvent?.Invoke(rulesetDataView);
		}
	}
}

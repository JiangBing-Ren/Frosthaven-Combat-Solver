using System;
using Assets.Script.GUI.MainMenu.Modding;
using GLOOM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIRulesetsWindow : MonoBehaviour
{
	[Header("Manage options")]
	[SerializeField]
	private Button createRulesetButton;

	[SerializeField]
	private ExtendedButton playRulesetButton;

	[SerializeField]
	private Button editRulesetButton;

	[SerializeField]
	private Button deleteRulesetButton;

	[SerializeField]
	private Button compileRulesetButton;

	[SerializeField]
	private GameObject rulesetManageOptionsPanel;

	[SerializeField]
	private GameObject PlayCampaignRuleset;

	[SerializeField]
	private GameObject PlayGuildmasterRuleset;

	[Header("Screen elements")]
	[SerializeField]
	private ExtendedButton closeButton;

	[SerializeField]
	private TextLocalizedListener subtitle;

	[SerializeField]
	private UIRulesetInventory rulesetInventory;

	[SerializeField]
	private UIRulesetView rulesetView;

	[SerializeField]
	private Image unfocusedMask;

	[SerializeField]
	private UIBackgroundTaskMask validationMask;

	[Header("Events")]
	[SerializeField]
	private UnityEvent OnClosed = new UnityEvent();

	private IRulesetService service = new RulesetService();

	private Action closeButtonCallback;

	private void Awake()
	{
		closeButton.onClick.AddListener(() =>
		{
			closeButtonCallback?.Invoke();
		});
		playRulesetButton.onClick.AddListener(PlaySelectedRuleset);
		editRulesetButton.onClick.AddListener(OpenSelectedRulesetEditView);
		createRulesetButton.onClick.AddListener(ChooseRulesetTypeToCreate);
		deleteRulesetButton.onClick.AddListener(AskDeleteRuleset);
		compileRulesetButton.onClick.AddListener(CompileSelectedRuleset);
		rulesetInventory.OnHoveredRuleset.AddListener(PreviewRuleset);
		rulesetInventory.OnUnhoveredRuleset.AddListener((IRuleset _) =>
		{
			HideRulesetView();
		});
		rulesetInventory.OnSelectedRuleset.AddListener(OpenVisualizeRulesetView);
		rulesetInventory.OnDeselectedRuleset.AddListener((IRuleset _) =>
		{
			HideRulesetView();
		});
		rulesetView.OnConfirmedRulesetEvent.AddListener(OnConfirmedRulesetChanges);
	}

	public void Show()
	{
		rulesetView.Setup(SceneController.Instance.ModService);
		rulesetInventory.Setup(service.GetRulesets());
		CloseEditCreateRulesetView();
		gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void Close()
	{
		if (gameObject.activeSelf)
		{
			Hide();
			OnClosed.Invoke();
		}
	}

	private void FocusOptions(bool focus)
	{
		unfocusedMask.gameObject.SetActive(!focus);
	}

	private void PlaySelectedRuleset()
	{
		PlayRuleset(rulesetInventory.SelectedRuleset);
	}

	private void PlayRuleset(IRuleset ruleset)
	{
	}

	private void PreviewRuleset(IRuleset ruleset)
	{
		rulesetView.Show(ruleset);
		rulesetManageOptionsPanel.SetActive(value: false);
	}

	private void HideRulesetView()
	{
		rulesetView.Hide();
		rulesetManageOptionsPanel.SetActive(value: false);
	}

	private void OpenVisualizeRulesetView(IRuleset ruleset)
	{
		playRulesetButton.interactable = ruleset.IsCompiled;
		rulesetView.Show(ruleset);
		rulesetManageOptionsPanel.SetActive(value: true);
	}

	private void OpenSelectedRulesetEditView()
	{
		OpenRulesetEditView(rulesetInventory.SelectedRuleset);
	}

	private void OpenRulesetEditView(IRuleset ruleset)
	{
		subtitle.SetTextKey("GUI_MODDING_EDIT_RULESET");
	}

	private void ChooseRulesetTypeToCreate()
	{
	}

	private void OpenEditCreateRulesetView(RulesetDataView viewData, string confirmationButtonKey, string confirmationCancelKey)
	{
		FocusOptions(focus: false);
		editRulesetButton.gameObject.SetActive(value: false);
		playRulesetButton.gameObject.SetActive(value: false);
		compileRulesetButton.gameObject.SetActive(value: false);
		rulesetManageOptionsPanel.SetActive(value: true);
		closeButton.TextLanguageKey = "GUI_CANCEL";
		closeButtonCallback = () =>
		{
			if (viewData.IsModified)
			{
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation(confirmationCancelKey), LocalizationManager.GetTranslation("GUI_MODDING_RULESET_LOSE_CHANGES_CONFIRMATION"), UIInfoTools.Instance.warningColor, UIInfoTools.DefInstance.descriptionTextColor, CloseEditCreateRulesetView);
			}
			else
			{
				CloseEditCreateRulesetView();
			}
		};
	}

	private void CloseEditCreateRulesetView()
	{
		FocusOptions(focus: true);
		closeButton.TextLanguageKey = "GUI_BACK";
		closeButtonCallback = Close;
		subtitle.SetTextKey("GUI_MODDING_LIST_RULESETS");
		editRulesetButton.gameObject.SetActive(value: true);
		deleteRulesetButton.gameObject.SetActive(value: true);
		playRulesetButton.gameObject.SetActive(value: true);
		compileRulesetButton.gameObject.SetActive(value: true);
		if (rulesetInventory.SelectedRuleset != null)
		{
			OpenVisualizeRulesetView(rulesetInventory.SelectedRuleset);
		}
		else
		{
			HideRulesetView();
		}
	}

	private void OnConfirmedRulesetChanges(RulesetDataView rulesetData)
	{
		if (rulesetInventory.SelectedRuleset == null)
		{
			CreateRuleset(rulesetData);
			return;
		}
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_FINISH_EDIT_RULESET"), LocalizationManager.GetTranslation("GUI_MODDING_FINISH_EDIT_RULESET_CONFIRMATION"), () =>
		{
			UpdateRuleset(rulesetInventory.SelectedRuleset, rulesetData);
		});
	}

	private void UpdateRuleset(IRuleset oldRuleset, RulesetDataView rulesetData)
	{
		validationMask.Show("GUI_MODDING_VALIDATING");
		service.UpdateRuleset(oldRuleset, rulesetData, (IRuleset newRuleset) =>
		{
			validationMask.Hide();
			UIModdingNotifications.ShowValidatedNotification(newRuleset.Name);
			rulesetInventory.Replace(oldRuleset, newRuleset);
			CloseEditCreateRulesetView();
		}, OnValidationFailed);
	}

	private void CreateRuleset(RulesetDataView rulesetData)
	{
		validationMask.Show("GUI_MODDING_VALIDATING");
		service.CreateRuleset(rulesetData, (IRuleset ruleset) =>
		{
			validationMask.Hide();
			UIModdingNotifications.ShowCreateRulesetNotification(ruleset.Name);
			rulesetInventory.Add(ruleset);
			CloseEditCreateRulesetView();
		}, OnValidationFailed);
	}

	private void OnValidationFailed()
	{
		validationMask.Hide();
		rulesetView.RefreshValidSelectedMods();
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_RULESET_FAIL_CONFIRMATION"), UIInfoTools.Instance.warningColor, "GUI_CLOSE");
	}

	protected void CompileSelectedRuleset()
	{
		CompileRuleset(rulesetInventory.SelectedRuleset);
	}

	protected void CompileRuleset(IRuleset ruleset)
	{
		validationMask.Show("GUI_MODDING_COMPILING");
		service.CompileRuleset(ruleset, (bool isValid) =>
		{
			validationMask.Hide();
			if (isValid)
			{
				UIModdingNotifications.ShowValidatedNotification(ruleset.Name);
				playRulesetButton.interactable = true;
			}
			else
			{
				playRulesetButton.interactable = false;
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_FAIL_CONFIRMATION_TITLE"), LocalizationManager.GetTranslation("GUI_MODDING_VALIDATION_RULESET_FAIL_CONFIRMATION"), UIInfoTools.Instance.warningColor, "GUI_CLOSE");
			}
		});
	}

	private void AskDeleteRuleset()
	{
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION"), LocalizationManager.GetTranslation("GUI_MODDING_DELETE_RULESET"), () =>
		{
			DeleteRuleset(rulesetInventory.SelectedRuleset);
		}, UIInfoTools.Instance.warningColor);
	}

	private void DeleteRuleset(IRuleset ruleset)
	{
		service.DeleteRuleset(ruleset);
		UIModdingNotifications.ShowDeleteRulesetNotification(ruleset.Name);
		rulesetInventory.Remove(ruleset);
		CloseEditCreateRulesetView();
	}
}

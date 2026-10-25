using System;
using System.Text;
using FFSNet;
using GLOOM;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIConfirmationBoxManager : Singleton<UIConfirmationBoxManager>
{
	public class CheckboxParams
	{
		public UnityAction<bool> OnCheckboxToggle;

		public bool StartingValue;

		public string Text;
	}

	[SerializeField]
	private UIWindow confirmationBox;

	[Space]
	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Localize confirmButtonText;

	[SerializeField]
	private Button cancelButton;

	[SerializeField]
	private Localize cancelButtonText;

	[SerializeField]
	private GameObject header;

	[Header("PRIMARY CONTENT")]
	[SerializeField]
	private GameObject primaryContent;

	[SerializeField]
	private TMP_Text primaryContentText;

	[Header("SECONDARY CONTENT", order = 1)]
	[Header("Price content", order = 2)]
	[SerializeField]
	private GameObject secondaryContent;

	[SerializeField]
	private TMP_Text secondaryContentFirstText;

	[SerializeField]
	private TMP_Text secondaryContentSecondText;

	[Header("General secondary")]
	[SerializeField]
	private GameObject secondaryGeneralContent;

	[SerializeField]
	private TMP_Text secondaryGeneralContentText;

	[Header("Report")]
	[SerializeField]
	private GameObject reportContent;

	[SerializeField]
	private Button reportButton;

	[Header("Checkbox")]
	[SerializeField]
	private GameObject checkboxContent;

	[SerializeField]
	private Toggle checkboxToggle;

	[SerializeField]
	private TMP_Text checkboxText;

	private Action onHidden;

	private bool usingAdventureStartConfirmation;

	public bool IsOpen
	{
		get
		{
			if (confirmationBox != null)
			{
				return confirmationBox.IsOpen;
			}
			return false;
		}
	}

	private void Start()
	{
		confirmationBox.IsPopUp = true;
		ResetConfirmationBox();
	}

	private void SetupReport(UnityAction onClicked)
	{
		reportContent.SetActive(value: true);
		reportButton.onClick.AddListener(() =>
		{
			ShowSendErrorWrapper("USER_REPORTS", (bool hasSendReport) =>
			{
				if (hasSendReport)
				{
					onClicked?.Invoke();
					Hide();
				}
			});
		});
	}

	public void ShowSendErrorWrapper(string errorMessageKey, UnityAction<bool> onFinished = null)
	{
		Hide(instant: true);
		SceneController.Instance.GlobalErrorMessage.ShowSendErrorReport(errorMessageKey, onFinished, takeScreenshot: false);
	}

	public void ShowGenericCancelConfirmation(string title, string explanation, Color? titleColor = null, string buttonKey = "GUI_CANCEL", UnityAction onFinished = null, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(title, explanation, titleColor ?? UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, null, onFinished, enableSoftlockReport: false, checkboxParams);
		confirmButton.gameObject.SetActive(value: false);
		cancelButtonText.SetTerm(buttonKey);
		header.SetActive(value: false);
	}

	public void ShowGenericInfo(string title, string explanation, Color? titleColor = null, string buttonKey = "GUI_CANCEL", UnityAction onFinished = null, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(title, explanation, titleColor ?? UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, null, onFinished, enableSoftlockReport: false, checkboxParams);
		confirmButton.gameObject.SetActive(value: false);
		cancelButtonText.SetTerm(buttonKey);
	}

	public void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, Color? titleColor = null, bool enableSoftlockReport = false, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(title, explanation, titleColor ?? UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, onActionConfirmed, enableSoftlockReport, checkboxParams);
	}

	public void ShowGenericConfirmation(string title, string explanation, Color titleColor, Color explanationColor, UnityAction onActionConfirmed, bool enableSoftlockReport = false, CheckboxParams checkboxParams = null)
	{
		ResetConfirmationBox();
		primaryContentText.text = title;
		primaryContentText.color = titleColor;
		if (explanation != null)
		{
			secondaryGeneralContentText.text = explanation;
			secondaryGeneralContentText.color = explanationColor;
			secondaryGeneralContent.SetActive(value: true);
		}
		else
		{
			secondaryGeneralContent.SetActive(value: false);
		}
		if (enableSoftlockReport)
		{
			SetupReport(onActionConfirmed);
		}
		if (checkboxParams != null)
		{
			checkboxContent.SetActive(value: true);
			checkboxText.text = checkboxParams.Text;
			checkboxToggle.onValueChanged.RemoveAllListeners();
			checkboxToggle.isOn = checkboxParams.StartingValue;
			checkboxToggle.onValueChanged.AddListener(checkboxParams.OnCheckboxToggle);
		}
		else
		{
			checkboxContent.SetActive(value: false);
		}
		confirmButton.onClick.AddListener(onActionConfirmed);
		confirmButton.onClick.AddListener(Hide);
		cancelButton.onClick.AddListener(Hide);
		confirmationBox.Show();
	}

	public void ShowGenericConfirmation(string explanation, UnityAction onActionConfirmed, Color? textColor = null, bool enableReportSoftlock = false, UnityAction onCancelled = null, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(null, explanation, textColor.GetValueOrDefault(UIInfoTools.DefInstance.neutralActionTextColor), UIInfoTools.DefInstance.neutralActionTextColor, onActionConfirmed, onCancelled, enableReportSoftlock, checkboxParams);
	}

	public void ShowGenericConfirmation(string title, string explanation, UnityAction onActionConfirmed, UnityAction onActionCancelled, string confirmButtonKey = null, string cancelButtonKey = null, Color? titleColor = null, bool showHeader = true, bool enableSoftlockReport = false, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(title, explanation, titleColor ?? UIInfoTools.DefInstance.basicTextColor, UIInfoTools.DefInstance.neutralActionTextColor, onActionConfirmed, onActionCancelled, enableSoftlockReport, checkboxParams);
		if (confirmButtonKey.IsNOTNullOrEmpty())
		{
			confirmButtonText.SetTerm(confirmButtonKey);
		}
		if (cancelButtonKey.IsNOTNullOrEmpty())
		{
			cancelButtonText.SetTerm(cancelButtonKey);
		}
		header.SetActive(showHeader);
	}

	public void ShowGenericConfirmation(string title, string explanation, Color titleColor, Color explanationColor, UnityAction onActionConfirmed, UnityAction onActionCancelled, bool enableSoftlockReport = false, CheckboxParams checkboxParams = null)
	{
		ShowGenericConfirmation(title, explanation, titleColor, explanationColor, () =>
		{
			onHidden = null;
			onActionConfirmed?.Invoke();
		}, enableSoftlockReport, checkboxParams);
		if (onActionCancelled != null)
		{
			cancelButton.onClick.AddListener(() =>
			{
				onHidden = null;
				onActionCancelled();
			});
			onHidden = onActionCancelled.Invoke;
		}
	}

	public void ShowGenericSpendConfirmation(string title, int price, UnityAction onActionConfirmed, Color titleColor, UnityAction onActionCancelled = null)
	{
		ResetConfirmationBox();
		primaryContentText.text = title;
		primaryContentText.color = titleColor;
		secondaryContentFirstText.text = GLOOM.LocalizationManager.GetTranslation("GUI_CONFIRMATION_BUY_COST");
		secondaryContentSecondText.text = price.ToString();
		secondaryContent.SetActive(value: true);
		confirmButton.onClick.AddListener(() =>
		{
			onHidden = null;
			onActionConfirmed?.Invoke();
			Hide();
		});
		cancelButton.onClick.AddListener(Hide);
		if (onActionCancelled != null)
		{
			onHidden = onActionCancelled.Invoke;
		}
		confirmationBox.Show();
	}

	public void ShowCancelActiveAbility(string abilityName, bool lost, UnityAction onActionConfirmed, string explanation = null, UnityAction onCancelled = null)
	{
		StringBuilder stringBuilder = new StringBuilder(string.Format(GLOOM.LocalizationManager.GetTranslation(lost ? "GUI_CONFIRMATION_CANCEL_ABILITY" : "GUI_CONFIRMATION_CANCEL_DISCARD_ABILITY"), abilityName));
		if (explanation.IsNOTNullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<color=#{0}>{1}</color>", UIInfoTools.DefInstance.basicTextColor.ToHex(), explanation);
		}
		ShowGenericConfirmation(GLOOM.LocalizationManager.GetTranslation("GUI_CANCEL") + " " + abilityName, stringBuilder.ToString(), UIInfoTools.DefInstance.neutralActionTextColor, UIInfoTools.DefInstance.neutralActionTextColor, onActionConfirmed, onCancelled);
	}

	public void SetButtonsInteraction(bool interactable)
	{
		confirmButton.interactable = interactable;
		cancelButton.interactable = interactable;
	}

	public void ProxyAnswer(GameAction gameAction)
	{
		if (gameAction.SupplementaryDataBoolean)
		{
			confirmButton.onClick.Invoke();
		}
		else
		{
			cancelButton.onClick.Invoke();
		}
	}

	private void ResetConfirmationBox()
	{
		cancelButtonText.SetTerm("GUI_CANCEL");
		confirmButtonText.SetTerm("GUI_CONFIRM");
		primaryContent.SetActive(value: true);
		secondaryGeneralContent.SetActive(value: false);
		secondaryContent.SetActive(value: false);
		secondaryGeneralContentText.color = UIInfoTools.DefInstance.neutralActionTextColor;
		reportContent.SetActive(value: false);
		confirmButton.gameObject.SetActive(value: true);
		header.SetActive(value: true);
		SetButtonsInteraction(interactable: true);
		Unsubscribe();
	}

	private void Unsubscribe()
	{
		confirmButton.onClick.RemoveAllListeners();
		cancelButton.onClick.RemoveAllListeners();
		confirmationBox.onHidden.RemoveAllListeners();
		confirmationBox.onHidden.AddListener(OnHidden);
		reportButton.onClick.RemoveAllListeners();
		checkboxToggle.onValueChanged.RemoveAllListeners();
		onHidden = null;
	}

	public void OnHidden()
	{
		if (usingAdventureStartConfirmation)
		{
			usingAdventureStartConfirmation = false;
		}
		onHidden?.Invoke();
	}

	public void Hide()
	{
		Hide(instant: false);
	}

	public void Hide(bool instant)
	{
		Unsubscribe();
		confirmationBox.Hide(instant);
		SetButtonsInteraction(interactable: true);
	}
}

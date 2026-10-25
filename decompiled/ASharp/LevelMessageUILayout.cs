using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary.CustomLevels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelMessageUILayout : LocalizedListener
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Transform pageContainer;

	[SerializeField]
	private List<LevelMessagePageUI> pagesUI;

	[SerializeField]
	public PaginationHandler pagination;

	[SerializeField]
	public ExtendedButton closeButton;

	[SerializeField]
	private string audioItemShowMessage;

	[SerializeField]
	private ExtendedButton fullScreenCloseButton;

	[SerializeField]
	private GameObject dismissIcon;

	private CLevelMessage message;

	private Action onCloseButtonPressed;

	private bool triggeredByDismiss;

	private int focusedFrame;

	public ExtendedButton closeButtonToUse
	{
		get
		{
			if (fullScreenCloseButton != null)
			{
				return fullScreenCloseButton;
			}
			return closeButton;
		}
	}

	public void Init(CLevelMessage message, Action onClose = null, bool playOpenSound = false)
	{
		if (closeButton != null)
		{
			closeButton.gameObject.SetActive(value: false);
		}
		if (fullScreenCloseButton != null)
		{
			fullScreenCloseButton.gameObject.SetActive(value: false);
		}
		if (dismissIcon != null)
		{
			dismissIcon.SetActive(value: false);
		}
		triggeredByDismiss = message.DismissTrigger.IsTriggeredByDismiss;
		if (triggeredByDismiss && closeButtonToUse != null)
		{
			if (dismissIcon != null)
			{
				dismissIcon.SetActive(value: true);
			}
			closeButtonToUse.gameObject.SetActive(value: true);
			closeButtonToUse.TextLanguageKey = "GUI_CONTINUE";
			closeButtonToUse.onClick.RemoveAllListeners();
			closeButtonToUse.onClick.AddListener(CloseButtonPressed);
			if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.FixedLowerRight)
			{
				title.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>().padding.bottom = 40;
			}
		}
		else if (message.LayoutType == CLevelMessage.ELevelMessageLayoutType.FixedLowerRight)
		{
			title.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
		}
		Image component = GetComponent<Image>();
		if (component != null)
		{
			component.enabled = message.ShowScreenBG;
		}
		onCloseButtonPressed = onClose;
		this.message = message;
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged -= OnControlSchemeChanged;
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged += OnControlSchemeChanged;
		if (!message.TitleKey.IsNullOrEmpty())
		{
			if (message.TitleKeyController.IsNullOrEmpty())
			{
				title.text = LocalizationManager.GetTranslation(message.TitleKey);
			}
			else
			{
				title.text = (Singleton<InputFHManager>.Instance.UsingGamepad ? LocalizationManager.GetTranslation(message.TitleKeyController) : LocalizationManager.GetTranslation(message.TitleKey));
			}
			title.gameObject.SetActive(value: true);
		}
		else
		{
			title.gameObject.SetActive(value: false);
		}
		if (message.Pages.Count > 0 && pagination != null)
		{
			PaginationHandler paginationHandler = pagination;
			paginationHandler.OnPageChanged = (Action<int, int>)Delegate.Combine(paginationHandler.OnPageChanged, new Action<int, int>(OnPageChanged));
			CreatePages(message.Pages);
		}
		if (playOpenSound)
		{
			AudioControllerUtils.PlaySound(audioItemShowMessage);
		}
		gameObject.SetActive(value: true);
	}

	private void OnControlSchemeChanged()
	{
		OnLanguageChanged();
		if (pagination != null)
		{
			for (int i = 0; i < message.Pages.Count && pagesUI[i].gameObject.activeSelf; i++)
			{
				pagesUI[i].RefreshText();
			}
		}
	}

	private void CreatePages(List<CLevelMessagePage> pages)
	{
		HelperTools.NormalizePool(ref pagesUI, pagesUI[0].gameObject, pageContainer, pages.Count);
		for (int i = 0; i < pages.Count; i++)
		{
			pagesUI[i].transform.SetSiblingIndex(1);
			pagesUI[i].Init(pages[i]);
		}
		pagination.Init((from it in pagesUI
			where it.gameObject.activeSelf
			select it.gameObject).ToList(), fullScreenCloseButton != closeButton && triggeredByDismiss);
	}

	public void OnPageChanged(int pageChangedTo, int maxPages)
	{
	}

	public void CloseButtonPressed()
	{
		if (gameObject.activeSelf)
		{
			if (pagination.currentPage < pagination.pages.Count)
			{
				pagination.GoNextPage();
			}
			else
			{
				onCloseButtonPressed?.Invoke();
			}
		}
	}

	protected override void OnLanguageChanged()
	{
		if (title != null && message != null && title.gameObject.activeSelf)
		{
			if (Singleton<InputFHManager>.Instance.UsingGamepad && message.TitleKeyController.IsNOTNullOrEmpty())
			{
				title.text = LocalizationManager.GetTranslation(message.TitleKeyController);
			}
			else
			{
				title.text = LocalizationManager.GetTranslation(message.TitleKey);
			}
		}
	}

	public void Hide()
	{
		Clear();
		gameObject.SetActive(value: false);
	}

	public void Clear()
	{
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged -= OnControlSchemeChanged;
	}

	protected override void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			Clear();
			base.OnDisable();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (title != null)
		{
			StartCoroutine(WaitRegenerateSubmeshes());
		}
	}

	private IEnumerator WaitRegenerateSubmeshes()
	{
		yield return null;
		RefreshSubmeshes();
	}

	private void RefreshSubmeshes()
	{
		TMP_SubMeshUI[] componentsInChildren = title.GetComponentsInChildren<TMP_SubMeshUI>();
		foreach (TMP_SubMeshUI tMP_SubMeshUI in componentsInChildren)
		{
			if (!(tMP_SubMeshUI.spriteAsset == null))
			{
				tMP_SubMeshUI.transform.SetParent(title.transform.parent);
				tMP_SubMeshUI.transform.SetSiblingIndex(title.transform.GetSiblingIndex());
			}
		}
	}
}

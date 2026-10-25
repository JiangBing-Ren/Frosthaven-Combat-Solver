using System;
using System.Collections.Generic;
using DevLocker.GFrame.Input.UIScope;
using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogPopup : MonoBehaviour
{
	private struct PreviousState
	{
		private bool wasActive;

		private Transform contentHolder;

		public GameObject GameObject;

		private Vector3 position;

		private Vector2 minAnchor;

		private Vector2 maxAnchor;

		public PreviousState(GameObject obj)
		{
			GameObject = obj;
			wasActive = obj.activeSelf;
			contentHolder = obj.transform.parent;
			position = obj.transform.localPosition;
			RectTransform component = obj.GetComponent<RectTransform>();
			minAnchor = component.anchorMin;
			maxAnchor = component.anchorMax;
		}

		public void ApplyPreviousState()
		{
			GameObject.SetActive(wasActive);
			GameObject.transform.SetParent(contentHolder);
			RectTransform component = GameObject.GetComponent<RectTransform>();
			component.anchorMin = minAnchor;
			component.anchorMax = maxAnchor;
			GameObject.transform.localPosition = position;
		}
	}

	[SerializeField]
	private Transform contentHolder;

	[SerializeField]
	private Transform horizontalOptionsHolder;

	[SerializeField]
	private Transform verticalOptionsHolder;

	[SerializeField]
	private TextMeshProUGUI contentText;

	[SerializeField]
	private GameObject captionHolder;

	[SerializeField]
	private TextMeshProUGUI captionText;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private GameObject optionButtonPrefab;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIScope _scope;

	private List<FHButton> optionButtons = new List<FHButton>();

	private List<PreviousState> contentState = new List<PreviousState>();

	private int cancelOption = -1;

	private UnityAction cancelAction;

	private bool _isUsingHelpBox;

	private DialogOption[] _options;

	private bool isListeningForControllerChanges;

	public UIWindow Window => window;

	public void Show(GameObject content, DialogOption[] options, int cancelOption = -1, UnityAction cancelAction = null, string helpBoxText = null)
	{
		Show(new List<GameObject> { content }, options, cancelOption, cancelAction, helpBoxText);
	}

	public void Show(List<GameObject> content, DialogOption[] options, int cancelOption = -1, UnityAction cancelAction = null, string helpBoxText = null)
	{
		Log.GUIInfo("Show from:");
		if (IsOpen())
		{
			Hide();
		}
		contentState.Clear();
		contentText.gameObject.SetActive(value: false);
		this.cancelOption = cancelOption;
		this.cancelAction = cancelAction;
		foreach (GameObject item in content)
		{
			if (!(item == null))
			{
				contentState.Add(new PreviousState(item));
				item.SetActive(value: true);
				item.transform.SetParent(contentHolder);
			}
		}
		backgroundImage.enabled = false;
		captionHolder.SetActive(value: false);
		horizontalOptionsHolder.gameObject.SetActive(value: true);
		verticalOptionsHolder.gameObject.SetActive(value: false);
		InitOptions(options);
		if (window == null)
		{
			gameObject.SetActive(value: true);
		}
		else
		{
			window.Show();
		}
		if (helpBoxText.IsNOTNullOrEmpty())
		{
			_isUsingHelpBox = true;
			Singleton<HelpBox>.InstanceFast.ShowTranslated(helpBoxText);
		}
		else
		{
			_isUsingHelpBox = false;
		}
	}

	public void Show(string descriptionString, string captionString, DialogOption[] options, int cancelOption = -1)
	{
		Log.GUIInfo("show");
		this.cancelOption = cancelOption;
		contentText.gameObject.SetActive(value: true);
		contentText.text = descriptionString;
		backgroundImage.enabled = true;
		if (string.IsNullOrEmpty(captionString))
		{
			captionHolder.SetActive(value: false);
		}
		else
		{
			captionHolder.SetActive(value: true);
			captionText.text = captionString;
		}
		horizontalOptionsHolder.gameObject.SetActive(value: false);
		verticalOptionsHolder.gameObject.SetActive(value: true);
		InitOptions(options);
		if (window == null)
		{
			gameObject.SetActive(value: true);
		}
		else
		{
			window.Show();
		}
		_isUsingHelpBox = false;
	}

	public void Show(string descriptionText, DialogOption[] options, int cancelOption = -1)
	{
		Show(descriptionText, null, options, cancelOption);
	}

	private void InitOptions(DialogOption[] options)
	{
		_options = options;
		HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, horizontalOptionsHolder, options.Length);
		int num = 0;
		foreach (DialogOption option in options)
		{
			FHButton fHButton = optionButtons[num];
			fHButton.Text.text = option.text;
			fHButton.interactable = option.enabled;
			fHButton.gameObject.SetActive(option.visible);
			fHButton.RemoveAllEventListeners();
			fHButton.onClick.AddListener(() =>
			{
				Hide();
				option.onMouseClickAction?.Invoke();
			});
			HotkeyClickableScopeElement component = fHButton.GetComponent<HotkeyClickableScopeElement>();
			component.SetInputAction(InputActionReference.Create(option.InputAction));
			component.ApplyInputActionToChildren();
			_scope.TryAppendOwnedScopeElements(component.GetComponentsInChildren<IScopeElement>());
			if (option.onMouseEnterAction != null)
			{
				fHButton.HighlightBegin += option.onMouseEnterAction;
			}
			if (option.onMouseExitAction != null)
			{
				fHButton.HighlightEnd += option.onMouseExitAction;
			}
			InteractabilityIsolatedUIControl interactabilityIsolatedUIControl = fHButton.GetComponent<InteractabilityIsolatedUIControl>();
			if (interactabilityIsolatedUIControl == null && option.ControlType.HasValue)
			{
				interactabilityIsolatedUIControl = fHButton.gameObject.AddComponent<InteractabilityIsolatedUIControl>();
				interactabilityIsolatedUIControl.FHButtonsToAllow.Add(fHButton);
			}
			if (interactabilityIsolatedUIControl != null)
			{
				if (option.ControlType.HasValue)
				{
					interactabilityIsolatedUIControl.ControlType = option.ControlType.Value;
				}
				interactabilityIsolatedUIControl.ControlIdentifier = num.ToString();
				InteractabilityManager.RegisterControlForInteractionLimiting(interactabilityIsolatedUIControl);
			}
			if (fHButton.TryGetComponent<FHDialogButtonVisuals>(out var component2))
			{
				bool hasValue = option.VisualParams.HasValue;
				if (hasValue)
				{
					component2.InitVisuals(option.VisualParams.Value);
				}
				else
				{
					component2.ResetVisuals();
				}
				if (fHButton.TryGetComponent<UITextTooltipTarget>(out var component3))
				{
					component3.SetText(hasValue ? option.text : string.Empty);
				}
			}
			if (option.enabledForPlayer != null && !isListeningForControllerChanges)
			{
				isListeningForControllerChanges = true;
				ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
			}
			num++;
		}
		RecalcOptionsEnabled();
	}

	public void Hide(bool cleanContent = true)
	{
		foreach (PreviousState item in contentState)
		{
			item.ApplyPreviousState();
		}
		if (window == null)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			HelperTools.NormalizePool(ref optionButtons, optionButtonPrefab, verticalOptionsHolder, 0);
			window.Hide();
		}
		if (cleanContent)
		{
			TryHideHelpBox();
			contentState.Clear();
		}
		foreach (FHButton optionButton in optionButtons)
		{
			if (!(optionButton == null))
			{
				InteractabilityIsolatedUIControl component = optionButton.GetComponent<InteractabilityIsolatedUIControl>();
				if (component != null)
				{
					component.ControlIdentifier = string.Empty;
					InteractabilityManager.DeregisterControlForInteractionLimiting(component);
				}
			}
		}
		cancelOption = -1;
		_options = null;
	}

	public void TryHide()
	{
		Hide();
	}

	public void Cancel()
	{
		TryHideHelpBox();
		if (cancelOption >= 0 && cancelOption < optionButtons.Count)
		{
			FHButton fHButton = optionButtons[cancelOption];
			if (fHButton.gameObject.activeSelf)
			{
				ExecuteEvents.Execute(fHButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
			}
		}
		else if (cancelAction != null)
		{
			Hide();
			cancelAction();
		}
	}

	private void TryHideHelpBox()
	{
		if (_isUsingHelpBox)
		{
			Singleton<HelpBox>.InstanceFast.Hide();
			_isUsingHelpBox = false;
		}
	}

	public void ShowLoadedContent()
	{
		foreach (PreviousState item in contentState)
		{
			item.GameObject.transform.SetParent(contentHolder);
			item.GameObject.SetActive(value: true);
		}
		if (window == null)
		{
			gameObject.SetActive(value: true);
		}
		else
		{
			window.Show();
		}
	}

	public bool IsOpen()
	{
		if (window == null)
		{
			return gameObject.activeSelf;
		}
		return window.IsOpen;
	}

	public void ChooseOption(int index)
	{
		optionButtons[index].onClick.Invoke();
	}

	public void RecalcOptionsEnabled()
	{
		if (_options == null)
		{
			return;
		}
		for (int i = 0; i < _options.Length; i++)
		{
			DialogOption dialogOption = _options[i];
			if (dialogOption != null)
			{
				bool flag = dialogOption.enabled;
				if (flag && dialogOption.enabledForPlayer != null && !dialogOption.enabledForPlayer.IsUnderMyControl())
				{
					flag = false;
				}
				FHButton fHButton = optionButtons[i];
				if (fHButton.interactable != flag)
				{
					fHButton.interactable = flag;
				}
			}
		}
	}

	private void OnControllerChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		RecalcOptionsEnabled();
	}

	private void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		if (isListeningForControllerChanges)
		{
			isListeningForControllerChanges = false;
			ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		}
		if (!IsOpen())
		{
			return;
		}
		foreach (PreviousState item in contentState)
		{
			item.ApplyPreviousState();
		}
		contentState.Clear();
	}
}

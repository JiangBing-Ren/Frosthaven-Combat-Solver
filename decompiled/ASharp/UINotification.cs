using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINotification : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private TextMeshProUGUI information;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject iconContainer;

	[SerializeField]
	private RectTransform buttonContainer;

	[SerializeField]
	private ExtendedButton button1;

	[SerializeField]
	private ExtendedButton button2;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private bool disableOnHide;

	[SerializeField]
	private LayoutElementExtended layoutElement;

	[SerializeField]
	private LayoutGroup layoutGroup;

	[SerializeField]
	private string showNotificationAudioItem = "PlaySound_UIMultiNotifPing";

	[SerializeField]
	private RectTransform contentContainer;

	[Header("Animation")]
	[SerializeField]
	private RectTransform messageTransform;

	[SerializeField]
	private float showAnimationTime = 0.3f;

	[SerializeField]
	private float moveInPositionOffset = 20f;

	[SerializeField]
	private float hideAnimationTime = 0.3f;

	[SerializeField]
	private float srinkAnimationTime = 0.2f;

	private Tween moveAnimation;

	private string id;

	private bool isShown;

	private string currentAudioItem;

	private GameObject currentContent;

	public UnityEvent OnHidden = new UnityEvent();

	public string ID => id;

	public int EnabledButtons { get; private set; }

	public void Show(UINotificationManager.NotificationData data, string id = null)
	{
		CancelAnimation();
		isShown = true;
		this.id = id;
		currentAudioItem = (string.IsNullOrEmpty(data.audioItem) ? showNotificationAudioItem : data.audioItem);
		title.SetTextKey(data.titleLoc);
		if (data.message == null)
		{
			information.gameObject.SetActive(value: false);
		}
		else
		{
			information.text = data.message;
			information.gameObject.SetActive(value: true);
		}
		EnabledButtons = 0;
		DecorateButton(button1, data.dataButton1);
		DecorateButton(button2, data.dataButton2);
		buttonContainer.gameObject.SetActive(button1.gameObject.activeSelf || button2.gameObject.activeSelf);
		if (data.icon == null)
		{
			iconContainer.SetActive(value: false);
		}
		else
		{
			icon.sprite = data.icon;
			iconContainer.SetActive(value: true);
		}
		Clean();
		if (data.content != null)
		{
			currentContent = data.content;
			currentContent.transform.SetParent(contentContainer);
		}
		canvasGroup.interactable = true;
		gameObject.SetActive(value: true);
		StartCoroutine(WaitRebuild());
	}

	private void DecorateButton(ExtendedButton button, UINotificationManager.NotificationDataButton data)
	{
		button.onClick.RemoveAllListeners();
		if (data == null || data.clickCallback == null)
		{
			button.gameObject.SetActive(value: false);
			return;
		}
		button.TextLanguageKey = data.buttonTextLoc;
		button.onClick.AddListener(data.clickCallback.Invoke);
		button.gameObject.SetActive(value: true);
		EnabledButtons++;
	}

	private IEnumerator WaitRebuild()
	{
		if (currentContent != null)
		{
			yield return null;
		}
		yield return null;
		layoutGroup.enabled = false;
		layoutElement.preferredHeight = messageTransform.sizeDelta.y;
		messageTransform.anchorMin = Vector2.zero;
		messageTransform.anchorMax = Vector2.one;
		RectTransform rectTransform = messageTransform;
		Vector2 offsetMax = (messageTransform.offsetMin = Vector2.zero);
		rectTransform.offsetMax = offsetMax;
		messageTransform.anchoredPosition = new Vector2(moveInPositionOffset, messageTransform.anchoredPosition.y);
		AudioControllerUtils.PlaySound(currentAudioItem);
		moveAnimation = DOTween.To((float val) =>
		{
			canvasGroup.alpha = val;
			messageTransform.anchoredPosition = new Vector2(moveInPositionOffset * (1f - val), messageTransform.anchoredPosition.y);
		}, 0f, 1f, showAnimationTime).SetTarget(gameObject).OnComplete(() =>
		{
			canvasGroup.blocksRaycasts = true;
			moveAnimation = null;
		});
	}

	public void Hide(bool instant = false)
	{
		CancelAnimation();
		if (!isShown)
		{
			return;
		}
		isShown = false;
		if (instant)
		{
			_Hide();
			return;
		}
		canvasGroup.blocksRaycasts = false;
		moveAnimation = canvasGroup.DOFade(0f, hideAnimationTime).OnComplete(() =>
		{
			messageTransform.gameObject.SetActive(value: false);
			moveAnimation = DOTween.To((float val) =>
			{
				layoutElement.scalePreferredHeight = val;
			}, 1f, 0f, srinkAnimationTime).SetTarget(layoutElement.gameObject).OnComplete(() =>
			{
				moveAnimation = null;
				_Hide();
			});
		});
	}

	private void _Hide()
	{
		gameObject.SetActive(value: false);
		Clean();
		OnHidden.Invoke();
	}

	private void Clean()
	{
		if (currentContent != null && currentContent.transform.parent == contentContainer && currentContent.activeSelf)
		{
			currentContent.SetActive(value: false);
		}
		currentContent = null;
		layoutElement.scalePreferredHeight = 1f;
		layoutElement.preferredHeight = -1f;
		layoutElement.ignoreLayout = false;
		layoutGroup.enabled = true;
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		messageTransform.gameObject.SetActive(value: true);
	}

	private void CancelAnimation()
	{
		if (moveAnimation != null)
		{
			moveAnimation.Kill();
		}
		moveAnimation = null;
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			CancelAnimation();
		}
	}
}

using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
[StateDump(Skip = true)]
public class YesNoDialog : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Button yesButton;

	[SerializeField]
	private Button noButton;

	[SerializeField]
	private RectTransform box;

	public bool IsShown
	{
		get
		{
			if (window != null)
			{
				return window.IsVisible;
			}
			return false;
		}
	}

	public Button YesButton => yesButton;

	public Button NoButton => noButton;

	private void Awake()
	{
		window.IsPopUp = true;
	}

	public void Init(string descriptionKey, bool autoClose, UnityAction yesAction, UnityAction noAction = null)
	{
		SetDescriptionKey(descriptionKey);
		yesButton.onClick.RemoveAllListeners();
		noButton.onClick.RemoveAllListeners();
		window.onShown.RemoveAllListeners();
		window.onHidden.RemoveAllListeners();
		if (noAction != null)
		{
			window.onShown.AddListener(() =>
			{
				Log.GUIInfo("onShown");
				window.onHidden.AddListener(() =>
				{
					Log.GUIInfo("onHidden clean onhidden");
					window.onHidden.RemoveAllListeners();
					Log.GUIInfo("onHidden call no action");
					noAction();
				});
				yesButton.onClick.AddListener(() =>
				{
					Log.GUIInfo("Yes button clean onhidden");
					window.onHidden.RemoveAllListeners();
				});
				noButton.onClick.AddListener(() =>
				{
					Log.GUIInfo("No button clean onhidden");
					window.onHidden.RemoveAllListeners();
				});
			});
			noButton.onClick.AddListener(() =>
			{
				noAction();
				Log.GUIInfo("noButton call no action");
			});
		}
		yesButton.onClick.AddListener(yesAction);
		if (autoClose)
		{
			yesButton.onClick.AddListener(Hide);
			noButton.onClick.AddListener(Hide);
		}
	}

	public void SetDescriptionKey(string descriptionKey)
	{
		descriptionText.text = LocalizationManager.GetTranslation(descriptionKey);
	}

	public void PrepareInteractabilityForPlayer(string playerGUID)
	{
		InteractabilityIsolatedUIControl component = yesButton.GetComponent<InteractabilityIsolatedUIControl>();
		InteractabilityIsolatedUIControl component2 = noButton.GetComponent<InteractabilityIsolatedUIControl>();
		if (component != null)
		{
			component.ControlSecondIdentifier = playerGUID;
		}
		if (component2 != null)
		{
			component2.ControlSecondIdentifier = playerGUID;
		}
	}

	public void Hide()
	{
		window.Hide();
	}

	public void Hide(bool instant)
	{
		window.Hide(instant);
	}

	public void Show(RectTransform position = null)
	{
		if (position != null)
		{
			box.position = position.position;
		}
		else
		{
			box.anchoredPosition = Vector2.zero;
		}
		window.Show();
	}
}

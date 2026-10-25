using System.Collections;
using GLOOM.MainMenu;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class EarlyAccessNotificationManager : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	public GameObject earlyAccessNotification;

	public string notificationId = "Notification.EarlyAccess.v3";

	private void Awake()
	{
		window.onHidden.AddListener(() =>
		{
			MainMenuUIManager.Instance.RequestDisableInteraction(disable: false, this);
		});
	}

	private void Start()
	{
		if (PlayerPrefs.GetInt(notificationId, 0) != 1)
		{
			ShowEarlyAccessNotification();
		}
	}

	public void ShowEarlyAccessNotification()
	{
		earlyAccessNotification.SetActive(value: true);
		MainMenuUIManager.Instance.RequestDisableInteraction(disable: true, this);
		UIManager.Instance.StartCoroutine(ShowDelayedNotification());
	}

	public IEnumerator ShowDelayedNotification()
	{
		yield return new WaitForEndOfFrame();
		window.Show();
		StoreShownNotification();
	}

	private void StoreShownNotification()
	{
		PlayerPrefs.SetInt(notificationId, 1);
		PlayerPrefs.Save();
	}
}

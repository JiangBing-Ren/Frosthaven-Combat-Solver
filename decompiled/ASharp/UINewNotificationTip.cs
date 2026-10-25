using TMPro;
using UnityEngine;

public class UINewNotificationTip : MonoBehaviour
{
	public GameObject notification;

	public TextMeshProUGUI text;

	public string format = "+{0}";

	public void Show(int value)
	{
		Show(string.Format(format, value));
	}

	public void Show(string message)
	{
		text.text = message;
		Show();
	}

	public void Show()
	{
		notification.SetActive(value: true);
	}

	public void Hide()
	{
		notification.SetActive(value: false);
	}
}

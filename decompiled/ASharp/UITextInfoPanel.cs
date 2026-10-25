using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITextInfoPanel : Singleton<UITextInfoPanel>
{
	[SerializeField]
	private UIWindow _window;

	[SerializeField]
	private TMP_Text header;

	[SerializeField]
	private TMP_Text information;

	public void Show(string title, string description = null)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			header.SetText(title);
			if (description.IsNullOrEmpty())
			{
				information.gameObject.SetActive(value: false);
			}
			else
			{
				information.text = description;
				information.gameObject.SetActive(value: true);
			}
			_window.Show();
		}
	}

	public void Hide()
	{
		_window.Hide();
	}
}

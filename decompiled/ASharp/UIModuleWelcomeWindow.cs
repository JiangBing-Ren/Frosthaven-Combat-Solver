using Libs.Build;
using UnityEngine;
using UnityEngine.UI;

public class UIModuleWelcomeWindow : MonoBehaviour
{
	public UIWindow Window;

	public FHButton ButtonClose;

	public bool NeedToShow()
	{
		return SaveData.Instance.Global.LastStartVerion != RuntimeBuildInfo.BuildVersion;
	}

	private void Awake()
	{
		ButtonClose.onClick.RemoveAllListeners();
		ButtonClose.onClick.AddListener(Hide);
	}

	public void Show()
	{
		Window.Show();
	}

	public void Hide()
	{
		SaveData.Instance.Global.LastStartVerion = RuntimeBuildInfo.BuildVersion;
		SaveData.Instance.SaveGlobalData();
		Window.Hide();
	}
}

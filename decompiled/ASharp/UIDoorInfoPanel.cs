using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIDoorInfoPanel : Singleton<UIDoorInfoPanel>
{
	[SerializeField]
	private UIWindow _window;

	[SerializeField]
	private TMP_Text doorState;

	public void Show(UnityGameEditorDoorProp doorProp)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			if (doorProp == null)
			{
				Log.GUIError("Trying to display door info but the door object does not exist.");
				return;
			}
			doorState.SetText(doorProp.IsDoorOpen ? LocalizationManager.GetTranslation("DOORWAY_TOOLTIP") : LocalizationManager.GetTranslation("CLOSED_DOOR_TOOLTIP"));
			_window.Show();
		}
	}

	public void Hide()
	{
		_window.Hide();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MEC;
using UnityEngine;

namespace Gloomhaven;

public sealed class DisplaySettings : LocalizedListener
{
	[SerializeField]
	private ExtendedDropdown displayModeSelectField;

	[SerializeField]
	private ExtendedDropdown resolutionSelectField;

	private Resolution[] resolutions;

	private int currentResolutionIndex;

	private int nativeResolutionIndex;

	private bool fullScreen;

	private bool visibleInBackground;

	private int selectedMonitor;

	private SelectorWrapper<DisplayModes> displayModeSelector;

	private void Awake()
	{
		DisplayModes[] source = (DisplayModes[])Enum.GetValues(typeof(DisplayModes));
		displayModeSelector = new SelectorWrapper<DisplayModes>(displayModeSelectField, source.Select((DisplayModes it) => new SelectorOptData<DisplayModes>(it, () => LocalizationManager.GetTranslation($"GUI_OPT_VIDEO_{it}"))).ToList());
		displayModeSelector.OnValuedChanged.AddListener(SetDisplayMode);
		resolutionSelectField.onValueChanged.AddListener(SetResolution);
	}

	private void Start()
	{
		int num = PlayerPrefs.GetInt("Screenmanager Fullscreen mode");
		int num2 = PlayerPrefs.GetInt("Visible In Background", 0);
		visibleInBackground = num2 == 1;
		fullScreen = num != 3;
		displayModeSelector.SetValueWithoutNotify(fullScreen ? ((!visibleInBackground) ? DisplayModes.Fullscreen : DisplayModes.BorderlessWindowed) : DisplayModes.Windowed);
		UpdateDisplaySelection();
		UpdateResolutionSelection(launching: true);
	}

	private void UpdateDisplaySelection()
	{
		selectedMonitor = PlayerPrefs.GetInt("UnitySelectMonitor");
	}

	private void UpdateResolutionSelection(bool launching)
	{
		List<Resolution> list = new List<Resolution>();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (!list.Contains(Screen.resolutions[i]))
			{
				list.Add(Screen.resolutions[i]);
			}
		}
		resolutions = list.ToArray();
		resolutionSelectField.ClearOptions();
		Log.GUIInfo("Resolutions amount: " + resolutions.Length);
		nativeResolutionIndex = 0;
		int num = -1;
		int num2 = PlayerPrefs.GetInt("Screenmanager Resolution Width");
		int num3 = PlayerPrefs.GetInt("Screenmanager Resolution Height");
		List<string> list2 = new List<string>();
		for (int j = 0; j < resolutions.Length; j++)
		{
			if (resolutions[j].width == Display.displays[selectedMonitor].systemWidth && resolutions[j].height == Display.displays[selectedMonitor].systemHeight)
			{
				nativeResolutionIndex = j;
			}
			if (launching && resolutions[j].width == num2 && resolutions[j].height == num3)
			{
				num = j;
			}
			list2.Add(resolutions[j].ToString());
		}
		resolutionSelectField.AddOptions(list2);
		if (num != -1)
		{
			resolutionSelectField.SetValueWithoutNotify(num);
			currentResolutionIndex = num;
		}
	}

	public void SetDisplayMode(DisplayModes displayModeID)
	{
		fullScreen = displayModeID != DisplayModes.Windowed;
		visibleInBackground = displayModeID == DisplayModes.BorderlessWindowed;
		UpdateResolution();
		Log.GUIInfo("Display Mode: " + displayModeID);
	}

	public void SetTargetDisplay(int targetDisplayID, string targetDisplayName)
	{
		Timing.RunCoroutine(SetTargetDisplay(targetDisplayID));
	}

	private IEnumerator<float> SetTargetDisplay(int targetDisplayID)
	{
		Log.GUIInfo("Switching target display to Display " + (targetDisplayID + 1));
		PlayerPrefs.SetInt("UnitySelectMonitor", targetDisplayID);
		yield return 0f;
		UpdateResolutionSelection(launching: false);
		Resolution resolution = resolutions[0];
		Log.GUIInfo("About to change to a temp resolution: " + resolution.ToString());
		resolutionSelectField.value = 0;
		yield return 0f;
		resolution = resolutions[nativeResolutionIndex];
		Log.GUIInfo("About to change to the native resolution: " + resolution.ToString());
		resolutionSelectField.value = nativeResolutionIndex;
	}

	public void SetResolution(int resolutionIndex)
	{
		currentResolutionIndex = resolutionIndex;
		UpdateResolution();
	}

	private void UpdateResolution()
	{
		Screen.SetResolution(resolutions[currentResolutionIndex].width, resolutions[currentResolutionIndex].height, fullScreen, resolutions[currentResolutionIndex].refreshRate);
		Screen.fullScreenMode = ((!fullScreen) ? FullScreenMode.Windowed : (visibleInBackground ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen));
		Log.GUIInfo("Resolution: " + resolutions[currentResolutionIndex].ToString());
	}

	public void CloseAllSelectFields()
	{
		if (displayModeSelectField.IsExpanded)
		{
			displayModeSelectField.Hide();
		}
		if (resolutionSelectField.IsExpanded)
		{
			resolutionSelectField.Hide();
		}
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("Visible In Background", visibleInBackground ? 1 : 0);
	}

	protected override void OnLanguageChanged()
	{
		displayModeSelector.RefreshTexts();
	}
}

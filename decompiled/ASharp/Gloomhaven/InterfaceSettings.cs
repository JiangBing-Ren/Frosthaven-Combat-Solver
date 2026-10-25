using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public sealed class InterfaceSettings : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler uiCanvasScaler;

	[SerializeField]
	private ButtonSwitch toggleAbilityFocusOutlines;

	[SerializeField]
	private ButtonSwitch toggleHoverOutlines;

	private const float widthRatio = 0.000965f;

	private const float heightRatio = 0.001333f;

	private void Start()
	{
		if (SaveData.Instance?.Global != null)
		{
			Initialize();
		}
		else
		{
			SceneController.Instance.OnSaveDataLoaded += Initialize;
		}
	}

	private void Initialize()
	{
		SceneController.Instance.OnSaveDataLoaded -= Initialize;
		toggleAbilityFocusOutlines.IsOn = !SaveData.Instance.Global.DisableAbilityFocusOutlines;
		toggleHoverOutlines.IsOn = !SaveData.Instance.Global.DisableHoverOutlines;
		toggleAbilityFocusOutlines.OnValueChanged.AddListener(ToggleAbilityFocusOutlines);
		toggleHoverOutlines.OnValueChanged.AddListener(ToggleHoverOutlines);
	}

	public void ToggleAbilityFocusOutlines(bool enable)
	{
		SaveData.Instance.Global.DisableAbilityFocusOutlines = !enable;
		SaveData.Instance.SaveGlobalData();
	}

	public void ToggleHoverOutlines(bool enable)
	{
		SaveData.Instance.Global.DisableHoverOutlines = !enable;
		SaveData.Instance.SaveGlobalData();
	}

	public void SetUIScale(float scaleFactor)
	{
		uiCanvasScaler.scaleFactor = Utility.Round(scaleFactor, 2);
	}
}

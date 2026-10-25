using FHGUI.Common;
using UnityEngine;

public class UnityGameEditorHazardousTerrainProp : MonoBehaviour, IHoverable, IGeneralInfoProvider
{
	private void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	public void OnCursorEnter()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIPropInfoPanel>.Instance?.ShowHazardousTerrain(gameObject);
		}
	}

	public void OnCursorExit()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIPropInfoPanel>.Instance?.Hide();
		}
	}

	public IGeneralInfoData GetInfoData()
	{
		return UIGeneralInfoUtils.GetPropData(gameObject.GetComponent<UnityGameEditorObject>());
	}
}

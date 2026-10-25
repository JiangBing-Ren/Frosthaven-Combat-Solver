using FHGUI.Common;
using UnityEngine;

public class UnityGameEditorDifficultTerrainProp : MonoBehaviour, IHoverable, IGeneralInfoProvider
{
	private void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("Hovering");
	}

	public void OnCursorEnter()
	{
		if (SaveData.Instance.Global.GameMode != EGameMode.LevelEditor)
		{
			Singleton<UIPropInfoPanel>.Instance?.ShowDifficultTerrain(gameObject);
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
		return UIGeneralInfoUtils.GetPropData(GetComponent<UnityGameEditorObject>());
	}
}

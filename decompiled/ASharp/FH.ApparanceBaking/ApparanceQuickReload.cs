using Libs.Utils;
using UnityEngine;

namespace FH.ApparanceBaking;

public class ApparanceQuickReload : MonoBehaviour
{
	private ByRef<bool> _isReloading = new ByRef<bool>(value: false);

	[SerializeField]
	private GameObject _reloadUI;

	public bool IsReloading => _isReloading.Value;

	public void Init()
	{
		ShowReload(show: false);
	}

	public void FullReload()
	{
		_isReloading.Value = true;
		ShowReload(show: true);
		EnvironmentLoader.Instance.RegenerateProcGen(() =>
		{
			_isReloading.Value = false;
			ShowReload(show: false);
		}, freeze: false);
	}

	private void ShowReload(bool show)
	{
		if ((bool)_reloadUI)
		{
			_reloadUI.SetActive(show);
			LevelEditorApparanceDirtyUI[] array = Object.FindObjectsOfType<LevelEditorApparanceDirtyUI>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MarkClean();
			}
		}
	}
}

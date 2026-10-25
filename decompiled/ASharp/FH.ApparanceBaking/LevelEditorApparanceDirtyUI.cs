using UnityEngine;

namespace FH.ApparanceBaking;

public class LevelEditorApparanceDirtyUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _showDirty;

	public void Init()
	{
		MarkClean();
	}

	public void MarkDirty()
	{
		_showDirty.SetActive(value: true);
	}

	public void MarkClean()
	{
		_showDirty.SetActive(value: false);
	}
}

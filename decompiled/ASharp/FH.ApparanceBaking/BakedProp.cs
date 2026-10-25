using UnityEngine;

namespace FH.ApparanceBaking;

public class BakedProp : MonoBehaviour
{
	public GameObject bakedProp;

	private GameObject _instance;

	public void Show()
	{
		if (Application.isPlaying && !(_instance != null))
		{
			_instance = Object.Instantiate(bakedProp, transform.position, transform.rotation, transform);
		}
	}

	public void Hide()
	{
		if (Application.isPlaying && !(_instance == null))
		{
			Object.Destroy(_instance);
			_instance = null;
		}
	}

	[ContextMenu("Type")]
	public void ShowType()
	{
		Debug.Log(GetType());
	}
}

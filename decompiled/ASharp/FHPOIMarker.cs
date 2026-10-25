using UnityEngine;
using UnityEngine.UI;

public class FHPOIMarker : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	public void Init(Sprite icon)
	{
		if ((bool)_icon)
		{
			_icon.sprite = icon;
		}
	}

	public void Show(Vector3 position)
	{
		transform.position = position;
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}
}

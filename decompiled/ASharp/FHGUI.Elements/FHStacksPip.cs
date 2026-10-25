using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHStacksPip : MonoBehaviour
{
	[SerializeField]
	private Image _fill;

	public void SetActive(bool active)
	{
		_fill.enabled = active;
	}
}

using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIAttackModifierPip : MonoBehaviour
{
	[SerializeField]
	private Image _active;

	[SerializeField]
	private Image _inactive;

	public void Init(bool active)
	{
		_active.enabled = active;
		_inactive.enabled = !active;
	}
}

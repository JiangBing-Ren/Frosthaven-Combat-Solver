using System;
using UnityEngine;

namespace FHGUI.Elements;

public class UIElementStateActivator : MonoBehaviour, IUIElementStateListener
{
	[SerializeField]
	private string _state;

	public void StateChanged(string state)
	{
		gameObject.SetActive(string.Compare(_state, state, StringComparison.InvariantCultureIgnoreCase) == 0);
	}

	public void ResetState()
	{
		gameObject.SetActive(value: false);
	}
}

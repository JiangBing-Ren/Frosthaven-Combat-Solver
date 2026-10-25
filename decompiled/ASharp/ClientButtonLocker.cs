using UnityEngine;
using UnityEngine.UI;

public class ClientButtonLocker : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private void OnEnable()
	{
		TryLockButton();
	}

	public void Initialize(Button buttonToLock)
	{
		button = buttonToLock;
		TryLockButton();
	}

	private void TryLockButton()
	{
		if (FFSNetwork.IsClient && button != null)
		{
			button.interactable = false;
		}
	}
}

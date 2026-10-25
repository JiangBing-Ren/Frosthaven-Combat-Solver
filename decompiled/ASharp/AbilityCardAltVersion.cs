using UnityEngine;

public class AbilityCardAltVersion : MonoBehaviour
{
	[SerializeField]
	private GameObject _alternativeCardTopActionContainer;

	[SerializeField]
	private GameObject _alternativeCardBottomActionContainer;

	[SerializeField]
	private GameObject[] _activateWhenShown;

	[SerializeField]
	private GameObject[] _deactivateWhenShown;

	public bool Active { get; private set; }

	public void Show()
	{
		Active = true;
		_alternativeCardTopActionContainer.gameObject.SetActive(value: true);
		_alternativeCardBottomActionContainer.gameObject.SetActive(value: true);
		GameObject[] activateWhenShown = _activateWhenShown;
		for (int i = 0; i < activateWhenShown.Length; i++)
		{
			activateWhenShown[i].SetActive(value: true);
		}
		activateWhenShown = _deactivateWhenShown;
		for (int i = 0; i < activateWhenShown.Length; i++)
		{
			activateWhenShown[i].SetActive(value: false);
		}
	}

	public void Hide()
	{
		Active = false;
		if (_alternativeCardTopActionContainer != null)
		{
			_alternativeCardTopActionContainer.gameObject.SetActive(value: false);
		}
		if (_alternativeCardBottomActionContainer != null)
		{
			_alternativeCardBottomActionContainer.gameObject.SetActive(value: false);
		}
		GameObject[] activateWhenShown = _activateWhenShown;
		foreach (GameObject gameObject in activateWhenShown)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		activateWhenShown = _deactivateWhenShown;
		foreach (GameObject gameObject2 in activateWhenShown)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: true);
			}
		}
	}

	public void Toggle(bool active)
	{
		if (active)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}
}

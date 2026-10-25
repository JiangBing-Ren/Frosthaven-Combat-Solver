using UnityEngine;

public class ProcessingUI : MonoBehaviour
{
	private static ProcessingUI _instance;

	private static bool _state;

	[SerializeField]
	private GameObject _view;

	[SerializeField]
	private GameObject _minimizedView;

	private static ProcessingUI Instance
	{
		get
		{
			if (!(_instance == null))
			{
				return _instance;
			}
			return _instance = Object.FindObjectOfType<ProcessingUI>();
		}
	}

	public static bool IsShown => _state;

	private void Start()
	{
		_view.SetActive(_state);
		Instance._minimizedView.SetActive(value: false);
	}

	public static void Show(bool show)
	{
		if (_state != show)
		{
			_state = show;
			if (!(Instance == null))
			{
				Instance._minimizedView.SetActive(value: false);
				Instance._view.SetActive(show);
			}
		}
	}

	public void Minimize()
	{
		if (!(Instance == null) && _state)
		{
			Instance._view.SetActive(value: false);
			Instance._minimizedView.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}
}

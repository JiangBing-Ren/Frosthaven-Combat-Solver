using System.Collections;
using System.Threading.Tasks;
using FH.Platforms;
using UnityEngine;
using UnityEngine.UI;

namespace FH;

public class TelemetryAndEulaPolicyUI : MonoBehaviour
{
	private static TelemetryAndEulaPolicyUI _instance;

	private static bool _state;

	[SerializeField]
	private GameObject _view;

	[SerializeField]
	private Toggle _telemetryToggle;

	private static TelemetryAndEulaPolicyUI Instance
	{
		get
		{
			if (!(_instance == null))
			{
				return _instance;
			}
			return _instance = Object.FindObjectOfType<TelemetryAndEulaPolicyUI>();
		}
	}

	private void Start()
	{
		_view.SetActive(_state);
	}

	private static void Show(bool show)
	{
		_state = show;
		if (!(Instance == null))
		{
			Instance._view.SetActive(show);
		}
	}

	public void AcceptEula()
	{
		TelemetryAndEulaPolicy.SetEulaAccepted(accepted: true);
		TelemetryAndEulaPolicy.SetTelemetryEnabled(_telemetryToggle.isOn);
		Show(show: false);
	}

	public void OpenEULA()
	{
		StartCoroutine(OpenEULACrt());
	}

	private IEnumerator OpenEULACrt()
	{
		Task<string> getLocale = PlatformLayer.Instance.NewPlatformLayer.GetLocale();
		while (!getLocale.IsCompleted)
		{
			yield return null;
		}
		string text = NewPlatformLocalization.TryTransformNameToISO639(getLocale.Result);
		Application.OpenURL("https://www.arcgames.com/" + text + "/about/terms/");
	}

	public void Quit()
	{
		Application.Quit();
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	internal static void ShowTelemetryAndEulaPolicy()
	{
		Show(show: true);
	}
}

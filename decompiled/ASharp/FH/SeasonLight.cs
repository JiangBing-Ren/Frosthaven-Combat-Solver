using UnityEngine;
using UnityEngine.Rendering;

namespace FH;

[ExecuteAlways]
public class SeasonLight : MonoBehaviour
{
	[Range(0f, 1f)]
	public float Season;

	public VolumeProfile SummerProfile;

	public VolumeProfile WinterProfile;

	[SerializeField]
	private Gradient LightGradient = new Gradient();

	public float SummerIntensity = 1f;

	public float WinterIntensity = 1f;

	public Vector3 SummerRot;

	public Vector3 WinterRot;

	public Vector3 CookieMovement = new Vector3(4f, 0f, 0f);

	public Volume SummerTone;

	public Volume WinterTone;

	public GameObject mainLight;

	public void Update()
	{
		if (Application.isPlaying)
		{
			mainLight.transform.position += CookieMovement * Time.deltaTime;
			LightChange();
		}
	}

	private void OnValidate()
	{
		LightChange();
	}

	private void LightChange()
	{
		mainLight.transform.eulerAngles = Vector3.Lerp(SummerRot, WinterRot, Season);
		Light component = mainLight.GetComponent<Light>();
		if (component == null)
		{
			Debug.LogError("No light component found on main light at " + name);
			return;
		}
		component.color = LightGradient.Evaluate(Season);
		component.intensity = Mathf.Lerp(SummerIntensity, WinterIntensity, Season);
		SummerTone.sharedProfile = SummerProfile;
		WinterTone.sharedProfile = WinterProfile;
		WinterTone.weight = Season;
	}
}

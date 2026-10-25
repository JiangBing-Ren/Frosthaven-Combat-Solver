using UnityEngine;

public class RFX4_DemoGUI : MonoBehaviour
{
	public GameObject[] Prefabs;

	public float[] ReactivationTimes;

	public Light Sun;

	public ReflectionProbe ReflectionProbe;

	public Light[] NightLights = new Light[0];

	public Texture HUETexture;

	public bool UseMobileVersion;

	public RFX4_DistortionAndBloom RFX4_DistortionAndBloom;

	private int currentNomber;

	private GameObject currentInstance;

	public GUIStyle guiStyleHeader = new GUIStyle();

	public GUIStyle guiStyleHeaderMobile = new GUIStyle();

	private float dpiScale;

	private bool isDay;

	private float colorHUE;

	private float startSunIntensity;

	private Quaternion startSunRotation;

	private Color startAmbientLight;

	private float startAmbientIntencity;

	private float startReflectionIntencity;

	private LightShadows startLightShadows;

	private bool isButtonPressed;

	private bool isUsedMobileBloom = true;

	private void Start()
	{
		if (Screen.dpi < 1f)
		{
			dpiScale = 1f;
		}
		if (Screen.dpi < 200f)
		{
			dpiScale = 1f;
		}
		else
		{
			dpiScale = Screen.dpi / 200f;
		}
		guiStyleHeader.fontSize = (int)(15f * dpiScale);
		guiStyleHeaderMobile.fontSize = (int)(17f * dpiScale);
		ChangeCurrent(0);
		startSunIntensity = Sun.intensity;
		startSunRotation = Sun.transform.rotation;
		startAmbientLight = RenderSettings.ambientLight;
		startAmbientIntencity = RenderSettings.ambientIntensity;
		startReflectionIntencity = RenderSettings.reflectionIntensity;
		startLightShadows = Sun.shadows;
	}

	private void OnGUI()
	{
		if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.DownArrow))
		{
			isButtonPressed = false;
		}
		if (GUI.Button(new Rect(10f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "PREVIOUS EFFECT") || (!isButtonPressed && Input.GetKeyDown(KeyCode.LeftArrow)))
		{
			isButtonPressed = true;
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(160f * dpiScale, 15f * dpiScale, 135f * dpiScale, 37f * dpiScale), "NEXT EFFECT") || (!isButtonPressed && Input.GetKeyDown(KeyCode.RightArrow)))
		{
			isButtonPressed = true;
			ChangeCurrent(1);
		}
		float num = 0f;
		if (UseMobileVersion)
		{
			num += 50f * dpiScale;
			if (GUI.Button(new Rect(10f * dpiScale, 63f * dpiScale, 285f * dpiScale, 37f * dpiScale), "ON / OFF REALISTIC BLOOM") || (!isButtonPressed && Input.GetKeyDown(KeyCode.DownArrow)))
			{
				isUsedMobileBloom = !isUsedMobileBloom;
				RFX4_DistortionAndBloom.UseBloom = isUsedMobileBloom;
			}
			GUI.Label(new Rect(400f * dpiScale, 15f * dpiScale, 100f * dpiScale, 20f * dpiScale), "Bloom is " + (isUsedMobileBloom ? "Enabled" : "Disabled"), guiStyleHeaderMobile);
		}
		if (GUI.Button(new Rect(10f * dpiScale, 63f * dpiScale + num, 285f * dpiScale, 37f * dpiScale), "Day / Night") || (!isButtonPressed && Input.GetKeyDown(KeyCode.DownArrow)))
		{
			isButtonPressed = true;
			if (ReflectionProbe != null)
			{
				ReflectionProbe.RenderProbe();
			}
			Sun.intensity = ((!isDay) ? 0.05f : startSunIntensity);
			Sun.shadows = (isDay ? startLightShadows : LightShadows.None);
			Light[] nightLights = NightLights;
			for (int i = 0; i < nightLights.Length; i++)
			{
				nightLights[i].shadows = ((!isDay) ? startLightShadows : LightShadows.None);
			}
			Sun.transform.rotation = (isDay ? startSunRotation : Quaternion.Euler(350f, 30f, 90f));
			RenderSettings.ambientLight = ((!isDay) ? new Color(0.1f, 0.1f, 0.1f) : startAmbientLight);
			RenderSettings.ambientIntensity = (isDay ? startAmbientIntencity : 1f);
			RenderSettings.reflectionIntensity = (isDay ? startReflectionIntencity : 0.2f);
			isDay = !isDay;
		}
		GUI.Label(new Rect(350f * dpiScale, 15f * dpiScale + num / 2f, 500f * dpiScale, 20f * dpiScale), "press left mouse button for the camera rotating and scroll wheel for zooming", guiStyleHeader);
		GUI.Label(new Rect(350f * dpiScale, 35f * dpiScale + num / 2f, 160f * dpiScale, 20f * dpiScale), "prefab name is: " + Prefabs[currentNomber].name, guiStyleHeader);
		GUI.DrawTexture(new Rect(12f * dpiScale, 120f * dpiScale + num, 285f * dpiScale, 15f * dpiScale), HUETexture, ScaleMode.StretchToFill, alphaBlend: false, 0f);
		float num2 = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12f * dpiScale, 140f * dpiScale + num, 285f * dpiScale, 15f * dpiScale), colorHUE, 0f, 360f);
		if (!((double)Mathf.Abs(num2 - colorHUE) > 0.001))
		{
			return;
		}
		RFX4_ColorHelper.ChangeObjectColorByHUE(currentInstance, colorHUE / 360f);
		RFX4_TransformMotion componentInChildren = currentInstance.GetComponentInChildren<RFX4_TransformMotion>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.HUE = colorHUE / 360f;
			foreach (GameObject collidedInstance in componentInChildren.CollidedInstances)
			{
				if (collidedInstance != null)
				{
					RFX4_ColorHelper.ChangeObjectColorByHUE(collidedInstance, colorHUE / 360f);
				}
			}
		}
		RFX4_RaycastCollision componentInChildren2 = currentInstance.GetComponentInChildren<RFX4_RaycastCollision>(includeInactive: true);
		if (!(componentInChildren2 != null))
		{
			return;
		}
		componentInChildren2.HUE = colorHUE / 360f;
		foreach (GameObject collidedInstance2 in componentInChildren2.CollidedInstances)
		{
			if (collidedInstance2 != null)
			{
				RFX4_ColorHelper.ChangeObjectColorByHUE(collidedInstance2, colorHUE / 360f);
			}
		}
	}

	private void ChangeCurrent(int delta)
	{
		currentNomber += delta;
		if (currentNomber > Prefabs.Length - 1)
		{
			currentNomber = 0;
		}
		else if (currentNomber < 0)
		{
			currentNomber = Prefabs.Length - 1;
		}
		if (currentInstance != null)
		{
			Object.Destroy(currentInstance);
			RemoveClones();
		}
		currentInstance = Object.Instantiate(Prefabs[currentNomber]);
		if (ReactivationTimes.Length == Prefabs.Length)
		{
			CancelInvoke();
			if (ReactivationTimes[currentNomber] > 0.1f)
			{
				InvokeRepeating("Reactivate", ReactivationTimes[currentNomber], ReactivationTimes[currentNomber]);
			}
		}
	}

	private void RemoveClones()
	{
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name.Contains("(Clone)"))
			{
				Object.Destroy(gameObject);
			}
		}
	}

	private void Reactivate()
	{
		currentInstance.SetActive(value: false);
		currentInstance.SetActive(value: true);
	}
}

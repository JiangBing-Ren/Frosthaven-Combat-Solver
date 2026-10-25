using FH.LevelComponents.ProcGen;
using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

[RequireComponent(typeof(UnityGameEditorDoorProp))]
public class DoorLightingTransition : MonoBehaviour
{
	[SerializeField]
	private float HorizontalOffset = 1.3f;

	[SerializeField]
	private float Height = 4.3f;

	private Ambience _ambience;

	private UnityGameEditorDoorProp _doorProp;

	private Light _light;

	public const string TRANSITION_LIGHT_NAME = "LightingTransitionLight";

	private void InsantiateLight()
	{
		GameObject gameObject = new GameObject("LightingTransitionLight");
		_light = gameObject.AddComponent<Light>();
		_light.transform.parent = transform;
	}

	public bool ConfigureLight(Ambience ambience, bool invertDirection)
	{
		_ambience = ambience;
		if (_ambience == null)
		{
			Debug.LogWarning("Skipped configuring lighting transition for door, because the map tile has no ambience.");
			return false;
		}
		if (!_ambience.GetDirectionalLight(out var directionalLight) && !_ambience.GetClosestLight(base.transform.position, out directionalLight))
		{
			Debug.LogWarning("Skipped configuring lighting transition for door, because the map tile has no ambience lights.");
			return false;
		}
		if (_light == null)
		{
			InsantiateLight();
		}
		if (_doorProp == null)
		{
			_doorProp = GetComponent<UnityGameEditorDoorProp>();
		}
		Vector3 position = base.transform.position;
		Vector3 vector = (invertDirection ? (-_doorProp.ForwardVector()) : _doorProp.ForwardVector());
		position += HorizontalOffset * vector;
		position += Height * Vector3.up;
		Quaternion rotation = Quaternion.LookRotation(Vector3.down + vector * 0.2f);
		Transform transform = _light.transform;
		transform.position = position;
		transform.rotation = rotation;
		_light.SetLayer(AmbienceDiscriminator.UnrevealedLayer);
		_light.bounceIntensity = 0f;
		_light.useColorTemperature = directionalLight.useColorTemperature;
		if (directionalLight.useColorTemperature)
		{
			_light.colorTemperature = directionalLight.colorTemperature;
		}
		else
		{
			_light.color = NormalizeColor(directionalLight.color);
		}
		_light.type = LightType.Spot;
		_light.range = 15f;
		_light.innerSpotAngle = 0f;
		_light.spotAngle = 105f;
		_light.intensity = 50f;
		_light.shadows = LightShadows.None;
		return true;
	}

	public void SetLightActive(bool active)
	{
		_light.enabled = active;
	}

	private static Color NormalizeColor(Color color)
	{
		Vector3 normalized = new Vector3(color.r, color.g, color.b).normalized;
		return new Color(normalized.x, normalized.y, normalized.z, color.a);
	}
}

using UnityEngine;

namespace Ara;

[RequireComponent(typeof(AraTrail))]
public class ColorFromSpeed : MonoBehaviour
{
	private AraTrail trail;

	[Tooltip("Maps trail speed to color. Control how much speed is transferred to the trail by setting inertia > 0. The trail will be colorized even if physics are disabled. ")]
	public Gradient colorFromSpeed = new Gradient();

	[Tooltip("Min speed used to map speed to color.")]
	public float minSpeed;

	[Tooltip("Max speed used to map speed to color.")]
	public float maxSpeed = 5f;

	private void OnEnable()
	{
		trail = GetComponent<AraTrail>();
		trail.onUpdatePoints += SetColorFromSpeed;
	}

	private void OnDisable()
	{
		trail.onUpdatePoints -= SetColorFromSpeed;
	}

	private void SetColorFromSpeed()
	{
		for (int i = 0; i < trail.points.Count; i++)
		{
			AraTrail.Point value = trail.points[i];
			value.color = colorFromSpeed.Evaluate((value.velocity.magnitude - minSpeed) / Mathf.Max(1E-05f, maxSpeed - minSpeed));
			trail.points[i] = value;
		}
	}
}

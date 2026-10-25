using UnityEngine;

public class RFX4_EffectSettingProjectile : MonoBehaviour
{
	public float FlyDistanceForProjectiles = 30f;

	public float SpeedMultiplier = 1f;

	public LayerMask CollidesWith = -1;

	private float startSpeed;

	private const string particlesAdditionalName = "Distance";

	private void Awake()
	{
		RFX4_TransformMotion componentInChildren = GetComponentInChildren<RFX4_TransformMotion>(includeInactive: true);
		if (componentInChildren != null)
		{
			startSpeed = componentInChildren.Speed;
		}
	}

	private void OnEnable()
	{
		RFX4_TransformMotion componentInChildren = GetComponentInChildren<RFX4_TransformMotion>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.Distance = FlyDistanceForProjectiles;
			componentInChildren.CollidesWith = CollidesWith;
			componentInChildren.Speed = startSpeed * SpeedMultiplier;
		}
		RFX4_RaycastCollision componentInChildren2 = GetComponentInChildren<RFX4_RaycastCollision>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.RaycastDistance = FlyDistanceForProjectiles;
		}
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (particleSystem.name.Contains("Distance"))
			{
				particleSystem.GetComponent<ParticleSystemRenderer>().lengthScale = FlyDistanceForProjectiles / particleSystem.main.startSize.constantMax;
			}
		}
	}
}

using UnityEngine;

public class RFX4_EffectSettingPhysxForce : MonoBehaviour
{
	public float ForceMultiplier = 1f;

	private void Start()
	{
	}

	private void Update()
	{
		RFX4_TransformMotion componentInChildren = GetComponentInChildren<RFX4_TransformMotion>(includeInactive: true);
		if (componentInChildren != null)
		{
			foreach (GameObject collidedInstance in componentInChildren.CollidedInstances)
			{
				RFX4_PhysicsForceCurves component = collidedInstance.GetComponent<RFX4_PhysicsForceCurves>();
				if (component != null)
				{
					component.forceAdditionalMultiplier = ForceMultiplier;
				}
			}
		}
		RFX4_PhysicsForceCurves[] componentsInChildren = GetComponentsInChildren<RFX4_PhysicsForceCurves>();
		foreach (RFX4_PhysicsForceCurves rFX4_PhysicsForceCurves in componentsInChildren)
		{
			if (rFX4_PhysicsForceCurves != null)
			{
				rFX4_PhysicsForceCurves.forceAdditionalMultiplier = ForceMultiplier;
			}
		}
	}
}

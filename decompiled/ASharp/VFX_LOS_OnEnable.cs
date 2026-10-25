using UnityEngine;

public class VFX_LOS_OnEnable : VFX_LOS
{
	[Tooltip("How big can the VFX get? This radius is used as the maximum reach of the VFX, for calculating the Line of Sight.")]
	[SerializeField]
	private float EffectRadius = 3f;

	private void OnEnable()
	{
		RequestLOS(transform.position, EffectRadius);
	}
}

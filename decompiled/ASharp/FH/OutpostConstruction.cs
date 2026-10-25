using UnityEngine;

namespace FH;

[ExecuteInEditMode]
[RequireComponent(typeof(PropHealthState))]
public class OutpostConstruction : MonoBehaviour
{
	private PropHealthState healthState;

	[Range(0f, 100f)]
	public int healthNow;

	[Range(0f, 100f)]
	public int healthMax = 100;

	private void OnEnable()
	{
		healthState = GetComponent<PropHealthState>();
		if ((bool)healthState)
		{
			healthState.Awake();
		}
	}

	private void Update()
	{
		if ((bool)healthState)
		{
			healthState.UpdateState(healthMax - healthNow, healthMax);
		}
	}
}

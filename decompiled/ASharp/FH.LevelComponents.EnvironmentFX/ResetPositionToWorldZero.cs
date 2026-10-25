using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

[ExecuteAlways]
public class ResetPositionToWorldZero : MonoBehaviour
{
	private void OnEnable()
	{
		transform.position = Vector3.zero;
	}
}

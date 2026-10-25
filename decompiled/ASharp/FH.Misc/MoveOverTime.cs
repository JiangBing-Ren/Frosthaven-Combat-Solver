using UnityEngine;

namespace FH.Misc;

public class MoveOverTime : MonoBehaviour
{
	public Vector3 direction;

	private void Update()
	{
		transform.position += direction * Time.deltaTime;
	}
}

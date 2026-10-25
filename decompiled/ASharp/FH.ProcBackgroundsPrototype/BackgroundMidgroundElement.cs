using UnityEngine;

namespace FH.ProcBackgroundsPrototype;

public class BackgroundMidgroundElement : MonoBehaviour
{
	public BackgroundElementType Type;

	private void OnDrawGizmos()
	{
		Color color = Gizmos.color;
		switch (Type)
		{
		case BackgroundElementType.Feature:
			Gizmos.color = Color.green;
			Gizmos.DrawCube(transform.position, new Vector3(1f, 10f, 1f));
			break;
		case BackgroundElementType.Island:
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, 0.5f);
			break;
		default:
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(transform.position, 0.5f);
			break;
		}
		Gizmos.color = color;
	}
}

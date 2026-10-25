using UnityEngine;

public class VFX_SetPosition : MonoBehaviour
{
	private bool modifyHeight = true;

	public float worldHeight;

	private Vector3 pos;

	private Transform parente;

	private void Update()
	{
		if (modifyHeight)
		{
			modifyHeight = false;
			pos = transform.position;
			parente = transform.parent;
			transform.SetParent(null);
			transform.position = new Vector3(pos.x, worldHeight, pos.z);
			transform.SetParent(parente);
		}
	}
}

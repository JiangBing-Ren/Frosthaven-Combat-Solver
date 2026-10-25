using UnityEngine;

public class EditorSceneDirectPlaySpawner : MonoBehaviour
{
	private void Awake()
	{
		Object.Destroy(gameObject);
	}
}

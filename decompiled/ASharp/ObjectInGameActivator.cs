using UnityEngine;

public class ObjectInGameActivator : MonoBehaviour
{
	[SerializeField]
	private GameObject[] objects;

	private void Awake()
	{
		for (int i = 0; i < objects?.Length; i++)
		{
			objects[i]?.SetActive(value: true);
		}
	}
}

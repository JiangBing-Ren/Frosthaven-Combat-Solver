using UnityEngine;

public sealed class DontDestroyOnLoad : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(gameObject);
	}
}

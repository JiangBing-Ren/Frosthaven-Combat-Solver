using UnityEngine;

public class ExcludeFromReleaseBuild : MonoBehaviour
{
	private void Awake()
	{
		gameObject.SetActive(value: false);
	}
}

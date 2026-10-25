using UnityEngine;

public class VFX_Unparent : MonoBehaviour
{
	private bool isUnparented;

	public bool zeroRotation;

	public bool randomRotY;

	private void Start()
	{
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		isUnparented = false;
	}

	private void Update()
	{
		if (isUnparented)
		{
			return;
		}
		isUnparented = true;
		transform.SetParent(null);
		if (zeroRotation)
		{
			transform.eulerAngles = new Vector3(0f, 0f, 0f);
		}
		if (randomRotY)
		{
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0, 360), transform.eulerAngles.z);
		}
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetActive(value: true);
		}
	}
}

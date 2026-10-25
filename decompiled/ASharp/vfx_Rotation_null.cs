using UnityEngine;

public class vfx_Rotation_null : MonoBehaviour
{
	private GameObject itsMe;

	private bool checkRotation = true;

	public bool allowCustomRotation;

	public Vector3 customRotation = new Vector3(0f, 0f, 0f);

	private void Start()
	{
	}

	private void OnEnable()
	{
		checkRotation = true;
	}

	private void Update()
	{
		if (checkRotation)
		{
			if (!allowCustomRotation)
			{
				transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
			else
			{
				transform.localEulerAngles = customRotation;
			}
			checkRotation = false;
		}
	}
}

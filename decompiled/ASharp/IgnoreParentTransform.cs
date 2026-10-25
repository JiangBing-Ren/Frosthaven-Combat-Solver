using UnityEngine;

public class IgnoreParentTransform : MonoBehaviour
{
	public bool IgnoreParentPosition;

	public bool IgnoreParentRotation;

	public bool SetInitialPosition;

	[ConditionalField("SetInitialPosition", null, true)]
	public Vector3 InitialPosition;

	public bool SetInitialRotation;

	[ConditionalField("SetInitialRotation", null, true)]
	public Vector3 InitialRotation;

	private Vector3 _initialPos;

	private Quaternion _initialRot;

	private void Start()
	{
		_initialPos = (SetInitialPosition ? InitialPosition : transform.position);
		_initialRot = (SetInitialRotation ? Quaternion.Euler(InitialRotation) : transform.rotation);
	}

	private void Update()
	{
		if (IgnoreParentPosition)
		{
			transform.position = _initialPos;
		}
		if (IgnoreParentRotation)
		{
			transform.rotation = _initialRot;
		}
	}
}

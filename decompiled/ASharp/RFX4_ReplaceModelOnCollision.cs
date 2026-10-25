using UnityEngine;

public class RFX4_ReplaceModelOnCollision : MonoBehaviour
{
	public GameObject[] PhysicsObjects;

	private bool isCollided;

	private Transform t;

	private void OnCollisionEnter(Collision collision)
	{
		if (!isCollided)
		{
			isCollided = true;
			GameObject[] physicsObjects = PhysicsObjects;
			for (int i = 0; i < physicsObjects.Length; i++)
			{
				physicsObjects[i].SetActive(value: true);
			}
			MeshRenderer component = GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = false;
			}
			Rigidbody component2 = GetComponent<Rigidbody>();
			component2.isKinematic = true;
			component2.detectCollisions = false;
		}
	}

	private void OnEnable()
	{
		isCollided = false;
		GameObject[] physicsObjects = PhysicsObjects;
		for (int i = 0; i < physicsObjects.Length; i++)
		{
			physicsObjects[i].SetActive(value: false);
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = true;
		}
		Rigidbody component2 = GetComponent<Rigidbody>();
		component2.isKinematic = false;
		component2.detectCollisions = true;
	}
}

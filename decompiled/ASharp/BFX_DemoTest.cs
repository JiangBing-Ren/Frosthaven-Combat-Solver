using UnityEngine;

public class BFX_DemoTest : MonoBehaviour
{
	public bool InfiniteDecal;

	public Light DirLight;

	public bool isVR = true;

	public GameObject BloodAttach;

	public GameObject[] BloodFX;

	public Vector3 direction;

	private int effectIdx;

	private Transform GetNearestObject(Transform hit, Vector3 hitPos)
	{
		float num = 100f;
		Transform result = null;
		Transform[] componentsInChildren = hit.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			float num2 = Vector3.Distance(transform.position, hitPos);
			if (num2 < num)
			{
				num = num2;
				result = transform;
			}
		}
		float num3 = Vector3.Distance(hit.position, hitPos);
		if (num3 < num)
		{
			num = num3;
			result = hit;
		}
		return result;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
		{
			float num = Mathf.Atan2(hitInfo.normal.x, hitInfo.normal.z) * 57.29578f + 180f;
			if (effectIdx == BloodFX.Length)
			{
				effectIdx = 0;
			}
			GameObject gameObject = Object.Instantiate(BloodFX[effectIdx], hitInfo.point, Quaternion.Euler(0f, num + 90f, 0f));
			effectIdx++;
			gameObject.GetComponent<BFX_BloodSettings>().LightIntensityMultiplier = DirLight.intensity;
			Transform nearestObject = GetNearestObject(hitInfo.transform.root, hitInfo.point);
			if (nearestObject != null)
			{
				Transform transform = Object.Instantiate(BloodAttach).transform;
				transform.position = hitInfo.point;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one * Random.Range(0.75f, 1.2f);
				transform.LookAt(hitInfo.point + hitInfo.normal, direction);
				transform.Rotate(90f, 0f, 0f);
				transform.transform.parent = nearestObject;
			}
		}
	}

	public float CalculateAngle(Vector3 from, Vector3 to)
	{
		return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
	}
}

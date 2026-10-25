using UnityEngine;

namespace UnityStandardAssets.Utility;

public class TimedParentDestructor : MonoBehaviour
{
	[SerializeField]
	private float m_TimeOut = 1f;

	[SerializeField]
	private bool m_DetachChildren;

	private GameObject papi;

	private void Awake()
	{
		Invoke("DestroyNow", m_TimeOut);
		papi = ((transform.parent == null) ? null : transform.parent.gameObject);
	}

	private void DestroyNow()
	{
		if (m_DetachChildren)
		{
			transform.DetachChildren();
		}
		if (papi != null)
		{
			Object.Destroy(papi);
		}
	}
}

using System.Collections;
using Ara;
using Chronos;
using UnityEngine;

public class TrailEffectEmissionHelper : MonoBehaviour
{
	public float onEnableWaitTime = 0.1f;

	private AraTrail trail;

	private void Start()
	{
		trail = gameObject.GetComponent<AraTrail>();
	}

	private void OnEnable()
	{
		if (trail != null)
		{
			trail.emit = false;
			CoroutineHelper.RunCoroutine(emissionWaitRoutine());
		}
	}

	private IEnumerator emissionWaitRoutine()
	{
		yield return Timekeeper.instance.WaitForSeconds(onEnableWaitTime);
		trail.emit = true;
		yield return new WaitForEndOfFrame();
	}
}

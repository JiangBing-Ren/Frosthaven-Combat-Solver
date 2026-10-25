using System.Collections;
using Chronos;
using UnityEngine;

public class DelayedActivatePropChildren : MonoBehaviour
{
	public float activateAfterTime;

	private void OnEnable()
	{
		StartCoroutine(ActivateTimeline());
		foreach (Transform item in transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	public IEnumerator ActivateTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(activateAfterTime);
		foreach (Transform item in transform)
		{
			item.gameObject.SetActive(value: true);
		}
	}
}

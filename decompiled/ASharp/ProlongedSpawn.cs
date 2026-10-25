using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;

public class ProlongedSpawn : MonoBehaviour
{
	private static List<ProlongedSpawn> s_SpawnsInProgress = new List<ProlongedSpawn>();

	public float resumeChoreographerAfterTime;

	private void OnEnable()
	{
		s_SpawnsInProgress.Add(this);
		StartCoroutine(Timeline());
	}

	private IEnumerator Timeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(resumeChoreographerAfterTime);
		s_SpawnsInProgress.Remove(this);
	}

	private void OnDisable()
	{
		s_SpawnsInProgress.Remove(this);
	}

	private void OnDestroy()
	{
		s_SpawnsInProgress.Remove(this);
	}

	public static bool AreSpawnsInProgress()
	{
		if (s_SpawnsInProgress.Count > 0)
		{
			s_SpawnsInProgress.RemoveAll((ProlongedSpawn i) => i == null);
		}
		return s_SpawnsInProgress.Count > 0;
	}
}

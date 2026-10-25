using System;
using System.Collections;
using Chronos;
using UnityEngine.Playables;

public class Director_OnEnable_Custom : EffectOnEnable
{
	[Serializable]
	public struct Playables
	{
		public PlayableAsset Playable;

		public float PlayAfterTime;
	}

	public Playables[] AllPlayables;

	private void OnEnable()
	{
		StartCoroutine("CyclePlayables");
	}

	private IEnumerator CyclePlayables()
	{
		for (int i = 0; i < AllPlayables.Length; i++)
		{
			yield return Timekeeper.instance.WaitForSeconds(AllPlayables[i].PlayAfterTime);
			PlayPlayable(AllPlayables[i].Playable);
		}
	}

	private void PlayPlayable(PlayableAsset playable)
	{
		PlayableDirector component = gameObject.GetComponent<PlayableDirector>();
		if (component != null)
		{
			component.Play(playable);
		}
		PlayableDirector[] componentsInChildren = gameObject.GetComponentsInChildren<PlayableDirector>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play(playable);
		}
	}
}

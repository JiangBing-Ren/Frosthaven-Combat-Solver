using System.Collections;
using Chronos;
using UnityEngine;

public class AudioSMB : StateMachineBehaviour
{
	[Tooltip("The SFX to play during this animation.")]
	[AudioEventName]
	public string AudioEvent;

	[Tooltip("Audio delay in seconds.")]
	public float AudioDelay;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(Choreographer.s_Choreographer == null))
		{
			CoroutineHelper.RunCoroutine(AudioCoroutine(animator));
		}
	}

	private IEnumerator AudioCoroutine(Animator animator)
	{
		if (!string.IsNullOrEmpty(AudioEvent))
		{
			if (AudioDelay > 0f)
			{
				yield return new WaitForSeconds(AudioDelay / Timekeeper.instance.m_GlobalClock.timeScale);
			}
			AudioController.Play(AudioEvent, animator.gameObject.transform, null, attachToParent: false);
		}
	}
}

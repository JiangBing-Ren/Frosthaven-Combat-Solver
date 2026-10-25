using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;

public class SwitchBodySMB : StateMachineBehaviour
{
	[AudioEventName]
	public string AudioEvent;

	public List<VFXShared.SpawnedEffect> CasterEffects;

	public float TriggerSwitchDelay = 1.5f;

	public List<VFXShared.Progress> ProgressChoreographer;

	public bool ControlChoreographerWithStateBehaviour;

	private void Awake()
	{
		foreach (VFXShared.SpawnedEffect casterEffect in CasterEffects)
		{
			casterEffect.OriginalStartTime = casterEffect.StartTime;
		}
		foreach (VFXShared.Progress item in ProgressChoreographer)
		{
			item.OriginalProgressTime = item.ProgressTime;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Timekeeper.instance.m_GlobalClock.timeScale > 0f)
		{
			animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		}
		if (!(Choreographer.s_Choreographer == null))
		{
			CoroutineHelper.RunCoroutine(EnterCoroutine(animator, stateInfo, layerIndex));
		}
	}

	private IEnumerator EnterCoroutine(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: true, ControlChoreographerWithStateBehaviour ? this : null);
		VFXShared.PlaySpawnedEffectsFromTimeline(CasterEffects, animator.gameObject);
		if (!string.IsNullOrEmpty(AudioEvent) && animator != null)
		{
			AudioController.Play(AudioEvent, animator.gameObject.transform, null, attachToParent: false);
		}
		yield return Timekeeper.instance.WaitForSeconds(TriggerSwitchDelay);
		ActorBehaviour.GetMultibodyActorBehaviour(animator.gameObject).ApplySwitchBody();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!(Choreographer.s_Choreographer == null))
		{
			VFXShared.ControlChoreographer(control: false, ControlChoreographerWithStateBehaviour ? this : null);
		}
	}
}

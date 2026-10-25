using System.Collections;
using Chronos;
using UnityEngine;

public class UnblockChoreographerSMB : StateMachineBehaviour
{
	public float unblockAfter;

	private bool hasUnblocked;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		hasUnblocked = false;
		if (unblockAfter > 0f)
		{
			CoroutineHelper.RunCoroutine(UnblockAfter());
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!hasUnblocked)
		{
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
			hasUnblocked = true;
		}
	}

	private IEnumerator UnblockAfter()
	{
		yield return Timekeeper.instance.WaitForSeconds(unblockAfter);
		if (!hasUnblocked)
		{
			Choreographer.s_Choreographer.SetBlockClientMessageProcessing(active: false);
			hasUnblocked = true;
		}
	}
}

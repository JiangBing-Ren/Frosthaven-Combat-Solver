using System.Collections;
using Chronos;
using UnityEngine;

public class HideObject_SMB : StateMachineBehaviour
{
	public string ObjectName;

	public float HideAfterSec;

	public float HideForSec;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		CoroutineHelper.RunCoroutine(FireCoroutine(animator));
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GameObject gameObject = animator.gameObject.FindInChildren(ObjectName);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: true);
		}
	}

	private IEnumerator FireCoroutine(Animator animator)
	{
		GameObject targetObj = animator.gameObject.FindInChildren(ObjectName);
		if ((bool)targetObj)
		{
			yield return new WaitForSeconds(HideAfterSec / Timekeeper.instance.m_GlobalClock.timeScale);
			targetObj.SetActive(value: false);
			yield return new WaitForSeconds(HideForSec / Timekeeper.instance.m_GlobalClock.timeScale);
			targetObj.SetActive(value: true);
		}
	}
}

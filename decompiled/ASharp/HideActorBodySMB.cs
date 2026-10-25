using UnityEngine;

public class HideActorBodySMB : StateMachineBehaviour
{
	[Tooltip("Specify what child should be hidden")]
	public string bodyName;

	[Tooltip("Delay the hiding for seconds")]
	public float delay;

	private GameObject body;

	private Coroutine coroutine;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		body = animator.gameObject.FindInChildren(bodyName);
		if (body != null)
		{
			coroutine = CoroutineHelper.RunCoroutine(CoroutineHelper.DelayedStartCoroutine(delay, () =>
			{
				body.SetActive(value: false);
			}));
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (body != null)
		{
			CoroutineHelper.StopCoroutineHelper(coroutine);
			body.SetActive(value: true);
		}
	}
}

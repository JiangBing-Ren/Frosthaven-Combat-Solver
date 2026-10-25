using UnityEngine;

public class StopActorAnimation : MonoBehaviour
{
	private Animator stoppedAnimator;

	private void OnEnable()
	{
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(transform.parent.gameObject);
		if (!(actorBehaviour == null))
		{
			stoppedAnimator = actorBehaviour.GetAnimator();
			stoppedAnimator.enabled = false;
		}
	}

	private void OnDisable()
	{
		if ((bool)stoppedAnimator)
		{
			stoppedAnimator.enabled = true;
		}
	}

	private void OnDestroy()
	{
		if ((bool)stoppedAnimator)
		{
			stoppedAnimator.enabled = true;
		}
	}
}

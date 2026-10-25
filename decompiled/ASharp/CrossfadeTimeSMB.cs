using UnityEngine;

public class CrossfadeTimeSMB : StateMachineBehaviour
{
	public float TransitionTimeCalm = 0.15f;

	public float TransitionTimeAlert = 0.15f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(animator.gameObject);
		if ((bool)actorBehaviour)
		{
			actorBehaviour.OverrideCrossfadeTime(TransitionTimeCalm, TransitionTimeAlert);
		}
	}
}

using UnityEngine;

public class ProgressChoreographerSMB : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		ActorEvents component = animator.gameObject.GetComponent<ActorEvents>();
		if (component != null)
		{
			component.ProgressChoreographer();
			return;
		}
		UnityGameEditorObject componentInParent = animator.gameObject.GetComponentInParent<UnityGameEditorObject>();
		if (componentInParent?.PropObject?.RuntimeAttachedActor != null)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(componentInParent.PropObject.RuntimeAttachedActor);
			if (gameObject != null)
			{
				ActorEvents.GetActorEvents(gameObject)?.ProgressChoreographer();
			}
		}
	}
}

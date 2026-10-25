using Chronos;
using UnityEngine;
using UnityEngine.Playables;

public class PlayTimeline_SMB : StateMachineBehaviour
{
	public PlayableAsset timeLine;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		PlayableDirector component = animator.GetComponent<PlayableDirector>();
		if (component == null)
		{
			Log.ErrorFormat("Unable to find PlayableDirector component in PlayTimeline_SMB. GameObject: {0} | AnimatorController: {1}", animator.gameObject.name, animator.runtimeAnimatorController.name);
		}
		else
		{
			component.playableAsset = timeLine;
			component.RebuildGraph();
			component.playableGraph.GetRootPlayable(0).SetSpeed(Timekeeper.instance.m_GlobalClock.timeScale);
			component.Play();
		}
	}
}

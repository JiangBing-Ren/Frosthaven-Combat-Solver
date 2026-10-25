using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class FixBannerBounds_SMB : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(animator.gameObject);
		if (!(actorBehaviour.Actor is CHeroSummonActor { IsBanner: not false }))
		{
			return;
		}
		foreach (Renderer item in actorBehaviour.Renderers.Where((Renderer x) => x is SkinnedMeshRenderer))
		{
			Vector3 center = new Vector3(-0.0071943253f, -0.06973156f, 1.0633513f);
			Vector3 size = new Vector3(0.1940815f, 0.1009428f, 0.7713494f);
			item.localBounds = new Bounds(center, size);
		}
	}
}

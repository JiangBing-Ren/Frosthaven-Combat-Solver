using UnityEngine;

public class JumpInto_SMB : StateMachineBehaviour
{
	[AudioEventName]
	public string m_SFX;

	private GameObject phaseStartEffect;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, animator.gameObject.transform);
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(animator.gameObject.transform.parent.gameObject);
		if (actorBehaviour != null && !actorBehaviour.IsMoving)
		{
			phaseStartEffect = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseStart, null, animator.transform.position, animator.transform.rotation);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if ((bool)phaseStartEffect)
		{
			ObjectPool.Recycle(phaseStartEffect, VFXShared.GetEffectLifetime(phaseStartEffect), GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseStart, "@ VFX Pool");
		}
	}
}

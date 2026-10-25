using Chronos;
using UnityEngine;

public class DeathDissolveSMB : StateMachineBehaviour
{
	[Tooltip("If enabled ignore DeathDelay property and spawn always at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[Tooltip("Delay amount of seconds ater animation start playing")]
	[SerializeField]
	private float m_DeathDelay;

	private float m_InstanceDeathDelay;

	private bool m_NotDead;

	private float m_OriginalDeathDelay;

	private void Awake()
	{
		m_OriginalDeathDelay = m_DeathDelay;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		m_NotDead = true;
		float length = animatorStateInfo.length;
		m_InstanceDeathDelay = (m_OnAnimationEnd ? (length - 0.2f) : Mathf.Min(m_DeathDelay, length));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		m_InstanceDeathDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		if (m_InstanceDeathDelay <= 0f && m_NotDead)
		{
			Die(animator);
		}
	}

	private void Die(Animator animator)
	{
		m_NotDead = false;
		animator.gameObject.GetComponentInChildren<DeathDissolve>().ExternalPlay();
	}
}

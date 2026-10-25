using Chronos;
using UnityEngine;

public class SpawnObjectAtLocation_SMB : StateMachineBehaviour
{
	public GameObject particles;

	public float StartTime;

	private float m_OriginalStartTime;

	private void Awake()
	{
		m_OriginalStartTime = StartTime;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		CoroutineHelper.RunCoroutine(VFXShared.PlayEffectCoroutine(particles, animator.gameObject.transform, particles.transform.position, particles.transform.rotation, StartTime));
	}
}

using Chronos;
using UnityEngine;

public class SpawnObjectAtBone_SMB : StateMachineBehaviour
{
	public GameObject particles;

	public float lifetime = 5f;

	private Transform hookpos;

	public string hook;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		if ((bool)animator && (bool)animator.transform.Find(hook))
		{
			hookpos = animator.transform.Find(hook).transform;
			Object.Destroy(Object.Instantiate(particles, hookpos), lifetime);
		}
	}
}

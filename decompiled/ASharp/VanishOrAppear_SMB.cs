using Chronos;
using UnityEngine;

public class VanishOrAppear_SMB : StateMachineBehaviour
{
	public enum states
	{
		Appear,
		Vanish
	}

	public states vanishOrAppear;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		animator.GetComponent<VanishAndAppear>().enabled = true;
		if (vanishOrAppear == states.Appear)
		{
			animator.GetComponent<VanishAndAppear>().doAppear();
		}
		if (vanishOrAppear == states.Vanish)
		{
			animator.GetComponent<VanishAndAppear>().doVanish();
		}
	}
}

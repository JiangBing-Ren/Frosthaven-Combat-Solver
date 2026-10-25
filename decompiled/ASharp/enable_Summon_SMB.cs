using Chronos;
using UnityEngine;

public class enable_Summon_SMB : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		animator.GetComponent<SummonAppear>().enabled = true;
	}
}

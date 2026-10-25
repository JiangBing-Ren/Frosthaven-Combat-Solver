using System.Collections;
using Chronos;
using UnityEngine;

public class stopEquippedProjectileParticles : StateMachineBehaviour
{
	public GameObject Projectile;

	private GameObject projectileInstance;

	public float particleStopTime = 1f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		VFXShared.ControlChoreographer(control: true);
		projectileInstance = animator.gameObject.FindInChildren(Projectile.name);
		if (projectileInstance == null)
		{
			Log.Error("Error: Could not find projectile " + Projectile.name + " as a child of " + animator.gameObject.name);
		}
		else
		{
			CoroutineHelper.RunCoroutine(stopPartTimeline());
		}
	}

	private IEnumerator stopPartTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleStopTime);
		ParticleSystem[] componentsInChildren = projectileInstance.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop(withChildren: true);
		}
	}
}

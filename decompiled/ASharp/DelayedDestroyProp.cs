using System.Collections;
using System.Collections.Generic;
using Chronos;
using FH.ApparanceBaking;
using ScenarioRuleLibrary;
using UnityEngine;

public class DelayedDestroyProp : MonoBehaviour
{
	public float destroyAfterTime;

	public bool disableAllParticleLoops;

	private List<ParticleSystem> UnloopedParticles = new List<ParticleSystem>();

	public void DelayDestroy(CObjectProp prop)
	{
		StartCoroutine(DestoryTimeline(prop));
	}

	public IEnumerator DestoryTimeline(CObjectProp prop)
	{
		if (disableAllParticleLoops)
		{
			UnloopedParticles.Clear();
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				if (main.loop)
				{
					UnloopedParticles.Add(particleSystem);
				}
				main.loop = false;
			}
		}
		yield return Timekeeper.instance.WaitForSeconds(destroyAfterTime);
		EnvironmentLoader.Instance.BeforePropDestroyed(gameObject, prop);
		Object.Destroy(gameObject);
	}

	public void Restore()
	{
		StopAllCoroutines();
		foreach (ParticleSystem unloopedParticle in UnloopedParticles)
		{
			ParticleSystem.MainModule main = unloopedParticle.main;
			main.loop = true;
			unloopedParticle.Clear();
			unloopedParticle.Play();
		}
	}
}

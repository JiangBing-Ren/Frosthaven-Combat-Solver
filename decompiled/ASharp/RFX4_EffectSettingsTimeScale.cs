using System;
using UnityEngine;

public class RFX4_EffectSettingsTimeScale : MonoBehaviour
{
	public float Time = 1f;

	private float prevTime;

	private ParticleSystem[] particles;

	private void Start()
	{
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (Math.Abs(prevTime - Time) > 0.0001f)
		{
			prevTime = Time;
			UpdateTime();
		}
	}

	private void UpdateTime()
	{
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.MainModule main = array[i].main;
			main.simulationSpeed = Time;
		}
		RFX4_TransformMotion componentInChildren = GetComponentInChildren<RFX4_TransformMotion>();
		if (componentInChildren != null)
		{
			componentInChildren.Speed *= Time;
			GameObject[] effectsOnCollision = componentInChildren.EffectsOnCollision;
			for (int i = 0; i < effectsOnCollision.Length; i++)
			{
				array = effectsOnCollision[i].GetComponentsInChildren<ParticleSystem>();
				for (int j = 0; j < array.Length; j++)
				{
					ParticleSystem.MainModule main2 = array[j].main;
					main2.simulationSpeed = Time;
				}
			}
		}
		RFX4_ShaderFloatCurve[] componentsInChildren = GetComponentsInChildren<RFX4_ShaderFloatCurve>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GraphTimeMultiplier /= Time;
		}
		RFX4_ShaderColorGradient[] componentsInChildren2 = GetComponentsInChildren<RFX4_ShaderColorGradient>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].TimeMultiplier /= Time;
		}
		RFX4_LightCurves[] componentsInChildren3 = GetComponentsInChildren<RFX4_LightCurves>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].GraphTimeMultiplier /= Time;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using SETUtil;
using SETUtil.Materials;
using UnityEngine;

public class ParticlesFadeScript : MonoBehaviour
{
	public float fadeTime;

	public bool fadeByLoopOff;

	public ParticleSystem[] fadeParticles;

	public ParticleSystem[] fadeParticlesOpacityValue;

	public ParticleSystem[] fadeParticleTrails;

	public AnimationCurve FadeCurve;

	public float startTime;

	[HideInInspector]
	public float fadeOnImpactMult;

	public Light particleLight;

	public bool StartOnHit;

	public bool alsoFade_Opacity;

	private float timePassed;

	private float val;

	private float animTime;

	private Coroutine fadeCoroutine;

	private bool isCoroutineRunning;

	private List<Material> _fadeParticleMaterials = new List<Material>();

	private List<Material> _fadeTrailMaterials = new List<Material>();

	private static readonly int FadeProperty = Shader.PropertyToID("_Opac");

	private static readonly int OpacityProperty = Shader.PropertyToID("_Opacity");

	private void Awake()
	{
		if (fadeByLoopOff)
		{
			return;
		}
		ParticleSystem[] array = fadeParticleTrails;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystemRenderer component = array[i].gameObject.GetComponent<ParticleSystemRenderer>();
			if (!(component.trailMaterial == null))
			{
				Material item = (component.trailMaterial = new Material(component.trailMaterial));
				_fadeTrailMaterials.Add(item);
			}
		}
		array = fadeParticles;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystemRenderer component2 = array[i].gameObject.GetComponent<ParticleSystemRenderer>();
			if (component2.enabled && component2.renderMode != ParticleSystemRenderMode.None)
			{
				_fadeParticleMaterials.AddRange(component2.GetMaterialClonesSlow());
			}
		}
	}

	private void OnEnable()
	{
		if (!StartOnHit)
		{
			fadeCoroutine = StartCoroutine(Fade());
		}
		else
		{
			FireSpawnedProjectileSMB.SpawnedProjectileHit += HandleOnSpawnedProjectileHit;
		}
	}

	private void OnDisable()
	{
		if (isCoroutineRunning)
		{
			StopCoroutine(fadeCoroutine);
		}
		if (fadeByLoopOff)
		{
			ParticleSystem[] array = fadeParticles;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.MainModule main = array[i].main;
				main.loop = true;
			}
		}
		else
		{
			for (int j = 0; j < _fadeParticleMaterials.Count; j++)
			{
				SceneUtil.SmartDestroy(_fadeParticleMaterials[j]);
			}
			for (int k = 0; k < _fadeTrailMaterials.Count; k++)
			{
				SceneUtil.SmartDestroy(_fadeTrailMaterials[k]);
			}
			_fadeParticleMaterials.Clear();
			_fadeTrailMaterials.Clear();
		}
		if (StartOnHit)
		{
			FireSpawnedProjectileSMB.SpawnedProjectileHit -= HandleOnSpawnedProjectileHit;
		}
	}

	private void HandleOnSpawnedProjectileHit(object sender, EventArgs e)
	{
		fadeCoroutine = StartCoroutine(Fade());
	}

	private IEnumerator Fade()
	{
		isCoroutineRunning = true;
		fadeOnImpactMult = 1f;
		timePassed = 0f;
		if (startTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(startTime);
			timePassed = startTime;
		}
		if (fadeByLoopOff)
		{
			ParticleSystem[] array = fadeParticles;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.MainModule main = array[i].main;
				main.loop = false;
			}
		}
		else
		{
			if ((bool)particleLight)
			{
				particleLight.intensity = 0f;
			}
			while (timePassed <= fadeTime + startTime * fadeOnImpactMult)
			{
				timePassed += Timekeeper.instance.m_GlobalClock.deltaTime;
				animTime = timePassed / fadeTime;
				val = FadeCurve.Evaluate(animTime);
				for (int j = 0; j < _fadeParticleMaterials.Count; j++)
				{
					_fadeParticleMaterials[j].SetFloat(FadeProperty, val * val);
					if (alsoFade_Opacity)
					{
						_fadeParticleMaterials[j].SetFloat(OpacityProperty, val * val);
					}
				}
				for (int k = 0; k < _fadeTrailMaterials.Count; k++)
				{
					_fadeTrailMaterials[k].SetFloat(FadeProperty, val);
				}
				yield return new WaitForEndOfFrame();
			}
		}
		isCoroutineRunning = false;
	}
}

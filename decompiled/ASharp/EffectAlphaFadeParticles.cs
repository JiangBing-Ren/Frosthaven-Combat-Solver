using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class EffectAlphaFadeParticles : MonoBehaviour
{
	public bool fadeOut;

	private List<ParticleSystemRenderer> emitters;

	private bool fadeStarted;

	private bool lastState;

	private Color[] theirColours;

	private Color[] theirTrailColours;

	public float animTime = 0.6f;

	private void Start()
	{
		emitters = GetComponentsInChildren<ParticleSystemRenderer>().ToList();
		for (int i = 0; i < emitters.Count; i++)
		{
			if (!emitters[i].sharedMaterial.HasProperty("_TintColor"))
			{
				emitters.Remove(emitters[i]);
				i--;
			}
		}
		theirColours = new Color[emitters.Count];
		theirTrailColours = new Color[emitters.Count];
		for (int j = 0; j < emitters.Count; j++)
		{
			Material material = emitters[j].GetMaterialClonesSlow().Single();
			theirColours[j] = material.GetColor("_TintColor");
			if (emitters[j].trailMaterial != null && emitters[j].trailMaterial.HasProperty("_TintColor"))
			{
				theirTrailColours[j] = emitters[j].trailMaterial.GetColor("_TintColor");
				emitters[j].trailMaterial = new Material(emitters[j].trailMaterial);
			}
		}
	}

	private void Update()
	{
		if (fadeOut != lastState && !fadeStarted)
		{
			lastState = fadeOut;
			if (fadeOut)
			{
				fadeIt(isOn: true);
			}
			else
			{
				fadeIt(isOn: false);
			}
		}
	}

	private void fadeIt(bool isOn)
	{
		fadeStarted = true;
		if (isOn)
		{
			StartCoroutine(fadeTimelineIn());
		}
		else
		{
			StartCoroutine(fadeTimelineOut());
		}
	}

	private IEnumerator fadeTimelineIn()
	{
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			float num = Timekeeper.instance.m_GlobalClock.time - startTime;
			for (int i = 0; i < emitters.Count; i++)
			{
				emitters[i].GetMaterialClonesSlow()[0].SetColor("_TintColor", new Color(theirColours[i].r, theirColours[i].g, theirColours[i].b, 1f - num / animTime));
				if (emitters[i].trailMaterial != null && emitters[i].trailMaterial.HasProperty("_TintColor"))
				{
					emitters[i].trailMaterial.SetColor("_TintColor", new Color(theirColours[i].r, theirColours[i].g, theirColours[i].b, 1f - num / animTime));
				}
			}
			fadeStarted = false;
			yield return null;
		}
	}

	private IEnumerator fadeTimelineOut()
	{
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			float num = Timekeeper.instance.m_GlobalClock.time - startTime;
			for (int i = 0; i < emitters.Count; i++)
			{
				emitters[i].GetMaterialClonesSlow()[0].SetColor("_TintColor", new Color(theirColours[i].r, theirColours[i].g, theirColours[i].b, Mathf.Lerp(0f, theirColours[i].a, num / animTime)));
				if (emitters[i].trailMaterial != null && emitters[i].trailMaterial.HasProperty("_TintColor"))
				{
					emitters[i].trailMaterial.SetColor("_TintColor", new Color(theirColours[i].r, theirColours[i].g, theirColours[i].b, Mathf.Lerp(0f, theirColours[i].a, num / animTime)));
				}
			}
			fadeStarted = false;
			yield return null;
		}
	}
}

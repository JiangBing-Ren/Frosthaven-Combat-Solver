using System;
using System.Collections;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
	private Renderer[] Breakables;

	private Material[][] BreakMats;

	[Obsolete]
	public ParticleSystem p_breakeffect;

	[Header("Use this one, ignore the above one:")]
	public GameObject BreakEffect;

	public float FadeDelay = 0.4f;

	public bool ManualMeshOverride;

	public Renderer[] OverrideMeshes;

	public bool debugBreak;

	public float debugWaitTime;

	private bool animCutoutNotOpacity;

	private Coroutine breakCoroutine;

	private bool hasBeenBroken;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void BreakObject()
	{
		breakCoroutine = StartCoroutine(BreakTimeline());
	}

	private void InitBreakables()
	{
		Breakables = GetComponentsInChildren<Renderer>(includeInactive: true);
		BreakMats = new Material[Breakables.Length][];
		for (int i = 0; i < Breakables.Length; i++)
		{
			BreakMats[i] = Breakables[i].GetMaterialClonesSlow();
		}
	}

	public IEnumerator BreakTimeline()
	{
		hasBeenBroken = true;
		if (debugBreak)
		{
			debugBreak = false;
		}
		else
		{
			debugWaitTime = 0f;
		}
		yield return Timekeeper.instance.WaitForSeconds(debugWaitTime);
		InitBreakables();
		if (BreakEffect != null)
		{
			UnityEngine.Object.Instantiate(BreakEffect, transform.position, transform.rotation, transform);
		}
		if (FadeDelay > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(FadeDelay);
		}
		float timer = 1f;
		while (timer > 0f)
		{
			timer -= Timekeeper.instance.m_GlobalClock.deltaTime;
			for (int i = 0; i < Breakables.Length; i++)
			{
				for (int j = 0; j < BreakMats[i].Length; j++)
				{
					if (!(BreakMats[i][j] == null))
					{
						BreakMats[i][j].SetFloat("_DeathDissolve", 1f - timer);
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
		Renderer[] breakables = Breakables;
		for (int k = 0; k < breakables.Length; k++)
		{
			breakables[k].enabled = false;
		}
		breakCoroutine = null;
	}

	public void Restore()
	{
		InitBreakables();
		if (Breakables == null)
		{
			return;
		}
		if (breakCoroutine != null)
		{
			StopCoroutine(breakCoroutine);
			breakCoroutine = null;
		}
		for (int i = 0; i < Breakables.Length; i++)
		{
			Renderer renderer = Breakables[i];
			if (hasBeenBroken)
			{
				renderer.enabled = true;
			}
			if (BreakMats == null)
			{
				continue;
			}
			for (int j = 0; j < BreakMats[i].Length; j++)
			{
				Material material = BreakMats[i][j];
				if (!(material == null))
				{
					material.SetFloat("_DeathDissolve", 0f);
				}
			}
		}
		hasBeenBroken = false;
	}
}

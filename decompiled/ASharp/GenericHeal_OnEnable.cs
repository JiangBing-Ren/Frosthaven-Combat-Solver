using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class GenericHeal_OnEnable : EffectOnEnable
{
	public AnimationCurve Curve;

	public float AnimStrength = 1f;

	public float AnimTime = 1f;

	public Color glowColor = new Color(1f, 0f, 1f, 1f);

	public float emissiveMapMask;

	private List<SkinnedMeshRenderer> m_rend;

	private float m_animTimeTaken;

	private float m_startTime;

	private string m_animProperty = "_Glow";

	private string m_colorProperty = "_GlowColor";

	private string m_maskProperty = "_EmissiveMapAsMask";

	public string animPropertyOverride;

	public string animColorOverride;

	public Texture2D emissiveMapOverrride;

	public bool enableAnimPropertyOverride;

	public bool enableAnimColorOverride;

	public bool enableEmissiveMapOverride;

	private Color[] initialCol;

	private float[] initialGlow;

	private void OnEnable()
	{
		if (enableAnimPropertyOverride)
		{
			m_animProperty = animPropertyOverride;
		}
		if (enableAnimColorOverride)
		{
			m_colorProperty = animColorOverride;
		}
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		if (component != null)
		{
			component.Play();
		}
		ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
		m_animTimeTaken = 0f;
		if (!(transform.parent != null))
		{
			return;
		}
		m_rend = transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
		Log.Info(name + ": " + m_rend.ToString());
		for (int j = 0; j < m_rend.Count; j++)
		{
			if (!m_rend[j].sharedMaterial.HasProperty(m_animProperty) || !m_rend[j].sharedMaterial.HasProperty(m_colorProperty))
			{
				m_rend.Remove(m_rend[j]);
			}
		}
		initialGlow = new float[m_rend.Count];
		initialCol = new Color[m_rend.Count];
		for (int k = 0; k < m_rend.Count; k++)
		{
			if (m_rend[k].sharedMesh != null)
			{
				initialGlow[k] = m_rend[k].sharedMaterial.GetFloat(m_animProperty);
				initialCol[k] = m_rend[k].sharedMaterial.GetColor(m_colorProperty);
				if (enableEmissiveMapOverride)
				{
					m_rend[k].GetMaterialClonesSlow()[0].SetTexture("_EmissiveMap", emissiveMapOverrride);
				}
			}
		}
		StartCoroutine(HealGlowCoroutine(m_startTime));
	}

	private IEnumerator HealGlowCoroutine(float startTime)
	{
		foreach (SkinnedMeshRenderer item in m_rend)
		{
			if (item.sharedMesh != null)
			{
				Material obj = item.GetMaterialClonesSlow()[0];
				obj.SetColor(m_colorProperty, glowColor);
				obj.SetFloat(m_maskProperty, emissiveMapMask);
			}
		}
		startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < AnimTime)
		{
			m_animTimeTaken = (Timekeeper.instance.m_GlobalClock.time - startTime) / AnimTime;
			float value = Curve.Evaluate(m_animTimeTaken) * AnimStrength;
			foreach (SkinnedMeshRenderer item2 in m_rend)
			{
				if (item2.sharedMesh != null)
				{
					item2.gameObject.GetComponent<SkinnedMeshRenderer>().GetMaterialClonesSlow()[0].SetFloat(m_animProperty, value);
				}
			}
			yield return null;
		}
		for (int i = 0; i < m_rend.Count; i++)
		{
			if (m_rend[i].sharedMesh != null)
			{
				Material obj2 = m_rend[i].GetMaterialClonesSlow()[0];
				obj2.SetColor(m_colorProperty, initialCol[i]);
				obj2.SetFloat(m_animProperty, initialGlow[i]);
			}
		}
		Log.Info("Initial Glow Values Applied");
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class TeleportCharacterScript : MonoBehaviour
{
	private int skinMeshCount;

	public Color TeleportColour = new Color(1f, 1f, 1f, 1f);

	public float teleportDelay = 1f;

	public float animTime = 1f;

	public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float particleIdleWaitTime = 1f;

	public bool teleportOut = true;

	public bool ToggleDissolveBackOff;

	public GameObject MeshEmitter;

	public Texture2D EmitterMeshTexture;

	private Color[][] m_SkinnedMeshesCol;

	private GameObject m_particleInstance;

	private SkinnedMeshRenderer m_EmitterMesh;

	private SkinnedMeshRenderer[] m_SkinnedMeshes;

	private Material[][] m_SkinnedMeshMaterials;

	private VFXLookup m_VFXLookup;

	private GameObject particlesIdle;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
	}

	private void Initialize()
	{
		GameObject[] array = transform.GetComponentInParent<CharacterManager>().EquippedWeapons.ToArray();
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		m_VFXLookup = transform.GetComponentInParent<VFXLookup>();
		particlesIdle = transform.GetComponentInParent<DeathDissolve>().particlesIdle;
		int num = m_VFXLookup.CharacterRenderers.Count((Renderer x) => x is SkinnedMeshRenderer);
		m_SkinnedMeshes = new SkinnedMeshRenderer[num];
		int num2 = 0;
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				m_SkinnedMeshes[num2] = skinnedMeshRenderer;
				num2++;
			}
		}
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			SkinnedMeshRenderer componentInChildren = array[num3].GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				list.Add(componentInChildren);
			}
		}
		m_SkinnedMeshes = m_SkinnedMeshes.Concat(list).ToArray();
		skinMeshCount = m_SkinnedMeshes.Length;
		m_SkinnedMeshMaterials = new Material[skinMeshCount][];
		m_SkinnedMeshesCol = new Color[skinMeshCount][];
		for (int num4 = 0; num4 < skinMeshCount; num4++)
		{
			Material[] materialClonesSlow = m_SkinnedMeshes[num4].GetMaterialClonesSlow();
			m_SkinnedMeshMaterials[num4] = materialClonesSlow;
			m_SkinnedMeshesCol[num4] = new Color[materialClonesSlow.Length];
			for (int num5 = 0; num5 < m_SkinnedMeshMaterials[num4].Length; num5++)
			{
				m_SkinnedMeshesCol[num4][num5] = materialClonesSlow[num5].GetColor("_CindersColour");
				materialClonesSlow[num5].SetColor("_CindersColour", TeleportColour);
				materialClonesSlow[num5].SetFloat("_Toggle_FlipDissolveDirection", 1f);
				materialClonesSlow[num5].SetFloat("_Toggle_Dissolve", 1f);
			}
		}
		Teleport();
	}

	private void Teleport()
	{
		m_EmitterMesh = m_VFXLookup.CharacterBodyMesh;
		m_particleInstance = ObjectPool.Spawn(MeshEmitter, gameObject.transform);
		ParticleSystem.ShapeModule shape = m_particleInstance.GetComponent<ParticleSystem>().shape;
		shape.skinnedMeshRenderer = m_EmitterMesh;
		shape.texture = EmitterMeshTexture;
		m_particleInstance.GetComponent<ParticleSystem>().Play();
		if (particlesIdle != null)
		{
			if (teleportOut)
			{
				StartCoroutine(ParticleIdleWaitOut());
			}
			else
			{
				StartCoroutine(ParticleIdleWaitIn());
			}
		}
		StartCoroutine(TeleportOutTimeline());
	}

	public IEnumerator ParticleIdleWaitOut()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime);
		particlesIdle.GetComponent<ParticleSystem>().Stop();
		Log.Info("TeleOut");
	}

	public IEnumerator ParticleIdleWaitIn()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime);
		particlesIdle.GetComponent<ParticleSystem>().Play();
		Log.Info("TeleIn");
	}

	public IEnumerator TeleportOutTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(teleportDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			for (int i = 0; i < skinMeshCount; i++)
			{
				for (int j = 0; j < m_SkinnedMeshMaterials[i].Length; j++)
				{
					m_SkinnedMeshMaterials[i][j].SetFloat("_Cutout", FadeCurve.Evaluate(dTime));
				}
			}
			yield return new WaitForEndOfFrame();
		}
		StopEffect();
	}

	private void StopEffect()
	{
		Log.Info("Cleanup started");
		if (particlesIdle != null)
		{
			particlesIdle.SetActive(value: true);
		}
		for (int i = 0; i < skinMeshCount; i++)
		{
			Material[] materialClonesSlow = m_SkinnedMeshes[i].GetMaterialClonesSlow();
			for (int j = 0; j < materialClonesSlow.Length; j++)
			{
				materialClonesSlow[j].SetColor("_CindersColour", m_SkinnedMeshesCol[i][j]);
				materialClonesSlow[j].SetFloat("_Toggle_FlipDissolveDirection", 0f);
				if (ToggleDissolveBackOff)
				{
					materialClonesSlow[j].SetFloat("_Toggle_Dissolve", 0f);
				}
			}
		}
	}
}

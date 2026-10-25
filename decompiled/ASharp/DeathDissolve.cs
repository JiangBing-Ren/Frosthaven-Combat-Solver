using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

public class DeathDissolve : MonoBehaviour
{
	public GameObject Particles;

	public bool ShowParticleEffect = true;

	public float DissolveLifeTime = 5f;

	public float DissolveInitialDelay = 0.2f;

	public AnimationCurve dissolveCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float AnimStrength = 1f;

	public Renderer CharacterMesh;

	private const string m_animProperty = "_DeathDissolve";

	private float m_animTimeTaken;

	private GameObject m_particleInstance;

	public GameObject particlesIdle;

	public float particleIdleWaitTime;

	[AudioEventName]
	[SerializeField]
	private string m_SFXOnDissolve;

	[SerializeField]
	[Tooltip("Audio timeout work in parallel to 'Initial Delay' (they not summed up)")]
	private float m_SFXTimeout;

	private bool m_playedExternally;

	[Tooltip("Disable all Renderer components after the dissolve animation completes.")]
	public bool DisableRendererAtEnd;

	private HashSet<Renderer> m_renderers;

	public static List<DeathDissolve> s_DeathDissolvesInProgress = new List<DeathDissolve>();

	public IEnumerator Play(bool cleanUpAtEnd = true)
	{
		if (!s_DeathDissolvesInProgress.Contains(this))
		{
			s_DeathDissolvesInProgress.Add(this);
		}
		CoroutineHelper.instance.StartCoroutine(ParticleIdleWait());
		CoroutineHelper.instance.StartCoroutine(PlaySFX());
		if (ShowParticleEffect)
		{
			if (!m_playedExternally)
			{
				yield return Timekeeper.instance.WaitForSeconds(DissolveInitialDelay);
			}
			if (CharacterMesh != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault((SkinnedMeshRenderer x) => x.name == CharacterMesh.name);
				if (skinnedMeshRenderer != null)
				{
					m_particleInstance = ObjectPool.Spawn(Particles, gameObject.transform);
					ParticleSystem component = m_particleInstance.GetComponent<ParticleSystem>();
					component.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
					ParticleSystem.ShapeModule shape = component.shape;
					shape.skinnedMeshRenderer = skinnedMeshRenderer;
					ParticleSystem.ShapeModule shape2 = m_particleInstance.transform.Find("Bits").GetComponent<ParticleSystem>().shape;
					shape2.skinnedMeshRenderer = skinnedMeshRenderer;
					ParticleSystem.ShapeModule shape3 = m_particleInstance.transform.Find("Fog").GetComponent<ParticleSystem>().shape;
					shape3.skinnedMeshRenderer = skinnedMeshRenderer;
					component.Play();
				}
			}
			else
			{
				Log.Error("CharacterMesh of DeathDissolve needs to be assigned for " + gameObject.name + ". VFX won't be player");
			}
		}
		m_renderers = gameObject.GetComponentsInChildren<Renderer>().ToHashSet();
		m_animTimeTaken = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < DissolveLifeTime)
		{
			if (m_animTimeTaken < DissolveLifeTime)
			{
				m_animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime / DissolveLifeTime;
				float value = dissolveCurve.Evaluate(m_animTimeTaken) * AnimStrength;
				foreach (Renderer renderer in m_renderers)
				{
					if (!(renderer != null))
					{
						continue;
					}
					Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
					foreach (Material material in materialClonesSlow)
					{
						if ((bool)material)
						{
							material.SetFloat("_DeathDissolve", value);
						}
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
		if (DisableRendererAtEnd)
		{
			foreach (Renderer renderer2 in m_renderers)
			{
				renderer2.enabled = false;
			}
		}
		if (cleanUpAtEnd)
		{
			Cleanup();
		}
		else
		{
			s_DeathDissolvesInProgress.Remove(this);
		}
	}

	private void Cleanup()
	{
		transform.parent.gameObject.SetActive(value: false);
		if (particlesIdle != null)
		{
			particlesIdle.SetActive(value: true);
		}
		if (m_renderers != null && m_renderers.Count > 0)
		{
			foreach (Renderer renderer in m_renderers)
			{
				Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
				foreach (Material material in materialClonesSlow)
				{
					if ((bool)material && material.HasProperty("_DeathDissolve"))
					{
						material.SetFloat("_DeathDissolve", 0f);
					}
				}
			}
		}
		if (ShowParticleEffect && m_particleInstance != null)
		{
			ObjectPool.Recycle(m_particleInstance, Particles);
		}
		CharacterManager characterManager = CharacterManager.GetCharacterManager(transform.parent.gameObject);
		if ((bool)characterManager)
		{
			Choreographer.CharacterOutlineRegistration(characterManager, register: false);
			characterManager.DeinitializeCharacter();
			if (characterManager.CharacterActor.Type == CActor.EType.Player)
			{
				characterManager.gameObject.transform.position = Choreographer.s_Choreographer.DeadPlayerLocation;
			}
			else
			{
				ObjectPool.Recycle(characterManager.gameObject, characterManager.CharacterPrefab);
			}
		}
		s_DeathDissolvesInProgress.Remove(this);
	}

	private IEnumerator ParticleIdleWait()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime + (m_playedExternally ? 0f : DissolveInitialDelay));
		if (particlesIdle != null)
		{
			ParticleSystem component = particlesIdle.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Stop();
			}
		}
	}

	private IEnumerator PlaySFX()
	{
		yield return Timekeeper.instance.WaitForSeconds(m_SFXTimeout);
		if (!string.IsNullOrEmpty(m_SFXOnDissolve))
		{
			AudioController.Play(m_SFXOnDissolve, transform);
		}
	}

	public void ExternalPlay()
	{
		m_playedExternally = true;
		StartCoroutine(Play(cleanUpAtEnd: false));
	}

	public void ExternalCleanup()
	{
		if (m_playedExternally)
		{
			if (!s_DeathDissolvesInProgress.Contains(this))
			{
				s_DeathDissolvesInProgress.Add(this);
			}
			StopCoroutine(Play());
			Cleanup();
			m_playedExternally = false;
		}
	}

	public bool PlayedExternally()
	{
		return m_playedExternally;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		s_DeathDissolvesInProgress.Remove(this);
	}
}

using System.Collections;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class VanishAndAppear : MonoBehaviour
{
	public GameObject Particles;

	public bool ShowParticleEffect = true;

	public float Lifetime = 5f;

	public float initialDelay;

	public float particleDelay;

	public AnimationCurve Curve;

	public float AnimStrength = 1f;

	public float AnimTime = 1f;

	public SkinnedMeshRenderer CharacterMesh;

	private const string m_dissolveToggle = "_Toggle_Dissolve";

	private const string m_animProperty = "_Cutout";

	private float m_animTimeTaken;

	private GameObject m_particleInstance;

	private SkinnedMeshRenderer[] m_renderers;

	public GameObject ParticlesIdle;

	public float particleIdleWaitTime;

	private bool isVisible;

	[AudioEventName]
	[SerializeField]
	private string m_SFXOnAppear;

	[SerializeField]
	[Tooltip("Audio timeout work in parallel to 'Initial Delay' (they not summed up)")]
	private float m_SFXTimeout;

	private void Awake()
	{
		enabled = false;
	}

	private void OnDisable()
	{
		enabled = false;
	}

	private void OnEnable()
	{
		m_renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] renderers = m_renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materialClonesSlow = renderers[i].GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (material.HasProperty("_Toggle_Dissolve"))
				{
					material.SetFloat("_Toggle_Dissolve", 1f);
				}
				material.SetFloat("_Cutout", 1f);
			}
		}
	}

	public void doVanish()
	{
		isVisible = true;
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
		if (actorBehaviour != null)
		{
			actorBehaviour.m_WorldspacePanelUI?.Hide();
		}
		ExecuteFade();
	}

	public void doAppear()
	{
		isVisible = false;
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(gameObject);
		if (actorBehaviour != null)
		{
			actorBehaviour.m_WorldspacePanelUI?.Show();
		}
		ExecuteFade();
	}

	private void ExecuteFade()
	{
		if (ShowParticleEffect)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault((SkinnedMeshRenderer x) => x.name == CharacterMesh.name);
			m_particleInstance = ObjectPool.Spawn(Particles, gameObject.transform, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f));
			ParticleSystem.ShapeModule shape = m_particleInstance.GetComponent<ParticleSystem>().shape;
			shape.skinnedMeshRenderer = skinnedMeshRenderer;
			ParticleSystem.ShapeModule shape2 = m_particleInstance.transform.Find("Bits").GetComponent<ParticleSystem>().shape;
			shape2.skinnedMeshRenderer = skinnedMeshRenderer;
			ParticleSystem.ShapeModule shape3 = m_particleInstance.transform.Find("Fog").GetComponent<ParticleSystem>().shape;
			shape3.skinnedMeshRenderer = skinnedMeshRenderer;
			m_particleInstance.GetComponent<ParticleSystem>().Play();
		}
		SkinnedMeshRenderer[] renderers = m_renderers;
		for (int num = 0; num < renderers.Length; num++)
		{
			Material[] materialClonesSlow = renderers[num].GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (material.HasProperty("_Toggle_Dissolve"))
				{
					material.SetFloat("_Toggle_Dissolve", 1f);
				}
			}
		}
		StartCoroutine(Play());
		if ((bool)ParticlesIdle)
		{
			StartCoroutine(ParticleIdleWait());
		}
		StartCoroutine(PlaySFX());
	}

	private IEnumerator PlaySFX()
	{
		if (m_SFXTimeout > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(m_SFXTimeout);
		}
		if (!string.IsNullOrEmpty(m_SFXOnAppear))
		{
			AudioController.Play(m_SFXOnAppear, transform);
		}
	}

	public IEnumerator ParticleIdleWait()
	{
		if (!isVisible)
		{
			if (particleIdleWaitTime + initialDelay > 0f)
			{
				ParticlesIdle.SetActive(value: false);
				yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime + initialDelay);
			}
			ParticlesIdle.SetActive(value: true);
		}
		else
		{
			if (particleIdleWaitTime + initialDelay > 0f)
			{
				yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime + initialDelay);
			}
			ParticlesIdle.SetActive(value: false);
		}
		yield return null;
	}

	public IEnumerator Play()
	{
		m_animTimeTaken = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < Lifetime)
		{
			if (m_animTimeTaken < Lifetime)
			{
				m_animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime / AnimTime;
				float num = Curve.Evaluate(m_animTimeTaken) * AnimStrength;
				SkinnedMeshRenderer[] renderers = m_renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null && isVisible)
					{
						Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
						for (int j = 0; j < materialClonesSlow.Length; j++)
						{
							materialClonesSlow[j].SetFloat("_Cutout", num);
						}
					}
					if (renderer != null && !isVisible)
					{
						Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
						for (int j = 0; j < materialClonesSlow.Length; j++)
						{
							materialClonesSlow[j].SetFloat("_Cutout", 1f - num);
						}
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}
}

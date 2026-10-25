using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using UnityEngine;
using UnityEngine.Playables;

public class PropHealthState : MonoBehaviour
{
	[AudioEventName]
	public string HurtAudio;

	[AudioEventName]
	public string[] AudioEvents;

	public GameObject[] TransitionVFX;

	[Tooltip("Reserve the last state for 0HP only")]
	public bool LastStateIsDeath;

	public bool UseDeathDissolve;

	public GameObject[] EntangledObjectPrefabs = new GameObject[0];

	public float EntanglementRadius = 3f;

	private GameObject[] _statesGO;

	private float DissolvePeriod = 2f;

	private int _prevHealth = int.MinValue;

	private bool _prevIsDead;

	private int _prevVFXIndex = -1;

	private int _prevAudioIndex = -1;

	private MaterialModification.MaterialCache materialCache;

	private List<Renderer> renderers = new List<Renderer>();

	private bool isDeathDisolved;

	private static readonly int SplatMaxPropertyName = Shader.PropertyToID("_SplatMax");

	private static readonly int SplatCountPropertyName = Shader.PropertyToID("_SplatCount");

	private static readonly int DissolvePropertyName = Shader.PropertyToID("_DeathDissolve");

	private void InitMaterialCache()
	{
		if (materialCache == null)
		{
			materialCache = new MaterialModification.MaterialCache();
		}
		foreach (Renderer renderer in renderers)
		{
			renderer.GetMaterialClonesSlow(materialCache);
		}
	}

	public void Awake()
	{
		_statesGO = (from tr in base.gameObject.FindInImmediateChildren<Transform>()
			where tr.name.StartsWith("State")
			select tr.gameObject).ToArray();
		if (_statesGO.Length == 0)
		{
			_statesGO = (from tr in transform.GetComponentsInChildren<Transform>(includeInactive: true)
				where tr.name.StartsWith("State")
				select tr.gameObject).ToArray();
			if (_statesGO.Length != 0)
			{
				_ = _statesGO[0].transform.parent;
			}
		}
		renderers = GetComponentsInChildren<Renderer>().ToList();
		if (Application.isPlaying)
		{
			InitMaterialCache();
		}
		GameObject[] entangledObjectPrefabs = EntangledObjectPrefabs;
		foreach (GameObject gameObject in entangledObjectPrefabs)
		{
			if (gameObject.name == base.gameObject.name)
			{
				Log.Error("Using entangled health state prop prefab with the same name (" + gameObject.name + ") as the master prop, that's illegal.");
			}
		}
		SetMaterialProperties(base.gameObject, 0f);
	}

	public void UpdateState(int currentHealth, int maxHealth, bool isActorDead = false)
	{
		if (base.gameObject == null)
		{
			return;
		}
		currentHealth = Math.Max(currentHealth, 0);
		if (_prevHealth == currentHealth && _prevIsDead == isActorDead)
		{
			return;
		}
		if (base.gameObject.CompareTag("EntangledHealthProp"))
		{
			ApparanceEntity componentInParent = GetComponentInParent<ApparanceEntity>();
			if ((bool)componentInParent)
			{
				componentInParent.Frozen = true;
			}
		}
		if (EntangledObjectPrefabs.Length != 0)
		{
			PropHealthState[] source = (from go in GameObject.FindGameObjectsWithTag("EntangledHealthProp")
				select go.GetComponent<PropHealthState>() into hs
				where Vector3.Distance(transform.position, hs.transform.position) < EntanglementRadius
				select hs).ToArray();
			GameObject[] entangledObjectPrefabs = EntangledObjectPrefabs;
			foreach (GameObject prefab in entangledObjectPrefabs)
			{
				if (!base.gameObject.name.StartsWith(prefab.name))
				{
					PropHealthState[] array = source.Where((PropHealthState hs) => hs.gameObject.name == prefab.name).ToArray();
					for (int num2 = 0; num2 < array.Length; num2++)
					{
						array[num2].UpdateState(currentHealth, maxHealth, isActorDead);
					}
				}
			}
		}
		bool flag = _prevHealth == int.MinValue;
		if (!flag && _prevHealth > currentHealth && !string.IsNullOrEmpty(HurtAudio))
		{
			AudioController.Play(HurtAudio, base.gameObject.transform, null, attachToParent: false);
		}
		_prevHealth = currentHealth;
		_prevIsDead = isActorDead;
		float num3 = (float)currentHealth / (float)maxHealth;
		GameObject gameObject = base.gameObject;
		if (_statesGO != null && _statesGO.Length > 1)
		{
			int num4 = _statesGO.Length - 1;
			int num5 = num4 - (int)(0.99f * (float)_statesGO.Length * num3);
			if (maxHealth == 1)
			{
				num5 = ((currentHealth == 0) ? num4 : 0);
			}
			else if (LastStateIsDeath && currentHealth > 0 && num4 > 0 && num5 == num4)
			{
				num5 = num4 - 1;
			}
			for (int num6 = 0; num6 < _statesGO.Length; num6++)
			{
				_statesGO[num6].SetActive(num6 == num5);
			}
			gameObject = _statesGO[num5];
			if (flag)
			{
				Animator[] componentsInChildren = _statesGO[num5].GetComponentsInChildren<Animator>();
				for (int num = 0; num < componentsInChildren.Length; num++)
				{
					componentsInChildren[num].Update(10f);
				}
				PlayableDirector[] componentsInChildren2 = _statesGO[num5].GetComponentsInChildren<PlayableDirector>();
				foreach (PlayableDirector obj in componentsInChildren2)
				{
					obj.time = obj.duration;
				}
			}
		}
		if (!flag)
		{
			if (TransitionVFX.Length != 0)
			{
				int num7 = TransitionVFX.Length - 1;
				int num8 = num7 - (int)(0f + ((float)TransitionVFX.Length + 0.99f) * num3);
				if (LastStateIsDeath && currentHealth > 0 && num8 == num7)
				{
					num8 = num7 - 1;
				}
				if (_prevVFXIndex != num8 && num8 > -1)
				{
					TransitionVFX[num8].SetActive(value: false);
					TransitionVFX[num8].SetActive(value: true);
				}
				_prevVFXIndex = num8;
			}
			if (AudioEvents.Length != 0)
			{
				int num9 = AudioEvents.Length - 1;
				int num10 = num9 - (int)(0f + ((float)AudioEvents.Length + 0.99f) * num3);
				if (LastStateIsDeath && currentHealth > 0 && num10 == num9)
				{
					num10 = num9 - 1;
				}
				if (_prevAudioIndex != num10 && num10 > -1)
				{
					AudioController.Play(AudioEvents[num10], base.gameObject.transform, null, attachToParent: false);
				}
				_prevAudioIndex = num10;
			}
		}
		if (UseDeathDissolve)
		{
			if (flag && currentHealth <= 0)
			{
				gameObject.SetActive(value: false);
			}
			else if (isActorDead && currentHealth <= 0)
			{
				if (gameObject.activeInHierarchy)
				{
					StartCoroutine(DissolveCrt(gameObject, flag));
				}
			}
			else if (currentHealth > 0)
			{
				gameObject.SetActive(value: true);
				List<Material> list = new List<Material>();
				Renderer[] componentsInChildren3 = gameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren3)
				{
					if (renderer is ParticleSystemRenderer)
					{
						continue;
					}
					Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
					foreach (Material material in materialClonesSlow)
					{
						if (material.HasProperty(DissolvePropertyName))
						{
							list.Add(material);
						}
					}
				}
				foreach (Material item in list)
				{
					item.SetFloat(DissolvePropertyName, 0f);
				}
			}
		}
		SetMaterialProperties(gameObject, 1f - num3);
	}

	private IEnumerator DissolveCrt(GameObject root, bool forceFinalResult)
	{
		isDeathDisolved = true;
		yield return new WaitForSeconds(1f);
		List<Material> materials = new List<Material>();
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is ParticleSystemRenderer)
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (material.HasProperty(DissolvePropertyName))
				{
					materials.Add(material);
				}
			}
		}
		float timer = DissolvePeriod;
		while (timer > 0f)
		{
			timer -= Timekeeper.instance.m_GlobalClock.deltaTime;
			if (forceFinalResult)
			{
				timer = 0f;
			}
			float value = 1f - timer / DissolvePeriod;
			foreach (Material item in materials)
			{
				item.SetFloat(DissolvePropertyName, value);
			}
			yield return 0;
		}
		root.SetActive(value: false);
	}

	public void ResetDeathDisolve()
	{
		if (!isDeathDisolved)
		{
			return;
		}
		StopAllCoroutines();
		ResetDeathDisolve(gameObject);
		if (_statesGO != null)
		{
			GameObject[] statesGO = _statesGO;
			foreach (GameObject root in statesGO)
			{
				ResetDeathDisolve(root);
			}
		}
		isDeathDisolved = false;
	}

	private void ResetDeathDisolve(GameObject root)
	{
		List<Material> list = new List<Material>();
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is ParticleSystemRenderer)
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (material.HasProperty(DissolvePropertyName))
				{
					list.Add(material);
				}
			}
		}
		float value = 0f;
		foreach (Material item in list)
		{
			item.SetFloat(DissolvePropertyName, value);
		}
		root.SetActive(value: true);
	}

	public void ResetPrevHealth()
	{
		_prevHealth = int.MinValue;
		_prevIsDead = false;
	}

	private void SetMaterialProperties(GameObject root, float factor)
	{
		if (materialCache == null || materialCache.materialsCopies == null)
		{
			InitMaterialCache();
			if (materialCache == null || materialCache.materialsCopies == null)
			{
				return;
			}
		}
		foreach (Material value in materialCache.materialsCopies.Values)
		{
			if (!(value == null) && value.HasProperty(SplatMaxPropertyName))
			{
				float num = value.GetFloat(SplatMaxPropertyName);
				value.SetFloat(SplatCountPropertyName, num * factor * 1.6f);
			}
		}
	}
}

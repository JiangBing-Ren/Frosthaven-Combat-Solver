using System;
using System.Collections.Generic;
using SETUtil.Materials;
using UnityEngine;

namespace FH.ObjectHealth;

public class ObjectHealth_Mesh : MonoBehaviour
{
	private const float MAX_HP = 100f;

	private static int _splatMaxPropertyName = Shader.PropertyToID("_SplatMax");

	private static int _splatCountPropertyName = Shader.PropertyToID("_SplatCount");

	[Header("Damage Mesh")]
	[SerializeField]
	private List<MeshRenderer> _meshes = new List<MeshRenderer>();

	[SerializeField]
	private List<GameObject> _lights = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _addons = new List<GameObject>();

	[SerializeField]
	[Range(0f, 100f)]
	private float _objectHealthPreview = 100f;

	[NonSerialized]
	private float _objectHealth = 100f;

	private void Awake()
	{
		SetHealth(100f);
	}

	private void Update()
	{
		if (_objectHealthPreview != _objectHealth)
		{
			SetHealth(_objectHealthPreview);
		}
	}

	public void SetHealth(float objectHealth)
	{
		_objectHealth = objectHealth;
		int num = _meshes.Count - 1;
		if (num == 0)
		{
			return;
		}
		float num2 = objectHealth / 100f;
		for (int i = 0; i < _meshes.Count; i++)
		{
			int num3 = i;
			bool flag = Mathf.Approximately(objectHealth, 0f);
			MeshRenderer meshRenderer = _meshes[i];
			if (!(meshRenderer == null))
			{
				Material[] materialClonesSlow = meshRenderer.GetMaterialClonesSlow();
				for (int j = 0; j < materialClonesSlow.Length; j++)
				{
					SetMaterialProperties(materialClonesSlow[j], objectHealth);
				}
				if (num3 == 0)
				{
					meshRenderer.enabled = flag;
					continue;
				}
				if (flag)
				{
					meshRenderer.enabled = false;
					continue;
				}
				bool flag2 = num2 > (float)(num3 - 1) / (float)num;
				bool flag3 = num2 <= (float)num3 / (float)num;
				meshRenderer.enabled = flag2 & flag3;
			}
		}
		for (int k = 0; k < _lights.Count; k++)
		{
			int num4 = k;
			Mathf.Approximately(objectHealth, 0f);
			GameObject gameObject = _lights[k];
			if (!(gameObject == null))
			{
				bool flag4 = num2 > (float)(num4 - 1) / (float)num;
				bool flag5 = num2 <= (float)num4 / (float)num;
				gameObject.SetActive(flag4 & flag5);
			}
		}
		num = _addons.Count - 1;
		for (int l = 0; l < _addons.Count; l++)
		{
			int num5 = l;
			Mathf.Approximately(objectHealth, 0f);
			GameObject gameObject2 = _addons[l];
			if (!(gameObject2 == null))
			{
				_ = (float)(num5 - 1) / (float)num;
				bool active = num2 <= (float)num5 / (float)num;
				gameObject2.SetActive(active);
			}
		}
	}

	private static void SetMaterialProperties(Material material, float objectHealth)
	{
		if ((bool)material && material.HasProperty(_splatMaxPropertyName))
		{
			float num = material.GetFloat(_splatMaxPropertyName);
			float a = Mathf.Lerp(num + 1f, 0f, objectHealth / 100f);
			a = Mathf.Min(a, num);
			material.SetFloat(_splatCountPropertyName, a);
		}
	}
}

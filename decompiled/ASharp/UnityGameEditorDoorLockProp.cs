using System.Collections.Generic;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

public class UnityGameEditorDoorLockProp : MonoBehaviour
{
	private UnityGameEditorObject m_ParentDoor;

	private void Start()
	{
		m_ParentDoor = GetComponentInParent<UnityGameEditorObject>();
		if (m_ParentDoor != null && m_ParentDoor.PropObject != null && m_ParentDoor.PropObject is CObjectDoor { Activated: not false })
		{
			gameObject.SetActive(value: false);
		}
	}

	public void Reactivate()
	{
		int nameID = Shader.PropertyToID("_DeathDissolve");
		DeathDissolve[] componentsInChildren = gameObject.GetComponentsInChildren<DeathDissolve>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].StopAllCoroutines();
		}
		List<Material> list = new List<Material>();
		Renderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren2)
		{
			if (renderer is ParticleSystemRenderer)
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material material in materialClonesSlow)
			{
				if (material.HasProperty(nameID))
				{
					list.Add(material);
					renderer.enabled = true;
				}
			}
		}
		float value = 0f;
		foreach (Material item in list)
		{
			item.SetFloat(nameID, value);
		}
		gameObject.SetActive(value: true);
	}
}

using UnityEngine;

public class ParticleSystem_OnEnable_Default : EffectOnEnable
{
	private void OnEnable()
	{
		if (GetComponentInParent<ObjectPool>() == null)
		{
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
		}
	}
}

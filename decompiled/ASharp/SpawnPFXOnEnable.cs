using System;
using System.Collections;
using Chronos;
using UnityEngine;

public class SpawnPFXOnEnable : MonoBehaviour
{
	[Serializable]
	private class Effect
	{
		public float Delay;

		public GameObject PFX;
	}

	[SerializeField]
	private Effect[] m_Effects;

	private Transform m_Transform;

	private void OnEnable()
	{
		m_Transform = transform;
		Effect[] effects = m_Effects;
		foreach (Effect effect in effects)
		{
			StartCoroutine(Spawn(effect));
		}
	}

	private IEnumerator Spawn(Effect effect)
	{
		yield return Timekeeper.instance.WaitForSeconds(effect.Delay);
		GameObject gameObject = ObjectPool.Spawn(effect.PFX, m_Transform);
		ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), effect.PFX);
	}
}

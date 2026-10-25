using System.Collections;
using Chronos;
using UnityEngine;

namespace WorldspaceUI;

public abstract class IEffectBlink : MonoBehaviour
{
	[SerializeField]
	private float m_BlinkInterval = 0.5f;

	protected virtual void OnEnable()
	{
		StartCoroutine(Blink());
	}

	protected virtual void OnDisable()
	{
		StopAllCoroutines();
	}

	public IEnumerator Blink()
	{
		while (true)
		{
			yield return Timekeeper.instance.WaitForSeconds(m_BlinkInterval);
			ProcessBlink();
		}
	}

	protected abstract void ProcessBlink();
}

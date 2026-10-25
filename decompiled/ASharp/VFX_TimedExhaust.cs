using System.Collections;
using UnityEngine;

public class VFX_TimedExhaust : MonoBehaviour
{
	public float psWaittime = 2f;

	public ParticleSystem[] psExhaustables;

	private bool routineCheck;

	private void Start()
	{
	}

	private void OnEnable()
	{
		ParticleSystem[] array = psExhaustables;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = true;
		}
		Log.Info("startRoutine");
		StartCoroutine(waiter());
	}

	private IEnumerator waiter()
	{
		yield return new WaitForSeconds(psWaittime);
		Log.Info("stopEmission");
		ParticleSystem[] array = psExhaustables;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = false;
		}
	}
}

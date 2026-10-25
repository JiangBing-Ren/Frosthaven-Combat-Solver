using System.Collections;
using ClockStone;
using UnityEngine;

public class StaticEnvSFX : MonoBehaviour
{
	[AudioEventName]
	public string m_AudioEvent;

	private AudioObject _audio;

	public void PlayAudio()
	{
		StartCoroutine(AudioCoroutine());
	}

	public void StopAudio()
	{
		if (_audio != null)
		{
			_audio.Stop(0.3f);
		}
	}

	private IEnumerator AudioCoroutine()
	{
		if (string.IsNullOrEmpty(m_AudioEvent))
		{
			yield break;
		}
		if (!Choreographer.s_Choreographer.LoadingComplete)
		{
			yield return new WaitUntil(() => Choreographer.s_Choreographer.LoadingComplete);
		}
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			yield return new WaitUntil(() => TransitionManager.s_Instance.TransitionDone);
		}
		yield return new WaitForSeconds(Random.Range(0f, 0.5f));
		_audio = AudioController.Play(m_AudioEvent, gameObject.transform, gameObject);
	}
}

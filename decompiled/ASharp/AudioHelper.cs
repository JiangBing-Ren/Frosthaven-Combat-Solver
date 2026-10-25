using UnityEngine;
using UnityEngine.Audio;

public class AudioHelper : MonoBehaviour
{
	public AudioMixerSnapshot Standard;

	public AudioMixerSnapshot Silent;

	public void FadeOut()
	{
		Silent.TransitionTo(0.5f);
	}

	public void FadeIn()
	{
		Standard.TransitionTo(0.5f);
	}
}

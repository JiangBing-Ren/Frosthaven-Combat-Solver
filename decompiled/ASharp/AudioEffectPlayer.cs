using ClockStone;
using UnityEngine;

public class AudioEffectPlayer : MonoBehaviour
{
	public void PlayEffect(string audioID)
	{
		if ((bool)SingletonMonoBehaviour<AudioController>.DoesInstanceExist())
		{
			AudioController.Play(audioID);
		}
	}
}

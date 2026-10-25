using ClockStone;
using UnityEngine;

public class AudioFocusController : MonoBehaviour
{
	private void OnApplicationFocus(bool focus)
	{
		if (!(SaveData.Instance == null) && SaveData.Instance.Global != null && SaveData.Instance.Global.MuteAudioInBackground && SingletonMonoBehaviour<AudioController>.Instance != null)
		{
			SingletonMonoBehaviour<AudioController>.Instance.soundMuted = !focus;
			SingletonMonoBehaviour<AudioController>.Instance.musicEnabled = focus;
			SingletonMonoBehaviour<AudioController>.Instance.ambienceSoundEnabled = focus;
		}
	}
}

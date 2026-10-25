using System;
using UnityEngine;

public class SFXBucket : MonoBehaviour
{
	[Serializable]
	public class AudioBucket
	{
		public CharacterManager.SurfaceTypes TargetSurface;

		[AudioEventName]
		public string AudioEvent;
	}

	public AudioBucket m_SFXBucket;

	public static void PlayClip(SFXBucket bucket, Transform parent)
	{
		if (!string.IsNullOrEmpty(bucket.m_SFXBucket.AudioEvent))
		{
			AudioController.Play(bucket.m_SFXBucket.AudioEvent, parent, null, attachToParent: false);
		}
	}
}

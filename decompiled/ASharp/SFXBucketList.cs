using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SFXBucketList : MonoBehaviour
{
	public List<GameObject> m_AudioBuckets;

	public static void PlayClipFromBucketList(CharacterManager.SurfaceTypes surface, SFXBucketList bucketList, Transform parent)
	{
		SFXBucket sFXBucket = bucketList.m_AudioBuckets.Select((GameObject x) => x.GetComponent<SFXBucket>()).ToList().SingleOrDefault((SFXBucket x) => x.m_SFXBucket.TargetSurface == surface);
		if (sFXBucket == null)
		{
			Log.AudioError("The SFX Bucket List " + bucketList.gameObject.name + " does not have an audio bucket definition for surface " + surface.ToString() + ". Unable to play audio clip.");
		}
		else
		{
			SFXBucket.PlayClip(sFXBucket, parent);
		}
	}
}

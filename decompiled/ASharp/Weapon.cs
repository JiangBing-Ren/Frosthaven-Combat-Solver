using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public enum WeaponTypes
	{
		Melee,
		Projectile,
		Cosmetic
	}

	public WeaponTypes WeaponType;

	public Collider weaponCollider;

	public GameObject OnHitAudioBucketList;

	public GameObject MeleeHitEffect;

	private void Awake()
	{
		weaponCollider = GetComponentInChildren<Collider>();
	}

	public void PlayHitFX(CharacterManager.SurfaceTypes surface)
	{
		SFXBucket sFXBucket = OnHitAudioBucketList.GetComponent<SFXBucketList>().m_AudioBuckets.Select((GameObject x) => x.GetComponent<SFXBucket>()).ToList().SingleOrDefault((SFXBucket x) => x.m_SFXBucket.TargetSurface == surface);
		if (sFXBucket == null)
		{
			Log.Error("The weapon " + gameObject.name + " does not have an audio bucket definition for surface " + surface.ToString() + ". Unable to play audio clip.");
		}
		else
		{
			SFXBucket.PlayClip(sFXBucket, gameObject.transform);
		}
	}

	public void PlayFromBucket(GameObject bucketGO)
	{
		SFXBucket component = bucketGO.GetComponent<SFXBucket>();
		if (component == null)
		{
			Log.Error("Could not find SFXBucket Component on GameObject " + bucketGO.name);
		}
		else
		{
			SFXBucket.PlayClip(component, gameObject.transform);
		}
	}
}

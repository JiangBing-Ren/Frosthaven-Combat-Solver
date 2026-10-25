using System;
using System.Collections;
using System.Collections.Generic;
using Ara;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class FireSpawnedProjectile : MonoBehaviour
{
	[Serializable]
	public class ExternalSettings
	{
		public GameObject Projectile;

		public string ProjectileAttachPoint;

		public float ProjectileStartTime;

		public float ProjectileSpeed;

		[AudioEventName]
		public string AbilityAudioEvent;

		[AudioEventName]
		public string AbilityHitAudioEvent;

		public GameObject ProjectileHitEffect;

		public bool HitEffectZeroLocalRotation;

		public bool ProgressChoreographerOnHit;

		public List<VFXShared.SpawnedEffect> SpawnedProjectileEffects;

		public bool orientOnFire;

		public bool clearPointDataOnFire = true;

		public bool fadeProjectileStartOnImpact;
	}

	public class SpawnedProjectileHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public SpawnedProjectileHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	public class SpawnedProjectileFireEventArgs : EventArgs
	{
		public Transform Target;

		public SpawnedProjectileFireEventArgs(Transform t)
		{
			Target = t;
		}
	}

	public GameObject Projectile;

	public string ProjectileAttachPoint;

	public float ProjectileStartTime;

	public float ProjectileSpeed;

	[AudioEventName]
	public string AbilityAudioEvent;

	[AudioEventName]
	public string AbilityHitAudioEvent;

	public GameObject ProjectileHitEffect;

	public bool HitEffectZeroLocalRotation;

	public bool ProgressChoreographerOnHit;

	public List<VFXShared.SpawnedEffect> SpawnedProjectileEffects;

	public bool orientOnFire;

	public bool clearPointDataOnFire = true;

	public bool fadeProjectileStartOnImpact;

	public static event EventHandler<SpawnedProjectileHitEventArgs> SpawnedProjectileHit;

	public static event EventHandler<SpawnedProjectileFireEventArgs> SpawnedProjectileFire;

	public void LoadSettings(ExternalSettings settings)
	{
		Projectile = settings.Projectile;
		ProjectileAttachPoint = settings.ProjectileAttachPoint;
		ProjectileStartTime = settings.ProjectileStartTime;
		ProjectileSpeed = settings.ProjectileSpeed;
		AbilityAudioEvent = settings.AbilityAudioEvent;
		AbilityHitAudioEvent = settings.AbilityHitAudioEvent;
		ProjectileHitEffect = settings.ProjectileHitEffect;
		HitEffectZeroLocalRotation = settings.HitEffectZeroLocalRotation;
		ProgressChoreographerOnHit = settings.ProgressChoreographerOnHit;
		ProgressChoreographerOnHit = settings.ProgressChoreographerOnHit;
		SpawnedProjectileEffects = settings.SpawnedProjectileEffects;
		orientOnFire = settings.orientOnFire;
		fadeProjectileStartOnImpact = settings.fadeProjectileStartOnImpact;
		if (SpawnedProjectileEffects == null)
		{
			return;
		}
		foreach (VFXShared.SpawnedEffect spawnedProjectileEffect in SpawnedProjectileEffects)
		{
			spawnedProjectileEffect.OriginalStartTime = spawnedProjectileEffect.StartTime;
		}
	}

	protected virtual void OnSpawnedProjectileHit(SpawnedProjectileHitEventArgs e)
	{
		if (SpawnedProjectileHit != null)
		{
			SpawnedProjectileHit(this, e);
		}
	}

	protected virtual void OnSpawnedProjectileFire(SpawnedProjectileFireEventArgs e)
	{
		if (SpawnedProjectileFire != null)
		{
			SpawnedProjectileFire(this, e);
		}
	}

	private void Awake()
	{
		if (SpawnedProjectileEffects == null)
		{
			return;
		}
		foreach (VFXShared.SpawnedEffect spawnedProjectileEffect in SpawnedProjectileEffects)
		{
			spawnedProjectileEffect.OriginalStartTime = spawnedProjectileEffect.StartTime;
		}
	}

	public void Fire(GameObject from, GameObject to, GameObject caster = null, Action callBack = null)
	{
		CoroutineHelper.RunCoroutine(EnterCoroutine(from, to, caster, callBack));
	}

	private IEnumerator EnterCoroutine(GameObject fromGO, GameObject toGO, GameObject caster = null, Action callBack = null)
	{
		Quaternion quaternion = default;
		if (orientOnFire)
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(toGO);
			quaternion = Quaternion.LookRotation(((characterManager == null) ? toGO.transform : characterManager.TargetBoneInstance.transform).position - fromGO.transform.position, Vector3.up);
		}
		VFXShared.PlaySpawnedEffectsFromTimeline(SpawnedProjectileEffects, fromGO, quaternion);
		if (!string.IsNullOrEmpty(AbilityAudioEvent))
		{
			AudioController.Play(AbilityAudioEvent, fromGO.transform);
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(fromGO);
		CActor cActor = ((actorBehaviour != null) ? actorBehaviour.Actor : null);
		if (cActor == null || cActor.SpawnProjectilesOnRangedAttack)
		{
			GameObject gameObject = fromGO.FindInChildren(ProjectileAttachPoint);
			if (gameObject == null)
			{
				CharacterManager characterManager2 = CharacterManager.GetCharacterManager(fromGO);
				gameObject = ((characterManager2 == null) ? fromGO : characterManager2.TargetBoneInstance);
			}
			yield return FireCoroutine(caster, base.gameObject, ProjectileStartTime, Projectile, gameObject.transform, toGO, orientOnFire ? quaternion : Quaternion.LookRotation(transform.forward, transform.up), ProjectileSpeed, ProjectileHitEffect, callBack);
		}
	}

	private IEnumerator FireCoroutine(GameObject caster, GameObject fromObject, float startTime, GameObject projectile, Transform spawnPoint, GameObject targetGO, Quaternion rotation, float projectileSpeed, GameObject hitEffect, Action callBack)
	{
		float t = 0f;
		if (startTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(startTime);
			t = startTime;
		}
		CharacterManager cmTarget = CharacterManager.GetCharacterManager(targetGO);
		Transform target = ((cmTarget == null) ? targetGO.transform : cmTarget.TargetBoneInstance.transform);
		OnSpawnedProjectileFire(new SpawnedProjectileFireEventArgs(target));
		GameObject projectileInstance = ObjectPool.Spawn(projectile, null, spawnPoint.position, rotation);
		if (orientOnFire)
		{
			Quaternion rotation2 = Quaternion.LookRotation(target.position - projectileInstance.transform.position, Vector3.up);
			projectileInstance.transform.rotation = rotation2;
		}
		if (clearPointDataOnFire)
		{
			AraTrail[] componentsInChildren = projectileInstance.GetComponentsInChildren<AraTrail>();
			if (componentsInChildren.Length != 0)
			{
				AraTrail[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Clear();
				}
			}
		}
		while (projectileInstance.transform.position != target.position)
		{
			yield return new WaitForEndOfFrame();
			projectileInstance.transform.position = Vector3.MoveTowards(projectileInstance.transform.position, target.position, projectileSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(targetGO);
		OnSpawnedProjectileHit(new SpawnedProjectileHitEventArgs(actorBehaviour));
		if (string.IsNullOrEmpty(AbilityHitAudioEvent))
		{
			AudioController.Play(AbilityHitAudioEvent, targetGO.transform);
		}
		if (fadeProjectileStartOnImpact)
		{
			ParticlesFadeScript component = projectileInstance.GetComponent<ParticlesFadeScript>();
			if (component != null)
			{
				component.fadeOnImpactMult = 0f;
			}
		}
		if ((bool)actorBehaviour)
		{
			WeaponCollision.SpawnActorHitVFX(cmTarget, actorBehaviour, cmTarget.GetImpactPointPos(), projectileInstance.transform.rotation);
		}
		float num = VFXShared.GetEffectLifetime(projectileInstance) - t;
		if (num > 0f)
		{
			ObjectPool.Recycle(projectileInstance, num, projectile);
		}
		else
		{
			ObjectPool.Recycle(projectileInstance, projectile);
		}
		if (ProgressChoreographerOnHit && caster != null)
		{
			ActorEvents actorEvents = caster.GetComponent<ActorEvents>();
			if (actorEvents == null)
			{
				actorEvents = caster.GetComponentInChildren<ActorEvents>();
			}
			actorEvents.ProgressChoreographer();
		}
		callBack?.Invoke();
		if (hitEffect != null)
		{
			GameObject gameObject = ObjectPool.Spawn(hitEffect, null, target.position, HitEffectZeroLocalRotation ? default(Quaternion) : rotation);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), hitEffect);
		}
	}
}

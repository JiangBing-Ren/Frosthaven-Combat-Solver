using System;
using System.Collections;
using System.Linq;
using Chronos;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using UnityEngine;

public class FireEquippedProjectileSMB : StateMachineBehaviour
{
	public class EquippedProjectileHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public EquippedProjectileHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	public float FireTime;

	public float RespawnTime;

	public GameObject Projectile;

	public float ProjectileSpeed;

	public bool FadeProjectile = true;

	public bool projectileHasPFX;

	public float FadeTime = 3f;

	public float DelayBeforeFade = 2.5f;

	[AudioEventName]
	public string FireSFXEvent;

	[AudioEventName]
	public string ImpactSFXEvent;

	public GameObject ProjectileHitEffect;

	public bool hiddenOnFire;

	public bool orientOnFire;

	public bool orientOnFireYAxis;

	public bool useStaticProjectile;

	private SkinnedMeshRenderer ProjectileMesh;

	private Material[] projectileMats;

	private Renderer projectileRend;

	private bool fadeInProgress;

	private float fadingTime;

	private float fadeStartTime;

	public static event EventHandler<EquippedProjectileHitEventArgs> EquippedProjectileHit;

	protected virtual void OnEquippedProjectileHit(EquippedProjectileHitEventArgs e)
	{
		if (EquippedProjectileHit != null)
		{
			EquippedProjectileHit(this, e);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		VFXShared.ControlChoreographer(control: true, this);
		GameObject gameObject = animator.gameObject.FindInChildren(Projectile.name);
		if (gameObject == null)
		{
			Log.Error("Error: Could not find projectile " + Projectile.name + " as a child of " + animator.gameObject.name);
			return;
		}
		if (gameObject != null)
		{
			fadeInProgress = true;
			fadingTime = 0f;
			projectileMats = new Material[0];
			projectileRend = gameObject.GetComponentInChildren<Renderer>();
			if (projectileRend != null)
			{
				projectileMats = gameObject.GetComponentInChildren<Renderer>().GetMaterialClonesSlow();
			}
		}
		Transform parent = gameObject.transform.parent;
		CoroutineHelper.RunCoroutine(FireCoroutine(animator, gameObject, ProjectileSpeed, FireTime, animator.transform.parent.gameObject, FadeProjectile, FadeTime, DelayBeforeFade, FireSFXEvent, ProjectileHitEffect));
		CoroutineHelper.RunCoroutine(SpawnNewProjectileCoroutine(Projectile, parent, RespawnTime, animator.transform.parent.gameObject));
		if (hiddenOnFire)
		{
			ProjectileMesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: false, this);
	}

	private IEnumerator FireCoroutine(Animator animator, GameObject projectileInstance, float projectileSpeed, float fireTime, GameObject firingCharacter, bool fadeProjectile, float fadeTime, float delayBeforeFade, string fireSFXEvent, GameObject hitEffect)
	{
		float t = 0f;
		while (t < fireTime)
		{
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
			yield return 0;
		}
		if (projectileInstance == null)
		{
			yield break;
		}
		Transform initialParent = projectileInstance.transform.parent;
		projectileInstance.transform.SetParent(null);
		projectileInstance.transform.localScale = Vector3.one;
		Weapon[] otherWeapons = new Weapon[0];
		if (useStaticProjectile)
		{
			otherWeapons = (from w in initialParent.GetComponentsInChildren<Weapon>()
				where w.gameObject.activeSelf && w.gameObject != projectileInstance
				select w).ToArray();
			Weapon[] array = otherWeapons;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].gameObject.SetActive(value: false);
			}
		}
		if (hiddenOnFire)
		{
			ProjectileMesh.enabled = false;
		}
		CharacterManager cmSource = CharacterManager.GetCharacterManager(firingCharacter);
		if (!useStaticProjectile)
		{
			cmSource.EquippedWeapons.Remove(projectileInstance);
		}
		CharacterManager cmTarget = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		Vector3 target = cmTarget.GetTargetZone();
		_ = target;
		if (orientOnFire)
		{
			Vector3 vector = target - projectileInstance.transform.position;
			if (orientOnFireYAxis)
			{
				projectileInstance.transform.up = -vector.normalized;
			}
			else
			{
				projectileInstance.transform.forward = vector.normalized;
			}
		}
		if (!string.IsNullOrEmpty(fireSFXEvent))
		{
			AudioController.Play(fireSFXEvent, projectileInstance.transform);
		}
		Transform transform = projectileInstance.transform.Find("HitPivot");
		Vector3 vector2 = ((transform != null) ? (projectileInstance.transform.position - transform.position) : Vector3.zero);
		target += vector2;
		GameObject shootVFX = projectileInstance.FindInChildren("ShootVFX", includeInactive: true);
		if ((bool)shootVFX)
		{
			ParticleSystem[] componentsInChildren = shootVFX.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				ParticleSystem.MainModule main = componentsInChildren[num].main;
				main.loop = true;
			}
			shootVFX.SetActive(value: false);
			shootVFX.SetActive(value: true);
		}
		AnimatedProjectile codeAnim = projectileInstance.GetComponent<AnimatedProjectile>();
		if ((bool)codeAnim)
		{
			Vector3 normalized = (target - projectileInstance.transform.position).normalized;
			codeAnim.StartAnim(Vector3.Cross(normalized, Vector3.up));
		}
		while (projectileInstance.transform.position != target)
		{
			yield return new WaitForEndOfFrame();
			projectileInstance.transform.position = Vector3.MoveTowards(projectileInstance.transform.position, target, projectileSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		if ((bool)codeAnim)
		{
			codeAnim.StopAnim();
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		OnEquippedProjectileHit(new EquippedProjectileHitEventArgs(actorBehaviour));
		if (!string.IsNullOrEmpty(ImpactSFXEvent))
		{
			AudioController.Play(ImpactSFXEvent, projectileInstance.transform, null, attachToParent: false);
		}
		projectileInstance.transform.SetParent(cmTarget.TargetBoneInstance.transform, worldPositionStays: true);
		if (!useStaticProjectile)
		{
			cmSource.EquippedWeapons.Remove(projectileInstance);
		}
		firingCharacter.GetComponentInChildren<ActorEvents>().ProgressChoreographer(this);
		VFXShared.ControlChoreographer(control: false, this);
		Weapon component = projectileInstance.GetComponent<Weapon>();
		if (component != null)
		{
			component.PlayHitFX(cmTarget.m_SurfaceType);
		}
		if (hitEffect != null)
		{
			GameObject gameObject = ObjectPool.Spawn(hitEffect, null, projectileInstance.transform.position, projectileInstance.transform.rotation);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), hitEffect);
		}
		if ((bool)actorBehaviour)
		{
			if (Choreographer.s_Choreographer.ActorsBeingTargetedForVFX != null && Choreographer.s_Choreographer.ActorsBeingTargetedForVFX.Count() > 1)
			{
				foreach (CActor item in Choreographer.s_Choreographer.ActorsBeingTargetedForVFX)
				{
					CharacterManager characterManager = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.FindClientActorGameObject(item));
					WeaponCollision.SpawnActorHitVFX(characterManager, actorBehaviour, characterManager.GetImpactPointPos(), projectileInstance.transform.rotation);
				}
			}
			else
			{
				WeaponCollision.SpawnActorHitVFX(cmTarget, actorBehaviour, cmTarget.GetImpactPointPos(), projectileInstance.transform.rotation);
			}
		}
		if (projectileInstance == null)
		{
			yield break;
		}
		Material[] array2;
		if (fadeProjectile)
		{
			yield return Timekeeper.instance.WaitForSeconds(delayBeforeFade);
			if (projectileInstance == null)
			{
				yield break;
			}
			if (projectileHasPFX)
			{
				ParticleSystem[] componentsInChildren = projectileInstance.GetComponentsInChildren<ParticleSystem>();
				for (int num = 0; num < componentsInChildren.Length; num++)
				{
					componentsInChildren[num].Stop();
				}
			}
			if ((bool)shootVFX)
			{
				shootVFX.SetActive(value: false);
				ParticleSystem[] componentsInChildren = shootVFX.GetComponentsInChildren<ParticleSystem>();
				for (int num = 0; num < componentsInChildren.Length; num++)
				{
					ParticleSystem.MainModule main2 = componentsInChildren[num].main;
					main2.loop = false;
				}
			}
			fadeStartTime = Timekeeper.instance.m_GlobalClock.time;
			while (fadeInProgress)
			{
				fadingTime = (Timekeeper.instance.m_GlobalClock.time - fadeStartTime) / fadeTime;
				array2 = projectileMats;
				foreach (Material material in array2)
				{
					if (material.HasProperty("_Opacity"))
					{
						material.SetFloat("_Opacity", 1f - fadingTime);
					}
				}
				if (fadingTime >= fadeTime)
				{
					fadeInProgress = false;
				}
				yield return new WaitForEndOfFrame();
				if (projectileInstance == null)
				{
					yield break;
				}
			}
		}
		else if ((bool)shootVFX)
		{
			shootVFX.SetActive(value: false);
			ParticleSystem[] componentsInChildren = shootVFX.GetComponentsInChildren<ParticleSystem>();
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				ParticleSystem.MainModule main3 = componentsInChildren[num].main;
				main3.loop = false;
			}
		}
		array2 = projectileMats;
		foreach (Material material2 in array2)
		{
			if (material2.HasProperty("_Opacity"))
			{
				material2.SetFloat("_Opacity", 1f);
			}
		}
		if (useStaticProjectile)
		{
			projectileInstance.transform.SetParent(initialParent);
			projectileInstance.transform.localPosition = Vector3.zero;
			projectileInstance.transform.localRotation = Quaternion.identity;
			projectileInstance.transform.localScale = Vector3.one;
			Weapon[] array = otherWeapons;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].gameObject.SetActive(value: true);
			}
		}
		else
		{
			ObjectPool.Recycle(projectileInstance, Projectile);
		}
	}

	private IEnumerator SpawnNewProjectileCoroutine(GameObject projectile, Transform parent, float respawnTime, GameObject firingCharacter)
	{
		if (!useStaticProjectile)
		{
			yield return Timekeeper.instance.WaitForSeconds(respawnTime);
			CharacterManager characterManager = CharacterManager.GetCharacterManager(firingCharacter);
			if (characterManager != null)
			{
				characterManager.DoEquip(projectile, parent);
			}
		}
	}
}

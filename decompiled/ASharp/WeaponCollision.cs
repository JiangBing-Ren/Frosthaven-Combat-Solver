using System;
using ScenarioRuleLibrary;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
	public class WeaponCollisionHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public WeaponCollisionHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	private ActorBehaviour actorBehavior;

	public static event EventHandler<WeaponCollisionHitEventArgs> WeaponCollisionHit;

	protected virtual void OnWeaponCollisionHit(WeaponCollisionHitEventArgs e)
	{
		if (WeaponCollisionHit != null)
		{
			WeaponCollisionHit(this, e);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		enabled = false;
	}

	public static void SpawnActorHitVFX(CharacterManager charManagerTarget, ActorBehaviour actorBehaviour, Vector3 pos, Quaternion rot)
	{
		if (!(actorBehaviour.Actor is CObjectActor))
		{
			int num = 0;
			num = ((DebugMenu.Instance.AttackValueOverride == int.MaxValue) ? charManagerTarget.CharacterActor.IncomingAttackDamage : DebugMenu.Instance.AttackValueOverride);
			GameObject prefab;
			if (num != 0)
			{
				if ((float)num >= 0.01f * (float)(charManagerTarget.CriticalHitTriggerPercent * actorBehaviour.Actor.MaxHealth))
				{
					prefab = ((charManagerTarget.CriticalHitEffect != null) ? charManagerTarget.CriticalHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultCriticalHitEffect);
				}
				else
				{
					prefab = ((charManagerTarget.StandardHitEffect != null) ? charManagerTarget.StandardHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultStandardHitEffect);
				}
			}
			else
			{
				prefab = ((charManagerTarget.ShieldHitEffect != null) ? charManagerTarget.ShieldHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultShieldHitEffect);
			}
			if (charManagerTarget.HitEffectOnRoot)
			{
				pos = actorBehaviour.gameObject.transform.position;
				rot = Quaternion.identity;
			}
			GameObject gameObject = ObjectPool.Spawn(prefab, null, pos, rot);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), prefab, "@ VFX Pool");
		}
	}

	public static void SpawnActorHitVFX(CharacterManager charManagerTarget, ActorBehaviour actorBehaviour, Quaternion rot)
	{
		SpawnActorHitVFX(charManagerTarget, actorBehaviour, charManagerTarget.GetImpactPointPos(), rot);
	}

	private ActorBehaviour GetActorBehavior(Transform transform)
	{
		if (transform == null)
		{
			return null;
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(transform.gameObject);
		if (actorBehaviour != null)
		{
			return actorBehaviour;
		}
		return GetActorBehavior(transform.parent);
	}
}

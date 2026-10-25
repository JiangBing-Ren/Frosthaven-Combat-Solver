using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using UnityEngine;

public class ShowWeaponTrail : StateMachineBehaviour
{
	[Serializable]
	public class WeaponState
	{
		public float StartTime;

		public GameObject TrailEffect;

		public string WeaponListName;

		[NonSerialized]
		public GameObject Weapon;

		[NonSerialized]
		public float OriginalStartTime;
	}

	public List<WeaponState> WeaponStates;

	private void Awake()
	{
		foreach (WeaponState weaponState in WeaponStates)
		{
			weaponState.OriginalStartTime = weaponState.StartTime;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		foreach (WeaponState ws in WeaponStates)
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(animator.transform.parent.gameObject);
			CharacterManager.CharacterWeaponList characterWeaponList = characterManager.WeaponLists.SingleOrDefault((CharacterManager.CharacterWeaponList x) => x.name == ws.WeaponListName);
			if (characterWeaponList == null)
			{
				break;
			}
			string weaponName = characterManager.EquippedWeapons.Select((GameObject y) => y.name).ToList().Intersect(characterWeaponList.Weapons.Select((GameObject z) => z.name).ToList())
				.Single();
			ws.Weapon = characterManager.EquippedWeapons.FirstOrDefault((GameObject x) => x.name == weaponName);
			CoroutineHelper.RunCoroutine(SpawnTrail(ws));
		}
	}

	private IEnumerator SpawnTrail(WeaponState ws)
	{
		yield return Timekeeper.instance.WaitForSeconds(ws.StartTime);
		try
		{
			ObjectPool.Recycle(ObjectPool.Spawn(ws.TrailEffect, ws.Weapon.transform, ws.TrailEffect.transform.position, Quaternion.LookRotation(ws.Weapon.transform.forward, ws.Weapon.transform.up)), VFXShared.GetEffectLifetime(ws.TrailEffect), ws.TrailEffect);
		}
		catch
		{
		}
	}
}

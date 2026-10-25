using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FFSNet;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FHRL;
using GLOOM;
using Libs.Utils.Profiler;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using SETUtil.Materials;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary;
using SharedLibrary.Client;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IControllable
{
	public enum SurfaceTypes
	{
		None,
		Flesh,
		Stone,
		Bone
	}

	[Serializable]
	public class CharacterWeaponList
	{
		[Tooltip("This should be set and should be unique.  It can be referenced by other scripts (such as the Weapon Trails) to find a specific Weapon List")]
		public string name;

		[Tooltip("The name of the bone that this weapon will be attached to")]
		public string AttachToBone;

		[Tooltip("A list of potential weapons to equip to the specified bone.  One will be randomly selected at runtime.")]
		public List<GameObject> Weapons;
	}

	[Tooltip("The base prefab for this character in the content folder")]
	public GameObject CharacterChildPrefab;

	[Tooltip("The preview model for the outpost menus for this character")]
	public GameObject CharacterMenuModelPrefab;

	[Tooltip("The type of surface that this character has.  Used to determine what hit sound effect to play.")]
	public SurfaceTypes m_SurfaceType;

	[Tooltip("The Hit Effect to play when this character receives standard damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject StandardHitEffect;

	[Tooltip("The Hit Effect to play when this character receives zero damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject ShieldHitEffect;

	[Tooltip("The Hit Effect to play when this character receives critical damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject CriticalHitEffect;

	[Tooltip("The percentage of the actor's health needed to be taken in order to trigger the critical hit effect")]
	public int CriticalHitTriggerPercent = 36;

	[Tooltip("For some creatures we want the hit effect to be played from the root of the actor instead of the hit position.")]
	public bool HitEffectOnRoot;

	[Tooltip("The name of the bone that should be targeted by projectiles fired at this character")]
	public string TargetBone = "DEF-spine.003";

	[Tooltip("The radius of a circle around the target area.  The projectile will hit somewhere within this circle")]
	public float HitRadius = 0.1f;

	[Tooltip("The height of the character, used in overhead UI")]
	public float Height = 1.8f;

	[Tooltip("The list of weapons that are equipped by this character")]
	public List<CharacterWeaponList> WeaponLists = new List<CharacterWeaponList>();

	public bool NonMercenaryPrefab;

	[ConditionalField("NonMercenaryPrefab", null, true)]
	public Sprite CharacterCardBackgroundSprite;

	[Header("Preview")]
	public float glowMaterialPreview = 10f;

	public float oppacityMaterialPreview = 0.588f;

	[NonSerialized]
	public GameObject CharacterChildPrefabInstance;

	[NonSerialized]
	public CActor CharacterActor;

	[NonSerialized]
	public GameObject CharacterActorGO;

	[NonSerialized]
	public List<GameObject> EquippedWeapons = new List<GameObject>();

	[NonSerialized]
	public GameObject TargetBoneInstance;

	[NonSerialized]
	public GameObject ImpactPointInstance;

	[NonSerialized]
	public GameObject CharacterPrefab;

	[NonSerialized]
	public List<GameObject> LootTokensToCollect = new List<GameObject>();

	[NonSerialized]
	public List<GameObject> ChestsToCollect = new List<GameObject>();

	private static Dictionary<string, HashSet<int>> s_WeaponAssignment = new Dictionary<string, HashSet<int>>();

	private Collider[] m_Colliders = new Collider[0];

	private Renderer[] m_Renderers = new Renderer[0];

	private Tween m_FadeOutTween;

	private Tween m_FadeInTween;

	private float m_InvisibilityValue;

	private float m_GlowValue;

	private float m_OpacityValue;

	[NonSerialized]
	private (int X, int Y) lastLitCActorPosition;

	private static readonly Dictionary<GameObject, CharacterManager> ActiveCharacters = new Dictionary<GameObject, CharacterManager>();

	private List<ParticleSystem> m_particleSystemCache = new List<ParticleSystem>();

	public Collider[] Colliders => m_Colliders;

	public Renderer[] Renderers => m_Renderers;

	public GameObject LastSpawnedPrefab { get; private set; }

	public bool IsParticipant => true;

	public bool IsAlive => !CharacterActor.IsDead;

	private void OnEnable()
	{
		ActiveCharacters.Add(gameObject, this);
	}

	private void OnDisable()
	{
		ActiveCharacters.Remove(gameObject);
	}

	public static void GatherCharacterPrefabsForWarmUp(GameObject characterPrefab, List<GameObject> outPrefabs, HashSet<GameObject> outEffects)
	{
		using (new MiniProfilerScope("CharacterManager.GatherCharacterPrefabsForWarmUp"))
		{
			outPrefabs.Add(characterPrefab);
			outPrefabs.Add(Choreographer.s_Choreographer.m_ActorPrefab);
			ActorBehaviour actorBehaviour = Choreographer.s_Choreographer.m_ActorPrefab?.GetComponent<ActorBehaviour>();
			if (actorBehaviour != null)
			{
				outPrefabs.Add(actorBehaviour.WorldspacePanelUIPrefab);
			}
			CharacterManager component = characterPrefab.GetComponent<CharacterManager>();
			if (component == null)
			{
				return;
			}
			if (component.CharacterChildPrefab != null)
			{
				outPrefabs.Add(component.CharacterChildPrefab);
			}
			if (component.CharacterMenuModelPrefab != null)
			{
				outPrefabs.Add(component.CharacterMenuModelPrefab);
			}
			if (component.StandardHitEffect != null)
			{
				outEffects.Add(component.StandardHitEffect);
			}
			if (component.ShieldHitEffect != null)
			{
				outEffects.Add(component.ShieldHitEffect);
			}
			if (component.CriticalHitEffect != null)
			{
				outEffects.Add(component.CriticalHitEffect);
			}
			foreach (GameObject item in component.WeaponLists.SelectMany((CharacterWeaponList wl) => wl.Weapons))
			{
				outPrefabs.Add(item);
			}
		}
	}

	public static void RecycleCharacterInstance(GameObject character)
	{
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(character);
		if ((bool)actorBehaviour)
		{
			if ((bool)actorBehaviour.m_WorldspacePanelUI)
			{
				ObjectPool.Recycle(actorBehaviour.m_WorldspacePanelUI.gameObject, actorBehaviour.WorldspacePanelUIPrefab);
			}
			actorBehaviour.m_WorldspacePanelUI = null;
		}
		CharacterManager characterManager = GetCharacterManager(character);
		if (characterManager == null)
		{
			if (actorBehaviour != null)
			{
				ObjectPool.Recycle(actorBehaviour.gameObject, Choreographer.s_Choreographer.m_ActorPrefab);
			}
		}
		else
		{
			characterManager.DeinitializeCharacter();
			ObjectPool.Recycle(character, characterManager.CharacterPrefab);
		}
	}

	public void InitialiseCharacter(bool forMenu = false, bool isPreview = false)
	{
		GameObject gameObject = ((forMenu && CharacterMenuModelPrefab != null) ? CharacterMenuModelPrefab : CharacterChildPrefab);
		bool flag = LastSpawnedPrefab != null && LastSpawnedPrefab != gameObject;
		if (flag && CharacterChildPrefabInstance != null)
		{
			DeinitializeCharacter();
		}
		LastSpawnedPrefab = gameObject;
		CharacterChildPrefabInstance = ObjectPool.Spawn(gameObject, transform, flag);
		TargetBoneInstance = base.gameObject.FindInChildren(TargetBone);
		if (TargetBoneInstance == null)
		{
			Log.Error("Error: Unable to find bone \"" + TargetBone + "\" on character \"" + base.gameObject.name + "\" with prefab \"" + gameObject.name + "\".  Make sure this property is set correctly on this characters prefab.");
		}
		ImpactPointInstance = base.gameObject.FindInChildren("EXT_Impact_Point");
		foreach (CharacterWeaponList weaponList in WeaponLists)
		{
			int nextWeaponIndex = GetNextWeaponIndex(base.gameObject.name + weaponList.name, weaponList.Weapons.Count);
			if (weaponList.Weapons.Count < nextWeaponIndex + 1)
			{
				Log.Error("Error: Unable to find weapon " + nextWeaponIndex + " on character " + base.gameObject.name + ".  Make sure this property is set correctly on this characters prefab.");
			}
			else
			{
				DoEquip(weaponList.Weapons[nextWeaponIndex], weaponList.AttachToBone);
			}
		}
		if (!isPreview && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && CharacterActor.OriginalType == CActor.EType.Player)
		{
			CPlayerActor cPlayerActor = (CPlayerActor)CharacterActor;
			int controllableID = ControllableRegistry.GetControllableID(cPlayerActor);
			NetworkControllable controllable = ControllableRegistry.GetControllable(controllableID);
			PartySlotID partySlotID = default;
			if (controllable == null)
			{
				CMapParty cMapParty = AdventureState.MapState?.MapParty;
				if (cMapParty != null)
				{
					int playerSlotIndex = cMapParty.GetPlayerSlotIndex(cPlayerActor.Class?.ID);
					if (playerSlotIndex >= 0)
					{
						partySlotID = PartySlotID.FromIndex(playerSlotIndex);
					}
				}
				if (partySlotID == default(PartySlotID))
				{
					if (!Choreographer.IsMultiplayerDisabled() && (SaveData.Instance?.Global?.CurrentCustomLevelData == null || SaveData.Instance.Global.CurrentCustomLevelData.PartySpawnType != ELevelPartyChoiceType.PresetSpawnSpecificLocations))
					{
						Log.Error($"Unable to decide controllable slot for {cPlayerActor}, assigning the first empty one");
					}
					partySlotID = PlayerRegistry.GetFirstSlotWithoutControllable();
				}
			}
			else
			{
				partySlotID = controllable.Slot;
			}
			ControllableRegistry.CreateControllable(controllableID, this, partySlotID);
			UpdateUnderMyControl();
		}
		Cloth[] componentsInChildren = transform.GetComponentsInChildren<Cloth>();
		foreach (Cloth obj in componentsInChildren)
		{
			obj.coefficients = obj.coefficients;
		}
		m_Colliders = GetComponentsInChildren<Collider>();
		m_Renderers = GetComponentsInChildren<Renderer>();
		m_InvisibilityValue = (isPreview ? 1 : 0);
		m_GlowValue = (isPreview ? glowMaterialPreview : 0f);
		m_OpacityValue = (isPreview ? oppacityMaterialPreview : 1f);
		RefreshVisibility();
		RefreshTaticalOnlyElements(CharacterChildPrefabInstance, isPreview);
		PreloadCharacterAudio();
	}

	private void PreloadCharacterAudio()
	{
		AudioSMB[] behaviours = GetComponentInChildren<Animator>().GetBehaviours<AudioSMB>();
		foreach (AudioSMB audioSMB in behaviours)
		{
			if (!string.IsNullOrEmpty(audioSMB.AudioEvent))
			{
				AudioController.PreloadAudio(audioSMB.AudioEvent);
			}
			else
			{
				Log.Error("AudioSMB on character " + gameObject.name + " has no AudioEvent set. Please set one or remove the script from the SMB.");
			}
		}
	}

	public void DeinitializeCharacter()
	{
		if (CharacterActorGO != null)
		{
			ObjectPool.Recycle(CharacterActorGO);
		}
		foreach (GameObject equippedWeapon in EquippedWeapons)
		{
			ObjectPool.Recycle(equippedWeapon);
		}
		EquippedWeapons.Clear();
		UnityEngine.Object.Destroy(CharacterChildPrefabInstance.GetComponentInChildren<ActorEvents>());
		ObjectPool.Recycle(CharacterChildPrefabInstance, LastSpawnedPrefab);
		LastSpawnedPrefab = null;
	}

	public void DoEquip(GameObject weapon, string attachToBone)
	{
		Transform bone = gameObject.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform x) => x.name == attachToBone);
		DoEquip(weapon, bone);
	}

	public void DoEquip(GameObject weapon, Transform bone)
	{
		try
		{
			if (!(weapon != null))
			{
				return;
			}
			if (bone != null)
			{
				Weapon component = weapon.GetComponent<Weapon>();
				if (component == null)
				{
					Log.Error("Error: Weapon " + weapon.name + " does not have the Weapon component attached.  Unable to load.");
					return;
				}
				GameObject gameObject = ObjectPool.Spawn(weapon, bone, weapon.transform.position, Quaternion.LookRotation(bone.forward, bone.up));
				if (component.WeaponType == Weapon.WeaponTypes.Melee)
				{
					WeaponCollision weaponCollision = gameObject.gameObject.GetComponent<WeaponCollision>();
					if (weaponCollision == null)
					{
						weaponCollision = gameObject.gameObject.AddComponent<WeaponCollision>();
					}
					weaponCollision.enabled = false;
					Rigidbody rigidbody = gameObject.gameObject.GetComponent<Rigidbody>();
					if (rigidbody == null)
					{
						rigidbody = gameObject.gameObject.AddComponent<Rigidbody>();
					}
					rigidbody.useGravity = false;
					rigidbody.isKinematic = true;
				}
				gameObject.name = weapon.name;
				EquippedWeapons.Add(gameObject);
			}
			else
			{
				Log.Error("Error: Null bone passed to DoEquip for GameObject " + base.gameObject.name + ".  Weapon has not been attached");
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "An exception occurred while trying to run DoEquip. \n");
		}
	}

	public Vector3 GetTargetZone()
	{
		if (TargetBoneInstance == null)
		{
			Log.Error("Error: Target Bone " + TargetBone + " in character " + gameObject.name + " is null.  Using characters transform as target position.");
			return gameObject.transform.position;
		}
		float num = ((SharedClient.GlobalRNG.Next(2) == 0) ? ((float)SharedClient.GlobalRNG.NextDouble() * HitRadius) : ((0f - (float)SharedClient.GlobalRNG.NextDouble()) * HitRadius));
		return new Vector3(y: ((SharedClient.GlobalRNG.Next(2) == 0) ? ((float)SharedClient.GlobalRNG.NextDouble() * HitRadius) : ((0f - (float)SharedClient.GlobalRNG.NextDouble()) * HitRadius)) + TargetBoneInstance.transform.position.y, x: num + TargetBoneInstance.transform.position.x, z: TargetBoneInstance.transform.position.z);
	}

	public static CharacterManager GetCharacterManager(GameObject character)
	{
		if (character == null)
		{
			return null;
		}
		if (ActiveCharacters.TryGetValue(character, out var value))
		{
			return value;
		}
		value = character.GetComponent<CharacterManager>();
		if (value == null)
		{
			value = character.GetComponentInParent<CharacterManager>();
		}
		return value;
	}

	public static IEnumerable<CharacterManager> GetActiveCharacterManagers()
	{
		return ActiveCharacters.Values;
	}

	public static void EnableWeaponColliderForAttack(CActor actor)
	{
		EnableWeaponColliderForAttack(Choreographer.s_Choreographer.FindClientActorGameObject(actor));
	}

	public static void EnableWeaponColliderForAttack(GameObject actorGO)
	{
		WeaponCollision[] array = actorGO?.GetComponentsInChildren<WeaponCollision>();
		if (array != null)
		{
			WeaponCollision[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
		}
	}

	public Vector3 GetImpactPointPos()
	{
		if (ImpactPointInstance != null)
		{
			return ImpactPointInstance.transform.position;
		}
		if (TargetBoneInstance != null)
		{
			return GetTargetZone();
		}
		return transform.position;
	}

	public static GameObject FindTargetAtTile(CClientTile tile)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) != null)
		{
			return Choreographer.s_Choreographer.FindClientActorGameObject(ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex));
		}
		return null;
	}

	private int GetNextWeaponIndex(string characterName, int Count)
	{
		int num = 0;
		SharedLibrary.Random random = new SharedLibrary.Random();
		if (s_WeaponAssignment.TryGetValue(characterName, out var value))
		{
			if (value.Count == Count)
			{
				value.Clear();
			}
			num = GloomUtility.ExclusiveRandomNumber(random, value, Count - 1);
			value.Add(num);
		}
		else
		{
			num = random.Next(Count);
			s_WeaponAssignment.Add(characterName, new HashSet<int>());
			s_WeaponAssignment[characterName].Add(num);
		}
		return num;
	}

	public void RefreshVisibility()
	{
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer == null)
			{
				continue;
			}
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			for (int j = 0; j < materialClonesSlow.Length; j++)
			{
				Material material = materialClonesSlow[j];
				if (material == null)
				{
					Log.Error("Null material found in " + renderer.name);
					continue;
				}
				material.SetFloat("_InvisibilityControl", m_InvisibilityValue);
				material.SetFloat("_Glow", m_GlowValue);
				material.SetFloat("_Opacity", m_OpacityValue);
				float value = (Mathf.Approximately(0f, m_InvisibilityValue) ? 0f : 1f);
				material.SetFloat("_Toggle_Dissolve", value);
				materialClonesSlow[j] = material;
			}
		}
	}

	public void RefreshTaticalOnlyElements(GameObject instance, bool isPreview)
	{
		if (!(instance == null))
		{
			TacticalOnlyElements component = instance.GetComponent<TacticalOnlyElements>();
			if (!(component == null))
			{
				component.EnableElements(!isPreview);
			}
		}
	}

	public void FadeOutVisibility(float duration = 0.5f)
	{
		StopFadeAnimations();
		m_FadeOutTween = DOTween.To(() => m_InvisibilityValue, (float value) =>
		{
			m_InvisibilityValue = value;
			RefreshVisibility();
		}, 1f, duration).OnComplete(() =>
		{
			m_FadeOutTween = null;
		});
	}

	public void FadeInVisibility(float duration = 0.5f)
	{
		StopFadeAnimations();
		m_FadeInTween = DOTween.To(() => m_InvisibilityValue, (float value) =>
		{
			m_InvisibilityValue = value;
			RefreshVisibility();
		}, 0f, duration).OnComplete(() =>
		{
			m_FadeInTween = null;
		});
	}

	private void StopFadeAnimations()
	{
		if (m_FadeInTween != null)
		{
			m_FadeInTween.Kill();
		}
		m_FadeInTween = null;
		if (m_FadeOutTween != null)
		{
			m_FadeOutTween.Kill();
		}
		m_FadeOutTween = null;
	}

	public void UpdateLayers()
	{
		if (CharacterActor == null || !EnvironmentLoader.IsInstanceValid || !EnvironmentLoader.Instance.IsLoadingComplete || ClientScenarioManager.s_ClientScenarioManager == null || ClientScenarioManager.s_ClientScenarioManager.ClientTileArray == null || CharacterActor.IsDead || ClientScenarioManager.s_ClientScenarioManager == null)
		{
			return;
		}
		(int, int) tuple = (CharacterActor.ArrayIndex.X, CharacterActor.ArrayIndex.Y);
		if (ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(0) <= tuple.Item1 || ClientScenarioManager.s_ClientScenarioManager.ClientTileArray.GetLength(1) <= tuple.Item2)
		{
			return;
		}
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tuple.Item1, tuple.Item2];
		GameObject map = EnvironmentLoader.Instance.GetMap(cClientTile?.m_Tile?.m_HexMap?.MapGuid);
		if (!map)
		{
			return;
		}
		ProceduralMapTile component = map.GetComponent<ProceduralMapTile>();
		if (!component || !component.Ambience)
		{
			return;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.SetLayer(component.Ambience.currentRenderingLayer);
			Material[] materialClonesSlow = renderer.GetMaterialClonesSlow();
			foreach (Material characterLightsTo in materialClonesSlow)
			{
				component.Ambience.SetCharacterLightsTo(characterLightsTo);
			}
			component.Ambience.SetReflectionsTo(renderer);
		}
		lastLitCActorPosition = tuple;
	}

	private void Update()
	{
		if (CharacterActor != null)
		{
			(int, int) valueTuple = (CharacterActor.ArrayIndex.X, CharacterActor.ArrayIndex.Y);
			(int, int) tuple = lastLitCActorPosition;
			(int, int) tuple2 = valueTuple;
			if (tuple.Item1 != tuple2.Item1 || tuple.Item2 != tuple2.Item2)
			{
				UpdateLayers();
			}
		}
	}

	public void OnBeforeForcedMovement()
	{
		ParticleSystem[] componentsInChildren = transform.GetComponentsInChildren<ParticleSystem>();
		m_particleSystemCache.Clear();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem.gameObject.activeInHierarchy && !particleSystem.isStopped && !particleSystem.isPaused && particleSystem.GetComponent<ParticleSystemRenderer>().enabled)
			{
				particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				m_particleSystemCache.Add(particleSystem);
			}
		}
	}

	public void OnAfterForcedMovement()
	{
		foreach (ParticleSystem item in m_particleSystemCache)
		{
			item.Simulate(0.5f, withChildren: true, restart: true);
			item.Play(withChildren: true);
		}
		m_particleSystemCache.Clear();
		Cloth[] componentsInChildren = transform.GetComponentsInChildren<Cloth>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ClearTransformMotion();
		}
		DynamicBone[] componentsInChildren2 = transform.GetComponentsInChildren<DynamicBone>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].ResetParticlesPosition();
		}
	}

	private void UpdateUnderMyControl()
	{
		NetworkControllable controllable = ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID((CPlayerActor)CharacterActor));
		if (controllable == null)
		{
			return;
		}
		if (controllable.Controller == PlayerRegistry.MyPlayer)
		{
			if (!CharacterActor.IsUnderMyControl())
			{
				Log.NetworkInfo(LocalizationManager.GetTranslation(CharacterActor.ActorLocKey()) + "(SCENARIO) is now under my control.");
			}
		}
		else if (CharacterActor.IsUnderMyControl())
		{
			Log.NetworkInfo(LocalizationManager.GetTranslation(CharacterActor.ActorLocKey()) + "(SCENARIO) is no longer under my control.");
		}
	}

	public void OnControlAssigned(NetworkPlayer controller)
	{
		UpdateUnderMyControl();
	}

	public void OnControlReleased()
	{
		UpdateUnderMyControl();
	}

	public string GetName()
	{
		if (CharacterActor == null)
		{
			return "UNINITIALIZED";
		}
		return CharacterActor.Class.ID;
	}

	public string GetID()
	{
		if (CharacterActor == null)
		{
			return "UNINITIALIZED";
		}
		return CharacterActor.Class.ID;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
	}
}

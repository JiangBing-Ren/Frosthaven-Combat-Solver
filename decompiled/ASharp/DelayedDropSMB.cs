using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FH.LevelComponents.ProcGen;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DelayedDropSMB : StateMachineBehaviour
{
	public enum ItemType
	{
		General,
		MoneyToken,
		Obstacle,
		Trap
	}

	[Tooltip("The prefab which will be spawned")]
	[SerializeField]
	private GameObject m_ItemPrefab;

	[Tooltip("If enabled ignore SpawnDelay property and spawn always at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[Tooltip("Delay amount of seconds ater animation start playing")]
	[SerializeField]
	private float m_SpawnDelay;

	private float m_InstanceSpawnDelay;

	[Tooltip("Type of item to spawn")]
	[SerializeField]
	private ItemType m_Type;

	private bool m_NotSpawned;

	private List<CClientTile> m_TargetTiles;

	private AnimatorStateInfo m_StateInfo;

	private CActor m_ActorPreemptivelyLooting;

	private CActor m_ActorToRemoveGoldPileFrom;

	private GameObject m_GoldPileObjectWeWantToRemoveFromPlayer;

	private float m_OriginalSpawnDelay;

	private List<Tuple<CObjectProp, CClientTile>> m_DroppedProps;

	private bool m_ReadyToSpawn;

	public AnimatorStateInfo StateInfo => m_StateInfo;

	private void Awake()
	{
		m_OriginalSpawnDelay = m_SpawnDelay;
	}

	private void OnDestroy()
	{
		if (m_NotSpawned)
		{
			TrySpawn();
		}
		if ((bool)Choreographer.s_Choreographer)
		{
			Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
		}
	}

	public void Clear()
	{
		m_ReadyToSpawn = false;
		m_DroppedProps?.Clear();
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		float num = animatorStateInfo.length;
		if (float.IsInfinity(num))
		{
			num = 1f;
		}
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		if (Timekeeper.instance.m_GlobalClock.timeScale != 0f)
		{
			num /= Timekeeper.instance.m_GlobalClock.timeScale;
		}
		Choreographer.s_Choreographer.AddPendingAnimationToWait(this);
		m_StateInfo = animatorStateInfo;
		m_NotSpawned = true;
		m_InstanceSpawnDelay = (m_OnAnimationEnd ? num : Mathf.Min(m_SpawnDelay, num));
		m_ActorPreemptivelyLooting = null;
		m_DroppedProps = new List<Tuple<CObjectProp, CClientTile>>();
		m_ReadyToSpawn = false;
		CharacterManager componentInParent = animator.gameObject.GetComponentInParent<CharacterManager>();
		CActor cActor = ((componentInParent != null) ? componentInParent.CharacterActor : null);
		ItemType type = m_Type;
		if (type != ItemType.MoneyToken && (uint)(type - 2) <= 1u)
		{
			m_TargetTiles = Choreographer.s_Choreographer.m_LastSelectedTiles;
			if (m_TargetTiles == null)
			{
				m_TargetTiles = new List<CClientTile> { Choreographer.s_Choreographer.m_lastSelectedTile };
			}
		}
		else if (componentInParent != null && cActor != null)
		{
			if ((cActor.OriginalType == CActor.EType.Enemy || cActor.OriginalType == CActor.EType.Enemy2) && !cActor.NoGoldDrop && !(cActor is CObjectActor))
			{
				CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				m_TargetTiles = new List<CClientTile> { item };
			}
			cActor.NoGoldDrop = false;
		}
		m_ReadyToSpawn = false;
		if (m_TargetTiles == null)
		{
			return;
		}
		foreach (CClientTile spawnTile in m_TargetTiles)
		{
			CObjectProp propObject = null;
			switch (m_Type)
			{
			case ItemType.MoneyToken:
				propObject = new CObjectGoldPile(EPropType.LootToken.ToString(), ScenarioManager.ObjectImportType.MoneyToken, spawnTile.m_Tile.m_HexMap.MapGuid, cActor);
				if (componentInParent != null && cActor != null)
				{
					Log.SimpleInfo($"Actor {cActor} dropped a Money Token");
				}
				break;
			case ItemType.Obstacle:
				propObject = new CObjectObstacle(EPropType.OneHexObstacle.ToString(), ScenarioManager.ObjectImportType.Obstacle, spawnTile.m_Tile.m_HexMap.MapGuid);
				break;
			case ItemType.Trap:
				propObject = new CObjectTrap(EPropType.Trap.ToString(), ScenarioManager.ObjectImportType.Trap, spawnTile.m_Tile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, ScenarioManager.TrapDamage);
				break;
			}
			m_DroppedProps.Add(new Tuple<CObjectProp, CClientTile>(propObject, spawnTile));
			CVector3 pos = GloomUtility.VToCV(spawnTile.m_GameObject.transform.position);
			ScenarioRuleClient.ExecuteSRLCommand(null, null, () =>
			{
				propObject.SetLocation(new TileIndex(spawnTile.m_Tile.m_ArrayIndex), pos, propObject.Rotation);
				spawnTile.m_Tile.SpawnProp(propObject, notifyClient: false);
				if (m_DroppedProps.All((Tuple<CObjectProp, CClientTile> x) => x.Item2.m_Tile.m_Props.Contains(x.Item1)))
				{
					m_ReadyToSpawn = true;
				}
			}, "SPAWN_LOOT_TOKEN");
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		m_InstanceSpawnDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		if (m_InstanceSpawnDelay <= 0f && m_NotSpawned)
		{
			TrySpawn();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (m_NotSpawned)
		{
			TrySpawn();
		}
	}

	private void TrySpawn()
	{
		m_NotSpawned = false;
		if (m_DroppedProps == null || m_DroppedProps.Count == 0)
		{
			Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
		}
		else
		{
			CoroutineHelper.instance.StartCoroutine(TrySpawnCoroutine());
		}
	}

	private IEnumerator TrySpawnCoroutine()
	{
		while (!m_ReadyToSpawn)
		{
			yield return new WaitForEndOfFrame();
		}
		Spawn();
	}

	private void Spawn()
	{
		foreach (Tuple<CObjectProp, CClientTile> droppedProp in m_DroppedProps)
		{
			CObjectProp item = droppedProp.Item1;
			CClientTile item2 = droppedProp.Item2;
			if (item != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_ItemPrefab, item2.m_GameObject.transform.parent);
				UnityGameEditorObject component = gameObject.GetComponent<UnityGameEditorObject>();
				if (component == null)
				{
					Log.Error("No UnityGameEditorObject script found on " + m_Type);
					UnityEngine.Object.Destroy(gameObject);
					continue;
				}
				gameObject.name = item.InstanceName;
				gameObject.transform.position = item2.m_GameObject.transform.position;
				component.PropObject = droppedProp.Item1;
				PropParent orAdd = gameObject.GetOrAdd<PropParent>();
				if (orAdd != null)
				{
					orAdd.Init(placedBeforehand: false);
					ProceduralMapTile componentInParent = orAdd.GetComponentInParent<ProceduralMapTile>();
					if ((bool)componentInParent)
					{
						orAdd.SetPropLighting(visible: true, componentInParent.Ambience);
					}
					switch (m_Type)
					{
					case ItemType.Trap:
					{
						SpawnPFXOnEnable componentInChildren5 = gameObject.GetComponentInChildren<SpawnPFXOnEnable>();
						if (componentInChildren5 != null)
						{
							componentInChildren5.enabled = true;
						}
						break;
					}
					case ItemType.MoneyToken:
					{
						PlayableDirector componentInChildren3 = gameObject.GetComponentInChildren<PlayableDirector>();
						componentInChildren3?.Play();
						if ((bool)gameObject.GetComponentInChildren<PropDynamicYPos>())
						{
							IEnumerable<CObjectProp> enumerable = item2.m_Tile.m_Props.Where((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.MoneyToken);
							int num = enumerable.Count();
							if (num > 1)
							{
								Vector3 vector = new Vector3(0f, 0f, 0.4f);
								float num2 = 360f / (float)num;
								float num3 = 0f;
								foreach (CObjectProp item3 in enumerable)
								{
									GameObject gameObject2 = Choreographer.FindPropGameObject(item3);
									if ((bool)gameObject2)
									{
										PropDynamicYPos componentInChildren4 = gameObject2.GetComponentInChildren<PropDynamicYPos>();
										if ((bool)componentInChildren4)
										{
											Vector3 vector2 = Quaternion.Euler(new Vector3(0f, num3, 0f)) * vector;
											componentInChildren4.HorizontalOffset = new Vector2(vector2.x, vector2.z);
										}
										num3 += num2;
									}
								}
							}
						}
						if (m_ActorPreemptivelyLooting != null)
						{
							m_ActorToRemoveGoldPileFrom = m_ActorPreemptivelyLooting;
							m_GoldPileObjectWeWantToRemoveFromPlayer = Choreographer.FindPropGameObject(item);
							m_ActorPreemptivelyLooting.LootTile(item2.m_Tile);
							componentInChildren3.stopped += PreemptiveLootingPostDirectorAction;
						}
						break;
					}
					case ItemType.Obstacle:
					{
						PlayableDirector componentInChildren = gameObject.GetComponentInChildren<PlayableDirector>();
						TimelineAssets componentInChildren2 = gameObject.GetComponentInChildren<TimelineAssets>();
						if (!(componentInChildren == null) && !(componentInChildren2 == null))
						{
							componentInChildren.playableAsset = componentInChildren2.m_CreateTimeline;
							componentInChildren.Play();
						}
						break;
					}
					}
				}
			}
			if (item.DelayedLogSpawn())
			{
				item.LogSpawn();
			}
		}
		m_ActorPreemptivelyLooting = null;
		Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
	}

	private void PreemptiveLootingPostDirectorAction(PlayableDirector playableDir)
	{
		TimelineAsset asset = playableDir.GetComponentInChildren<TimelineAssets>().FindTimelineAsset("LOOTPickUpTimeline ");
		playableDir.Play(asset);
		playableDir.stopped -= PreemptiveLootingPostDirectorAction;
		if (m_ActorToRemoveGoldPileFrom != null && !(m_GoldPileObjectWeWantToRemoveFromPlayer == null))
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.FindClientActorGameObject(m_ActorToRemoveGoldPileFrom));
			if (m_GoldPileObjectWeWantToRemoveFromPlayer != null)
			{
				characterManager.LootTokensToCollect.Remove(m_GoldPileObjectWeWantToRemoveFromPlayer);
			}
		}
	}

	public void SetActorToPreemptivelyLootOnTiles(CActor actorWaitingToLoot)
	{
		m_ActorPreemptivelyLooting = null;
		if (m_Type == ItemType.MoneyToken && m_TargetTiles != null && m_TargetTiles.Count != 0)
		{
			m_ActorPreemptivelyLooting = actorWaitingToLoot;
		}
	}
}

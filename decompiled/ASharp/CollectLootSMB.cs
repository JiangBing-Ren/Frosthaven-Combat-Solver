using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CollectLootSMB : StateMachineBehaviour
{
	[SerializeField]
	private float m_Delay;

	private float m_InstanceDelay;

	[SerializeField]
	private GameObject m_PFXOnPickup;

	private bool m_IsLooted;

	private List<GameObject> m_ActorObjectsToCheckForDelayedDroppers;

	private CActor m_ActorPerformingLoot;

	private int m_ExtraGoldFromAdditionalEffects;

	private float m_OriginalDelay;

	private void Awake()
	{
		m_OriginalDelay = m_Delay;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		m_IsLooted = false;
		m_InstanceDelay = m_Delay;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (m_InstanceDelay > 0f || m_IsLooted)
		{
			m_InstanceDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		else
		{
			CollectLoot(animator);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!m_IsLooted)
		{
			CollectLoot(animator);
		}
	}

	public static void CollectLoot(GameObject lootToken)
	{
		PlayableDirector componentInChildren = lootToken.GetComponentInChildren<PlayableDirector>();
		TimelineAsset asset = lootToken.GetComponentInChildren<TimelineAssets>().FindTimelineAsset("LOOTPickUpTimeline ");
		componentInChildren.Play(asset);
	}

	private void CollectLoot(Animator animator)
	{
		m_IsLooted = true;
		CharacterManager componentInParent = animator.gameObject.GetComponentInParent<CharacterManager>();
		if (componentInParent == null)
		{
			Log.Error("CollectLootSMB: need CharacterManager on hero game object");
			return;
		}
		if (m_PFXOnPickup != null && (componentInParent.LootTokensToCollect.Count > 0 || componentInParent.ChestsToCollect.Count > 0))
		{
			GameObject gameObject = ObjectPool.Spawn(m_PFXOnPickup, animator.transform);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), m_PFXOnPickup);
		}
		foreach (GameObject item in componentInParent.LootTokensToCollect)
		{
			if (item != null)
			{
				CollectLoot(item);
			}
		}
		componentInParent.LootTokensToCollect.Clear();
		componentInParent.ChestsToCollect.Clear();
		CheckLootedTilesForDelayedDropSMBToWaitFor();
	}

	public void CheckLootedTilesForDelayedDropSMBToWaitFor()
	{
		if (m_ActorObjectsToCheckForDelayedDroppers == null || m_ActorPerformingLoot == null)
		{
			m_ActorObjectsToCheckForDelayedDroppers = null;
			m_ActorPerformingLoot = null;
			return;
		}
		for (int i = 0; i < m_ActorObjectsToCheckForDelayedDroppers.Count; i++)
		{
			GameObject gameObject = m_ActorObjectsToCheckForDelayedDroppers[i];
			if (gameObject == null)
			{
				continue;
			}
			List<DelayedDropSMB> list = MF.GameObjectAnimatorStateBehaviours<DelayedDropSMB>(gameObject);
			for (int j = 0; j < list.Count; j++)
			{
				if (MF.GameObjectAnimatorControllerIsCurrentState(gameObject, list[j].StateInfo.fullPathHash))
				{
					list[j].SetActorToPreemptivelyLootOnTiles(m_ActorPerformingLoot);
				}
			}
		}
		m_ActorObjectsToCheckForDelayedDroppers = null;
		m_ActorPerformingLoot = null;
	}

	public void SetTilesToCheckForDelayedDropSMB(List<GameObject> actorObjectsToCheckToCheck, CActor actorLooting)
	{
		m_ActorObjectsToCheckForDelayedDroppers = actorObjectsToCheckToCheck;
		m_ActorPerformingLoot = actorLooting;
	}

	public void SetExtraGoldFromAdditionalEffects(int extraGold)
	{
		m_ExtraGoldFromAdditionalEffects = extraGold;
	}

	public void SetActor(CActor actorLooting)
	{
		m_ActorPerformingLoot = actorLooting;
	}
}

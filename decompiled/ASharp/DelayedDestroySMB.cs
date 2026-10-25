using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class DelayedDestroySMB : StateMachineBehaviour
{
	public enum ItemType
	{
		General,
		MoneyToken,
		Obstacle,
		Trap
	}

	[Tooltip("Self destroy object on which this SMB script is located")]
	[SerializeField]
	private bool m_WillSelfDestruct;

	[Tooltip("If enabled ignore SpawnDelay property and destroy at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[SerializeField]
	private float m_DelayAfterAnimationEnd;

	[Tooltip("Delay amount of seconds after animation start playing")]
	[SerializeField]
	private float m_DestroyDelay;

	private float m_InstanceDestroyDelay;

	[Tooltip("Type of item to destroy")]
	[SerializeField]
	private ItemType m_Type;

	private bool m_NotDestroyed;

	private List<CClientTile> m_TargetTiles;

	private readonly object spawnTile;

	private float m_OriginalDelayAfterAnimationEnd;

	private float m_OriginalDestroyDelay;

	private GameObject m_DestroyObject;

	private void Awake()
	{
		m_OriginalDelayAfterAnimationEnd = m_DelayAfterAnimationEnd;
		m_OriginalDestroyDelay = m_DestroyDelay;
	}

	private void OnDestroy()
	{
		if ((bool)Choreographer.s_Choreographer)
		{
			Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		m_NotDestroyed = true;
		float length = animatorStateInfo.length;
		m_InstanceDestroyDelay = (m_OnAnimationEnd ? length : Mathf.Min(m_DestroyDelay, length));
		Choreographer.s_Choreographer.AddPendingAnimationToWait(this);
		ItemType type = m_Type;
		if (type != ItemType.MoneyToken && (uint)(type - 2) <= 1u)
		{
			m_TargetTiles = new List<CClientTile>();
			UnityGameEditorObject componentInParent = animator.gameObject.GetComponentInParent<UnityGameEditorObject>();
			m_DestroyObject = componentInParent.gameObject;
			if (componentInParent != null && (componentInParent.m_ObjectType == ScenarioManager.ObjectImportType.Trap || componentInParent.m_ObjectType == ScenarioManager.ObjectImportType.Obstacle))
			{
				CTile cTile = CObjectProp.FindPropTile(componentInParent.name);
				if (cTile != null)
				{
					m_TargetTiles = new List<CClientTile> { ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y] };
				}
			}
			if (m_TargetTiles.Count == 0)
			{
				m_TargetTiles = Choreographer.s_Choreographer.m_LastSelectedTiles;
				if (m_TargetTiles == null)
				{
					m_TargetTiles = new List<CClientTile> { Choreographer.s_Choreographer.m_lastSelectedTile };
				}
			}
		}
		else
		{
			CharacterManager componentInParent2 = animator.gameObject.GetComponentInParent<CharacterManager>();
			if (componentInParent2 != null)
			{
				CActor characterActor = componentInParent2.CharacterActor;
				CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[characterActor.ArrayIndex.X, characterActor.ArrayIndex.Y];
				m_TargetTiles = new List<CClientTile> { item };
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (m_NotDestroyed)
		{
			if (m_OnAnimationEnd && m_DelayAfterAnimationEnd > 0f)
			{
				CoroutineHelper.RunCoroutine(CoroutineHelper.DelayedStartCoroutine(m_DelayAfterAnimationEnd, Destroy));
			}
			else
			{
				Destroy();
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!m_OnAnimationEnd)
		{
			m_InstanceDestroyDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
			if (m_InstanceDestroyDelay <= 0f && m_NotDestroyed)
			{
				Destroy();
			}
		}
	}

	private void Destroy()
	{
		Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
		if (m_DestroyObject == null)
		{
			return;
		}
		if (m_WillSelfDestruct)
		{
			Object.Destroy(m_DestroyObject);
			return;
		}
		m_NotDestroyed = false;
		if (m_Type == ItemType.Trap)
		{
			UnityGameEditorObject componentInParent = m_DestroyObject.GetComponentInParent<UnityGameEditorObject>();
			if (componentInParent != null)
			{
				Object.Destroy(componentInParent.gameObject);
				return;
			}
		}
		if (m_TargetTiles == null || m_TargetTiles.Count == 0)
		{
			return;
		}
		foreach (CClientTile targetTile in m_TargetTiles)
		{
			if (m_Type == ItemType.Trap && targetTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap prop)
			{
				Object.Destroy(Choreographer.FindPropGameObject(prop));
			}
		}
	}
}

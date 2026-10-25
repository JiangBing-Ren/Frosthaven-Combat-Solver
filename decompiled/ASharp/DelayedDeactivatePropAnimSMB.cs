using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class DelayedDeactivatePropAnimSMB : StateMachineBehaviour
{
	public enum ItemType
	{
		General,
		MoneyToken,
		Obstacle,
		Trap
	}

	[Tooltip("If enabled ignore SpawnDelay property and destroy at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[SerializeField]
	private float m_DelayAfterAnimationEnd;

	[Tooltip("Delay amount of seconds after animation start playing")]
	[SerializeField]
	private float m_DeactivateDelay;

	private float m_InstanceDeactivateDelay;

	[Tooltip("Type of item to deactivate")]
	[SerializeField]
	private ItemType m_Type;

	private bool m_NotDeactivated;

	private List<CClientTile> m_TargetTiles;

	private readonly object spawnTile;

	private float m_OriginalDelayAfterAnimationEnd;

	private float m_OriginalDeactivateDelay;

	private void Awake()
	{
		m_OriginalDelayAfterAnimationEnd = m_DelayAfterAnimationEnd;
		m_OriginalDeactivateDelay = m_DeactivateDelay;
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
		m_NotDeactivated = true;
		float length = animatorStateInfo.length;
		m_InstanceDeactivateDelay = (m_OnAnimationEnd ? length : Mathf.Min(m_DeactivateDelay, length));
		Choreographer.s_Choreographer.AddPendingAnimationToWait(this);
		if (m_Type != ItemType.Trap)
		{
			return;
		}
		m_TargetTiles = new List<CClientTile>();
		UnityGameEditorObject componentInParent = animator.gameObject.GetComponentInParent<UnityGameEditorObject>();
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

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (m_NotDeactivated)
		{
			if (m_OnAnimationEnd && m_DelayAfterAnimationEnd > 0f)
			{
				CoroutineHelper.RunCoroutine(CoroutineHelper.DelayedStartCoroutine(m_DelayAfterAnimationEnd, DeactivateProp, animator));
			}
			else
			{
				DeactivateProp(animator);
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (!m_OnAnimationEnd)
		{
			m_InstanceDeactivateDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
			if (m_InstanceDeactivateDelay <= 0f && m_NotDeactivated)
			{
				DeactivateProp(animator);
			}
		}
	}

	private void DeactivateProp(Animator animator)
	{
		Choreographer.s_Choreographer.RemovePendingAnimationToWait(this);
		if (animator == null)
		{
			return;
		}
		m_NotDeactivated = false;
		if (m_TargetTiles == null || m_TargetTiles.Count == 0)
		{
			return;
		}
		foreach (CClientTile targetTile in m_TargetTiles)
		{
			switch (m_Type)
			{
			case ItemType.Trap:
				if (targetTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap prop)
				{
					CDeactivatePropAnim_MessageData message = new CDeactivatePropAnim_MessageData(null)
					{
						m_Prop = prop,
						m_InitialLoad = false
					};
					Choreographer.s_Choreographer.PostMessageFromUI(message);
				}
				break;
			}
		}
	}
}

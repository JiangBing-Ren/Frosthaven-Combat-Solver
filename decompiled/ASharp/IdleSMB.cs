using Chronos;
using UnityEngine;

public class IdleSMB : StateMachineBehaviour
{
	private Animator m_Animator;

	private bool HasSaveData
	{
		get
		{
			if (SaveData.Instance != null)
			{
				return SaveData.Instance.Global != null;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (HasSaveData)
		{
			SaveData.Instance.Global.GameSpeedChanged += ChangeSpeed;
		}
	}

	private void OnDestroy()
	{
		if (HasSaveData)
		{
			SaveData.Instance.Global.GameSpeedChanged -= ChangeSpeed;
		}
	}

	private void ChangeSpeed()
	{
		if (!HasSaveData || m_Animator == null)
		{
			return;
		}
		if (SaveData.Instance.Global.DebugSpeedMode)
		{
			m_Animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
			return;
		}
		ActorBehaviour componentInChildren = m_Animator.gameObject.GetComponentInChildren<ActorBehaviour>();
		if (componentInChildren != null && componentInChildren.IsMoving)
		{
			m_Animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		}
		else
		{
			m_Animator.speed = 1f;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_Animator = animator;
		ChangeSpeed();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator != null)
		{
			animator.speed = 1f;
		}
		if (m_Animator != null)
		{
			m_Animator.speed = 1f;
			m_Animator = null;
		}
	}
}

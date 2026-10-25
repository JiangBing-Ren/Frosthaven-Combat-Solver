using UnityEngine;

public class ToggleAlternativeIdleFxSMB : StateMachineBehaviour
{
	private Animator m_Animator;

	private ParticleSystem m_standardFx;

	private ParticleSystem m_alterFx;

	public bool toggleAlternativeIdleFx;

	public string idleFxInitialChildFx = "standardFx";

	public string idleFxAlternativeChildFx = "alterFx";

	public bool idleFxIsAnimatorChild;

	public string idleFxName = "IdleFx";

	public bool keepToggleOnExit;

	private void ChangeIdleFx()
	{
		if (!(m_Animator != null))
		{
			return;
		}
		if (!idleFxIsAnimatorChild)
		{
			DeathDissolve component = m_Animator.GetComponent<DeathDissolve>();
			if (component != null)
			{
				GameObject gameObject = component.particlesIdle.FindInImmediateChildren(idleFxInitialChildFx);
				if (gameObject != null)
				{
					m_standardFx = gameObject.GetComponent<ParticleSystem>();
				}
				GameObject gameObject2 = component.particlesIdle.FindInImmediateChildren(idleFxAlternativeChildFx);
				if (gameObject2 != null)
				{
					m_alterFx = gameObject2.GetComponent<ParticleSystem>();
				}
			}
		}
		else
		{
			GameObject gameObject3 = m_Animator.gameObject.FindInImmediateChildren(idleFxName);
			if (gameObject3 != null)
			{
				GameObject gameObject4 = gameObject3.FindInImmediateChildren(idleFxInitialChildFx);
				if (gameObject4 != null)
				{
					m_standardFx = gameObject4.GetComponent<ParticleSystem>();
				}
				GameObject gameObject5 = gameObject3.FindInImmediateChildren(idleFxAlternativeChildFx);
				if (gameObject5 != null)
				{
					m_alterFx = gameObject5.GetComponent<ParticleSystem>();
				}
			}
		}
		if (m_standardFx == null || m_alterFx == null)
		{
			Log.Error("Missing standard or alternate idle FX in ToggleAlternativeIdleFxSMB");
		}
		if (!m_alterFx.gameObject.activeInHierarchy)
		{
			m_alterFx.gameObject.SetActive(value: true);
		}
		m_alterFx.Play();
		m_standardFx.Stop();
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_Animator = animator;
		if (toggleAlternativeIdleFx)
		{
			ChangeIdleFx();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator != null && toggleAlternativeIdleFx)
		{
			if (keepToggleOnExit)
			{
				m_standardFx.Stop();
			}
			else
			{
				m_alterFx.Stop();
				m_standardFx.Play();
			}
		}
		if (m_Animator != null)
		{
			m_Animator = null;
		}
	}
}

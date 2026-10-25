using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class Jump_SMB : StateMachineBehaviour
{
	[AudioEventName]
	public string m_SFX;

	private VFXLookup m_VFXLookup;

	private float m_Fade;

	private bool m_JumpSMBActive;

	private const float c_FadeSpeed = 10f;

	public static readonly int JumpEffectProperty = Shader.PropertyToID("_JumpEffect");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_JumpSMBActive)
		{
			ProcessStateEnter(animator);
		}
	}

	public void ProcessStateEnter(Animator animator)
	{
		m_JumpSMBActive = true;
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, animator.gameObject.transform);
		}
		m_VFXLookup = animator.gameObject.GetComponent<VFXLookup>();
		m_Fade = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_JumpSMBActive)
		{
			return;
		}
		m_Fade = Mathf.Min(m_Fade + Timekeeper.instance.m_GlobalClock.deltaTime * 10f, 1f);
		if (animator.GetBool("ExitJump"))
		{
			return;
		}
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer != null)
			{
				Material[] materialClonesSlow = characterRenderer.GetMaterialClonesSlow();
				for (int i = 0; i < materialClonesSlow.Length; i++)
				{
					materialClonesSlow[i].SetFloat(JumpEffectProperty, m_Fade);
				}
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (m_JumpSMBActive)
		{
			m_JumpSMBActive = false;
		}
	}
}

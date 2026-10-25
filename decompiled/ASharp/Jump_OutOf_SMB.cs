using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class Jump_OutOf_SMB : StateMachineBehaviour
{
	[AudioEventName]
	public string m_SFX;

	private VFXLookup m_VFXLookup;

	private float m_Fade;

	private const float c_FadeSpeed = 5f;

	private static readonly int JumpEffectProperty = Shader.PropertyToID("_JumpEffect");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, animator.gameObject.transform);
		}
		m_VFXLookup = animator.gameObject.GetComponent<VFXLookup>();
		if (!animator.GetBool("ExitJump"))
		{
			GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseArrive, null, animator.transform.position, animator.transform.rotation);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseArrive, "@ VFX Pool");
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_Fade = Mathf.Max(m_Fade - Timekeeper.instance.m_GlobalClock.deltaTime * 5f, 0f);
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer != null && !(characterRenderer is ParticleSystemRenderer))
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
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer != null && !(characterRenderer is ParticleSystemRenderer))
			{
				Material[] materialClonesSlow = characterRenderer.GetMaterialClonesSlow();
				for (int i = 0; i < materialClonesSlow.Length; i++)
				{
					materialClonesSlow[i].SetFloat(JumpEffectProperty, 0f);
				}
			}
		}
		animator.SetBool("ExitJump", value: false);
	}
}

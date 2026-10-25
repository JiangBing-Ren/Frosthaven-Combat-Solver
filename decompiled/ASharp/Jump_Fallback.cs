using Chronos;
using SETUtil.Materials;
using UnityEngine;

public class Jump_Fallback : MonoBehaviour
{
	[AudioEventName]
	public string m_SFX;

	private VFXLookup m_VFXLookup;

	private float m_Fade;

	private bool m_JumpSMBActive;

	private const float c_FadeSpeed = 10f;

	private static readonly int JumpEffectProperty = Shader.PropertyToID("_JumpEffect");

	private void OnEnable()
	{
		if (!m_JumpSMBActive)
		{
			ProcessStateEnter();
		}
	}

	public void ProcessStateEnter()
	{
		m_JumpSMBActive = true;
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, gameObject.transform);
		}
		m_VFXLookup = gameObject.GetComponent<VFXLookup>();
		m_Fade = 0f;
	}

	private void Update()
	{
		if (!m_JumpSMBActive)
		{
			return;
		}
		m_Fade = Mathf.Min(m_Fade + Timekeeper.instance.m_GlobalClock.deltaTime * 10f, 1f);
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

	public void ForceStop()
	{
		OnDisable();
	}

	private void OnDisable()
	{
		m_JumpSMBActive = false;
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer != null)
			{
				Material[] materialClonesSlow = characterRenderer.GetMaterialClonesSlow();
				for (int i = 0; i < materialClonesSlow.Length; i++)
				{
					materialClonesSlow[i].SetFloat(JumpEffectProperty, 0f);
				}
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimationGUIAnimator : CorrutineGUIAnimator
{
	[SerializeField]
	private float delay;

	[FormerlySerializedAs("animation")]
	[SerializeField]
	private Animation m_Animation;

	[SerializeField]
	private List<AnimationClip> clipsSequence;

	protected override IEnumerator Animation()
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		float num = 0f;
		if (clipsSequence.Count > 0)
		{
			foreach (AnimationClip item in clipsSequence)
			{
				num += item.length;
				m_Animation.PlayQueued(item.name);
			}
		}
		else
		{
			num = m_Animation.clip.length;
			m_Animation.Play();
		}
		yield return Timekeeper.instance.WaitForSeconds(num);
	}

	protected override void ResetFinishState()
	{
		if (clipsSequence.Count == 0)
		{
			m_Animation.ResetToEnd(m_Animation.clip.name);
			return;
		}
		for (int i = 0; i < clipsSequence.Count; i++)
		{
			m_Animation.ResetToEnd(clipsSequence[i].name);
		}
	}

	protected override void ResetInitialState()
	{
		if (clipsSequence.Count == 0)
		{
			m_Animation.ResetToStart(m_Animation.clip.name);
			return;
		}
		for (int num = clipsSequence.Count - 1; num >= 0; num--)
		{
			m_Animation.ResetToStart(clipsSequence[num].name);
		}
	}

	protected override void Clear()
	{
		base.Clear();
		m_Animation.Stop();
	}
}

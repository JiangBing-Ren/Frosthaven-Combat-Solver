using System.Collections;
using Chronos;
using UnityEngine;

public class UILoadingIconAnimator : CorrutineGUIAnimator
{
	[SerializeField]
	private RectTransform m_Icon;

	[SerializeField]
	private GUIAnimator animator;

	[SerializeField]
	private float m_IconSpinSpeed;

	[SerializeField]
	private float m_IconUpdateSpeed;

	[SerializeField]
	private bool autoStart;

	private void OnEnable()
	{
		if (autoStart)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	protected override void ResetFinishState()
	{
		m_Icon.transform.rotation = Quaternion.identity;
		animator.GoToFinishState();
	}

	protected override void ResetInitialState()
	{
		m_Icon.transform.rotation = Quaternion.identity;
		animator.GoInitState();
	}

	protected override IEnumerator Animation()
	{
		animator.Play();
		while (IsPlaying)
		{
			m_Icon.Rotate(new Vector3(0f, 0f, m_IconSpinSpeed));
			yield return Timekeeper.instance.WaitForSeconds(m_IconUpdateSpeed);
		}
	}

	protected override void Clear()
	{
		base.Clear();
		animator.Stop();
	}
}

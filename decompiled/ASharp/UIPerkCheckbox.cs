using UnityEngine;
using UnityEngine.UI;

public class UIPerkCheckbox : Toggle
{
	[SerializeField]
	private LoopAnimator anim;

	protected override void Awake()
	{
		base.Awake();
		anim = GetComponentInChildren<LoopAnimator>(includeInactive: true);
		Highlight(highlight: false);
	}

	public void Highlight(bool highlight)
	{
		if (!(anim == null))
		{
			anim.gameObject.SetActive(highlight);
			if (highlight)
			{
				anim.StartLoop();
			}
			else
			{
				anim.StopLoop();
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Highlight(highlight: false);
	}
}

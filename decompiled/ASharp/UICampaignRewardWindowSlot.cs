using ScenarioRuleLibrary.YML;
using UnityEngine;

public class UICampaignRewardWindowSlot : UICampaignReward
{
	[SerializeField]
	private GUIAnimator revealAnimator;

	[SerializeField]
	private LoopAnimator highlightAnimator;

	private void Awake()
	{
		revealAnimator.OnAnimationFinished.AddListener(() =>
		{
			highlightAnimator.StartLoop();
		});
	}

	public override void ShowReward(Reward reward)
	{
		ShowReward(reward, highlight: false);
	}

	public void ShowReward(Reward reward, bool highlight)
	{
		highlightAnimator.StopLoop(resetToInitial: true);
		highlightAnimator.gameObject.SetActive(highlight);
		revealAnimator.Stop();
		revealAnimator.GoInitState();
		base.ShowReward(reward);
	}

	public void Reveal()
	{
		revealAnimator.Play(fromStart: true);
	}

	public override void StopAnimations()
	{
		base.StopAnimations();
		revealAnimator.Stop();
		highlightAnimator.StopLoop();
	}

	public void Hide()
	{
		StopAnimations();
		revealAnimator.GoInitState();
		glowAnimator.GoInitState();
	}
}

using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "UI Events/Actor Events Channel", fileName = "UIActorEventsChannel")]
public class UIActorEventsChannel : ScriptableObject
{
	[NonSerialized]
	public UnityEvent<object, CActor, bool, bool> OnActorHighlighted;

	[NonSerialized]
	public UnityEvent<object, CActor, bool, bool> OnActorFocused;

	public bool CanShowHighlight
	{
		get
		{
			if (Singleton<InputFHManager>.Instance != null && !Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				if (PhaseManager.PhaseType != CPhase.PhaseType.MonsterClassesSelectAbilityCards)
				{
					return GameState.ThreadSleepReason != "ShowingNextMonsterCards";
				}
				return false;
			}
			if (Singleton<TakeDamagePanel>.Instance != null && Singleton<TakeDamagePanel>.Instance.IsOpen)
			{
				return false;
			}
			if (!PhaseManager.IsInActionSelectionPhase && !PhaseManager.IsInCardsSelectionPhase)
			{
				return false;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (OnActorHighlighted == null)
		{
			OnActorHighlighted = new UnityEvent<object, CActor, bool, bool>();
		}
		if (OnActorFocused == null)
		{
			OnActorFocused = new UnityEvent<object, CActor, bool, bool>();
		}
	}

	private void OnDisable()
	{
		if (OnActorHighlighted != null)
		{
			OnActorHighlighted.RemoveAllListeners();
		}
		if (OnActorFocused != null)
		{
			OnActorFocused.RemoveAllListeners();
		}
	}

	public void ActorHighlighted(object sender, CActor actor, bool highlighted, bool showCards)
	{
		if (!highlighted || CanShowHighlight)
		{
			OnActorHighlighted.Invoke(sender, actor, highlighted, showCards);
		}
	}

	public void ActorFocused(object sender, CActor actor, bool focused, bool showCards)
	{
		if (!focused || CanShowHighlight)
		{
			OnActorFocused.Invoke(sender, actor, focused, showCards);
		}
	}
}

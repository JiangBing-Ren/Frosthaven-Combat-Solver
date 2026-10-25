using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;

public class InitiativeTrackPlayerAvatarExtension : InitiativeTrackActorAvatarExtension
{
	[Header("Player Avatar Extension")]
	[SerializeField]
	private MultiplayerVisualsUIDataSO m_MultiplayerVisualsSO;

	public override InitiativeTrack.ActorPhaseState GetPhaseState(CActor actor)
	{
		CPlayerActor cPlayerActor = (CPlayerActor)actor;
		if (cPlayerActor != null && (cPlayerActor.IsLongRestSelected || cPlayerActor.IsLongRestActionSelected || cPlayerActor.CharacterClass.ShortRest))
		{
			return InitiativeTrack.ActorPhaseState.Resting;
		}
		if (PhaseManager.IsInCardsSelectionPhase)
		{
			return InitiativeTrack.ActorPhaseState.CardSelection;
		}
		return InitiativeTrack.ActorPhaseState.None;
	}

	public override void PostSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		Base.OnRefreshAbilities();
		CPlayerActor cPlayerActor = (CPlayerActor)aaParams.Actor;
		if (cPlayerActor != null)
		{
			int id = (FFSNetwork.IsOnline ? (FFSNetwork.GetControllingPlayer(cPlayerActor)?.PlayerID.ID ?? 0) : 0);
			MultiplayerVisualsUIDataSO.PlayerVisuals playerVisuals = m_MultiplayerVisualsSO.GetPlayerVisuals(id);
			if ((bool)m_FrameImage)
			{
				m_FrameImage.sprite = playerVisuals.InitiativeSlotFrame;
				m_FrameImage.enabled = FFSNetwork.IsOnline;
			}
			m_InnerFrameImage.sprite = playerVisuals.InitiativeSlotInnerFrame;
		}
	}

	public override string GetInitiativeText(CActor actor)
	{
		CPlayerActor cPlayerActor = (CPlayerActor)actor;
		if (!PhaseManager.IsInCardsSelectionPhase)
		{
			return base.GetInitiativeText(actor);
		}
		if (cPlayerActor.IsLongRestSelected)
		{
			return base.GetInitiativeText(actor);
		}
		if (cPlayerActor.CharacterClass.ShortRest)
		{
			return base.GetInitiativeText(actor);
		}
		if (actor.IsUnderMyControl())
		{
			return base.GetInitiativeText(actor);
		}
		int maxRoundCardsCount = cPlayerActor.MaxRoundCardsCount;
		int count = cPlayerActor.CharacterClass.RoundAbilityCards.Count;
		return $"{count}/{maxRoundCardsCount}";
	}

	public override void OnClick(CActor actor, bool isSelected)
	{
		if (isSelected || PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			Singleton<CardsHandManager>.Instance.ToggleViewHeroCards(actor as CPlayerActor);
		}
	}

	public override void OnSelect(CActor actor)
	{
		if (actor != null && PhaseManager.IsInCardsSelectionPhase && (Choreographer.s_Choreographer.LastMessage == null || (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.StartTurn && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.ActionSelectionPhaseStart)) && (Singleton<CardsHandManager>.Instance.IsShown || Singleton<CardsHandManager>.Instance.CurrentHand?.PlayerActor != actor) && !Singleton<AbilityEffectManager>.Instance.IsControlAbilityAffectingActor(actor))
		{
			Singleton<CardsHandManager>.Instance.SwitchHand((CPlayerActor)actor);
		}
	}
}

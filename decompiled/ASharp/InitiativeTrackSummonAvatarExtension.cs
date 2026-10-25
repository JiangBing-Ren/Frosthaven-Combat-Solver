using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrackSummonAvatarExtension : InitiativeTrackActorAvatarExtension
{
	[Header("Summon Avatar Extension")]
	[SerializeField]
	private Image m_SummonElementImage;

	[SerializeField]
	private MultiplayerVisualsUIDataSO m_MultiplayerVisualsSO;

	public override InitiativeTrack.ActorPhaseState GetPhaseState(CActor actor)
	{
		return InitiativeTrack.ActorPhaseState.None;
	}

	public override void PostSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		if (aaParams.Actor != null)
		{
			CPlayerActor summoner = ((CHeroSummonActor)aaParams.Actor).Summoner;
			int id = (FFSNetwork.IsOnline ? (FFSNetwork.GetControllingPlayer(summoner)?.PlayerID.ID ?? 0) : 0);
			MultiplayerVisualsUIDataSO.PlayerVisuals playerVisuals = m_MultiplayerVisualsSO.GetPlayerVisuals(id);
			m_FrameImage.sprite = playerVisuals.InitiativeSlotFrame;
			m_FrameImage.enabled = FFSNetwork.IsOnline;
			m_InnerFrameImage.sprite = playerVisuals.InitiativeSlotInnerFrame;
			m_SummonElementImage.sprite = playerVisuals.InitiativeSlotSummonElement;
		}
	}

	public override void OnClick(CActor actor, bool isSelected)
	{
	}

	public override void OnSelect(CActor actor)
	{
	}

	public override string GetInitiativeText(CActor actor)
	{
		if (!PhaseManager.IsInCardsSelectionPhase)
		{
			return base.GetInitiativeText(actor);
		}
		if (!(actor is CHeroSummonActor { Summoner: var summoner } cHeroSummonActor))
		{
			Log.GUIError($"{actor.ID} is using summon avatar but is not a summon!");
			return base.GetInitiativeText(actor);
		}
		if (summoner.IsDead)
		{
			return base.GetInitiativeText(actor);
		}
		if (FFSNetwork.IsOnline && !summoner.IsUnderMyControl())
		{
			return "?";
		}
		return base.GetInitiativeText(cHeroSummonActor);
	}
}

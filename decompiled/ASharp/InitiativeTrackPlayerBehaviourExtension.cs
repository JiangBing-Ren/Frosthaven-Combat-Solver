using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Animations;

public class InitiativeTrackPlayerBehaviourExtension : InitiativeTrackActorBehaviourExtension
{
	[SerializeField]
	private Transform cardsHighlightHolder;

	[SerializeField]
	private MultiplayerUserState controllerPlayerInfo;

	[SerializeField]
	private GUIAnimator warningAnimation;

	private bool _isAnchorInit;

	public override void OnSelect()
	{
	}

	public override void OnDeselect()
	{
	}

	public override void OnAvatarHighlight(bool active)
	{
		if (Singleton<CardsHandManager>.Instance == null)
		{
			return;
		}
		CPlayerActor cPlayerActor = (CPlayerActor)Base.Actor;
		if ((!active || !Base.IsSelected || PhaseManager.PhaseType == CPhase.PhaseType.EndRound || !Singleton<CardsHandManager>.Instance.IsShowingPlayerHand(cPlayerActor)) && (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor == null || !cPlayerActor.Equals(Choreographer.s_Choreographer.m_CurrentActor)))
		{
			if (active && !PhaseManager.IsInCardsSelectionPhase && !Singleton<CardsHandManager>.Instance.IsShowingPlayerHand(cPlayerActor))
			{
				Singleton<CardsHandManager>.Instance.Preview(cPlayerActor, cardsHighlightHolder);
			}
			else
			{
				Singleton<CardsHandManager>.Instance.StopPreview();
			}
		}
	}

	public override void OnSetAttributesDirect(InitiativeTrack.ActorAttributesParams aaParams)
	{
		OnSetAttributes();
		TryInitCardAnchor();
	}

	private void TryInitCardAnchor()
	{
		if (_isAnchorInit)
		{
			return;
		}
		if (cardsHighlightHolder == null || !cardsHighlightHolder.TryGetComponent<PositionConstraint>(out var component))
		{
			_isAnchorInit = true;
			return;
		}
		ActorStatPanel instanceFast = Singleton<ActorStatPanel>.InstanceFast;
		if (!(instanceFast == null))
		{
			if (instanceFast.PlayerCardsAnchor == null)
			{
				_isAnchorInit = true;
				return;
			}
			component.SetSource(0, new ConstraintSource
			{
				sourceTransform = instanceFast.PlayerCardsAnchor,
				weight = 1f
			});
			component.weight = 1f;
			component.constraintActive = true;
			_isAnchorInit = true;
		}
	}

	private void OnSetAttributes()
	{
		RefreshPlayerController();
		ShowWarning(show: false);
	}

	public void ShowWarning(bool show)
	{
		if (show)
		{
			warningAnimation.Play();
			return;
		}
		warningAnimation.Stop();
		warningAnimation.GoInitState();
	}

	public void RefreshPlayerController()
	{
		if (FFSNetwork.IsOnline)
		{
			NetworkPlayer networkPlayer = ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(Base.Actor.Class.ID))?.Controller;
			if (networkPlayer != null)
			{
				controllerPlayerInfo.Show(networkPlayer, PlayerRegistry.AllPlayers.Contains(networkPlayer));
			}
			else
			{
				controllerPlayerInfo.Hide();
			}
		}
		else
		{
			controllerPlayerInfo.Hide();
		}
	}
}

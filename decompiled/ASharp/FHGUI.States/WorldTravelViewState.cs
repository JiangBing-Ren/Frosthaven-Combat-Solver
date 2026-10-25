using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class WorldTravelViewState : FHMapViewState
{
	private const string SYNC_COMPLETE = "WORLD_TRAVEL_COMPLETE";

	public Action OnDone;

	private string _sourceId;

	private string _destinationId;

	private float _startAt;

	private float _endAt;

	private bool _playPath;

	private OutpostEventDTO _eventDto;

	private bool _readyCheckPassed;

	private bool _hasBeenShown;

	public override StateCategory Category => StateCategory.WorldMap;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Base;

	[Preserve]
	private WorldMapModule WorldMap => View.Modules.WorldMap.WorldMap;

	[Preserve]
	private InputActionHintsModule InputActionHints => View.Modules.InputActionHints;

	public bool IsTravelling
	{
		get
		{
			if (!View.OutpostSceneController.WorldMapScene.IsTravelling)
			{
				return !_readyCheckPassed;
			}
			return true;
		}
	}

	public WorldTravelViewState(float startAt, float endAt, OutpostEventDTO eventDto, bool playPath = true)
	{
		_sourceId = View.GameState.PrevPartyMarkerLocation;
		_destinationId = View.GameState.PartyMarkerLocation;
		_startAt = startAt;
		_endAt = endAt;
		_playPath = playPath;
		_eventDto = eventDto;
		_hasBeenShown = false;
	}

	public override void OnShow()
	{
		if (_hasBeenShown)
		{
			return;
		}
		_hasBeenShown = true;
		View.GetService<InputActionHintsService>().Setup(InputActionHints, this);
		View.OutpostSceneController.OpenWorldMap();
		if (_playPath)
		{
			CampaignState.PartyTravelStateType partyTravelState = GameState.PartyTravelState;
			if (partyTravelState == CampaignState.PartyTravelStateType.TravelingToOutpost || partyTravelState == CampaignState.PartyTravelStateType.TravelingToScenarioAfterEncounter || partyTravelState == CampaignState.PartyTravelStateType.TravelingToSpecialEncounter)
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance?.questMovementStartAudioItem);
			}
			View.OutpostSceneController.WorldMapScene.BeginPartyTravel(new FHPartyTravel.TravelParams
			{
				sourceId = _sourceId,
				destinationId = _destinationId,
				startAt = _startAt,
				endAt = _endAt,
				partySize = 2,
				Event = _eventDto,
				OnTravelComplete = OnFinish
			});
		}
	}

	private void OnFinish()
	{
		SyncAndDone();
	}

	private void OnSyncAndDoneComplete()
	{
		_readyCheckPassed = true;
		switch (GameState.PartyTravelState)
		{
		case CampaignState.PartyTravelStateType.TravelingToScenario:
			View.TryUpdateTravel(_destinationId, (_eventDto != null) ? CampaignState.PartyTravelStateType.WaitingEncounter : CampaignState.PartyTravelStateType.WaitingInScenario);
			break;
		case CampaignState.PartyTravelStateType.TravelingToScenarioAfterEncounter:
			View.TryUpdateTravel(_destinationId, CampaignState.PartyTravelStateType.WaitingInScenario);
			break;
		case CampaignState.PartyTravelStateType.TravelingToSpecialEncounter:
			View.TryUpdateTravel(_destinationId, CampaignState.PartyTravelStateType.WaitingEncounter);
			break;
		case CampaignState.PartyTravelStateType.TravelingToOutpost:
			View.TryUpdateTravel(_destinationId, CampaignState.PartyTravelStateType.WaitingInOutpost);
			break;
		}
		View.OutpostSceneController.WorldMapScene.StopPartyTravel();
		OnDone?.Invoke();
	}

	public override void OnHide()
	{
		View.OutpostSceneController.Reset();
	}

	private void SyncAndDone()
	{
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("WORLD_TRAVEL_COMPLETE", OnSyncAndDoneComplete);
	}
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FFSNet;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.States;
using FHRL;
using ScenarioRuleLibrary;

namespace FHGUI.Services;

public class ScenarioViewService : FHMapViewService, IReadyCheckProcessor
{
	private const string SCENARIO_READY_CHECK_COMMAND_PREFIX = "SCENARIO=";

	public readonly ScenarioInfoDTO ReturnToOutpostDTO = new ScenarioInfoDTO
	{
		Id = -1,
		Icon = UIInfoTools.Instance.GetQuestChainIcon(ScenarioChainType.None),
		NameKey = "GUI_ReturnToOutpost",
		DescriptionKey = "GUI_ReturnToOutpost_Desc",
		IsCompleted = false,
		IsPlayable = true
	};

	public bool HasPlayerJoining => PlayerRegistry.OtherPlayersAreJoining();

	public ScenarioViewService(FHMapView view)
		: base(view)
	{
	}

	private string GetScenarioVotingCommandID(int scenarioId)
	{
		return string.Format("{0}{1}", "SCENARIO=", scenarioId);
	}

	private ScenarioDef TryGetVotingCommandScenarioDef(string readyCheckCommandId, out int scenarioId)
	{
		if (!int.TryParse(readyCheckCommandId.Replace("SCENARIO=", ""), out scenarioId))
		{
			throw new FHMultiplayerException("Tried to get scenario from ready check command but the id [" + readyCheckCommandId + "] was incompatible.");
		}
		if (scenarioId < 0)
		{
			return null;
		}
		ScenarioID key = new ScenarioID((uint)scenarioId);
		return _view.Choreographer.DefRepo.Scenarios[key];
	}

	private bool StartScenarioReadyCheck(int scenarioId)
	{
		if (!_view.IsOnline)
		{
			return false;
		}
		if (_view.IsReadyCheckRunning)
		{
			return false;
		}
		string scenarioVotingCommandID = GetScenarioVotingCommandID(scenarioId);
		if (_view.MultiplayerGameState.ReadyCheckID == scenarioVotingCommandID)
		{
			return true;
		}
		if (!_view.IsPartyLeader)
		{
			return false;
		}
		Choreographer.ExecuteCommand("StartReadyCheck", _view.PlayerID, scenarioVotingCommandID);
		return true;
	}

	public void CancelScenarioReadyCheck()
	{
		if (!_view.MultiplayerGameState.ReadyCheckID.IsNullOrEmpty() && _view.IsPartyLeader)
		{
			Choreographer.ExecuteCommand("CancelReadyCheck", _view.PlayerID);
		}
	}

	private void TogglePlayerReadyForScenario(int scenarioId)
	{
		string scenarioVotingCommandID = GetScenarioVotingCommandID(scenarioId);
		string readyCheckID = _view.MultiplayerGameState.ReadyCheckID;
		if (readyCheckID != scenarioVotingCommandID)
		{
			throw new FHMultiplayerException($"Player {_view.PlayerID} tried to set ready for scenario [{scenarioVotingCommandID}] " + "but ReadyCheckID is for [" + readyCheckID + "]");
		}
		bool flag = !_view.Player.Ready;
		Choreographer.ExecuteCommand("SetReady", _view.PlayerID, flag);
	}

	private void StartScenario(ScenarioDef scenarioDef)
	{
		if (_view.IsPartyLeader && _view.GameState.IsInPrologue)
		{
			_view.GetService<PartyViewService>().ConfirmPrologueParty();
		}
		_view.Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.StartScenario, scenarioDef);
	}

	public void SetupSelectedScenario()
	{
		_view.ReplaceState(new BattleGoalSelectionViewState(isFromSummary: false));
	}

	public void ContinueTravelToScenario()
	{
		FHMapView view = _view;
		uint iD = _view.GameState.SelectedScenario.Def.ID.ID;
		view.TryUpdateTravel(iD.ToString(), CampaignState.PartyTravelStateType.TravelingToScenarioAfterEncounter);
	}

	public bool IsApplicableReadyCheckCommand(string readyCheckID)
	{
		return readyCheckID.StartsWith("SCENARIO=");
	}

	public bool ProcessReadyCheckResult(MultiplayerGameController.Result readyCheckResult)
	{
		if (!IsApplicableReadyCheckCommand(readyCheckResult.ReadyCheckID))
		{
			return false;
		}
		ScenarioDef scenarioDef = TryGetVotingCommandScenarioDef(readyCheckResult.ReadyCheckID, out var scenarioId);
		_view.SetSelectedScenario(scenarioId);
		if (!_view.IsPartyLeader)
		{
			return true;
		}
		if (readyCheckResult.Action != MultiplayerGameController.Action.AllPlayersReady)
		{
			return true;
		}
		if (scenarioDef == null)
		{
			ReturnToOutpost();
		}
		else
		{
			StartScenario(scenarioDef);
		}
		return true;
	}

	public void PrepareScenario([AllowNull] ScenarioDef scenarioDef, out ButtonsBarParam continueParams)
	{
		if (scenarioDef == null)
		{
			continueParams = default;
			return;
		}
		bool flag = _view.GameController.ScenarioController.IsScenarioPlayable(scenarioDef);
		if (!flag && _view.GameState.IsInPrologue)
		{
			flag = _view.GameState.AllCharacters.Count >= _view.GameState.Settings.MinPartySize && !_view.GameState.AllCharacters.Any((CharacterState state) => state.PartyState == CharacterPartyStateType.CreationProcess && !state.CreationFlowPassed);
		}
		if (_view.IsSoloGame)
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_START",
				Callback = () =>
				{
					StartScenario(scenarioDef);
				},
				Interactable = flag
			};
			return;
		}
		int scenarioId = (int)scenarioDef.ID.ID;
		bool isReadyCheckRunning = _view.IsReadyCheckRunning;
		bool flag2 = ValidateScenarioPlayers();
		if ((_view.IsPartyLeader & isReadyCheckRunning) && (!flag || !flag2))
		{
			CancelScenarioReadyCheck();
		}
		if (!_view.IsPartyLeader)
		{
			bool flag3 = _view.GetService<PartyViewService>().GetPartySlotsData(null).Any((PartySlotDTO c) => c.MultiplayerUserData?.Id == _view.PlayerID.ID && c.Character != ECharacter.None);
			bool flag4 = flag3 && _view.Player.Ready;
			bool interactable = flag & flag3 & isReadyCheckRunning & flag2;
			continueParams = new ButtonsBarParam
			{
				LabelKey = (flag4 ? "GUI_CANCEL" : "GUI_READY"),
				Callback = () =>
				{
					TogglePlayerReadyForScenario((int)scenarioDef.ID.ID);
				},
				Interactable = interactable
			};
		}
		else if (!isReadyCheckRunning)
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_START",
				Callback = () =>
				{
					StartScenarioReadyCheck(scenarioId);
				},
				Interactable = (flag & flag2)
			};
		}
		else
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_CANCEL",
				Callback = CancelScenarioReadyCheck
			};
		}
	}

	public void PrepareReturnToOutpost(out ButtonsBarParam continueParams)
	{
		if (_view.IsSoloGame)
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_START",
				Callback = ReturnToOutpost,
				Interactable = true
			};
			return;
		}
		int scenarioId = ReturnToOutpostDTO.Id;
		bool isReadyCheckRunning = _view.IsReadyCheckRunning;
		bool flag = ValidateScenarioPlayers();
		if ((_view.IsPartyLeader & isReadyCheckRunning) && !flag)
		{
			CancelScenarioReadyCheck();
		}
		if (!_view.IsPartyLeader)
		{
			bool ready = _view.Player.Ready;
			bool interactable = isReadyCheckRunning & flag;
			continueParams = new ButtonsBarParam
			{
				LabelKey = (ready ? "GUI_CANCEL" : "GUI_READY"),
				Callback = () =>
				{
					TogglePlayerReadyForScenario(scenarioId);
				},
				Interactable = interactable
			};
		}
		else if (!isReadyCheckRunning)
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_START",
				Callback = () =>
				{
					StartScenarioReadyCheck(scenarioId);
				},
				Interactable = flag
			};
		}
		else
		{
			continueParams = new ButtonsBarParam
			{
				LabelKey = "GUI_CANCEL",
				Callback = CancelScenarioReadyCheck
			};
		}
	}

	private void ReturnToOutpost()
	{
		_view.TryUpdateTravel(_view.GameState.GameMode.OutpostMarkerLocation, CampaignState.PartyTravelStateType.TravelingToOutpost);
	}

	public bool ValidateScenarioPlayers()
	{
		if (!FFSNetwork.IsOnline)
		{
			return true;
		}
		if (HasPlayerJoining)
		{
			return false;
		}
		return !PlayerRegistry.AllPlayers.Exists((NetworkPlayer x) => !x.IsParticipant && x.PlayerReadyForAssignment);
	}

	public bool ValidateParty()
	{
		if (!_view.GameState.IsInPrologue)
		{
			return _view.GameController.CharacterController.IsValidParty(throwException: false);
		}
		if (_view.GameState.AllCharacters.Count >= _view.GameState.Settings.MinPartySize)
		{
			return !_view.GameState.AllCharacters.Any((CharacterState state) => state.PartyState == CharacterPartyStateType.CreationProcess && !state.CreationFlowPassed);
		}
		return false;
	}

	public void ProcessStartScenarioCommand(CampaignController.Result result)
	{
		if (_view.GameState.IsInPrologue && !(_view.CurrentState is ProloguePartyCreationViewState) && _view.GameState.PartyCharacters.Count() < GameController.Settings.MinPartySize)
		{
			_view.ReplaceState(new ProloguePartyCreationViewState());
		}
		else
		{
			if (_view.GameState.GameMode.CanPlayWithoutCharacters || _view.GameState.ActiveScenario == null || _view.GameState.ActiveScenario.Def.NoIntroOutro)
			{
				return;
			}
			FHRL.ScenarioState selectedScenario = _view.GameState.SelectedScenario;
			uint iD;
			if (_view.GameState.IsInPrologue && string.IsNullOrEmpty(_view.GameState.PartyMarkerLocation))
			{
				FHMapView view = _view;
				iD = selectedScenario.ScenarioID.ID;
				view.TryUpdateTravel(iD.ToString(), CampaignState.PartyTravelStateType.WaitingInScenario);
				SetupSelectedScenario();
				return;
			}
			string partyMarkerLocation = _view.GameState.PartyMarkerLocation;
			iD = selectedScenario.ScenarioID.ID;
			if ((!(partyMarkerLocation == iD.ToString()) || _view.GameState.PartyTravelState != CampaignState.PartyTravelStateType.TravelingToScenarioAfterEncounter) && (result == null || !result.HasEvent<CampaignEvent.CampaignSectionTextActivated>()) && !(_view.CurrentState is RetirementStoryViewState))
			{
				FHMapView view2 = _view;
				iD = selectedScenario.ScenarioID.ID;
				view2.TryUpdateTravel(iD.ToString(), CampaignState.PartyTravelStateType.TravelingToScenario);
			}
		}
	}

	public IEnumerable<string> GetProceedWarning()
	{
		if (!ValidateScenarioPlayers())
		{
			yield return "GUI_MULTIPLAYER_WARNING_TEXT_WaitingForCharAssignment";
		}
		if (!ValidateParty())
		{
			yield return "GUI_PARTY_CREATE_TWO_CHARACTERS";
		}
	}

	public List<FHRL.ScenarioState> GetGetVisibleScenarios()
	{
		if (_view.GameState.IsPartyTravelling || _view.GameState.ActiveScenario != null)
		{
			return GameController?.ScenarioController?.GetUnlockedOrCompletedScenarios()?.Where((FHRL.ScenarioState state) => GetVisibleScenarios().Contains(state.ScenarioID.ID)).ToList();
		}
		return GameController?.ScenarioController?.GetUnlockedOrCompletedScenarios().ToList();
		IEnumerable<uint> GetVisibleScenarios()
		{
			if (uint.TryParse(_view.GameState.PrevPartyMarkerLocation, out var result))
			{
				yield return result;
			}
			if (uint.TryParse(_view.GameState.PartyMarkerLocation, out var result2))
			{
				yield return result2;
			}
		}
	}
}

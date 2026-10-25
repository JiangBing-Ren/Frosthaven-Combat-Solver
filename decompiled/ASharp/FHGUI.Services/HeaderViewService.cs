using System;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.States;
using FHRL;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Services;

public class HeaderViewService : FHMapViewService
{
	[Flags]
	public enum HeaderFunctionality
	{
		None = 0,
		OpenCharacterInfo = 1,
		OpenOutpostMap = 2,
		All = -1
	}

	public class HeaderParams
	{
		public Action OnTownClicked;

		public Action OnPortraitClicked;

		public Action OnCloseClicked;

		public Action OnSettingsClicked;

		public Action OnWikiClicked;

		public Action OnEventLogClicked;

		public bool IsStandalone;
	}

	private readonly CharacterDetailsViewService _characterDetailsService;

	private readonly UIWindow _escMenu;

	public HeaderViewService(FHMapView view, CharacterDetailsViewService characterDetailsService)
		: base(view)
	{
		_characterDetailsService = characterDetailsService;
		_escMenu = UnityEngine.Object.FindObjectOfType<UIMapEscMenu>()?.GetComponent<UIWindow>();
	}

	public void SetupHeader(MainHeaderModule module, HeaderFunctionality allowed = HeaderFunctionality.All, ConfirmActionDTO onConfirmActionClick = null)
	{
		_view.TryEnsureSelectedCharacter();
		MainHeaderDTO headerData = GetHeaderData(_view.SelectedCharacter);
		module.Init(headerData, allowed, GetHeaderParams(allowed, onConfirmActionClick));
	}

	private HeaderParams GetHeaderParams(HeaderFunctionality allowed = HeaderFunctionality.All, ConfirmActionDTO onConfirmActionClick = null)
	{
		return new HeaderParams
		{
			OnPortraitClicked = InsertAction(onConfirmActionClick, allowed.HasFlag(HeaderFunctionality.OpenCharacterInfo) ? new Action(OpenCharacterInfoScreen) : null),
			OnTownClicked = InsertAction(onConfirmActionClick, allowed.HasFlag(HeaderFunctionality.OpenOutpostMap) ? new Action(OpenOutpostMap) : null),
			OnCloseClicked = null,
			IsStandalone = _view.Choreographer.IsStandaloneScene,
			OnSettingsClicked = () =>
			{
				_escMenu?.Show();
			},
			OnWikiClicked = () =>
			{
				Singleton<FHUICodex>.InstanceFast?.Toggle();
			},
			OnEventLogClicked = () =>
			{
				_view.ReplaceState(new EventLogViewState());
			}
		};
		static Action InsertAction(ConfirmActionDTO confirmAction, Action defaultAction)
		{
			if (confirmAction == null)
			{
				return defaultAction;
			}
			return () =>
			{
				confirmAction.Execute(defaultAction, null);
			};
		}
	}

	private MainHeaderDTO GetHeaderData(CharacterDataDTO hero)
	{
		return new MainHeaderDTO
		{
			OutpostResources = _view.GameState.Outpost.Resources,
			Prosperity = new RangedValueDTO((int)_view.GameState.Outpost.Prosperity, (int)_view.GameController.OutpostController.ProsperityForNextLevel),
			ProsperityLevel = (int)_view.GameState.Outpost.ProsperityLevel,
			Morale = new RangedValueDTO(_view.GameState.Outpost.Morale, (int)_view.GameController.OutpostController.MaxMorale),
			Inspiration = (int)_view.GameState.Inspiration,
			TotalDefense = _view.GameController.AttackController.BaseDefense,
			Soldiers = new RangedValueDTO((int)_view.GameState.Outpost.Soldiers, (int)_view.GameController.OutpostController.SoldierCapacity),
			SelectedHero = hero,
			Week = _view.GameState.Outpost.Week,
			Season = _view.GameState.Outpost.Calendar.Season,
			Calendar = _view.GetService<OutpostCalendarViewService>().GetCalendarData(),
			HasNewCodexEntry = (Singleton<FHUICodex>.InstanceFast?.HasUnseenTopics() ?? false)
		};
	}

	private MainHeaderDTO GetHeaderData(CharacterState selectedHero)
	{
		return new MainHeaderDTO
		{
			OutpostResources = _view.GameState.Outpost.Resources,
			Prosperity = new RangedValueDTO((int)_view.GameState.Outpost.Prosperity, (int)_view.GameController.OutpostController.ProsperityForNextLevel),
			ProsperityLevel = (int)_view.GameState.Outpost.ProsperityLevel,
			Morale = new RangedValueDTO(_view.GameState.Outpost.Morale, (int)_view.GameController.OutpostController.MaxMorale),
			Inspiration = (int)_view.GameState.Inspiration,
			TotalDefense = _view.GameController.AttackController.BaseDefense,
			Soldiers = new RangedValueDTO((int)_view.GameState.Outpost.Soldiers, (int)_view.GameController.OutpostController.SoldierCapacity),
			SelectedHero = ((selectedHero != null) ? _characterDetailsService.PopulateCharacterDTO(selectedHero) : null),
			Week = _view.GameState.Outpost.Week,
			Season = _view.GameState.Outpost.Calendar.Season,
			Calendar = _view.GetService<OutpostCalendarViewService>().GetCalendarData(),
			HasNewCodexEntry = (Singleton<FHUICodex>.InstanceFast?.HasUnseenTopics() ?? false)
		};
	}

	public bool SetupCharacterDetailsHeader(MainHeaderModule mainHeader, SimpleHeaderModule simpleHeader, string simpleHeaderTitle, HeaderFunctionality allowed = HeaderFunctionality.All, bool forceSimpleHeader = false, ConfirmActionDTO onConfirmActionClick = null)
	{
		bool flag = _view.GameState.ActiveScenario != null;
		int num;
		if (!forceSimpleHeader && !_view.GameState.IsInPrologue)
		{
			CharacterState selectedCharacter = _view.SelectedCharacter;
			if (selectedCharacter == null || selectedCharacter.PartyState != CharacterPartyStateType.InParty)
			{
				CharacterState selectedCharacter2 = _view.SelectedCharacter;
				num = ((selectedCharacter2 == null || selectedCharacter2.PartyState != CharacterPartyStateType.RetireProcess) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag2 = (byte)num != 0;
		if (flag)
		{
			mainHeader.gameObject.SetActive(value: false);
			simpleHeader.gameObject.SetActive(value: false);
			return true;
		}
		if (flag2)
		{
			mainHeader.gameObject.SetActive(value: false);
			simpleHeader.gameObject.SetActive(value: true);
			simpleHeader.SetTitleNotLocalized(simpleHeaderTitle);
			simpleHeader.SetFunctionality(CharacterDetailsScreenModule.Functionality.None);
			return true;
		}
		mainHeader.gameObject.SetActive(value: true);
		simpleHeader.gameObject.SetActive(value: false);
		SetupHeader(mainHeader, allowed, onConfirmActionClick);
		return false;
	}

	private void OpenCharacterInfoScreen()
	{
		if (_view.SelectedCharacter != null)
		{
			_view.ReplaceState(new EditCharacterViewState(_view.SelectedCharacter.WasImported && _view.GameState.IsInPrologue));
		}
	}

	private void OpenOutpostMap()
	{
		if (!(_view.CurrentState is OutpostVillageMapState))
		{
			_view.ReplaceState(new OutpostVillageMapState());
		}
	}
}

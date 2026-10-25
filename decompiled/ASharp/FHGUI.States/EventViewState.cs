using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using I2.Loc;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class EventViewState : FHMapViewState
{
	public class Data
	{
		public OutpostEventState Event;

		public IEnumerable<CampaignEffect> Effects;

		public string Text;

		public string TextKey;

		public List<string> VoiceOvers;
	}

	private const string SYNC_EVENT_COMPLETE = "EVENT_COMPLETE";

	private OutpostEventState _event;

	private Data _data;

	[CanBeNull]
	public Action OnDone;

	[Preserve]
	private OutpostEventModule EventModalModule => GetModule<OutpostEventModule>();

	[Preserve]
	private MainHeaderModule HeaderModule => View.Modules.MainHeaderModule;

	[Preserve]
	private OutpostQuickAccessBarModule QuickAccessBar => View.Modules.QuickAccessBar;

	[Preserve]
	private PartyColumnModule PartyModule => View.Modules.Characters.PartyColumn;

	[Preserve]
	private OutpostVillageMapModule VillageMapModule => GetModule<OutpostVillageMapModule>();

	private OutpostEventsService EventsService => View.GetService<OutpostEventsService>();

	private HeaderViewService HeaderService => View.GetService<HeaderViewService>();

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PartyViewService PartyViewService => View.GetService<PartyViewService>();

	private BuildingViewService BuildingViewService => View.GetService<BuildingViewService>();

	public OutpostEventState EventState => _event;

	public EventViewState(OutpostEventState ev)
	{
		_event = ev;
	}

	public EventViewState(CampaignEvent.OutpostEventResolved data)
	{
		string text = NarrationSetting.FromOutpostEventOutcome(data.Event).Text;
		string text2 = NarrationSetting.FromCustomEventOutcome(data.Event);
		string text3 = (string.IsNullOrEmpty(text2) ? LocalizationManager.GetTermTranslation("FH/" + text) : text2);
		_data = new Data
		{
			Event = data.Event,
			Effects = data.Event.AppliedEffectsFromOutcome,
			Text = text3,
			TextKey = text,
			VoiceOvers = data.Event.CustomOutcomeLocKeys?.Select((OutpostEventState.CustomOutcomeKey x) => x.locKey)?.ToList()
		};
	}

	public EventViewState(OutpostEventState ev, string key, string text, IEnumerable<CampaignEffect> effects)
	{
		_data = new Data
		{
			Event = ev,
			Effects = effects,
			Text = text,
			TextKey = key
		};
	}

	public override void OnShow()
	{
		OutpostEventDef outpostEventDef = _event?.Def ?? _data?.Event?.Def;
		HeaderService.SetupHeader(HeaderModule, HeaderViewService.HeaderFunctionality.None);
		QuickAccessBarService.Setup((outpostEventDef.Type == OutpostEventType.Outpost) ? MainViewMainCategories.Outpost : MainViewMainCategories.WorldMap);
		if (outpostEventDef.Type == OutpostEventType.Outpost)
		{
			View.OutpostSceneController.OpenOutpostMap();
			PartyViewService.SetupPartyColumn(PartyModule);
		}
		else
		{
			View.OutpostSceneController.OpenWorldMap();
			PartyModule.gameObject.SetActive(value: false);
		}
		if (_event != null)
		{
			EventModalModule.Init(EventsService.GetEventDTO(_event));
			EventModalModule.OnOptionSelected = OnOptionSelected;
		}
		else
		{
			EventModalModule.Init(EventsService.GetEventDTO(_data));
			EventModalModule.OnDone = SyncAndDone;
		}
	}

	private void SyncAndDone()
	{
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("EVENT_COMPLETE", OnFinished);
	}

	private void OnFinished()
	{
		PopThisState();
		OnDone?.Invoke();
	}

	public override void OnHide()
	{
		View.OutpostSceneController.Reset();
		base.OnHide();
	}

	private void OnOptionSelected(int index)
	{
		if (!View.IsPartyLeader)
		{
			if (EventsService.IsOptionSuggested(index))
			{
				EventsService.DeSuggestOption(index);
			}
			else
			{
				EventsService.SuggestOption(index);
			}
			return;
		}
		PopThisState();
		Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveCurrentEvent, index);
		OnDone?.Invoke();
		if (!_event.Completed)
		{
			EventModalModule.OnDone = () =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ContinueResolveCurrentEvent);
				PopThisState();
				OnDone?.Invoke();
			};
			EventModalModule.OnOptionSelected = null;
		}
	}
}

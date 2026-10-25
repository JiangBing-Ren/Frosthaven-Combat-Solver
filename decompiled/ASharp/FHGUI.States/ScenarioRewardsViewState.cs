using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ScenarioRewardsViewState : FHMapViewState
{
	private const string SYNC_EVENT_KEY = "ScenarioRewards";

	private IEnumerable<CampaignEffect> _effects;

	private uint _scenarioID;

	public Action OnResult;

	private bool _readyCheckPassed;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private ScenarioRewardsModule ScenarioRewards => View.Modules.WorldMap.ScenarioRewards;

	private OutpostEventsService EventsService => View.GetService<OutpostEventsService>();

	public ScenarioRewardsViewState(uint scenarioID, IEnumerable<CampaignEffect> effects)
	{
		_effects = effects;
		_scenarioID = scenarioID;
	}

	public override void OnShow()
	{
		if (!IsValid)
		{
			OnValidFail?.Invoke();
			return;
		}
		List<CampaignEffect> list = View.GetService<OutpostEventsService>().CombineCampaignEffects(_effects);
		List<OutpostEventDTO.Effect> effects = list.Select((CampaignEffect e) => EventsService.GetFormattedEffect(e)).ToList();
		ScenarioRewardsModule.Params obj = new ScenarioRewardsModule.Params
		{
			ScenarioNameKey = $"FS_{_scenarioID}_NAME",
			Effects = effects,
			ReadyUsers = View.TryGetReadyUsersList(),
			ReadyCheckPassed = _readyCheckPassed
		};
		ScenarioRewards.Show(obj);
		ScenarioRewards.OnDone = SyncAndDone;
		UIContextualTutorialModule.OnEvent("ScenarioCompleted", list);
	}

	private void SyncAndDone()
	{
		_readyCheckPassed = true;
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("ScenarioRewards", OnFinished);
	}

	private void OnFinished()
	{
		PopThisState();
		OnResult?.Invoke();
	}
}

using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class LinkingSectionViewState : FHMapViewState
{
	private const string SYNC_COMPLETE = "LINKING_SECTION_COMPLETE";

	[CanBeNull]
	public Action OnDone;

	private readonly CampaignSectionDef _sectionDef;

	private readonly CampaignUnresolvedAction _unresolvedAction;

	private readonly string _overrideTextKey;

	private readonly IList<CampaignEffect> _appliedEffects;

	private bool _readyCheckPassed;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private LinkingSectionModuleModule EventModalModule => GetModule<LinkingSectionModuleModule>();

	private OutpostEventsService EventsService => View.GetService<OutpostEventsService>();

	public LinkingSectionViewState(CampaignUnresolvedAction unresolvedAction)
	{
		_unresolvedAction = unresolvedAction;
	}

	public LinkingSectionViewState(CampaignSectionDef section, string overrideTextKey)
	{
		_sectionDef = section;
		_overrideTextKey = overrideTextKey;
	}

	public LinkingSectionViewState(CampaignSectionDef section, IList<CampaignEffect> appliedEffects)
	{
		_sectionDef = section;
		_appliedEffects = appliedEffects;
	}

	public override void OnShow()
	{
		LinkingSectionDTO linkingSectionDTO = ((_unresolvedAction.Type == CampaignInteractionType.None) ? EventsService.GetLinkingSectionDTO(_sectionDef, _appliedEffects, _overrideTextKey) : EventsService.FromCampaignUnresolvedAction(_unresolvedAction));
		linkingSectionDTO.IsProceedInteractable &= !_readyCheckPassed;
		EventModalModule.Init(linkingSectionDTO);
		EventModalModule.OnOptionSelected = OnOptionSelected;
		EventModalModule.OnDone = SyncAndDone;
	}

	private void SyncAndDone()
	{
		_readyCheckPassed = true;
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("LINKING_SECTION_COMPLETE", OnFinished);
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

	private void OnOptionSelected(int index, OutpostEventDTO.Option option)
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
		}
		else
		{
			PopThisState();
			OnDone?.Invoke();
			option?.Action?.Invoke();
		}
	}
}

using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using FHRL;
using UnityEngine;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ViewCampaignSectionViewState : FHMapViewState
{
	private readonly LinkingSectionDTO _section;

	private readonly CampaignUnresolvedAction _unresolvedAction;

	private readonly IList<LinkingSectionDTO> _linkedSections;

	private int _linkedSectionIndex = -1;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private ViewSectionModule EventModalModule => GetModule<ViewSectionModule>();

	private OutpostEventsService EventsService => View.GetService<OutpostEventsService>();

	private bool CanProceed
	{
		get
		{
			if (_linkedSections != null)
			{
				return _linkedSectionIndex + 1 < _linkedSections.Count;
			}
			return false;
		}
	}

	private bool CanGoBack
	{
		get
		{
			if (_linkedSections != null)
			{
				return _linkedSectionIndex >= 0;
			}
			return false;
		}
	}

	public ViewCampaignSectionViewState(LinkingSectionDTO section, IList<LinkingSectionDTO> linkedSections = null)
	{
		_section = section;
		_linkedSections = linkedSections;
	}

	public override void OnShow()
	{
		_linkedSectionIndex = -1;
		EventModalModule.Init(_section, OnFinished, CanProceed ? new Action(OnNextEvent) : null, CanGoBack ? new Action(OnPreviousEvent) : null);
	}

	private void ShowLinkedEvent(int index)
	{
		if (_linkedSections == null || index < 0 || index + 1 > _linkedSections.Count)
		{
			EventModalModule.Init(_section, OnFinished, CanProceed ? new Action(OnNextEvent) : null, CanGoBack ? new Action(OnPreviousEvent) : null);
			return;
		}
		LinkingSectionDTO dto = _linkedSections[index];
		_linkedSectionIndex = index;
		EventModalModule.Init(dto, OnFinished, CanProceed ? new Action(OnNextEvent) : null, CanGoBack ? new Action(OnPreviousEvent) : null);
	}

	private void OnPreviousEvent()
	{
		_linkedSectionIndex = ((_linkedSectionIndex - 1 < -1) ? (-1) : (_linkedSectionIndex - 1));
		if (_linkedSections == null)
		{
			OnFinished();
		}
		else
		{
			ShowLinkedEvent(_linkedSectionIndex);
		}
	}

	private void OnNextEvent()
	{
		_linkedSectionIndex = Mathf.Clamp(_linkedSectionIndex + 1, -1, _linkedSections.Count);
		if (_linkedSections == null || _linkedSectionIndex >= _linkedSections.Count)
		{
			OnFinished();
		}
		else
		{
			ShowLinkedEvent(_linkedSectionIndex);
		}
	}

	private void OnFinished()
	{
		PopThisState();
	}

	public override void OnHide()
	{
		_linkedSectionIndex = -1;
		View.OutpostSceneController.Reset();
		base.OnHide();
	}
}

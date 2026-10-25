using System;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class NarrationViewState : FHMapViewState
{
	private const string SYNC_EVENT_COMPLETE = "NARRATION_COMPLETE";

	private NarrationSetting _settings;

	private int _currentStep;

	private bool _complete;

	private bool _popStateOnDone;

	public Action OnDone;

	public override StateCategory Category => StateCategory.Narration;

	[Preserve]
	private NarrationModalModule NarrationModalModule => GetModule<NarrationModalModule>();

	public NarrationViewState(NarrationSetting settings, bool popStateOnDone = true)
	{
		_settings = settings;
		_popStateOnDone = popStateOnDone;
		Validate(settings);
		NarrationModalModule.OnStepChange = (int i) =>
		{
			_currentStep = i;
		};
	}

	public override void OnShow()
	{
		View.OutpostSceneController.Reset();
		NarrationModalModule.Params obj = new NarrationModalModule.Params
		{
			Narration = _settings,
			IsPartyLeader = View.IsPartyLeader,
			SuggestingUsers = View.TryGetReadyUsersList(),
			Step = _currentStep,
			IsOnline = View.IsOnline,
			Complete = _complete
		};
		NarrationModalModule.Init(obj);
		NarrationModalModule.OnDone = () =>
		{
			_complete = true;
			SyncAndDone();
		};
		if (!_settings.NotEmpty)
		{
			PopThisState();
			OnDone?.Invoke();
		}
	}

	private void Validate(NarrationSetting settings)
	{
		if (settings.Story.Count == 0)
		{
			settings.Story.Add(new NarrationSetting.Setting
			{
				Text = "Missing story data!"
			});
		}
	}

	private void OnFinished()
	{
		if (_popStateOnDone)
		{
			PopThisState();
		}
		OnDone?.Invoke();
	}

	private void SyncAndDone()
	{
		View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("NARRATION_COMPLETE", OnFinished);
	}
}

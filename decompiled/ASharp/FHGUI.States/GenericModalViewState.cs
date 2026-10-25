using System;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class GenericModalViewState : FHMapViewState
{
	private const string SYNC_EVENT_KEY = "GenricModal";

	private string Text;

	private string Title;

	private string ConfirmButton;

	private string CancelButton;

	public bool ButtonCancelEnabled = true;

	public bool BackgroundEnabled;

	public bool IsSyncPoint;

	public Action<bool> OnResult;

	private bool _readyCheckPassed;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private GenericModalViewModule GenericModalModule => View.Modules.GenericModals.Modal;

	public GenericModalViewState(string text, Action<bool> onResult, bool canCancel = true, bool isSyncPoint = false)
		: this(text, null, onResult, canCancel, isSyncPoint)
	{
	}

	public GenericModalViewState(string text, string title, Action<bool> onResult, bool canCancel = true, bool isSyncPoint = false)
		: this(text, title, null, onResult, canCancel, isSyncPoint)
	{
	}

	public GenericModalViewState(string text, string title, string confirmButton, Action<bool> onResult, bool canCancel = true, bool isSyncPoint = false)
		: this(text, title, confirmButton, null, onResult, canCancel, isSyncPoint)
	{
	}

	public GenericModalViewState(string text, string title, string confirmButton, string cancelButton, Action<bool> onResult, bool canCancel = true, bool isSyncPoint = false)
	{
		Text = text;
		Title = title;
		ConfirmButton = confirmButton;
		CancelButton = cancelButton;
		ButtonCancelEnabled = canCancel;
		IsSyncPoint = isSyncPoint;
		OnResult = onResult;
	}

	public override void OnShow()
	{
		if (!IsValid)
		{
			OnValidFail?.Invoke();
			return;
		}
		GenericModalModule.Show(new GenericModalViewModule.Params
		{
			TitleLocalised = Title,
			DescriptionLocalised = Text,
			ShowCancel = ButtonCancelEnabled,
			SuggestingUsers = (IsSyncPoint ? View.TryGetReadyUsersList() : null),
			ConfirmButton = ConfirmButton,
			CancelButton = CancelButton,
			OnResult = (bool r) =>
			{
				SyncAndDone(() =>
				{
					PopThisState();
					OnResult?.Invoke(r);
				});
			}
		});
	}

	private void SyncAndDone(Action OnDone)
	{
		if (IsSyncPoint)
		{
			_readyCheckPassed = true;
			View.GetService<MultiplayerSyncViewService>().SetMultiplayerSyncPoint("GenricModal", OnDone);
		}
		else
		{
			OnDone?.Invoke();
		}
	}
}

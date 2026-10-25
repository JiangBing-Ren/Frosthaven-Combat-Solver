using System;
using FHGUI.Common.Elements;
using FHGUI.Modules;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ResourcePickerModalViewState : FHMapViewState
{
	private UIPicker.UIPickerData Data;

	public Action<UIPicker.UIPickerResult> OnResult;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private ResourcePickerModalModule PickerModule => View.Modules.GenericModals.ResourcePickerModal;

	public ResourcePickerModalViewState(UIPicker.UIPickerData data, Action<UIPicker.UIPickerResult> onResult)
	{
		Data = data;
		OnResult = onResult;
	}

	public override void OnShow()
	{
		PickerModule.Init(Data, (UIPicker.UIPickerResult result) =>
		{
			OnResult(result);
			PopThisState();
			return true;
		}, () =>
		{
			OnResult?.Invoke(default);
			PopThisState();
		});
	}
}

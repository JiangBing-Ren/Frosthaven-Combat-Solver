using System.Linq;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInputTileSelectonSuppressor : MonoBehaviour, IScopeElement
{
	[InputControlSchemePicker]
	[NonReorderable]
	[Tooltip("Control schemes for suppression. Empty for all control schemes.\nUse the picker to avoid typos.")]
	public string[] ControlSchemesFilter;

	private void Update()
	{
		if (ShouldSuppress() && Singleton<WorldspacePicker>.InstanceFast != null)
		{
			Singleton<WorldspacePicker>.InstanceFast.ShouldSuppressSelectionThisFrame = true;
		}
	}

	private bool ShouldSuppress()
	{
		if (ControlSchemesFilter == null || ControlSchemesFilter.Length == 0)
		{
			return true;
		}
		InputControlScheme lastUsedInputControlScheme = Singleton<InputFHManager>.Instance.InputContext.GetLastUsedInputControlScheme();
		if (ControlSchemesFilter.Contains(lastUsedInputControlScheme.name))
		{
			return true;
		}
		return false;
	}
}

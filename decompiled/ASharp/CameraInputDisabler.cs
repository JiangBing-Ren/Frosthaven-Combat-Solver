using Libs.Utils.Unity;
using UnityEngine;

public class CameraInputDisabler : MonoBehaviour
{
	[EnumMask]
	public CameraController.ECameraInput DisableInput = (CameraController.ECameraInput)(-1);

	private void OnEnable()
	{
		if ((bool)CameraController.s_CameraController)
		{
			CameraController.s_CameraController.DisableCameraInput(DisableInput, this);
			CameraController.s_CameraController.ToggleStickUINavigation(this, enable: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)CameraController.s_CameraController)
		{
			CameraController.s_CameraController.DisableCameraInput(CameraController.ECameraInput.None, this);
			CameraController.s_CameraController.ToggleStickUINavigation(this, enable: false);
		}
	}
}

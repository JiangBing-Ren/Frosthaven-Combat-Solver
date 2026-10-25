using FHGUI.States;

public class UIMapEscMenu : ESCMenu
{
	protected override void OnShow()
	{
		base.OnShow();
		if (Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			CheckMultiplayerButtonState();
		}
	}

	private void CheckMultiplayerButtonState()
	{
		if ((bool)Singleton<FHMapChoreographer>.InstanceFast && Singleton<FHMapChoreographer>.InstanceFast != null)
		{
			RefreshMultiplayerButton();
		}
	}

	protected override bool CheckMultiplayerButton(out string tooltip)
	{
		if (!base.CheckMultiplayerButton(out tooltip))
		{
			return false;
		}
		if (Singleton<FHMapChoreographer>.InstanceFast != null && Singleton<FHMapChoreographer>.InstanceFast.View.CurrentState is LinkedScenarioChoiceViewState)
		{
			tooltip = null;
			return false;
		}
		return true;
	}
}

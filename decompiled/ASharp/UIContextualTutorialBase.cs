using FHGUI.Common;
using FHGUI.Scenario;

public class UIContextualTutorialBase : UIGeneralInfoBase, IUIGeneralInfo<ContextualData>, IUIGeneralInfo
{
	public ContextualData Data { get; protected set; }

	public override UIGeneralInfoNavigator Navigator => null;

	public void Close()
	{
		if (Singleton<FHScenarioViewController>.InstanceFast?.ContextualTutorial != null)
		{
			Singleton<FHScenarioViewController>.InstanceFast.ContextualTutorial.Close();
		}
		else if (Singleton<FHMapChoreographer>.InstanceFast?.View != null)
		{
			Singleton<FHMapChoreographer>.InstanceFast.View.Modules.ContextualTutorial.Close();
		}
	}

	public virtual bool Show(ContextualData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		Data = data;
		if (data.OnShown != null)
		{
			data.OnShown();
		}
		return true;
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		if (Data.OnClosed != null)
		{
			Data.OnClosed();
		}
	}
}

using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionOptions : MainOption
{
	[SerializeField]
	private RectTransform optionsParent;

	private Transform defaultOptionsParent;

	public override void Select()
	{
		base.Select();
		Singleton<PromotionDLCManager>.InstanceFast?.Hide();
		if (defaultOptionsParent == null || Singleton<UIOptionsWindow>.Instance.transform.parent != optionsParent)
		{
			defaultOptionsParent = Singleton<UIOptionsWindow>.Instance.transform.parent;
		}
		Singleton<UIOptionsWindow>.Instance.transform.SetParent(optionsParent);
		Singleton<UIOptionsWindow>.Instance.Show(Button.transform as RectTransform, Button.Deselect, showHideOption: true);
	}

	public override void Deselect()
	{
		Singleton<PromotionDLCManager>.InstanceFast?.Show();
		base.Deselect();
		Singleton<UIOptionsWindow>.Instance.Hide();
	}

	private void OnDisable()
	{
		if (defaultOptionsParent != null)
		{
			Singleton<UIOptionsWindow>.Instance.transform.SetParent(defaultOptionsParent);
			Log.GUIInfo("Return options to parent " + defaultOptionsParent.name + " " + Singleton<UIOptionsWindow>.Instance.transform.parent?.name);
		}
		defaultOptionsParent = null;
	}

	private void OnDestroy()
	{
		if (defaultOptionsParent != null)
		{
			Log.GUIInfo("Return options to parent before destroy " + defaultOptionsParent?.ToString() + name);
			Singleton<UIOptionsWindow>.Instance.transform.SetParent(defaultOptionsParent);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FHGUI.Testing;

public abstract class TestState
{
	protected TestingCharacterCreationFlowViewData Controller;

	public virtual IEnumerable<IUIViewModule> Modules => ExtractModuleReferencesFromThis();

	protected IEnumerable<IUIViewModule> ExtractModuleReferencesFromThis()
	{
		return from prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where prop.CanRead
			where typeof(IUIViewModule).IsAssignableFrom(prop.PropertyType)
			select (IUIViewModule)prop.GetValue(this);
	}

	public TestState(TestingCharacterCreationFlowViewData controller)
	{
		Controller = controller;
	}

	public virtual void OnShow()
	{
		Debug.LogWarning($"OnShow {GetType()}");
	}

	public virtual void OnHide()
	{
		Debug.LogWarning($"OnHide {GetType()}");
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FHGUI;

public abstract class FHViewStateBase
{
	public IEnumerable<IUIViewModule> UseModules()
	{
		return from prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where prop.CanRead
			where typeof(IUIViewModule).IsAssignableFrom(prop.PropertyType)
			select (IUIViewModule)prop.GetValue(this);
	}

	public virtual void OnShow()
	{
	}

	public virtual void OnHide()
	{
	}
}

using System;

namespace FHGUI.Common;

public interface IUIGeneralInfo
{
	Action OnHide { get; set; }

	UIGeneralInfoNavigator Navigator { get; }

	void Hide();
}
public interface IUIGeneralInfo<T> : IUIGeneralInfo where T : IGeneralInfoData
{
	bool Show(T data, params object[] parameters);
}

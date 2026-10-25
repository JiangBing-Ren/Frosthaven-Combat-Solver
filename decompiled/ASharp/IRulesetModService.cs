using System;
using System.Collections.Generic;
using Assets.Script.GUI.MainMenu.Modding;

public interface IRulesetModService
{
	List<IMod> GetMods();

	void ValidateMod(IMod modData, Action onValidatedCallback);

	void ViewModErrors(IMod modData);
}

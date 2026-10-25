using System;
using System.Collections.Generic;

namespace Assets.Script.GUI.MainMenu.Modding;

public interface IModdingService
{
	List<IMod> GetMods();

	IMod CreateMod(ModDataView modData);

	void UploadMod(IMod modData);

	void EditMod(IMod modData);

	void OpenSteam();

	void ValidateMod(IMod modData, Action onValidatedCallback);

	void ViewModErrors(IMod modData);

	void CreateDummyRuleset(IMod modData);
}

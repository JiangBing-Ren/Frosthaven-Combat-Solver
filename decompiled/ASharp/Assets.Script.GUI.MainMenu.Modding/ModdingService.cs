using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Script.GUI.MainMenu.Modding;

public class ModdingService : IModdingService, IRulesetModService
{
	public List<IMod> GetMods()
	{
		return null;
	}

	public IMod CreateMod(ModDataView modData)
	{
		return null;
	}

	public void UploadMod(IMod modData)
	{
	}

	public void EditMod(IMod modData)
	{
	}

	public void OpenSteam()
	{
		Application.OpenURL("steam://url/SteamWorkshopPage/780290");
	}

	public void ValidateMod(IMod mod, Action onValidatedCallback)
	{
	}

	public void ViewModErrors(IMod modData)
	{
		if (File.Exists(modData.LastValidationResultsFile))
		{
			Application.OpenURL(modData.LastValidationResultsFile);
		}
		else
		{
			Log.GUIError("File does not exist: " + modData.LastValidationResultsFile);
		}
	}

	public void CreateDummyRuleset(IMod modData)
	{
	}
}

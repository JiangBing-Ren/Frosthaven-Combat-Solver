using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class ImportFromPartyViewState : FHMapViewState
{
	public class ImportPartyParams
	{
		public List<ImportPartyDTO> partyData;

		public Action<string> onImport;

		public Action<CharacterDataDTO> onCharacterInspect;

		public Action onBack;
	}

	public override StateCategory Category => StateCategory.Roster;

	[Preserve]
	private ImportPartyModule ImportPartyModule => View.Modules.Characters.ImportParty;

	[Preserve]
	private CharacterDetailsScreenModule CharacterDetailsScreenModule => View.Modules.Characters.MainScreen;

	private ImportPartyService ImportService => View.GetService<ImportPartyService>();

	public override async void OnShow()
	{
		ImportPartyParams importPartyParams = new ImportPartyParams
		{
			onBack = OnBack,
			onImport = OnImport
		};
		ImportPartyParams importPartyParams2 = importPartyParams;
		importPartyParams2.partyData = await ImportService.GetAvailableParties();
		importPartyParams.onCharacterInspect = OnCharacterInspect;
		ImportPartyModule.Init(importPartyParams);
		CharacterDetailsScreenModule.ClearAll();
		CharacterDetailsScreenModule.SetFunctionality(CharacterDetailsScreenModule.Functionality.CreationProcess);
		View.OutpostSceneController.OpenDefault();
	}

	public override void OnHide()
	{
	}

	private void OnBack()
	{
		PopThisState();
	}

	private void OnImport(string partyId)
	{
		ImportService.ImportParty(partyId);
		PopThisState();
	}

	private void OnCharacterInspect(CharacterDataDTO character)
	{
		View.PushState(new EditCharacterViewState(character, readOnly: true, dontRefresh: true));
	}
}

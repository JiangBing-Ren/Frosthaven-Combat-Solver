using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICampaignAdventurePartyAssemblyWindow : UIAdventurePartyAssemblyWindow
{
	[Header("Delete")]
	[SerializeField]
	private ExtendedButton deleteButton;

	protected override void Awake()
	{
		base.Awake();
		deleteButton.onClick.AddListener(ConfirmDeleteCharacter);
	}

	public override void Show(RectTransform point, CMapCharacter defaultSelected = null, Action onHidden = null, bool canEdit = true, bool instant = false)
	{
		base.Show(point, defaultSelected, onHidden, canEdit, instant);
		deleteButton.interactable = canEdit && (!FFSNetwork.IsOnline || selectedCharacter == null || selectedCharacter.IsMapCharacterUnderMyControl);
		deleteButton.buttonText.color = (deleteButton.interactable ? UIInfoTools.Instance.warningColor : UIInfoTools.DefInstance.greyedOutTextColor);
		RefreshDisabledCharacterOptions();
	}

	private void ConfirmDeleteCharacter()
	{
		if ((!FFSNetwork.IsHost || ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || PlayerRegistry.JoiningPlayers.Count <= 0) && (!FFSNetwork.IsClient || ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || !PlayerRegistry.OtherClientsAreJoining))
		{
			DeleteCharacter(selectedCharacter);
		}
	}

	private void DeleteCharacter(CMapCharacter character, bool networkIfOnline = true)
	{
		if (FFSNetwork.IsOnline & networkIfOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(character);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.DeleteCharacter, ActionPhaseType.NONE, disableAutoReplication: false, 0, 0, 0, controllableID, supplementaryDataBoolean: false, null, new CampaignCharacterData(character.CharacterID, character.CharacterName));
			return;
		}
		bool executionFinished = false;
		if (RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline: false))
		{
			ControllableRegistry.DestroyControllable(ControllableRegistry.GetControllableID(character));
			roster.RemoveCharacterOption(character);
			party.DeleteCharacter(character);
			SaveData.Instance.ManualSaveCurrentAdventureData();
		}
	}

	public override bool RemoveCharacterFromParty(CMapCharacter character, ref bool executionFinished, bool networkIfOnline = true, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		bool result = base.RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline, save, isProxyCall, switchingPlayerID);
		if (IsVisible && !party.CheckCharacters.Contains(character))
		{
			roster.RemoveCharacterOption(character);
		}
		if (IsVisible)
		{
			RefreshDisabledCharacterOptions();
		}
		return result;
	}

	protected override void PreviewCharacterInfo(CMapCharacter character)
	{
		base.PreviewCharacterInfo(character);
		deleteButton.gameObject.SetActive(selectedCharacter == character);
	}

	public void RefreshDisabledCharacterOptions()
	{
		if (window.IsOpen)
		{
			List<string> usedCharacterModels = party.SelectedCharacters.Select((CMapCharacter it) => it.CharacterYMLData.ID).ToList();
			roster.DisableOptions(party.CheckCharacters.FindAll((CMapCharacter it) => (selectedCharacter == null || it.CharacterYMLData.ID != selectedCharacter.CharacterYMLData.ID) && usedCharacterModels.Contains(it.CharacterYMLData.ID)));
		}
	}

	protected override void ShowNoSelectedCharacter()
	{
		base.ShowNoSelectedCharacter();
		RefreshDisabledCharacterOptions();
	}

	public override void ShowSelectedCharacter(CMapCharacter character)
	{
		base.ShowSelectedCharacter(character);
		RefreshDisabledCharacterOptions();
	}

	public void ProxyDeleteCharacter(GameAction action)
	{
		string characterID = ControllableRegistry.GetCharacterIDByControllerID(action.SupplementaryDataIDMax, errorIfNotFound: false);
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == characterID);
		if (cMapCharacter != null)
		{
			DeleteCharacter(cMapCharacter, networkIfOnline: false);
		}
	}
}

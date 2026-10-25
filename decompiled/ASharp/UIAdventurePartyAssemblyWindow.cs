using System;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIAdventurePartyAssemblyWindow : MonoBehaviour
{
	[SerializeField]
	protected UIWindow window;

	[SerializeField]
	protected UIAdventurePartyAssemblyRoster roster;

	[SerializeField]
	protected UIAdventurePartyAssemblyCharacterDisplay characterDisplay;

	[SerializeField]
	private CanvasGroup warningPanel;

	[SerializeField]
	private TextMeshProUGUI warningText;

	protected CMapCharacter selectedCharacter;

	private Action<CMapCharacter, NewPartyCharacterUI> onSelected;

	private Action<CMapCharacter> onDeselected;

	protected CMapParty party;

	private Action onHidden;

	public static bool IsChangingCharacter;

	public bool IsVisible => window.IsVisible;

	public CMapCharacter SelectedCharacter
	{
		set
		{
			selectedCharacter = value;
		}
	}

	protected virtual void Awake()
	{
		window.onHidden.AddListener(OnHidden);
		roster.OnCharacterHover.AddListener(PreviewCharacterInfo);
		roster.OnCharacterUnhover.AddListener(FinishPreviewCharacterInfo);
		roster.OnCharacterSelected.AddListener((CMapCharacter character) =>
		{
			if (selectedCharacter == null || selectedCharacter != character)
			{
				bool executionFinished = false;
				SelectCharacter(NewPartyDisplayUI.PartyDisplay.SelectedUISlot, character, ref executionFinished);
			}
		});
		roster.OnCharacterDeselected.AddListener((CMapCharacter character) =>
		{
			Log.NetworkTrace("SelectedCharacter: " + selectedCharacter);
			if (selectedCharacter != null)
			{
				Log.NetworkTrace("Selected Char ID: " + selectedCharacter.CharacterID);
			}
			if (selectedCharacter == character)
			{
				bool executionFinished = false;
				RemoveCharacterFromParty(selectedCharacter, ref executionFinished);
			}
		});
	}

	public virtual void Initialize(CMapParty party, Action<CMapCharacter, NewPartyCharacterUI> onSelected, Action<CMapCharacter> onDeselected)
	{
		this.party = party;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		roster.Refresh(party.CheckCharacters, null);
	}

	public virtual void Show(RectTransform point, CMapCharacter defaultSelected = null, Action onHidden = null, bool canEdit = true, bool instant = false)
	{
		selectedCharacter = defaultSelected;
		this.onHidden = onHidden;
		HideInformation();
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.character_select);
		roster.Show(party.CheckCharacters, selectedCharacter, point, instant);
		if (defaultSelected != null)
		{
			ShowSelectedCharacter(defaultSelected);
		}
		else
		{
			ShowNoSelectedCharacter();
		}
		if (!canEdit)
		{
			roster.Lock(party.SelectedCharactersArray);
		}
		window.Show();
		ValidatePartyCharacter();
	}

	public bool SelectCharacter(NewPartyCharacterUI targetUISlot, CMapCharacter characterToSelect, ref bool executionFinished, NetworkPlayer initialController = null, bool networkActionIfOnline = true, bool isLocalExecution = true, bool isProxyCall = false, int switchingPlayerID = 0, bool notifySwitch = true)
	{
		if (FFSNetwork.IsHost)
		{
			Log.GUIInfo("Select Character action discarded as another player is already switching");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsOnline & networkActionIfOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterToSelect);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.SwitchCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, controllableID, 0, NewPartyDisplayUI.PartyDisplay.SelectedUISlotIndex, PlayerRegistry.MyPlayer.PlayerID.ID);
			executionFinished = true;
			IsChangingCharacter = true;
			DetermineRosterSlotInteractability();
			return true;
		}
		CMapCharacter data = targetUISlot.Data;
		IsChangingCharacter = true;
		if (party.SelectedCharacters.Contains(characterToSelect) && data != null && data.CharacterID != null && party.SelectedCharacters.Contains(data))
		{
			try
			{
				if (FFSNetwork.IsOnline && switchingPlayerID != 0 && PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID.ID != switchingPlayerID && (from se in s.MyControllables
					select se.ControllableObject into w
					where w is NewPartyCharacterUI { Data: var data2 } && data2?.CharacterID == characterToSelect.CharacterID
					select w).Count() > 0) != null)
				{
					executionFinished = true;
					Log.GUIInfo("Player not switched as the controllable is owned by another player");
					IsChangingCharacter = false;
					DetermineRosterSlotInteractability();
					return false;
				}
			}
			catch (InvalidOperationException ex)
			{
				Log.GUIException(ex, "More than one matching element was found in PlayerRegistery.AllPlayers for PlayerID: " + switchingPlayerID);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00049", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				return false;
			}
			int num = party.SelectedCharactersArray.IndexOf(data);
			int num2 = party.SelectedCharactersArray.IndexOf(characterToSelect);
			party.SelectedCharactersArray[num] = characterToSelect;
			party.SelectedCharactersArray[num2] = data;
		}
		else
		{
			party.RemoveNewUnlockedCharacter(characterToSelect.CharacterYMLData.ID);
			if (data != null && data.CharacterID != null)
			{
				int num3 = party.SelectedCharactersArray.IndexOf(data);
				party.SelectedCharactersArray[num3] = characterToSelect;
				party.AddSelectedCharacter(characterToSelect, num3);
				if (Singleton<UIReadyTrackerBar>.Instance != null)
				{
					Singleton<UIReadyTrackerBar>.Instance.RemoveTracker(data.CharacterID);
				}
			}
			else
			{
				int characterSlot = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot);
				if (characterSlot >= 0 && characterSlot <= 3)
				{
					party.SelectedCharactersArray[characterSlot] = characterToSelect;
					party.AddSelectedCharacter(characterToSelect, characterSlot);
				}
				else
				{
					Log.GUIError("Invalid slot index `" + characterSlot + "' sent for character assignment.");
				}
			}
		}
		if (FFSNetwork.IsHost & notifySwitch)
		{
			Log.GUIInfo("[IsSwitchingCharacter] True - Host SelectCharacter about to send ToggleIsSwitchingCharacter side action");
			int characterSlot2 = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot);
			Synchronizer.SendSideAction(GameActionType.ToggleIsSwitchingCharacter, null, canBeUnreliable: false, sendToHostOnly: false, 0, switchingPlayerID, characterSlot2, dataBool: true);
		}
		if (isLocalExecution)
		{
			if (data != null && data.CharacterID != null)
			{
				roster.ClearIfCharacterSelected(data);
			}
			selectedCharacter = characterToSelect;
			onSelected(characterToSelect, targetUISlot);
			ShowSelectedCharacter(characterToSelect);
		}
		else
		{
			NewPartyDisplayUI.PartyDisplay.AssignNewCharacterToUISlot(characterToSelect, targetUISlot, initialController);
			if (IsVisible)
			{
				if (targetUISlot == NewPartyDisplayUI.PartyDisplay.SelectedUISlot)
				{
					DeselectAllCharacters();
					selectedCharacter = characterToSelect;
					ShowSelectedCharacter(characterToSelect);
				}
				else
				{
					DetermineRosterSlotInteractability();
				}
			}
		}
		Singleton<APartyDisplayUI>.Instance?.UpdatePartyLevel();
		int characterSlot3 = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot);
		string text = "Selected Characters:";
		try
		{
			CMapCharacter[] selectedCharactersArray = party.SelectedCharactersArray;
			foreach (CMapCharacter cMapCharacter in selectedCharactersArray)
			{
				if (cMapCharacter != null)
				{
					text = text + " Character ID: " + cMapCharacter.CharacterID;
					text = text + " Character Name: " + cMapCharacter.CharacterName;
				}
			}
		}
		catch
		{
		}
		if (data == null)
		{
			Log.SimpleInfo("Party Slot: " + characterSlot3 + " Selected: " + characterToSelect.CharacterID + " Character Name: " + characterToSelect.CharacterID);
		}
		else
		{
			Log.SimpleInfo("Party Slot: " + characterSlot3 + " Switched Character: " + data.CharacterID + " Character Name: " + data.CharacterID + " for " + characterToSelect.CharacterID + " Character Name: " + characterToSelect.CharacterID);
		}
		Log.SimpleInfo(text);
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			SaveData.Instance.ManualSaveCurrentAdventureData();
		}
		executionFinished = true;
		IsChangingCharacter = false;
		DetermineRosterSlotInteractability();
		return true;
	}

	public virtual void ShowSelectedCharacter(CMapCharacter character)
	{
		roster.ShowCharacterSelected(character);
		PreviewCharacterInfo(character);
		ValidatePartyCharacter();
	}

	public void DeselectAllCharacters()
	{
		roster.DeselectAllCharacters();
	}

	public void DetermineRosterSlotInteractability()
	{
		roster.DetermineSlotInteractability();
	}

	protected virtual void PreviewCharacterInfo(CMapCharacter character)
	{
		characterDisplay.Display(character);
	}

	protected virtual void FinishPreviewCharacterInfo(CMapCharacter previeweCharacter)
	{
		if (window.IsOpen)
		{
			if (selectedCharacter != null && selectedCharacter.CharacterID != null)
			{
				PreviewCharacterInfo(selectedCharacter);
			}
			else
			{
				ShowNoSelectedCharacter();
			}
		}
	}

	public virtual bool RemoveCharacterFromParty(CMapCharacter character, ref bool executionFinished, bool networkIfOnline = true, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		if (character == null)
		{
			Log.NetworkWarning("Trying to remove a character but the character returns null.");
			executionFinished = true;
			return false;
		}
		if (!isProxyCall && !SceneController.Instance.RetiringCharacter && ((FFSNetwork.IsHost && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && PlayerRegistry.OtherClientsAreJoining)))
		{
			Log.GUIInfo("Remove Character action discarded as another player is joining the game");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsHost)
		{
			Log.GUIInfo("Remove Character action discarded as another player is already switching");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsOnline & networkIfOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(character);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.RemoveCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, controllableID, 0, 0, PlayerRegistry.MyPlayer.PlayerID.ID, supplementaryDataBoolean: false, null, new CampaignCharacterData(character.CharacterID, character.CharacterName));
			executionFinished = true;
			IsChangingCharacter = true;
			DetermineRosterSlotInteractability();
			return true;
		}
		IsChangingCharacter = true;
		if (FFSNetwork.IsHost)
		{
			Log.GUIInfo("[IsSwitchingCharacter] True - Host RemoveCharacterFromParty about to send ToggleIsSwitchingCharacter side action");
			Synchronizer.SendSideAction(GameActionType.ToggleIsSwitchingCharacter, null, canBeUnreliable: false, sendToHostOnly: false, 0, switchingPlayerID, 0, dataBool: true);
		}
		if (Singleton<UIReadyTrackerBar>.Instance != null)
		{
			Singleton<UIReadyTrackerBar>.Instance.RemoveTracker(character.CharacterID);
		}
		if (character == selectedCharacter)
		{
			selectedCharacter = null;
			roster.ClearIfCharacterSelected(character);
			ShowNoSelectedCharacter();
		}
		party.RemoveSelectedCharacter(character);
		onDeselected(character);
		if (Singleton<APartyDisplayUI>.Instance != null)
		{
			Singleton<APartyDisplayUI>.Instance.UpdatePartyLevel();
		}
		if (save)
		{
			SaveData.Instance.ManualSaveCurrentAdventureData();
		}
		string text = "Selected Characters:";
		try
		{
			CMapCharacter[] selectedCharactersArray = party.SelectedCharactersArray;
			foreach (CMapCharacter cMapCharacter in selectedCharactersArray)
			{
				if (cMapCharacter != null)
				{
					text = text + " Character ID: " + cMapCharacter.CharacterID;
					text = text + " Character Name: " + cMapCharacter.CharacterName;
				}
			}
		}
		catch
		{
		}
		Log.SimpleInfo("Removed Character: " + character.CharacterID + " Character Name: " + character.CharacterName);
		Log.SimpleInfo(text);
		executionFinished = true;
		IsChangingCharacter = false;
		DetermineRosterSlotInteractability();
		return true;
	}

	protected virtual void ShowNoSelectedCharacter()
	{
		characterDisplay.Hide();
		ValidatePartyCharacter();
	}

	private void ValidatePartyCharacter()
	{
		if (party.SelectedCharacters.Count() >= 2)
		{
			HideInformation();
			return;
		}
		int count = AdventureState.MapState.MapParty.CheckCharacters.Count;
		if (count < 2)
		{
			if (party.SelectedCharacters.Count() < count)
			{
				warningText.text = LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROE");
				warningPanel.alpha = 1f;
			}
			else
			{
				HideInformation();
			}
		}
		else
		{
			warningText.text = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROES"), 2);
			warningPanel.alpha = 1f;
		}
	}

	protected void HideInformation()
	{
		warningPanel.alpha = 0f;
	}

	public void Hide(bool instant = false, bool playHideAudio = true)
	{
		string audioItemHide = window.AudioItemHide;
		if (!playHideAudio)
		{
			window.AudioItemHide = null;
		}
		window.Hide(instant);
		window.AudioItemHide = audioItemHide;
	}

	protected virtual void OnHidden()
	{
		characterDisplay.Hide(instant: true);
		roster.Hide();
		onHidden?.Invoke();
	}
}

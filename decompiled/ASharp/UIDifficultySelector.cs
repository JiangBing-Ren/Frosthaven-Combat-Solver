using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input.UIScope;
using FFSNet;
using FFSNet.Backend;
using FHRL;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.YML.Locations;
using UnityEngine;
using UnityEngine.UI;

public class UIDifficultySelector : MonoBehaviour
{
	private enum ECommitChangesMode
	{
		OnSelectDifficulty,
		OnHide
	}

	private class DifficultyOption
	{
		private UIOptionEntry m_OptionUI;

		public CAdventureDifficulty Difficulty { get; private set; }

		public DifficultyOption(CAdventureDifficulty difficulty, UIOptionEntry optionUI)
		{
			Difficulty = difficulty;
			m_OptionUI = optionUI;
			optionUI.Init(difficulty.Text);
		}

		public void CheckSelected(EAdventureDifficulty selectedDifficulty)
		{
			m_OptionUI.Enable(Difficulty.ActiveOn.Contains(selectedDifficulty));
		}
	}

	public delegate void DifficultySelectedEventHandler(object sender, DifficultySelectedEventArgs eventArgs);

	public class DifficultySelectedEventArgs : EventArgs
	{
		public EAdventureDifficulty SelectedDifficulty { get; private set; }

		public DifficultySelectedEventArgs(EAdventureDifficulty selectedDifficulty)
		{
			SelectedDifficulty = selectedDifficulty;
		}
	}

	[SerializeField]
	private Toggle m_Story;

	[SerializeField]
	private Toggle m_Normal;

	[SerializeField]
	private Toggle m_Veteran;

	[SerializeField]
	private Toggle m_Hard;

	[SerializeField]
	private Toggle m_Brutal;

	[SerializeField]
	private Toggle m_Unfair;

	[SerializeField]
	private List<UIOptionEntry> m_DifficultyEntryPool;

	[SerializeField]
	private EGameMode m_GameMode;

	[SerializeField]
	private ECommitChangesMode commitChangesMode = ECommitChangesMode.OnHide;

	[SerializeField]
	private SelectionController m_SelectionController;

	private readonly List<DifficultyOption> m_DifficultyEntries = new List<DifficultyOption>();

	private readonly Dictionary<EAdventureDifficulty, UIDifficultyLevelCard> m_DifficultyCards = new Dictionary<EAdventureDifficulty, UIDifficultyLevelCard>();

	public EAdventureDifficulty SelectedDifficulty { get; private set; }

	public event DifficultySelectedEventHandler DifficultySelected;

	private void Awake()
	{
		m_Story.onValueChanged.AddListener(OnValueChanged_Story);
		m_Normal.onValueChanged.AddListener(OnValueChanged_Normal);
		m_Veteran.onValueChanged.AddListener(OnValueChanged_Veteran);
		m_Hard.onValueChanged.AddListener(OnValueChanged_Hard);
		m_Brutal.onValueChanged.AddListener(OnValueChanged_Brutal);
		m_Unfair.onValueChanged.AddListener(OnValueChanged_Unfair);
		tryRegisterDifficultyCard(EAdventureDifficulty.Story, m_Story);
		tryRegisterDifficultyCard(EAdventureDifficulty.Normal, m_Normal);
		tryRegisterDifficultyCard(EAdventureDifficulty.Veteran, m_Veteran);
		tryRegisterDifficultyCard(EAdventureDifficulty.Hard, m_Hard);
		tryRegisterDifficultyCard(EAdventureDifficulty.Brutal, m_Brutal);
		tryRegisterDifficultyCard(EAdventureDifficulty.Unfair, m_Unfair);
		void tryRegisterDifficultyCard(EAdventureDifficulty difficulty, Toggle toggle)
		{
			if (toggle.TryGetComponent<UIDifficultyLevelCard>(out var component))
			{
				m_DifficultyCards.Add(difficulty, component);
			}
		}
	}

	private IEnumerable<Toggle> GetAllToggles()
	{
		yield return m_Story;
		yield return m_Normal;
		yield return m_Veteran;
		yield return m_Hard;
		yield return m_Brutal;
		yield return m_Unfair;
	}

	public void ClearSelection()
	{
		bool allowSwitchOff = m_Story.group.allowSwitchOff;
		m_Story.group.allowSwitchOff = false;
		foreach (Toggle allToggle in GetAllToggles())
		{
			allToggle.SetIsOnWithoutNotify(value: false);
			if (allToggle.TryGetComponent<UIDifficultyLevelCard>(out var component))
			{
				component.OnDifficultySelected(isOn: false);
			}
		}
		m_Story.group.allowSwitchOff = allowSwitchOff;
	}

	private void OnEnable()
	{
		EGameMode gameMode = SaveData.Instance.Global.GameMode;
		bool flag = gameMode == EGameMode.Campaign;
		if ((m_GameMode == EGameMode.None) & flag)
		{
			m_GameMode = gameMode;
		}
		List<CAdventureDifficulty> list = MapRuleLibraryClient.MRLYML?.Difficulty?.SingleOrDefault((DifficultyYMLData s) => s.GameMode == m_GameMode.ToString())?.DifficultySettings?.ToList();
		if (list == null)
		{
			return;
		}
		list.RemoveAll((CAdventureDifficulty difficulty) => string.IsNullOrEmpty(difficulty.Text));
		DetermineToggleInteractability();
		if (FFSNetwork.IsHost)
		{
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(DetermineToggleInteractability));
		}
		m_DifficultyEntries.Clear();
		HelperTools.NormalizePool(ref m_DifficultyEntryPool, m_DifficultyEntryPool[0].gameObject, m_DifficultyEntryPool[0].transform.parent, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			m_DifficultyEntries.Add(new DifficultyOption(list[num], m_DifficultyEntryPool[num]));
		}
		foreach (KeyValuePair<EAdventureDifficulty, UIDifficultyLevelCard> pair in m_DifficultyCards)
		{
			List<string> list2 = (from option in m_DifficultyEntries
				where option.Difficulty.ActiveOn.Contains(pair.Key) && !string.IsNullOrEmpty(option.Difficulty.Text)
				select option.Difficulty.Text).ToList();
			if (list2.Count == 0)
			{
				list2 = new List<string> { "DIFFICULTY_NO_MODIFIER" };
			}
			pair.Value.SetModifierTooltip(list2);
		}
		EGameState currentGameState = SaveData.Instance.Global.CurrentGameState;
		bool flag2 = flag && (currentGameState == EGameState.Map || currentGameState == EGameState.Scenario);
		SelectedDifficulty = (flag2 ? AdventureState.MapState.DifficultySetting : EAdventureDifficulty.Normal);
		ClearSelection();
		ToggleDifficulty(SelectedDifficulty);
	}

	public void ToggleDifficulty(EAdventureDifficulty difficulty)
	{
		Toggle toggle = difficulty switch
		{
			EAdventureDifficulty.None => m_Story, 
			EAdventureDifficulty.Story => m_Story, 
			EAdventureDifficulty.Normal => m_Normal, 
			EAdventureDifficulty.Veteran => m_Veteran, 
			EAdventureDifficulty.Hard => m_Hard, 
			EAdventureDifficulty.Brutal => m_Brutal, 
			EAdventureDifficulty.Unfair => m_Unfair, 
			_ => throw new ArgumentOutOfRangeException("difficulty", difficulty, null), 
		};
		ClearSelection();
		toggle.SetIsOnWithoutNotify(value: true);
		toggle.Select();
		SetDifficulty(difficulty, networkAction: false);
		if (m_DifficultyCards.TryGetValue(difficulty, out var value))
		{
			value.OnDifficultySelected(isOn: true);
		}
	}

	public void RefreshSelectionController()
	{
		if (m_SelectionController != null && m_DifficultyCards.TryGetValue(SelectedDifficulty, out var value))
		{
			m_SelectionController.SetPersistantSelection(value.Selectable);
		}
	}

	private void OnDisable()
	{
		ClearSelection();
		if (FFSNetwork.IsHost)
		{
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(DetermineToggleInteractability));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(DetermineToggleInteractability));
			if (FFSNetwork.IsOnline && commitChangesMode == ECommitChangesMode.OnHide && CanChangeDifficulty())
			{
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.PickDifficulty, ActionProcessor.CurrentPhase, disableAutoReplication: false, 0, (int)SelectedDifficulty);
			}
		}
	}

	private bool CanChangeDifficulty()
	{
		if (AdventureState.MapState != null && AdventureState.MapState.DifficultySetting == SelectedDifficulty)
		{
			return false;
		}
		return true;
	}

	private void OnValueChanged_Story(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Story);
		}
	}

	private void OnValueChanged_Normal(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Normal);
		}
	}

	private void OnValueChanged_Veteran(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Veteran);
		}
	}

	private void OnValueChanged_Hard(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Hard);
		}
	}

	private void OnValueChanged_Brutal(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Brutal);
		}
	}

	private void OnValueChanged_Unfair(bool value)
	{
		if (value)
		{
			SetDifficulty(EAdventureDifficulty.Unfair);
		}
	}

	private void SetDifficulty(EAdventureDifficulty difficultySetting, bool networkAction = true)
	{
		SelectedDifficulty = difficultySetting;
		RefreshSelectionController();
		foreach (DifficultyOption difficultyEntry in m_DifficultyEntries)
		{
			difficultyEntry.CheckSelected(SelectedDifficulty);
		}
		if (CanChangeDifficulty())
		{
			if (!FFSNetwork.IsOnline)
			{
				DifficultySelected?.Invoke(this, new DifficultySelectedEventArgs(SelectedDifficulty));
			}
			else if ((FFSNetwork.IsHost & networkAction) && commitChangesMode == ECommitChangesMode.OnSelectDifficulty)
			{
				Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.PickDifficulty, ActionPhaseType.MapHQ, disableAutoReplication: false, 0, (int)SelectedDifficulty);
			}
		}
	}

	private void DetermineToggleInteractability()
	{
		bool flag = (SaveData.Instance.Global.CurrentGameState == EGameState.None || Singleton<FHMapChoreographer>.InstanceFast != null) && ((!FFSNetwork.IsHost) ? (!FFSNetwork.IsOnline) : (PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.ConnectingUsers.Count == 0));
		Toggle story = m_Story;
		Toggle normal = m_Normal;
		Toggle veteran = m_Veteran;
		Toggle hard = m_Hard;
		Toggle brutal = m_Brutal;
		bool flag2 = (m_Unfair.interactable = flag);
		bool flag4 = (brutal.interactable = flag2);
		bool flag6 = (hard.interactable = flag4);
		bool flag8 = (veteran.interactable = flag6);
		bool interactable = (normal.interactable = flag8);
		story.interactable = interactable;
		m_Story.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Story"));
		m_Normal.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Normal"));
		m_Veteran.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Veteran"));
		m_Hard.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Hard"));
		m_Brutal.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Brutal"));
		m_Unfair.gameObject.SetActive(CampaignSettings.ValidForBetaDifficulties.Contains("Unfair"));
	}

	public static async void ClientSetDifficulty(GameAction action)
	{
		EAdventureDifficulty supplementaryDataIDMin = (EAdventureDifficulty)action.SupplementaryDataIDMin;
		SaveData.Instance.Global.CurrentAdventureData.ChangeDifficulty(supplementaryDataIDMin);
		Log.NetworkInfo("Difficulty switched to " + supplementaryDataIDMin);
	}

	private void DetermineToggleInteractability(NetworkBackendPlayer connection)
	{
		DetermineToggleInteractability();
	}

	private void DetermineToggleInteractability(NetworkPlayer player)
	{
		DetermineToggleInteractability();
	}
}

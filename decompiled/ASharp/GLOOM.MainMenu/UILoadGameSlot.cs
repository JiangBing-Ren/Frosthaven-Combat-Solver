using System;
using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHRL;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UILoadGameSlot : MonoBehaviour
{
	public class GameSlotEvent : UnityEvent<IGameSaveData>
	{
	}

	public class GameDLCEvent : UnityEvent<IGameSaveData, DLCRegistry.EDLCKey>
	{
	}

	[Serializable]
	private class TextStateColor
	{
		public TextMeshProUGUI text;

		public Color defaultColor;

		[SerializeField]
		private Color invalidColor;

		public void SetValid(bool valid)
		{
			text.color = (valid ? defaultColor : invalidColor);
		}
	}

	[SerializeField]
	private FHButton button;

	[SerializeField]
	private TextMeshProUGUI partyName;

	[SerializeField]
	private UIMultiplayerSaveOwner saveOwner;

	[SerializeField]
	private TextMeshProUGUI date;

	[SerializeField]
	private string formatDate = "dd/MM/y, HH:mm";

	[SerializeField]
	private TextMeshProUGUI questName;

	[SerializeField]
	private UILoadGameCharacter[] characterSlots;

	[SerializeField]
	private TextMeshProUGUI location;

	[SerializeField]
	private TextMeshProUGUI week;

	[SerializeField]
	private TextMeshProUGUI difficulty;

	[SerializeField]
	private ExtendedDropdown dlcDropdown;

	[SerializeField]
	private List<UILoadGameOwnedDLC> ownedDLCSlot;

	[SerializeField]
	private Button loadButton;

	[SerializeField]
	private Button deleteButton;

	[SerializeField]
	private List<Graphic> validMasks;

	[SerializeField]
	private List<TextStateColor> validTexts;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	public GameSlotEvent OnLoadData = new GameSlotEvent();

	public GameSlotEvent OnDeleteData = new GameSlotEvent();

	public GameDLCEvent OnEnableDLC = new GameDLCEvent();

	private SelectorWrapper<DLCRegistry.EDLCKey> dlcSelector;

	private IGameSaveData data;

	private const string c_NewQuestString = "GUI_LOAD_ADVENTURE_START_QUEST";

	private const string c_DLCQuest = "GUI_LOAD_GAME_DLC_QUEST";

	private UnityAction onClickLoad;

	public event UnityAction OnHovered
	{
		add
		{
			button.PointerEntered += value;
		}
		remove
		{
			button.PointerEntered -= value;
		}
	}

	private void Awake()
	{
		loadButton.onClick.AddListener(() =>
		{
			onClickLoad?.Invoke();
		});
		deleteButton.onClick.AddListener(Delete);
		dlcSelector = new DLCSelector(dlcDropdown);
		dlcSelector.OnValuedChanged.AddListener(EnableDLC);
	}

	private void EnableDLC(DLCRegistry.EDLCKey dlc)
	{
		if (data != null)
		{
			dlcSelector.ResetSelection();
			OnEnableDLC.Invoke(data, dlc);
		}
	}

	private void Load()
	{
		if (data != null)
		{
			OnLoadData.Invoke(data);
		}
	}

	private void Delete()
	{
		if (data != null)
		{
			OnDeleteData.Invoke(data);
		}
	}

	public void SetData(IGameSaveData data)
	{
		this.data = data;
		partyName.text = data.GameName;
		date.text = data.LastSavedTimeStamp.ToString(formatDate);
		saveOwner.SetPlayer(data.Owner);
		bool flag = data.HasInvalidDLCs(out var invalidDLCs);
		for (int i = 0; i < validMasks.Count; i++)
		{
			validMasks[i].material = (flag ? UIInfoTools.Instance.greyedOutMaterial : null);
		}
		for (int j = 0; j < validTexts.Count; j++)
		{
			validTexts[j].SetValid(!flag);
		}
		loadButton.SetTextKey(flag ? "GUI_BUY_DLC" : "GUI_LOAD");
		if (flag)
		{
			tooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_LOAD_SAVE_MISSING_DLC"), LocalizationManager.GetTranslation(invalidDLCs[0].ToString())));
			tooltip.enabled = true;
			onClickLoad = () =>
			{
				PlatformLayer.Instance.NewPlatformLayer.OpenStore();
			};
		}
		else
		{
			tooltip.enabled = false;
			onClickLoad = Load;
		}
		SetCharacters(data.SelectedCharacters, invalidDLCs);
		string translation;
		if (string.IsNullOrEmpty(data.CurrentQuest))
		{
			questName.text = LocalizationManager.GetTranslation("GUI_LOAD_ADVENTURE_START_QUEST");
		}
		else if (invalidDLCs.Count == 0)
		{
			questName.text = LocalizationManager.GetTranslation(data.CurrentQuest);
		}
		else if (LocalizationManager.TryGetTranslation(data.CurrentQuest, out translation))
		{
			questName.text = translation;
		}
		else
		{
			questName.text = LocalizationManager.GetTranslation("GUI_LOAD_GAME_DLC_QUEST");
		}
		if ((bool)location)
		{
			if (data.ScenarioID.HasValue)
			{
				ScenarioDef value = YMLDefRepository.Repo.DefRepository.Scenarios.FirstOrDefault((KeyValuePair<ScenarioID, ScenarioDef> pair) =>
				{
					ScenarioID key = pair.Key;
					ScenarioID? scenarioID = data.ScenarioID;
					return key == scenarioID;
				}).Value;
				location.text = LocalizationManager.GetTranslation($"FS_{value.ID}_NAME");
				string translation2 = LocalizationManager.GetTranslation($"FS_{value.ID}_NAME");
				if (data.CurrentScenarioRound > 0)
				{
					location.text = string.Format("{0}, {1} {2}", translation2, LocalizationManager.GetTranslation("GUI_COMBATLOG_START_ROUND"), data.CurrentScenarioRound);
				}
				else
				{
					location.text = translation2;
				}
			}
			else
			{
				location.text = LocalizationManager.GetTranslation("GUI_Outpost");
			}
		}
		if (week != null)
		{
			string translation3 = LocalizationManager.GetTranslation((new Calendar((uint)data.Week).Season == CalendarSeason.Summer) ? "GUI_SUMMER" : "GUI_Winter");
			week.text = $"{data.Week}, {translation3}";
		}
		if (difficulty != null)
		{
			difficulty.text = LocalizationManager.GetTranslation("GUI_DIFFICULTY_" + data.PartyAdventureData.Difficulty.ToString().ToUpper());
		}
	}

	private void SetCharacters(List<IGameSaveCharacterData> characters, List<DLCRegistry.EDLCKey> invalidDLCs)
	{
		for (int i = 0; i < characterSlots.Length; i++)
		{
			if (i < characters.Count && characters[i] != null)
			{
				DLCRegistry.EDLCKey eDLCKey = characters[i].BelongsToDLC(invalidDLCs);
				if (eDLCKey != DLCRegistry.EDLCKey.None)
				{
					characterSlots[i].ShowMissing(eDLCKey);
				}
				else
				{
					characterSlots[i].ShowCharacter(characters[i].CharacterModel, characters[i].Level, characters[i].CharacterName, characters[i].Gold, invalidDLCs.IsNullOrEmpty());
				}
			}
			else
			{
				characterSlots[i].ShowEmpty();
			}
		}
	}
}

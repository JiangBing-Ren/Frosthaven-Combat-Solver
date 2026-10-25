using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FH.YML;
using FHRL;
using MapRuleLibrary.Adventure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameDataStep : UICreateGameStep
{
	[SerializeField]
	private UIKeyboard keyboard;

	[Header("Party Name")]
	[SerializeField]
	private TMP_InputField m_PartyNameInput;

	[SerializeField]
	private TextMeshProUGUI m_PartyNameWarning;

	[SerializeField]
	private HelpBoxLine m_PartyNameHelpBox;

	[SerializeField]
	private string m_RepeatedPartyNameErrorLocKey = "GUI_CAMPAIGN_REPEATED_NAME";

	[SerializeField]
	private string m_InvalidPartyNameErrorLocKey = "GUI_CAMPAIGN_INVALID_NAME";

	[Header("Difficulty")]
	[SerializeField]
	private UIDifficultySelector m_DifficultySelector;

	[Header("Confirm")]
	[SerializeField]
	private GUIAnimator m_IncompletedDataWarningAnimator;

	[SerializeField]
	private GameObject m_HighlightConfirmButton;

	[SerializeField]
	private GUIAnimator m_EnableConfirmAnimator;

	[SerializeField]
	private string m_EnableConfirmAudioItem;

	private Color m_DefaultInputColor;

	protected IGameModeService m_Service;

	private bool m_IsConfirmEnabled;

	protected override void InitCache()
	{
		if (!m_Initialized)
		{
			base.InitCache();
			m_DefaultInputColor = m_PartyNameInput.textComponent.color;
			m_Window.onHidden.AddListener(OnHidden);
			m_PartyNameInput.onValueChanged.AddListener((string newString) =>
			{
				PopulateName(newString);
			});
			TMP_InputField partyNameInput = m_PartyNameInput;
			partyNameInput.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(partyNameInput.onValidateInput, new TMP_InputField.OnValidateInput(HandleOnValidateInput));
		}
	}

	protected override async void Show(bool instant = false)
	{
		if (m_Data.HasParam("GameModeID"))
		{
			string campaignMode = m_Data.GetParam<string>("GameModeID");
			GameMode gameMode = YMLDefRepository.Repo?.GameModes?.Items?.SingleOrDefault((GameMode x) => x.Name == campaignMode);
			if (gameMode != null && gameMode.NoDifficulty)
			{
				string partyName = FixName(campaignMode);
				PopulateName(partyName, checkValid: false);
				m_DifficultySelector.ToggleDifficulty(EAdventureDifficulty.Normal);
				OnConfirmedStep();
				return;
			}
		}
		m_IncompletedDataWarningAnimator?.Stop();
		m_EnableConfirmAnimator?.Stop();
		base.Show(instant);
		if (m_PartyNameInput.text.IsNullOrEmpty())
		{
			await PopulateName(await GetUniquePartyName(m_Service));
			m_PartyNameInput.Select();
			m_PartyNameInput.ActivateInputField();
			m_PartyNameInput.MoveTextEnd(shift: true);
		}
	}

	protected override void Setup(IGameModeService service, GameData gameData, Action onConfirmed, Action onCancelled = null)
	{
		m_Service = service;
		base.Setup(service, gameData, onConfirmed, onCancelled);
		m_DifficultySelector.ToggleDifficulty(gameData.Difficulty);
		PopulateName(gameData.GameName);
	}

	private char HandleOnValidateInput(string text, int charIndex, char addedChar)
	{
		if (addedChar == ' ')
		{
			if (charIndex > 0 && text[charIndex - 1] == ' ')
			{
				return '\0';
			}
			if (text.Length > charIndex && text[charIndex] == ' ')
			{
				return '\0';
			}
		}
		if (UIHelperUtil.IsEmoji(addedChar) || addedChar == '\ufe0f')
		{
			return '\0';
		}
		if (GlobalData.UnsupportedCharacters.Contains(addedChar))
		{
			return '\0';
		}
		return addedChar;
	}

	private string FixName(string partyName)
	{
		if (!string.IsNullOrWhiteSpace(partyName))
		{
			partyName = partyName.ToTitleCase();
			foreach (char item in GlobalData.UnsupportedCharacters.Where((char value) => partyName.Contains(value) || UIHelperUtil.IsEmoji(value)))
			{
				partyName = partyName.RemoveChar(item);
			}
			partyName = Regex.Replace(partyName, " +", " ");
		}
		return partyName;
	}

	private async Task<bool> PopulateName(string partyName, bool checkValid = true)
	{
		FixName(partyName);
		bool isValid = true;
		if (!checkValid)
		{
			ClearWarning();
		}
		else if (string.IsNullOrWhiteSpace(partyName))
		{
			isValid = false;
			m_PartyNameHelpBox.Show(m_InvalidPartyNameErrorLocKey, null, null, HelpBox.FormatTarget.ALL);
			m_PartyNameHelpBox.HighlightWarning();
			m_IncompletedDataWarningAnimator?.Play();
			m_PartyNameInput.textComponent.color = m_DefaultInputColor;
		}
		else if (await m_Service.ValidateExistsGame(partyName))
		{
			isValid = false;
			m_PartyNameHelpBox.Show(m_RepeatedPartyNameErrorLocKey, null, null, HelpBox.FormatTarget.ALL);
			m_PartyNameInput.textComponent.color = UIInfoTools.Instance.warningColor;
			m_IncompletedDataWarningAnimator?.Play();
		}
		else
		{
			ClearWarning();
		}
		EnableConfirmationButton(isValid);
		m_PartyNameInput.SetTextWithoutNotify(partyName);
		return isValid;
	}

	protected override async Task<bool> Validate()
	{
		if (!(await base.Validate()))
		{
			return false;
		}
		if (!(await PopulateName(m_PartyNameInput.text)))
		{
			return false;
		}
		return true;
	}

	private void ClearWarning()
	{
		RestoreDefaultInputFieldColor();
		m_PartyNameHelpBox.Hide();
	}

	private void RestoreDefaultInputFieldColor()
	{
		m_PartyNameInput.textComponent.color = m_DefaultInputColor;
	}

	protected override void EnableConfirmationButton(bool enable)
	{
		if (!enable)
		{
			m_EnableConfirmAnimator?.Stop();
		}
		else if (!m_IsConfirmEnabled)
		{
			AudioControllerUtils.PlaySound(m_EnableConfirmAudioItem);
			m_EnableConfirmAnimator?.Play();
		}
		m_IsConfirmEnabled = enable;
		base.EnableConfirmationButton(enable);
		if (m_HighlightConfirmButton != null)
		{
			m_HighlightConfirmButton.SetActive(enable);
		}
	}

	private void OnHidden()
	{
		m_IncompletedDataWarningAnimator?.Stop();
		m_EnableConfirmAnimator?.Stop();
	}

	private void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			OnHidden();
		}
	}

	protected override void OnConfirmedStep()
	{
		BuildGameModel(m_Data);
		base.OnConfirmedStep();
	}

	protected virtual GameData BuildGameModel(GameData data)
	{
		data.GameName = m_PartyNameInput.text;
		data.Difficulty = m_DifficultySelector.SelectedDifficulty;
		return data;
	}

	protected static async Task<string> GetUniquePartyName(IGameModeService service)
	{
		string genericNameBase = "New Party";
		string result = genericNameBase;
		int i = 1;
		while (i < 10000 && await service.ValidateExistsGame(result))
		{
			result = $"{genericNameBase}#{i:D4}";
			int num = i + 1;
			i = num;
		}
		return result;
	}
}

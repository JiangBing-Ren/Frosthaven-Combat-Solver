using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class HelpBox : Singleton<HelpBox>
{
	public enum FormatTarget
	{
		NONE,
		TITLE,
		TEXT,
		ALL
	}

	public class HelpInfo
	{
		public string keyboardTitle;

		public string controllerTitle;

		public Func<string> controllerLines;

		public string keybaordLines;

		public readonly FormatTarget useWarningFormat;

		public HelpInfo(string titleKeyboard, string keybaordLine, string titleController = null, Func<string> controllerLine = null, FormatTarget useWarningFormat = FormatTarget.NONE)
		{
			controllerTitle = titleKeyboard;
			controllerLines = controllerLine;
			keybaordLines = keybaordLine;
			this.useWarningFormat = useWarningFormat;
			controllerTitle = titleController;
		}
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private HelpBoxLine _boxLinePrefab;

	[SerializeField]
	private List<HelpBoxLine> linesPool;

	private string currentId;

	private HelpInfo m_DefaultHelpInfo;

	private Dictionary<string, HelpInfo> m_AdditionalHelpInfos = new Dictionary<string, HelpInfo>();

	private CAbility m_Ability;

	private CBaseCard m_Card;

	private CAbility m_OverrideAbiliy;

	private CBaseCard m_OverrideCard;

	public bool IsShown => window.IsOpen;

	public bool ShowingAdditionalLines => m_AdditionalHelpInfos.Count > 0;

	public Func<string> ControllerText => m_DefaultHelpInfo?.controllerLines;

	public string ControllerTitle => m_DefaultHelpInfo?.controllerTitle;

	private void ShowGamepad()
	{
		m_OverrideAbiliy = null;
		m_OverrideCard = null;
		if (m_DefaultHelpInfo != null)
		{
			RefreshTranslated(controller: true, m_DefaultHelpInfo.useWarningFormat);
		}
	}

	private void ShowKeyboard()
	{
		m_OverrideAbiliy = null;
		m_OverrideCard = null;
		if (m_DefaultHelpInfo != null)
		{
			RefreshTranslated(controller: false, m_DefaultHelpInfo.useWarningFormat);
		}
	}

	public void ClearHelpInfo(bool clearAdditional = true)
	{
		m_DefaultHelpInfo = null;
		if (clearAdditional)
		{
			m_AdditionalHelpInfos.Clear();
		}
		if ((bool)Singleton<InputFHManager>.InstanceFast)
		{
			Singleton<InputFHManager>.InstanceFast.InputContext.LastUsedInputControlSchemeChanged -= OnControllerTypeChanged;
		}
	}

	public void ShowControllerOrKeyboardTip(string keyboardText, Func<string> controllerText, string titleKeyboard = null, string titleController = null, string id = null)
	{
		ClearHelpInfo();
		currentId = id;
		HelpInfo helpInfo = new HelpInfo(titleKeyboard, keyboardText, titleController, controllerText);
		if (id == null)
		{
			m_DefaultHelpInfo = helpInfo;
		}
		else
		{
			m_AdditionalHelpInfos[id] = helpInfo;
		}
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged -= OnControllerTypeChanged;
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged += OnControllerTypeChanged;
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			ShowGamepad();
		}
		else
		{
			ShowKeyboard();
		}
	}

	public void OverrideControllerOrKeyboardTip(Func<string> controllerText, string controllerTitle = null)
	{
		if (m_DefaultHelpInfo == null)
		{
			ShowControllerOrKeyboardTip(null, controllerText, null, controllerTitle);
			return;
		}
		m_DefaultHelpInfo.controllerLines = controllerText;
		m_DefaultHelpInfo.controllerTitle = controllerTitle;
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			ShowGamepad();
		}
	}

	public void OverrideKeyboardTip(string text, string title = null, string id = null)
	{
		if (m_DefaultHelpInfo != null)
		{
			currentId = id;
			m_DefaultHelpInfo.keyboardTitle = title;
			m_DefaultHelpInfo.keyboardTitle = text;
			if (window.IsOpen && !Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				ShowKeyboard();
			}
		}
	}

	public void ClearOverrideController()
	{
		if (m_DefaultHelpInfo != null)
		{
			m_DefaultHelpInfo.controllerLines = null;
			if (window.IsOpen && Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				ShowKeyboard();
			}
		}
	}

	private void OnControllerTypeChanged()
	{
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			ShowGamepad();
		}
		else
		{
			ShowKeyboard();
		}
	}

	public void Show(List<string> texts, string titleLocKey = null, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ClearHelpInfo();
		currentId = id;
		HelperTools.NormalizePool(ref linesPool, _boxLinePrefab.gameObject, transform, texts.Count);
		for (int i = 0; i < texts.Count; i++)
		{
			linesPool[i].Show(texts[i], titleLocKey, null, useWarningFormat);
		}
		window.Show();
	}

	public void Show(string textLoc, string titleLocKey, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ShowTranslated(GLOOM.LocalizationManager.GetTranslation(textLoc), titleLocKey.IsNullOrEmpty() ? null : GLOOM.LocalizationManager.GetTranslation(titleLocKey), useWarningFormat, id);
	}

	public void ShowDefault()
	{
		ShowTranslated((string)null, (string)null, FormatTarget.NONE, (string)null, true);
	}

	public void Show(string textLoc, CAbility ability, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		m_OverrideAbiliy = ability;
		Show(textLoc, (string)null, useWarningFormat, id);
	}

	public void Show(string textLoc, CBaseCard card, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		m_OverrideCard = card;
		Show(textLoc, (string)null, useWarningFormat, id);
	}

	public void Show(string textLoc, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		Show(textLoc, (string)null, useWarningFormat, id);
	}

	public void ShowTranslated(string text, string title, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null, bool clearOldInfo = true)
	{
		ClearHelpInfo(clearOldInfo);
		currentId = id;
		HelpInfo helpInfo = new HelpInfo(title, text, null, null, useWarningFormat);
		if (id == null)
		{
			m_DefaultHelpInfo = helpInfo;
		}
		else
		{
			m_AdditionalHelpInfos[id] = helpInfo;
		}
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged -= OnControllerTypeChanged;
		Singleton<InputFHManager>.Instance.InputContext.LastUsedInputControlSchemeChanged += OnControllerTypeChanged;
		RefreshTranslated(controller: false, useWarningFormat);
	}

	public void ShowTranslated(string text, CAbility ability, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null, bool clearOldInfo = true)
	{
		m_OverrideAbiliy = ability;
		ShowTranslated(text, (string)null, useWarningFormat, id, clearOldInfo);
	}

	public void ShowTranslated(string text, CBaseCard card, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		m_OverrideCard = card;
		ShowTranslated(text, (string)null, useWarningFormat, id, true);
	}

	public void ShowTranslated(string text, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ShowTranslated(text, (string)null, useWarningFormat, id, true);
	}

	public void OnReadyButtonShown(string customHelpBoxTextLocKey = null)
	{
		if (window.IsOpen && customHelpBoxTextLocKey == null)
		{
			return;
		}
		CAbility cAbility = (PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability;
		if (cAbility != null)
		{
			if (customHelpBoxTextLocKey != null)
			{
				ShowTranslated(GLOOM.LocalizationManager.GetTranslation(customHelpBoxTextLocKey), "");
			}
			else
			{
				ShowTranslated(null, cAbility);
			}
		}
	}

	private void ResolveAbility(ref string text, ref string title)
	{
		m_Ability = m_OverrideAbiliy;
		m_Card = m_OverrideCard;
		m_OverrideAbiliy = null;
		m_OverrideCard = null;
		if (m_Ability == null && m_Card == null)
		{
			m_Ability = (PhaseManager.CurrentPhase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability;
		}
		if (m_Card == null)
		{
			m_Card = m_Ability?.GetAbilityBaseCard();
		}
		CBaseCard card = m_Card;
		if (card != null && card.CardType == CBaseCard.ECardType.Dummy)
		{
			m_Card = m_Ability?.ParentAbilityBaseCard ?? m_Ability?.ParentAbility?.GetAbilityBaseCard() ?? m_Card;
		}
		if (m_Card == null)
		{
			m_Card = m_Ability?.ParentAbilityBaseCard;
		}
		if (m_Card != null && m_Card.CardType.In(CBaseCard.ECardType.Dummy, CBaseCard.ECardType.ScenarioModifier, CBaseCard.ECardType.AttackModifier))
		{
			m_Card = null;
		}
		if (m_Ability == null || !m_Ability.AllTargets || !(m_Ability is CAbilityCreateLimitedHazardousTerrain { m_State: CAbilityCreateLimitedHazardousTerrain.CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions }))
		{
			ResolveTitle(ref title);
			ResolveText(ref text);
		}
	}

	private void ResolveTitle(ref string title)
	{
		if (title != null)
		{
			return;
		}
		string text = GetCardNameKey();
		if (string.IsNullOrEmpty(text) && PhaseManager.PhaseType == CPhase.PhaseType.InfuserMechanicSelection)
		{
			text = GameState.PendingInfusionPhase?.TriggerActor?.CharacterClass?.LocKey;
		}
		if (string.IsNullOrEmpty(text) && PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			text = "GUI_TOOLTIP_TITLE_START_TURN";
		}
		if (string.IsNullOrEmpty(text) && GameState.s_ShortRestingPlayer != null)
		{
			text = GameState.s_ShortRestingPlayer.ActorLocKey();
		}
		if (!string.IsNullOrEmpty(text))
		{
			title = GLOOM.LocalizationManager.GetTranslation(text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: false, useDefaultIfMissing: false, returnNullIfNotFound: true);
		}
		if (title == null)
		{
			return;
		}
		if (m_Ability != null)
		{
			string translation = GLOOM.LocalizationManager.GetTranslation(m_Ability.AbilityType.ToString(), fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: false, useDefaultIfMissing: false, returnNullIfNotFound: true);
			if (translation != null)
			{
				string text2 = string.Empty;
				AddConditionsToText(ref text2, out var isCondition);
				if (isCondition)
				{
					title = title + " (" + text2 + ")";
				}
				else
				{
					title = title + " (" + translation + text2 + ")";
				}
			}
		}
		CCharacterClass cCharacterClass = null;
		CBaseCard card = m_Card;
		CAbilityCard abilityCard = card as CAbilityCard;
		if (abilityCard != null)
		{
			cCharacterClass = CharacterClassManager.Classes.Find((CCharacterClass s) => s.CharacterID == abilityCard.ClassID);
		}
		else if (m_Ability?.TargetingActor != null)
		{
			cCharacterClass = m_Ability.TargetingActor.CharacterClass;
		}
		else if (GameState.s_ShortRestingPlayer != null && text == GameState.s_ShortRestingPlayer.ActorLocKey())
		{
			cCharacterClass = GameState.s_ShortRestingPlayer.CharacterClass;
		}
		if (cCharacterClass != null)
		{
			ECharacter characterModel = cCharacterClass.CharacterModel;
			title = "<color=#" + UIInfoTools.Instance.GetCharacterColor(characterModel).ToHex() + ">" + title + "</color>";
		}
	}

	private void AddConditionsToText(ref string text, out bool isCondition)
	{
		isCondition = false;
		if (m_Ability == null)
		{
			return;
		}
		CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditionFromAbility(m_Ability.AbilityType);
		if (eNegativeCondition != CCondition.ENegativeCondition.NA)
		{
			text = text + "<sprite name=" + eNegativeCondition.ToString() + ">";
			isCondition = true;
		}
		CCondition.EPositiveCondition ePositiveCondition = CCondition.PositiveConditionFromAbility(m_Ability.AbilityType);
		if (ePositiveCondition != CCondition.EPositiveCondition.NA)
		{
			text = text + "<sprite name=" + ePositiveCondition.ToString() + ">";
			isCondition = true;
		}
		if ((m_Ability.PositiveConditions == null || m_Ability.PositiveConditions.Count <= 0) && (m_Ability.NegativeConditions == null || m_Ability.NegativeConditions.Count <= 0))
		{
			return;
		}
		if (!isCondition)
		{
			text += ", ";
		}
		if (m_Ability.PositiveConditions != null)
		{
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in m_Ability.PositiveConditions)
			{
				text = text + "<sprite name=" + positiveCondition.Key.ToString() + ">";
			}
		}
		if (m_Ability.NegativeConditions == null)
		{
			return;
		}
		foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in m_Ability.NegativeConditions)
		{
			text = text + "<sprite name=" + negativeCondition.Key.ToString() + ">";
		}
	}

	private void ResolveText(ref string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			return;
		}
		if (m_Ability == null)
		{
			if (GameState.s_ShortRestingPlayer != null)
			{
				text = GLOOM.LocalizationManager.GetTranslation("GUI_SHORT_REST_CONFIRMATION");
			}
			else if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				text = GLOOM.LocalizationManager.GetTranslation(Choreographer.StartTurnHelpBoxKey());
			}
			return;
		}
		if (m_Ability.AbilityType == CAbility.EAbilityType.Move)
		{
			if (!string.IsNullOrEmpty(m_Ability.InvalidTileReason))
			{
				text = GLOOM.LocalizationManager.GetTranslation(m_Ability.InvalidTileReason);
			}
			else
			{
				text = GLOOM.LocalizationManager.GetTranslation("GUI_CHOOSE_ABILITY_TARGET_Move");
			}
			return;
		}
		string cardNameKey = GetCardNameKey();
		cardNameKey = LanguageSourceData.GetKeyFromFullTerm(cardNameKey ?? "");
		string suffix = ((m_Ability.NumberTargets == 1) ? "_Single" : "_Plural");
		CAbilityFilterContainer abilityFilter = m_Ability.AbilityFilter;
		bool flag = abilityFilter != null && abilityFilter.AbilityFilters?.Count == 1 && m_Ability.AbilityFilter.AbilityFilters[0].FilterTargetType == CAbilityFilter.EFilterTargetType.Self;
		if ((flag && TryLocalize("GUI_CHOOSE_ABILITY_" + cardNameKey, ref text)) || TryLocalize("GUI_CHOOSE_ABILITY_TARGET_" + cardNameKey, ref text))
		{
			return;
		}
		if (flag)
		{
			if (!TryLocalize($"GUI_CHOOSE_ABILITY_{m_Ability.AbilityType}", ref text))
			{
				text = GLOOM.LocalizationManager.GetTranslation("GUI_CONFIRM_ABILITY");
			}
		}
		else if (!TryLocalize($"GUI_CHOOSE_ABILITY_TARGET_{m_Ability.AbilityType}", ref text))
		{
			text = GLOOM.LocalizationManager.GetTranslation("GUI_CHOOSE_ABILITY_TARGET" + suffix);
		}
		bool TryLocalize(string key, ref string reference)
		{
			reference = GLOOM.LocalizationManager.GetTranslation(key + suffix, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
			if (reference != null)
			{
				return true;
			}
			reference = GLOOM.LocalizationManager.GetTranslation(key, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
			if (reference != null)
			{
				return true;
			}
			return false;
		}
	}

	private string GetCardNameKey()
	{
		PerksYMLData perksYMLData = m_Ability?.perkData;
		if (perksYMLData != null)
		{
			return perksYMLData.Name;
		}
		if (m_Card == null)
		{
			if (m_Ability?.TargetingActor != null)
			{
				CAbility ability = m_Ability;
				if (ability is CAbilityMove || ability is CAbilityAttack)
				{
					return m_Ability.TargetingActor.ActorLocKey();
				}
			}
			return null;
		}
		if (m_Card.CardType == CBaseCard.ECardType.ScenarioModifier)
		{
			return null;
		}
		if (m_Card.CardType == CBaseCard.ECardType.Dummy)
		{
			return null;
		}
		if (m_Card.CardType == CBaseCard.ECardType.AttackModifier)
		{
			return null;
		}
		if (m_Card.CardType == CBaseCard.ECardType.MonsterAbility)
		{
			MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.Find((MonsterCardYMLData s) => s.ID == m_Card.ID);
			if (!string.IsNullOrEmpty(monsterCardYMLData?.Name))
			{
				return monsterCardYMLData.Name;
			}
			return MonsterClassManager.MonsterAndObjectClassesWithInitiative.First((CMonsterClass c) => c.ID == ((CBaseAbilityCard)m_Card).ClassID).LocKey;
		}
		return m_Card.Name;
	}

	private void RefreshTranslated(bool controller = false, FormatTarget useWarningFormat = FormatTarget.NONE)
	{
		int num = 0;
		if (!controller)
		{
			RefreshTranslated(m_DefaultHelpInfo?.keybaordLines, m_DefaultHelpInfo?.keyboardTitle, useWarningFormat, num);
			{
				foreach (KeyValuePair<string, HelpInfo> additionalHelpInfo in m_AdditionalHelpInfos)
				{
					num++;
					RefreshTranslated(additionalHelpInfo.Value?.keybaordLines, additionalHelpInfo.Value?.keyboardTitle, useWarningFormat, num);
				}
				return;
			}
		}
		RefreshTranslated(m_DefaultHelpInfo?.controllerLines(), m_DefaultHelpInfo?.controllerTitle, useWarningFormat, num);
		foreach (KeyValuePair<string, HelpInfo> additionalHelpInfo2 in m_AdditionalHelpInfos)
		{
			num++;
			RefreshTranslated(additionalHelpInfo2.Value?.controllerLines(), additionalHelpInfo2.Value?.controllerTitle, useWarningFormat, num);
		}
	}

	private void RefreshTranslated(string text, string title = null, FormatTarget useWarningFormat = FormatTarget.NONE, int id = 0)
	{
		if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(text) && (Choreographer.s_Choreographer.m_WaitState == null || Choreographer.s_Choreographer.m_WaitState.m_State < Choreographer.ChoreographerStateType.Play || GameState.ThreadIsSleeping))
		{
			window.Hide();
			return;
		}
		ResolveAbility(ref text, ref title);
		if (id > 0)
		{
			title = string.Empty;
		}
		HelperTools.NormalizePool(ref linesPool, _boxLinePrefab.gameObject, transform, id + 1);
		linesPool[id].ShowTranslated(text, title, null, useWarningFormat);
		if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(text))
		{
			window.Hide();
		}
		else
		{
			window.Show();
		}
	}

	public static void GetTooltipData(out CAbility ability, out CBaseCard card)
	{
		ability = Singleton<HelpBox>.InstanceFast?.m_Ability;
		card = Singleton<HelpBox>.InstanceFast?.m_Card;
	}

	public void Hide(string id = null, bool onlyIfIdMatches = false)
	{
		if (id == null || (!onlyIfIdMatches && currentId == null) || !(id != currentId))
		{
			currentId = null;
			m_Ability = null;
			m_Card = null;
			m_OverrideAbiliy = null;
			m_OverrideCard = null;
			ClearHelpInfo();
			RefreshTranslated(null);
		}
	}

	public void HideKeyboard(string id = null, bool onlyIfIdMatches = false)
	{
		if (id != null && (onlyIfIdMatches || currentId != null) && id != currentId)
		{
			return;
		}
		if (m_DefaultHelpInfo == null)
		{
			Hide();
			return;
		}
		currentId = null;
		m_DefaultHelpInfo.keybaordLines = null;
		m_DefaultHelpInfo.keyboardTitle = null;
		if (!Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			ShowKeyboard();
		}
	}

	public void HighlightWarning()
	{
		for (int i = 0; i < linesPool.Count && linesPool[i].gameObject.activeSelf; i++)
		{
			linesPool[i].HighlightWarning();
		}
	}

	private void OnDisable()
	{
		ClearHelpInfo();
	}
}

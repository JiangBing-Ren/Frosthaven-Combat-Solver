using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using FH.Tools;
using Libs.Utils;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Achievments;

namespace ScenarioRuleLibrary;

public class CPlayerActor : CActor
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EAgility
	{
		None,
		Fast,
		Slow
	}

	public enum EForm
	{
		None,
		Choose,
		Melee,
		Range
	}

	public enum EPressure
	{
		None,
		Low,
		Regular,
		High,
		Over
	}

	public bool CanUseLongRest = true;

	public bool CanUseDefaultActions = true;

	public bool CanCancelActiveBonuses = true;

	public bool CanUseItems = true;

	public bool UseTakenPileForShortRest;

	public bool PlayBottomActionFirst;

	public int CompletedMasteries;

	private string m_CharacterName;

	private string m_CompanionGuid;

	private CHeroSummonActor m_CompanionCached;

	private List<int> m_characterHealthTable;

	private int m_TimeTokens = -1;

	private int m_TrophyTokens = -1;

	private int m_ResonanceTokens = -1;

	private bool m_IsInLoad;

	private EAgility m_Agility;

	private EForm m_PrevForm;

	private EForm m_Form;

	private EForm m_PendingFrom;

	[StateDump(Skip = true)]
	public bool SwitchingForm;

	private EPressure m_Pressure;

	public bool IsChangingPressure;

	public int PressureChangeValue;

	public new CCharacterClass CharacterClass => (CCharacterClass)m_Class;

	public string CharacterName
	{
		get
		{
			return m_CharacterName;
		}
		set
		{
			m_CharacterName = value;
		}
	}

	public bool SkipTopCardAction { get; set; }

	public bool SkipBottomCardAction { get; set; }

	public bool IsLongRestSelected { get; set; }

	public bool IsLongRestActionSelected { get; set; }

	public CAbilityExtraTurn.EExtraTurnType SelectingCardsForExtraTurnOfType { get; set; }

	public override bool IgnoreIceSlideEffect => CharacterClass.Perks.Any((PerksYMLData x) => x.Flags.HasFlag(EPerkFlag.IgnoreIceSlideEffect));

	public override bool IgnoreIceSlideWeight => CharacterClass.Perks.Any((PerksYMLData x) => x.Flags.HasFlag(EPerkFlag.IgnoreIceSlideEffect));

	public CActiveBonus SummonsContinueAfterDeathBonus => FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.GetMiscVar("SummonsContinueAfterSummonerDeath").GetBool());

	public bool SummonsContinueAfterDeath => SummonsContinueAfterDeathBonus != null;

	public override bool Invulnerable => CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Invulnerability).Count > 0;

	public override bool PierceInvulnerability
	{
		get
		{
			if (ScenarioScript.current != null && ScenarioScript.current.ShouldPierceInvulnerability(this))
			{
				return true;
			}
			return CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.PierceInvulnerability).Count > 0;
		}
	}

	public override bool Untargetable
	{
		get
		{
			if (CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.Untargetable).Count <= 0)
			{
				return IsDowned;
			}
			return true;
		}
	}

	public override List<CAbility.EAbilityType> Immunities
	{
		get
		{
			List<CAbility.EAbilityType> list = new List<CAbility.EAbilityType>();
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this, CAbility.EAbilityType.ImmunityTo))
			{
				if (item.IsRestricted(this) || !(item.Ability is CAbilityImmunityTo { NotImmuneToSelf: false } cAbilityImmunityTo))
				{
					continue;
				}
				foreach (CAbility.EAbilityType immuneToAbilityType in cAbilityImmunityTo.ImmuneToAbilityTypes)
				{
					if (!list.Contains(immuneToAbilityType))
					{
						list.Add(immuneToAbilityType);
					}
				}
			}
			return list;
		}
	}

	public CHeroSummonActor CompanionSummon
	{
		get
		{
			if (m_CompanionCached != null)
			{
				return m_CompanionCached;
			}
			m_CompanionCached = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == m_CompanionGuid);
			return m_CompanionCached;
		}
	}

	public override CPlayerActor PlayerActor => this;

	public override int SelectedRoundCardsCount => CharacterClass.RoundAbilityCardsSelectedInCardSelection.Count;

	public override int MaxRoundCardsCount
	{
		get
		{
			int num = 0;
			foreach (CActiveBonus item in ActiveBonuses.Where((CActiveBonus ab) => !ab.Ability.ActiveBonusData.GetVar("ExtraRoundCards").isUnknown))
			{
				int num2 = item.Ability.ActiveBonusData.GetVar("ExtraRoundCards").GetInt();
				num += num2;
			}
			if (CharacterClass?.RoundAbilityCardsSelectedInCardSelection != null && CharacterClass.RoundAbilityCardsSelectedInCardSelection.Count > 0)
			{
				return Math.Max(2 + num, CharacterClass.RoundAbilityCardsSelectedInCardSelection.Count);
			}
			return 2 + num;
		}
	}

	public override int MaxCardsClampedToHand => Math.Min(MaxRoundCardsCount, CharacterClass.HandAbilityCards.Count + CharacterClass.RoundAbilityCards.Count);

	public int MaxCardsClampedToRound => Math.Min(MaxRoundCardsCount, Math.Max(CharacterClass.RoundAbilityCards.Count, CharacterClass.RoundAbilityCardsSelectedInCardSelection.Count));

	public override bool MustPlayAllRoundCards => ActiveBonuses.Any((CActiveBonus ab) => ab.Ability.ActiveBonusData.GetVar("MustPlayAllRoundCards").GetBool());

	public string PersonalQuestID { get; set; }

	public EAgility Agility
	{
		get
		{
			return m_Agility;
		}
		set
		{
			bool flag = m_Agility != value;
			if (flag)
			{
				Log.Info($"Changing Agility from {m_Agility} to {value}");
			}
			m_Agility = value;
			if (flag)
			{
				CheckForCachedValuesAfterActiveBonusesUpdate();
			}
		}
	}

	public EForm Form
	{
		get
		{
			return m_Form;
		}
		set
		{
			if (m_Form == value)
			{
				return;
			}
			Log.Info($"Changing Form from {m_Form} to {value}");
			if (m_Form != EForm.Choose)
			{
				PrevForm = m_Form;
			}
			m_Form = value;
			if (m_Form == EForm.None || m_Form == EForm.Choose)
			{
				return;
			}
			if (PrevForm != EForm.None && PrevForm != m_Form)
			{
				if (m_Form == EForm.Range)
				{
					CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "COMBAT_LOG_SWITCH_FORM",
						m_Actors = new List<CActor> { this },
						m_Params = new List<string> { "Actor_0", "<sprite name=GMMeleeToRanged><sprite name=GMRanged>" }
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else if (m_Form == EForm.Melee)
				{
					CCombatLogMessage_MessageData message2 = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "COMBAT_LOG_SWITCH_FORM",
						m_Actors = new List<CActor> { this },
						m_Params = new List<string> { "Actor_0", "<sprite name=GMRangedToMelee><sprite name=GMMelee>" }
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
			CPlayerActorChangedForm_MessageData message3 = new CPlayerActorChangedForm_MessageData(this, m_PrevForm, m_Form);
			ScenarioRuleClient.MessageHandler(message3);
			if (m_Form != EForm.None && m_Form != EForm.Choose && !m_IsInLoad)
			{
				SEventLogMessageHandler.AddEventLogMessage(new SEventActorForm(m_Form, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorChangeForm, this)));
			}
			CActor mirrorActor = GetMirrorActor(includeMirroredClone: true);
			if (mirrorActor != null && CauseOfDeath == ECauseOfDeath.StillAlive)
			{
				CPlayerActorChangedForm_MessageData message4 = new CPlayerActorChangedForm_MessageData(mirrorActor, m_PrevForm, m_Form);
				ScenarioRuleClient.MessageHandler(message4);
			}
		}
	}

	public EForm PrevForm
	{
		get
		{
			return m_PrevForm;
		}
		set
		{
			if (m_PrevForm != value)
			{
				Log.Info($"Changing PrevForm from {m_PrevForm} to {value}");
			}
			m_PrevForm = value;
		}
	}

	public EForm PendingForm
	{
		get
		{
			return m_PendingFrom;
		}
		set
		{
			if (m_PendingFrom != value)
			{
				Log.Info($"Changing PendingForm from {m_PendingFrom} to {value}");
			}
			m_PendingFrom = value;
			CPlayerActorChangedPendingForm_MessageData message = new CPlayerActorChangedPendingForm_MessageData(this, m_Form, m_PendingFrom);
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override EPressure Pressure
	{
		get
		{
			return m_Pressure;
		}
		set
		{
			if (value != EPressure.None)
			{
				Log.Info($"{m_CharacterName}: Changing Pressure with {PressureChangeValue} from {m_Pressure} to {value}");
			}
			m_Pressure = value;
		}
	}

	public int NextSummonID { get; private set; }

	public CPlayerActor(Point startPosition, CCharacterClass characterClass, int currentHealth, int maxHealth, int level, string actorGuid, int chosenModelIndex, string characterName, int timeTokens, EAgility agility, EForm form, EForm prevForm, EForm pendingForm, int nextSummonID, int trophyTokens, int resonanceTokens)
		: base(EType.Player, characterClass, startPosition, currentHealth, maxHealth, level, actorGuid, chosenModelIndex)
	{
		m_IsInLoad = true;
		m_CharacterName = characterName;
		SetTimeTokens(timeTokens, 5, log: false);
		SetTrophyTokens(trophyTokens, 5, log: false);
		SetResonanceTokens(resonanceTokens, 5, log: false);
		Agility = agility;
		Form = form;
		PrevForm = prevForm;
		PendingForm = pendingForm;
		NextSummonID = nextSummonID;
		if (actorGuid == null && CharacterClass.InitialState)
		{
			CharacterClass.InitialState = false;
			CharacterClass.ResetAttackModifierDeck();
		}
		m_OnMovedListeners = (MovedListener)Delegate.Combine(m_OnMovedListeners, new MovedListener(OnMoved));
		Log.Info("Created a new CPlayerActor with ModelInstanceID: " + characterClass.ModelInstanceID);
		m_IsInLoad = false;
	}

	private void OnMoved(CAbility moveAbility, CActor movedActor, List<CActor> actorsCarried, bool newActorCarried, int moveHexes, bool finalMovement, int difficultTerrainTilesEntered, int hazardousTerrainTilesEntered, int thisMoveHexes, Point prevPosition)
	{
		if (moveAbility is CAbilityMove)
		{
			CampaignStatRepository.Instance.TilesMovedByCharacters.Increment(CharacterName, moveHexes);
		}
	}

	public void SetTimeTokens(int value, int maxValue = 5, bool log = true)
	{
		if (UsesTimeTokens())
		{
			int timeTokens = m_TimeTokens;
			m_TimeTokens = Math.Clamp(value, 0, maxValue);
			int num = m_TimeTokens - timeTokens;
			ScenarioRuleClient.MessageHandler(new CHeroTokensUpdated_MessageData(this)
			{
				m_TimeTokens = (log ? num : 0)
			});
		}
	}

	public int GetTimeTokens()
	{
		return m_TimeTokens;
	}

	public bool UsesTimeTokens()
	{
		return CharacterClass.TimeTokens >= 0;
	}

	public void SetTrophyTokens(int value, int maxValue = 5, bool log = true)
	{
		if (UsesTrophyTokens())
		{
			int num = Math.Max(0, m_TrophyTokens);
			int num2 = Math.Clamp(value, 0, maxValue);
			int num3 = num2 - num;
			m_TrophyTokens = num2;
			if (log)
			{
				GameState.AddTrophyTokensGainedThisRound(num3);
				ScenarioRuleClient.MessageHandler(new CHeroTokensUpdated_MessageData(this)
				{
					m_TrophyTokens = num3
				});
			}
		}
	}

	public int GetTrophyTokens()
	{
		return m_TrophyTokens;
	}

	public int GetReservedTrophyTokens()
	{
		if (PhaseManager.CurrentPhaseAction == null)
		{
			return 0;
		}
		return PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Sum((CActionAugmentation x) => x.Trohpies);
	}

	public int GetCurrentAndReservedTrophyTokens()
	{
		return GetTrophyTokens() - GetReservedTrophyTokens();
	}

	public bool UsesTrophyTokens()
	{
		return CharacterClass.TrophyTokens >= 0;
	}

	public void SetResonanceTokens(int value, int maxValue = 5, bool log = true)
	{
		if (!UsesResonanceTokens())
		{
			return;
		}
		int num = Math.Clamp(value, 0, maxValue);
		int num2 = num - Math.Max(0, m_ResonanceTokens);
		bool flag = num2 > 0;
		m_ResonanceTokens = num;
		if (!ScenarioManager.IsLoading && log)
		{
			if (flag)
			{
				CPlaySound_MessageData message = new CPlaySound_MessageData("Shattersong_ResonanceGained");
				ScenarioRuleClient.MessageHandler(message);
			}
			ScenarioRuleClient.MessageHandler(new CHeroTokensUpdated_MessageData(this)
			{
				m_ResonanceTokens = num2
			});
		}
	}

	public int GetResonanceTokens()
	{
		return m_ResonanceTokens;
	}

	public int GetReservedResonanceTokens()
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (PhaseManager.CurrentPhaseAction == null)
			{
				return 0;
			}
			return 0 + PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.Sum((CActionAugmentation x) => x.Resonance) + (from x in FindApplicableActiveBonuses()
				where x.IsActiveBonusToggledAndNotRestricted(this) && x.Ability.ActiveBonusData.IsToggleBonus
				select x).Sum((CActiveBonus x) => x.Ability.ActiveBonusData.GetMiscVar("ResonanceCost").GetInt());
		}
	}

	public int GetCurrentAndReservedResonanceTokens()
	{
		return GetResonanceTokens() - GetReservedResonanceTokens();
	}

	public bool UsesResonanceTokens()
	{
		return CharacterClass.ResonanceTokens >= 0;
	}

	public bool IsExhausted()
	{
		if (ScenarioScript.current != null && ScenarioScript.current.IsExhaustedOverride(this, out var isExhausted))
		{
			return isExhausted;
		}
		int cardsInHand;
		return CountCardsForExhausted(null, out cardsInHand);
	}

	public bool CountCardsForExhausted(Func<CAbilityCard, bool> count, out int cardsInHand, bool checkWillBeExhausted = false)
	{
		if (count == null)
		{
			count = (CAbilityCard c) => true;
		}
		bool result = false;
		CCharacterClass characterClass = CharacterClass;
		bool restDisabled = ScenarioManager.CurrentScenarioState.ScenarioModifiers.Any((CScenarioModifier x) => x.IsActiveInRound(ScenarioManager.CurrentScenarioState.RoundNumber) && x.ScenarioModifierType == EScenarioModifierType.DisableRest);
		bool freeLongRest = ScenarioManager.CurrentScenarioState.FreeLongRest || ScenarioManager.CurrentScenarioState.FreeShortRestButMuddle || ScenarioManager.CurrentScenarioState.FreeShortRest;
		cardsInHand = 0;
		if (Class.ID == "GeminateID")
		{
			int num = characterClass.HandAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Melee);
			int num2 = characterClass.RoundAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Melee);
			int num3 = characterClass.DiscardedAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Melee);
			int num4 = characterClass.HandAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Range);
			int num5 = characterClass.RoundAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Range);
			int num6 = characterClass.DiscardedAbilityCards.Count((CAbilityCard x) => count(x) && x.GetAbilityCardYML.RequiredForm == EForm.Range);
			cardsInHand = Math.Max(num + num2, num4 + num5);
			if (PendingForm == EForm.None)
			{
				if (CharacterClass.HasShortRested || num3 + num6 <= 1)
				{
					if (Form == EForm.Melee)
					{
						result = CalcExhausted(num, num2, num3, freeLongRest, restDisabled, checkWillBeExhausted);
					}
					else if (Form == EForm.Range)
					{
						result = CalcExhausted(num4, num5, num6, freeLongRest, restDisabled, checkWillBeExhausted);
					}
					else if (Form == EForm.Choose)
					{
						result = CalcExhausted(num, num2, num3, freeLongRest, restDisabled, checkWillBeExhausted) && CalcExhausted(num4, num5, num6, freeLongRest, restDisabled, checkWillBeExhausted);
					}
				}
				else
				{
					result = CalcExhausted(num, num2, num3, freeLongRest, restDisabled, checkWillBeExhausted) && CalcExhausted(num4, num5, num6, freeLongRest, restDisabled, checkWillBeExhausted);
				}
			}
			else
			{
				result = CalcExhausted(num, num2, num3, freeLongRest, restDisabled, checkWillBeExhausted) && CalcExhausted(num4, num5, num6, freeLongRest, restDisabled, checkWillBeExhausted);
			}
		}
		else
		{
			int num7 = characterClass.HandAbilityCards.Count((CAbilityCard x) => count(x));
			int num8 = characterClass.RoundAbilityCards.Count((CAbilityCard x) => count(x));
			int discarded = characterClass.DiscardedAbilityCards.Count((CAbilityCard x) => count(x));
			cardsInHand = num7 + num8;
			result = CalcExhausted(num7, num8, discarded, freeLongRest, restDisabled, checkWillBeExhausted);
		}
		return result;
	}

	public static bool CalcExhausted(int hand, int round, int discarded, bool freeLongRest, bool restDisabled, bool checkWillBeExhausted = false)
	{
		if (checkWillBeExhausted)
		{
			return CalcWillBeExhausted(hand, round, discarded, freeLongRest, restDisabled);
		}
		if (restDisabled)
		{
			return hand + round < 2;
		}
		if (hand + round >= 2 || discarded >= 2)
		{
			if (freeLongRest)
			{
				return hand + round + discarded == 0;
			}
			return false;
		}
		return true;
	}

	public static bool CalcWillBeExhausted(int hand, int round, int discarded, bool freeLongRest, bool restDisabled)
	{
		if (restDisabled)
		{
			return hand + round <= 2;
		}
		int num = hand + round;
		int num2 = num + Math.Max(discarded - 1, 0);
		bool flag = discarded >= 2;
		if (num >= 2 || (num2 >= 2 && flag))
		{
			if (freeLongRest)
			{
				return hand + round + discarded == 0;
			}
			return false;
		}
		return true;
	}

	public bool WillBeExhaustedThisRound()
	{
		int cardsInHand;
		return CountCardsForExhausted(null, out cardsInHand, checkWillBeExhausted: true);
	}

	public override int Initiative()
	{
		if (InitiativeLockedDuringRound.HasValue)
		{
			return InitiativeLockedDuringRound.Value;
		}
		int num = 0;
		int initiativeBonus = GetInitiativeBonus();
		List<CActiveBonus> list = FindApplicableActiveBonuses(CAbility.EAbilityType.AdjustInitiative, CActiveBonus.EActiveBonusBehaviourType.SetInitiative);
		if (CharacterClass.TempSetInitiative.HasValue)
		{
			num = CharacterClass.TempSetInitiative.Value;
			num += initiativeBonus;
		}
		else if (list.Count > 0)
		{
			num = list.First().BespokeBehaviourStrength();
			num += initiativeBonus;
		}
		else if (CharacterClass.LongRest || IsLongRestSelected)
		{
			num = initiativeBonus;
			num += 99;
		}
		else if (GameState.IsPerformingCardOutOfPhase && GameState.CurrentOutOfPhaseCardSelectionAbility.TargetingActor == this && GameState.CurrentOutOfPhaseCardSelectionAbility.GetAbilityBaseCard() is CAbilityCard cAbilityCard)
		{
			num += cAbilityCard.Initiative;
		}
		else if (SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && CharacterClass.ExtraTurnInitiativeAbilityCard != null)
		{
			num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
		}
		else if (TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if ((CharacterClass.ExtraTurnCardsSelectedInCardSelection.Count > 0 && CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard)) || (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard)))
			{
				num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
			}
		}
		else
		{
			num = initiativeBonus;
			if (CharacterClass.RoundAbilityCards.Count > 0 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
			{
				num += CharacterClass.InitiativeAbilityCard.Initiative;
			}
			else if (HasPendingExtraTurn)
			{
				num += CharacterClass.ExtraTurnInitiativeAbilityCard.Initiative;
			}
			else if (CharacterClass.InitiativeAbilityCard != null)
			{
				num += CharacterClass.InitiativeAbilityCard.Initiative;
			}
		}
		if (UsesTimeTokens() && Agility == EAgility.Slow && !CharacterClass.LongRest)
		{
			num += 30;
		}
		return num;
	}

	public override int SubInitiative()
	{
		int num = 0;
		if (CharacterClass.LongRest)
		{
			num += 99;
		}
		else if (TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater)
		{
			if (CharacterClass.ExtraTurnCardsSelectedInCardSelection.Count > 0 && CharacterClass.ExtraTurnCardsSelectedInCardSelection.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
			{
				num += CharacterClass.ExtraTurnCardsSelectedInCardSelection.FirstOrDefault((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard)?.Initiative ?? 0;
			}
			else if (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
			{
				num += CharacterClass.NextPendingExtraTurnCards.FirstOrDefault((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard)?.Initiative ?? 0;
			}
		}
		else if (CharacterClass.RoundAbilityCards.Count > 0 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.RoundAbilityCards.FirstOrDefault((CAbilityCard f) => f != CharacterClass.InitiativeAbilityCard)?.Initiative ?? 0;
		}
		else if (CharacterClass.NextPendingExtraTurnCards.Count > 0 && CharacterClass.NextPendingExtraTurnCards.Contains(CharacterClass.ExtraTurnInitiativeAbilityCard))
		{
			num += CharacterClass.NextPendingExtraTurnCards.FirstOrDefault((CAbilityCard f) => f != CharacterClass.ExtraTurnInitiativeAbilityCard)?.Initiative ?? 0;
		}
		else if (CharacterClass.SubInitiativeAbilityCard != null)
		{
			num += CharacterClass.SubInitiativeAbilityCard.Initiative;
		}
		if (num > 0 && UsesTimeTokens() && Agility == EAgility.Slow && !CharacterClass.LongRest)
		{
			num += 30;
		}
		return num;
	}

	public int OriginalInitiative()
	{
		int num = (CharacterClass.TempInitiativeBonus.HasValue ? CharacterClass.TempInitiativeBonus.Value : 0);
		if (CharacterClass.LongRest)
		{
			num += 99;
		}
		else if (CharacterClass.RoundAbilityCards.Count > 0 && CharacterClass.RoundAbilityCards.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.InitiativeAbilityCard.Initiative;
		}
		else if (SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater && CharacterClass.RoundAbilityCardsSelectedInCardSelection.Contains(CharacterClass.InitiativeAbilityCard))
		{
			num += CharacterClass.InitiativeAbilityCard.Initiative;
		}
		if (CharacterClass.TempInitiativeBonus != 0)
		{
			return Math.Max(1, num);
		}
		return num;
	}

	public override string ActorLocKey()
	{
		return CharacterClass.LocKey;
	}

	public override string GetPrefabName()
	{
		if (string.IsNullOrEmpty(OverrideCharacterModel))
		{
			return CharacterClass.CharacterModel.ToString();
		}
		return OverrideCharacterModel;
	}

	public override void Move(int maxMoveCount, bool jump, bool fly, int range, bool allowMove, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, Point? freeTileAwayFromCaster = null, CAbilityAttack attack = null, bool firstMove = false, bool moveTest = false, bool carryOtherActors = false)
	{
		CActorStatic.AIMove(this, maxMoveCount, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, freeTileAwayFromCaster, isWaterMonster: false, Type == EType.Player || Type == EType.Ally, range, allowMove, attack, firstMove, moveTest, carryOtherActors);
	}

	public static List<Point> FindPlayerPath(CActor actorFindingPath, Point startLocation, Point endLocation, bool ignoreBlocked, bool ignoreMoveCost, out bool foundPath, bool avoidHazards = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, int waterTilesThatCostZero = 0, bool carryOtherActors = false, EType carryType = EType.Unknown, bool logFailure = false, bool assignLevelObjects = false, int maxMovementPoints = int.MaxValue, bool ignoreCannotStop = false, bool jump = false, bool fly = false, bool ignoreDoorLocks = false)
	{
		HashSet<Point> hashSet = new HashSet<Point>();
		HashSet<Point> hashSet2 = new HashSet<Point>();
		if (ignoreBlocked && actorFindingPath.IgnoreActorCollision)
		{
			List<CObjectObstacle> list;
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				list = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
			}
			foreach (CObjectObstacle item in list)
			{
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					hashSet.Add(new Point(pathingBlocker));
				}
			}
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				if (!@object.IsTransientBlocker)
				{
					continue;
				}
				foreach (Point tile in @object.AttachedProp.GetTiles())
				{
					hashSet.Add(tile);
				}
			}
		}
		if (actorFindingPath.HasMirrorActor())
		{
			CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour();
			if (mirrorAbilityBehaviour != null && (actorFindingPath == mirrorAbilityBehaviour.ActiveBonus.Actor || actorFindingPath == mirrorAbilityBehaviour.MirrorTarget))
			{
				CTile[,] tiles = ScenarioManager.Tiles;
				foreach (CTile cTile in tiles)
				{
					if (cTile != null)
					{
						if (!mirrorAbilityBehaviour.IsMoveTileValidForAbilityMirrored(cTile.m_ArrayIndex, actorFindingPath))
						{
							hashSet.Add(cTile.m_ArrayIndex);
						}
						if (!ignoreCannotStop && !mirrorAbilityBehaviour.IsStopTileValidForAbilityMirrored(cTile.m_ArrayIndex, actorFindingPath))
						{
							hashSet2.Add(cTile.m_ArrayIndex);
						}
					}
				}
			}
		}
		if (!ignoreBlocked && (!actorFindingPath.IgnoreActorCollision || ((actorFindingPath.IgnoreActorCollision & carryOtherActors) && carryType != EType.Unknown)))
		{
			if (!CActor.AreActorsAllied(carryType, EType.Enemy, null, null))
			{
				foreach (CEnemyActor item2 in new List<CEnemyActor>(ScenarioManager.Scenario.Enemies))
				{
					if (item2.VisibleBlocksPathing && item2.IsTransientBlocker && !CActor.AreActorsAllied(carryType, item2.Type, null, item2))
					{
						hashSet.AddRange(from x in item2.GetTilesCanAttackFrom()
							select x.m_ArrayIndex);
					}
				}
			}
			if (!CActor.AreActorsAllied(carryType, EType.Enemy2, null, null))
			{
				foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
				{
					if (enemy2Monster.VisibleBlocksPathing && enemy2Monster.IsTransientBlocker && !CActor.AreActorsAllied(carryType, enemy2Monster.Type, null, enemy2Monster))
					{
						hashSet.AddRange(from x in enemy2Monster.GetTilesCanAttackFrom()
							select x.m_ArrayIndex);
					}
				}
			}
			if (actorFindingPath != null)
			{
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					if (playerActor.VisibleBlocksPathing && playerActor.IsTransientBlocker && !CActor.AreActorsAllied(actorFindingPath.Type, playerActor.Type, actorFindingPath, playerActor))
					{
						hashSet.AddRange(from x in playerActor.GetTilesCanAttackFrom()
							select x.m_ArrayIndex);
					}
				}
			}
			if (!CActor.AreActorsAllied(carryType, EType.Neutral, null, null))
			{
				foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
				{
					if (neutralMonster.VisibleBlocksPathing && neutralMonster.IsTransientBlocker && !CActor.AreActorsAllied(carryType, neutralMonster.Type, null, neutralMonster))
					{
						hashSet.AddRange(from x in neutralMonster.GetTilesCanAttackFrom()
							select x.m_ArrayIndex);
					}
				}
			}
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				if (allActor.IsDowned && !hashSet.Contains(allActor.ArrayIndex))
				{
					hashSet.AddRange(from x in allActor.GetTilesCanAttackFrom()
						select x.m_ArrayIndex);
				}
			}
			foreach (CObjectActor object2 in ScenarioManager.Scenario.Objects)
			{
				if (object2.VisibleBlocksPathing && object2.IsTransientBlocker)
				{
					hashSet.AddRange(from x in object2.GetTilesCanAttackFrom()
						select x.m_ArrayIndex);
				}
			}
		}
		HashSet<Point> hashSet3 = new HashSet<Point>();
		List<CObjectObstacle> list2;
		List<CObjectDoor> list3;
		lock (ScenarioManager.CurrentScenarioState.Props)
		{
			list2 = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectObstacle>().ToList();
			list3 = (from d in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>()
				where d.DoorHasExtraLock && d.DoorExtraLockIsLocked
				select d).ToList();
		}
		foreach (CObjectObstacle item3 in list2)
		{
			foreach (TileIndex pathingBlocker2 in item3.PathingBlockers)
			{
				if (item3.IgnoresFlyAndJump)
				{
					hashSet3.Add(new Point(pathingBlocker2));
				}
			}
			if (item3.RuntimeAttachedActor == null || !item3.RuntimeAttachedActor.IsTransientBlocker)
			{
				continue;
			}
			foreach (Point tile2 in item3.GetTiles())
			{
				hashSet2.Add(tile2);
			}
		}
		if (actorFindingPath.TreatTrapsAsObstacles)
		{
			CScenarioModifierMakeTrapsObstacles cScenarioModifierMakeTrapsObstacles = ScenarioManager.CurrentScenarioState.ScenarioModifiers.OfType<CScenarioModifierMakeTrapsObstacles>().First();
			foreach (CObjectTrap item4 in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>())
			{
				if (cScenarioModifierMakeTrapsObstacles.ScenarioModifierFilter.IsValidProp(item4))
				{
					hashSet3.Add(new Point(item4.ArrayIndex));
				}
			}
		}
		if (actorFindingPath is CHeroSummonActor)
		{
			foreach (Point item5 in from d in ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDoor>()
				where !d.DoorIsOpen
				select new Point(d.ArrayIndex))
			{
				hashSet3.Add(item5);
			}
		}
		if (!ignoreDoorLocks)
		{
			foreach (CObjectDoor item6 in list3)
			{
				hashSet3.Add(new Point(item6.ArrayIndex));
			}
		}
		return ScenarioManager.PathFinder.FindPath(actorFindingPath, startLocation, endLocation, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false, hashSet.ToList(), hashSet3.ToList(), hashSet2.ToList(), assignLevelObjects, maxMovementPoints, jump, isCalculatingDistance: false, ignoreCannotStop, fly);
	}

	public new CPlayerActor Clone()
	{
		CPlayerActor cPlayerActor = (CPlayerActor)MemberwiseClone();
		cPlayerActor.Inventory.InventoryActor = cPlayerActor;
		return cPlayerActor;
	}

	public void SetXP(int amount)
	{
		m_XP = amount;
	}

	public void SetPlayerLevel(int level)
	{
		m_Level = Math.Max(1, Math.Min(9, level));
	}

	public override void ActionSelection()
	{
		base.ActionSelection();
		GameState.InternalCurrentActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
	}

	public int GetMaxHealthForCharacterLevel()
	{
		return CharacterClass.HealthTable[Level];
	}

	public void SetCompanionGuid(string companionGuid)
	{
		m_CompanionGuid = companionGuid;
	}

	public override void LoadFields(ActorState state)
	{
		if (state is PlayerState playerState)
		{
			CCharacterClass obj = Class as CCharacterClass;
			m_IsInLoad = true;
			base.LoadFields(state);
			m_CharacterName = playerState.CharacterName;
			m_CarriedQuestProps = new List<CObjectProp>(playerState.CarriedQuestProps);
			SetTimeTokens(playerState.TimeTokens, 5, log: false);
			SetTrophyTokens(playerState.TrophyTokens, 5, log: false);
			SetResonanceTokens(playerState.ResonanceTokens, 5, log: false);
			CharacterClass.ResetInitiativeBonus(this);
			InitiativeLockedDuringRound = ((state.InitiativeLockedDuringRound != -1) ? state.InitiativeLockedDuringRound : ((int?)null));
			Agility = playerState.Agility;
			Form = playerState.Form;
			PrevForm = playerState.PrevForm;
			PendingForm = playerState.PendingForm;
			NextSummonID = playerState.NextSummonID;
			Pressure = playerState.Pressure;
			m_Gold = playerState.ScenarioGold;
			if (playerState.ScenarioLoot != null)
			{
				m_TotalLoot = new LootState();
				m_TotalLoot.AccumulateLootFrom(playerState.ScenarioLoot);
			}
			if (playerState.ScenarioLootFromLootCards != null)
			{
				m_LootFromLootCards = new LootState();
				m_LootFromLootCards.AccumulateLootFrom(playerState.ScenarioLootFromLootCards);
			}
			m_XP = playerState.ScenarioXP;
			IsLongRestSelected = playerState.IsLongRestSelected;
			IsLongRestActionSelected = playerState.IsLongRestActionSelected;
			obj.HasStartedLongRest = false;
			obj.LongRest = playerState.IsLongResting;
			obj.HasLongRested = playerState.HasLongRested;
			obj.ShortRest = playerState.IsShortResting;
			obj.HasShortRested = playerState.HasShortRested;
			obj.HasImprovedShortRested = playerState.HasImprovedShortRested;
			obj.HasQueuedPreRestBonuses = playerState.HasQueuedPreRestBonuses;
			obj.ShortRestCardRedrawn = playerState.ShortRestCardRedrawn;
			obj.ShortRestCardBurned = playerState.ShortRestCardBurned;
			obj.ImprovedShortRest = playerState.ImprovedShortRestState;
			obj.ImprovedShortRestDrawCount = playerState.ImprovedShortRestDrawCountState;
			m_CompanionGuid = playerState.CompanionSummonGuid;
			LoadInventory(playerState);
			m_IsInLoad = false;
		}
	}

	public void LoadInventory(PlayerState playerState)
	{
		Inventory.SmallItemMax = playerState.MaxSmallItems;
		foreach (CItem item in playerState.Items)
		{
			CItem.EItemSlotState slotState = item.SlotState;
			Inventory.AddItem(item, reactivate: false);
			switch (slotState)
			{
			case CItem.EItemSlotState.Consumed:
			case CItem.EItemSlotState.Spent:
				item.SlotState = slotState;
				break;
			case CItem.EItemSlotState.Active:
				item.SlotState = CItem.EItemSlotState.Selected;
				Inventory.HandleActiveItemTriggered(item, ignoreConsume: true);
				break;
			case CItem.EItemSlotState.TempLocked:
				item.SlotState = CItem.EItemSlotState.TempLocked;
				break;
			}
		}
	}

	public int GetNextSummonId(CHeroSummonClass summonClass)
	{
		int nextSummonID;
		if (NextSummonID == 0)
		{
			foreach (CHeroSummonActor item in ScenarioManager.Scenario.HeroSummons.Where((CHeroSummonActor sm) => sm.Summoner.ActorGuid == ActorGuid))
			{
				if (item.StandeeID > NextSummonID)
				{
					NextSummonID = item.StandeeID;
				}
			}
			nextSummonID = NextSummonID + 1;
			NextSummonID = nextSummonID;
		}
		int nextSummonID2 = NextSummonID;
		nextSummonID = NextSummonID + 1;
		NextSummonID = nextSummonID;
		return nextSummonID2;
	}

	public void RecycleSummonId(CHeroSummonClass summonClass, int id)
	{
	}

	public EAgility CalcAgility()
	{
		if (MindControlAbility != null && MindControlAbility.TargetingActor != this && MindControlAbility.TargetingActor is CPlayerActor cPlayerActor)
		{
			return cPlayerActor.CalcAgility();
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(this))
		{
			if (item.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.TreatSlowAgilityAsFast))
			{
				return EAgility.Fast;
			}
		}
		return Agility;
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		bool result = base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility, attackSummary);
		if (startedOnDeathAbility)
		{
			TargetingActor = targetingActor;
			return false;
		}
		LogActiveBonusTrackers(this);
		return result;
	}

	public override void OnDeathAbilityComplete()
	{
		if (CauseOfDeath == ECauseOfDeath.None)
		{
			Log.ErrorWithReport("Cause of death was not set before calling OnDeathAbilityComplete()");
			CauseOfDeath = ECauseOfDeath.Undetermined;
		}
		base.OnDeath(TargetingActor, CauseOfDeath, out var _, fromDeathAbilityComplete: true, m_CauseOfDeathAbility, AttackSummary);
		GameState.KillActorInternal(TargetingActor, this, m_OnDeathAbilityUsed);
		TargetingActor = null;
		m_OnDeathAbilityUsed = false;
		m_CauseOfDeathAbility = null;
	}

	public IEnumerable<CHeroSummonActor> GetAllHeroSummons()
	{
		foreach (HeroSummonState heroSummon in ScenarioManager.CurrentScenarioState.HeroSummons)
		{
			if (heroSummon?.Actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor?.Summoner == this && !cHeroSummonActor.IsDead)
			{
				yield return cHeroSummonActor;
			}
		}
	}

	public List<CHeroSummonActor> GetHeroSummonList()
	{
		return GetAllHeroSummons().ToList();
	}

	public static void LogHeroSummonMoveChanged(CPlayerActor playerActor)
	{
		foreach (CHeroSummonActor allHeroSummon in playerActor.GetAllHeroSummons())
		{
			LogHeroSummonMoveChanged(allHeroSummon);
		}
	}

	public static void LogHeroSummonMoveChanged(CHeroSummonActor heroSummon)
	{
		if (heroSummon?.Summoner != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventHeroSummonRangeChange(heroSummon.GetDistanceTo(heroSummon.Summoner, viaPathfinding: true), heroSummon.Summoner?.CauseOfDeath ?? ECauseOfDeath.None, heroSummon.RoundSpawned, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorHeroSummonRangeChange, heroSummon)));
		}
	}

	public static void LogActiveBonusTrackers(CPlayerActor player, List<CActiveBonus> cachedActiveBonuses = null)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventActiveBonus(player, new SEventActor.SEventActorBaseData(ESESubTypeActor.ActorActiveBonusTracker, player), cachedActiveBonuses));
	}

	public void BuffFromAllShadows()
	{
		CSpawnProjectiles_MessageData message = new CSpawnProjectiles_MessageData
		{
			projectileSettingsName = "BuffProjectileSettings",
			m_TargetActor = this,
			m_FromProps = ScenarioManager.CurrentScenarioState.ShadowProps.ToList()
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool CanBattleGoalsCompleteOnLoseScenario()
	{
		return Inventory.AllItemsAndPets.Any((CItem x) => x.GetVar("CanBattleGoalsCompleteOnLoseScenario").GetBool());
	}

	public override void EndTurn()
	{
		base.EndTurn();
		if (GetVar("LostAllCards").GetBool())
		{
			SetTempVar("LostAllCards", false);
		}
		foreach (CItem item in Inventory.AllItems.Where((CItem item) => item.YMLData.Usage == CItem.EUsageType.Flip && item.YMLData.FlipEndOfTurn && item.SlotState == CItem.EItemSlotState.TempLocked).ToList())
		{
			Inventory.FlipItem(item);
		}
	}

	public static void FinishChangePressure(bool endTurn)
	{
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor actor) => actor.Class.ID == "MetalMosaicID");
		if (cPlayerActor == null)
		{
			return;
		}
		if (cPlayerActor.IsChangingPressure)
		{
			int pressureChangeValue = cPlayerActor.PressureChangeValue;
			cPlayerActor.Pressure = (EPressure)Math.Clamp((int)(cPlayerActor.Pressure + cPlayerActor.PressureChangeValue), 1, 4);
			cPlayerActor.PressureChangeValue = 0;
			cPlayerActor.IsChangingPressure = false;
			CChangePressure_MessageData message = new CChangePressure_MessageData(null, cPlayerActor)
			{
				m_PressureChange = pressureChangeValue
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		foreach (CActiveBonus item in (from bonus in CharacterClassManager.FindAllActiveBonuses().Distinct()
			where bonus.Ability.GetMiscVar("FinishPressure").isValid
			select bonus).ToList())
		{
			if (endTurn || !item.Ability.GetMiscVar("FinishOnlyAtEndTurn").GetBool())
			{
				if (cPlayerActor.Pressure.ToString() == item.Ability.GetMiscVar("FinishPressure").ValueString())
				{
					item.Finish();
				}
				else if (cPlayerActor.Pressure.ToString() == item.Ability.GetMiscVar("FinishPressureAlt").ValueString())
				{
					item.Finish();
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FHGUI;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Scenario.Use_Bar;
using GLOOM;
using Libs.GameConsole;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ActorStatPanel : Singleton<ActorStatPanel>
{
	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private RectTransform playerCardsAnchor;

	[SerializeField]
	private float hiddenCardsOffset = 5000f;

	[Header("Character Stats")]
	[SerializeField]
	private GameObject characterStatsObject;

	[SerializeField]
	private TMP_Text characterLifeAmount;

	[SerializeField]
	private FHImageFillElement characterHpBar;

	[SerializeField]
	private UICharacterStats characterStats;

	[SerializeField]
	private UIPersonalQuestProgress personalQuestProgress;

	[SerializeField]
	private GameObject personalQuestContainer;

	[SerializeField]
	private UIScenarioBattleGoalProgress battleGoalProgress;

	[SerializeField]
	private GameObject battleGoalContainer;

	[SerializeField]
	private GoldCounter goldCounter;

	[Header("Attack Modifiers")]
	[SerializeField]
	private UIAttackModifierCalculator characterMissMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterTimesTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMissMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyTimesTwoMod;

	[SerializeField]
	private Sprite availableCounter;

	[SerializeField]
	private Sprite usedCounter;

	[SerializeField]
	private GameObject counterPrefab;

	[SerializeField]
	private GameObject ModifierObject;

	[SerializeField]
	private RectTransform modifierContainer;

	[SerializeField]
	private UIScenarioAttackModifier modifierPrefab;

	[Header("Monster Card")]
	[SerializeField]
	private GameObject enemyStatsObject;

	[SerializeField]
	private GameObject enemyCard;

	[SerializeField]
	private TMP_Text enemyCardTitle;

	[SerializeField]
	private TMP_Text enemyName;

	[SerializeField]
	private TMP_Text enemyLevel;

	[SerializeField]
	private Image enemyHealthIcon;

	[SerializeField]
	private TMP_Text enemyHealthName;

	[SerializeField]
	private TMP_Text enemyHealthAmount;

	[SerializeField]
	private FHImageFillElement enemyHpBar;

	[SerializeField]
	private Image enemyMovementIcon;

	[SerializeField]
	private TMP_Text enemyMovementName;

	[SerializeField]
	private TMP_Text enemyMovementAmount;

	[SerializeField]
	private Image enemyAttackIcon;

	[SerializeField]
	private TMP_Text enemyAttackName;

	[SerializeField]
	private TMP_Text enemyAttackAmount;

	[SerializeField]
	private Image enemyRangeIcon;

	[SerializeField]
	private TMP_Text enemyRangeName;

	[SerializeField]
	private TMP_Text enemyRangeAmount;

	[SerializeField]
	private GameObject enemyPetCaptureInfo;

	[SerializeField]
	private GameObject enemyImmunitiesObject;

	[SerializeField]
	private Transform enemyImmunitiesContentHolder;

	[SerializeField]
	private AutoScrollRect enemyImmunitiesScroller;

	[SerializeField]
	private GameObject uiMonsterImmunityPrefab;

	[SerializeField]
	private Image enemyPortrait;

	[SerializeField]
	private GameObject onAttackConditionsObject;

	[SerializeField]
	private TMP_Text onAttackConditionsTitle;

	[SerializeField]
	private Transform onAttackConditionsContentHolder;

	[SerializeField]
	private AutoScrollRect onAttackConditionsScroller;

	[SerializeField]
	private GameObject onAttackConditionsPrefab;

	[SerializeField]
	private Transform enemyModifierContainer;

	[Header("Current Turn Enemy Stats")]
	[SerializeField]
	private GameObject currentTurnObject;

	[SerializeField]
	private TMP_Text currentTurnTitle;

	[SerializeField]
	private Transform currentTurnCardHolder;

	[SerializeField]
	private MonsterBaseUI currentTurnCard;

	[Header("Conditions & Effects")]
	[SerializeField]
	private GameObject statusEffects;

	[SerializeField]
	private TMP_Text statusEffectsTitle;

	[SerializeField]
	private GameObject statusEffectsContent;

	[SerializeField]
	private GameObject statusEffectsFrame;

	[SerializeField]
	private GameObject statusEffectPrefab;

	[SerializeField]
	private GameObject statusEffectPermanentPrefab;

	[SerializeField]
	private AutoScrollRect statusEffectsScroller;

	[SerializeField]
	private Transform permanentEffectsContainer;

	[SerializeField]
	private Transform permanentEffectsHolder;

	[SerializeField]
	private Transform positiveEffectsContainer;

	[SerializeField]
	private Transform positiveEffectsHolder;

	[SerializeField]
	private Transform negativeEffectsContainer;

	[SerializeField]
	private Transform negativeEffectsHolder;

	[Header("Active Abilities")]
	[SerializeField]
	private GameObject activeAbilitiesObject;

	[SerializeField]
	private TMP_Text activeAbilitiesTitle;

	[SerializeField]
	private GameObject activeAbilitiesContent;

	[SerializeField]
	private GameObject activeAbilitiesFrame;

	[SerializeField]
	private Transform activeAbilitiesHolder;

	[SerializeField]
	private GameObject activeAbilityPrefab;

	[SerializeField]
	private AutoScrollRect activeAbilityScroll;

	[SerializeField]
	private Transform persistentAbilitiesContainer;

	[SerializeField]
	private Transform persistentAbilitiesHolder;

	[SerializeField]
	private Transform roundAbilitiesContainer;

	[SerializeField]
	private Transform roundAbilitiesHolder;

	[SerializeField]
	private Transform summonedAbilitiesContainer;

	[SerializeField]
	private Transform summonedAbilitiesHolder;

	[SerializeField]
	private Transform attributesContainer;

	[SerializeField]
	private Transform attributesHolder;

	[SerializeField]
	private UIStatusEffect passivePerkPrefab;

	[Header("Spawner")]
	[SerializeField]
	private GameObject spawnerPanel;

	private List<UIMonsterImmunity> monsterImmunities = new List<UIMonsterImmunity>();

	private List<UIOnAttackCondition> onAttackConditions = new List<UIOnAttackCondition>();

	private List<GameObject> permanentEffects = new List<GameObject>();

	private List<UIStatusEffect> positiveEffects = new List<UIStatusEffect>();

	private List<UIStatusEffect> negativeEffects = new List<UIStatusEffect>();

	private List<GameObject> activeAbilities = new List<GameObject>();

	private List<UIScenarioAttackModifier> conditionalAttackModifiersPool = new List<UIScenarioAttackModifier>();

	private bool isGhosted;

	private ISerializable m_SpawnerShown;

	private CActor m_ActorLockedOnPanel;

	[ConsoleVariable(Alias = "scenario_detailed_actor_panel", Persistent = true)]
	private static bool s_IsDetailedActorPanel_kvar;

	public Sprite AvailableCounter => availableCounter;

	public Sprite UsedCounter => usedCounter;

	public GameObject CounterPrefab => counterPrefab;

	public RectTransform PlayerCardsAnchor => playerCardsAnchor;

	public bool IsTitleOnly
	{
		get
		{
			if (!enemyImmunitiesObject.activeSelf && !onAttackConditionsObject.activeSelf && !activeAbilitiesObject.activeSelf && !statusEffects.activeSelf && !currentTurnCardHolder.gameObject.activeSelf)
			{
				return !spawnerPanel.activeSelf;
			}
			return false;
		}
	}

	public CActor ActorShown { get; private set; }

	protected override void AwakeInit()
	{
		base.AwakeInit();
		AttachEvents();
	}

	protected override void AwakenedDestroy()
	{
		base.AwakenedDestroy();
		DettachEvents();
	}

	public void AttachEvents()
	{
		_actorEventsChannel.OnActorHighlighted.AddListener(OnActorHighlighted);
		_actorEventsChannel.OnActorFocused.AddListener(OnActorFocused);
	}

	public void DettachEvents()
	{
		_actorEventsChannel.OnActorHighlighted.RemoveListener(OnActorHighlighted);
		_actorEventsChannel.OnActorFocused.RemoveListener(OnActorFocused);
	}

	private bool CanShow()
	{
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			return false;
		}
		if (Singleton<UIResultsManager>.Instance.IsShown)
		{
			return false;
		}
		return true;
	}

	private void OnActorHighlighted(object sender, CActor actor, bool highlighted, bool showCards)
	{
		if (highlighted)
		{
			CClass overrideClass = null;
			if (sender is InitiativeTrackActorAvatarBase avatar)
			{
				overrideClass = InitiativeTrack.Instance.FindInitiativeTrackActor(avatar)?.Class;
			}
			Show(actor, lockOnScreen: false, showCards, overrideClass);
		}
		else
		{
			HideForActor(actor);
		}
	}

	private void OnActorFocused(object sender, CActor actor, bool focused, bool showCards)
	{
		if (focused)
		{
			CClass overrideClass = null;
			if (sender is InitiativeTrackActorAvatarBase avatar)
			{
				overrideClass = InitiativeTrack.Instance.FindInitiativeTrackActor(avatar)?.Class;
			}
			Show(actor, lockOnScreen: false, showCards, overrideClass);
		}
		else
		{
			HideForActor(actor, unlock: true);
		}
	}

	public void Show(ISerializable spawnerOrProp)
	{
		if (!CanShow())
		{
			return;
		}
		CMonsterClass cMonsterClass = null;
		if (spawnerOrProp is CSpawner cSpawner)
		{
			cMonsterClass = cSpawner.GetNextMonsterClassToSpawn(ScenarioManager.CurrentScenarioState.Players.Count);
		}
		else
		{
			CObjectProp prop = spawnerOrProp as CObjectProp;
			foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
			{
				if (scenarioScript.GetPropData(prop, out var classID))
				{
					cMonsterClass = MonsterClassManager.MonsterAndObjectClassesWithInitiative.FirstOrDefault((CMonsterClass x) => x.ID == classID);
					break;
				}
			}
		}
		if (cMonsterClass != null)
		{
			m_SpawnerShown = spawnerOrProp;
			InitializeMonsterCard(cMonsterClass);
			InitializeConditionsAndEffectsForSpawner(cMonsterClass);
			currentTurnObject.SetActive(value: false);
			characterStatsObject.SetActive(value: false);
			activeAbilitiesObject.SetActive(value: false);
			isGhosted = true;
			spawnerPanel.gameObject.SetActive(value: true);
			window.Show();
		}
	}

	public void Show(CActor actor, bool lockOnScreen = false, bool showCards = false, CClass overrideClass = null)
	{
		if (!CanShow())
		{
			return;
		}
		if (actor == null)
		{
			Log.GUIError("Trying to show the stat panel but actor provided is null.");
		}
		else
		{
			if (actor.SkipVisualsAndGUI || (actor is CObjectActor { IsAttachedToProp: not false } cObjectActor && !cObjectActor.AttachedProp.HasInitiative))
			{
				return;
			}
			playerCardsAnchor.anchoredPosition = (showCards ? Vector2.zero : (hiddenCardsOffset * Vector2.up));
			Hide();
			ActorShown = actor;
			WorldspaceUITools.Instance.EnableHoveredOutline(actor);
			if (lockOnScreen)
			{
				m_ActorLockedOnPanel = actor;
			}
			spawnerPanel.gameObject.SetActive(value: false);
			isGhosted = false;
			if (actor is CEnemyActor enemyActor)
			{
				InitializeMonsterCard(enemyActor, overrideClass as CMonsterClass);
				SpawnCurrentTurnCard(enemyActor, overrideClass as CMonsterClass);
				characterStatsObject.SetActive(value: false);
				activeAbilitiesObject.SetActive(value: false);
				bool flag = actor is CObjectActor cObjectActor2 && cObjectActor2.AttachedProp != null;
				currentTurnObject.SetActive(!flag || (actor as CObjectActor).AttachedProp.HasInitiative);
				bool active = s_IsDetailedActorPanel_kvar && !flag;
				ModifierObject.SetActive(active);
			}
			else
			{
				if (actor is CHeroSummonActor heroSummonActor)
				{
					InitializeHeroSummonCard(heroSummonActor);
					characterStatsObject.SetActive(value: false);
				}
				else
				{
					InitializeCharacterCard(actor);
					enemyStatsObject.SetActive(value: false);
				}
				currentTurnObject.SetActive(value: false);
				ModifierObject.SetActive(s_IsDetailedActorPanel_kvar);
			}
			InitializeActiveAbilities(actor);
			InitializeConditionsAndEffects(actor);
			window.Show();
		}
	}

	private static void SetHpBar(FHImageFillElement bar, int health, int maxHealth)
	{
		if (!(bar == null))
		{
			float progress = ((maxHealth == 0) ? 1f : ((float)health / (float)maxHealth));
			bar.SetProgress(progress);
		}
	}

	private void InitializeCharacterCard(CActor actor)
	{
		characterStatsObject.SetActive(value: true);
		characterStats.Init(actor as CPlayerActor);
		characterLifeAmount.text = $"{actor.Health}/{actor.MaxHealth}";
		SetHpBar(characterHpBar, actor.Health, actor.MaxHealth);
		CPersonalQuestState cPersonalQuestState = ((AdventureState.MapState?.MapParty == null || !AdventureState.MapState.IsCampaign) ? null : AdventureState.MapState.MapParty.SelectedCharacters.ToList().FirstOrDefault((CMapCharacter x) => x != null && x.CharacterID == actor.Class.ID)?.PersonalQuest);
		if (!s_IsDetailedActorPanel_kvar || cPersonalQuestState == null || (cPersonalQuestState.IsConcealed && FFSNetwork.IsOnline && !actor.IsUnderMyControl()))
		{
			personalQuestContainer.SetActive(value: false);
		}
		else
		{
			personalQuestContainer.SetActive(value: true);
			personalQuestProgress.SetPersonalQuest(cPersonalQuestState);
		}
		if (s_IsDetailedActorPanel_kvar && (!FFSNetwork.IsOnline || actor.IsUnderMyControl()))
		{
			CBattleGoalState cBattleGoalState = AdventureState.MapState?.CurrentMapScenarioState?.GetChosenBattleGoal(actor.Class.ID);
			if (cBattleGoalState != null)
			{
				battleGoalProgress.SetBattleGoal(cBattleGoalState);
				battleGoalContainer.SetActive(value: true);
			}
			else
			{
				battleGoalContainer.SetActive(value: false);
			}
		}
		else
		{
			battleGoalContainer.SetActive(value: false);
		}
		CCharacterClass charClass = (CCharacterClass)actor.Class;
		if (s_IsDetailedActorPanel_kvar && AdventureState.MapState != null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			goldCounter.gameObject.SetActive(value: true);
			goldCounter.SetCount(actor.Gold);
		}
		else
		{
			goldCounter.gameObject.SetActive(value: false);
		}
		ShowConditionalModifiers(charClass);
		characterMissMod.SetupCounters("*0", actor, charClass);
		characterMinusTwoMod.SetupCounters("-2", actor, charClass);
		characterMinusOneMod.SetupCounters("-1", actor, charClass);
		characterZeroMod.SetupCounters("+0", actor, charClass);
		characterPlusOneMod.SetupCounters("+1", actor, charClass);
		characterPlusTwoMod.SetupCounters("+2", actor, charClass);
		characterTimesTwoMod.SetupCounters("*2", actor, charClass);
	}

	private void ShowConditionalModifiers(CCharacterClass charClass)
	{
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in charClass.AttackModifierCards.Concat(charClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, modifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(modifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), charClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
	}

	private void InitializeMonsterCard(CMonsterClass enemyClass)
	{
		InitializeMonsterClass(enemyClass, enemyClass.StatLevel, enemyClass.Health(), enemyClass.Health());
	}

	private void InitializeMonsterCard(CEnemyActor enemyActor, CMonsterClass overrideClass = null)
	{
		string titleKey = enemyActor.Type switch
		{
			CActor.EType.Ally => "GUI_ALLY", 
			CActor.EType.Neutral => "GUI_NEUTRAL", 
			CActor.EType.Enemy2 => "GUI_ENEMY2", 
			_ => "GUI_MONSTER_CARD", 
		};
		string overrideLocKey = string.Empty;
		string overrideCharacterPortrait = string.Empty;
		if (enemyActor is CObjectActor cObjectActor)
		{
			titleKey = "GUI_OBJECT_WITH_HEALTH";
			if (cObjectActor.IsAttachedToProp)
			{
				overrideLocKey = (string.IsNullOrEmpty(cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey) ? cObjectActor.AttachedProp.PrefabName : cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey);
				overrideCharacterPortrait = (string.IsNullOrEmpty(cObjectActor.AttachedProp.PropHealthDetails.ActorSpriteName) ? string.Empty : cObjectActor.AttachedProp.PropHealthDetails.ActorSpriteName);
			}
		}
		if (!string.IsNullOrEmpty(enemyActor.CustomLocKey))
		{
			overrideLocKey = enemyActor.CustomLocKey;
		}
		string empty = string.Empty;
		InitializeMonsterClass(overrideClass ?? enemyActor.MonsterClass, enemyActor.Level, enemyActor.Health, enemyActor.MaxHealth, titleKey, empty, enemyActor, overrideLocKey, overrideCharacterPortrait);
	}

	private void InitializeMonsterClass(CMonsterClass monsterClass, int level, int health, int maxHealth, string titleKey = "GUI_MONSTER_CARD", string suffixName = "", CEnemyActor enemyActor = null, string overrideLocKey = null, string overrideCharacterPortrait = null)
	{
		string term = (string.IsNullOrEmpty(overrideLocKey) ? monsterClass.LocKey : overrideLocKey);
		string actorModel = (string.IsNullOrEmpty(overrideCharacterPortrait) ? monsterClass.DefaultModel : overrideCharacterPortrait);
		if (enemyActor != null && !string.IsNullOrEmpty(enemyActor.OverrideCharacterModel))
		{
			actorModel = enemyActor.OverrideCharacterModel.ToString();
		}
		enemyStatsObject.SetActive(value: true);
		onAttackConditionsObject.SetActive(value: false);
		bool active = s_IsDetailedActorPanel_kvar && enemyActor != null && !enemyActor.IsScenarioActor;
		enemyCard.SetActive(active);
		enemyCardTitle.text = LocalizationManager.GetTranslation(titleKey);
		enemyLevel.text = level.ToString();
		string translation = LocalizationManager.GetTranslation(term);
		enemyName.text = translation + suffixName;
		enemyPortrait.sprite = UIInfoTools.Instance.GetActorPortrait(actorModel);
		InitStatValues(health, maxHealth, monsterClass.Move, monsterClass.Attack, monsterClass.Range, monsterClass.StatIsBasedOnXEntries);
		CMonsterAttackModifierDeck enemyMonsterAttackModifierDeck = MonsterClassManager.EnemyMonsterAttackModifierDeck;
		if (enemyActor != null)
		{
			MonsterClassManager.GetAttackModifiersDeck(enemyActor);
		}
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in enemyMonsterAttackModifierDeck.AttackModifierCards.Concat(enemyMonsterAttackModifierDeck.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, enemyModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(enemyModifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), enemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
		enemyMissMod.SetupCounters("*0", enemyActor);
		enemyMinusTwoMod.SetupCounters("-2", enemyActor);
		enemyMinusOneMod.SetupCounters("-1", enemyActor);
		enemyZeroMod.SetupCounters("+0", enemyActor);
		enemyPlusOneMod.SetupCounters("+1", enemyActor);
		enemyPlusTwoMod.SetupCounters("+2", enemyActor);
		enemyTimesTwoMod.SetupCounters("*2", enemyActor);
		List<CAbility.EAbilityType> immunityNames = ((enemyActor != null) ? enemyActor.Immunities.ToList() : monsterClass.Immunities.ToList());
		if (enemyPetCaptureInfo != null)
		{
			enemyPetCaptureInfo.SetActive(enemyActor != null && enemyActor.IsCapturable && ScenarioManager.Scenario.AllPlayers.Any((CPlayerActor x) => x.Inventory.AllItems.Any((CItem y) => y.FindAbilityOnItemCard(CAbility.EAbilityType.Capture) != null)));
		}
		if (immunityNames.Count > 0 && (enemyActor == null || !enemyActor.IsScenarioActor))
		{
			enemyImmunitiesObject.SetActive(value: true);
			monsterImmunities.ForEach((UIMonsterImmunity x) =>
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			monsterImmunities.Clear();
			if (CActor.ImmuneToAllConditionsCheck(immunityNames))
			{
				immunityNames = immunityNames.Except(CActor.ImmunityConditionAbilityTypes).ToList();
				UIMonsterImmunity component = UnityEngine.Object.Instantiate(uiMonsterImmunityPrefab, enemyImmunitiesContentHolder, worldPositionStays: false).GetComponent<UIMonsterImmunity>();
				component.Initialize("AllConditions");
				monsterImmunities.Add(component);
			}
			if (monsterClass?.CurrentMonsterStat?.ImmuneToForcedMovement ?? CActor.ImmunityForcedMovementAbilityTypes.All((CAbility.EAbilityType i) => immunityNames.Contains(i)))
			{
				immunityNames = immunityNames.Except(UICharacterUtils.MovementImmunities).ToList();
				UIMonsterImmunity component2 = UnityEngine.Object.Instantiate(uiMonsterImmunityPrefab, enemyImmunitiesContentHolder, worldPositionStays: false).GetComponent<UIMonsterImmunity>();
				component2.Initialize("AA_Immovable");
				monsterImmunities.Add(component2);
			}
			foreach (CAbility.EAbilityType item in immunityNames)
			{
				if (UICharacterUtils.AllowedImmunities.Contains(item) && item != CAbility.EAbilityType.ControlActor)
				{
					UIMonsterImmunity component3 = UnityEngine.Object.Instantiate(uiMonsterImmunityPrefab, enemyImmunitiesContentHolder, worldPositionStays: false).GetComponent<UIMonsterImmunity>();
					component3.Initialize(item.ToString());
					monsterImmunities.Add(component3);
				}
			}
			enemyImmunitiesScroller.StartAutoscroll();
		}
		else
		{
			enemyImmunitiesScroller.StopAutoscroll();
			enemyImmunitiesObject.SetActive(value: false);
		}
	}

	private void InitializeHeroSummonCard(CHeroSummonActor heroSummonActor)
	{
		enemyStatsObject.SetActive(value: true);
		enemyImmunitiesObject.SetActive(value: false);
		enemyCardTitle.text = LocalizationManager.GetTranslation("GUI_SUMMON_CARD");
		string text = "<color=#" + ColorUtility.ToHtmlStringRGB(UIInfoTools.DefInstance.greyedOutTextColor) + ">" + Choreographer.GetActorIDForCombatLogIfNeeded(heroSummonActor) + "</color>";
		enemyName.text = LocalizationManager.GetTranslation(heroSummonActor.ActorLocKey()) + text;
		TMP_Text tMP_Text = enemyLevel;
		int strength = heroSummonActor.Level;
		tMP_Text.text = strength.ToString();
		enemyPortrait.sprite = UIInfoTools.Instance.GetActorPortrait(heroSummonActor.GetPrefabName());
		if (enemyPetCaptureInfo != null)
		{
			enemyPetCaptureInfo.SetActive(value: false);
		}
		HeroSummonYMLData summonData = heroSummonActor.SummonData;
		CAbilityAttack.GetAttackBonuses(heroSummonActor, out strength, out var range, out var _, out var _, out var negativeConditions);
		InitStatValues(heroSummonActor.Health, heroSummonActor.MaxHealth, summonData.GetMoveFromActor(heroSummonActor), summonData.GetAttackFromActor(heroSummonActor), summonData.Range + range, summonData.StatIsBasedOnXEntries);
		CCharacterClass characterClass = heroSummonActor.Summoner.CharacterClass;
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in characterClass.AttackModifierCards.Concat(characterClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, enemyModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(enemyModifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), characterClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
		enemyMissMod.SetupCounters("*0", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyMinusTwoMod.SetupCounters("-2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyMinusOneMod.SetupCounters("-1", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyZeroMod.SetupCounters("+0", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyPlusOneMod.SetupCounters("+1", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyPlusTwoMod.SetupCounters("+2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyTimesTwoMod.SetupCounters("*2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		negativeConditions = heroSummonActor?.SummonData?.OnAttackConditions;
		if (negativeConditions == null || negativeConditions.Count <= 0)
		{
			if (heroSummonActor != null)
			{
				HeroSummonYMLData summonData2 = heroSummonActor.SummonData;
				if (summonData2 != null && summonData2.AttackInfuse.HasValue)
				{
					goto IL_0390;
				}
			}
			onAttackConditionsScroller.StopAutoscroll();
			onAttackConditionsObject.SetActive(value: false);
			return;
		}
		goto IL_0390;
		IL_0390:
		onAttackConditionsObject.SetActive(value: true);
		if ((bool)onAttackConditionsTitle)
		{
			onAttackConditionsTitle.text = LocalizationManager.GetTranslation("GUI_ON_ATTACK_CONDITIONS_TITLE");
		}
		onAttackConditions.ForEach((UIOnAttackCondition x) =>
		{
			UnityEngine.Object.Destroy(x.gameObject);
		});
		onAttackConditions.Clear();
		if (heroSummonActor?.SummonData?.OnAttackConditions != null)
		{
			foreach (CCondition.ENegativeCondition item in heroSummonActor?.SummonData?.OnAttackConditions)
			{
				UIOnAttackCondition component = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
				component.Initialize(item);
				onAttackConditions.Add(component);
			}
		}
		if (heroSummonActor != null)
		{
			HeroSummonYMLData summonData3 = heroSummonActor.SummonData;
			if (summonData3 != null && summonData3.AttackInfuse.HasValue)
			{
				UIOnAttackCondition component2 = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
				component2.Initialize(CCondition.ENegativeCondition.NA, (heroSummonActor?.SummonData?.AttackInfuse).Value);
				onAttackConditions.Add(component2);
			}
		}
		if (heroSummonActor != null && heroSummonActor.SummonData?.AdjacentAttackOnDeath > 0)
		{
			string onDeath = string.Format(LocalizationManager.GetTranslation("GUI_ON_DEATH_ADJACENT"), heroSummonActor.SummonData.AdjacentAttackOnDeath);
			UIOnAttackCondition component3 = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
			component3.Initialize(CCondition.ENegativeCondition.NA, null, onDeath);
			onAttackConditions.Add(component3);
		}
		onAttackConditionsScroller.StartAutoscroll();
	}

	private void InitStatValues(int health, int maxHealth, int move, int attack, int range, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null)
	{
		enemyHealthAmount.text = health + "/" + maxHealth;
		SetHpBar(enemyHpBar, health, maxHealth);
		if (move > 0 || (statIsBasedOnXEntries != null && statIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Move)))
		{
			enemyMovementAmount.text = move.ToString();
			Image image = enemyMovementIcon;
			TMP_Text tMP_Text = enemyMovementName;
			Color color = (enemyMovementAmount.color = UIInfoTools.DefInstance.neutralActionTextColor);
			Color color2 = (tMP_Text.color = color);
			image.color = color2;
		}
		else
		{
			enemyMovementAmount.text = "-";
			Image image2 = enemyMovementIcon;
			TMP_Text tMP_Text2 = enemyMovementName;
			Color color = (enemyMovementAmount.color = UIInfoTools.DefInstance.greyedOutTextColor);
			Color color2 = (tMP_Text2.color = color);
			image2.color = color2;
		}
		bool flag = statIsBasedOnXEntries?.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Attack) ?? false;
		if ((attack > 0) | flag)
		{
			enemyAttackAmount.text = attack.ToString();
			Image image3 = enemyAttackIcon;
			TMP_Text tMP_Text3 = enemyAttackName;
			Color color = (enemyAttackAmount.color = UIInfoTools.DefInstance.neutralActionTextColor);
			Color color2 = (tMP_Text3.color = color);
			image3.color = color2;
		}
		else
		{
			enemyAttackAmount.text = "-";
			Image image4 = enemyAttackIcon;
			TMP_Text tMP_Text4 = enemyAttackName;
			Color color = (enemyAttackAmount.color = UIInfoTools.DefInstance.greyedOutTextColor);
			Color color2 = (tMP_Text4.color = color);
			image4.color = color2;
		}
		if (range > 1)
		{
			enemyRangeAmount.text = range.ToStringWithInfinity();
			Image image5 = enemyRangeIcon;
			TMP_Text tMP_Text5 = enemyRangeName;
			Color color = (enemyRangeAmount.color = UIInfoTools.DefInstance.neutralActionTextColor);
			Color color2 = (tMP_Text5.color = color);
			image5.color = color2;
		}
		else
		{
			enemyRangeAmount.text = "-";
			Image image6 = enemyRangeIcon;
			TMP_Text tMP_Text6 = enemyRangeName;
			Color color = (enemyRangeAmount.color = UIInfoTools.DefInstance.greyedOutTextColor);
			Color color2 = (tMP_Text6.color = color);
			image6.color = color2;
		}
	}

	private void SpawnCurrentTurnCard(CEnemyActor enemyActor, CMonsterClass overrideClass = null)
	{
		currentTurnObject.SetActive(value: true);
		CMonsterAbilityCard cMonsterAbilityCard = (overrideClass ?? enemyActor.MonsterClass)?.RoundAbilityCard;
		if (overrideClass == null && enemyActor.OverrideRoundAbilityCard != null)
		{
			cMonsterAbilityCard = enemyActor.OverrideRoundAbilityCard;
		}
		if (cMonsterAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			currentTurnCardHolder.gameObject.SetActive(value: true);
			if ((bool)currentTurnTitle)
			{
				currentTurnTitle.color = UIInfoTools.DefInstance.basicTextColor;
			}
			currentTurnCard.GenerateCard(cMonsterAbilityCard, enemyActor);
		}
		else
		{
			currentTurnCardHolder.gameObject.SetActive(value: false);
			if ((bool)currentTurnTitle)
			{
				currentTurnTitle.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
		}
	}

	private void InitializeConditionsAndEffects(CActor actor)
	{
		if (actor.IsScenarioActor)
		{
			statusEffects.SetActive(value: false);
			return;
		}
		List<CCondition.ENegativeCondition> scenarioModifierAddConditionToAttacks = GetScenarioModifierAddConditionToAttacks(actor);
		Dictionary<string, string> monsterOrSummonBaseStats = PersistentAbilitiesUI.GetMonsterOrSummonBaseStats(actor, scenarioModifierAddConditionToAttacks);
		bool flag = actor is CEnemyActor && actor.MindControlDuration != CAbilityControlActor.EControlDurationType.None;
		bool flag2 = (monsterOrSummonBaseStats.Count > 0) | flag;
		bool flag3 = actor.Tokens.CheckPositiveTokens.Count > 0;
		bool flag4 = actor.Tokens.CheckNegativeTokens.Count > 0;
		if (flag2 | flag3 | flag4)
		{
			statusEffects.SetActive(value: true);
			if ((bool)statusEffectsContent)
			{
				statusEffectsContent.SetActive(value: true);
			}
			if ((bool)statusEffectsFrame)
			{
				statusEffectsFrame.SetActive(value: true);
			}
			if ((bool)statusEffectsTitle)
			{
				statusEffectsTitle.color = UIInfoTools.DefInstance.basicTextColor;
			}
			if (flag2)
			{
				permanentEffectsContainer.gameObject.SetActive(value: true);
				permanentEffects.ForEach((GameObject x) =>
				{
					UnityEngine.Object.Destroy(x);
				});
				permanentEffects.Clear();
				foreach (KeyValuePair<string, string> item in monsterOrSummonBaseStats)
				{
					UIStatusEffect component = UnityEngine.Object.Instantiate(statusEffectPermanentPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component.Initialize("$Innate$", UIInfoTools.Instance.GetActiveAbilityIcon(item.Value), UIInfoTools.DefInstance.basicTextColor, CreateLayout.LocaliseText(item.Key));
					component.GetComponent<ContentSizeFitter>().enabled = false;
					permanentEffects.Add(component.gameObject);
				}
				if (flag)
				{
					string customEffectDescription = string.Empty;
					if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction)
					{
						customEffectDescription = "$GUI_MIND_CONTROL_ACTION$";
					}
					else if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneTurn)
					{
						customEffectDescription = "$GUI_MIND_CONTROL_TURN$";
					}
					UIStatusEffect component2 = UnityEngine.Object.Instantiate(statusEffectPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component2.Initialize("$GUI_MIND_CONTROL$", UIInfoTools.Instance.GetActiveAbilityIcon("MindControl"), UIInfoTools.DefInstance.basicTextColor, customEffectDescription);
					component2.GetComponent<ContentSizeFitter>().enabled = false;
					permanentEffects.Add(component2.gameObject);
				}
			}
			else
			{
				permanentEffectsContainer.gameObject.SetActive(value: false);
			}
			if (flag3)
			{
				positiveEffectsContainer.gameObject.SetActive(value: true);
				positiveEffects.ForEach((UIStatusEffect x) =>
				{
					UnityEngine.Object.Destroy(x.gameObject);
				});
				positiveEffects.Clear();
				foreach (CCondition.EPositiveCondition allPositiveCondition in actor.Tokens.GetAllPositiveConditions())
				{
					UIStatusEffect component3 = UnityEngine.Object.Instantiate(statusEffectPrefab, positiveEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component3.Initialize(allPositiveCondition.ToString(), UIInfoTools.DefInstance.positiveStatusEffectColor);
					component3.GetComponent<ContentSizeFitter>().enabled = false;
					positiveEffects.Add(component3);
				}
			}
			else
			{
				positiveEffectsContainer.gameObject.SetActive(value: false);
			}
			if (flag4)
			{
				negativeEffectsContainer.gameObject.SetActive(value: true);
				negativeEffects.ForEach((UIStatusEffect x) =>
				{
					UnityEngine.Object.Destroy(x.gameObject);
				});
				negativeEffects.Clear();
				foreach (CCondition.ENegativeCondition allNegativeCondition in actor.Tokens.GetAllNegativeConditions())
				{
					UIStatusEffect component4 = UnityEngine.Object.Instantiate(statusEffectPrefab, negativeEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component4.Initialize(allNegativeCondition.ToString(), UIInfoTools.DefInstance.negativeStatusEffectColor);
					component4.GetComponent<ContentSizeFitter>().enabled = false;
					negativeEffects.Add(component4);
				}
			}
			else
			{
				negativeEffectsContainer.gameObject.SetActive(value: false);
			}
			statusEffectsScroller.StartAutoscroll();
		}
		else
		{
			statusEffects.SetActive(value: false);
			statusEffectsScroller.StopAutoscroll();
			if ((bool)statusEffectsContent)
			{
				statusEffectsContent.SetActive(value: false);
			}
			if ((bool)statusEffectsFrame)
			{
				statusEffectsFrame.SetActive(value: false);
			}
			if ((bool)statusEffectsTitle)
			{
				statusEffectsTitle.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
			permanentEffectsContainer.gameObject.SetActive(value: false);
			positiveEffectsContainer.gameObject.SetActive(value: false);
			negativeEffectsContainer.gameObject.SetActive(value: false);
		}
	}

	private void InitializeConditionsAndEffectsForSpawner(CMonsterClass monsterClass)
	{
		Dictionary<string, string> dictionary = PersistentAbilitiesUI.BuildMonsterBaseStatsDict(monsterClass.CurrentMonsterStat, monsterClass.OriginalBaseStats);
		positiveEffectsContainer.gameObject.SetActive(value: false);
		negativeEffectsContainer.gameObject.SetActive(value: false);
		if (dictionary.Count > 0)
		{
			if ((bool)statusEffectsContent)
			{
				statusEffectsContent.SetActive(value: true);
			}
			if ((bool)statusEffectsFrame)
			{
				statusEffectsFrame.SetActive(value: true);
			}
			if ((bool)statusEffectsTitle)
			{
				statusEffectsTitle.color = UIInfoTools.DefInstance.basicTextColor;
			}
			permanentEffectsContainer.gameObject.SetActive(value: true);
			permanentEffects.ForEach((GameObject x) =>
			{
				UnityEngine.Object.Destroy(x);
			});
			permanentEffects.Clear();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				UIStatusEffect component = UnityEngine.Object.Instantiate(statusEffectPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component.Initialize("$Innate$", UIInfoTools.Instance.GetActiveAbilityIcon(item.Value), UIInfoTools.DefInstance.basicTextColor, CreateLayout.LocaliseText(item.Key));
				component.GetComponent<ContentSizeFitter>().enabled = false;
				permanentEffects.Add(component.gameObject);
			}
			statusEffectsScroller.StartAutoscroll();
		}
		else
		{
			permanentEffectsContainer.gameObject.SetActive(value: false);
			statusEffectsScroller.StopAutoscroll();
			if ((bool)statusEffectsContent)
			{
				statusEffectsContent.SetActive(value: false);
			}
			if ((bool)statusEffectsFrame)
			{
				statusEffectsFrame.SetActive(value: false);
			}
			if ((bool)statusEffectsTitle)
			{
				statusEffectsTitle.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
		}
	}

	private void InitializeActiveAbilities(CActor actor)
	{
		List<CActiveBonus> list = (from x in GetAllActiveBonuses(actor)
			where !x.Ability.ActiveBonusData.Hidden && (x.Caster == actor || !x.Ability.ActiveBonusData.HiddenWhenAura) && !x.IsHiddenInUI() && !x.Ability.GetMiscVar("DontShowActiveBonusInActorStatPanel").GetBool()
			select x).ToList();
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			List<Guid> onSummonAbilitiesIDs = cHeroSummonActor?.SummonData?.OnSummonAbilities?.Select((CAbility osa) => osa.ID).ToList();
			if (onSummonAbilitiesIDs != null)
			{
				list.RemoveAll((CActiveBonus ab) => onSummonAbilitiesIDs.Contains(ab.Ability.ID));
			}
		}
		List<CharacterPerk> activePassivePerks = GetActivePassivePerks(actor);
		List<Tuple<string, string, Sprite>> passiveAttributes = GetPassiveAttributes(actor);
		if (!actor.IsScenarioActor && (list.Count > 0 || (activePassivePerks != null && activePassivePerks.Count > 0) || !passiveAttributes.IsNullOrEmpty()))
		{
			activeAbilitiesObject.SetActive(value: true);
			if ((bool)activeAbilitiesContent)
			{
				activeAbilitiesContent.SetActive(value: true);
			}
			if ((bool)activeAbilitiesFrame)
			{
				activeAbilitiesFrame.SetActive(value: true);
			}
			if ((bool)activeAbilitiesTitle)
			{
				activeAbilitiesTitle.color = UIInfoTools.DefInstance.basicTextColor;
			}
			activeAbilities.ForEach((GameObject x) =>
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			activeAbilities.Clear();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int num4 = 0; num4 < activePassivePerks.Count; num4++)
			{
				PerkDTO perkDTO = new PerkDTO(activePassivePerks[num4]);
				UIStatusEffect component = UnityEngine.Object.Instantiate(passivePerkPrefab, persistentAbilitiesHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component.Initialize(perkDTO.NameLocalized, perkDTO.Icon, UIInfoTools.DefInstance.basicTextColor, perkDTO.DescriptionLocalized);
				activeAbilities.Add(component.gameObject);
				num++;
			}
			for (int num5 = 0; num5 < passiveAttributes.Count; num5++)
			{
				UIStatusEffect component2 = UnityEngine.Object.Instantiate(statusEffectPrefab, attributesHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component2.Initialize(LocalizationManager.GetTranslation(passiveAttributes[num5].Item1), passiveAttributes[num5].Item3, UIInfoTools.DefInstance.basicTextColor, LocalizationManager.GetTranslation(passiveAttributes[num5].Item2));
				activeAbilities.Add(component2.gameObject);
			}
			foreach (IGrouping<string, CActiveBonus> item in from x in list
				group x by x.BaseCard.Name)
			{
				foreach (IGrouping<Guid, CActiveBonus> item2 in from x in item
					group x by x.Ability.ID)
				{
					CActiveBonus cActiveBonus = item2.FirstOrDefault();
					if (cActiveBonus != null && (!(cActiveBonus.Ability is CAbilityImmunityTo) || !CActor.ImmuneToAllConditionsCheck(((cActiveBonus as CImmunityActiveBonus)?.Ability as CAbilityImmunityTo)?.ImmuneToAbilityTypes)))
					{
						Transform parent = activeAbilitiesHolder;
						if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Persistent)
						{
							parent = persistentAbilitiesHolder;
							num++;
						}
						else if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Round)
						{
							parent = roundAbilitiesHolder;
							num2++;
						}
						else if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
						{
							parent = summonedAbilitiesHolder;
							num3++;
						}
						UIActiveAbility component3 = UnityEngine.Object.Instantiate(activeAbilityPrefab, parent, worldPositionStays: false).GetComponent<UIActiveAbility>();
						int instances = ((cActiveBonus.Type() == CAbility.EAbilityType.Summon || cActiveBonus.IsDoom) ? 1 : item2.Count());
						component3.Initialize(cActiveBonus, actor, instances);
						activeAbilities.Add(component3.gameObject);
						if (cActiveBonus.IsDoom)
						{
							break;
						}
					}
				}
			}
			persistentAbilitiesContainer.gameObject.SetActive(num > 0);
			roundAbilitiesContainer.gameObject.SetActive(num2 > 0);
			summonedAbilitiesContainer.gameObject.SetActive(num3 > 0);
			attributesContainer.gameObject.SetActive(!passiveAttributes.IsNullOrEmpty());
			activeAbilityScroll.StartAutoscroll();
		}
		else
		{
			activeAbilityScroll.StopAutoscroll();
			activeAbilitiesObject.SetActive(value: false);
			if ((bool)activeAbilitiesContent)
			{
				activeAbilitiesContent.SetActive(value: false);
			}
			if ((bool)activeAbilitiesFrame)
			{
				activeAbilitiesFrame.SetActive(value: false);
			}
			if ((bool)activeAbilitiesTitle)
			{
				activeAbilitiesTitle.color = UIInfoTools.DefInstance.greyedOutTextColor;
			}
			persistentAbilitiesContainer.gameObject.SetActive(value: false);
			roundAbilitiesContainer.gameObject.SetActive(value: false);
			summonedAbilitiesContainer.gameObject.SetActive(value: false);
			attributesContainer.gameObject.SetActive(value: false);
		}
	}

	private List<CCondition.ENegativeCondition> GetScenarioModifierAddConditionToAttacks(CActor actor)
	{
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		foreach (CScenarioModifierAddConditionsToAbilities item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities))
		{
			if (item.ShouldAddConditions(actor, CAbility.EAbilityType.Attack))
			{
				list.AddRange(item.NegativeConditions);
			}
		}
		return list.Distinct().ToList();
	}

	private List<CharacterPerk> GetActivePassivePerks(CActor actor)
	{
		if (!(actor is CPlayerActor) || AdventureState.MapState?.MapParty == null)
		{
			return new List<CharacterPerk>();
		}
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == actor.Class.ID);
		if (cMapCharacter != null)
		{
			return cMapCharacter.GetActivePassivePerks();
		}
		return new List<CharacterPerk>();
	}

	private List<Tuple<string, string, Sprite>> GetPassiveAttributes(CActor actor)
	{
		List<Tuple<string, string, Sprite>> list = new List<Tuple<string, string, Sprite>>();
		if (!(actor is CPlayerActor cPlayerActor) || cPlayerActor.CarriedQuestItems.Count == 0)
		{
			return list;
		}
		ActiveAbilityDataSO.QuestItemActiveBonusData questItem = default;
		for (int i = 0; i < cPlayerActor.CarriedQuestItems.Count; i++)
		{
			CObjectProp cObjectProp = cPlayerActor.CarriedQuestItems[i];
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem && UIInfoTools.Instance?.ActiveAbilityData?.GetQuestItemData(cObjectProp.SpecificPropType, out questItem) == true)
			{
				string translation = LocalizationManager.GetTranslation(questItem.OverrideBonusName.IsNullOrEmpty() ? FHUILocFormatting.GetScenarioQuestItemName(cObjectProp) : questItem.OverrideBonusName);
				string translation2 = LocalizationManager.GetTranslation(questItem.OverrideBonusDescription.IsNullOrEmpty() ? FHUILocFormatting.GetScenarioQuestItemDesc(cObjectProp) : questItem.OverrideBonusDescription);
				Sprite item = questItem.Icon ?? UIInfoTools.Instance.CarryableQuestItemIconSprite;
				list.Add(new Tuple<string, string, Sprite>(translation, translation2, item));
			}
		}
		return list;
	}

	private List<CActiveBonus> GetAllActiveBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		list.AddRange(actor.ActiveBonuses);
		return list;
	}

	public void HideForActor(CActor actor = null, bool unlock = false)
	{
		if (unlock)
		{
			m_ActorLockedOnPanel = null;
		}
		if ((m_ActorLockedOnPanel == null || m_ActorLockedOnPanel != actor) && m_SpawnerShown == null && (actor == null || actor == ActorShown))
		{
			if (m_ActorLockedOnPanel != null)
			{
				Show(m_ActorLockedOnPanel);
			}
			else
			{
				Hide();
			}
		}
	}

	public void HideForSpawner(ISerializable spawnerOrProp = null)
	{
		if (ActorShown == null && (spawnerOrProp == null || spawnerOrProp == m_SpawnerShown))
		{
			if (m_ActorLockedOnPanel != null)
			{
				Show(m_ActorLockedOnPanel);
			}
			else
			{
				Hide();
			}
		}
	}

	private void Hide()
	{
		if (ActorShown != null)
		{
			WorldspaceUITools.Instance.DisableHoveredOutline(ActorShown);
		}
		ActorShown = null;
		m_SpawnerShown = null;
		activeAbilityScroll.StopAutoscroll();
		statusEffectsScroller.StopAutoscroll();
		enemyImmunitiesScroller.StopAutoscroll();
		onAttackConditionsScroller.StopAutoscroll();
		window.Hide();
	}
}

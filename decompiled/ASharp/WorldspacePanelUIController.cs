using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;
using WorldspaceUI;

public class WorldspacePanelUIController : WorldspaceDisplayPanelBase, IPooleable
{
	[SerializeField]
	public WorldspaceCharacterInfoPanel InfoUI;

	[SerializeField]
	private HealthBar m_HealthBar;

	[SerializeField]
	private EffectsBar m_EffectsBar;

	[SerializeField]
	private ShieldBar m_ShieldBar;

	[SerializeField]
	private AttackModBar m_AttackModBar;

	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	private EffectsBar m_PreviewEffectsBar;

	[SerializeField]
	private UITextTooltipTarget m_Tooltip;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_UnfocusedOpacity = 0.5f;

	[SerializeField]
	private Color m_PlayerHealthColor = Color.green;

	[SerializeField]
	private Color m_EnemyHealthColor = Color.red;

	[SerializeField]
	private Color m_NeutralHealthColor = Color.cyan;

	[SerializeField]
	private Color m_Enemy2HealthColor = Color.blue;

	[SerializeField]
	private Color m_AllyMonsterHealthColor = Color.blue;

	[SerializeField]
	private Color m_HeroSummonHealthColor = Color.blue;

	[SerializeField]
	private Sprite defaultHealthIcon;

	[SerializeField]
	private Sprite eliteHealthIcon;

	[SerializeField]
	private Sprite bossHealthIcon;

	public Vector2 m_BossScaleIncrease = Vector2.one;

	[SerializeField]
	private Toggle m_SelectionIndicador;

	[SerializeField]
	private float m_HiddenPreviewEffectPanelPositionY = 48f;

	[SerializeField]
	private GUIAnimator addTargetAnimation;

	private GameObject CharacterToTrack;

	private Outlinable Outlinable;

	private HashSet<string> focusedRequests = new HashSet<string>();

	private Vector2? shownPreviewEffectPosition;

	private static List<CActor> m_SelectedActorCacheToAdd = new List<CActor>();

	private static List<CActor> m_SelectedActorCacheToRemove = new List<CActor>();

	private static List<CActor> m_SelectedActorCache = new List<CActor>();

	private static List<CActor> m_TargetedActorCachePositiveToAdd = new List<CActor>();

	private static List<CActor> m_TargetedActorCacheNegativeToAdd = new List<CActor>();

	private static List<CActor> m_TargetedActorCacheToRemove = new List<CActor>();

	private static List<CActor> m_TargetedActorCachePositive = new List<CActor>();

	private static List<CActor> m_TargetedActorCacheNegative = new List<CActor>();

	private int retaliateDisplayed;

	public static object PreviewEffectsLock = new object();

	public Vector3 TargetPosition => CharacterToTrack.transform.position;

	public HealthBar HealthBar => m_HealthBar;

	public EffectsBar EffectBar => m_EffectsBar;

	public EffectsBar PreviewBar => m_PreviewEffectsBar;

	public Color AllegianceColor
	{
		get
		{
			switch ((m_Actor == null) ? CActor.EType.Enemy : m_Actor.OriginalType)
			{
			case CActor.EType.Neutral:
				return m_NeutralHealthColor;
			case CActor.EType.Enemy2:
				return m_Enemy2HealthColor;
			case CActor.EType.Enemy:
				return m_EnemyHealthColor;
			case CActor.EType.Ally:
				return m_AllyMonsterHealthColor;
			case CActor.EType.HeroSummon:
				if (m_Actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.Summoner.OverrideType == CActor.EType.Enemy)
				{
					return m_EnemyHealthColor;
				}
				return m_HeroSummonHealthColor;
			case CActor.EType.Player:
				if (m_Actor.OverrideType == CActor.EType.Enemy)
				{
					return m_EnemyHealthColor;
				}
				return m_PlayerHealthColor;
			default:
				return m_EnemyHealthColor;
			}
		}
	}

	public void Init(GameObject character, CActor actor, float height = 1.8f)
	{
		Log.GUIInfo("WorldspacePanelUIController Init");
		CharacterToTrack = character;
		Outlinable = character.GetComponentInChildren<Outlinable>();
		Transform uIZoomMeasurePoint = CharacterToTrack.FindInChildren("UIZoomMeasurePoint").transform;
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(character);
		transform.localScale = Vector3.one;
		RectTransform previewEffectTransform = (RectTransform)m_PreviewEffectsBar.transform;
		Vector2 valueOrDefault = shownPreviewEffectPosition.GetValueOrDefault();
		if (!shownPreviewEffectPosition.HasValue)
		{
			valueOrDefault = previewEffectTransform.anchoredPosition;
			shownPreviewEffectPosition = valueOrDefault;
		}
		Vector2 hiddenPreviewEffectPosition = new Vector2(shownPreviewEffectPosition.Value.x, m_HiddenPreviewEffectPanelPositionY);
		m_InfoBar.OnShow.AddListener(() =>
		{
			previewEffectTransform.anchoredPosition = shownPreviewEffectPosition.Value;
		});
		m_InfoBar.OnHide.AddListener(() =>
		{
			previewEffectTransform.anchoredPosition = hiddenPreviewEffectPosition;
		});
		m_InfoBar.SetInfoGroupVisible(shouldSetVisible: false);
		bool flag = actor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.Boss;
		Init(CharacterToTrack, uIZoomMeasurePoint, actor, actorBehaviour, flag ? m_BossScaleIncrease : Vector2.one);
		InfoUI.Init(actorBehaviour);
		m_AttackModBar.Actor = character;
		m_WorldspaceOffsetY = height;
		UnhighlightPreview();
		if (actor is CEnemyActor)
		{
			HealthBar healthBar = m_HealthBar;
			int maxHealth = actor.MaxHealth;
			int health = actor.Health;
			int originalMaxHealth = actor.OriginalMaxHealth;
			Color allegianceColor = AllegianceColor;
			Sprite healthIcon;
			if (flag)
			{
				healthIcon = bossHealthIcon;
			}
			else
			{
				healthIcon = ((((CEnemyActor)actor).MonsterClass.NonEliteVariant != null) ? eliteHealthIcon : defaultHealthIcon);
			}
			healthBar.InitHealth(maxHealth, health, originalMaxHealth, allegianceColor, healthIcon);
		}
		else
		{
			m_HealthBar.InitHealth(actor.MaxHealth, actor.Health, actor.OriginalMaxHealth, AllegianceColor, defaultHealthIcon);
		}
	}

	public void UpdateHealth(int maxHealth, int currentHealth, int originalMaxHealth, bool animate = true, Action onUpdateHealth = null)
	{
		m_HealthBar.UpdateHealth(maxHealth, Mathf.Max(0, currentHealth), originalMaxHealth, animate, AllegianceColor, onUpdateHealth);
		UnhighlightPreview();
	}

	public void ShowHeal(int amount, bool overheal)
	{
		Focus(focus: true);
		m_InfoBar.ShowHeal(amount, overheal);
	}

	public void ShowDamage(int amount)
	{
		if (!m_AttackModBar.IsFlowActive)
		{
			Focus(focus: true);
			m_InfoBar.ShowDamage(amount);
		}
	}

	[Obsolete("Use PreviewSimpleDamage(int amount, int health)")]
	public void PreviewSimpleDamage(int amount)
	{
		if (!m_Actor.IsDead)
		{
			PreviewSimpleDamage(amount, m_Actor.Health);
		}
	}

	public void PreviewSimpleDamage(int amount, int health)
	{
		if (!m_Actor.IsDead && !m_Actor.Immunities.Contains(CAbility.EAbilityType.Damage))
		{
			m_HealthBar.PreviewAttack(health, amount, 0, m_Actor.OriginalMaxHealth);
			m_InfoBar.PreviewAttack(health, amount, 0, isEnemyAttacking: false, justDamage: true);
		}
	}

	public static void RefreshHealPreviews(CAbilityHeal healAbility, CActor actorSpawningMessage)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			foreach (GameObject clientActorObject in Choreographer.s_Choreographer.ClientActorObjects)
			{
				if (clientActorObject != null)
				{
					ActorBehaviour.GetActorBehaviour(clientActorObject).m_WorldspacePanelUI?.ResetPreview();
				}
			}
			List<CActor> list = null;
			list = ((healAbility.AreaEffect == null) ? healAbility.ValidActorsInRange.ToList() : healAbility.GetValidActorsInArea(healAbility.ValidTilesInAreaAffected));
			foreach (CActor item in list)
			{
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(item));
				int num = healAbility.ModifiedStrength();
				List<CActiveBonus> list2 = CActiveBonus.FindApplicableActiveBonuses(item, CAbility.EAbilityType.AddHeal);
				actorBehaviour.m_WorldspacePanelUI?.PreviewEffectsAndHandleSubability(healAbility, item);
				foreach (CActiveBonus item2 in list2)
				{
					if (item2.Ability?.ActiveBonusData != null && item2.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingHeal))
					{
						num += item2.ReferenceStrength(new CAbility(), item);
					}
				}
				if (healAbility.MiscAbilityData != null && healAbility.MiscAbilityData.HealPercentageOfHealth.HasValue)
				{
					num += (int)Math.Floor((float)item.OriginalMaxHealth * healAbility.MiscAbilityData.HealPercentageOfHealth.Value);
				}
				else
				{
					foreach (AbilityData.StatIsBasedOnXData item3 in healAbility.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.BasedOn == CAbility.EStatIsBasedOnXType.TargetHPDifference && x.AbilityStatType == CAbility.EAbilityStatType.Strength))
					{
						num += (int)Math.Round((float)Math.Abs(item.OriginalMaxHealth - item.Health) * item3.Multiplier);
					}
					foreach (AbilityData.StatIsBasedOnXData item4 in healAbility.StatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.BasedOn == CAbility.EStatIsBasedOnXType.TargetDamageTaken && x.AbilityStatType == CAbility.EAbilityStatType.Strength))
					{
						num += (int)Math.Round((float)Math.Abs(item.MaxHealth - item.Health) * item4.Multiplier);
					}
				}
				actorBehaviour.m_WorldspacePanelUI?.OnSelectingHealFocus(num);
			}
			if (healAbility == null || healAbility.TargetingActor == null || !healAbility.ShouldPreviewPotentialDamage())
			{
				return;
			}
			foreach (CAbilityDamage item5 in SubAbilitiesAndRelatedPhaseAbilities(healAbility).OfType<CAbilityDamage>())
			{
				if (!item5.Flags.HasFlag(CAbility.EAbilityFlag.SubAbilityPreview))
				{
					continue;
				}
				foreach (GameObject clientActorObject2 in Choreographer.s_Choreographer.ClientActorObjects)
				{
					if (clientActorObject2 != null)
					{
						ActorBehaviour.GetActorBehaviour(clientActorObject2).m_WorldspacePanelUI?.PreviewPotentialDamageAbility(item5, healAbility.TargetingActor, active: true, healAbility);
					}
				}
			}
		}
	}

	public void PreviewHeal(int healAmount)
	{
		if (!m_Actor.Immunities.Contains(CAbility.EAbilityType.Heal))
		{
			m_HealthBar.PreviewHeal(m_Actor.MaxHealth, m_Actor.Health, m_Actor.OriginalMaxHealth, healAmount);
			m_InfoBar.PreviewHeal(m_Actor.MaxHealth, m_Actor.Health, m_Actor.MaxHealth > m_Actor.OriginalMaxHealth && m_Actor.Health + healAmount > m_Actor.OriginalMaxHealth, healAmount);
		}
	}

	public void PreviewDamage(int amount)
	{
		if (!m_Actor.Immunities.Contains(CAbility.EAbilityType.Damage))
		{
			m_HealthBar.PreviewAttack(m_Actor.Health, amount, 0, m_Actor.OriginalMaxHealth);
			m_InfoBar.PreviewAttack(m_Actor.Health, amount, amount, isEnemyAttacking: false, justDamage: true);
		}
	}

	public void ResetDamagePreview(int amount)
	{
		retaliateDisplayed = 0;
		m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health + amount), m_Actor.OriginalMaxHealth);
		m_HealthBar.ResetColors();
		m_InfoBar.ResetPreview();
		m_InfoBar.ResetColors();
		UnhighlightPreview();
	}

	public void ResetPreview()
	{
		ResetDamagePreview(0);
		m_PreviewEffectsBar.DisableFlags();
		m_ShieldBar.ResetShieldIcons();
		PreviewAddTarget(show: false);
		Focus(focus: false, "PREVIEW_FOCUS");
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.DisableAbilityFocusOutline(Outlinable);
		}
		PreviewAugmentEffects();
	}

	public void UpdateEffects(EffectsBar.DTO dto)
	{
		EffectBar.UpdateEffects(dto);
	}

	public void AccentuateEffect(EffectsBar.FEffect effect)
	{
		m_EffectsBar.AccentuateEffect(effect);
	}

	public void RetaliateActivatedEffect(int range)
	{
		m_EffectsBar.RetaliateActiveEffect(range);
	}

	public void RetaliateWarnEffect(bool active, int range)
	{
		m_EffectsBar.RetaliateWarnEffect(active, range);
	}

	public void CancelRetaliateEffect()
	{
		m_EffectsBar.CancelRetaliateEffect();
	}

	public void ImmobilizedWarnEffect(bool active)
	{
		m_EffectsBar.ImmobilizedWarnEffect(active);
	}

	public void DisarmedWarnEffect(bool active)
	{
		m_EffectsBar.DisarmedWarnEffect(active);
	}

	public void ImpairedWarnEffect(bool active)
	{
		m_EffectsBar.ImpairedWarnEffect(active);
	}

	public void StunnedWarnEffect(bool active)
	{
		m_EffectsBar.StunnedWarnEffect(active);
	}

	public void SleepingWarnEffect(bool active)
	{
		m_EffectsBar.SleepingWarnEffect(active);
	}

	public void AddEffect(string id, Sprite icon, string title = null, string description = null, bool blink = false)
	{
		m_EffectsBar.AddCustomEffect(id, icon, title, description, blink);
	}

	public void RemoveEffect(string id)
	{
		m_EffectsBar.RemoveCustomEffect(id);
	}

	public void RefreshCarryingQuestItem()
	{
		if (m_Actor.CarriedQuestItems.Count == 0)
		{
			m_EffectsBar.ToggleCarryingQuestItemEffect();
		}
		else
		{
			m_EffectsBar.UntoggleCarryingQuestItemEffect();
		}
	}

	public void UpdateShields(int activeShields, bool ignoreShields)
	{
		m_ShieldBar.UpdateShields(activeShields, ignoreShields);
	}

	public void Focus(bool focus, string request = "MAIN_FOCUS")
	{
		if (focus)
		{
			focusedRequests.Add(request ?? gameObject.GetInstanceID().ToString());
		}
		else
		{
			focusedRequests.Remove(request ?? gameObject.GetInstanceID().ToString());
		}
		if (focusedRequests.Count == 0)
		{
			m_CanvasGroup.alpha = m_UnfocusedOpacity;
			m_HealthBar.Focus(focus: false);
		}
		else
		{
			m_CanvasGroup.alpha = 1f;
			m_HealthBar.Focus(focus: true);
		}
	}

	public void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		if (retaliateDamage == 0)
		{
			if (forceUpdate || retaliateDisplayed != 0)
			{
				m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
				m_InfoBar.SetInfoGroupVisible(shouldSetVisible: false);
			}
		}
		else
		{
			m_HealthBar.PreviewRetaliate(m_Actor.Health, retaliateDamage, m_Actor.OriginalMaxHealth);
			m_InfoBar.PreviewRetaliate(m_Actor.Health, retaliateDamage);
		}
		retaliateDisplayed = retaliateDamage;
	}

	public void OnSelectingAttackFocus(CAbilityAttack abilityAttack, CActor attacker, CAttackSummary.TargetSummary attackSummary, bool isEnemyAttacking = false)
	{
		if (abilityAttack.Flags.HasFlag(CAbility.EAbilityFlag.DontRefreshTargetingUI) || (abilityAttack.State != CAbilityAttack.EAttackState.SelectAttackFocus && abilityAttack.State != CAbilityAttack.EAttackState.SelectAttackFocusAdditionalTargets))
		{
			return;
		}
		Focus(focus: true, "PREVIEW_FOCUS");
		if (!m_Actor.Immunities.Contains(CAbility.EAbilityType.Damage))
		{
			if (isEnemyAttacking || abilityAttack.AllTargetsOnMovePath)
			{
				m_HealthBar.UpdateColors(attackSummary.FinalAttackStrength, abilityAttack.Strength);
				m_InfoBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength, abilityAttack.Strength, isEnemyAttacking: true, justDamage: false, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
			}
			else
			{
				m_HealthBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff), m_Actor.OriginalMaxHealth, highlight: false);
				m_InfoBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff), isEnemyAttacking: false, justDamage: false, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
				m_InfoBar.InitBreakdown(attackSummary, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff));
				PreviewAttackTargets(attackSummary.AttackAbilityWithOverrides, attackSummary.Parent, attackSummary);
				PreviewAugmentEffects();
			}
			m_ShieldBar.SetPierce(attackSummary.ActorToAttack.CachedShieldNeutralized ? 99999 : attackSummary.Pierce);
		}
		PreviewAdvantage(abilityAttack, attackSummary.ActorToAttack, removePreview: false);
		PreviewAddTarget(abilityAttack.State == CAbilityAttack.EAttackState.SelectAttackFocusAdditionalTargets);
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
		}
	}

	private void PreviewAddTarget(bool show)
	{
		if (show)
		{
			addTargetAnimation.gameObject.SetActive(value: true);
			addTargetAnimation.Play();
		}
		else
		{
			addTargetAnimation.gameObject.SetActive(value: false);
			addTargetAnimation.Stop();
		}
	}

	public void OnSelectingDamageFocus(CAbility abilityDamage, CActor attacker, CAttackSummary.TargetSummary damageSummary, bool isEnemyAttacking = false)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			Focus(focus: true);
			int totalAttack = damageSummary.FinalAttackStrength;
			if (abilityDamage.GetMiscVar("DamageIsEqualToTargetShieldValue").GetBool())
			{
				totalAttack = damageSummary.ActorToAttack.CalculateShield();
			}
			if (!m_Actor.Immunities.Contains(CAbility.EAbilityType.Damage))
			{
				m_HealthBar.PreviewAttack(m_Actor.Health, totalAttack, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff), m_Actor.OriginalMaxHealth, highlight: false);
				m_InfoBar.PreviewAttack(m_Actor.Health, totalAttack, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff), isEnemyAttacking: false, justDamage: true, damageSummary.OverallAdvantage == EAdvantageStatuses.Advantage, damageSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
				m_InfoBar.InitBreakdown(damageSummary, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff));
			}
			PreviewEffects(abilityDamage, m_Actor);
			PreviewAugmentEffects();
			if (Outlinable != null)
			{
				WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
			}
			if (abilityDamage.AbilityText.IsNOTNullOrEmpty())
			{
				Singleton<HelpBox>.Instance.Show(abilityDamage.AbilityText.Replace("$", ""));
			}
			if (abilityDamage.ResourcesToTakeFromTargets == null)
			{
				return;
			}
			foreach (string key in abilityDamage.ResourcesToTakeFromTargets.Keys)
			{
				m_EffectsBar.SetCustomEffectBlinkActive(key, active: true);
			}
		}
	}

	public static void ResetPreviewEffects()
	{
		foreach (GameObject clientActorObject in Choreographer.s_Choreographer.ClientActorObjects)
		{
			if (clientActorObject != null)
			{
				ActorBehaviour.GetActorBehaviour(clientActorObject).m_WorldspacePanelUI?.m_PreviewEffectsBar.DisableFlags();
			}
		}
	}

	public static void PreviewAttack(CAbilityAttack m_AttackAbility, CActor m_AttackingActor, List<CActor> actorsToTarget = null)
	{
		CAttackSummary attackSummary = m_AttackAbility.AttackSummary;
		ResetPreviewEffects();
		int targetIndex = 0;
		IEnumerable<CAttackSummary.TargetSummary> enumerable;
		if (!m_AttackAbility.MaxTargetsSelected())
		{
			IEnumerable<CAttackSummary.TargetSummary> targets = attackSummary.Targets;
			enumerable = targets;
		}
		else
		{
			enumerable = attackSummary.Targets.Where((CAttackSummary.TargetSummary it) => m_AttackAbility.ActorsToTarget.Contains(it.Actor));
		}
		IEnumerable<CAttackSummary.TargetSummary> source = enumerable;
		foreach (GameObject clientActorObject in Choreographer.s_Choreographer.ClientActorObjects)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(clientActorObject);
			if (actorBehaviour == null)
			{
				continue;
			}
			CActor actor = actorBehaviour.Actor;
			if (actor == null)
			{
				continue;
			}
			bool flag = false;
			if (actorsToTarget != null)
			{
				if (actorsToTarget.Contains(actor))
				{
					CAttackSummary.TargetSummary targetSummary = attackSummary?.FindTarget(actor, ref targetIndex);
					if (targetSummary != null && ScenarioManager.Scenario.HasActor(targetSummary.ActorToAttack))
					{
						actorBehaviour.m_WorldspacePanelUI?.OnSelectingAttackFocus(m_AttackAbility, m_AttackingActor, targetSummary);
						flag = true;
					}
				}
			}
			else
			{
				foreach (CAttackSummary.TargetSummary item in source.Where((CAttackSummary.TargetSummary x) => x.ActorToAttack == actor))
				{
					if (ScenarioManager.Scenario.HasActor(item.ActorToAttack))
					{
						actorBehaviour.m_WorldspacePanelUI?.OnSelectingAttackFocus(m_AttackAbility, m_AttackingActor, item, m_AttackingActor.Type != CActor.EType.Player);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				actorBehaviour.m_WorldspacePanelUI.ResetPreview();
			}
		}
	}

	public static void PreviewEffects(CAbility ability, bool onlyEnable = false)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (ability.AreaEffect == null)
			{
				return;
			}
			ResetPreviewEffects();
			foreach (CActor item in ability.GetValidActorsInArea(ability.ValidTilesInAreaAffected))
			{
				PreviewForActor(item, ability, active: true);
			}
		}
		void PreviewForActor(CActor actor, CAbility ab, bool active)
		{
			if (actor != null)
			{
				ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor)).m_WorldspacePanelUI?.PreviewEffects(ab, actor, !active, onlyEnable);
			}
		}
	}

	public static void PreviewAugmentEffects(bool onlyEnable = false)
	{
		if (!(PhaseManager.Phase is CPhaseAction cPhaseAction))
		{
			return;
		}
		foreach (CActionAugmentation item in cPhaseAction.ActionAugmentationsToConsume.ToList())
		{
			PreviewAugmentEffects(item, cPhaseAction.CurrentPhaseAbility?.m_Ability, active: true, onlyEnable);
		}
	}

	private static IEnumerable<CActor> GetAbilityActorsInRange(CAbility ab, CAbility parentAbility)
	{
		CAbility prevParent = ab.ParentAbility;
		if (parentAbility != null)
		{
			ab.ParentAbility = parentAbility;
		}
		foreach (CActor item in GetAbilityActorsInRange(ab, parentAbility.TargetingActor))
		{
			yield return item;
		}
		ab.ParentAbility = prevParent;
	}

	private static IEnumerable<CActor> GetAbilityActorsInRange(CAbility ab, CActor targetingActor)
	{
		if (targetingActor == null || ab.AbilityType == CAbility.EAbilityType.Null)
		{
			yield break;
		}
		CActor ta = ab.TargetingActor;
		CActor fa = ab.FilterActor;
		ab.TargetingActor = targetingActor;
		ab.FilterActor = targetingActor;
		SharedAbilityTargeting.GetValidActorsInRange(ab);
		foreach (CActor item in ab.ValidActorsInRange)
		{
			yield return item;
		}
		ab.TargetingActor = ta;
		ab.FilterActor = fa;
	}

	public static void PreviewAugmentEffects(CActionAugmentation augment, CAbility parentAbility, bool active, bool onlyEnable = false)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (augment == null || parentAbility == null)
			{
				return;
			}
			if (augment.CostAbility != null)
			{
				Preview(augment.CostAbility, active);
			}
			foreach (CActionAugmentationOp augmentationOp in augment.AugmentationOps)
			{
				if (augmentationOp.Ability != null)
				{
					Preview(augmentationOp.Ability, active);
				}
			}
		}
		void Preview(CAbility ab, bool active2)
		{
			foreach (CActor item in GetAbilityActorsInRange(ab, parentAbility))
			{
				PreviewForActor(item, ab, active2);
			}
		}
		void PreviewForActor(CActor actor, CAbility ab, bool flag)
		{
			if (actor != null)
			{
				ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor)).m_WorldspacePanelUI?.PreviewEffects(ab, actor, !flag, onlyEnable);
			}
		}
	}

	private static IEnumerable<CAbility> SubAbilitiesAndRelatedPhaseAbilities(CAbility ab)
	{
		foreach (CAbility subAbility in ab.SubAbilities)
		{
			yield return subAbility;
		}
		if (PhaseManager.CurrentPhaseAction == null)
		{
			yield break;
		}
		foreach (CPhaseAction.CPhaseAbility remainingPhaseAbility in PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities)
		{
			if ((remainingPhaseAbility.m_Ability.ParentAbility == null || !(remainingPhaseAbility.m_Ability.ParentAbility.ID != ab.ID)) && !ab.SubAbilities.Contains(remainingPhaseAbility.m_Ability))
			{
				yield return remainingPhaseAbility.m_Ability;
			}
		}
	}

	public static void PreviewAttackTargets(CAbility ab, CAttackSummary attackSummary, CAttackSummary.TargetSummary currentTargetSummary)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (currentTargetSummary != null)
			{
				CActor actorToAttack = currentTargetSummary.ActorToAttack;
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actorToAttack));
				if (actorBehaviour != null)
				{
					actorBehaviour.m_WorldspacePanelUI?.PreviewEffects(ab, actorToAttack);
				}
			}
			foreach (CAbility item in SubAbilitiesAndRelatedPhaseAbilities(ab))
			{
				if (!item.Flags.HasFlag(CAbility.EAbilityFlag.SubAbilityPreview) || (item.Targeting == CAbility.EAbilityTargeting.Custom && item.TargetingVars == null) || !item.MeetsStartAbilityRequirements(ab.TargetingActor))
				{
					continue;
				}
				lock (PreviewEffectsLock)
				{
					foreach (CActor item2 in GetAbilityActorsInRange(item, ab).ToList())
					{
						ActorBehaviour actorBehaviour2 = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(item2));
						actorBehaviour2.m_WorldspacePanelUI?.PreviewEffectsSingleAbility(item, item2, false, false);
						if (item is CAbilityDamage damageAbility)
						{
							actorBehaviour2.m_WorldspacePanelUI?.PreviewPotentialDamageAbility(damageAbility, ab.TargetingActor, active: true, ab);
						}
					}
				}
			}
		}
	}

	public void PreviewEffectsAndHandleSubability(CAbility ability, CActor target)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			lock (PreviewEffectsLock)
			{
				PreviewEffects(ability, target, !ability.ActorsToTarget.Contains(m_Actor));
				foreach (CAbility item in from s in SubAbilitiesAndRelatedPhaseAbilities(ability)
					where s?.MiscAbilityData != null && s.MiscAbilityData.TreatAsNonSubAbility == true
					select s)
				{
					if (item.Flags.HasFlag(CAbility.EAbilityFlag.SubAbilityPreview) && item.MeetsStartAbilityRequirements(ability.TargetingActor) && (item.Targeting != CAbility.EAbilityTargeting.Custom || item.TargetingVars != null))
					{
						PreviewEffectsSingleAbility(item, target, !GetAbilityActorsInRange(item, ability).Contains(target), false);
					}
				}
			}
		}
	}

	public static void RefreshPreviewPotentialTileSubabilities(CAbility ability)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (ability.IsCurrentlyTargetingActors())
			{
				return;
			}
			ResetPreviewEffects();
			foreach (CActor allActor in ScenarioManager.Scenario.AllActors)
			{
				ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(allActor));
				if (actorBehaviour?.m_WorldspacePanelUI == null)
				{
					continue;
				}
				actorBehaviour.m_WorldspacePanelUI.PreviewPotentialTrapSpring(ability, allActor);
				foreach (CAbility item in SubAbilitiesAndRelatedPhaseAbilities(ability))
				{
					if (item.Flags.HasFlag(CAbility.EAbilityFlag.SubAbilityPreview) && item.Targeting == CAbility.EAbilityTargeting.Custom && item.MeetsStartAbilityRequirements(ability.TargetingActor))
					{
						CAbility parentAbility = item.ParentAbility;
						item.ParentAbility = ability;
						bool flag = GetAbilityActorsInRange(item, ability.TargetingActor).Contains(allActor);
						actorBehaviour.m_WorldspacePanelUI.PreviewEffects(item, allActor, !flag);
						if (item is CAbilityDamage damageAbility)
						{
							actorBehaviour.m_WorldspacePanelUI.PreviewPotentialDamageAbility(damageAbility, ability.TargetingActor, flag, ability, forceRefresh: true);
						}
						item.ParentAbility = parentAbility;
					}
				}
			}
		}
	}

	public void PreviewPotentialDamageAbility(CAbilityDamage damageAbility, CActor targetingActor, bool active, CAbility overrideParent, bool forceRefresh = false)
	{
		CAbility parentAbility = damageAbility.ParentAbility;
		if (parentAbility == null && overrideParent != null)
		{
			damageAbility.ParentAbility = overrideParent;
		}
		PreviewPotentialDamageAbility(damageAbility, targetingActor, active, forceRefresh);
		damageAbility.ParentAbility = parentAbility;
	}

	public void PreviewPotentialDamageAbility(CAbilityDamage damageAbility, CActor targetingActor, bool active, bool forceRefresh = false)
	{
		if (!damageAbility.MeetsStartAbilityRequirements(targetingActor) || !(GetAbilityActorsInRange(damageAbility, targetingActor).Contains(m_Actor) | forceRefresh))
		{
			return;
		}
		int num = 0;
		CAbilityAttack cAbilityAttack = damageAbility.ParentAbility as CAbilityAttack;
		if (cAbilityAttack != null)
		{
			if (damageAbility.NumberTargets != -1)
			{
				return;
			}
			if (!cAbilityAttack.AbilityHasHappened)
			{
				if (cAbilityAttack.IsAttackAbilityWithOverrides)
				{
					CAbilityAttack cAbilityAttack2 = cAbilityAttack.GetMiscVar("OriginalAttackAbility").GetObject<CAbilityAttack>();
					if (cAbilityAttack2 != null)
					{
						cAbilityAttack = cAbilityAttack2;
					}
				}
				CAttackSummary.TargetSummary targetSummary = cAbilityAttack.AttackSummary?.FindTarget(m_Actor);
				if (cAbilityAttack.AreaEffect != null && targetSummary != null)
				{
					num = targetSummary.CalcExpressionString(calcPostShieldScalars: false, calcShield: true);
				}
				else
				{
					if (targetSummary != null && (cAbilityAttack.ActorsToTarget.Count == 0 || cAbilityAttack.ActorsToTarget.Contains(m_Actor)))
					{
						num = targetSummary.CalcExpressionString(calcPostShieldScalars: false, calcShield: true);
					}
					if (cAbilityAttack.ActorsToTarget.Count == 0 && damageAbility.MiscAbilityData.UseParentTiles.HasValue && damageAbility.MiscAbilityData.UseParentTiles.Value)
					{
						return;
					}
				}
			}
		}
		int strength = damageAbility.Strength;
		damageAbility.SetStatBasedOnX(targetingActor, damageAbility.StatIsBasedOnXEntries, damageAbility.AbilityFilter, m_Actor);
		int num2 = damageAbility.Strength;
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			if (num > 0)
			{
				num *= 2;
			}
			else
			{
				num2 *= 2;
			}
		}
		if (m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
		{
			if (num > 0)
			{
				num /= 2;
			}
			else
			{
				num2 /= 2;
			}
		}
		num2 += num;
		if (active)
		{
			PreviewDamage(num2);
			HighlightPreview(isPositive: false);
		}
		else
		{
			ResetDamagePreview(0);
			UnhighlightPreview();
		}
		damageAbility.Strength = strength;
	}

	public void PreviewPotentialTrapSpring(CAbility ability, CActor target)
	{
		TValue miscVar = ability.GetMiscVar("PreviewSpringTrap");
		if (!miscVar.isUnknown)
		{
			CObjectTrap trap = miscVar.GetObject<CObjectTrap>(ability.VarsContext);
			CActor cActor = ability.GetMiscVar("PreviewSpringTrapOn").GetObject<CActor>(ability.VarsContext);
			PreviewPotentialTrapSpring(trap, new List<CActor> { cActor }, cActor == target);
		}
	}

	public void PreviewPotentialTrapSpring(CAbility ability, bool active)
	{
		TValue miscVar = ability.GetMiscVar("PreviewSpringTrap");
		if (!miscVar.isUnknown)
		{
			CObjectTrap trap = miscVar.GetObject<CObjectTrap>(ability.VarsContext);
			PreviewPotentialTrapSpring(trap, ability.ValidActorsInRange, active);
		}
	}

	public void PreviewPotentialTrapSpring(CObjectTrap trap, IEnumerable<CActor> targets, bool active)
	{
		if (!targets.Contains(m_Actor))
		{
			return;
		}
		PreviewEffect(CCondition.EPositiveCondition.Bless, EffectsBar.FEffect.Blessed, trap, m_Actor, !active);
		PreviewEffect(CCondition.EPositiveCondition.Regenerate, EffectsBar.FEffect.Regenerate, trap, m_Actor, !active);
		PreviewEffect(CCondition.EPositiveCondition.Strengthen, EffectsBar.FEffect.Strengthened, trap, m_Actor, !active);
		PreviewEffect(CCondition.EPositiveCondition.Invisible, EffectsBar.FEffect.Invisible, trap, m_Actor, !active);
		PreviewEffect(CCondition.EPositiveCondition.Ward, EffectsBar.FEffect.Ward, trap, m_Actor, !active);
		PreviewEffect(CCondition.EPositiveCondition.Mutate, EffectsBar.FEffect.Mutate, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Curse, EffectsBar.FEffect.Cursed, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Disarm, EffectsBar.FEffect.Disarmed, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Immobilize, EffectsBar.FEffect.Immobilized, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Muddle, EffectsBar.FEffect.Muddled, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Poison, EffectsBar.FEffect.Poisoned, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Stun, EffectsBar.FEffect.Stunned, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Wound, EffectsBar.FEffect.Wounded, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Bane, EffectsBar.FEffect.Baned, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.StopFlying, EffectsBar.FEffect.StopFlying, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Sleep, EffectsBar.FEffect.Sleep, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Brittle, EffectsBar.FEffect.Brittle, trap, m_Actor, !active);
		PreviewEffect(CCondition.ENegativeCondition.Impair, EffectsBar.FEffect.Impaired, trap, m_Actor, !active);
		if (trap == null)
		{
			ResetDamagePreview(0);
			return;
		}
		int num = trap.HealValue;
		int num2 = trap.GetTrapDamage();
		bool flag = m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle);
		bool flag2 = m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward);
		if (!m_Actor.IsEnemyMonsterType && m_Actor.WillActorDie(num2, 0, null, includePlayers: true))
		{
			if (num > 0)
			{
				int num3 = Math.Max(0, num2 - num);
				num -= num2 - num3;
				num2 = num3;
				flag = false;
			}
			if (flag)
			{
				num2 *= 2;
			}
			if (flag2)
			{
				num2 /= 2;
			}
		}
		else
		{
			if (flag)
			{
				num2 *= 2;
			}
			if (flag2)
			{
				num2 /= 2;
			}
			int num4 = Math.Max(0, num2 - num);
			num -= num2 - num4;
			num2 = num4;
		}
		int num5 = num2 - num;
		if (num5 > 0)
		{
			if (active)
			{
				PreviewDamage(num5);
			}
			else
			{
				ResetDamagePreview(0);
			}
		}
		else if (num5 < 0)
		{
			if (active)
			{
				PreviewHeal(-num5);
			}
			else
			{
				ResetDamagePreview(0);
			}
		}
	}

	public void PreviewPotentialTrapDamage(CObjectTrap trap, CActor targetingActor, bool active)
	{
		int num = 0;
		int num2 = trap.DamageValue;
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			if (num > 0)
			{
				num *= 2;
			}
			else
			{
				num2 *= 2;
			}
		}
		if (m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward))
		{
			if (num > 0)
			{
				num /= 2;
			}
			else
			{
				num2 /= 2;
			}
		}
		num2 += num;
		if (active)
		{
			PreviewDamage(num2);
		}
		else
		{
			ResetDamagePreview(num2);
		}
	}

	public void PreviewEffects(CAbility ability, CActor target, bool removePreview = false, bool onlyEnable = false)
	{
		lock (PreviewEffectsLock)
		{
			PreviewEffectsSingleAbility(ability, target, removePreview, onlyEnable);
			if (ability is CAbilityControlActor cAbilityControlActor)
			{
				foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
				{
					if (controlAbility.AbilityFilter == null || controlAbility.AbilityFilter.IsValidTarget(target, target, ability.IsTargetedAbility, useTargetOriginalType: false, false))
					{
						PreviewEffectsSingleAbility(controlAbility, target, removePreview, onlyEnable, CAbility.EAbilityType.ControlActor);
					}
				}
			}
			if (ability == null || ability.TargetingActor == null || !ability.ShouldPreviewPotentialDamage())
			{
				return;
			}
			foreach (CAbilityDamage item in SubAbilitiesAndRelatedPhaseAbilities(ability).OfType<CAbilityDamage>())
			{
				if (item.Flags.HasFlag(CAbility.EAbilityFlag.SubAbilityPreview))
				{
					PreviewPotentialDamageAbility(item, ability.TargetingActor, active: true, ability);
				}
			}
		}
	}

	public void PreviewEffectsSingleAbility(CAbility ability, CActor actor, bool removePreview = false, bool onlyEnable = false, params CAbility.EAbilityType[] ignoreTypes)
	{
		PreviewEffect(CCondition.EPositiveCondition.Bless, EffectsBar.FEffect.Blessed, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.EPositiveCondition.Regenerate, EffectsBar.FEffect.Regenerate, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.EPositiveCondition.Strengthen, EffectsBar.FEffect.Strengthened, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.EPositiveCondition.Invisible, EffectsBar.FEffect.Invisible, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.EPositiveCondition.Ward, EffectsBar.FEffect.Ward, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.EPositiveCondition.Mutate, EffectsBar.FEffect.Mutate, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Curse, EffectsBar.FEffect.Cursed, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Disarm, EffectsBar.FEffect.Disarmed, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Immobilize, EffectsBar.FEffect.Immobilized, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Muddle, EffectsBar.FEffect.Muddled, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Poison, EffectsBar.FEffect.Poisoned, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Stun, EffectsBar.FEffect.Stunned, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Wound, EffectsBar.FEffect.Wounded, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Bane, EffectsBar.FEffect.Baned, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.StopFlying, EffectsBar.FEffect.StopFlying, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Sleep, EffectsBar.FEffect.Sleep, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Brittle, EffectsBar.FEffect.Brittle, ability, actor, removePreview, onlyEnable);
		PreviewEffect(CCondition.ENegativeCondition.Impair, EffectsBar.FEffect.Impaired, ability, actor, removePreview, onlyEnable);
		PreviewAbilityEffect(ability, actor, removePreview, ignoreTypes);
	}

	public void PreviewAbilityEffect(CAbility ability, CActor actor, bool removePreview = false, params CAbility.EAbilityType[] ignoreTypes)
	{
		PreviewDoomEffect(ability, removePreview);
		PreviewEffectAbilityType(EffectsBar.FEffect.BlockHealing, ability, actor, CAbility.EAbilityType.BlockHealing, removePreview, ignoreTypes);
		PreviewEffectAbilityType(EffectsBar.FEffect.NeutralizeShield, ability, actor, CAbility.EAbilityType.NeutralizeShield, removePreview, ignoreTypes);
		PreviewEffectAbilityType(EffectsBar.FEffect.Push, ability, actor, CAbility.EAbilityType.Push, removePreview, ignoreTypes);
		PreviewEffectAbilityType(EffectsBar.FEffect.Pull, ability, actor, CAbility.EAbilityType.Pull, removePreview, ignoreTypes);
		if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Enemy))
		{
			PreviewEffectAbilityType(EffectsBar.FEffect.Controlled, ability, actor, CAbility.EAbilityType.ControlActor, removePreview, ignoreTypes);
		}
		PreviewAdvantage(ability, actor, removePreview);
		PreviewPotentialTrapSpring(ability, !removePreview);
	}

	private void PreviewAdvantage(CAbility ability, CActor actor, bool removePreview)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			int targetIndex = 0;
			CAttackSummary.TargetSummary targetSummary = cAbilityAttack.AttackSummary?.FindTarget(actor, ref targetIndex);
			if (targetSummary != null)
			{
				bool flag = targetSummary.OverallAdvantage == EAdvantageStatuses.Advantage;
				bool flag2 = targetSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage;
				m_PreviewEffectsBar.ToggleEffect(EffectsBar.FEffect.Advantage, !removePreview & flag);
				m_PreviewEffectsBar.ToggleEffect(EffectsBar.FEffect.Disadvantage, !removePreview & flag2);
			}
		}
	}

	private void PreviewEffectAbilityType(EffectsBar.FEffect effect, CAbility ability, CActor actor, CAbility.EAbilityType abilityType, bool removePreview = false, params CAbility.EAbilityType[] ignoreTypes)
	{
		if (!ignoreTypes.Any((CAbility.EAbilityType x) => x == abilityType))
		{
			m_PreviewEffectsBar.ToggleEffect(effect, !removePreview && (ability.AbilityType == abilityType || ability.SubAbilities.Exists((CAbility it) => it.AbilityType == abilityType && !it.Flags.HasFlag(CAbility.EAbilityFlag.DontPreview) && it.AbilityFilter.IsValidTarget(actor, ability.TargetingActor, it.IsTargetedAbility, useTargetOriginalType: false, false) && it.MeetsStartAbilityRequirements(ability.TargetingActor))));
		}
	}

	public void PreviewDoomEffect(CAbility ability, bool removePreview = false)
	{
	}

	private void PreviewEffect(CCondition.ENegativeCondition condition, EffectsBar.FEffect effect, CObjectTrap trap, CActor actor, bool removePreview = false, bool onlyEnable = false)
	{
		bool flag = trap?.Conditions?.Contains(condition) == true;
		bool flag2 = flag;
		if (Enum.TryParse<CAbility.EAbilityType>(condition.ToString(), ignoreCase: true, out var result) && actor.Immunities.Contains(result))
		{
			return;
		}
		if (removePreview)
		{
			flag = !removePreview;
		}
		if (onlyEnable)
		{
			if (flag | flag2)
			{
				m_PreviewEffectsBar.EnableFlag(effect);
			}
		}
		else
		{
			m_PreviewEffectsBar.ToggleEffect(effect, flag | flag2);
		}
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag & flag2);
	}

	private void PreviewEffect(CCondition.EPositiveCondition condition, EffectsBar.FEffect effect, CObjectTrap trap, CActor actor, bool removePreview = false, bool onlyEnable = false)
	{
		bool flag = trap?.PositiveConditions?.Contains(condition) == true;
		bool flag2 = flag;
		if (Enum.TryParse<CAbility.EAbilityType>(condition.ToString(), ignoreCase: true, out var result) && actor.Immunities.Contains(result))
		{
			return;
		}
		if (removePreview)
		{
			flag = !removePreview;
		}
		if (onlyEnable)
		{
			if (flag | flag2)
			{
				m_PreviewEffectsBar.EnableFlag(effect);
			}
		}
		else
		{
			m_PreviewEffectsBar.ToggleEffect(effect, flag | flag2);
		}
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag & flag2);
	}

	private void PreviewEffect(CCondition.EPositiveCondition condition, EffectsBar.FEffect effect, CAbility ability, CActor actor, bool removePreview = false, bool onlyEnable = false)
	{
		bool flag = ability.HasCondition(condition, ignoreSubAbilities: true) || (actor != null && ability.HaveActiveItemsCondition(condition, actor));
		bool flag2 = ability.HaveActiveItemsCondition(condition);
		if (Enum.TryParse<CAbility.EAbilityType>(condition.ToString(), ignoreCase: true, out var result) && actor.Immunities.Contains(result))
		{
			return;
		}
		if (removePreview)
		{
			flag = !removePreview;
		}
		if (ability is CAbilityRemoveConditions && actor != null)
		{
			if (removePreview)
			{
				m_EffectsBar.SetBlinkActive(effect);
				return;
			}
			bool isActive = flag && actor.Tokens.HasKey(condition);
			m_EffectsBar.SetBlinkActive(effect, isActive);
			return;
		}
		if (onlyEnable)
		{
			if (flag | flag2)
			{
				m_PreviewEffectsBar.EnableFlag(effect);
			}
		}
		else
		{
			m_PreviewEffectsBar.ToggleEffect(effect, flag | flag2);
		}
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag & flag2);
	}

	private void PreviewEffect(CCondition.ENegativeCondition condition, EffectsBar.FEffect effect, CAbility ability, CActor actor, bool removePreview = false, bool onlyEnable = false)
	{
		bool flag = ability.HasCondition(condition, ignoreSubAbilities: true, actor) || (actor != null && ability.HaveActiveItemsCondition(condition, actor));
		bool flag2 = ability.HaveActiveItemsCondition(condition) && !Choreographer.s_Choreographer.m_TileSelectionDisabled;
		if (Enum.TryParse<CAbility.EAbilityType>(condition.ToString(), ignoreCase: true, out var result) && actor.Immunities.Contains(result))
		{
			return;
		}
		if (removePreview)
		{
			flag = !removePreview;
		}
		if (ability is CAbilityRemoveConditions && actor != null)
		{
			if (removePreview)
			{
				m_EffectsBar.SetBlinkActive(effect);
				return;
			}
			bool isActive = flag && actor.Tokens.HasKey(condition);
			m_EffectsBar.SetBlinkActive(effect, isActive);
			return;
		}
		if (onlyEnable)
		{
			if (flag | flag2)
			{
				m_PreviewEffectsBar.EnableFlag(effect);
			}
		}
		else
		{
			m_PreviewEffectsBar.ToggleEffect(effect, flag | flag2);
		}
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag & flag2);
	}

	public void OnSelectingHealFocus(int healAmount)
	{
		Focus(focus: true);
		bool flag = false;
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Poisoned, isActive: true);
			flag = true;
		}
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Wound))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Wounded, isActive: true);
		}
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Brittle, isActive: true);
		}
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Bane))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Baned, isActive: true);
		}
		if (!flag)
		{
			PreviewHeal(healAmount);
		}
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
		}
	}

	public void OnSelectingDoomFocus(CAbility ability)
	{
		Focus(focus: true);
		PreviewDoomEffect(ability);
	}

	public void OnWonGold(int gold)
	{
		m_InfoBar.ShowGold(gold);
	}

	public void OnLootGain(int numTokens)
	{
		m_InfoBar.ShowLootGain(numTokens);
	}

	public void OnEarnedXP(int xp)
	{
		m_InfoBar.ShowXP(xp);
	}

	public void OnMessage(CMessageData message)
	{
		if (message.m_Type == CMessageData.MessageType.ActionSelection || message.m_Type == CMessageData.MessageType.ActorIsAttacking || message.m_Type == CMessageData.MessageType.EndAbilityAnimSync || message.m_Type == CMessageData.MessageType.ActorHasTeleported || message.m_Type == CMessageData.MessageType.ActorHasTeleported2 || (message.m_Type == CMessageData.MessageType.PlayerFinishedSelectingToAvoidDamageOrNot && message.m_ActorSpawningMessage == m_Actor))
		{
			ResetPreview();
			m_ShieldBar.ResetShieldIcons();
			if (m_Actor.CachedShieldNeutralized)
			{
				m_ShieldBar.SetPierce(99999);
			}
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Advantage);
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Disadvantage);
			m_EffectsBar.ResetEffectBlink();
			m_EffectsBar.ResetCustomEffectBlink();
			HideInfoTooltip();
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsImmobilized)
		{
			ImmobilizedWarnEffect(active: false);
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsDisarmed)
		{
			DisarmedWarnEffect(active: false);
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsImpaired)
		{
			ImpairedWarnEffect(active: false);
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsStunned)
		{
			StunnedWarnEffect(active: false);
		}
		if (message.m_Type == CMessageData.MessageType.StartTurn)
		{
			UnhighlightPreview();
			m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
			if (m_Actor == message.m_ActorSpawningMessage)
			{
				Focus(focus: true);
			}
			else if (message.m_ActorSpawningMessage is CEnemyActor && (!(m_Actor is CEnemyActor) || m_Actor.Type == CActor.EType.Ally))
			{
				Focus(focus: true);
			}
			else
			{
				Focus(focus: false);
			}
		}
		else if (message.m_Type == CMessageData.MessageType.ActionSelection)
		{
			m_HealthBar.ResetColors();
			m_InfoBar.ResetColors();
			UnhighlightPreview();
			m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
			Focus(m_Actor == message.m_ActorSpawningMessage);
		}
		else if (message.m_Type == CMessageData.MessageType.ActorIsHealing || message.m_Type == CMessageData.MessageType.ActorIsApplyingConditionActiveBonus)
		{
			UnhighlightPreview();
			if (message is CActorIsApplyingConditionActiveBonus_MessageData cActorIsApplyingConditionActiveBonus_MessageData)
			{
				PreviewDoomEffect(cActorIsApplyingConditionActiveBonus_MessageData.m_Ability, removePreview: true);
			}
		}
		else if (message.m_Type == CMessageData.MessageType.NextRound)
		{
			Focus(focus: true);
		}
		else if (message.m_Type == CMessageData.MessageType.StartAbility || message.m_Type == CMessageData.MessageType.EndAbility || message.m_Type == CMessageData.MessageType.UpdateCurrentActor)
		{
			Focus(message.m_ActorSpawningMessage == m_Actor);
		}
		else if (message.m_Type == CMessageData.MessageType.ActorSelectedAttackFocus)
		{
			CActorSelectedAttackFocus_MessageData cActorSelectedAttackFocus_MessageData = (CActorSelectedAttackFocus_MessageData)message;
			PreviewEffectsAndHandleSubability(cActorSelectedAttackFocus_MessageData.m_Ability, m_Actor);
			PreviewAugmentEffects(onlyEnable: true);
		}
		else if (message.m_Type == CMessageData.MessageType.OnItemCallback)
		{
			COnItemCallback_MessageData cOnItemCallback_MessageData = message as COnItemCallback_MessageData;
			if (cOnItemCallback_MessageData.m_Item.FindAbilityOnItemCard(CAbility.EAbilityType.Damage) is CAbilityDamage damageAbility && (cOnItemCallback_MessageData.m_CallbackType == ESESubTypeItem.ItemDeSelected || cOnItemCallback_MessageData.m_CallbackType == ESESubTypeItem.ItemSelected))
			{
				PreviewPotentialDamageAbility(damageAbility, cOnItemCallback_MessageData.m_ActorSpawningMessage, cOnItemCallback_MessageData.m_Item.SlotState == CItem.EItemSlotState.Selected, PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability);
			}
		}
		else if (message.m_Type == CMessageData.MessageType.ActorIsSelectingTargetingFocus)
		{
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = (CActorIsSelectingTargetingFocus_MessageData)message;
			PreviewEffectsAndHandleSubability(cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility, m_Actor);
			PreviewAugmentEffects(onlyEnable: true);
		}
		else if (message.m_Type == CMessageData.MessageType.ActorIsSelectingDamageFocus)
		{
			CActorIsSelectingDamageFocus_MessageData cActorIsSelectingDamageFocus_MessageData = (CActorIsSelectingDamageFocus_MessageData)message;
			PreviewEffectsAndHandleSubability(cActorIsSelectingDamageFocus_MessageData.m_DamageAbility, m_Actor);
			PreviewAugmentEffects(onlyEnable: true);
		}
		else if (message.m_Type == CMessageData.MessageType.ChosenAbilityChanged)
		{
			CChosenAbilityChanged_MessageData cChosenAbilityChanged_MessageData = (CChosenAbilityChanged_MessageData)message;
			PreviewEffectsAndHandleSubability(cChosenAbilityChanged_MessageData.m_ChooseAbility, m_Actor);
			PreviewAugmentEffects(onlyEnable: true);
		}
	}

	public void DisplayAttackModifierFlow(CAttackSummary.TargetSummary targetSummary, CAbilityAttack abilityAttack)
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		Log.GUIInfo("[AttackModifiers]: DisplayAttackModifierFlow " + (m_AttackModBar.m_AttackModFinishCoroutine != null));
		Focus(focus: true);
		UnhighlightPreview();
		m_InfoBar.PrepareForAttack(targetSummary);
		m_HealthBar.PrepareForAttack(m_Actor.Health, m_Actor.OriginalMaxHealth);
		if (m_AttackModBar.m_AttackModCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModCoroutine);
		}
		m_AttackModBar.m_AttackModCoroutine = StartCoroutine(m_AttackModBar.ShowModifiers(targetSummary, abilityAttack));
		if (m_AttackModBar.m_AttackModFinishCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
			m_AttackModBar.m_AttackModFinishCoroutine = null;
		}
		m_AttackModBar.m_AttackModFinishEnumerator = m_AttackModBar.ShowAttackModifDamage(targetSummary, abilityAttack, () =>
		{
			Log.GUIInfo("[AttackModifiers]: Finish  ShowAttackModifDamage coroutine " + (m_AttackModBar.m_AttackModFinishCoroutine != null));
			if (m_AttackModBar.m_AttackModFinishCoroutine != null)
			{
				StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
				m_AttackModBar.m_AttackModFinishCoroutine = null;
			}
		});
	}

	public void ConfirmFinishedDrawing()
	{
		m_AttackModBar.ConfirmFinishedDrawing();
	}

	public void DisplayAttackModifierDamageFlow()
	{
		if (gameObject.activeInHierarchy)
		{
			Log.GUIInfo("[AttackModifiers]: DisplayAttackModifierDamageFlow");
			Focus(focus: true);
			UnhighlightPreview();
			if (m_AttackModBar.m_AttackModFinishCoroutine != null)
			{
				StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
			}
			m_AttackModBar.m_AttackModFinishCoroutine = StartCoroutine(m_AttackModBar.m_AttackModFinishEnumerator);
		}
	}

	public void FinalizeAttackFlow()
	{
		Log.GUIInfo("[AttackModifiers]: FinalizeAttackFlow");
		if (m_AttackModBar.m_AttackModCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModCoroutine);
		}
		m_AttackModBar.m_AttackModCoroutine = null;
		if (m_AttackModBar.m_AttackModFinishCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
		}
		m_AttackModBar.m_AttackModFinishCoroutine = null;
		m_AttackModBar.FinalizeFlow();
	}

	public bool FlowControlActive()
	{
		return m_AttackModBar.IsFlowActive;
	}

	public void Destroy(Action onDestroy = null)
	{
		Focus(focus: true);
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			Log.GUIInfo("Destroyed healthbar " + m_Actor.ActorLocKey());
			onDestroy?.Invoke();
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			StartCoroutine(DestroyDelayed(onDestroy));
		}
	}

	public void Hide()
	{
		Focus(focus: true);
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			gameObject.SetActive(value: false);
			return;
		}
		StartCoroutine(WaitEndAnimation(() =>
		{
			gameObject.SetActive(value: false);
		}));
	}

	public void Show()
	{
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			gameObject.SetActive(value: true);
			return;
		}
		StartCoroutine(WaitEndAnimation(() =>
		{
			gameObject.SetActive(value: true);
		}));
	}

	private IEnumerator DestroyDelayed(Action onDestroy)
	{
		yield return new WaitWhile(() => FlowControlActive() || m_HealthBar.IsAnimated);
		Log.GUIInfo("DestroyDelayed healthbar " + m_Actor.ActorLocKey());
		onDestroy?.Invoke();
		UnityEngine.Object.Destroy(gameObject);
	}

	private IEnumerator WaitEndAnimation(Action callback)
	{
		yield return new WaitWhile(() => FlowControlActive() || m_HealthBar.IsAnimated);
		callback();
	}

	public void HighlightPreview(bool isPositive)
	{
		m_SelectionIndicador.isOn = isPositive;
		m_SelectionIndicador.gameObject.SetActive(value: true);
		Focus(focus: true, "HIGHLIGHT_PREVIEW_FOCUS");
	}

	public void UnhighlightPreview()
	{
		m_SelectionIndicador.gameObject.SetActive(value: false);
		Focus(focus: false, "HIGHLIGHT_PREVIEW_FOCUS");
	}

	public void HighlightSelected()
	{
		m_HealthBar.HighlightPreview(highlight: true);
	}

	public void UnhighlightSelected()
	{
		m_HealthBar.HighlightPreview(highlight: false);
	}

	public void ShowInfoTooltip(string tooltipTitle, string tooltipText)
	{
		m_Tooltip.SetText((tooltipText == null) ? tooltipTitle : ("<color=#" + UIInfoTools.DefInstance.mainColor.ToHex() + ">" + tooltipTitle + "</color>"), refreshTooltip: false, tooltipText);
		m_Tooltip.OnPointerEnter(null);
	}

	public void HideInfoTooltip()
	{
		m_Tooltip.OnPointerExit(null);
	}

	private void OnEnable()
	{
		WorldspaceUITools.Instance.RegisterPanel(this);
	}

	private void OnDisable()
	{
		WorldspaceUITools.Instance.UnregisterPanel(this);
		HideInfoTooltip();
		addTargetAnimation.Stop();
	}

	public void ReplaceCondition(EffectsBar.FEffect oldCondition, List<CCondition.EPositiveCondition> newConditions)
	{
		if (newConditions.Count > 0)
		{
			m_EffectsBar.ReplaceCondition(oldCondition, newConditions[0]);
		}
	}

	public void OnReturnedToPool()
	{
	}

	public void OnRemovedFromPool()
	{
		m_InfoBar.OnShow.RemoveAllListeners();
		m_InfoBar.OnHide.RemoveAllListeners();
		if ((bool)m_HealthBar)
		{
			m_HealthBar.gameObject.SetActive(value: true);
		}
		if (shownPreviewEffectPosition.HasValue)
		{
			RectTransform rectTransform = (RectTransform)m_PreviewEffectsBar.transform;
			RectTransform rectTransform2 = (RectTransform)m_PreviewEffectsBar.transform;
			Vector2 anchoredPosition = (rectTransform.anchoredPosition = shownPreviewEffectPosition.Value);
			rectTransform2.anchoredPosition = anchoredPosition;
		}
	}

	public static void ResetHighlightCaches()
	{
		m_SelectedActorCacheToAdd = new List<CActor>();
		m_SelectedActorCacheToRemove = new List<CActor>();
		m_SelectedActorCache = new List<CActor>();
		m_TargetedActorCachePositiveToAdd = new List<CActor>();
		m_TargetedActorCacheNegativeToAdd = new List<CActor>();
		m_TargetedActorCacheToRemove = new List<CActor>();
		m_TargetedActorCachePositive = new List<CActor>();
		m_TargetedActorCacheNegative = new List<CActor>();
	}

	public static void OnActorsHexSelected(List<CActor> actors = null)
	{
		if (ScenarioManager.Scenario == null)
		{
			return;
		}
		foreach (CActor actor in ScenarioManager.Scenario.AllActorsThreadSafe)
		{
			if (actors != null && actors.Contains(actor))
			{
				if (m_SelectedActorCacheToRemove.Contains(actor))
				{
					m_SelectedActorCacheToRemove.RemoveAll((CActor x) => x == actor);
				}
				if (!m_SelectedActorCache.Contains(actor) && !m_SelectedActorCacheToAdd.Contains(actor))
				{
					m_SelectedActorCacheToAdd.Add(actor);
				}
			}
			else if (!m_SelectedActorCacheToRemove.Contains(actor))
			{
				m_SelectedActorCacheToRemove.Add(actor);
			}
		}
	}

	public static void OnActorsHexTargeted<T>(List<T> actors, bool isPositive = false, bool untargetOthers = true) where T : CActor
	{
		if (ScenarioManager.Scenario == null)
		{
			return;
		}
		foreach (CActor actor in ScenarioManager.Scenario.AllActorsThreadSafe)
		{
			if (actors != null && actors.Contains(actor))
			{
				if (m_TargetedActorCacheToRemove.Contains(actor))
				{
					m_TargetedActorCacheToRemove.RemoveAll((CActor x) => x == actor);
				}
				if (isPositive)
				{
					if (!m_TargetedActorCachePositive.Contains(actor) && !m_TargetedActorCachePositiveToAdd.Contains(actor))
					{
						m_TargetedActorCachePositiveToAdd.Add(actor);
					}
				}
				else if (!m_TargetedActorCacheNegative.Contains(actor) && !m_TargetedActorCacheNegativeToAdd.Contains(actor))
				{
					m_TargetedActorCacheNegativeToAdd.Add(actor);
				}
			}
			else if (!m_TargetedActorCacheToRemove.Contains(actor))
			{
				m_TargetedActorCacheToRemove.Add(actor);
			}
		}
	}

	public static void OnActorHexTargeted(CActor enemyActor, bool isPositive = false)
	{
		OnActorsHexTargeted(new List<CActor> { enemyActor }, isPositive);
	}

	public static void OnActorsHexTargeted<T>(List<Tuple<T, bool>> actors, bool untargetOthers = true) where T : CActor
	{
		if (ScenarioManager.Scenario == null)
		{
			return;
		}
		foreach (CActor actor in ScenarioManager.Scenario.AllActorsThreadSafe)
		{
			Tuple<T, bool> tuple = actors.FirstOrDefault((Tuple<T, bool> x) => x.Item1 == actor);
			if (((tuple != null) ? tuple.Item1 : null) != null)
			{
				if (m_TargetedActorCacheToRemove.Contains(actor))
				{
					m_TargetedActorCacheToRemove.RemoveAll((CActor x) => x == actor);
				}
				if (tuple.Item2)
				{
					if (!m_TargetedActorCachePositive.Contains(actor) && !m_TargetedActorCachePositiveToAdd.Contains(actor))
					{
						m_TargetedActorCachePositiveToAdd.Add(actor);
					}
				}
				else if (!m_TargetedActorCacheNegative.Contains(actor) && !m_TargetedActorCacheNegativeToAdd.Contains(actor))
				{
					m_TargetedActorCacheNegativeToAdd.Add(actor);
				}
			}
			else if (!m_TargetedActorCacheToRemove.Contains(actor))
			{
				m_TargetedActorCacheToRemove.Add(actor);
			}
		}
	}

	public static void RefreshActorHexHighlights()
	{
		m_SelectedActorCache.AddRange(m_SelectedActorCacheToAdd);
		foreach (CActor item in m_SelectedActorCacheToRemove)
		{
			m_SelectedActorCache.Remove(item);
		}
		m_TargetedActorCachePositive.AddRange(m_TargetedActorCachePositiveToAdd);
		m_TargetedActorCacheNegative.AddRange(m_TargetedActorCacheNegativeToAdd);
		foreach (CActor item2 in m_TargetedActorCacheToRemove)
		{
			m_TargetedActorCachePositive.Remove(item2);
			m_TargetedActorCacheNegative.Remove(item2);
		}
		RefreshActorHexSelected(m_SelectedActorCacheToAdd, highlight: true);
		RefreshActorHexSelected(m_SelectedActorCacheToRemove, highlight: false);
		RefreshActorHexTargeted(m_TargetedActorCachePositiveToAdd, isPositive: true, highlight: true);
		RefreshActorHexTargeted(m_TargetedActorCacheNegativeToAdd, isPositive: false, highlight: true);
		RefreshActorHexTargeted(m_TargetedActorCacheToRemove, isPositive: false, highlight: false);
	}

	private static void RefreshActorHexSelected(List<CActor> actorsToRefersh, bool highlight)
	{
		foreach (CActor item in actorsToRefersh)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActor(item));
			if (!(actorBehaviour == null) && !(actorBehaviour.gameObject == null) && actorBehaviour.Actor != null)
			{
				if (highlight)
				{
					actorBehaviour.m_WorldspacePanelUI?.HighlightSelected();
				}
				else
				{
					actorBehaviour.m_WorldspacePanelUI?.UnhighlightSelected();
				}
			}
		}
		actorsToRefersh.Clear();
	}

	private static void RefreshActorHexTargeted(List<CActor> actorsToRefersh, bool isPositive, bool highlight)
	{
		foreach (CActor item in actorsToRefersh)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActor(item));
			if (!(actorBehaviour == null) && !(actorBehaviour.gameObject == null) && actorBehaviour.Actor != null)
			{
				if (highlight)
				{
					actorBehaviour.m_WorldspacePanelUI?.HighlightPreview(isPositive);
				}
				else
				{
					actorBehaviour.m_WorldspacePanelUI?.UnhighlightPreview();
				}
			}
		}
		actorsToRefersh.Clear();
	}
}

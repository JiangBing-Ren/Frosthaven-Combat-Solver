using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using FFSNet;
using FHGUI;
using FHGUI.Common;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class PersistentAbilitiesUI : MonoBehaviour
{
	[SerializeField]
	private GameObject persistentAbilityPrefab;

	[SerializeField]
	private Transform abilitiesHolder;

	[SerializeField]
	private Transform abilitiesHolderExtended;

	[SerializeField]
	private Image actorBackgroundImage;

	[SerializeField]
	private Transform tooltipHolder;

	[SerializeField]
	private GameObject immunityPrefab;

	[SerializeField]
	private GameObject monsterStatPrefab;

	[SerializeField]
	private GameObject passivePerkPrefab;

	[SerializeField]
	private PassivePerkPreviewUI companionPreview;

	[SerializeField]
	private List<Image> selectedImage;

	[SerializeField]
	private Image warningSelectedImage;

	[SerializeField]
	protected Sprite[] selectionSprites;

	private CActor actor;

	private List<PersistentAbilityPreviewUI> persistentAbilitiesPreviewsList = new List<PersistentAbilityPreviewUI>();

	private bool isInteractable;

	private List<ImmunityPreviewUI> immunitiesPreviewsList = new List<ImmunityPreviewUI>();

	private List<StatusEffectPreviewUI> monsterStatsPreviewsList = new List<StatusEffectPreviewUI>();

	private List<PassivePerkPreviewUI> passivesPreviewsList = new List<PassivePerkPreviewUI>();

	private const int MAX_ABILITIES_HOLDER = 5;

	private Tuple<Selectable, Selectable> navigation;

	private List<Selectable> bonuses = new List<Selectable>();

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			bool flag = value && !(actor is CEnemyActor);
			if (isInteractable != flag)
			{
				isInteractable = flag;
				RefreshInteractable(IsInteractable);
			}
		}
	}

	private void Start()
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			SubscribeToControllableChanges();
		}
		else
		{
			SubscribeToMyPlayerInitialized();
		}
	}

	private void OnDestroy()
	{
		if (FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			UnsubscribeFromControllableChanges();
		}
		else
		{
			UnsubscribeFromMyPlayerInitialized();
		}
	}

	public void Init(CActor actor, bool interactable)
	{
		this.actor = actor;
		isInteractable = interactable && !(actor is CEnemyActor);
		if (actor is CPlayerActor && actorBackgroundImage != null)
		{
			actorBackgroundImage.sprite = UIInfoTools.Instance.GetActiveAbilityIconBackground(actor);
		}
		RefreshAbilities();
	}

	private List<CActiveBonus> FilterActiveBonus(CActor actor)
	{
		return (from it in GetActorBonuses(actor)
			where !it.IsHiddenInUI() && (it.Layout != null || actor.IsMonsterType || it.IsAugment || it.IsSong || it.IsDoom)
			select it).ToList();
	}

	private List<CActiveBonus> GetActorBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		list.AddRange(CActiveBonus.FindApplicableActiveBonuses(actor));
		return list;
	}

	private CHeroSummonActor GetCompanion(CActor actor)
	{
		return ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor it) => it.Summoner.ActorGuid == actor.ActorGuid && it.IsCompanionSummon);
	}

	public void RefreshAbilities()
	{
		if (actor == null)
		{
			return;
		}
		List<CActiveBonus> list = (from x in GetActorBonuses(actor)
			where x.Layout == null
			select x).ToList();
		for (int num = 0; num < list.Count(); num++)
		{
			UIActiveAbility.CheckForLayoutToGenerate(list[num]);
		}
		List<CActiveBonus> list2 = new List<CActiveBonus>();
		list2.AddRange(FilterActiveBonus(actor));
		List<List<CActiveBonus>> list3 = new List<List<CActiveBonus>>();
		foreach (IGrouping<string, CActiveBonus> item in from x in list2
			where !x.Ability.ActiveBonusData.Hidden && (x.Caster == actor || !x.Ability.ActiveBonusData.HiddenWhenAura)
			group x by x.BaseCard.Name)
		{
			if (item.Any((CActiveBonus x) => x.IsDoom))
			{
				list3.Add(new List<CActiveBonus> { item.First((CActiveBonus x) => x.IsDoom) });
				continue;
			}
			foreach (IGrouping<Guid, CActiveBonus> item2 in from x in item
				group x by x.Ability.ID)
			{
				list3.Add(item2.ToList());
			}
		}
		int num2 = list3.Count;
		bonuses.Clear();
		HelperTools.NormalizePool(ref persistentAbilitiesPreviewsList, persistentAbilityPrefab, abilitiesHolder, num2);
		int num3 = 0;
		foreach (List<CActiveBonus> activeBonusGroupEntry in list3)
		{
			int instances = ((activeBonusGroupEntry[0].Type() == CAbility.EAbilityType.Summon || activeBonusGroupEntry[0].IsDoom) ? 1 : activeBonusGroupEntry.Count);
			PersistentAbilityPreviewUI slot = persistentAbilitiesPreviewsList[num3];
			slot.SetAbilityBonus(activeBonusGroupEntry.FirstOrDefault(), actor, tooltipHolder, () =>
			{
				OnCancelActiveAbility(activeBonusGroupEntry, actor, () =>
				{
					OnFinishedCancel(slot);
				});
			}, instances);
			slot.descriptionPopup.activeAbility.RefreshTracker();
			slot.SetInteractable(isInteractable);
			slot.transform.SetParent((num3 >= 5) ? abilitiesHolderExtended : abilitiesHolder);
			bonuses.Add(slot.Selectable);
			num3++;
		}
		List<KeyValuePair<string, string>> list4 = GetMonsterOrSummonBaseStats(actor).ToList();
		HelperTools.NormalizePool(ref monsterStatsPreviewsList, monsterStatPrefab, abilitiesHolder, list4.Count);
		for (int num4 = 0; num4 < list4.Count; num4++)
		{
			monsterStatsPreviewsList[num4].SetEffect("$Innate$", CreateLayout.LocaliseText(list4[num4].Key), UIInfoTools.Instance.GetActiveAbilityIcon(list4[num4].Value), tooltipHolder);
			monsterStatsPreviewsList[num4].transform.SetParent((num3 >= 5) ? abilitiesHolderExtended : abilitiesHolder);
			bonuses.Add(monsterStatsPreviewsList[num4].GetComponentInChildren<Selectable>());
			num3++;
		}
		if (actor is CEnemyActor)
		{
			StatusEffectDTO[] array = UICharacterUtils.GetImmunities(actor).ToArray();
			HelperTools.NormalizePool(ref immunitiesPreviewsList, immunityPrefab, abilitiesHolder, array.Length);
			if (array.Length != 0)
			{
				for (int num5 = 0; num5 < array.Length; num5++)
				{
					immunitiesPreviewsList[num5].SetImmunity(array[num5].ID, tooltipHolder);
					immunitiesPreviewsList[num5].transform.SetParent((num3 >= 5) ? abilitiesHolderExtended : abilitiesHolder);
					num3++;
					bonuses.Add(immunitiesPreviewsList[num5].GetComponentInChildren<Selectable>());
				}
			}
			num2 += array.Length + list4.Count;
			actorBackgroundImage.enabled = num2 > list4.Count;
			goto IL_0792;
		}
		List<Tuple<Sprite, string, string, Action, bool>> customEffects = GetCustomEffects(actor);
		List<CharacterPerk> list5 = new List<CharacterPerk>();
		if (AdventureState.MapState != null)
		{
			CMapScenarioState currentMapScenarioState = AdventureState.MapState.CurrentMapScenarioState;
			if (currentMapScenarioState != null && currentMapScenarioState.MapScenarioType == EMapScenarioType.Custom)
			{
				CMapScenarioState currentMapScenarioState2 = AdventureState.MapState.CurrentMapScenarioState;
				if (currentMapScenarioState2 == null || currentMapScenarioState2.CustomLevelData.PartySpawnType != ELevelPartyChoiceType.LoadAdventureParty)
				{
					goto IL_0567;
				}
			}
			CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == actor.Class.ID);
			if (cMapCharacter != null)
			{
				list5 = cMapCharacter.GetActivePassivePerks();
			}
		}
		goto IL_0567;
		IL_0792:
		gameObject.SetActive(num2 > 0);
		abilitiesHolderExtended.gameObject.SetActive(num2 > 5);
		if (selectionSprites.Length > 1)
		{
			for (int num6 = 0; num6 < selectedImage.Count; num6++)
			{
				selectedImage[num6].sprite = (gameObject.activeSelf ? selectionSprites[1] : selectionSprites[0]);
			}
		}
		if (warningSelectedImage != null && selectionSprites.Length > 1)
		{
			warningSelectedImage.sprite = (gameObject.activeSelf ? selectionSprites[1] : selectionSprites[0]);
		}
		return;
		IL_0567:
		HelperTools.NormalizePool(ref passivesPreviewsList, passivePerkPrefab, abilitiesHolder, list5.Count + customEffects.Count);
		for (int num7 = 0; num7 < list5.Count; num7++)
		{
			Sprite image = null;
			if ((list5[num7].Perk.Flags & EPerkFlag.IgnoreNegativeItemEffects) != 0)
			{
				image = UIInfoTools.Instance.IgnoreNegativeItemEffectsIcon;
			}
			else if ((list5[num7].Perk.Flags & EPerkFlag.IgnoreNegativeScenarioEffects) != 0)
			{
				image = UIInfoTools.Instance.IgnoreNegativeScenarioEffectsIcon;
			}
			passivesPreviewsList[num7].SetPassive(image, list5[num7].Perk.Name, list5[num7].Perk.Description, tooltipHolder);
			passivesPreviewsList[num7].transform.SetParent((num3 >= 5) ? abilitiesHolderExtended : abilitiesHolder);
			passivesPreviewsList[num7].SetInteractable(interactable: false);
			bonuses.Add(passivesPreviewsList[num7].Selectable);
			num3++;
			num2++;
		}
		for (int num8 = 0; num8 < customEffects.Count; num8++)
		{
			PassivePerkPreviewUI passivePerkPreviewUI = passivesPreviewsList[num8 + list5.Count];
			passivePerkPreviewUI.SetPassive(customEffects[num8].Item1, customEffects[num8].Item2, customEffects[num8].Item3, tooltipHolder, customEffects[num8].Item4, localizeKeys: true, customEffects[num8].Item5);
			passivePerkPreviewUI.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl()));
			bonuses.Add(passivePerkPreviewUI.Selectable);
			passivePerkPreviewUI.transform.SetParent((num3 >= 5) ? abilitiesHolderExtended : abilitiesHolder);
			num3++;
			num2++;
		}
		actorBackgroundImage.enabled = true;
		RefreshCompanion();
		goto IL_0792;
	}

	private void OnFinishedCancel(PersistentAbilityPreviewUI slot)
	{
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			if (bonuses.Contains(slot.Selectable))
			{
				EventSystem.current.SetSelectedGameObject(slot.Selectable.gameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(slot.Selectable.navigation.selectOnLeft?.gameObject);
			}
		}
	}

	public void RefreshCompanion()
	{
		CHeroSummonActor companion = GetCompanion(actor);
		if (companion == null)
		{
			companionPreview.gameObject.SetActive(value: false);
			return;
		}
		HeroSummonYMLData summonData = companion.SummonData;
		PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(summonData.ID);
		Sprite image = previewEffectConfig?.previewEffectIcon ?? UIInfoTools.Instance.GetCharacterSprite(((CPlayerActor)actor).CharacterClass.CharacterModel, highlight: false, ((CPlayerActor)actor).CharacterClass.CharacterYML.CustomCharacterConfig);
		string descriptionKey = string.Format((previewEffectConfig != null && previewEffectConfig.previewEffectText != null) ? LocalizationManager.GetTranslation(previewEffectConfig.previewEffectText) : LocalizationManager.GetTranslation(summonData.Model + "_TOOLTIP"), companion.MaxHealth, summonData.Move, summonData.Attack, (summonData.Range < 2) ? "-" : summonData.Range.ToString());
		companionPreview.gameObject.SetActive(value: true);
		companionPreview.SetPassive(image, LocalizationManager.GetTranslation(summonData.LocKey), descriptionKey, null, null, localizeKeys: false);
	}

	private List<Tuple<Sprite, string, string, Action, bool>> GetCustomEffects(CActor actor)
	{
		List<Tuple<Sprite, string, string, Action, bool>> list = new List<Tuple<Sprite, string, string, Action, bool>>();
		CPlayerActor playerActor = actor as CPlayerActor;
		if (playerActor != null)
		{
			if (playerActor.CharacterClass.ImprovedShortRest && PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				list.Add(new Tuple<Sprite, string, string, Action, bool>(UIInfoTools.Instance.GetCharacterSprite(playerActor.CharacterClass.CharacterModel, highlight: false, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig), "GUI_IMPROVED_SHORT_REST_TITLE_TOOLTIP", "GUI_IMPROVED_SHORT_REST_ACTIVE_BONUS_TOOLTIP", () =>
				{
					Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation("GUI_IMPROVED_SHORT_REST_TITLE_TOOLTIP"), lost: false, () =>
					{
						Singleton<CardsHandManager>.Instance.CancelImprovedShortRest((CPlayerActor)actor);
						if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl())
						{
							int controllableID = ControllableRegistry.GetControllableID(playerActor);
							Synchronizer.ReplicateControllableStateChange(GameActionType.CancelImprovedShortRest, ActionProcessor.CurrentPhase, controllableID);
						}
					});
				}, item5: true));
			}
			for (int num = 0; num < playerActor.CarriedQuestItems.Count; num++)
			{
				CObjectProp cObjectProp = playerActor.CarriedQuestItems[num];
				if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
				{
					list.Add(new Tuple<Sprite, string, string, Action, bool>(UIInfoTools.Instance.CarryableQuestItemIconSprite, FHUILocFormatting.GetScenarioQuestItemName(cObjectProp), FHUILocFormatting.GetScenarioQuestItemDesc(cObjectProp), null, item5: false));
				}
			}
		}
		return list;
	}

	private void OnCancelActiveAbility(List<CActiveBonus> bonuses, CActor actor, UnityAction onFinished)
	{
		CActiveBonus cActiveBonus = bonuses.FirstOrDefault((CActiveBonus it) => it.Ability.ActiveBonusData.CannotCancel);
		if (cActiveBonus != null)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation(cActiveBonus.Ability.GetAbilityBaseCard().Name), "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_CANNOT_CANCEL_ACTIVE_BONUS_BEFORE_END_ROUND") + "</color>", null, "GUI_CONTINUE", onFinished);
		}
		else
		{
			for (int num = 0; num < bonuses.Count; num++)
			{
				OnCancelActiveAbility(bonuses[num], actor, (num == bonuses.Count - 1) ? onFinished : null);
			}
		}
	}

	private void OnCancelActiveAbility(CActiveBonus bonus, CActor actor, UnityAction onFinished)
	{
		if (bonus.Ability.ActiveBonusData.CannotCancel)
		{
			UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
			string translation = LocalizationManager.GetTranslation(bonus.Ability.GetAbilityBaseCard().Name);
			string explanation = "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_CANNOT_CANCEL_ACTIVE_BONUS_BEFORE_END_ROUND") + "</color>";
			UnityAction onFinished2 = onFinished;
			instance.ShowGenericCancelConfirmation(translation, explanation, null, "GUI_CONTINUE", onFinished2);
		}
		else if (bonus.BaseCard is CItem cItem)
		{
			if (cItem.YMLData.Trigger == CItem.EItemTrigger.PassiveEffect)
			{
				return;
			}
			Singleton<UIConfirmationBoxManager>.Instance.ShowCancelActiveAbility(LocalizationManager.GetTranslation(bonus.BaseCard.Name), cItem.YMLData.Usage == CItem.EUsageType.Consumed, () =>
			{
				CClass.CancelActiveBonus(bonus);
				RefreshAbilities();
				if (actor is CPlayerActor playerActor)
				{
					Singleton<CardsHandManager>.Instance.NetworkCancelingActiveBonuses(playerActor, bonus.BaseCard);
				}
				else
				{
					Log.NetworkInfo("Trying to cancel an item based active ability on a non-playerActor.");
				}
				onFinished?.Invoke();
			}, null, onFinished);
		}
		else if (bonus.BaseCard != null && bonus.BaseCard is CAbilityCard card)
		{
			Singleton<CardsHandManager>.Instance.DiscardActiveAbilityCard((CPlayerActor)actor, card, onFinished);
		}
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
		persistentAbilitiesPreviewsList.SingleOrDefault((PersistentAbilityPreviewUI s) => s.gameObject.activeSelf && s.ActiveBonus == activeBonus)?.OnActiveBonusTriggered();
	}

	public void RefreshInteractable(bool isInteractable)
	{
		foreach (PersistentAbilityPreviewUI persistentAbilitiesPreviews in persistentAbilitiesPreviewsList)
		{
			persistentAbilitiesPreviews.SetInteractable(isInteractable);
		}
		passivesPreviewsList.ForEach((PassivePerkPreviewUI x) =>
		{
			x.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl()));
		});
	}

	public static Dictionary<string, string> GetMonsterOrSummonBaseStats(CActor actor, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		if (actor is CEnemyActor cEnemyActor)
		{
			result = BuildMonsterBaseStatsDict(cEnemyActor.MonsterClass.CurrentMonsterStat, cEnemyActor.MonsterClass.OriginalBaseStats, additionalConditions);
		}
		else if (actor is CHeroSummonActor summonActor)
		{
			result = BuildSummonBaseStatsDict(summonActor);
		}
		return result;
	}

	public static Dictionary<string, string> BuildMonsterBaseStatsDict(CMonsterClass monsterClass, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		return BuildMonsterBaseStatsDict(monsterClass.CurrentMonsterStat, monsterClass.OriginalBaseStats, additionalConditions);
	}

	public static Dictionary<string, string> BuildMonsterBaseStatsDict(BaseStats currentStats, BaseStats originalStats, List<CCondition.ENegativeCondition> additionalConditions = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (currentStats.Invulnerable)
		{
			dictionary.Add("$ScenarioAbility_Invulnerable$", "FH_ICO_Immune");
		}
		if (currentStats.Untargetable)
		{
			dictionary.Add("$ScenarioAbility_Untargetable$", "Immune");
		}
		if (currentStats.PierceInvulnerablility)
		{
			dictionary.Add("$ScenarioAbility_PierceInvulnerable$", "Pierce");
		}
		if (currentStats.Target > 1)
		{
			dictionary.Add("$Target$ <sprite name=Target> " + currentStats.Target, "Target");
		}
		if (currentStats.Pierce > 0)
		{
			dictionary.Add("$Pierce$ <sprite name=Pierce> " + currentStats.Pierce, "Pierce");
		}
		if (currentStats.Push > 0)
		{
			dictionary.Add("$Push$ <sprite name=Push> " + currentStats.Push, "Push");
		}
		if (currentStats.Flying)
		{
			dictionary.Add("$Flying$", "Flying");
		}
		if (currentStats.Advantage)
		{
			dictionary.Add("$Advantage$", "Advantage");
		}
		if (currentStats.AttackersGainDisadvantage)
		{
			dictionary.Add("$AttackersGainDisadvantage$", "Disadvantage");
		}
		if (currentStats.IsWaterMonster)
		{
			dictionary.Add("$monsterability_lightningeel$", "IsWaterMonster");
		}
		if (currentStats.Shield > 0 && (currentStats.ShieldStatIsBasedOnXEntries == null || !currentStats.ShieldStatIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Shield)))
		{
			string text = currentStats.Shield.ToString();
			string key = "$Shield$ <sprite name=Shield> " + text;
			dictionary.Add(key, "Shield");
		}
		if (currentStats.Retaliate > 0)
		{
			string text2 = "$Retaliate$ <sprite name=Retaliate> " + currentStats.Retaliate;
			text2 += ((currentStats.RetaliateRange > 1) ? (", $Retaliate$ $Range$ <sprite name=Range> " + currentStats.RetaliateRange) : "");
			dictionary.Add(text2, "Retaliate");
		}
		if (currentStats.Conditions.Count > 0 || (additionalConditions != null && additionalConditions.Count > 0))
		{
			List<CCondition.ENegativeCondition> list = currentStats.Conditions.ToList();
			if (additionalConditions != null)
			{
				list.AddRange(additionalConditions);
				list = list.Distinct().ToList();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("$AttacksApply$ ");
			foreach (CCondition.ENegativeCondition item in list)
			{
				stringBuilder.Append("<nobr>$" + item.ToString() + "$ <sprite name=" + item.ToString() + "></nobr>");
				if (list.IndexOf(item) != list.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			dictionary.Add(stringBuilder.ToString(), "Attack");
		}
		if (currentStats.AttackStatIsBasedOnXEntries != null || currentStats.ShieldStatIsBasedOnXEntries != null)
		{
			List<AbilityData.StatIsBasedOnXData> list2 = new List<AbilityData.StatIsBasedOnXData>();
			if (currentStats.AttackStatIsBasedOnXEntries != null)
			{
				list2.AddRange(currentStats.AttackStatIsBasedOnXEntries.Where((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength));
			}
			if (currentStats.ShieldStatIsBasedOnXEntries != null)
			{
				list2.AddRange(currentStats.ShieldStatIsBasedOnXEntries);
			}
			foreach (AbilityData.StatIsBasedOnXData item2 in list2)
			{
				switch (item2.BasedOn)
				{
				case CAbility.EStatIsBasedOnXType.HexesMovedThisTurn:
					dictionary.Add("$Boss_AddHexesMovedToAttackBuffText$", "Attack");
					break;
				case CAbility.EStatIsBasedOnXType.ActorCount:
				case CAbility.EStatIsBasedOnXType.DeadActorCount:
				{
					string text3 = null;
					string className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterEnemyClasses?.FirstOrDefault();
					if (!className.IsNullOrEmpty())
					{
						MonsterYMLData monsterYMLData = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == className);
						if (monsterYMLData != null)
						{
							text3 = monsterYMLData.LocKey;
						}
						else
						{
							Log.GUIError("Unable to find monster class " + className);
						}
					}
					else
					{
						className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterPlayerClasses?.FirstOrDefault();
						if (!className.IsNullOrEmpty())
						{
							CharacterYMLData characterYMLData = ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData s) => s.ID == className);
							if (characterYMLData != null)
							{
								text3 = characterYMLData.LocKey;
							}
							else
							{
								Log.GUIError("Unable to find character class " + className);
							}
						}
						else
						{
							className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterHeroSummonClasses?.FirstOrDefault();
							if (!className.IsNullOrEmpty())
							{
								HeroSummonYMLData heroSummonYMLData = ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == className);
								if (heroSummonYMLData != null)
								{
									text3 = heroSummonYMLData.LocKey;
								}
								else
								{
									Log.GUIError("Unable to find Hero Summon class " + className);
								}
							}
							else
							{
								className = item2?.Filter?.AbilityFilters.FirstOrDefault().FilterObjectClasses?.FirstOrDefault();
								if (!className.IsNOTNullOrEmpty())
								{
									MonsterYMLData monsterYMLData2 = ScenarioRuleClient.SRLYML.Monsters.SingleOrDefault((MonsterYMLData s) => s.ID == className);
									if (monsterYMLData2 != null)
									{
										text3 = monsterYMLData2.LocKey;
									}
								}
							}
						}
					}
					if (!text3.IsNOTNullOrEmpty())
					{
						break;
					}
					if (item2.BaseStatType == EMonsterBaseStats.Attack)
					{
						string text4 = "$Boss_BaseAttackStrengthIsEqualTo$ ";
						text4 += string.Format(LocalizationManager.GetTranslation("Boss_NumberOfXsPresent"), LocalizationManager.GetTranslation(text3));
						if (item2.Multiplier != 1f)
						{
							text4 = text4 + " $times$ " + item2.Multiplier;
						}
						dictionary.Add(text4, "Attack");
					}
					else if (item2.BaseStatType == EMonsterBaseStats.Shield)
					{
						string key2 = string.Format(LocalizationManager.GetTranslation((item2.BasedOn == CAbility.EStatIsBasedOnXType.DeadActorCount) ? "INNATE_ShieldBasedOnNumberOfDeadXsPresent" : "INNATE_ShieldBasedOnNumberOfXsPresent"), (originalStats.Shield > 0) ? originalStats.Shield : 0, (item2.Multiplier >= 0f) ? " $plus$ " : " $minus$ ", LocalizationManager.GetTranslation(text3));
						dictionary.Add(key2, "Shield");
					}
					break;
				}
				}
			}
		}
		if (currentStats.OnDeathAbilities != null && currentStats.OnDeathAbilities.Count > 0)
		{
			foreach (CAbility item3 in currentStats.OnDeathAbilities.Where((CAbility x) => x.AbilityText.IsNOTNullOrEmpty()))
			{
				string locText = "$" + item3.AbilityText + "$";
				locText = CreateLayout.LocaliseText(locText);
				locText = string.Format(locText, item3.Strength);
				dictionary.Add(locText, "Kill");
			}
		}
		return dictionary;
	}

	public static Dictionary<string, string> BuildSummonBaseStatsDict(CHeroSummonActor summonActor)
	{
		return BuildSummonBaseStatsDict(summonActor.SummonData, summonActor);
	}

	public static Dictionary<string, string> BuildSummonBaseStatsDict(HeroSummonYMLData summon, CHeroSummonActor summonActor = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (summon.Flying)
		{
			dictionary.Add("$Flying$", "Flying");
		}
		if (summon.Shield > 0)
		{
			dictionary.Add("$Shield$ " + summon.Shield, "Shield");
		}
		if (summon.Retaliate > 0)
		{
			dictionary.Add("$Retaliate$ " + summon.Retaliate + ((summon.RetaliateRange > 1) ? (", $Retaliate$ $Range$ <sprite name=Range> " + summon.RetaliateRange) : ""), "Retaliate");
		}
		int pierceFromActor = summon.GetPierceFromActor(summonActor);
		if (pierceFromActor > 0)
		{
			dictionary.Add("$Pierce$ " + pierceFromActor, "Pierce");
		}
		if (summon.AttackStatIsBasedOnXEntries != null)
		{
			foreach (AbilityData.StatIsBasedOnXData attackStatIsBasedOnXEntry in summon.AttackStatIsBasedOnXEntries)
			{
				if (attackStatIsBasedOnXEntry.BasedOn == CAbility.EStatIsBasedOnXType.PercentageOfCurrentHP && attackStatIsBasedOnXEntry.AbilityStatType == CAbility.EAbilityStatType.Strength)
				{
					dictionary.Add(string.Format(LocalizationManager.GetTranslation("INNATE_StrengthIsPercentageOfCurrentHP"), ((int)(attackStatIsBasedOnXEntry.Multiplier * 100f)).ToString()), "Attack");
				}
			}
		}
		List<CCondition.ENegativeCondition> onAttackConditions = summon.OnAttackConditions;
		if (onAttackConditions != null && onAttackConditions.Count > 0)
		{
			foreach (CCondition.ENegativeCondition item in summon.OnAttackConditions.ToList())
			{
				dictionary.Add($"$AttacksApply$ <nobr>${item}$ <sprite name=\"{item}\"></nobr>", "Attack");
			}
		}
		return dictionary;
	}

	private void SubscribeToMyPlayerInitialized()
	{
		Log.NetworkInfo("Active Ability: Subscribing to MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.AddListener(SubscribeToControllableChanges);
	}

	private void UnsubscribeFromMyPlayerInitialized()
	{
		Log.NetworkInfo("Active Ability: Unsubscribing from MyPlayerInitialized.");
		PlayerRegistry.MyPlayerInitialized.RemoveListener(SubscribeToControllableChanges);
	}

	private void SubscribeToControllableChanges()
	{
		Log.NetworkInfo("Active Ability: Subscribing to controllable changes.");
	}

	private void UnsubscribeFromControllableChanges()
	{
		Log.NetworkInfo("Active Ability: Unsubscribing from controllable changes.");
	}

	private void OnControllableOwnershipChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		persistentAbilitiesPreviewsList.ForEach((PersistentAbilityPreviewUI x) =>
		{
			x.SetInteractable(isInteractable);
		});
		passivesPreviewsList.ForEach((PassivePerkPreviewUI x) =>
		{
			x.SetInteractable(isInteractable && (!FFSNetwork.IsOnline || actor.IsUnderMyControl()));
		});
	}
}

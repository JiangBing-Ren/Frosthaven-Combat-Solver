using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class UIActiveAbility : MonoBehaviour
{
	[SerializeField]
	private TMP_Text abilityName;

	[SerializeField]
	private UIActiveAbilityDescription activeBonusDescription;

	[SerializeField]
	private GameObject abilityDescriptionHolder;

	[SerializeField]
	private PersistentAbilityTracker abilityTracker;

	[SerializeField]
	private UIActiveAbilityDescription activeBonusDescriptionPrefab;

	[SerializeField]
	private UIGeneralInfoSection MonsterCards;

	public CActiveBonus ActiveBonus;

	private List<UIActiveAbilityDescription> extraDescriptionsUI = new List<UIActiveAbilityDescription>();

	public PersistentAbilityTracker Tracker => abilityTracker;

	public void Initialize(ActiveAbilityDTO dto)
	{
		ActiveBonus = dto.Bonus;
		RecycleCards();
		gameObject.SetActive(value: true);
		abilityName.text = dto.Name;
		abilityName.enabled = name.IsNOTNullOrEmpty();
		activeBonusDescription.Init(dto.Description, dto.Icon, dto.Duration, dto.Discard, dto.Instances);
		HelperTools.NormalizePool(ref extraDescriptionsUI, activeBonusDescriptionPrefab.gameObject, abilityDescriptionHolder.transform, 0);
		RefreshTracker();
	}

	public void Initialize(CActiveBonus activeBonus, CActor contextActor = null, int instances = 1, bool forceGeneration = false)
	{
		if (ActiveBonus != activeBonus || forceGeneration)
		{
			ActiveBonus = activeBonus;
			RecycleCards();
			GenerateDescriptionLayout(instances, contextActor, forceGeneration);
			string activeBonusName = ActiveBonusUIUtils.GetActiveBonusName(activeBonus, contextActor, forceGeneration);
			abilityName.text = activeBonusName;
			abilityName.enabled = activeBonusName.IsNOTNullOrEmpty();
			if (ActiveBonus.Layout == null)
			{
				gameObject.SetActive(value: false);
				HelperTools.NormalizePool(ref extraDescriptionsUI, activeBonusDescriptionPrefab.gameObject, abilityDescriptionHolder.transform, 0);
			}
			else
			{
				gameObject.SetActive(value: true);
				RefreshTracker();
			}
		}
	}

	public void RefreshTracker()
	{
		if (abilityTracker != null)
		{
			abilityTracker.Init(ActiveBonus);
		}
	}

	private void GenerateDescriptionLayout(int instances, CActor contextActor = null, bool forceGeneration = false)
	{
		if (forceGeneration && ActiveBonus.Layout == null)
		{
			ActiveBonusUIUtils.CheckForLayoutToGenerate(ActiveBonus, out var _, contextActor);
		}
		if (ActiveBonus.Layout == null || (ActiveBonus.Ability.ActiveBonusData.Hidden && !forceGeneration))
		{
			return;
		}
		int count = ActiveBonus.Layout.IconNames.Count;
		int count2 = ActiveBonus.Layout.ListLayouts.Count;
		int num = Mathf.Max(count, count2);
		CardLayoutGroup.SummonLayout summonLayout = null;
		if (ActiveBonus?.Layout?.Layout?.Collection != null)
		{
			summonLayout = ActiveBonus.Layout.Layout.Collection.SingleOrDefault((CardLayoutGroup x) => x.Summon != null)?.Summon;
		}
		string description = ActiveBonusUIUtils.GetActiveBonusDescription(ActiveBonus, contextActor);
		int summonID = 0;
		if (ActiveBonus.Actor is CHeroSummonActor cHeroSummonActor)
		{
			summonID = cHeroSummonActor.ID;
		}
		string text = ActiveBonus.Layout.IconNames.FirstOrDefault();
		Sprite icon = (text.IsNOTNullOrEmpty() ? UIInfoTools.Instance.GetActiveAbilityIcon(text) : ActiveBonusUIUtils.GetActiveBonusIcon(ActiveBonus, contextActor));
		activeBonusDescription.Init(description, icon, ActiveBonus.Duration, ActiveBonus.Layout.Discard, instances, summonLayout, summonID);
		HelperTools.NormalizePool(ref extraDescriptionsUI, activeBonusDescriptionPrefab.gameObject, abilityDescriptionHolder.transform, Mathf.Max(num - 1, 0));
		abilityDescriptionHolder.SetActive(num > 1);
		for (int num2 = 1; num2 < num; num2++)
		{
			string description2 = ((num2 < count2) ? ActiveBonusUIUtils.ActivePropertyLookup(ActiveBonus, ActiveBonusUIUtils.LocalisationAndPropertyLookup(ActiveBonus, ActiveBonus.Layout.ListLayouts[num2])) : string.Empty);
			extraDescriptionsUI[num2 - 1].Init(description2, (num2 < count) ? ActiveBonus.Layout.IconNames[num2] : null, ActiveBonus.Duration, ActiveBonus.Layout.Discard);
		}
	}

	public static string CheckForLayoutToGenerate(CActiveBonus activeBonus)
	{
		ActiveBonusUIUtils.CheckForLayoutToGenerate(activeBonus, out var title);
		return title;
	}

	private void RecycleCards()
	{
		if (!(MonsterCards == null))
		{
			MonsterRoundCardUI[] componentsInChildren = MonsterCards.Content.GetComponentsInChildren<MonsterRoundCardUI>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				ObjectPool.RecycleCard(componentsInChildren[num].CardID, ObjectPool.ECardType.Monster, componentsInChildren[num].gameObject);
			}
			MonsterCards.gameObject.SetActive(value: false);
			MonsterCards.Fill<RectTransform>(showLabel: false, showContent: false);
		}
	}

	public void ShowMonsterCards(List<CMonsterAbilityCard> cards, CMonsterClass monsterClass, string labelKey = null)
	{
		if (MonsterCards == null)
		{
			return;
		}
		MonsterCards.gameObject.SetActive(value: true);
		if (labelKey != null)
		{
			MonsterCards.Label.SetTextKey(labelKey);
		}
		MonsterCards.Fill(labelKey != null, cards.Count > 0, (RectTransform rectTransform) =>
		{
			foreach (CMonsterAbilityCard card in cards)
			{
				int iD = card.ID;
				MonsterCardUIContext context = new MonsterCardUIContext
				{
					CardYML = card.GetMonsterCardYML,
					EnemyForBaseStats = null,
					FallbackMonsterClass = monsterClass,
					ShowFaction = UICharacterUtils.HasFactions(),
					CardPile = CardPileType.Hand
				};
				ObjectPool.SpawnCard(iD, ObjectPool.ECardType.Monster, rectTransform, resetLocalScale: true).GetComponent<MonsterRoundCardUI>().Init(context);
			}
		});
	}
}

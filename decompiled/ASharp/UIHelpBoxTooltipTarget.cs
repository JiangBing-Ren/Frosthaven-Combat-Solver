using FHGUI.Common;
using FHGUI.Common.Tooltips;
using FHGUI.DTO;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIHelpBoxTooltipTarget : UITooltipTarget
{
	public delegate void GetTooltipDataDelegate(out CAbility ability, out CBaseCard card);

	private GameObject tooltipObj;

	private AbilityCardUI cardUI;

	private MonsterRoundCardUI monsterCardUI;

	private FHItemTooltip itemCardUI;

	private UIStatusEffect perkUI;

	private GetTooltipDataDelegate GetTooltipData;

	private void Awake()
	{
		autoAdjustHeight = false;
		screenBound = true;
		anchoredPositionOffset = new Vector2(0f, 350f);
	}

	public void Initialize(GetTooltipDataDelegate dataDelegate)
	{
		GetTooltipData = dataDelegate;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		if (GetTooltipData == null)
		{
			GetTooltipData = HelpBox.GetTooltipData;
		}
		GetTooltipData(out var ability, out var card);
		if (ability != null || card != null)
		{
			if (card != null && card.CardType == CBaseCard.ECardType.CharacterAbility)
			{
				ShowCharacterAbilityCardTooltip(ability, card);
			}
			else if (card != null && card.CardType == CBaseCard.ECardType.MonsterAbility)
			{
				ShowMonsterAbilityCardTooltip(ability, card);
			}
			else if ((card != null && card.CardType == CBaseCard.ECardType.Item) || (card != null && card.CardType == CBaseCard.ECardType.Pet))
			{
				ShowItemTooltip(ability, card);
			}
			else if ((card == null || card.CardType != CBaseCard.ECardType.HeroSummonAbility) && ability?.perkData != null)
			{
				ShowPerkTooltip(ability);
				base.ShowTooltip(delay);
				return;
			}
			if (!(tooltipObj == null))
			{
				base.ShowTooltip(delay);
			}
		}
	}

	private void ShowCharacterAbilityCardTooltip(CAbility ability, CBaseCard card)
	{
		Log.GUIInfo("Show HelpBox Character Ability Card Tooltip");
		tooltipObj = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Ability, UITooltipManager.GetTransform(), resetLocalScale: true, resetToMiddle: true);
		cardUI = tooltipObj.GetComponent<AbilityCardUI>();
		cardUI.Init(card as CAbilityCard, disableEventDetection: true);
		CBaseCard.ActionType abilityActionType = (card as CAbilityCard).GetAbilityActionType(ability);
		cardUI.fullAbilityCard.ToggleHighlightHover(active: true, abilityActionType == CBaseCard.ActionType.TopAction, isDefault: false);
		cardUI.fullAbilityCard.SetInteractable(abilityActionType == CBaseCard.ActionType.TopAction, CBaseCard.ActionType.TopAction);
		cardUI.fullAbilityCard.SetInteractable(abilityActionType == CBaseCard.ActionType.BottomAction, CBaseCard.ActionType.BottomAction);
		if (!string.IsNullOrEmpty(ability?.Name))
		{
			cardUI.fullAbilityCard.OnAbilityStart(ability.Name);
		}
		UITooltipManager.ShowBackground(show: false);
	}

	private void ShowMonsterAbilityCardTooltip(CAbility ability, CBaseCard card)
	{
		Log.GUIInfo("Show HelpBox Monster Ability Card Tooltip");
		CEnemyActor cEnemyActor = ability?.ControllingActor as CEnemyActor;
		if (cEnemyActor == null)
		{
			cEnemyActor = ability?.TargetingActor as CEnemyActor;
		}
		if (cEnemyActor != null)
		{
			tooltipObj = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Monster, UITooltipManager.GetTransform(), resetLocalScale: true, resetToMiddle: true);
			monsterCardUI = tooltipObj.GetComponent<MonsterRoundCardUI>();
			monsterCardUI.Init((card as CMonsterAbilityCard).GetMonsterCardYML, cEnemyActor, UICharacterUtils.HasFactions());
			UITooltipManager.ShowBackground(show: false);
		}
	}

	private void ShowItemTooltip(CAbility ability, CBaseCard card)
	{
		Log.GUIInfo("Show HelpBox Item Tooltip");
		if (card is CItem item)
		{
			FHItemTooltip itemTooltip = UIInfoTools.Instance.ItemTooltip;
			UITooltipManager.ResetContent();
			tooltipObj = ObjectPool.Spawn(itemTooltip.GameObject, UITooltipManager.GetTransform(), Vector3.zero, Quaternion.identity);
			itemCardUI = tooltipObj.GetComponent<FHItemTooltip>();
			ItemDTOUtilities.GetFullItemInfo(item, out var itemData, out var flipItem);
			itemCardUI.Show(new FHItemTooltip.ItemTooltipParams
			{
				ItemData = itemData,
				FlipItem = flipItem
			});
			UIUtility.ForceRecalclulateLayouts(itemCardUI.gameObject.transform as RectTransform);
			UITooltipManager.ShowBackground(show: false);
		}
	}

	private void ShowPerkTooltip(CAbility ability)
	{
		Log.GUIInfo("Show HelpBox Perk Tooltip");
		PerksYMLData perksYMLData = ability?.perkData;
		string translation = LocalizationManager.GetTranslation(perksYMLData.Name);
		string translation2 = LocalizationManager.GetTranslation(perksYMLData.Description);
		Sprite perkIcon = UIPlayerCharacterUtils.GetPerkIcon(perksYMLData, perksYMLData.CharacterID);
		GameObject tooltipPrefab = UIInfoTools.Instance.PerksUIData.TooltipPrefab;
		tooltipObj = Object.Instantiate(tooltipPrefab, UITooltipManager.GetTransform(), worldPositionStays: false);
		perkUI = tooltipObj.GetComponent<UIStatusEffect>();
		perkUI.Initialize(translation, perkIcon, UIInfoTools.DefInstance.basicTextColor, translation2);
		UITooltipManager.ShowBackground(show: false);
	}

	public override void HideTooltip(float delay = -1f)
	{
		Log.GUIInfo("Hide HelpBox Tooltip");
		base.HideTooltip(delay);
		if (!(tooltipObj == null))
		{
			if (cardUI != null)
			{
				ObjectPool.RecycleCard(cardUI.CardID, ObjectPool.ECardType.Ability, tooltipObj);
				cardUI = null;
			}
			if (monsterCardUI != null)
			{
				ObjectPool.RecycleCard(monsterCardUI.CardID, ObjectPool.ECardType.Monster, tooltipObj);
				monsterCardUI = null;
			}
			if (itemCardUI != null)
			{
				ObjectPool.Recycle(itemCardUI.GameObject, UIInfoTools.Instance.ItemTooltip.gameObject, "Tooltips Pool");
				itemCardUI = null;
			}
			if (perkUI != null)
			{
				Object.Destroy(perkUI.gameObject);
				perkUI = null;
			}
			tooltipObj = null;
		}
	}
}

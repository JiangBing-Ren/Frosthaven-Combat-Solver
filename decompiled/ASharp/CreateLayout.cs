using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FH.GraphicalUserInterface;
using FHGUI.Common;
using GLOOM;
using SETUtil.Extend;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CreateLayout
{
	public class Anchors
	{
		public Vector2 AnchorMin { get; set; }

		public Vector2 AnchorMax { get; set; }

		public Anchors()
		{
			AnchorMin = Vector2.zero;
			AnchorMax = Vector2.one;
		}

		public Anchors(Vector2 anchorMin, Vector2 anchorMax)
		{
			AnchorMin = anchorMin;
			AnchorMax = anchorMax;
		}

		public Anchors(float minx, float miny, float maxx, float maxy)
		{
			AnchorMin = new Vector2(minx, miny);
			AnchorMax = new Vector2(maxx, maxy);
		}
	}

	public enum LayoutTypes
	{
		TopPreview,
		BottomPreview,
		TopFull,
		BottomFull
	}

	public enum GrowContentType
	{
		None,
		GrowVertical,
		GrowHorizontal,
		GrowBoth
	}

	private const string trTextBox = "PreviewText";

	private const string trTextContainer = "TextContainer";

	private const string AbilityCardRes = "AbilityCard/";

	private const string ConsumeButtonRes = "ConsumeButton";

	private const string ElementRes = "InfuseElement";

	private const string InfuseContainerRes = "InfuseContainer";

	private const string EnhancementContainerRes = "EnhancementContainer";

	private const string SummonContainerRes = "SummonContainer";

	private const string OnDeathAOEContainerRes = "OnDeathAOEContainer";

	private const string AugmentContainerRes = "AugmentContainer";

	private const string DoomContainerRes = "DoomContainer";

	private const string SongContainerRes = "SongContainer";

	private const string EnhancementCircleRes = "Enhancement_Circle";

	private const string EnhancementSquareRes = "Enhancement_Square";

	private const string EnhancementDiamondRes = "Enhancement_Diamond";

	private const string EnhancementDiamondPlusRes = "Enhancement_DiamondPlus";

	private const string EnhancedAreaHex = "EnhancedAreaHex";

	private const string IconContainerRes = "InfuseContainer";

	private const string XPContainerRes = "XPContainer";

	private const string XPRes = "XPRes";

	private const string XPArrowRes = "XPArrow";

	private const string DurationRes = "DurationRes";

	public const char LocKeySymbol = '$';

	private const string LayoutParentName = "Layout Parent";

	public const string RowContainerName = "Row Container";

	public const string ColumnContainerName = "Column Container";

	private const string TextContainerName = "Text Container";

	public const string RowContainerNameIgnoringColumnLayout = "Row Container (IgnoreMonsterColumnLayout)";

	private const string ConsumeButtonName = "Consume Button";

	private const string ConsumeButtonDescriptionName = "Description";

	private const string ElementName = "Element";

	private const string InfuseContainerName = "InfuseContainer";

	private const string XPContainerName = "XPContainer";

	private const string IconContainerName = "IconContainer";

	private const string EnhancementContainerName = "EnhancementContainer";

	private const string EnhancementName = "Enhancement";

	private const string OnDeathHeaderName = "On Death Header";

	private const string AreaEffectContainerName = "AreaEffectContainer";

	private const string AreaEffectColumnName = "AreaEffectColumn";

	private const string SummonContainerName = "SummonContainer";

	private const string DividerRes = "DividerCardElement";

	private const string MandatoryTriggerRes = "MandatoryTriggerContainer";

	private const string MandatoryTriggerContainerName = "MandatoryTrigger";

	private const string ConditionalElementRes = "ConditionalElementContainer";

	private const string ConditionalElementContainerName = "ConditionalElement";

	private const string BlinkbladeFastStateElementRes = "Blinkblade_FastStateUIElement";

	private const string BlinkbladeSlowStateElementRes = "Blinkblade_SlowStateUIElement";

	private const string BlinkbladeStateElementName = "BlinkbladeState_{0}";

	private const string AddedEffectRes = "AddedEffectContainer";

	private const string AddedEffectContainerName = "AddedEffect";

	public List<ConsumeButton> _consumeButtons = new List<ConsumeButton>();

	public List<InfuseElement> _infuseElements = new List<InfuseElement>();

	public List<EnhancementButton> _enhanceElements = new List<EnhancementButton>();

	public bool IsLongRest;

	public bool IsItemCard;

	public float _fontSize;

	private CEnemyActor EnemyForBaseStats;

	private Rect _fullArea;

	private float _hexWidth;

	private int _layoutID;

	private bool _inConsume;

	private float _rowSpacing;

	private MonsterCardYMLData _monsterCard;

	private FullAbilityCardAction.CardHalf _cardHalf;

	private List<(Graphic, string)> _graphicElements = new List<(Graphic, string)>(10);

	public GameObject FullLayout { get; private set; }

	public List<LayoutRow> LayoutRows { get; private set; }

	public CreateLayout(CardLayoutGroup parentGroup, Rect fullArea, int layoutID, bool isLongRest, bool inConsume = false, RectTransform parent = null, bool isItemCard = false, FullAbilityCardAction.CardHalf cardHalf = FullAbilityCardAction.CardHalf.NA, GrowContentType growContentType = GrowContentType.None)
	{
		_layoutID = layoutID;
		_fullArea = fullArea;
		_hexWidth = _fullArea.width * GlobalSettings.Instance.m_AbilityCardSettings.AreaHexWidth;
		_inConsume = inConsume;
		IsLongRest = isLongRest;
		IsItemCard = isItemCard;
		_fontSize = GlobalSettings.Instance.m_AbilityCardSettings.StandardFontSize;
		_cardHalf = cardHalf;
		if (isLongRest)
		{
			if (parent != null)
			{
				LongRestRows componentInChildren = parent.GetComponentInChildren<LongRestRows>();
				if (componentInChildren != null)
				{
					componentInChildren.BurnText.text = LocalizationManager.GetTranslation("LongRest_Burn");
					componentInChildren.HealText.text = LocalizationManager.GetTranslation("LongRest_Heal");
					componentInChildren.RefreshText.text = LocalizationManager.GetTranslation("LongRest_Refresh");
					componentInChildren.FooterText.text = LocalizationManager.GetTranslation("LongRest_Footer");
				}
			}
			else
			{
				Log.GUIInfo("No Long Rest parent RectTransform provided, cannot complete layout using LongRestRowContainers from prefab");
			}
		}
		else
		{
			FullLayout = CreateStretchContainer(_fullArea, LayoutTypes.TopFull);
			FullLayout.name = "Layout Parent";
			CreateFullLayoutText(FullLayout, LocaliseLayout(parentGroup), growContentType);
		}
	}

	public CreateLayout(CardLayoutGroup parentGroup, GameObject layout, int layoutID, GrowContentType growContentType)
	{
		_layoutID = layoutID;
		_fullArea = layout.GetComponent<RectTransform>().rect;
		_hexWidth = _fullArea.width * GlobalSettings.Instance.m_AbilityCardSettings.AreaHexWidth;
		_inConsume = false;
		IsLongRest = false;
		IsItemCard = false;
		_fontSize = GlobalSettings.Instance.m_AbilityCardSettings.StandardFontSize;
		_cardHalf = FullAbilityCardAction.CardHalf.NA;
		FullLayout = layout;
		CreateFullLayoutText(FullLayout, LocaliseLayout(parentGroup), growContentType);
	}

	public CreateLayout(MonsterCardYMLData monsterCard, Rect fullArea, CEnemyActor enemyForBaseStats = null)
	{
		_monsterCard = monsterCard;
		_layoutID = monsterCard.ID;
		_fullArea = fullArea;
		_hexWidth = fullArea.width * GlobalSettings.Instance.m_AbilityCardSettings.AreaHexWidth;
		_fontSize = GlobalSettings.Instance.m_AbilityCardSettings.StandardFontSize;
		_cardHalf = FullAbilityCardAction.CardHalf.NA;
		EnemyForBaseStats = enemyForBaseStats;
		FullLayout = CreateVerticalLayoutContainer(_fullArea);
		FullLayout.name = "Layout Parent";
		LayoutRows = new List<LayoutRow>();
		Transform transform = FullLayout.transform;
		List<string> list = new List<string>();
		bool flag = false;
		foreach (CActionAugmentation augmentation in monsterCard.CardAction.Augmentations)
		{
			foreach (CActionAugmentationOp augmentationOp in augmentation.AugmentationOps)
			{
				if (augmentationOp.Ability != null && augmentationOp.Ability.AreaEffectYMLString.IsNOTNullOrEmpty() && augmentationOp.Ability.ShowArea)
				{
					list.Add(augmentationOp.Ability.AreaEffectYMLString);
					flag = true;
				}
				if (augmentationOp.AbilityOverride != null && augmentationOp.AbilityOverride.AreaEffectYMLString.IsNOTNullOrEmpty() && augmentationOp.AbilityOverride.ShowArea != false)
				{
					list.Add(augmentationOp.AbilityOverride.AreaEffectYMLString);
					flag = true;
				}
			}
		}
		bool flag2 = monsterCard.CardAction.Abilities.Any((CAbility x) => x.AreaEffectYMLString.IsNOTNullOrEmpty() && x.ShowArea);
		if (flag2)
		{
			foreach (CAbility item2 in monsterCard.CardAction.Abilities.Where((CAbility x) => x != null))
			{
				if (item2.AreaEffectLayoutOverrideYMLString.IsNOTNullOrEmpty() && item2.ShowArea)
				{
					list.Add(item2.AreaEffectLayoutOverrideYMLString);
				}
				else if (item2.AreaEffectYMLString.IsNOTNullOrEmpty() && item2.ShowArea)
				{
					list.Add(item2.AreaEffectYMLString);
				}
			}
		}
		float num = 0.97f;
		float num2 = 0f;
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		if (list.Count > 0)
		{
			float num3 = 0f;
			foreach (string item3 in list)
			{
				float item = GetAreaEffectDimensionsInHexes(item3).Item1;
				if (item > num3)
				{
					num3 = item;
				}
			}
			float num4 = num3 * _hexWidth;
			num2 = num - num4 / fullArea.width;
			num2 -= 0.02f;
			UnityEngine.Object.DestroyImmediate(transform.GetComponent<VerticalLayoutGroup>());
			if (flag2)
			{
				gameObject2 = CreateContainer(new Anchors(0f, 0f, num2, 1f), LayoutTypes.TopFull, transform);
				gameObject2.name = "ColumnCombined";
				VerticalLayoutGroup verticalLayoutGroup = gameObject2.AddComponent<VerticalLayoutGroup>();
				verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
				verticalLayoutGroup.spacing = GlobalSettings.Instance.m_GlossaryCardSettings.RowSpacing;
				verticalLayoutGroup.childControlHeight = true;
				verticalLayoutGroup.childControlWidth = true;
				verticalLayoutGroup.childForceExpandHeight = false;
				verticalLayoutGroup.childForceExpandWidth = false;
				SetGrowContent(gameObject2, GrowContentType.None);
			}
			else
			{
				gameObject2 = CreateContainer(new Anchors(0f, 0f, num2, 0.5f), LayoutTypes.TopFull, transform);
				gameObject2.name = "ColumnforConsume";
				VerticalLayoutGroup verticalLayoutGroup2 = gameObject2.AddComponent<VerticalLayoutGroup>();
				verticalLayoutGroup2.childAlignment = TextAnchor.MiddleCenter;
				verticalLayoutGroup2.spacing = GlobalSettings.Instance.m_GlossaryCardSettings.RowSpacing;
				verticalLayoutGroup2.childControlHeight = true;
				verticalLayoutGroup2.childControlWidth = true;
				verticalLayoutGroup2.childForceExpandHeight = false;
				verticalLayoutGroup2.childForceExpandWidth = false;
				SetGrowContent(gameObject2, GrowContentType.None);
				gameObject = CreateContainer(new Anchors(0f, 0.5f, 1f, 1f), LayoutTypes.TopFull, transform);
				gameObject.name = "ColumnforAbility";
				VerticalLayoutGroup verticalLayoutGroup3 = gameObject.AddComponent<VerticalLayoutGroup>();
				verticalLayoutGroup3.childAlignment = TextAnchor.MiddleCenter;
				verticalLayoutGroup3.spacing = GlobalSettings.Instance.m_GlossaryCardSettings.RowSpacing;
				verticalLayoutGroup3.childControlHeight = true;
				verticalLayoutGroup3.childControlWidth = true;
				verticalLayoutGroup3.childForceExpandHeight = false;
				verticalLayoutGroup3.childForceExpandWidth = false;
				SetGrowContent(gameObject, GrowContentType.None);
			}
		}
		if (flag2)
		{
			string areaString = null;
			int num5 = 0;
			foreach (CAbility ability in monsterCard.CardAction.Abilities)
			{
				if (ability.AreaEffectLayoutOverrideYMLString.IsNOTNullOrEmpty() && ability.ShowArea)
				{
					areaString = ability.AreaEffectLayoutOverrideYMLString;
					break;
				}
				if (ability.AreaEffectYMLString.IsNOTNullOrEmpty() && ability.ShowArea)
				{
					areaString = ability.AreaEffectYMLString;
					break;
				}
				num5++;
			}
			float num6 = 0.05f;
			float num7 = 0.95f;
			int num8 = monsterCard.CardAction.Abilities.Count + monsterCard.CardAction.Augmentations.Count + ((monsterCard.CardAction.Infusions.Count > 0) ? 1 : 0);
			if (!flag && num8 > 1)
			{
				float num9 = (num7 - num6) / (float)num8;
				num7 -= num9 * (float)num5;
				num6 = num7 - num9;
			}
			GameObject gameObject3 = CreateContainer(new Anchors(num2, num6, num, num7), LayoutTypes.TopFull, transform);
			gameObject3.name = "AreaEffectColumn";
			SetGrowContent(gameObject3, GrowContentType.None);
			ProcessAreaEffect(areaString, gameObject3.transform as RectTransform, _hexWidth);
			transform = gameObject2.transform;
		}
		GenerateMonsterCardAbilityLayout(monsterCard.CardAction.Abilities, monsterCard.CardAction.Augmentations, (gameObject != null) ? gameObject.transform : transform, LayoutRows, bold: true, inConsume: false, gameObject2, num2);
		if (monsterCard.CardAction.Infusions != null && monsterCard.CardAction.Infusions.Count > 0)
		{
			CreateGap(transform, LayoutRows);
			CreateInfuse(monsterCard.CardAction.Infusions, transform);
		}
		foreach (CAbility item4 in monsterCard.CardAction.Abilities.Where((CAbility x) => x.OnDeath).ToList())
		{
			CreateGap(transform, LayoutRows);
			GameObject go = PrefabLoaderCache.MiscGui.Instantiate("OnDeathAOEContainer", transform);
			go.FindInChildren("On Death Header").GetComponent<TextMeshProUGUI>().text = LocaliseText("$OnDeath$");
			string glossaryName = item4.AbilityType.ToString();
			int? value = item4.Strength;
			float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize;
			CAbility.EAbilityType abilityType = item4.AbilityType;
			string name = item4.Name;
			LayoutRow layoutRow = LayoutRow.CreateLayoutRow(glossaryName, value, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
			layoutRow.transform.SetParent(go.FindInImmediateChildren("Text Container").transform);
			layoutRow.UpdateText(bold: true);
			layoutRow.transform.SetSiblingIndex(1);
		}
	}

	private void GenerateMonsterCardAbilityLayout(List<CAbility> abilities, List<CActionAugmentation> consumes, Transform parent, List<LayoutRow> layoutRows, bool bold = true, bool inConsume = false, GameObject consumeContentContainer = null, float columnSplitX = 0.5f)
	{
		float value = (inConsume ? GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize : GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize);
		float value2 = (inConsume ? GlobalSettings.Instance.m_GlossaryCardSettings.SmallerFontSize : GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize);
		bool flag = false;
		List<CActionAugmentation> list = consumes?.ToList();
		List<CAbility> list2 = new List<CAbility>();
		foreach (CAbility ability2 in abilities)
		{
			if (!ability2.OnDeath)
			{
				if (ability2 is CAbilityControlActor cAbilityControlActor)
				{
					list2.AddRange(cAbilityControlActor.ControlActorData.ControlAbilities);
				}
				else
				{
					list2.Add(ability2);
				}
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			CAbility ability = list2[i];
			string text = ability.AbilityText;
			if (ability.SkipGUI)
			{
				continue;
			}
			if (list != null)
			{
				CActionAugmentation cActionAugmentation = list.SingleOrDefault((CActionAugmentation x) => x.LayoutProperties.LayoutPriorTo == ability.Name);
				if (cActionAugmentation != null)
				{
					if (cActionAugmentation.LayoutProperties.GapAbove > 0)
					{
						CreateGap(parent, LayoutRows, small: false, cActionAugmentation.LayoutProperties.GapAbove);
					}
					AbilityConsume abilityConsume = new AbilityConsume(cActionAugmentation);
					if (abilityConsume.Text == null)
					{
						abilityConsume.Text = new CardLayout("", abilities, "MonsterCard_" + _monsterCard.ID, DiscardType.None, _monsterCard.FileName);
					}
					CreateConsume(abilityConsume, (consumeContentContainer == null) ? parent : consumeContentContainer.transform, isMonsterCard: true, columnSplitX);
					list.Remove(cActionAugmentation);
					if (cActionAugmentation.LayoutProperties.GapBelow > 0)
					{
						CreateGap(parent, LayoutRows, small: false, cActionAugmentation.LayoutProperties.GapBelow);
					}
				}
			}
			AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
			if (miscAbilityData != null && miscAbilityData.GapAbove.HasValue)
			{
				CreateGap(parent, LayoutRows, small: false, ability.MiscAbilityData.GapAbove.Value);
			}
			if (i > 0 && !flag && (!ability.AbilityTextOnly || ability.AbilityText != null))
			{
				flag = false;
				if (!ability.AbilityNoLayoutSpace)
				{
					CreateGap(parent, LayoutRows);
				}
			}
			int? num = null;
			if (CAbility.AbilityDisplayValues.ContainsKey(ability.AbilityType))
			{
				switch (CAbility.AbilityDisplayValues[ability.AbilityType])
				{
				case CAbility.EAbilityStatType.Strength:
					num = ability.Strength;
					break;
				case CAbility.EAbilityStatType.Range:
					num = ability.Range;
					break;
				}
			}
			if (ability.StatIsBasedOnXEntries != null && ability.StatIsBasedOnXEntries.Count > 0 && EnemyForBaseStats != null)
			{
				AbilityData.StatIsBasedOnXData statIsBasedOnXData = ability.StatIsBasedOnXEntries.SingleOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
				if (statIsBasedOnXData != null)
				{
					num = ability.GetStatIsBasedOnXValue(EnemyForBaseStats, statIsBasedOnXData, ability.AbilityFilter);
				}
				text = LocaliseText(text);
				if (text.Contains("{0}"))
				{
					text = string.Format(text, num);
				}
			}
			if (ability is CAbilityAttack { ChainAttack: not false } cAbilityAttack)
			{
				if (EnemyForBaseStats != null)
				{
					num = EnemyForBaseStats.MonsterClass.CurrentMonsterStat.Attack;
				}
				text = LocaliseText(text);
				int num2 = num.GetValueOrDefault();
				List<string> list3 = new List<string>();
				for (int num3 = 0; num3 < cAbilityAttack.NumberTargets; num3++)
				{
					list3.Add(num2.ToString());
					num2 = Math.Max(0, num2 - cAbilityAttack.ChainAttackDamageReduction);
				}
				string format = text;
				object[] args = list3.ToArray();
				text = string.Format(format, args);
			}
			if (ability.AbilityTextOnly)
			{
				if (text != string.Empty)
				{
					LayoutRow layoutRow = LayoutRow.CreateLayoutRow(text, num, GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize);
					layoutRow.transform.SetParent(parent);
					layoutRows.Add(layoutRow);
				}
			}
			else
			{
				if (ability.AbilityType == CAbility.EAbilityType.None || ability.AbilityType == CAbility.EAbilityType.Null)
				{
					continue;
				}
				if (ability.AbilityType == CAbility.EAbilityType.Summon)
				{
					CAbilitySummon cAbilitySummon = ability as CAbilitySummon;
					string locKey;
					if (cAbilitySummon.SelectedSummonYMLData != null)
					{
						locKey = cAbilitySummon.SelectedSummonYMLData.LocKey;
					}
					else
					{
						if (cAbilitySummon.SelectedMonsterYMLData == null)
						{
							flag = true;
							continue;
						}
						locKey = cAbilitySummon.SelectedMonsterYMLData.LocKey;
					}
					int num4 = cAbilitySummon.NumberTargets;
					if (cAbilitySummon.StatIsBasedOnXEntries != null && EnemyForBaseStats != null)
					{
						AbilityData.StatIsBasedOnXData statIsBasedOnXData2 = cAbilitySummon.StatIsBasedOnXEntries.SingleOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets);
						if (statIsBasedOnXData2 != null)
						{
							num4 = cAbilitySummon.GetStatIsBasedOnXValue(EnemyForBaseStats, statIsBasedOnXData2, cAbilitySummon.AbilityFilter);
						}
					}
					LayoutRow layoutRow2 = LayoutRow.CreateLayoutRow("$Summon$ " + ((num4 > 1) ? (num4 + " ") : "") + "$" + locKey + "$", value);
					AbilityData.MiscAbilityData miscAbilityData2 = cAbilitySummon.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.IgnoreMonsterColumnLayout == true)
					{
						GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("TextContainer");
						gameObject.transform.SetParent(parent.parent);
						RectTransform component = gameObject.GetComponent<RectTransform>();
						component.anchorMin = new Vector2(0f, 0.5f);
						component.anchorMax = new Vector2(1f, 0.5f);
						VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
						verticalLayoutGroup.childControlHeight = true;
						verticalLayoutGroup.childControlWidth = true;
						layoutRow2.transform.SetParent(gameObject.transform);
						layoutRow2.gameObject.name = "Row Container (IgnoreMonsterColumnLayout)";
						if (cAbilitySummon.MiscAbilityData.IgnoreMCLAdjustY.HasValue && cAbilitySummon.MiscAbilityData.IgnoreMCLAdjustY.Value != 0)
						{
							layoutRow2.TextBox.margin = new Vector4(0f, 0f, 0f, cAbilitySummon.MiscAbilityData.IgnoreMCLAdjustY.Value);
						}
						layoutRow2.TextBox.enableAutoSizing = false;
						Rect rect = (layoutRow2.TextBox.transform as RectTransform).rect;
						rect.width = (gameObject.transform as RectTransform).rect.width;
						CreateGap(parent, LayoutRows, small: false, 200);
					}
					else
					{
						layoutRow2.transform.SetParent(parent);
					}
					layoutRow2.UpdateText(bold);
					layoutRows.Add(layoutRow2);
					continue;
				}
				string glossaryName = ability.AbilityType.ToString();
				int? value3 = num;
				float? fontSize = value;
				CAbility.EAbilityType abilityType = ability.AbilityType;
				string name = ability.Name;
				LayoutRow layoutRow3 = LayoutRow.CreateLayoutRow(glossaryName, value3, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
				layoutRow3.transform.SetParent(parent);
				bool bold2 = bold;
				AbilityData.MiscAbilityData miscAbilityData3 = ability.MiscAbilityData;
				layoutRow3.UpdateText(bold2, miscAbilityData3 != null && miscAbilityData3.ShowPlusX == true);
				layoutRow3.FromControlAbility = ability.IsControlAbility;
				layoutRows.Add(layoutRow3);
				if (ability.ShowRange)
				{
					if (ability.AbilityType == CAbility.EAbilityType.Retaliate && (ability as CAbilityRetaliate).RetaliateRange > 1)
					{
						int retaliateRange = (ability as CAbilityRetaliate).RetaliateRange;
						int? value4 = retaliateRange;
						fontSize = value2;
						abilityType = ability.AbilityType;
						name = ability.Name;
						LayoutRow layoutRow4 = LayoutRow.CreateLayoutRow("RetaliateRange", value4, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
						layoutRow4.transform.SetParent(parent);
						layoutRow4.Entry.ShowSymbolNullable = false;
						layoutRow4.UpdateText();
						layoutRows.Add(layoutRow4);
					}
					else if (ability.AbilityType != CAbility.EAbilityType.Loot && ability.Range != 0)
					{
						string glossaryName2 = CAbility.EAbilityStatType.Range.ToString();
						int? value5 = ability.Range;
						fontSize = value2;
						abilityType = ability.AbilityType;
						name = ability.Name;
						LayoutRow layoutRow5 = LayoutRow.CreateLayoutRow(glossaryName2, value5, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
						if (ability.RangeIsBase)
						{
							layoutRow5.Entry.ShowSymbolNullable = false;
							layoutRow5.UpdateText();
						}
						layoutRow5.transform.SetParent(parent);
						layoutRows.Add(layoutRow5);
					}
					else if (EnemyForBaseStats != null && EnemyForBaseStats.MonsterClass.Range > 1 && ability.AbilityType == CAbility.EAbilityType.Attack && (ability.AreaEffectYMLString.IsNullOrEmpty() || !ability.AreaEffectYMLString.Contains("G")))
					{
						string glossaryName3 = CAbility.EAbilityStatType.Range.ToString();
						int? value6 = EnemyForBaseStats.MonsterClass.Range;
						fontSize = value2;
						abilityType = ability.AbilityType;
						name = ability.Name;
						LayoutRow layoutRow6 = LayoutRow.CreateLayoutRow(glossaryName3, value6, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name, showValueMultiplier: false, fromBaseStats: true);
						layoutRow6.transform.SetParent(parent);
						layoutRow6.Entry.ShowSymbolNullable = false;
						layoutRow6.UpdateText();
						layoutRows.Add(layoutRow6);
					}
				}
				if (ability.ShowTarget)
				{
					AbilityData.StatIsBasedOnXData statIsBasedOnXData3 = null;
					if (ability.StatIsBasedOnXEntries != null)
					{
						statIsBasedOnXData3 = ability.StatIsBasedOnXEntries.Find((AbilityData.StatIsBasedOnXData stx) => stx.BasedOn == CAbility.EStatIsBasedOnXType.BaseStatPlusX && stx.BaseStatType == EMonsterBaseStats.Target);
					}
					if (ability.TargetsSet && ability.NumberTargets > 0 && (!ability.TargetIsBase || ability.NumberTargets != -1))
					{
						string glossaryName4 = CAbility.EAbilityStatType.NumberOfTargets.ToString();
						int? value7 = ability.NumberTargets;
						fontSize = value2;
						abilityType = ability.AbilityType;
						name = ability.Name;
						LayoutRow layoutRow7 = LayoutRow.CreateLayoutRow(glossaryName4, value7, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
						if (ability.TargetIsBase)
						{
							layoutRow7.Entry.ShowSymbolNullable = false;
							layoutRow7.UpdateText();
						}
						layoutRow7.transform.SetParent(parent);
						layoutRows.Add(layoutRow7);
					}
					else if (!ability.TargetsSet && EnemyForBaseStats != null && EnemyForBaseStats.MonsterClass.Target > 1 && !ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
					{
						AbilityData.MiscAbilityData miscAbilityData4 = ability.MiscAbilityData;
						if (miscAbilityData4 != null && miscAbilityData4.TargetOneEnemyWithAllAttacks == false && (ability.AbilityType == CAbility.EAbilityType.Attack || ability.AbilityType == CAbility.EAbilityType.Heal))
						{
							int value8 = EnemyForBaseStats.MonsterClass.Target;
							if (statIsBasedOnXData3 != null)
							{
								value8 = ability.GetStatIsBasedOnXValue(EnemyForBaseStats, statIsBasedOnXData3, CAbilityFilterContainer.CreateDefaultFilter());
							}
							string glossaryName5 = CAbility.EAbilityStatType.NumberOfTargets.ToString();
							int? value9 = value8;
							fontSize = value2;
							abilityType = ability.AbilityType;
							name = ability.Name;
							LayoutRow layoutRow8 = LayoutRow.CreateLayoutRow(glossaryName5, value9, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name, showValueMultiplier: false, fromBaseStats: true);
							layoutRow8.transform.SetParent(parent);
							layoutRow8.Entry.ShowSymbolNullable = false;
							layoutRow8.UpdateText();
							layoutRows.Add(layoutRow8);
						}
					}
				}
				foreach (CAbility item in ability.SubAbilities.Where((CAbility x) => x.AbilityType == CAbility.EAbilityType.Pull || x.AbilityType == CAbility.EAbilityType.Push || x.AbilityType == CAbility.EAbilityType.Teleport2))
				{
					string glossaryName6 = item.AbilityType.ToString();
					int? value10 = item.Strength;
					fontSize = value2;
					abilityType = ability.AbilityType;
					name = ability.Name;
					LayoutRow layoutRow9 = LayoutRow.CreateLayoutRow(glossaryName6, value10, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
					layoutRow9.transform.SetParent(parent);
					layoutRows.Add(layoutRow9);
				}
				if (ability.AbilityType == CAbility.EAbilityType.Attack && (ability as CAbilityAttack).Pierce > 0)
				{
					int? value11 = (ability as CAbilityAttack).Pierce;
					fontSize = value2;
					abilityType = ability.AbilityType;
					name = ability.Name;
					LayoutRow layoutRow10 = LayoutRow.CreateLayoutRow("Pierce", value11, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
					layoutRow10.transform.SetParent(parent);
					layoutRows.Add(layoutRow10);
				}
				if (ability.AbilityType == CAbility.EAbilityType.Move && (ability as CAbilityMove).Jump)
				{
					string glossaryName7 = EEnhancement.Jump.ToString();
					fontSize = value2;
					abilityType = ability.AbilityType;
					name = ability.Name;
					LayoutRow layoutRow11 = LayoutRow.CreateLayoutRow(glossaryName7, null, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
					layoutRow11.transform.SetParent(parent);
					layoutRows.Add(layoutRow11);
				}
				Dictionary<string, int> conditions = ability.NegativeConditions.ToDictionary((KeyValuePair<CCondition.ENegativeCondition, CAbility> x) => x.Key.ToString(), (KeyValuePair<CCondition.ENegativeCondition, CAbility> x) => x.Value.Strength);
				ability.PositiveConditions.ToDictionary((KeyValuePair<CCondition.EPositiveCondition, CAbility> x) => x.Key.ToString(), (KeyValuePair<CCondition.EPositiveCondition, CAbility> x) => x.Value.Strength).ToList().ForEach((KeyValuePair<string, int> x) =>
				{
					conditions.Add(x.Key, x.Value);
				});
				if (CCondition.NegativeConditions.Select((CCondition.ENegativeCondition x) => x.ToString()).Concat(CCondition.PositiveConditions.Select((CCondition.EPositiveCondition x) => x.ToString())).ToList()
					.Any((string x) => x == ability.AbilityType.ToString()))
				{
					layoutRow3.RefreshText(conditions.Keys.ToList());
				}
				else
				{
					if (EnemyForBaseStats != null && ability.AbilityType == CAbility.EAbilityType.Attack)
					{
						foreach (CCondition.ENegativeCondition condition in EnemyForBaseStats.MonsterClass.Conditions)
						{
							string text2 = condition.ToString();
							if (text2 == CCondition.ENegativeCondition.Curse.ToString() && conditions.ContainsKey(text2))
							{
								if (conditions[text2] == 0)
								{
									conditions[text2]++;
								}
								conditions[text2]++;
							}
							else if (!conditions.ContainsKey(text2))
							{
								conditions.Add(text2, 0);
							}
						}
					}
					foreach (KeyValuePair<string, int> item2 in conditions)
					{
						LayoutRow layoutRow12;
						if (item2.Value > 1)
						{
							string key = item2.Key;
							int? value12 = item2.Value;
							fontSize = value2;
							abilityType = ability.AbilityType;
							name = ability.Name;
							layoutRow12 = LayoutRow.CreateLayoutRow(key, value12, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name, showValueMultiplier: true);
						}
						else
						{
							string key2 = item2.Key;
							fontSize = value2;
							abilityType = ability.AbilityType;
							name = ability.Name;
							layoutRow12 = LayoutRow.CreateLayoutRow(key2, null, null, null, fontSize, monsterCard: true, isBuff: false, abilityType, name);
						}
						layoutRow12.transform.SetParent(parent);
						layoutRows.Add(layoutRow12);
					}
				}
				if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
				{
					IconGlossaryYML.IconGlossaryEntry iconGlossaryEntry = ScenarioRuleClient.SRLYML.IconGlossary.SingleOrDefault((IconGlossaryYML.IconGlossaryEntry x) => x.Name == ability.AbilityType.ToString());
					if (iconGlossaryEntry != null && iconGlossaryEntry.ShowSelf)
					{
						string glossaryName8 = CAbilityFilter.EFilterTargetType.Self.ToString();
						fontSize = value2;
						abilityType = ability.AbilityType;
						LayoutRow layoutRow13 = LayoutRow.CreateLayoutRow(glossaryName8, null, null, null, fontSize, monsterCard: true, isBuff: false, abilityType);
						layoutRow13.transform.SetParent(parent);
						layoutRows.Add(layoutRow13);
					}
				}
				if (text != string.Empty)
				{
					LayoutRow layoutRow14 = LayoutRow.CreateLayoutRow(text, num, value2);
					layoutRow14.transform.SetParent(parent);
					layoutRows.Add(layoutRow14);
				}
				AbilityData.MiscAbilityData miscAbilityData5 = ability.MiscAbilityData;
				if (miscAbilityData5 != null && miscAbilityData5.GapBelow.HasValue)
				{
					CreateGap(parent, LayoutRows, small: false, ability.MiscAbilityData.GapBelow.Value);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (CActionAugmentation item3 in list)
		{
			if (!item3.LayoutProperties.Hidden)
			{
				if (item3.LayoutProperties.GapAbove > 0)
				{
					CreateGap(parent, LayoutRows, small: false, item3.LayoutProperties.GapAbove);
				}
				AbilityConsume abilityConsume2 = new AbilityConsume(item3);
				if (abilityConsume2.Text == null)
				{
					abilityConsume2.Text = new CardLayout("", abilities, "MonsterCard_" + _monsterCard.ID, DiscardType.None, _monsterCard.FileName);
				}
				CreateConsume(abilityConsume2, (consumeContentContainer == null) ? parent : consumeContentContainer.transform, isMonsterCard: true, columnSplitX);
				if (item3.LayoutProperties.GapBelow > 0)
				{
					CreateGap(parent, LayoutRows, small: false, item3.LayoutProperties.GapBelow);
				}
			}
		}
	}

	private static void CreateGap(Transform parent, List<LayoutRow> layoutRows, bool small = false, int sizeOverride = -1)
	{
		string text;
		if (sizeOverride >= 0)
		{
			text = "<size=" + sizeOverride + "%> ";
		}
		else
		{
			text = (small ? "<size=20%> " : "<size=50%> ");
		}
		LayoutRow layoutRow = LayoutRow.CreateLayoutRow(text, GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize);
		layoutRow.transform.SetParent(parent);
		layoutRows.Add(layoutRow);
	}

	private void CreateFullLayoutText(GameObject container, CardLayoutGroup group, GrowContentType growType = GrowContentType.None)
	{
		switch (group.DataType)
		{
		case CardLayoutGroup.DataTypes.ColumnCollection:
			ProcessColumnCollection(container, group, growType);
			break;
		case CardLayoutGroup.DataTypes.RowCollection:
			ProcessRowCollection(container, group, growType);
			break;
		case CardLayoutGroup.DataTypes.RowData:
			CreateRowText(group.Data, container.transform, group.RowAtt);
			break;
		case CardLayoutGroup.DataTypes.Consume:
			CreateConsume(group.Consume, container.transform);
			break;
		case CardLayoutGroup.DataTypes.Element:
			CreateInfuse(group.Elements, container.transform);
			break;
		case CardLayoutGroup.DataTypes.Summon:
			CreateSummon(group.Summon, container.transform, IsItemCard, _layoutID, _enhanceElements);
			break;
		case CardLayoutGroup.DataTypes.Augment:
			CreateAugment(group.Augment, container.transform);
			break;
		case CardLayoutGroup.DataTypes.Doom:
			CreateDoom(group.Doom, container.transform);
			break;
		case CardLayoutGroup.DataTypes.Song:
			CreateSong(group.Song, container.transform);
			break;
		case CardLayoutGroup.DataTypes.Duration:
			CreateDurationIcon(group.Duration, container.transform);
			break;
		case CardLayoutGroup.DataTypes.XP:
		{
			bool showPersistentIcon = group.Duration == CActiveBonus.EActiveBonusDurationType.Persistent;
			CreateXPIcons(group.XP, showPersistentIcon, container.transform);
			break;
		}
		case CardLayoutGroup.DataTypes.Divider:
			CreateDivider(container.transform, ResolveColor(group));
			break;
		case CardLayoutGroup.DataTypes.MandatoryTrigger:
			CreateMandatoryTrigger(group.MandatoryTrigger, container.transform, ResolveColor(group));
			break;
		case CardLayoutGroup.DataTypes.Conditional:
			CreateConditionalData(group.ConditionalElement, container.transform, ResolveColor(group));
			break;
		case CardLayoutGroup.DataTypes.BlinkbladeState:
			CreateBlinkbladeState(group.BlinkbladeState, container.transform, ResolveColor(group));
			break;
		case CardLayoutGroup.DataTypes.AddedEffect:
			CreateAddedEffect(group.AddedEffect, container.transform, ResolveColor(group));
			break;
		default:
			Log.GUIError("Unexpected DataType: " + group.DataType);
			break;
		}
	}

	private void CreateConsume(AbilityConsume consume, Transform parent, bool isMonsterCard = false, float columnSplitX = 0.5f)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("ConsumeButton", parent);
		if (!(gameObject != null))
		{
			return;
		}
		gameObject.name = "Consume Button";
		ConsumeButton component = gameObject.GetComponent<ConsumeButton>();
		component.Init(consume, _layoutID, (parent.transform as RectTransform).rect.width);
		RectTransform rectTransform = gameObject.transform as RectTransform;
		rectTransform.anchoredPosition = Vector2.zero;
		_consumeButtons.Add(component);
		float elementIconSize = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize;
		float num = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize * (float)consume.ConsumeData.Elements.Count() + ConsumeButton.colonWidth + ConsumeButton.colonToDescSpacer;
		if (!isMonsterCard)
		{
			component.Description.GetComponent<VerticalLayoutGroup>().enabled = false;
			return;
		}
		component.GenerateLayout(monsterCard: true);
		RectTransform rectTransform2 = component.Description.transform as RectTransform;
		Rect rect = rectTransform2.rect;
		List<ElementInfusionBoardManager.EElement> infusions = consume.ConsumeData.Infusions;
		if (infusions != null && infusions.Count > 0)
		{
			rect.width = elementIconSize * (float)consume.ConsumeData.Infusions.Count;
			rect.height = elementIconSize;
			CreateInfuse(consume.ConsumeData.Infusions, component.Description.transform);
		}
		else
		{
			List<LayoutRow> list = new List<LayoutRow>();
			bool flag = true;
			foreach (CActionAugmentationOp augmentationOp in consume.ConsumeData.AugmentationOps)
			{
				if (!flag)
				{
					CreateGap(component.Description.transform, list, small: true);
				}
				if (augmentationOp.Ability == null)
				{
					bool flag2 = false;
					if (augmentationOp.AbilityOverride.AbilityTextOnly != true)
					{
						if (augmentationOp.AbilityOverride.Strength.HasValue)
						{
							flag2 = true;
							string glossaryName = augmentationOp.ParentAbilityType.ToString();
							int? value = augmentationOp.AbilityOverride.Strength.Value;
							float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
							CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
							string parentAbilityName = augmentationOp.ParentAbilityName;
							LayoutRow layoutRow = LayoutRow.CreateLayoutRow(glossaryName, value, null, null, fontSize, monsterCard: true, isBuff: true, parentAbilityType, parentAbilityName);
							layoutRow.transform.SetParent(component.Description.transform);
							list.Add(layoutRow);
							if (augmentationOp.AbilityOverride.Range.HasValue || augmentationOp.AbilityOverride.NumberOfTargets.HasValue)
							{
								CreateGap(component.Description.transform, list, small: true);
							}
						}
						if (augmentationOp.AbilityOverride.Range.HasValue)
						{
							flag2 = true;
							bool flag3 = augmentationOp.AbilityOverride.RangeIsBase != true;
							int? value2 = augmentationOp.AbilityOverride.Range.Value;
							float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
							bool isBuff = flag3;
							CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
							string parentAbilityName = augmentationOp.ParentAbilityName;
							LayoutRow layoutRow = LayoutRow.CreateLayoutRow("Range", value2, null, null, fontSize, monsterCard: true, isBuff, parentAbilityType, parentAbilityName);
							if (!flag3)
							{
								layoutRow.Entry.ShowSymbolNullable = false;
								layoutRow.UpdateText();
							}
							layoutRow.transform.SetParent(component.Description.transform);
							list.Add(layoutRow);
							if (augmentationOp.AbilityOverride.NumberOfTargets.HasValue)
							{
								CreateGap(component.Description.transform, list, small: true);
							}
						}
						if (augmentationOp.AbilityOverride.NumberOfTargets.HasValue)
						{
							flag2 = true;
							bool flag4 = augmentationOp.AbilityOverride.TargetIsBase != true;
							int? value3 = augmentationOp.AbilityOverride.NumberOfTargets.Value;
							float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
							bool isBuff = flag4;
							CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
							string parentAbilityName = augmentationOp.ParentAbilityName;
							LayoutRow layoutRow = LayoutRow.CreateLayoutRow("Target", value3, null, null, fontSize, monsterCard: true, isBuff, parentAbilityType, parentAbilityName);
							if (!flag4)
							{
								layoutRow.Entry.ShowSymbolNullable = false;
								layoutRow.UpdateText();
							}
							layoutRow.transform.SetParent(component.Description.transform);
							list.Add(layoutRow);
						}
						if (augmentationOp.AbilityOverride.NegativeConditions != null)
						{
							flag2 = true;
							for (int i = 0; i < augmentationOp.AbilityOverride.NegativeConditions.Count; i++)
							{
								string glossaryName2 = augmentationOp.AbilityOverride.NegativeConditions[i].ToString();
								float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
								CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
								string parentAbilityName = augmentationOp.ParentAbilityName;
								LayoutRow layoutRow = LayoutRow.CreateLayoutRow(glossaryName2, null, null, null, fontSize, monsterCard: true, isBuff: false, parentAbilityType, parentAbilityName);
								layoutRow.transform.SetParent(component.Description.transform);
								list.Add(layoutRow);
								if (i != augmentationOp.AbilityOverride.NegativeConditions.Count - 1)
								{
									CreateGap(component.Description.transform, list, small: true);
								}
							}
						}
						if (augmentationOp.AbilityOverride.PositiveConditions != null)
						{
							flag2 = true;
							for (int j = 0; j < augmentationOp.AbilityOverride.PositiveConditions.Count; j++)
							{
								string glossaryName3 = augmentationOp.AbilityOverride.PositiveConditions[j].ToString();
								float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
								CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
								string parentAbilityName = augmentationOp.ParentAbilityName;
								LayoutRow layoutRow = LayoutRow.CreateLayoutRow(glossaryName3, null, null, null, fontSize, monsterCard: true, isBuff: false, parentAbilityType, parentAbilityName);
								layoutRow.transform.SetParent(component.Description.transform);
								list.Add(layoutRow);
								if (j != augmentationOp.AbilityOverride.PositiveConditions.Count - 1)
								{
									CreateGap(component.Description.transform, list, small: true);
								}
							}
						}
						if (augmentationOp.AbilityOverride.SubAbilities != null)
						{
							flag2 = true;
							foreach (CAbility item in augmentationOp.AbilityOverride.SubAbilities.Where((CAbility x) => x.AbilityType == CAbility.EAbilityType.Pull || x.AbilityType == CAbility.EAbilityType.Push || x.AbilityType == CAbility.EAbilityType.Teleport2))
							{
								string glossaryName4 = item.AbilityType.ToString();
								int? value4 = item.Strength;
								float? fontSize = GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize;
								CAbility.EAbilityType parentAbilityType = augmentationOp.ParentAbilityType;
								string parentAbilityName = augmentationOp.ParentAbilityName;
								LayoutRow layoutRow = LayoutRow.CreateLayoutRow(glossaryName4, value4, null, null, fontSize, monsterCard: true, isBuff: false, parentAbilityType, parentAbilityName);
								layoutRow.transform.SetParent(component.Description.transform);
								list.Add(layoutRow);
							}
						}
					}
					if (augmentationOp.AbilityOverride.AbilityText != null)
					{
						flag2 = true;
						LayoutRow layoutRow = LayoutRow.CreateLayoutRow(augmentationOp.AbilityOverride.AbilityText, GlobalSettings.Instance.m_GlossaryCardSettings.SmallFontSize);
						layoutRow.transform.SetParent(component.Description.transform);
						list.Add(layoutRow);
					}
					if (augmentationOp.AbilityOverride.AreaEffectYMLString.IsNOTNullOrEmpty())
					{
						string areaString = (augmentationOp.AbilityOverride.AreaEffectLayoutOverrideYMLString.IsNOTNullOrEmpty() ? augmentationOp.AbilityOverride.AreaEffectLayoutOverrideYMLString : augmentationOp.AbilityOverride.AreaEffectYMLString);
						RectTransform rectTransform3 = FullLayout.transform.Find("AreaEffectColumn") as RectTransform;
						if (rectTransform3 != null)
						{
							rectTransform3.anchorMin = new Vector2(rectTransform3.anchorMin.x, 0.52f);
						}
						GameObject gameObject2 = CreateContainer(new Anchors(flag2 ? columnSplitX : 0.3f, 0f, 0.95f, 0.48f), LayoutTypes.TopFull, FullLayout.transform);
						gameObject2.name = "Column Container";
						SetGrowContent(gameObject2, GrowContentType.None);
						ProcessAreaEffect(areaString, gameObject2.transform as RectTransform, _hexWidth);
					}
				}
				else
				{
					bool bold = ((augmentationOp.ParentAbilityType == CAbility.EAbilityType.None) ? augmentationOp.Ability.AbilityType : augmentationOp.ParentAbilityType) != CAbility.EAbilityType.Attack || augmentationOp.Ability.AbilityType < CAbility.EAbilityType.Invisible || augmentationOp.Ability.AbilityType > CAbility.EAbilityType.Bless;
					GenerateMonsterCardAbilityLayout(new List<CAbility> { augmentationOp.Ability }, null, component.Description.transform, list, bold, inConsume: true);
					if (augmentationOp.Ability.AreaEffectYMLString.IsNOTNullOrEmpty())
					{
						string areaString2 = (augmentationOp.Ability.AreaEffectLayoutOverrideYMLString.IsNOTNullOrEmpty() ? augmentationOp.Ability.AreaEffectLayoutOverrideYMLString : augmentationOp.Ability.AreaEffectYMLString);
						RectTransform rectTransform4 = FullLayout.transform.Find("AreaEffectColumn") as RectTransform;
						if (rectTransform4 != null)
						{
							rectTransform4.anchorMin = new Vector2(rectTransform4.anchorMin.x, 0.52f);
						}
						GameObject gameObject3 = CreateContainer(new Anchors(columnSplitX, 0f, 0.95f, 0.48f), LayoutTypes.TopFull, FullLayout.transform);
						gameObject3.name = "Column Container";
						SetGrowContent(gameObject3, GrowContentType.None);
						ProcessAreaEffect(areaString2, gameObject3.transform as RectTransform, _hexWidth);
					}
				}
				flag = false;
			}
			float num2 = 0f;
			float num3 = 0f;
			foreach (LayoutRow item2 in list)
			{
				item2.TextBox.text = TextWrappingBalancer.BalanceText(item2.TextBox, rectTransform.rect.width - num, rectTransform.rect.height);
				item2.TextBox.ForceMeshUpdate();
				num2 = Math.Max(num2, item2.TextBox.preferredWidth);
				num3 += item2.TextBox.preferredHeight;
			}
			rect.size = new Vector2(num2, num3);
		}
		rect.width = Math.Min(rect.width, rectTransform.rect.width - num);
		rectTransform2.sizeDelta = new Vector2(rect.width, rect.height);
		LayoutElement component2 = gameObject.GetComponent<LayoutElement>();
		component2.preferredWidth = num + rect.width;
		component2.preferredHeight = ((GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize > rect.height) ? GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize : rect.height);
	}

	private void CreateInfuse(List<ElementInfusionBoardManager.EElement> elements, Transform parent)
	{
		RectTransform rectTransform = PrefabLoaderCache.MiscGui.Instantiate("InfuseContainer", parent).transform as RectTransform;
		rectTransform.gameObject.name = "InfuseContainer";
		rectTransform.sizeDelta = new Vector2((float)elements.Count * GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize);
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("InfuseElement", rectTransform);
			if (gameObject != null)
			{
				gameObject.name = "Element";
				InfuseElement component = gameObject.GetComponent<InfuseElement>();
				component.Init(element);
				_infuseElements.Add(component);
			}
		}
	}

	private void ProcessColumnCollection(GameObject container, CardLayoutGroup group, GrowContentType growContentType = GrowContentType.None)
	{
		List<GameObject> list = new List<GameObject>();
		float num = 0f;
		float num2 = group.ColAtt.ColumnSpacing;
		num2 /= 1000f;
		num2 += GlobalSettings.Instance.m_AbilityCardSettings.ColumnSpacing;
		float num3 = (float)(group.Collection.Count() - 1) * num2;
		for (int i = 0; i < group.Collection.Count; i++)
		{
			float num4 = 0f;
			num4 = ((group.ColAtt.ColumnWidths.Length == 0) ? (num + (1f - num3) / (float)group.Collection.Count()) : (num + (float)group.ColAtt.ColumnWidths[i] / 100f * (1f - num3)));
			GameObject gameObject = CreateContainer(new Anchors(num, 0f, num4, 1f), LayoutTypes.TopFull, container.transform);
			SetGrowContent(gameObject, growContentType);
			gameObject.name = "Column Container";
			CreateFullLayoutText(gameObject, group.Collection[i]);
			num = num4 + num2;
			list.Add(gameObject);
		}
	}

	private void ProcessRowCollection(GameObject container, CardLayoutGroup group, GrowContentType growContentType = GrowContentType.None)
	{
		List<GameObject> list = new List<GameObject>();
		float num = 0f;
		float num2 = 1f / (float)group.Collection.Count();
		float num3 = 0f;
		foreach (CardLayoutGroup item in group.Collection)
		{
			float num4 = num + num2 * 1f;
			GameObject gameObject = CreateContainer(new Anchors(0f, num, 1f, num4), LayoutTypes.TopFull, container.transform);
			SetGrowContent(gameObject, growContentType);
			gameObject.name = "Row Container";
			CreateFullLayoutText(gameObject, item);
			num = num4;
			num3 += (gameObject.transform as RectTransform).rect.height;
			list.Add(gameObject);
		}
	}

	private void CreateRowText(CardLayoutRow abilityRow, Transform parent, CardLayoutGroup.RowAttributes rowAtt)
	{
		abilityRow.SetEnhancements();
		if (abilityRow.IsArea)
		{
			GameObject gameObject = CreateContainer(new Anchors(), LayoutTypes.TopFull, parent);
			gameObject.name = "AreaEffectContainer";
			ProcessAreaEffect(abilityRow.Text(), gameObject.transform as RectTransform, _hexWidth, abilityRow);
			_ = gameObject.transform;
			(float, int) areaEffectDimensionsInHexes = GetAreaEffectDimensionsInHexes(abilityRow.Text());
			(float, float) tuple = (areaEffectDimensionsInHexes.Item1, areaEffectDimensionsInHexes.Item2);
			LayoutElement layoutElement = gameObject.gameObject.AddComponent<LayoutElement>();
			layoutElement.preferredWidth = tuple.Item1 * _hexWidth;
			layoutElement.preferredHeight = tuple.Item2 * _hexWidth;
			return;
		}
		GameObject gameObject2 = PrefabLoaderCache.MiscGui.Instantiate("PreviewText", parent);
		gameObject2.name = "Text Container";
		RectTransform rectTransform = gameObject2.transform as RectTransform;
		TextMeshProUGUI component = gameObject2.GetComponent<TextMeshProUGUI>();
		component.enableAutoSizing = rowAtt.AutoSize;
		component.fontSizeMin = rowAtt.AutoSizeMin;
		component.fontSizeMax = ((rowAtt.AutoSizeMax == 0) ? GlobalSettings.Instance.m_AbilityCardSettings.StandardFontSize : ((float)rowAtt.AutoSizeMax));
		component.characterSpacing = rowAtt.CharSpacing;
		component.wordSpacing = rowAtt.WordSpacing;
		component.lineSpacing = rowAtt.LineSpacing;
		component.paragraphSpacing = rowAtt.ParSpacing;
		component.alignment = GetTextAlignment(rowAtt.Alignment);
		component.enableWordWrapping = rowAtt.Wrapping;
		component.overflowMode = GetOverflow(rowAtt.Overflow);
		component.font = GlobalSettings.Instance.m_AbilityCardSettings.CardFont;
		component.fontSize = (IsItemCard ? GlobalSettings.Instance.m_AbilityCardSettings.ItemCardFontSize : GlobalSettings.Instance.m_AbilityCardSettings.StandardFontSize);
		component.ForceMeshUpdate();
		string text = (component.text = abilityRow.Text());
		if (_inConsume)
		{
			text = (component.text = TextWrappingBalancer.BalanceText(component, (parent as RectTransform).rect.width, (parent as RectTransform).rect.height));
		}
		if (abilityRow.EnhancementLine != EEnhancementLine.None && abilityRow.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == abilityRow.EnhancementLine).Count() > 0)
		{
			List<CEnhancement> list = abilityRow.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == abilityRow.EnhancementLine).ToList();
			component.enableWordWrapping = false;
			component.text = text.Replace("<sprite name=NoEnhancement>", "").TrimEnd();
			component.ForceMeshUpdate();
			float num = (float)list.Count * GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSize + (float)(list.Count - 1) * GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSpacing;
			float xStartPos = component.preferredWidth / 2f + num / 2f + GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSpacing;
			CreateEnhancement(list, xStartPos, parent, abilityRow.AbilityNameLookupText, num, _enhanceElements);
		}
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.anchoredPosition = Vector2.zero;
		gameObject2.AddComponent<UITextTooltipTarget>();
	}

	private static void CreateEnhancement(List<CEnhancement> enhancements, float xStartPos, Transform parent, string abilityNameLookupText, float containerWidth, List<EnhancementButton> enhanceElements)
	{
		RectTransform rectTransform = PrefabLoaderCache.MiscGui.Instantiate("EnhancementContainer", parent).transform as RectTransform;
		rectTransform.gameObject.name = "EnhancementContainer";
		rectTransform.GetComponent<HorizontalLayoutGroup>().spacing = GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSpacing;
		rectTransform.sizeDelta = new Vector2(containerWidth, GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSize);
		rectTransform.anchoredPosition = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconInitialSpace + xStartPos, rectTransform.anchoredPosition.y);
		foreach (CEnhancement enhancement in enhancements)
		{
			GameObject gameObject = InstantiateEnhancement(enhancement.MarkType, rectTransform);
			if (gameObject != null)
			{
				gameObject.name = "Enhancement";
				EnhancementButton component = gameObject.GetComponent<EnhancementButton>();
				component.Init(enhancement, abilityNameLookupText, parent as RectTransform);
				enhanceElements?.Add(component);
			}
		}
	}

	private static GameObject InstantiateEnhancement(EEnhancementMarkType markType, RectTransform container)
	{
		return markType switch
		{
			EEnhancementMarkType.Circle => PrefabLoaderCache.MiscGui.Instantiate("Enhancement_Circle", container), 
			EEnhancementMarkType.Diamond => PrefabLoaderCache.MiscGui.Instantiate("Enhancement_Diamond", container), 
			EEnhancementMarkType.DiamondPlus => PrefabLoaderCache.MiscGui.Instantiate("Enhancement_DiamondPlus", container), 
			EEnhancementMarkType.Square => PrefabLoaderCache.MiscGui.Instantiate("Enhancement_Square", container), 
			EEnhancementMarkType.Hex => null, 
		};
	}

	public float GenerateFullLayout(bool scaleHeight = true, bool scaleWidth = false, bool shrinkWidthToFit = false)
	{
		try
		{
			if (_consumeButtons.Count > 0)
			{
				foreach (ConsumeButton consumeButton in _consumeButtons)
				{
					consumeButton.GenerateLayout();
				}
				float num = _consumeButtons.Min((ConsumeButton x) => x.layout._fontSize);
				if (num != _fontSize)
				{
					_fontSize = num;
					foreach (ConsumeButton consumeButton2 in _consumeButtons)
					{
						if (!(consumeButton2.layout._fontSize > _fontSize))
						{
							continue;
						}
						foreach (GameObject textContainer in GetTextContainers(consumeButton2.layout.FullLayout))
						{
							TextMeshProUGUI component = textContainer.GetComponent<TextMeshProUGUI>();
							component.fontSize = _fontSize;
							component.ForceMeshUpdate();
						}
						consumeButton2.layout._fontSize = _fontSize;
					}
					foreach (GameObject textContainer2 in GetTextContainers(FullLayout, stopAtConsume: true))
					{
						TextMeshProUGUI component2 = textContainer2.GetComponent<TextMeshProUGUI>();
						component2.fontSize = ((_fontSize > GlobalSettings.Instance.m_AbilityCardSettings.SmallFontSize) ? _fontSize : (_fontSize + 1f));
						component2.ForceMeshUpdate();
					}
				}
			}
			float totalHeight = GetTotalHeight(FullLayout);
			if (scaleHeight)
			{
				float num2 = _fontSize;
				while (totalHeight > _fullArea.height && num2 > GlobalSettings.Instance.m_AbilityCardSettings.MinFontSize)
				{
					foreach (GameObject textContainer3 in GetTextContainers(FullLayout))
					{
						TextMeshProUGUI component3 = textContainer3.GetComponent<TextMeshProUGUI>();
						if (component3.fontSize > GlobalSettings.Instance.m_AbilityCardSettings.MinFontSize)
						{
							component3.fontSize--;
						}
						num2 = Math.Min(num2, component3.fontSize);
						component3.ForceMeshUpdate();
					}
					totalHeight = GetTotalHeight(FullLayout);
				}
				_fontSize = Math.Min(_fontSize, num2);
				FixConsumeDescDimensionsOnShrink(FullLayout);
			}
			totalHeight = GetTotalHeight(FullLayout, andSet: true);
			if (scaleWidth | shrinkWidthToFit)
			{
				float totalWidth = GetTotalWidth(FullLayout, fixConsumes: false);
				RectTransform rectTransform = FullLayout.transform as RectTransform;
				if (shrinkWidthToFit && totalWidth < rectTransform.rect.width)
				{
					rectTransform.sizeDelta = new Vector2(totalWidth, rectTransform.sizeDelta.y);
				}
				if (scaleWidth)
				{
					float num3 = _fontSize;
					while (totalWidth > rectTransform.rect.width && num3 > GlobalSettings.Instance.m_AbilityCardSettings.MinFontSize)
					{
						foreach (GameObject textContainer4 in GetTextContainers(FullLayout))
						{
							TextMeshProUGUI component4 = textContainer4.GetComponent<TextMeshProUGUI>();
							if (component4.fontSize > GlobalSettings.Instance.m_AbilityCardSettings.MinFontSize)
							{
								component4.fontSize--;
							}
							num3 = Math.Min(num3, component4.fontSize);
							component4.ForceMeshUpdate();
						}
						totalWidth = GetTotalWidth(FullLayout, fixConsumes: false);
					}
					_fontSize = Math.Min(_fontSize, num3);
					FixConsumeDescDimensionsOnShrink(FullLayout);
				}
			}
			return totalHeight;
		}
		catch (Exception ex)
		{
			Log.GUIError(ex.Message + "\n" + ex.StackTrace);
			return 0f;
		}
	}

	public void SetVisualsFromConfig(CharacterConfigUI config)
	{
		foreach (var graphicElement in _graphicElements)
		{
			Color color = Color.white;
			string item = graphicElement.Item2;
			Color color2;
			if (item == "class")
			{
				color = config.questConfig.color;
			}
			else if (item == "alt")
			{
				color = config.questConfig.altColor;
			}
			else if (ColorUtility.TryParseHtmlString(graphicElement.Item2, out color2))
			{
				color = color2;
			}
			graphicElement.Item1.color = color;
		}
	}

	private float GetTotalHeight(GameObject parent, bool andSet = false)
	{
		float num = 0f;
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform child = parent.transform.GetChild(i);
			float num2 = 0f;
			if (child.name == "Column Container" || child.name == "Row Container")
			{
				num2 = GetTotalHeight(child.gameObject, andSet);
				if (child.name == "Row Container" && !GetChildren(child).Any((Transform x) => x.name != "Text Container"))
				{
					num2 += GlobalSettings.Instance.m_AbilityCardSettings.RowSpacing;
				}
			}
			else if (child.name == "Text Container")
			{
				TextMeshProUGUI component = child.GetComponent<TextMeshProUGUI>();
				component.ForceMeshUpdate();
				num2 = component.preferredHeight;
			}
			else if (child.name == "AreaEffectContainer")
			{
				num2 = child.GetComponent<LayoutElement>().preferredHeight;
			}
			else if (child.name == "Consume Button" || child.name == "InfuseContainer" || child.name == "SummonContainer" || child.name == "XPContainer" || child.name == "IconContainer" || child.name == "DividerCardElement" || child.name == "MandatoryTrigger" || child.name == "ConditionalElement" || child.name == "BlinkbladeState_Fast" || child.name == "BlinkbladeState_Slow" || child.name == "AddedEffect")
			{
				num2 = (child as RectTransform).rect.height;
			}
			num = ((!(child.name == "Column Container")) ? (num + num2) : Math.Max(num, num2));
		}
		if (andSet)
		{
			List<Transform> children = GetChildren(parent.transform);
			if (parent.name == "Row Container")
			{
				RectTransform obj = parent.transform as RectTransform;
				obj.sizeDelta = new Vector2(obj.sizeDelta.x, num + (children.Any((Transform x) => x.name != "Text Container") ? 0f : GlobalSettings.Instance.m_AbilityCardSettings.RowSpacing));
				obj.anchorMin = new Vector2(0f, 0.5f);
				obj.anchorMax = new Vector2(1f, 0.5f);
				obj.pivot = new Vector2(0.5f, 1f);
			}
			if (children.Any((Transform x) => x.name == "Row Container"))
			{
				children.Reverse();
				for (int num3 = 0; num3 < children.Count; num3++)
				{
					children[num3].SetSiblingIndex(num3);
				}
				float num4 = num / 2f;
				for (int num5 = 0; num5 < parent.transform.childCount; num5++)
				{
					RectTransform rectTransform = parent.transform.GetChild(num5) as RectTransform;
					rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, num4);
					num4 -= rectTransform.rect.height;
				}
			}
		}
		return num;
	}

	private float GetTotalWidth(GameObject parent, bool fixConsumes)
	{
		float num = 20f;
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform child = parent.transform.GetChild(i);
			float num2 = 0f;
			if (child.name == "Row Container" || child.name == "Column Container")
			{
				num2 = GetTotalWidth(child.gameObject, fixConsumes);
			}
			else if (child.name == "Text Container")
			{
				TextMeshProUGUI component = child.GetComponent<TextMeshProUGUI>();
				if (fixConsumes)
				{
					component.text = TextWrappingBalancer.BalanceText(component, (child as RectTransform).rect.width);
				}
				component.ForceMeshUpdate();
				num2 = component.preferredWidth;
			}
			else if (child.name == "AreaEffectContainer")
			{
				num2 = child.GetComponent<LayoutElement>().preferredWidth;
			}
			else if (child.name == "InfuseContainer" || child.name == "Consume Button" || child.name == "XPContainer" || child.name == "IconContainer" || child.name == "AreaEffectContainer")
			{
				num2 = (child as RectTransform).rect.width;
			}
			if (child.name == "Column Container")
			{
				num += num2;
			}
			else if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void FixConsumeDescDimensionsOnShrink(GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform child = parent.transform.GetChild(i);
			if (child.name == "Row Container" || child.name == "Column Container" || child.name == "Consume Button")
			{
				FixConsumeDescDimensionsOnShrink(child.gameObject);
			}
			else
			{
				if (!(child.name == "Description"))
				{
					continue;
				}
				RectTransform rectTransform = child as RectTransform;
				float width = rectTransform.rect.width;
				float height = rectTransform.rect.height;
				float num = width;
				float num2 = height;
				for (int j = 0; j < child.childCount; j++)
				{
					Transform child2 = child.GetChild(j);
					if (child2.name == "Layout Parent")
					{
						num = GetTotalWidth(child2.gameObject, fixConsumes: true);
						num2 = GetTotalHeight(child2.gameObject);
						RectTransform rectTransform2 = child2 as RectTransform;
						if (num != rectTransform2.rect.width || num2 != rectTransform2.rect.height)
						{
							rectTransform2.sizeDelta = new Vector2(num, num2);
						}
						break;
					}
				}
				if (width != num || height != num2)
				{
					rectTransform.sizeDelta = new Vector2(num, num2);
					RectTransform rectTransform3 = child.parent as RectTransform;
					rectTransform3.sizeDelta = new Vector2(rectTransform3.sizeDelta.x, Math.Max(num2, GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize));
					rectTransform3.anchoredPosition = new Vector2(rectTransform3.anchoredPosition.x + (width - num) / 2f, rectTransform3.anchoredPosition.y);
				}
			}
		}
	}

	public List<Transform> GetChildren(Transform parent)
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < parent.childCount; i++)
		{
			list.Add(parent.GetChild(i));
		}
		return list;
	}

	public float GetFullLayoutHeight()
	{
		return GetTotalHeight(FullLayout);
	}

	private void CreateAugment(CardLayoutGroup.AugmentLayout augment, Transform parent)
	{
		AugmentContainer augmentContainer = PrefabLoaderCache.MiscGui.Instantiate<AugmentContainer>("AugmentContainer", parent);
		RectTransform obj = parent.transform as RectTransform;
		obj.anchorMin = new Vector2(0f, 0f);
		obj.anchorMax = new Vector2(1f, 1f);
		parent.name = "AugmentRow";
		augmentContainer.AugmentKeywordText.text = LocaliseText("$Augment$");
		augmentContainer.OnYourMeleeAttacksText.text = LocaliseText("$Augment_OnYourMeleeAttacks$");
		if (augment.DiscardText == null)
		{
			augmentContainer.DiscardReminderText.text = LocaliseText("$Augment_DiscardReminder$");
		}
		else
		{
			augmentContainer.DiscardReminderText.text = LocaliseText(augment.DiscardText);
		}
		augmentContainer.AugmentIcon.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(augment.AugmentIcon);
		augmentContainer.PersistentIcon.sprite = UIInfoTools.Instance.GetDurationIcon(CActiveBonus.EActiveBonusDurationType.Persistent);
		float num = 4f;
		LayoutElement component = augmentContainer.AugmentEffectContainer.GetComponent<LayoutElement>();
		CreateLayout createLayout = new CreateLayout(augment.AugmentAbilityLayout.ParentGroup, new Rect(0f, 0f, component.preferredWidth - num, component.preferredHeight - num), _layoutID, isLongRest: false);
		createLayout.FullLayout.transform.SetParent(augmentContainer.AugmentEffectContainer.transform);
		createLayout.GenerateFullLayout();
		(createLayout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		(createLayout.FullLayout.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
		LayoutElement component2 = augmentContainer.CardContentContainer.GetComponent<LayoutElement>();
		CreateLayout createLayout2 = new CreateLayout(augment.NormalContentLayout.ParentGroup, new Rect(0f, 0f, component2.preferredWidth - num, component2.preferredHeight - num), _layoutID, isLongRest: false);
		createLayout2.FullLayout.transform.SetParent(augmentContainer.CardContentContainer.transform);
		createLayout2.GenerateFullLayout();
		(createLayout2.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		(createLayout2.FullLayout.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
	}

	private void CreateDoom(CardLayoutGroup.DoomLayout doomLayout, Transform parent)
	{
		DoomContainer doomContainer = PrefabLoaderCache.MiscGui.Instantiate<DoomContainer>("DoomContainer", parent);
		RectTransform obj = parent.transform as RectTransform;
		obj.anchorMin = new Vector2(0f, 0f);
		obj.anchorMax = new Vector2(1f, 1f);
		parent.name = "DoomRow";
		doomContainer.DoomKeywordText.text = LocaliseText("$ChooseAbility$");
		doomContainer.DoomKeywordText.ForceMeshUpdate();
		doomContainer.DoomKeywordText.GetComponent<LayoutElement>().minWidth = doomContainer.DoomKeywordText.preferredWidth;
		doomContainer.DoomReminderText.text = (doomLayout.ReminderTextOverride.IsNOTNullOrEmpty() ? LocaliseText(doomLayout.ReminderTextOverride) : LocaliseText("$Doom_MarkText$"));
		if (doomLayout.DiscardText == null)
		{
			doomContainer.DoomDiscardReminderText.text = LocaliseText("$Doom_DiscardReminderText$");
		}
		else
		{
			doomContainer.DoomDiscardReminderText.text = LocaliseText(doomLayout.DiscardText);
		}
		doomContainer.DoomIcon.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(doomLayout.DoomIcon);
		doomContainer.PersistentIcon.sprite = UIInfoTools.Instance.GetDurationIcon(CActiveBonus.EActiveBonusDurationType.Persistent);
		float num = 4f;
		LayoutElement component = doomContainer.MainBox.GetComponent<LayoutElement>();
		CreateLayout createLayout = new CreateLayout(doomLayout.DoomAbilityLayout.ParentGroup, new Rect(0f, 0f, component.preferredWidth - num, component.preferredHeight - num), _layoutID, isLongRest: false);
		createLayout.FullLayout.transform.SetParent(doomContainer.MainBox.transform);
		createLayout.GenerateFullLayout();
		(createLayout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		(createLayout.FullLayout.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
	}

	private void CreateSong(CardLayoutGroup.SongLayout songLayout, Transform parent)
	{
		SongContainer songContainer = PrefabLoaderCache.MiscGui.Instantiate<SongContainer>("SongContainer", parent);
		RectTransform obj = parent.transform as RectTransform;
		obj.anchorMin = new Vector2(0f, 0f);
		obj.anchorMax = new Vector2(1f, 1f);
		parent.name = "SongRow";
		songContainer.SongKeywordText.text = LocaliseText("$Song$");
		songContainer.XPText.text = LocaliseText("$Song_XPText$");
		if (songLayout.DiscardText == null)
		{
			songContainer.DiscardReminderText.text = LocaliseText("$Song_DiscardReminder$");
		}
		else
		{
			songContainer.DiscardReminderText.text = LocaliseText(songLayout.DiscardText);
		}
		songContainer.SongIcon.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(songLayout.SongIcon);
		songContainer.PersistentIcon.sprite = UIInfoTools.Instance.GetDurationIcon(CActiveBonus.EActiveBonusDurationType.Persistent);
		float num = 4f;
		LayoutElement component = songContainer.MainBox.GetComponent<LayoutElement>();
		CreateLayout createLayout = new CreateLayout(songLayout.SongAbilityLayout.ParentGroup, new Rect(0f, 0f, component.preferredWidth - num, component.preferredHeight - num), _layoutID, isLongRest: false);
		createLayout.FullLayout.transform.SetParent(songContainer.MainBox.transform);
		createLayout.GenerateFullLayout();
		(createLayout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
		(createLayout.FullLayout.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
	}

	public static void CreateSummon(CardLayoutGroup.SummonLayout summon, Transform parent, bool isItemCard, int layoutID, List<EnhancementButton> enhanceElements = null, bool leftAlignSummonName = false, bool useDataLocKey = false, int summonID = 0)
	{
		SummonContainer summonContainer = PrefabLoaderCache.MiscGui.Instantiate<SummonContainer>("SummonContainer", parent);
		summonContainer.name = "SummonContainer";
		VerticalLayoutGroup component = summonContainer.GetComponent<VerticalLayoutGroup>();
		if (leftAlignSummonName)
		{
			summonContainer.SummonNameText.alignment = TextAlignmentOptions.Left;
			if (component != null)
			{
				component.childAlignment = TextAnchor.UpperLeft;
				component.childForceExpandWidth = false;
			}
		}
		else
		{
			summonContainer.SummonNameText.alignment = TextAlignmentOptions.Center;
			if (component != null)
			{
				component.childAlignment = TextAnchor.MiddleCenter;
				component.childForceExpandWidth = true;
			}
		}
		summonContainer.SummonNameText.text = LocaliseText("$Summon$ " + (useDataLocKey ? (summon.SummonAbility as CAbilitySummon).SelectedSummonYMLData.LocKey : summon.SummonName));
		if (summon.HideSummonBoxes)
		{
			summonContainer.SummonLT.transform.parent.parent.gameObject.SetActive(value: false);
			return;
		}
		summonContainer.SummonLT.transform.parent.parent.gameObject.SetActive(value: true);
		GameObject summonLT = summonContainer.SummonLT;
		GameObject summonLB = summonContainer.SummonLB;
		GameObject summonMT = summonContainer.SummonMT;
		GameObject summonMB = summonContainer.SummonMB;
		GameObject summonR = summonContainer.SummonR;
		CAbilitySummon cAbilitySummon = summon.SummonAbility as CAbilitySummon;
		HeroSummonYMLData selectedSummonYMLData = cAbilitySummon.SelectedSummonYMLData;
		float baseHealthModifier = 1f;
		if (ScenarioManager.CurrentScenarioState != null)
		{
			baseHealthModifier = ScenarioManager.CurrentScenarioState.SummonHealthModifier;
		}
		int health = selectedSummonYMLData.GetHealth(0, baseHealthModifier);
		string text = health switch
		{
			0 => "-", 
			-1 => "X", 
			_ => health.ToString(), 
		};
		string text2;
		if (selectedSummonYMLData.Attack == -1)
		{
			text2 = "X";
		}
		else
		{
			text2 = ((selectedSummonYMLData.Attack == 0) ? "-" : selectedSummonYMLData.Attack.ToString());
		}
		string text3;
		if (selectedSummonYMLData.Move == -1)
		{
			text3 = "X";
		}
		else
		{
			text3 = ((selectedSummonYMLData.Move == 0) ? "-" : selectedSummonYMLData.Move.ToString());
		}
		string text4;
		if (selectedSummonYMLData.Range == -1)
		{
			text4 = "X";
		}
		else
		{
			text4 = ((selectedSummonYMLData.Range == 0 || selectedSummonYMLData.Range == 1) ? "-" : selectedSummonYMLData.Range.ToString());
		}
		string text5 = "<size=130%><sprite name=Heal> " + text + "</size>";
		Transform transform = summonLT.transform;
		List<CEnhancement> enhancements = cAbilitySummon.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.SummonHealth).ToList();
		List<EnhancementButton> enhanceElements2 = enhanceElements;
		CreateSummonStatText(text5, transform, enhancements, "SUMMON_HEALTH", null, enhanceElements2);
		CreateSummonStatText("<size=130%><sprite name=Attack> " + text2 + "</size>", summonLB.transform, cAbilitySummon.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.SummonAttack).ToList(), "SUMMON_ATTACK", selectedSummonYMLData.AttackInfuse, enhanceElements);
		string text6 = "<size=130%><sprite name=Move> " + text3 + "</size>";
		Transform transform2 = summonMT.transform;
		List<CEnhancement> enhancements2 = cAbilitySummon.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.SummonMove).ToList();
		enhanceElements2 = enhanceElements;
		CreateSummonStatText(text6, transform2, enhancements2, "SUMMON_MOVE", null, enhanceElements2);
		string text7 = "<size=130%><sprite name=Range> " + text4 + "</size>";
		Transform transform3 = summonMB.transform;
		List<CEnhancement> enhancements3 = cAbilitySummon.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.SummonRange).ToList();
		enhanceElements2 = enhanceElements;
		CreateSummonStatText(text7, transform3, enhancements3, "SUMMON_RANGE", null, enhanceElements2);
		if (summon.SpecialText != null)
		{
			float num = 6f;
			LayoutElement component2 = summonR.GetComponent<LayoutElement>();
			CreateLayout createLayout = new CreateLayout(summon.SpecialText.ParentGroup, new Rect(0f, 0f, component2.preferredWidth - num, component2.preferredHeight - num), layoutID, isLongRest: false);
			createLayout.FullLayout.transform.SetParent(summonR.transform);
			createLayout.GenerateFullLayout();
			(createLayout.FullLayout.transform as RectTransform).anchoredPosition = Vector2.zero;
			(createLayout.FullLayout.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
			(createLayout.FullLayout.transform as RectTransform).localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			summonR.SetActive(value: false);
		}
		if (isItemCard)
		{
			summonR.SetActive(value: false);
		}
	}

	public static void CreateSummonStatText(string text, Transform parent, List<CEnhancement> enhancements, string abilityNameLookupText, ElementInfusionBoardManager.EElement? infuse = null, List<EnhancementButton> enhanceElements = null)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("PreviewText", parent);
		gameObject.name = "Text Container";
		gameObject.AddComponent<UITextTooltipTarget>();
		TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
		component.text = text;
		component.font = GlobalSettings.Instance.m_AbilityCardSettings.CardFont;
		component.fontSize = GlobalSettings.Instance.m_AbilityCardSettings.SummonStatFontSize;
		component.alignment = TextAlignmentOptions.Center;
		if (enhancements.Count > 0 || infuse.HasValue)
		{
			component.text += "<line-height=1>\n<align=left><voffset=0.5><pos=70%>";
			if (enhancements.Count > 0)
			{
				component.enableWordWrapping = false;
				component.text = text.Replace("<sprite name=NoEnhancement>", "").TrimEnd();
				component.ForceMeshUpdate();
				float num = (float)enhancements.Count * GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSize + (float)(enhancements.Count - 1) * GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSpacing;
				CreateEnhancement(xStartPos: component.preferredWidth / 2f + num / 2f, enhancements: enhancements.ToList(), parent: parent, abilityNameLookupText: abilityNameLookupText, containerWidth: num, enhanceElements: enhanceElements);
			}
			if (infuse.HasValue)
			{
				string text2 = component.text;
				ElementInfusionBoardManager.EElement? eElement = infuse;
				component.text = text2 + " <voffset=0.9><sprite name=" + eElement.ToString() + ">";
			}
		}
		RectTransform obj = gameObject.transform as RectTransform;
		obj.anchorMin = Vector2.zero;
		obj.anchorMax = Vector2.one;
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.anchoredPosition = Vector2.zero;
	}

	private void CreateDurationIcon(CActiveBonus.EActiveBonusDurationType duration, Transform parent)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("InfuseContainer", parent);
		gameObject.name = "IconContainer";
		(gameObject.transform as RectTransform).sizeDelta = new Vector2(GlobalSettings.Instance.m_AbilityCardSettings.DurationIconSize, GlobalSettings.Instance.m_AbilityCardSettings.DurationIconSize);
		GameObject gameObject2 = PrefabLoaderCache.MiscGui.Instantiate("DurationRes", gameObject.transform);
		gameObject2.GetComponentInChildren<Image>().sprite = UIInfoTools.Instance.GetDurationIcon(duration);
		UITextTooltipTarget component = gameObject2.GetComponent<UITextTooltipTarget>();
		component.tooltipText = "Glossary_" + duration;
		component.Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
		LayoutElement component2 = gameObject2.GetComponent<LayoutElement>();
		component2.preferredWidth = GlobalSettings.Instance.m_AbilityCardSettings.DurationIconSize;
		component2.preferredHeight = GlobalSettings.Instance.m_AbilityCardSettings.DurationIconSize;
	}

	private void CreateXPIcons(List<int> xpList, bool showPersistentIcon, Transform parent)
	{
		(PrefabLoaderCache.MiscGui.Instantiate("XPContainer", parent).transform as RectTransform).GetComponent<XPTrackerContainer>().Init(xpList);
	}

	private void CreateDivider(Transform parent, string color)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("DividerCardElement", parent);
		gameObject.name = "DividerCardElement";
		Image componentInChildren = gameObject.GetComponentInChildren<Image>();
		if (componentInChildren != null)
		{
			_graphicElements.Add((componentInChildren, color));
		}
	}

	private void CreateMandatoryTrigger(CardLayoutGroup.MandatoryTriggerLayout layout, Transform parent, string color)
	{
		GlobalSettings.AbilityCardSettings abilityCardSettings = GlobalSettings.Instance.m_AbilityCardSettings;
		MandatoryTriggerContainer mandatoryTriggerContainer = PrefabLoaderCache.MiscGui.Instantiate<MandatoryTriggerContainer>("MandatoryTriggerContainer", parent);
		mandatoryTriggerContainer.name = "MandatoryTrigger";
		RectTransform rectTransform = mandatoryTriggerContainer.transform as RectTransform;
		RectTransform rectTransform2 = parent as RectTransform;
		GameObject gameObject = mandatoryTriggerContainer.gameObject.FindInChildren("Left");
		mandatoryTriggerContainer.gameObject.FindInChildren("Right");
		float num = ((gameObject == null) ? 0f : (gameObject.transform as RectTransform).rect.width);
		rectTransform.sizeDelta = new Vector2(rectTransform2.rect.width - num, rectTransform.sizeDelta.y);
		CreateLayout createLayout = new CreateLayout(layout.Layout.ParentGroup, (mandatoryTriggerContainer.transform as RectTransform).rect, _layoutID, isLongRest: false);
		createLayout.FullLayout.transform.SetParent(mandatoryTriggerContainer.Container.transform);
		RectTransform obj = createLayout.FullLayout.transform as RectTransform;
		obj.anchorMin = Vector2.zero;
		obj.anchorMax = Vector2.one;
		obj.offsetMin = Vector2.zero;
		obj.offsetMax = Vector2.zero;
		createLayout.GenerateFullLayout();
		float totalWidth = GetTotalWidth(createLayout.FullLayout, fixConsumes: true);
		float y = createLayout.GetTotalHeight(createLayout.FullLayout) + 2f * abilityCardSettings.MandatoryTriggerPadding;
		float x = Mathf.Min(totalWidth + num + 2f * abilityCardSettings.MandatoryTriggerPadding, rectTransform2.rect.width);
		rectTransform.sizeDelta = new Vector2(x, y);
		RectTransform obj2 = mandatoryTriggerContainer.Container.gameObject.FindInChildren("Content").transform as RectTransform;
		float mandatoryTriggerPadding = abilityCardSettings.MandatoryTriggerPadding;
		float y2 = obj2.offsetMax.y - mandatoryTriggerPadding;
		float y3 = obj2.offsetMin.y + mandatoryTriggerPadding;
		float x2 = obj2.offsetMin.x + mandatoryTriggerPadding;
		float x3 = obj2.offsetMax.x - mandatoryTriggerPadding;
		obj2.offsetMin = new Vector2(x2, y3);
		obj2.offsetMax = new Vector2(x3, y2);
		Image[] componentsInChildren = mandatoryTriggerContainer.Background.GetComponentsInChildren<Image>();
		foreach (Image item in componentsInChildren)
		{
			_graphicElements.Add((item, color));
		}
		_graphicElements.Add((mandatoryTriggerContainer.MandatorySymbol, color));
	}

	private void CreateConditionalData(CardLayoutGroup.ConditionalElementLayout layout, Transform parent, string color)
	{
		GlobalSettings.AbilityCardSettings abilityCardSettings = GlobalSettings.Instance.m_AbilityCardSettings;
		ConditionalElementContainer conditionalElementContainer = PrefabLoaderCache.MiscGui.Instantiate<ConditionalElementContainer>("ConditionalElementContainer", parent);
		RectTransform rectTransform = (RectTransform)conditionalElementContainer.transform;
		RectTransform rectTransform2 = (RectTransform)parent;
		CreateLayout createLayout = new CreateLayout(layout.Layout.ParentGroup, rectTransform.rect, _layoutID, isLongRest: false);
		if (!layout.Show)
		{
			rectTransform2.anchoredPosition = Vector2.zero;
			rectTransform2.sizeDelta = Vector2.zero;
			rectTransform2.localScale = Vector3.zero;
			rectTransform2.name += "_Dummy";
		}
		conditionalElementContainer.name = "ConditionalElement";
		conditionalElementContainer.TopElement.gameObject.SetActive(layout.HasTop);
		if (!layout.HasTop || !layout.Show)
		{
			RectTransform rectTransform3 = (RectTransform)conditionalElementContainer.Background.transform;
			rectTransform3.anchoredPosition = Vector2.zero;
			rectTransform3.sizeDelta = Vector2.zero;
		}
		rectTransform.sizeDelta = (layout.Show ? new Vector2(rectTransform2.rect.width - 2f * abilityCardSettings.ConditionalElementPadding, rectTransform.sizeDelta.y) : Vector2.zero);
		createLayout.FullLayout.transform.SetParent(conditionalElementContainer.transform.GetChild(0));
		createLayout.FullLayout.transform.localPosition = Vector3.zero;
		createLayout.GenerateFullLayout(scaleHeight: true, scaleWidth: true, shrinkWidthToFit: true);
		float totalWidth = GetTotalWidth(createLayout.FullLayout, fixConsumes: true);
		totalWidth = Mathf.Min(totalWidth + 2f * abilityCardSettings.MandatoryTriggerPadding, rectTransform2.rect.width);
		rectTransform.sizeDelta = new Vector2
		{
			x = (layout.Show ? totalWidth : 0f),
			y = (layout.Show ? (createLayout.GetTotalHeight(createLayout.FullLayout) + 2f * abilityCardSettings.ConditionalElementPadding) : 0f)
		};
		_graphicElements.Add((conditionalElementContainer.Background, color));
		Image[] componentsInChildren = conditionalElementContainer.TopElement.GetComponentsInChildren<Image>();
		foreach (Image item in componentsInChildren)
		{
			_graphicElements.Add((item, color));
		}
	}

	private void CreateBlinkbladeState(CardLayoutGroup.BlinkbladeStateLayout layout, Transform parent, string color)
	{
		RectTransform rectTransform = parent as RectTransform;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		GlobalSettings.AbilityCardSettings abilityCardSettings = GlobalSettings.Instance.m_AbilityCardSettings;
		bool isFast = layout.IsFast;
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate(isFast ? "Blinkblade_FastStateUIElement" : "Blinkblade_SlowStateUIElement", parent);
		gameObject.name = string.Format("BlinkbladeState_{0}", isFast ? "Fast" : "Slow");
		GameObject gameObject2 = gameObject.FindInChildren("Content");
		RectTransform rectTransform2 = gameObject.transform as RectTransform;
		GameObject gameObject3 = gameObject.FindInChildren("Left");
		GameObject gameObject4 = gameObject.FindInChildren("Right");
		float num = ((gameObject3 == null) ? 0f : (gameObject3.transform as RectTransform).rect.width);
		float num2 = ((gameObject4 == null) ? 0f : (gameObject4.transform as RectTransform).rect.width);
		float num3 = rectTransform.rect.width - num - num2;
		bool flag = layout.Width != -1f;
		float num4;
		if (flag)
		{
			num4 = (layout.IsPercent ? (num3 * layout.Width / 100f) : Mathf.Min(layout.Width, num3));
		}
		else
		{
			num4 = num3;
		}
		rectTransform2.sizeDelta = new Vector2(num4, rectTransform2.sizeDelta.y);
		CreateLayout createLayout = new CreateLayout(layout.Layout.ParentGroup, (gameObject.transform as RectTransform).rect, _layoutID, isLongRest: false);
		createLayout.FullLayout.transform.SetParent(gameObject2.transform);
		RectTransform obj = createLayout.FullLayout.transform as RectTransform;
		obj.anchorMin = Vector2.zero;
		obj.anchorMax = Vector2.one;
		obj.offsetMin = Vector2.zero;
		obj.offsetMax = Vector2.zero;
		RectTransform obj2 = gameObject2.transform as RectTransform;
		float mandatoryTriggerPadding = abilityCardSettings.MandatoryTriggerPadding;
		float y = obj2.offsetMax.y - mandatoryTriggerPadding;
		float y2 = obj2.offsetMin.y + mandatoryTriggerPadding;
		float x = obj2.offsetMin.x + mandatoryTriggerPadding;
		float x2 = obj2.offsetMax.x - mandatoryTriggerPadding;
		obj2.offsetMin = new Vector2(x, y2);
		obj2.offsetMax = new Vector2(x2, y);
		createLayout.GenerateFullLayout();
		float totalWidth = GetTotalWidth(createLayout.FullLayout, fixConsumes: false);
		float num5 = (flag ? num4 : Mathf.Max(num4, totalWidth));
		float num6 = (isFast ? num : num2);
		float y3 = Mathf.Max(createLayout.GetTotalHeight(createLayout.FullLayout, andSet: true) + 2f * abilityCardSettings.MandatoryTriggerPadding, 30f);
		num4 = Mathf.Min(num5 + num6 + 2f * abilityCardSettings.MandatoryTriggerPadding, rectTransform.rect.width);
		rectTransform2.sizeDelta = new Vector2(num4, y3);
	}

	private void CreateAddedEffect(string addedEffect, Transform parent, string color)
	{
		GlobalSettings.AbilityCardSettings abilityCardSettings = GlobalSettings.Instance.m_AbilityCardSettings;
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("AddedEffectContainer", parent);
		gameObject.name = "AddedEffect";
		RectTransform rectTransform = gameObject.transform as RectTransform;
		RectTransform rectTransform2 = parent as RectTransform;
		rectTransform.sizeDelta = new Vector2(rectTransform2.rect.width - 2f * abilityCardSettings.AddedEffectPadding, rectTransform.rect.height);
		rectTransform.ForceUpdateRectTransforms();
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		componentInChildren.gameObject.GetOrAdd<UITextTooltipTarget>();
		componentInChildren.text = LocaliseText(addedEffect);
		componentInChildren.fontSize = _fontSize;
		componentInChildren.font = GlobalSettings.Instance.m_AbilityCardSettings.CardFont;
		componentInChildren.ForceMeshUpdate();
		float x = Mathf.Min(rectTransform2.rect.width - 2f * abilityCardSettings.AddedEffectPadding, componentInChildren.preferredWidth);
		float y = componentInChildren.preferredHeight;
		if (componentInChildren.preferredHeight <= abilityCardSettings.AddedEffectSingleRowHeight)
		{
			y = abilityCardSettings.AddedEffectSingleRowHeight;
		}
		rectTransform.sizeDelta = new Vector2(x, y);
		componentInChildren.ForceMeshUpdate();
		x = Mathf.Min(rectTransform2.rect.width, componentInChildren.preferredWidth + 2f * abilityCardSettings.AddedEffectPadding);
		rectTransform.sizeDelta = new Vector2(x, y);
		_graphicElements.Add((gameObject.GetComponentInChildren<Image>(), color));
	}

	private static GameObject CreateVerticalLayoutContainer(Rect parentRect, TextAnchor childAlign = TextAnchor.MiddleCenter)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("TextContainer");
		RectTransform obj = gameObject.transform as RectTransform;
		obj.anchorMin = Vector2.zero;
		obj.anchorMax = Vector2.one;
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.anchoredPosition = Vector2.zero;
		obj.sizeDelta = parentRect.size;
		VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.childAlignment = childAlign;
		verticalLayoutGroup.spacing = GlobalSettings.Instance.m_GlossaryCardSettings.RowSpacing;
		verticalLayoutGroup.childControlHeight = true;
		verticalLayoutGroup.childControlWidth = true;
		verticalLayoutGroup.childForceExpandHeight = false;
		verticalLayoutGroup.childForceExpandWidth = false;
		return gameObject;
	}

	private static GameObject CreateStretchContainer(Rect parentRect, LayoutTypes textAlign)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("TextContainer");
		RectTransform obj = gameObject.transform as RectTransform;
		obj.sizeDelta = parentRect.size;
		SetContentAlignment(obj, textAlign, setAnchors: true);
		return gameObject;
	}

	private static GameObject CreateContainer(Anchors area, LayoutTypes textAlign, Transform parent)
	{
		GameObject gameObject = PrefabLoaderCache.MiscGui.Instantiate("TextContainer", parent);
		RectTransform obj = gameObject.transform as RectTransform;
		SetContentAlignment(obj, textAlign, setAnchors: false);
		obj.anchorMin = area.AnchorMin;
		obj.anchorMax = area.AnchorMax;
		obj.anchoredPosition = Vector2.zero;
		return gameObject;
	}

	private static void SetGrowContent(GameObject container, GrowContentType growContentType)
	{
		if (growContentType != GrowContentType.None)
		{
			container.AddComponent<VerticalLayoutGroup>();
			ContentSizeFitter contentSizeFitter = container.AddComponent<ContentSizeFitter>();
			if (growContentType == GrowContentType.GrowBoth || growContentType == GrowContentType.GrowVertical)
			{
				contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			}
			if (growContentType == GrowContentType.GrowBoth || growContentType == GrowContentType.GrowHorizontal)
			{
				contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			}
		}
	}

	private static void SetContentAlignment(RectTransform rect, LayoutTypes textAlign, bool setAnchors)
	{
		switch (textAlign)
		{
		case LayoutTypes.TopPreview:
			if (setAnchors)
			{
				rect.anchorMin = new Vector2(0f, 0.5f);
				rect.anchorMax = new Vector2(0f, 0.5f);
				rect.anchoredPosition = Vector2.zero;
			}
			rect.pivot = new Vector2(0f, 0.5f);
			break;
		case LayoutTypes.BottomPreview:
			if (setAnchors)
			{
				rect.anchorMin = new Vector2(1f, 0.5f);
				rect.anchorMax = new Vector2(1f, 0.5f);
				rect.anchoredPosition = Vector2.zero;
			}
			rect.pivot = new Vector2(1f, 0.5f);
			break;
		case LayoutTypes.TopFull:
		case LayoutTypes.BottomFull:
			if (setAnchors)
			{
				rect.anchorMin = new Vector2(0.5f, 0.5f);
				rect.anchorMax = new Vector2(0.5f, 0.5f);
				rect.anchoredPosition = Vector2.zero;
			}
			rect.pivot = new Vector2(0.5f, 0.5f);
			break;
		}
	}

	private static TextAlignmentOptions GetTextAlignment(string alignment)
	{
		return ((TextAlignmentOptions[])Enum.GetValues(typeof(TextAlignmentOptions))).SingleOrDefault((TextAlignmentOptions x) => x.ToString() == alignment);
	}

	private static TextOverflowModes GetOverflow(string overflow)
	{
		return ((TextOverflowModes[])Enum.GetValues(typeof(TextOverflowModes))).SingleOrDefault((TextOverflowModes x) => x.ToString() == overflow);
	}

	public static (float, int) GetAreaEffectDimensionsInHexes(string areaString)
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		string[] array = areaString.Split('|');
		foreach (string text in array)
		{
			if (!(text == "CantFlip"))
			{
				string[] array2 = text.Split(',');
				if (array2.Length != 3)
				{
					Log.GUIError("Invalid Area Effect string: " + areaString);
					return (0f, 0);
				}
				int.TryParse(array2[0], out var result);
				num = Math.Min(num, result);
				num2 = Math.Max(num2, result);
				int.TryParse(array2[1], out var result2);
				num3 = Math.Min(num3, result2);
				num4 = Math.Max(num4, result2);
			}
		}
		return ((num2 - num) / 2f + 1f, num4 - num3 + 1);
	}

	private static void ProcessAreaEffect(string areaString, RectTransform parent, float hexWidth, CardLayoutRow abilityRow = null)
	{
		List<GameObject> list = new List<GameObject>();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		string[] array = areaString.Split('|');
		int enhancedSlotID = 0;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == "CantFlip")
			{
				continue;
			}
			string[] array3 = text.Split(',');
			if (array3.Length != 3)
			{
				Log.GUIError("Invalid Area Effect string: " + areaString);
				return;
			}
			string text2 = ((array3[2].Length == 1) ? GetAreaIconName(array3[2]) : array3[2]);
			GameObject gameObject = null;
			EnhancedAreaHex enhancedAreaHex = null;
			if (text2.ToLower() == "dot")
			{
				gameObject = PrefabLoaderCache.MiscGui.Instantiate("EnhancedAreaHex", parent);
				enhancedAreaHex = gameObject.GetComponent<EnhancedAreaHex>();
			}
			else
			{
				gameObject = new GameObject("AreaEffectHex");
				gameObject.transform.SetParent(parent);
				gameObject.AddComponent<Image>().sprite = GetAtlasSprite(UIInfoTools.Instance.AreaEffectSpriteAtlas, text2);
			}
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			component.sizeDelta = new Vector2(hexWidth, hexWidth);
			if (enhancedAreaHex != null && abilityRow != null)
			{
				CEnhancement cEnhancement = abilityRow.AbilityEnhancements.SingleOrDefault((CEnhancement s) => s.EnhancementLine == EEnhancementLine.AreaHex && s.EnhancementSlot == enhancedSlotID);
				if (cEnhancement != null)
				{
					enhancedSlotID++;
					enhancedAreaHex.Init(cEnhancement, "AbilityCard/Dot", "AbilityCard/Enhanced", abilityRow.AbilityNameLookupText, parent.transform.parent as RectTransform);
				}
				else
				{
					Log.GUIError("Error: Enhancement for area could not be found");
				}
			}
			if (!int.TryParse(array3[0], out var result))
			{
				Log.GUIError("Unable to parse area effect hexX " + array3[0]);
				continue;
			}
			if (!int.TryParse(array3[1], out var result2))
			{
				Log.GUIError("Unable to parse area effect hexY " + array3[1]);
				continue;
			}
			float num5 = hexWidth / 2f * (float)result;
			float num6 = hexWidth * 0.75f * (float)result2;
			component.anchoredPosition = new Vector2(num5, num6);
			float num7 = num5 - hexWidth / 2f;
			float num8 = num5 + hexWidth / 2f;
			float num9 = num6 + hexWidth / 2f;
			float num10 = num6 - hexWidth / 2f;
			num = ((num7 < num) ? num7 : num);
			num3 = ((num8 > num3) ? num8 : num3);
			num2 = ((num9 > num2) ? num9 : num2);
			num4 = ((num10 < num4) ? num10 : num4);
			gameObject.AddComponent<UITextTooltipTarget>();
			list.Add(gameObject);
		}
		float num11 = (parent.rect.width / 2f - num3 - (parent.rect.width / 2f + num)) / 2f;
		float num12 = (parent.rect.height / 2f - num2 - (parent.rect.height / 2f + num4)) / 2f;
		foreach (GameObject item in list)
		{
			RectTransform component2 = item.GetComponent<RectTransform>();
			float x = component2.anchoredPosition.x + num11;
			float y = component2.anchoredPosition.y + num12;
			component2.anchoredPosition = new Vector2(x, y);
		}
	}

	public static Sprite GetAtlasSprite(SpriteAtlas atlas, string eType)
	{
		return atlas.GetSprite(eType);
	}

	private static string GetAreaIconName(string value)
	{
		return value switch
		{
			"G" => "Grey", 
			"R" => "Red", 
			"B" => "Blue", 
			"E" => "Dot", 
			"S" => "Blank", 
			_ => string.Empty, 
		};
	}

	private static CardLayoutGroup LocaliseLayout(CardLayoutGroup parentGroup)
	{
		foreach (CardLayoutRow abilityRow in GetAbilityRows(parentGroup))
		{
			abilityRow.SetLocalisedText(string.Empty);
			abilityRow.SetLocalisedText(LocaliseText(abilityRow.Text()));
		}
		return parentGroup;
	}

	private static IEnumerable<CardLayoutRow> GetAbilityRows(CardLayoutGroup group)
	{
		if (group.DataType == CardLayoutGroup.DataTypes.RowData)
		{
			yield return group.Data;
		}
		else if (group.DataType == CardLayoutGroup.DataTypes.Consume)
		{
			foreach (CardLayoutRow abilityRow in GetAbilityRows(group.Consume.Text.ParentGroup))
			{
				yield return abilityRow;
			}
		}
		else
		{
			if ((group.DataType != CardLayoutGroup.DataTypes.RowCollection && group.DataType != CardLayoutGroup.DataTypes.ColumnCollection) || group.Collection == null)
			{
				yield break;
			}
			foreach (CardLayoutGroup item in group.Collection)
			{
				if (item.DataType == CardLayoutGroup.DataTypes.RowData)
				{
					yield return item.Data;
					continue;
				}
				foreach (CardLayoutRow abilityRow2 in GetAbilityRows(item))
				{
					yield return abilityRow2;
				}
			}
		}
	}

	private static IEnumerable<GameObject> GetTextContainers(GameObject parent, bool stopAtConsume = false)
	{
		for (int x = 0; x < parent.transform.childCount; x++)
		{
			Transform child = parent.transform.GetChild(x);
			if (child.name == "Text Container")
			{
				yield return child.gameObject;
			}
			else
			{
				if (stopAtConsume && child.name == "Consume Button")
				{
					continue;
				}
				foreach (GameObject textContainer in GetTextContainers(child.gameObject, stopAtConsume))
				{
					yield return textContainer;
				}
			}
		}
	}

	public static string LocaliseText(string locText, bool skipWarnings = false, string arg0 = "", string arg1 = "")
	{
		while (locText.Contains('$'))
		{
			string key = CardLayoutRow.GetKey(locText, '$');
			string translation = LocalizationManager.GetTranslation(key, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings);
			if (translation == null)
			{
				Console.WriteLine("Localisation keyword " + key + " could not be found in the Localisation Manager.");
				locText = CardLayoutRow.ReplaceKey(locText, '$', "LOC_MISSING");
			}
			else
			{
				locText = CardLayoutRow.ReplaceKey(locText, '$', translation);
			}
			locText = Regex.Replace(locText, "{hazardous}", ScenarioManager.HazardousTerrainDamage.ToString());
			locText = Regex.Replace(locText, "{trap}", ScenarioManager.TrapDamage.ToString());
			locText = string.Format(locText, arg0, arg1);
		}
		return locText;
	}

	private static string ResolveColor(CardLayoutGroup group)
	{
		if (group.Color != "inherit")
		{
			return group.Color;
		}
		CardLayoutGroup parent = group.Parent;
		while (parent != null && (parent.Color == "inherit" || parent.Color == ""))
		{
			parent = parent.Parent;
		}
		if (parent != null)
		{
			return parent.Color;
		}
		return "";
	}
}

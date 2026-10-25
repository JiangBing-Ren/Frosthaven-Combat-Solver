using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class PreviewEffectGenerator : Singleton<PreviewEffectGenerator>
{
	[SerializeField]
	private TMP_SpriteAsset iconSpriteAsset;

	private const string SCALAR_FORMAT = "X{0}";

	public static string GeneratePreview(ItemData data)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (data.ShieldValue > 0 && (data.Abilities == null || !data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Shield)))
		{
			if (data.ShieldValue == int.MaxValue)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.PreventDamage));
			}
			else
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Shield, data.ShieldValue));
			}
		}
		if (data.RetaliateValue > 0 && (data.Abilities == null || !data.Abilities.Exists((CAbility it) => it.AbilityType == CAbility.EAbilityType.Retaliate)))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Retaliate, data.RetaliateValue));
		}
		if (data.Abilities != null && data.Abilities.Count > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			string text = GenerateDescription(data.Abilities, isForPreview: true, isFromItemPreview: true);
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.AppendLine(text.TrimEnd());
			}
		}
		else if (data.Overrides != null)
		{
			if (data.CompareAbility != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				string text2 = GenerateDescription(data.Overrides[0], data.CompareAbility.AbilityType);
				if (!string.IsNullOrEmpty(text2))
				{
					stringBuilder.AppendLine(text2.TrimEnd());
				}
			}
			else
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				string text3 = GenerateDescription(data.Overrides);
				if (!string.IsNullOrEmpty(text3))
				{
					stringBuilder.AppendLine(text3.TrimEnd());
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string GenerateDescription(CActionAugmentation augmentation)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < augmentation.AugmentationOps.Count; i++)
		{
			CActionAugmentationOp op = augmentation.AugmentationOps[i];
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(op));
		}
		for (int j = 0; j < augmentation.Infusions.Count; j++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(GenerateLine(augmentation.Infusions[j].ToString()));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CAbility.EAbilityType type, int value = 0, bool self = false, bool isScalar = false, bool isBuff = false)
	{
		if ((type == CAbility.EAbilityType.Damage) & self)
		{
			type = CAbility.EAbilityType.AddHeal;
			value = -Math.Abs(value);
		}
		string icon = GetIcon(type);
		AbilityTypePreviewConfigUI abilityPreviewConfig = UIInfoTools.Instance.GetAbilityPreviewConfig(type);
		if (abilityPreviewConfig != null)
		{
			bool flag = abilityPreviewConfig.showValueSign;
			if (flag && !isBuff)
			{
				flag = false;
			}
			string format = abilityPreviewConfig.GetFormat();
			if (format.IsNOTNullOrEmpty())
			{
				string format2 = format;
				string arg = icon;
				string arg2 = (isScalar ? $"X{value}" : ((flag && value > 0) ? $"+{value}" : value.ToString()));
				return string.Format(format2, arg, arg2);
			}
		}
		if (type != CAbility.EAbilityType.PreventDamage && CCondition.NegativeConditionFromAbility(type) == CCondition.ENegativeCondition.NA && value != 0)
		{
			if (type == CAbility.EAbilityType.RemoveConditions && value == 1)
			{
				return "";
			}
			switch (type)
			{
			case CAbility.EAbilityType.Shield:
			case CAbility.EAbilityType.Retaliate:
				return GenerateLine(icon, isScalar ? $"X{value}" : value.ToString());
			case CAbility.EAbilityType.Push:
			case CAbility.EAbilityType.Pull:
			case CAbility.EAbilityType.Teleport2:
				return GenerateLine(icon, isScalar ? $"X{value}" : value.ToString(), iconBeforeValue: false);
			default:
			{
				string icon2 = icon;
				string value2;
				if (isScalar)
				{
					value2 = $"X{value}";
				}
				else
				{
					value2 = (((value > 0) & isBuff) ? $"+{value}" : value.ToString());
				}
				return GenerateLine(icon2, value2, iconBeforeValue: false);
			}
			}
		}
		return GenerateLine(icon);
	}

	public static string GenerateDescription(CActionAugmentationOp op)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (op.Ability != null)
		{
			stringBuilder.Append(GenerateDescription(op.Ability));
		}
		CAbilityOverride abilityOverride = op.AbilityOverride;
		if (abilityOverride != null && abilityOverride.AbilityType.HasValue)
		{
			stringBuilder.Append(GenerateDescription(op.AbilityOverride));
			if (op.AbilityOverride.AbilityType != CAbility.EAbilityType.None && op.ParentAbilityType != CAbility.EAbilityType.None && op.AbilityOverride.AbilityType != op.ParentAbilityType)
			{
				stringBuilder.Append(GenerateDescription(op.AbilityOverride.AbilityType.Value));
			}
		}
		else if (op.AbilityOverride != null)
		{
			stringBuilder.Append(GenerateDescription(op.AbilityOverride, op.AbilityOverride.AbilityType ?? op.ParentAbilityType));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateLine(string icon, string value = null, bool iconBeforeValue = true)
	{
		string text = string.Empty;
		if (icon != null && icon.Length > 0)
		{
			text = "<sprite name=\"" + icon + "\">";
		}
		if (value != null)
		{
			if (value == "99")
			{
				value = "∞";
			}
			if (iconBeforeValue)
			{
				return string.Format("{1}{0}", value, text);
			}
			return value + text;
		}
		return text;
	}

	public static string GenerateDescription(CAbilityOverride cAbilityOverride, CAbility.EAbilityType? abilityType = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (cAbilityOverride.GetVar("PreviewOverride").GetString(out var val))
		{
			stringBuilder.AppendLine(val);
			return stringBuilder.ToString();
		}
		if (!abilityType.HasValue)
		{
			abilityType = cAbilityOverride.AbilityType;
		}
		if (cAbilityOverride.DamageSelfBeforeAttack.HasValue)
		{
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Damage, cAbilityOverride.DamageSelfBeforeAttack.Value, self: true));
		}
		if (abilityType.HasValue && cAbilityOverride.Strength.HasValue)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(abilityType.Value, cAbilityOverride.Strength.Value, cAbilityOverride.AbilityFilter != null && cAbilityOverride.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), cAbilityOverride.ActiveBonusData != null && (cAbilityOverride.ActiveBonusData.StrengthIsScalar || cAbilityOverride.ActiveBonusData.AbilityStrengthIsScalar), isBuff: true));
		}
		if (cAbilityOverride.Pierce.HasValue && cAbilityOverride.Pierce > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			string text = ((cAbilityOverride.Pierce > 1) ? cAbilityOverride.Pierce.ToString() : null);
			if (text != null && cAbilityOverride.Pierce >= 99999)
			{
				stringBuilder.Append(GenerateLine("IgnoreShield"));
			}
			else
			{
				stringBuilder.Append(GenerateLine("Pierce", text));
			}
		}
		if (cAbilityOverride.Range.HasValue)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription((abilityType.HasValue && abilityType == CAbility.EAbilityType.Loot) ? abilityType.Value : CAbility.EAbilityType.AddRange, cAbilityOverride.Range.Value, self: false, isScalar: false, isBuff: true));
		}
		if (cAbilityOverride.RetaliateRange.HasValue)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddRange, cAbilityOverride.RetaliateRange.Value, self: false, isScalar: false, isBuff: true));
		}
		if (cAbilityOverride.NumberOfTargets.HasValue && cAbilityOverride.NumberOfTargets > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddTarget, cAbilityOverride.NumberOfTargets.Value, self: false, isScalar: false, isBuff: true));
		}
		if (cAbilityOverride.Jump.HasValue && cAbilityOverride.Jump.Value)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateLine("Jump"));
		}
		if (cAbilityOverride.MiscAbilityData != null && cAbilityOverride.MiscAbilityData.AttackHasAdvantage.HasValue && cAbilityOverride.MiscAbilityData.AttackHasAdvantage.Value)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Advantage));
		}
		if (cAbilityOverride.NegativeConditions != null)
		{
			foreach (CCondition.ENegativeCondition negativeCondition in cAbilityOverride.NegativeConditions)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(negativeCondition.ToString())));
			}
		}
		if (cAbilityOverride.PositiveConditions != null)
		{
			foreach (CCondition.EPositiveCondition positiveCondition in cAbilityOverride.PositiveConditions)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(positiveCondition.ToString())));
			}
		}
		if (cAbilityOverride.SubAbilities != null)
		{
			foreach (CAbility subAbility in cAbilityOverride.SubAbilities)
			{
				if (subAbility.AbilityType != CAbility.EAbilityType.ImmunityTo || !string.IsNullOrEmpty(subAbility.PreviewEffectText))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(GenerateDescription(subAbility));
				}
			}
		}
		if (cAbilityOverride.ConditionalOverrides != null)
		{
			foreach (CConditionalOverride conditionalOverride in cAbilityOverride.ConditionalOverrides)
			{
				foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
				{
					string value = GenerateDescription(abilityOverride, abilityType);
					if (value.IsNOTNullOrEmpty())
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append(value);
					}
				}
			}
		}
		if (cAbilityOverride.AttackEffects != null)
		{
			foreach (CAttackEffect attackEffect in cAbilityOverride.AttackEffects)
			{
				string value2 = GenerateDescription(attackEffect);
				if (value2.IsNOTNullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(value2);
				}
			}
		}
		if (cAbilityOverride.ChooseAbilities != null)
		{
			foreach (CAbility chooseAbility in cAbilityOverride.ChooseAbilities)
			{
				string value3 = GenerateDescription(chooseAbility);
				if (value3.IsNOTNullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(value3);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CAttackEffect effect)
	{
		if (effect.Effect == CAttackEffect.EAttackEffect.StandardBuff || effect.Effect == CAttackEffect.EAttackEffect.FirstTargetOnly)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (effect.Strength > 0)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.Damage, effect.Strength, self: false, isScalar: false, isBuff: true));
			}
			if (effect.AttackType == CAbility.EAttackType.Ranged || effect.Range > 0)
			{
				stringBuilder.Append(GenerateDescription(CAbility.EAbilityType.AddRange, effect.Range, self: false, isScalar: false, isBuff: true));
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	public static string GenerateDescription(List<CAbility> abilities, bool isForPreview = false, bool isFromItemPreview = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < abilities.Count; i++)
		{
			string value = GenerateDescription(abilities[i], isForPreview, isFromItemPreview);
			if (value.IsNOTNullOrEmpty())
			{
				if (i > 0 && stringBuilder.ToString() != string.Empty)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetPreviewEffectTextWithVars(CAbility ability, string overrideText = null)
	{
		string text = overrideText ?? ability.PreviewEffectText;
		if (LocalizationManager.TryGetTranslation(overrideText ?? ability.PreviewEffectText, out var translation))
		{
			text = translation;
		}
		List<string> list = ability.PreviewEffectTextParams();
		if (list == null)
		{
			return text;
		}
		string format = text;
		object[] args = list.ToArray();
		return string.Format(format, args);
	}

	public static string GenerateDescription(CAbility ability, bool isForPreview = false, bool isFromItemPreview = false, bool isForSelectedOption = false)
	{
		if (ability.GetMiscVar("DontPreviewDescription").GetBool() & isForPreview)
		{
			return string.Empty;
		}
		if (!isFromItemPreview && ability.GetAbilityBaseCard() != null && ability.GetAbilityBaseCard().CardType == CBaseCard.ECardType.Item && ability is CAbilityConsume { ShowPicker: not false })
		{
			return string.Empty;
		}
		string text = ability.GetMiscVar("PreviewEffectTextWhenSelected").GetString();
		if (isForSelectedOption && text.IsNOTNullOrEmpty())
		{
			return LocalizationManager.ReplaceTextVariables(GetPreviewEffectTextWithVars(ability, text));
		}
		if (ability.PreviewEffectText.IsNOTNullOrEmpty())
		{
			return GetPreviewEffectTextWithVars(ability);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (ability.AbilityType == CAbility.EAbilityType.Infuse)
		{
			foreach (IGrouping<ElementInfusionBoardManager.EElement, ElementInfusionBoardManager.EElement> item in from it in (ability as CAbilityInfuse).ElementsToInfuse
				group it by it)
			{
				int num = item.Count();
				stringBuilder.Append(GenerateLine(item.Key.ToString(), (num > 1) ? ("x" + num) : null));
			}
		}
		else if (ability.AbilityType == CAbility.EAbilityType.ConsumeElement)
		{
			foreach (ElementInfusionBoardManager.EElement item2 in (ability as CAbilityConsumeElement).ElementsToConsume)
			{
				stringBuilder.Append(GenerateLine(item2.ToString()));
			}
		}
		else if (ability is CAbilityPlaySong cAbilityPlaySong)
		{
			stringBuilder.Append(GenerateDescription(cAbilityPlaySong.Song));
		}
		else if (ability is CAbilityForgoActionsForCompanion cAbilityForgoActionsForCompanion)
		{
			stringBuilder.AppendLine(GenerateDescription(cAbilityForgoActionsForCompanion.ForgoTopActionAbility, isForPreview, isFromItemPreview));
			stringBuilder.Append(GenerateDescription(cAbilityForgoActionsForCompanion.ForgoBottomActionAbility, isForPreview, isFromItemPreview));
		}
		else if (ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
		{
			stringBuilder.Append(GenerateDescription(cAbilityAddActiveBonus.AddAbility, isForPreview, isFromItemPreview));
		}
		else if (ability is CAbilityChoose cAbilityChoose)
		{
			for (int num2 = 0; num2 < cAbilityChoose.ChooseAbilities.Count; num2++)
			{
				if (num2 > 0)
				{
					stringBuilder.Append("/");
				}
				stringBuilder.Append(GenerateDescription(cAbilityChoose.ChooseAbilities[num2].AbilityType));
			}
		}
		else if (ability is CAbilityAdjustInitiative)
		{
			if (ability.ActiveBonusData.IsToggleBonus)
			{
				stringBuilder.Append("+/-" + ability.Strength + " " + LocalizationManager.GetTranslation("Initiative"));
			}
		}
		else if (ability is CAbilityMuddle && ability.Range > 1 && ability.Range != 99)
		{
			string icon = GetIcon(ability.AbilityType);
			stringBuilder.AppendLine();
			stringBuilder.Append($"{GenerateLine(icon)} <sprite name=Range> {ability.Range}");
		}
		else if (ability is CAbilitySelectAndPlayCard)
		{
			string value = GenerateDescription(CAbility.EAbilityType.ImprovedShortRest);
			stringBuilder.Append(value);
		}
		else if (ability is CAbilitySelectAndMoveCard cAbilitySelectAndMoveCard)
		{
			string translation2;
			if (cAbilitySelectAndMoveCard.PilesToMoveFrom.Contains(CBaseCard.ECardPile.Discarded) && (cAbilitySelectAndMoveCard.PileToMoveTo == CBaseCard.ECardPile.Hand || cAbilitySelectAndMoveCard.PileToMoveTo == CBaseCard.ECardPile.Round || cAbilitySelectAndMoveCard.PileToMoveTo == CBaseCard.ECardPile.OwnedUnused) && LocalizationManager.TryGetTranslation("GUI_TOOLTIP_SELECT_RECOVER_CARD", out var translation))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(translation);
			}
			else if ((cAbilitySelectAndMoveCard.PileToMoveTo == CBaseCard.ECardPile.Discarded || cAbilitySelectAndMoveCard.PileToMoveTo == CBaseCard.ECardPile.Taken) && LocalizationManager.TryGetTranslation("GUI_TOOLTIP_SELECT_DISCARD_CARD_TITLE", out translation2))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(translation2);
			}
		}
		else if (ability is CAbilityTeleport2)
		{
			string value2 = GenerateDescription(CAbility.EAbilityType.Teleport, ability.Strength, ability.AbilityFilter != null && ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), ability.ActiveBonusData != null && (ability.ActiveBonusData.StrengthIsScalar || ability.ActiveBonusData.AbilityStrengthIsScalar));
			stringBuilder.Append(value2);
		}
		else if (ability is CAbilityAttack { ActiveBonusData: var activeBonusData } cAbilityAttack && activeBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffAttack) && cAbilityAttack.AttackEffects != null && cAbilityAttack.AttackEffects.Count > 0)
		{
			foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
			{
				string value3 = GenerateDescription(attackEffect);
				if (!string.IsNullOrEmpty(value3))
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(value3);
				}
			}
		}
		else if (ability is CAbilityControlActor cAbilityControlActor)
		{
			string value4 = GenerateDescription(cAbilityControlActor.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Enemy) ? ability.AbilityType : CAbility.EAbilityType.None, ability.Strength, ability.AbilityFilter != null && ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), ability.ActiveBonusData != null && (ability.ActiveBonusData.StrengthIsScalar || ability.ActiveBonusData.AbilityStrengthIsScalar), ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA);
			stringBuilder.Append(value4);
		}
		else
		{
			string value5 = GenerateDescription(ability.AbilityType, ability.Strength, ability.AbilityFilter != null && ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true), ability.ActiveBonusData != null && (ability.ActiveBonusData.StrengthIsScalar || ability.ActiveBonusData.AbilityStrengthIsScalar), ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA);
			stringBuilder.Append(value5);
		}
		if (ability is CAbilityMove { Jump: not false })
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(GenerateLine("Jump"));
		}
		else if (ability.Range > 1 && ability.Range != 99 && !(ability is CAbilityMuddle))
		{
			if (ability.ActiveBonusData == null || !ability.ActiveBonusData.IsToggleBonus || (ability.ActiveBonusData.IsToggleBonus && ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffRange)))
			{
				string value6 = GenerateDescription(CAbility.EAbilityType.AddRange, ability.Range, self: false, isScalar: false, ability.ActiveBonusData != null && ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA);
				if (value6.IsNOTNullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append(value6);
				}
			}
		}
		else if (ability is CAbilityRetaliate { RetaliateRange: >1, Range: not 99 } cAbilityRetaliate)
		{
			string value7 = GenerateDescription(CAbility.EAbilityType.AddRange, cAbilityRetaliate.RetaliateRange, self: false, isScalar: false, isBuff: true);
			if (value7.IsNOTNullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(value7);
			}
		}
		if (ability is CAbilityAttack { MiscAbilityData: var miscAbilityData } cAbilityAttack2)
		{
			if (miscAbilityData != null && miscAbilityData.AttackHasAdvantage == true)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(CCondition.EPositiveCondition.Advantage.ToString())));
			}
			AbilityData.MiscAbilityData miscAbilityData2 = cAbilityAttack2.MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.AttackHasDisadvantage == true)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(CCondition.ENegativeCondition.Disadvantage.ToString())));
			}
		}
		foreach (CAbility withChildAbility in CAbilityCard.GetWithChildAbilities(ability, source: false, subAbilities: true, augments: true, abilityData: true, consumes: true, controlActor: true, merged: true, doom: true, performFromShadow: true, chooseAbility: false, addActiveBonus: false))
		{
			string value8 = GenerateDescription(withChildAbility, isForPreview, isFromItemPreview);
			if (value8.IsNOTNullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(value8);
			}
		}
		if (ability.ActiveBonusData?.AbilityData != null && ability != ability.ActiveBonusData.AbilityData)
		{
			string value9 = GenerateDescription(ability.ActiveBonusData.AbilityData, isForPreview, isFromItemPreview);
			if (value9.IsNOTNullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(value9);
			}
		}
		if (ability.NegativeConditions != null)
		{
			foreach (KeyValuePair<CCondition.ENegativeCondition, CAbility> negativeCondition in ability.NegativeConditions)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(negativeCondition.Key.ToString())));
			}
		}
		if (ability.PositiveConditions != null)
		{
			foreach (KeyValuePair<CCondition.EPositiveCondition, CAbility> positiveCondition in ability.PositiveConditions)
			{
				stringBuilder.Append(GenerateLine(Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(positiveCondition.Key.ToString())));
			}
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CSong song)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < song.SongEffects.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(song.SongEffects[i]));
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(CSong.SongEffect songEffect)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (songEffect.Abilities != null)
		{
			for (int i = 0; i < songEffect.Abilities.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateDescription(songEffect.Abilities[i]));
			}
		}
		if (songEffect.AbilityOverrides != null)
		{
			for (int j = 0; j < songEffect.AbilityOverrides.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(GenerateDescription(songEffect.AbilityOverrides[j], songEffect.AbilityType));
			}
		}
		return stringBuilder.ToString();
	}

	private static string GenerateDescription(List<CAbilityOverride> abilities)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < abilities.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(GenerateDescription(abilities[i]));
		}
		return stringBuilder.ToString();
	}

	private string ConvertoToIconText(string icon)
	{
		string text = icon.SplitCamelCase().ToTitleCase().Replace(" ", "_");
		if (iconSpriteAsset.GetSpriteIndexFromName(text) < 0)
		{
			text = "AA_" + text;
		}
		if (iconSpriteAsset.GetSpriteIndexFromName(text) < 0)
		{
			text = string.Empty;
		}
		return text;
	}

	public static string GetIcon(CAbility.EAbilityType type)
	{
		string abilityPreviewEffectIconName = UIInfoTools.Instance.GetAbilityPreviewEffectIconName(type);
		if (abilityPreviewEffectIconName.IsNOTNullOrEmpty())
		{
			return abilityPreviewEffectIconName;
		}
		if (Choreographer.TargetingAbilitySpritesToSkip.Contains(type) || Choreographer.TargetingAbilityTypesToSkip.Contains(type))
		{
			return string.Empty;
		}
		return Singleton<PreviewEffectGenerator>.Instance.ConvertoToIconText(type.ToString());
	}

	public static bool TryGetAbilityOverrideIcon(CAbility ability, out Sprite icon)
	{
		icon = null;
		string text = ability?.GetMiscVar("OverrideIcon").GetString();
		if (text.IsNOTNullOrEmpty())
		{
			icon = UIInfoTools.Instance.GetActiveAbilityIcon(text);
		}
		return icon != null;
	}
}

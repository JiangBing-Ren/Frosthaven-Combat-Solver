using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Utils;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CardLayoutRow
{
	public delegate string GetStatValuesHandler(CAbility ability, int amount, string subtype, CAbilityOverride abilityOverride, int? overrideLevel = null);

	public delegate string DecorateAbilityTextHandler(AbilityPropertyTextParams textParams);

	public record PropertyLookupContext
	{
		public CActor Actor;

		public CActor TargetingActor;

		public CClass Class;

		public float? summonBaseHealthModifier = 1f;

		public int? OverrideLevel;
	}

	public class AbilityPropertyTextParams
	{
		public CAbility Ability;

		public int Enhancement;

		public string Value;
	}

	public const string SUBTYPE_ATTACK = "Attack";

	public const string SUBTYPE_MOVE = "Move";

	public const string SUBTYPE_HEAL = "Heal";

	public const string SUBTYPE_RANGE = "Range";

	public const string SUBTYPE_HEALTH = "Health";

	public const string SUBTYPE_TARGET = "Target";

	public const string SUBTYPE_PIERCE = "Pierce";

	public const string SUBTYPE_STRENGTH = "Strength";

	public const string SUBTYPE_RETALIATE_RANGE = "RetaliateRange";

	public const string SUBTYPE_TOKENS = "Tokens";

	public const string TYPE_INNATE = "INNATE";

	public const string TYPE_SHIELD = "Shield";

	public const string TYPE_PUSH = "Push";

	public const string SUBTYPE_RETALIATE = "Retaliate";

	public const string TYPE_TRAP_DAMAGE = "TrapDamage";

	public const string TYPE_PARTY_SIZE = "PartySize";

	public const string TYPE_HAZARDOUS_TERRAIN = "HazardousTerrainDamage";

	public const string SUBTYPE_HALF_ROUND_UP = "HalfRoundUp";

	public const string SUBTYPE_HALF_ROUND_DOWN = "HalfRoundDown";

	public const string SUBTYPE_MULTIPLY = "Multiply";

	public const string SUBTYPE_ADD = "Add";

	public const string MARK_X = "X";

	public const string MARK_NONE = "-";

	public const char PropertyKeySymbol = '*';

	public const char EnhancementSlotsKeySymbol = '@';

	public const char AreaEffectKeySymbol = '£';

	public const char LookupKeySymbol = '$';

	public const char ActivePropertyKeySymbol = '^';

	public const char SubtypeSplitKeySymbol = '.';

	public IEnumerable<CAbility> linkedAbilities;

	public IEnumerable<CAbilityOverride> linkedOverrides;

	public string originalRowText;

	private string _localisedRowText;

	private string _cardName;

	private bool _calculateIsBasedOnX;

	public List<CEnhancement> AbilityEnhancements { get; private set; }

	public EEnhancementLine EnhancementLine { get; private set; }

	public bool IsArea { get; private set; }

	public bool IsBasedOnX { get; private set; }

	public string AbilityName { get; private set; }

	public string AbilityNameLookupText { get; private set; }

	public CardLayoutRow(string rowText, string cardName, IEnumerable<CAbility> linked, IEnumerable<CAbilityOverride> overrides = null, bool calculateIsBasedOnX = true)
	{
		originalRowText = rowText;
		linkedAbilities = linked;
		linkedOverrides = overrides;
		_cardName = cardName;
		_calculateIsBasedOnX = calculateIsBasedOnX;
		AbilityEnhancements = new List<CEnhancement>();
		if (rowText.Contains('£'))
		{
			IsArea = true;
		}
		else
		{
			IsArea = false;
		}
		_localisedRowText = string.Empty;
		EnhancementLine = EEnhancementLine.Mainline;
	}

	public void SetLocalisedText(string localisedText)
	{
		_localisedRowText = localisedText;
	}

	private string GetLinkedAbilityAreaEffectString(CAbility ability)
	{
		AbilityName = ability.Name;
		if (ability.AreaEffectLayoutOverrideYMLString != null && ability.AreaEffectLayoutOverrideYMLString.Length > 0)
		{
			if (AbilityEnhancements.Count == 0)
			{
				AbilityEnhancements.AddRange(ability.AbilityEnhancements);
			}
			return ability.AreaEffectLayoutOverrideYMLString;
		}
		if (ability.AreaEffectYMLString != null && ability.AreaEffectYMLString.Length > 0)
		{
			if (AbilityEnhancements.Count == 0)
			{
				AbilityEnhancements.AddRange(ability.AbilityEnhancements);
			}
			return ability.AreaEffectYMLString;
		}
		return null;
	}

	public string Text()
	{
		string empty = string.Empty;
		empty = ((_localisedRowText.Length <= 0) ? originalRowText : _localisedRowText);
		if (IsArea)
		{
			AbilityNameLookupText = "ATTACK_AREA";
			EnhancementLine = EEnhancementLine.AreaHex;
			string text = null;
			string text2 = null;
			string text3 = null;
			if (originalRowText.Contains('.'))
			{
				string[] array = GetKey(originalRowText, '£').Split('.');
				if (array.Length > 1)
				{
					text2 = array[0];
				}
			}
			if (linkedAbilities != null)
			{
				foreach (CAbility linkedAbility in linkedAbilities)
				{
					text = GetLinkedAbilityAreaEffectString(linkedAbility);
					if (text != null)
					{
						text3 = linkedAbility?.Name;
						break;
					}
					if (linkedAbility is CAbilityControlActor cAbilityControlActor)
					{
						foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
						{
							text = GetLinkedAbilityAreaEffectString(controlAbility);
							if (text != null)
							{
								text3 = controlAbility?.Name;
								break;
							}
						}
					}
					if (linkedAbility.ConditionalOverrides == null)
					{
						continue;
					}
					foreach (CConditionalOverride conditionalOverride in linkedAbility.ConditionalOverrides)
					{
						foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
						{
							if (text2 == null || !(abilityOverride.AbilityName != text2))
							{
								if (abilityOverride.AreaEffectLayoutOverrideYMLString != null && abilityOverride.AreaEffectLayoutOverrideYMLString.Length > 0)
								{
									text = abilityOverride.AreaEffectLayoutOverrideYMLString;
								}
								else if (abilityOverride.AreaEffectYMLString != null && abilityOverride.AreaEffectYMLString.Length > 0)
								{
									text = abilityOverride.AreaEffectYMLString;
								}
								if (text != null)
								{
									break;
								}
							}
						}
					}
				}
			}
			if (linkedOverrides != null && (text2 == null || text2 != text3))
			{
				foreach (CAbilityOverride linkedOverride in linkedOverrides)
				{
					if (text2 == null || !(linkedOverride.AbilityName != text2))
					{
						if (linkedOverride.AreaEffectLayoutOverrideYMLString != null && linkedOverride.AreaEffectLayoutOverrideYMLString.Length > 0)
						{
							text = linkedOverride.AreaEffectLayoutOverrideYMLString;
						}
						else if (linkedOverride.AreaEffectYMLString != null && linkedOverride.AreaEffectYMLString.Length > 0)
						{
							text = linkedOverride.AreaEffectYMLString;
						}
						if (text != null)
						{
							break;
						}
					}
				}
			}
			if (text == null)
			{
				GetKey(empty, '£');
			}
			return text;
		}
		if (empty.Contains('$'))
		{
			AbilityNameLookupText = GetKey(empty, '$');
		}
		empty = PropertyLookup(empty);
		if (empty.Contains('@'))
		{
			empty = ReplaceKey(empty, '@', "");
		}
		return empty;
	}

	public string PropertyLookup(string text, PropertyLookupContext context = null, GetStatValuesHandler getStatValues = null, DecorateAbilityTextHandler decorate = null, bool throwErrorIfNotFound = true)
	{
		while (text.Contains('*'.ToString()))
		{
			string text2 = GetKey(text, '*');
			string defaultValue = "0";
			string subtype = string.Empty;
			if (text2.Contains(","))
			{
				string[] array = text2.Split(',');
				text2 = array[0];
				defaultValue = array[1];
			}
			if (text2.Contains('.'))
			{
				string[] array2 = text2.Split('.', 2);
				text2 = array2[0];
				subtype = array2[1];
			}
			CAbility ability = FindAbilityByName(text2, linkedAbilities, _cardName, throwErrorIfNotFound);
			GetAbilityAmount(text2, ability, defaultValue, subtype, out var enhancement, out var stringVal, null, getStatValues, context, throwErrorIfNotFound);
			if (decorate != null)
			{
				AbilityPropertyTextParams textParams = new AbilityPropertyTextParams
				{
					Ability = ability,
					Enhancement = enhancement,
					Value = stringVal
				};
				stringVal = decorate?.Invoke(textParams);
			}
			text = ReplaceKey(text, '*', stringVal);
		}
		return text;
	}

	public static CAbility FindAbilityByName(string abilityName, IEnumerable<CAbility> linkedAbilities, string cardName = "", bool throwError = true)
	{
		foreach (CAbility linkedAbility in linkedAbilities)
		{
			CAbility cAbility = linkedAbility.FindAbility(abilityName);
			if (cAbility != null)
			{
				return cAbility;
			}
		}
		if (throwError)
		{
			Log.ErrorWithReport(cardName + ": Unable to find ability " + abilityName);
		}
		return null;
	}

	public void SetEnhancements()
	{
		if (AbilityEnhancements.Count != 0)
		{
			return;
		}
		string text = PropertyLookup(originalRowText);
		if (text.Contains('@'.ToString()))
		{
			string text2 = GetKey(text, '@');
			if (text2.Contains('.'))
			{
				text2 = text2.Split('.')[0];
			}
			CAbility cAbility = FindAbilityByName(text2, linkedAbilities);
			if (cAbility != null)
			{
				AbilityEnhancements.AddRange(cAbility.AbilityEnhancements);
			}
			else
			{
				Log.ErrorWithReport(_cardName + ": Unable to find ability with name " + text2 + " for enhancements");
			}
		}
	}

	public int? GetAbilityAmount(string abilityName, CAbility ability, string defaultValue, string subtype, out int enhancement, out string stringVal, CAbilityOverride abilityOverride = null, GetStatValuesHandler getStatValues = null, PropertyLookupContext context = null, bool throwErrorIfNotFound = true)
	{
		float baseHealthModifier = context?.summonBaseHealthModifier ?? 1f;
		CActor cActor = context?.TargetingActor;
		int? num = ((cActor != null) ? new int?(cActor.Level) : context?.OverrideLevel);
		CClass cClass = cActor?.Class ?? context?.Class;
		if (abilityName.Equals("TrapDamage", StringComparison.InvariantCultureIgnoreCase))
		{
			enhancement = 0;
			int trapDamage = ScenarioManager.TrapDamage;
			stringVal = trapDamage.ToString();
			return trapDamage;
		}
		if (abilityName.Equals("PartySize", StringComparison.InvariantCultureIgnoreCase))
		{
			enhancement = 0;
			return GetAmountBasedOnPartySize(subtype, out stringVal);
		}
		if (abilityName.Equals("HazardousTerrainDamage", StringComparison.InvariantCultureIgnoreCase))
		{
			enhancement = 0;
			int hazardousTerrainDamage = ScenarioManager.HazardousTerrainDamage;
			stringVal = hazardousTerrainDamage.ToString();
			return hazardousTerrainDamage;
		}
		if (abilityName.Equals("INNATE", StringComparison.InvariantCultureIgnoreCase))
		{
			enhancement = 0;
			stringVal = string.Empty;
			if (context == null)
			{
				return 0;
			}
			if (!(cClass is CMonsterClass cMonsterClass))
			{
				return 0;
			}
			int num2 = 0;
			switch (subtype)
			{
			case "Pierce":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).Pierce : cMonsterClass.Pierce);
				stringVal = num2.ToString();
				return num2;
			case "Target":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).Target : cMonsterClass.Target);
				stringVal = num2.ToString();
				return num2;
			case "Shield":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).Shield : cMonsterClass.Shield);
				stringVal = num2.ToString();
				return num2;
			case "Push":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).Push : cMonsterClass.Push);
				stringVal = num2.ToString();
				return num2;
			case "RetaliateRange":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).RetaliateRange : cMonsterClass.RetaliateRange);
				stringVal = num2.ToString();
				return num2;
			case "Retaliate":
				num2 = (num.HasValue ? cMonsterClass.GetMonsterStatsData(num.Value).Retaliate : cMonsterClass.Retaliate);
				stringVal = num2.ToString();
				return num2;
			}
		}
		enhancement = 0;
		int result = 0;
		bool flag = !string.IsNullOrEmpty(subtype);
		if (ability != null)
		{
			bool flag2 = ability.StatIsBasedOnXEntries != null && ability.StatIsBasedOnXEntries.Any();
			if (ability.AbilityType == CAbility.EAbilityType.AddActiveBonus)
			{
				ability = (ability as CAbilityAddActiveBonus).AddAbility;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Loot)
			{
				result = ability.Range;
				result = GetOverrideValue(result, "Range", abilityOverride);
				stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
				return result;
			}
			if (ability is CAbilityDamage cAbilityDamage && !flag)
			{
				result = cAbilityDamage.Strength;
				stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
				return result;
			}
			if (ability is CAbilityTrap cAbilityTrap && !flag)
			{
				int num3 = cAbilityTrap.TrapData.Heal;
				int num4 = cAbilityTrap.TrapData.Damage;
				if (abilityOverride != null)
				{
					num3 += abilityOverride.TrapData.Heal;
					num4 += abilityOverride.TrapData.Damage;
				}
				foreach (CEnhancement abilityEnhancement in ability.AbilityEnhancements)
				{
					switch (abilityEnhancement.Enhancement)
					{
					case EEnhancement.PlusDamage:
						num4++;
						enhancement++;
						break;
					case EEnhancement.PlusHeal:
						num3++;
						enhancement++;
						break;
					}
				}
				result = ((num3 > 0) ? num3 : num4);
				stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
				return result;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Push && ability.IsSubAbility)
			{
				EnhancementLine = EEnhancementLine.Push;
				result = 0;
				if (ability is CAbilityPush { Strength: var num5 })
				{
					foreach (CEnhancement item in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.Push && w.Enhancement == EEnhancement.PlusPush))
					{
						_ = item;
						num5++;
						enhancement++;
					}
					result = GetOverrideValue(num5, "Strength", abilityOverride);
				}
				stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
				return result;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Pull && ability.IsSubAbility)
			{
				EnhancementLine = EEnhancementLine.Pull;
				result = 0;
				if (ability is CAbilityPull { Strength: var num6 })
				{
					foreach (CEnhancement item2 in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.Pull && w.Enhancement == EEnhancement.PlusPull))
					{
						_ = item2;
						num6++;
						enhancement++;
					}
					result = GetOverrideValue(num6, "Strength", abilityOverride);
				}
				result = GetOverrideValue(result, "Strength", abilityOverride);
				stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
				return result;
			}
			if (ability.AbilityType == CAbility.EAbilityType.Summon)
			{
				CAbilitySummon cAbilitySummon = (CAbilitySummon)ability;
				HeroSummonYMLData selectedSummonYMLData = cAbilitySummon.SelectedSummonYMLData;
				if (!flag)
				{
					stringVal = "$Summon$ " + cAbilitySummon.SelectedSummonYMLData.LocKey;
					return null;
				}
				switch (subtype)
				{
				case "Attack":
					enhancement = (ability.AbilityEnhancements.Any((CEnhancement e) => e.Enhancement == EEnhancement.SummonAttack) ? 1 : 0);
					result = selectedSummonYMLData.Attack;
					break;
				case "Move":
					enhancement = (ability.AbilityEnhancements.Any((CEnhancement e) => e.Enhancement == EEnhancement.SummonMove) ? 1 : 0);
					result = selectedSummonYMLData.Move;
					break;
				case "Range":
					enhancement = (ability.AbilityEnhancements.Any((CEnhancement e) => e.Enhancement == EEnhancement.SummonRange) ? 1 : 0);
					result = selectedSummonYMLData.Range;
					break;
				case "Health":
					enhancement = (ability.AbilityEnhancements.Any((CEnhancement e) => e.Enhancement == EEnhancement.SummonHP) ? 1 : 0);
					result = selectedSummonYMLData.GetHealth(0, baseHealthModifier);
					break;
				}
				string text;
				switch (result)
				{
				case -1:
					text = "X";
					break;
				case 0:
					text = "-";
					break;
				case 1:
					if (subtype == "Range")
					{
						text = "-";
						break;
					}
					goto default;
				default:
					text = (result + enhancement).ToString();
					break;
				}
				stringVal = text;
				if (!flag2)
				{
					return null;
				}
			}
			if (!Enum.TryParse<CAbility.EAbilityStatType>(subtype, out var abilityStatType) && subtype == "Target")
			{
				abilityStatType = CAbility.EAbilityStatType.NumberOfTargets;
			}
			if (flag2 && _calculateIsBasedOnX && (subtype.IsNullOrEmpty() || abilityStatType == CAbility.EAbilityStatType.None))
			{
				abilityStatType = CAbility.EAbilityStatType.Strength;
			}
			if (flag2 && abilityStatType != CAbility.EAbilityStatType.None)
			{
				AbilityData.StatIsBasedOnXData statIsBasedOnXData = ability.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == abilityStatType);
				if (statIsBasedOnXData != null)
				{
					IsBasedOnX = true;
					int num7 = ((_calculateIsBasedOnX && ScenarioManager.CurrentScenarioState != null) ? ability.GetStatIsBasedOnXValue(ability.TargetingActor ?? cActor, statIsBasedOnXData, ability.AbilityFilter) : 0);
					if (statIsBasedOnXData.AddTo)
					{
						int num8;
						switch (abilityStatType)
						{
						case CAbility.EAbilityStatType.Strength:
							if (ability.StrengthIsBase)
							{
								num8 = ability.Strength;
								break;
							}
							goto default;
						case CAbility.EAbilityStatType.Range:
							if (ability.RangeIsBase)
							{
								num8 = ability.Range;
								break;
							}
							goto default;
						case CAbility.EAbilityStatType.NumberOfTargets:
							num8 = ability.NumberTargets;
							break;
						default:
							num8 = 0;
							break;
						}
						result = num8;
						result += num7;
					}
					else
					{
						result = num7;
					}
					stringVal = getStatValues?.Invoke(ability, result, subtype, abilityOverride) ?? result.ToString();
					return result;
				}
			}
			int num9 = 0;
			switch (subtype)
			{
			case "Range":
				num9 = ability.Range;
				EnhancementLine = EEnhancementLine.Range;
				foreach (CEnhancement item3 in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.Range && w.Enhancement == EEnhancement.PlusRange))
				{
					_ = item3;
					num9++;
					enhancement++;
				}
				num9 = GetOverrideValue(num9, "Range", abilityOverride);
				break;
			case "Target":
				num9 = ability.NumberTargets;
				EnhancementLine = EEnhancementLine.Targets;
				foreach (CEnhancement item4 in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.Targets && w.Enhancement == EEnhancement.PlusTarget))
				{
					_ = item4;
					num9++;
					enhancement++;
				}
				num9 = GetOverrideValue(num9, "Target", abilityOverride);
				break;
			case "Pierce":
				EnhancementLine = EEnhancementLine.Pierce;
				if (!(ability is CAbilityAttack { Pierce: var num11 } cAbilityAttack))
				{
					break;
				}
				foreach (CEnhancement item5 in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.Pierce && w.Enhancement == EEnhancement.PlusPierce))
				{
					_ = item5;
					num11++;
					enhancement++;
				}
				num9 = GetOverrideValue(num11, "Pierce", abilityOverride);
				foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
				{
					num9 += attackEffect.Pierce;
				}
				break;
			case "RetaliateRange":
				EnhancementLine = EEnhancementLine.RetaliateRange;
				if (!(ability is CAbilityRetaliate { RetaliateRange: var num10 }))
				{
					break;
				}
				foreach (CEnhancement item6 in ability.AbilityEnhancements.Where((CEnhancement w) => w.EnhancementLine == EEnhancementLine.RetaliateRange && w.Enhancement == EEnhancement.PlusRetaliateRange))
				{
					_ = item6;
					num10++;
					enhancement++;
				}
				num9 = GetOverrideValue(num10, "RetaliateRange", abilityOverride);
				break;
			case "Tokens":
				num9 = ability.Tokens;
				num9 = GetOverrideValue(num9, "Tokens", abilityOverride);
				break;
			default:
				if (ability.ConditionalOverrides.Any((CConditionalOverride co) => co.Name == subtype))
				{
					CAbilityOverride abilityOverride2 = ability.ConditionalOverrides.First((CConditionalOverride co) => co.Name == subtype).AbilityOverrides.FirstOrDefault();
					return GetAbilityAmount(abilityName, ability, defaultValue, "", out enhancement, out stringVal, abilityOverride2, getStatValues, context).GetValueOrDefault();
				}
				if (ability is CAbilityChooseAbility cAbilityChooseAbility)
				{
					string[] splitAbilities = subtype.Split('.', 2);
					CAbility cAbility = cAbilityChooseAbility.ChooseAbilities.FirstOrDefault((CAbility co) => co.Name == splitAbilities.FirstOrDefault());
					if (cAbility != null)
					{
						return GetAbilityAmount(abilityName, cAbility, defaultValue, splitAbilities.ElementAtOrDefault(1), out enhancement, out stringVal, null, getStatValues, context).GetValueOrDefault();
					}
				}
				if (ability is CAbilityConsume cAbilityConsume)
				{
					string consumeName = subtype;
					if (subtype.Contains('.'))
					{
						string[] source = subtype.Split('.', 2);
						consumeName = source.ElementAtOrDefault(0);
						subtype = source.ElementAtOrDefault(1);
					}
					else
					{
						subtype = string.Empty;
					}
					CAbility cAbility2 = cAbilityConsume.Consumes?.SelectMany((AbilityConsume c) => c.ConsumeData.AugmentationOps.Select((CActionAugmentationOp op) => op.Ability)).FirstOrDefault((CAbility ab) => ab.Name == consumeName);
					if (cAbility2 == null)
					{
						stringVal = defaultValue;
						if (!int.TryParse(defaultValue, out result))
						{
							return null;
						}
						return result;
					}
					return GetAbilityAmount(abilityName, cAbility2, defaultValue, subtype, out enhancement, out stringVal, null, getStatValues, context);
				}
				if (!string.IsNullOrEmpty(subtype))
				{
					string overrideName = subtype;
					if (subtype.Contains('.'))
					{
						string[] array = subtype.Split('.');
						overrideName = array[0];
						subtype = array[1];
					}
					else
					{
						subtype = string.Empty;
					}
					CAbilityOverride cAbilityOverride = ability.ConditionalOverrides.FirstOrDefault((CConditionalOverride co) => co.Name == overrideName)?.AbilityOverrides.FirstOrDefault() ?? linkedOverrides?.FirstOrDefault((CAbilityOverride co) => co.AbilityName == overrideName);
					if (cAbilityOverride != null)
					{
						return GetAbilityAmount(abilityName, ability, defaultValue, subtype, out enhancement, out stringVal, cAbilityOverride, getStatValues, context);
					}
					CAbility cAbility3 = ability.SubAbilities?.FirstOrDefault((CAbility a) => a.Name == overrideName);
					if (cAbility3 != null)
					{
						return GetAbilityAmount(cAbility3.Name, cAbility3, defaultValue, subtype, out enhancement, out stringVal, cAbilityOverride, getStatValues, context);
					}
				}
				EnhancementLine = EEnhancementLine.Mainline;
				num9 = ability.Strength;
				foreach (CEnhancement abilityEnhancement2 in ability.AbilityEnhancements)
				{
					switch (abilityEnhancement2.Enhancement)
					{
					case EEnhancement.PlusMove:
					case EEnhancement.PlusAttack:
					case EEnhancement.PlusShield:
					case EEnhancement.PlusPush:
					case EEnhancement.PlusPull:
					case EEnhancement.PlusRetaliate:
					case EEnhancement.PlusHeal:
					case EEnhancement.PlusDamage:
					case EEnhancement.PlusTeleport:
						num9++;
						enhancement++;
						break;
					}
				}
				num9 = GetOverrideValue(num9, "Strength", abilityOverride);
				stringVal = getStatValues?.Invoke(ability, num9, subtype, abilityOverride, context?.OverrideLevel) ?? num9.ToString();
				return num9;
			}
			stringVal = getStatValues?.Invoke(ability, num9, subtype, abilityOverride) ?? num9.ToString();
			return num9;
		}
		if (throwErrorIfNotFound)
		{
			Log.ErrorWithReport(_cardName + ": Unable to find ability " + abilityName + ".  Using the default value to set the strength for this ability");
		}
		stringVal = defaultValue;
		if (!int.TryParse(defaultValue, out result))
		{
			return null;
		}
		return result;
	}

	private static int GetOverrideValue(int current, string subtype, CAbilityOverride abilityOverride)
	{
		if (abilityOverride == null)
		{
			return current;
		}
		return subtype switch
		{
			"Strength" => GetOverrideValue(current, abilityOverride.Strength, abilityOverride.StrengthIsBase == true), 
			"Range" => GetOverrideValue(current, abilityOverride.Range, abilityOverride.RangeIsBase == true), 
			"Target" => GetOverrideValue(current, abilityOverride.NumberOfTargets, abilityOverride.TargetIsBase == true), 
			"Pierce" => GetOverrideValue(current, abilityOverride.Pierce, isBase: false), 
			"RetaliateRange" => GetOverrideValue(current, abilityOverride.RetaliateRange, isBase: false), 
			"Tokens" => GetOverrideValue(current, abilityOverride.Tokens, isBase: false), 
			_ => current, 
		};
	}

	private static int GetOverrideValue(int current, int? overrideVal, bool isBase)
	{
		if (!overrideVal.HasValue)
		{
			return current;
		}
		if (!isBase)
		{
			return current + overrideVal.Value;
		}
		return overrideVal.Value;
	}

	private static int? GetAmountBasedOnPartySize(string subtype, out string stringVal)
	{
		stringVal = "C";
		int num = ScenarioManager.CurrentScenarioState?.Players.Count ?? 0;
		if (string.IsNullOrEmpty(subtype))
		{
			stringVal = ((num == 0) ? stringVal : num.ToString());
			return num;
		}
		string[] array = subtype.Split('.');
		if (num == 0)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text switch
				{
					"HalfRoundDown" => "/2D", 
					"HalfRoundUp" => "/2U", 
					"Multiply" => "x", 
					"Add" => "+", 
					_ => text, 
				};
				if (text2[0].Equals('-') && stringVal.Last() == '+')
				{
					stringVal = stringVal.Remove(stringVal.Length - 1);
				}
				else if (text2[0] == '/')
				{
					stringVal = stringVal.Insert(0, "(");
					stringVal += ")";
				}
				stringVal += text2;
			}
		}
		else
		{
			bool flag = false;
			bool flag2 = false;
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				switch (text3)
				{
				case "HalfRoundDown":
					num = (int)MathF.Floor((float)num * 0.5f);
					continue;
				case "HalfRoundUp":
					num = (int)MathF.Ceiling((float)num * 0.5f);
					continue;
				case "Multiply":
					flag = true;
					flag2 = false;
					continue;
				case "Add":
					flag = false;
					flag2 = true;
					continue;
				}
				int num2 = int.Parse(text3);
				if (flag)
				{
					num *= num2;
				}
				if (flag2)
				{
					num += num2;
				}
				flag = false;
				flag2 = false;
			}
			stringVal = num.ToString();
		}
		return num;
	}

	private string GetEnhancementIcons(string abilityName, CAbility ability, string subtype)
	{
		string text = string.Empty;
		AbilityName = abilityName;
		if (ability != null)
		{
			switch (subtype)
			{
			case "Range":
				foreach (EEnhancement item in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Range
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item.ToString() + "> ";
				}
				break;
			case "Target":
				foreach (EEnhancement item2 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Targets
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item2.ToString() + "> ";
				}
				break;
			case "Pierce":
				foreach (EEnhancement item3 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Pierce
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item3.ToString() + "> ";
				}
				break;
			default:
				foreach (EEnhancement item4 in from s in ability.AbilityEnhancements
					where s.EnhancementLine == EEnhancementLine.Mainline
					select s.Enhancement)
				{
					text = text + "<sprite name=" + item4.ToString() + "> ";
				}
				break;
			}
			return text.TrimEnd();
		}
		Log.ErrorWithReport(_cardName + ": Unable to find ability " + abilityName + ".  Unable to set enhancement icons.");
		return string.Empty;
	}

	public static string GetKey(string word, char symbol)
	{
		return word.Substring(word.IndexOf(symbol) + 1, word.Substring(word.IndexOf(symbol) + 1).IndexOf(symbol));
	}

	public static string ReplaceKey(string text, char symbol, string replaceWith)
	{
		int num = text.IndexOf(symbol);
		return text.Remove(num, text.Substring(num + 1).IndexOf(symbol) + 2).Insert(num, replaceWith);
	}

	private static string RemoveKey(string word, char symbol)
	{
		return word.Remove(word.IndexOf(symbol), word.Substring(word.IndexOf(symbol) + 1).IndexOf(symbol) + 2);
	}
}

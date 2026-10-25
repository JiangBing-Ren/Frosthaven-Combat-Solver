using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierCard : CBaseCard
{
	public const string FONT_ATTACKMODSDOUBLE = "<b><color=#D92727>{0}</color></b>";

	public const string FONT_ATTACKMODSPOSITIVE = "<b><color=#FFA414>{0}</color></b>";

	public const string FONT_ATTACKMODSNEGATIVE = "<b><color=#A050E0>{0}</color></b>";

	private AttackModifierYMLData m_YMLData;

	public AttackModifierYMLData YMLData
	{
		get
		{
			if (m_YMLData == null)
			{
				m_YMLData = ScenarioRuleClient.SRLYML.AttackModifiers.FirstOrDefault((AttackModifierYMLData s) => s.Name == Name).Copy();
			}
			return m_YMLData;
		}
		set
		{
			m_YMLData = value;
		}
	}

	public bool Infuse
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityInfuse);
			}
			return false;
		}
	}

	public bool Consume
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityConsume);
			}
			return false;
		}
	}

	public List<ElementInfusionBoardManager.EElement> InfuseElements
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return new List<ElementInfusionBoardManager.EElement>();
			}
			return YMLData.Abilities.Where((CAbility w) => w is CAbilityInfuse).SelectMany((CAbility s) => (s as CAbilityInfuse).ElementsToInfuse).ToList();
		}
	}

	public List<ElementInfusionBoardManager.EElement> ConsumeElements
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return new List<ElementInfusionBoardManager.EElement>();
			}
			return YMLData.Abilities.Where((CAbility w) => w is CAbilityConsume).SelectMany((CAbility s) => (s as CAbilityConsume).Consumes).SelectMany((AbilityConsume c) => c.ConsumeData.Elements)
				.ToList();
		}
	}

	public bool Shield
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityShield);
			}
			return false;
		}
	}

	public bool Kill
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityKill);
			}
			return false;
		}
	}

	public bool Brittle
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityBrittle);
			}
			return false;
		}
	}

	public bool Pierce
	{
		get
		{
			if (YMLData.Overrides != null)
			{
				return YMLData.Overrides.Any((CAbilityOverride a) => a.Pierce.HasValue && a.Pierce > 0);
			}
			return false;
		}
	}

	public bool Heal
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => (a is CAbilityHeal && !a.AbilityFilter.FilterAlly()) || YMLData.Abilities.Any((CAbility cAbility) => cAbility is CAbilityTrap cAbilityTrap && cAbilityTrap.TrapData.Heal > 0));
			}
			return false;
		}
	}

	public bool HealAlly
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && a.AbilityFilter.FilterAlly());
			}
			return false;
		}
	}

	public int HealAmount
	{
		get
		{
			if (YMLData.Abilities == null)
			{
				return 0;
			}
			return YMLData.Abilities.Where((CAbility a) => a is CAbilityHeal).Sum((CAbility a) => a.Strength);
		}
	}

	public bool Damage
	{
		get
		{
			if (!YMLData.DamageIfTargetMoves.HasValue || YMLData.DamageIfTargetMoves.Value <= 0)
			{
				if (YMLData.Abilities == null || !YMLData.Abilities.Any((CAbility a) => a is CAbilityDamage))
				{
					if (YMLData.Abilities != null)
					{
						return YMLData.Abilities.Any((CAbility a) => a is CAbilityTrap cAbilityTrap && cAbilityTrap.TrapData.Damage > 0);
					}
					return false;
				}
				return true;
			}
			return true;
		}
	}

	public int DamageAmount => ((YMLData.Abilities != null) ? YMLData.Abilities.Where((CAbility a) => a is CAbilityDamage).Sum((CAbility a) => a.Strength) : 0) + (YMLData.DamageIfTargetMoves.HasValue ? YMLData.DamageIfTargetMoves.Value : 0);

	public bool Strengthen
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityStrengthen);
			}
			return false;
		}
	}

	public bool Push
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityPush);
			}
			return false;
		}
	}

	public bool Push2
	{
		get
		{
			if (YMLData.Abilities != null && YMLData.Abilities.Any((CAbility a) => a is CAbilityPush))
			{
				return YMLData.Abilities.Where((CAbility a) => a is CAbilityPush).Sum((CAbility a) => a.Strength) > 1;
			}
			return false;
		}
	}

	public bool Push3
	{
		get
		{
			if (YMLData.Abilities != null && YMLData.Abilities.Any((CAbility a) => a is CAbilityPush))
			{
				return YMLData.Abilities.Where((CAbility a) => a is CAbilityPush).Sum((CAbility a) => a.Strength) > 2;
			}
			return false;
		}
	}

	public bool Pull
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityPull);
			}
			return false;
		}
	}

	public bool TimeToken
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => (a.Flags & CAbility.EAbilityFlag.ModifiesTimeTokens) != 0);
			}
			return false;
		}
	}

	public List<CCondition.ENegativeCondition> NegativeConditions
	{
		get
		{
			if (YMLData.Overrides == null)
			{
				return new List<CCondition.ENegativeCondition>();
			}
			return YMLData.Overrides.Where((CAbilityOverride w) => w.NegativeConditions != null).SelectMany((CAbilityOverride s) => s.NegativeConditions).ToList();
		}
	}

	public List<CCondition.EPositiveCondition> PositiveConditions
	{
		get
		{
			if (YMLData.Overrides == null)
			{
				return new List<CCondition.EPositiveCondition>();
			}
			return YMLData.Overrides.Where((CAbilityOverride w) => w.PositiveConditions != null).SelectMany((CAbilityOverride s) => s.PositiveConditions).ToList();
		}
	}

	public bool HasAdditionalEffect
	{
		get
		{
			if ((YMLData.Abilities == null || YMLData.Abilities.Count <= 0) && (YMLData.Overrides == null || YMLData.Overrides.Count <= 0))
			{
				return YMLData.AddTarget;
			}
			return true;
		}
	}

	public bool HasNegativeEffect
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility ability) => ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !ability.IsPositive());
			}
			return false;
		}
	}

	public bool Bless
	{
		get
		{
			if (!YMLData.IsBless)
			{
				if (YMLData.Abilities != null)
				{
					return YMLData.Abilities.Any((CAbility ability) => (ability.PositiveConditions != null && ability.PositiveConditions.ContainsKey(CCondition.EPositiveCondition.Bless)) || ability.AbilityType == CAbility.EAbilityType.Bless);
				}
				return false;
			}
			return true;
		}
	}

	public int MainAbilityStrength
	{
		get
		{
			if (YMLData.Abilities == null || YMLData.Abilities.Count == 0)
			{
				return 0;
			}
			return YMLData.Abilities[0].Strength;
		}
	}

	public bool CreateIce => CreateTerrainAbilities().Any((CAbilityCreateTerrain x) => x.TerrainType == ScenarioManager.ObjectImportType.TerrainIce);

	public bool Move
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				if (!YMLData.Abilities.Any((CAbility x) => x.AbilityType == CAbility.EAbilityType.Move))
				{
					return YMLData.Abilities.Any((CAbility x) => x is CAbilityControlActor cAbilityControlActor && cAbilityControlActor.ControlActorData.ControlAbilities.Any((CAbility y) => y.AbilityType == CAbility.EAbilityType.Move));
				}
				return true;
			}
			return false;
		}
	}

	public bool ModeTransfer
	{
		get
		{
			if (YMLData.Abilities != null)
			{
				return YMLData.Abilities.Any((CAbility a) => a is CAbilityModeTransfer cAbilityModeTransfer && cAbilityModeTransfer.ModeUsage == CAbilityModeTransfer.EModeUse.Transfer);
			}
			return false;
		}
	}

	public IEnumerable<CAbilityCreateTerrain> CreateTerrainAbilities()
	{
		foreach (CAbility item in YMLData.Abilities.Where((CAbility x) => x.AbilityType == CAbility.EAbilityType.CreateTerrain))
		{
			yield return item as CAbilityCreateTerrain;
		}
	}

	public AttackModifierCard(string name)
		: base(name.GetHashCode(), ECardType.AttackModifier, name)
	{
	}

	public string AttackModifierLogString(string overrideMathMod = null, bool skipLoc = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		if (YMLData.CustomCombatLogDescriptionLocKey != null && !skipLoc)
		{
			return YMLData.CustomCombatLogDescriptionLocKey;
		}
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.NegativeConditions != null && a.NegativeConditions.Count > 0))
		{
			foreach (CCondition.ENegativeCondition item in YMLData.Overrides.Where((CAbilityOverride w) => w.NegativeConditions != null).SelectMany((CAbilityOverride s) => s.NegativeConditions))
			{
				stringBuilder.Append("<sprite name=" + item.ToString() + "> ");
			}
			flag = true;
		}
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.PositiveConditions != null && a.PositiveConditions.Count > 0))
		{
			foreach (CCondition.EPositiveCondition item2 in YMLData.Overrides.Where((CAbilityOverride w) => w.PositiveConditions != null).SelectMany((CAbilityOverride s) => s.PositiveConditions))
			{
				stringBuilder.Append("<sprite name=" + item2.ToString() + "> ");
			}
			flag = true;
		}
		if (Bless)
		{
			stringBuilder.Append("<sprite name=Bless> ");
		}
		if (YMLData.IsCurse)
		{
			stringBuilder.Append("<sprite name=Curse> ");
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityInfuse))
		{
			foreach (CAbilityInfuse item3 in YMLData.Abilities.OfType<CAbilityInfuse>())
			{
				if (item3.ElementsToInfuse.Count == 2 && !item3.ElementsToInfuse.Contains(ElementInfusionBoardManager.EElement.Any) && item3.ShowPicker)
				{
					stringBuilder.Append("<sprite name=CreateMixed-" + getMixedInfusion(item3.ElementsToInfuse[0], item3.ElementsToInfuse[1]) + "> ");
					continue;
				}
				foreach (ElementInfusionBoardManager.EElement item4 in item3.ElementsToInfuse)
				{
					stringBuilder.Append("<sprite name=" + item4.ToString() + "> ");
				}
			}
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && a.AbilityFilter.FilterAlly()))
		{
			stringBuilder.Append(YMLData.MathModifier + " " + YMLData.Abilities.Where((CAbility a) => a is CAbilityHeal).Sum((CAbility a) => a.Strength) + "<sprite name=Heal>");
			flag = true;
			flag2 = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityHeal && !a.AbilityFilter.FilterAlly()))
		{
			stringBuilder.Append("<sprite name=Heal> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a.Flags.HasFlag(CAbility.EAbilityFlag.ModifiesResonanceTokens)))
		{
			stringBuilder.Append("<sprite name=SSResonance> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityDamage))
		{
			stringBuilder.Append(YMLData.MathModifier + " " + YMLData.Abilities.Where((CAbility a) => a is CAbilityDamage).Sum((CAbility a) => a.Strength) + "<sprite name=Damage> ");
			flag = true;
			flag2 = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityShield))
		{
			stringBuilder.Append("<sprite name=Shield> ");
			flag = true;
		}
		if (YMLData.Overrides.Any((CAbilityOverride a) => a.Pierce.HasValue && a.Pierce > 0))
		{
			stringBuilder.Append("<sprite name=Pierce> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityPush))
		{
			stringBuilder.Append("<sprite name=Push> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityPull))
		{
			stringBuilder.Append("<sprite name=Pull> ");
			flag = true;
		}
		if (YMLData.AddTarget)
		{
			stringBuilder.Append("+1 <sprite name=Target> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityRefreshItemCards))
		{
			stringBuilder.Append("<sprite name=Refresh> ");
			flag = true;
		}
		if (YMLData.Abilities.Any((CAbility a) => a is CAbilityCreateLimitedHazardousTerrain))
		{
			stringBuilder.Append("<sprite name=HazardHex> <sprite name=Range>1");
			flag = false;
			flag2 = true;
		}
		string text = YMLData.MathModifier;
		if (!string.IsNullOrEmpty(overrideMathMod))
		{
			text = overrideMathMod;
		}
		if (text == "+0")
		{
			if (!flag && !flag2)
			{
				stringBuilder.Append(ApplyModifierFont(text));
			}
		}
		else if (!flag2)
		{
			stringBuilder.Append(ApplyModifierFont(text));
		}
		if (YMLData.Rolling)
		{
			stringBuilder.Append(" <sprite name=Rolling>");
			flag = true;
		}
		if (YMLData.Shuffle)
		{
			stringBuilder.Append(" <sprite name=Shuffle>");
			flag = true;
		}
		return stringBuilder.ToString();
		static string getMixedInfusion(ElementInfusionBoardManager.EElement element1, ElementInfusionBoardManager.EElement element2)
		{
			switch (element1)
			{
			case ElementInfusionBoardManager.EElement.Air:
				switch (element2)
				{
				case ElementInfusionBoardManager.EElement.Dark:
					break;
				case ElementInfusionBoardManager.EElement.Earth:
					goto IL_00ef;
				case ElementInfusionBoardManager.EElement.Fire:
					goto IL_00f5;
				case ElementInfusionBoardManager.EElement.Ice:
					goto IL_00fb;
				case ElementInfusionBoardManager.EElement.Light:
					goto IL_0101;
				default:
					goto end_IL_0001;
				}
				goto IL_00e9;
			case ElementInfusionBoardManager.EElement.Dark:
				switch (element2)
				{
				case ElementInfusionBoardManager.EElement.Air:
					break;
				case ElementInfusionBoardManager.EElement.Earth:
					goto IL_0107;
				case ElementInfusionBoardManager.EElement.Fire:
					goto IL_010d;
				case ElementInfusionBoardManager.EElement.Ice:
					goto IL_0113;
				case ElementInfusionBoardManager.EElement.Light:
					goto IL_0119;
				default:
					goto end_IL_0001;
				}
				goto IL_00e9;
			case ElementInfusionBoardManager.EElement.Earth:
				switch (element2)
				{
				case ElementInfusionBoardManager.EElement.Air:
					break;
				case ElementInfusionBoardManager.EElement.Dark:
					goto IL_0107;
				case ElementInfusionBoardManager.EElement.Fire:
					goto IL_011f;
				case ElementInfusionBoardManager.EElement.Ice:
					goto IL_0125;
				case ElementInfusionBoardManager.EElement.Light:
					goto IL_012b;
				default:
					goto end_IL_0001;
				}
				goto IL_00ef;
			case ElementInfusionBoardManager.EElement.Fire:
				switch (element2)
				{
				case ElementInfusionBoardManager.EElement.Air:
					break;
				case ElementInfusionBoardManager.EElement.Dark:
					goto IL_010d;
				case ElementInfusionBoardManager.EElement.Earth:
					goto IL_011f;
				case ElementInfusionBoardManager.EElement.Ice:
					goto IL_0131;
				case ElementInfusionBoardManager.EElement.Light:
					goto IL_0137;
				default:
					goto end_IL_0001;
				}
				goto IL_00f5;
			case ElementInfusionBoardManager.EElement.Ice:
				switch (element2)
				{
				case ElementInfusionBoardManager.EElement.Air:
					break;
				case ElementInfusionBoardManager.EElement.Dark:
					goto IL_0113;
				case ElementInfusionBoardManager.EElement.Earth:
					goto IL_0125;
				case ElementInfusionBoardManager.EElement.Fire:
					goto IL_0131;
				case ElementInfusionBoardManager.EElement.Light:
					goto IL_013d;
				default:
					goto end_IL_0001;
				}
				goto IL_00fb;
			case ElementInfusionBoardManager.EElement.Light:
				{
					switch (element2)
					{
					case ElementInfusionBoardManager.EElement.Air:
						break;
					case ElementInfusionBoardManager.EElement.Dark:
						goto IL_0119;
					case ElementInfusionBoardManager.EElement.Earth:
						goto IL_012b;
					case ElementInfusionBoardManager.EElement.Fire:
						goto IL_0137;
					case ElementInfusionBoardManager.EElement.Ice:
						goto IL_013d;
					default:
						goto end_IL_0001;
					}
					goto IL_0101;
				}
				IL_013d:
				return "IL";
				IL_0137:
				return "FL";
				IL_0131:
				return "FI";
				IL_012b:
				return "EL";
				IL_0125:
				return "IE";
				IL_011f:
				return "FE";
				IL_0119:
				return "LD";
				IL_0113:
				return "ID";
				IL_010d:
				return "FD";
				IL_0107:
				return "ED";
				IL_0101:
				return "AL";
				IL_00fb:
				return "IA";
				IL_00f5:
				return "FA";
				IL_00ef:
				return "AE";
				IL_00e9:
				return "AD";
				end_IL_0001:
				break;
			}
			return string.Empty;
		}
	}

	public static string ApplyModifierFont(string modifier)
	{
		if (modifier.StartsWith("*"))
		{
			if (modifier == "*0")
			{
				return string.Format("<b><color=#A050E0>{0}</color></b>", "x0");
			}
			return string.Format("<b><color=#D92727>{0}</color></b>", modifier.Replace("*", "x"));
		}
		if (modifier.StartsWith("+"))
		{
			return $"<b><color=#FFA414>{modifier}</color></b>";
		}
		if (modifier.StartsWith("-"))
		{
			return $"<b><color=#A050E0>{modifier}</color></b>";
		}
		Log.ErrorWithReport("Unexpected modifier type for combat log: " + modifier);
		return $"<b><color=#FFA414>{modifier}</color></b>";
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAttackSummary
{
	public class ConsumeData
	{
		public int BuffAmount { get; private set; }

		public List<ElementInfusionBoardManager.EElement> Elements { get; private set; }

		public ConsumeData(int buffAmount, List<ElementInfusionBoardManager.EElement> elements)
		{
			BuffAmount = buffAmount;
			Elements = elements;
		}
	}

	public class TargetSummary
	{
		public enum SummaryState
		{
			SelectingTargets,
			DrawingModifiers,
			FinishedDrawingModifiers
		}

		public bool ShadowMarked;

		private List<AttackModifierYMLData> m_UsedAttackMods;

		private List<AttackModifierYMLData> m_NotUsedAttackMods;

		private List<AttackModifierYMLData> m_DrawnAttackMods;

		private int m_PreModifierAttackStrength;

		public int StrengthFromModifierOverrides;

		private int m_InitialAbilityAttackStrength;

		public List<string> NonScalarMathModifiers = new List<string>();

		public List<string> AdditionalNonScalarModifiers = new List<string>();

		public List<string> PreShieldScalarModifiers = new List<string>();

		public List<(string, string)> PostShieldScalarModifiers = new List<(string, string)>();

		public string OverrideMathModifier = string.Empty;

		public SEventAttackModifier AttackModifierEvent;

		private List<SEventAttackModifier.OverrideBuffData> m_eventOverrideBuffData;

		public SummaryState State;

		public CAttackSummary Parent { get; set; }

		public CActor Actor { get; private set; }

		public CActor RedirectedFromActor { get; set; }

		public int Shield { get; private set; }

		public int Pierce { get; private set; }

		public int ShieldMinusPierce { get; private set; }

		public bool Poison { get; private set; }

		public bool ExploitedVulnerability { get; private set; }

		public int ExploitedVulnerabilityStrength { get; private set; }

		public bool Brittle { get; set; }

		public bool Ward { get; set; }

		public bool Invulnerable { get; private set; }

		public CAbilityAttack ItemOverrideAbility { get; set; }

		public int ActualDamageTaken { get; set; }

		public CAbility AttackAbilityWithOverrides { get; set; }

		public CAbility OriginalAbility { get; set; }

		public CAbilityAttack AttackAbility => AttackAbilityWithOverrides as CAbilityAttack;

		public CActor DamageSourceActor { get; set; }

		public bool IsTargeted { get; set; }

		public bool ProcessedStatusEffects { get; set; }

		public bool PerformedRetaliate { get; set; }

		public bool DisadvantageFromAdjacent { get; private set; }

		public bool AttackersGainDisadvantage { get; private set; }

		public bool AttackersGainAdvantage { get; private set; }

		public EAdvantageStatuses OverallAdvantage { get; set; }

		public List<CActiveBonus> PassiveShieldBonuses { get; private set; }

		public int ActiveBonusShieldBuff { get; private set; }

		public List<int> ConditionalOverridesStrengthBuffs { get; private set; }

		public List<int> ConditionalOverridesXPBuffs { get; private set; }

		public List<int> ItemStrengthBuffs { get; private set; }

		public int AttackEffectBuff { get; private set; }

		public int AttackEffectPierce { get; private set; }

		public int AttackEffectXP { get; private set; }

		public int AttackScalar { get; private set; }

		public int AttackAbilityScalar { get; private set; }

		public int ScenarioBonusStrength { get; private set; }

		public List<AttackModifierYMLData> UsedAttackMods
		{
			get
			{
				return m_UsedAttackMods;
			}
			set
			{
				m_UsedAttackMods = value;
			}
		}

		public List<AttackModifierYMLData> NotUsedAttackMods
		{
			get
			{
				return m_NotUsedAttackMods;
			}
			set
			{
				m_NotUsedAttackMods = value;
			}
		}

		public List<AttackModifierYMLData> DrawnAttackMods
		{
			get
			{
				return m_DrawnAttackMods;
			}
			set
			{
				m_DrawnAttackMods = value;
			}
		}

		public List<int> ActiveBonusStrengthBuffs { get; private set; }

		public List<int> ActiveBonusXPBuffs { get; private set; }

		public List<CActiveBonus> ThisTargetAttackActiveBonuses { get; private set; }

		public int DamageIfTargetMoves { get; private set; }

		public int PreShieldAttackStrength => AttackAbilityWithOverrides?.Strength ?? OriginalAbility?.Strength ?? 0;

		public int FinalAttackStrength => CalcExpressionString(calcPostShieldScalars: true, calcShield: true);

		public int TargetNumber { get; private set; }

		public IEnumerable<AttackModifierYMLData> AllDrawnModifiers => m_DrawnAttackMods;

		public IEnumerable<AttackModifierYMLData> UsedModifiers => m_UsedAttackMods;

		public CActor ActorToAttack => Actor;

		public CActor ActorToShowOptionsFor
		{
			get
			{
				CActor actorToAttack = ActorToAttack;
				if (actorToAttack.IsMirrorActor(out var mirrorSource, useCacheOnly: true))
				{
					return mirrorSource;
				}
				if (actorToAttack is CHeroSummonActor cHeroSummonActor)
				{
					return cHeroSummonActor.Summoner;
				}
				return actorToAttack;
			}
		}

		public bool NoDrawing()
		{
			bool flag = false;
			if (!(AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack))
			{
				return true;
			}
			foreach (CAttackEffect attackEffect in cAbilityAttack.AttackEffects)
			{
				attackEffect.GetBonus(ActorToAttack, cAbilityAttack, out var _, out var _, out var _, out var _, out var _, out var skipAttackModifiers, out var _, out var _);
				flag |= skipAttackModifiers;
			}
			return flag;
		}

		public TargetSummary(TargetSummary other)
		{
			State = other.State;
			Parent = other.Parent;
			Actor = other.Actor;
			RedirectedFromActor = other.RedirectedFromActor;
			Shield = other.Shield;
			Pierce = other.Pierce;
			ProcessedStatusEffects = other.ProcessedStatusEffects;
			ShieldMinusPierce = other.ShieldMinusPierce;
			Poison = other.Poison;
			ExploitedVulnerability = other.ExploitedVulnerability;
			ExploitedVulnerabilityStrength = other.ExploitedVulnerabilityStrength;
			Brittle = other.Brittle;
			Ward = other.Ward;
			Invulnerable = other.Invulnerable;
			ItemOverrideAbility = other.ItemOverrideAbility;
			ActualDamageTaken = other.ActualDamageTaken;
			AttackAbilityWithOverrides = other.AttackAbilityWithOverrides;
			OriginalAbility = other.OriginalAbility;
			DamageSourceActor = other.DamageSourceActor;
			IsTargeted = other.IsTargeted;
			DisadvantageFromAdjacent = other.DisadvantageFromAdjacent;
			AttackersGainDisadvantage = other.AttackersGainDisadvantage;
			AttackersGainAdvantage = other.AttackersGainAdvantage;
			OverallAdvantage = other.OverallAdvantage;
			PassiveShieldBonuses = other.PassiveShieldBonuses?.ToList();
			ActiveBonusShieldBuff = other.ActiveBonusShieldBuff;
			ConditionalOverridesStrengthBuffs = other.ConditionalOverridesStrengthBuffs?.ToList();
			ConditionalOverridesXPBuffs = other.ConditionalOverridesXPBuffs?.ToList();
			ItemStrengthBuffs = other.ItemStrengthBuffs?.ToList();
			AttackEffectBuff = other.AttackEffectBuff;
			AttackEffectPierce = other.AttackEffectPierce;
			AttackEffectXP = other.AttackEffectXP;
			AttackScalar = other.AttackScalar;
			AttackAbilityScalar = other.AttackAbilityScalar;
			ScenarioBonusStrength = other.ScenarioBonusStrength;
			UsedAttackMods = other.UsedAttackMods?.ToList();
			NotUsedAttackMods = other.NotUsedAttackMods?.ToList();
			DrawnAttackMods = other.DrawnAttackMods?.ToList();
			ActiveBonusStrengthBuffs = other.ActiveBonusStrengthBuffs?.ToList();
			ActiveBonusXPBuffs = other.ActiveBonusXPBuffs?.ToList();
			ThisTargetAttackActiveBonuses = other.ThisTargetAttackActiveBonuses?.ToList();
			DamageIfTargetMoves = other.DamageIfTargetMoves;
			ShadowMarked = other.ShadowMarked;
			TargetNumber = other.TargetNumber;
			m_UsedAttackMods = other.m_UsedAttackMods?.ToList();
			m_NotUsedAttackMods = other.m_NotUsedAttackMods?.ToList();
			m_DrawnAttackMods = other.m_DrawnAttackMods?.ToList();
			m_PreModifierAttackStrength = other.m_PreModifierAttackStrength;
			m_InitialAbilityAttackStrength = other.m_InitialAbilityAttackStrength;
			NonScalarMathModifiers = other.NonScalarMathModifiers?.ToList() ?? new List<string>();
			AdditionalNonScalarModifiers = other.AdditionalNonScalarModifiers?.ToList() ?? new List<string>();
			PreShieldScalarModifiers = other.PreShieldScalarModifiers?.ToList() ?? new List<string>();
			PostShieldScalarModifiers = other.PostShieldScalarModifiers?.ToList() ?? new List<(string, string)>();
			OverrideMathModifier = other.OverrideMathModifier;
			AttackModifierEvent = null;
			if (other.AttackModifierEvent != null)
			{
				SerializationInfo info = new SerializationInfo(typeof(SEventAttackModifier), new FormatterConverter());
				other.AttackModifierEvent.GetObjectData(info, default);
				AttackModifierEvent = new SEventAttackModifier(info, default);
			}
			m_eventOverrideBuffData = other.m_eventOverrideBuffData?.ToList();
			PerformedRetaliate = other.PerformedRetaliate;
		}

		public TargetSummary(int strength, CActor target, CActor source)
		{
			State = SummaryState.SelectingTargets;
			Actor = target;
			IsTargeted = false;
			DamageSourceActor = source;
			ConditionalOverridesStrengthBuffs = new List<int>();
			ConditionalOverridesXPBuffs = new List<int>();
			ItemStrengthBuffs = new List<int>();
			UsedAttackMods = new List<AttackModifierYMLData>();
			DrawnAttackMods = new List<AttackModifierYMLData>();
			m_eventOverrideBuffData = new List<SEventAttackModifier.OverrideBuffData>();
			Poison = Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison);
			Brittle = Actor.Tokens.HasKey(CCondition.ENegativeCondition.Brittle);
			Ward = Actor.Tokens.HasKey(CCondition.EPositiveCondition.Ward);
			AttackAbilityScalar = 0;
			m_InitialAbilityAttackStrength = (m_PreModifierAttackStrength = strength);
			Invulnerable = Actor.Invulnerable;
			ShadowMarked = target.IsShadowMarked;
			PassiveShieldBonuses = (from x in Actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Shield)
				where !x.Ability.ActiveBonusData.IsToggleBonus && x.IsActiveBonusToggledAndNotRestricted(Actor)
				select x).ToList();
		}

		public void AddNonScalarModifier(int val, bool hidden = false)
		{
			if (val != 0)
			{
				string text = "";
				if (val > 0)
				{
					text += "+";
				}
				text += val;
				if (hidden)
				{
					NonScalarMathModifiers.Add(text);
				}
				else
				{
					AdditionalNonScalarModifiers.Add(text);
				}
			}
		}

		public void RemoveNonScalarModifier(int val, bool hidden = false)
		{
			if (val != 0)
			{
				string text = "";
				if (val > 0)
				{
					text += "+";
				}
				text += val;
				if (hidden)
				{
					AdditionalNonScalarModifiers.Add(text);
				}
				else
				{
					AdditionalNonScalarModifiers.Remove(text);
				}
			}
		}

		public TargetSummary(CAttackSummary parent, CActor target, CAbility ability, int targetNumber = 0)
			: this(ability.Strength, target, ability.TargetingActor)
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				TargetNumber = targetNumber;
				Parent = parent;
				AttackAbilityWithOverrides = CAbility.CopyAbility(ability, generateNewID: false, fullCopy: true);
				AttackAbilityWithOverrides.ConditionalOverrides.Clear();
				if (AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack)
				{
					cAbilityAttack.IsAttackAbilityWithOverrides = true;
					cAbilityAttack.AttackSummary = null;
					cAbilityAttack.SetMiscVar("OriginalAttackAbility", new TValue(ability));
				}
				OriginalAbility = ability;
				foreach (CAbility subAbility in AttackAbilityWithOverrides.SubAbilities)
				{
					subAbility.ParentAbility = AttackAbilityWithOverrides;
				}
				if (ability is CAbilityAttack { ChainAttack: not false } cAbilityAttack2)
				{
					int num = 0;
					num = ((!cAbilityAttack2.ActorsToTarget.Contains(target)) ? cAbilityAttack2.ActorsToTarget.Count : cAbilityAttack2.ActorsToTarget.IndexOf(target));
					m_InitialAbilityAttackStrength -= cAbilityAttack2.ChainAttackDamageReduction * num;
				}
				ActiveBonusStrengthBuffs = new List<int>();
				ActiveBonusXPBuffs = new List<int>();
				ThisTargetAttackActiveBonuses = new List<CActiveBonus>();
				if (parent.AttackActiveBonuses != null)
				{
					ThisTargetAttackActiveBonuses.AddRange(parent.AttackActiveBonuses);
				}
				ThisTargetAttackActiveBonuses.AddRange((from x in CActiveBonus.FindApplicableActiveBonuses(target, CAbility.EAbilityType.Attack)
					where x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingAttacks)
					select x).ToList());
				int strength = AttackAbilityWithOverrides.Strength;
				AttackAbilityWithOverrides.Strength = OriginalAbility.ModifiedStrength();
				AttackAbilityWithOverrides.SetStatBasedOnXFromTarget(target, OriginalAbility.TargetingActor, OriginalAbility.StatIsBasedOnXEntries, OriginalAbility.AbilityFilter, OriginalAbility);
				int val = AttackAbilityWithOverrides.Strength - strength;
				AddNonScalarModifier(val, hidden: true);
				AttackAbilityWithOverrides.Strength = strength;
				if (ability is CAbilityAttack cAbilityAttack3)
				{
					foreach (CAbilityOverride currentOverride in cAbilityAttack3.CurrentOverrides)
					{
						currentOverride.OverrideStrength(AttackAbilityWithOverrides);
						if (currentOverride.Vars == null)
						{
							continue;
						}
						foreach (var var in currentOverride.Vars)
						{
							if (var.Item1.Equals("Strength", StringComparison.InvariantCultureIgnoreCase))
							{
								TValue item = var.Item2;
								TValue tValue = item.Calc(ability.VarsContext);
								currentOverride.OverrideStrength(AttackAbilityWithOverrides, tValue.GetInt(ability.VarsContext));
							}
						}
					}
				}
				int xpBuff = (m_InitialAbilityAttackStrength = (m_PreModifierAttackStrength = AttackAbilityWithOverrides.Strength));
				if (ThisTargetAttackActiveBonuses != null)
				{
					int num2 = 0;
					foreach (CActiveBonus thisTargetAttackActiveBonuse in ThisTargetAttackActiveBonuses)
					{
						if (!thisTargetAttackActiveBonuse.WillBeValidOnReachTargetActor(AttackAbility, target))
						{
							continue;
						}
						CActor cActor = (thisTargetAttackActiveBonuse.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffIncomingAttacks) ? ability.TargetingActor : target);
						if (thisTargetAttackActiveBonuse.SingleTarget != null && thisTargetAttackActiveBonuse.SingleTarget != cActor)
						{
							continue;
						}
						List<(string, TValue)> list = thisTargetAttackActiveBonuse?.Ability?.ActiveBonusData?.Vars;
						if (list != null)
						{
							int num3 = Math.Min(ability.ActorsToTarget.Count + 1, targetNumber);
							list.RemoveAll(((string, TValue) p) => p.Item1 == "TargetSelectionNumber");
							list.Add(("TargetSelectionNumber", num3));
						}
						int num4 = thisTargetAttackActiveBonuse.ReferenceStrength(ability, cActor);
						if (thisTargetAttackActiveBonuse.Ability.ActiveBonusData.AttackType == CAbility.EAttackType.Default)
						{
							num2 = Math.Max(num2, num4);
						}
						else if (num4 != 0)
						{
							ActiveBonusStrengthBuffs.Add(num4);
						}
						int num5 = thisTargetAttackActiveBonuse.ReferenceXP(ability, cActor);
						if (num5 != 0)
						{
							ActiveBonusXPBuffs.Add(num5);
						}
						string overrideBuffName = thisTargetAttackActiveBonuse.Ability.ActiveBonusData?.GetMiscVar("OverrideLayoutName").GetString(null, thisTargetAttackActiveBonuse.BaseCard.Name);
						m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.ActiveBonus, overrideBuffName, num4, thisTargetAttackActiveBonuse.Ability.NegativeConditions.Keys.ToList(), thisTargetAttackActiveBonuse.Ability.PositiveConditions.Keys.ToList(), thisTargetAttackActiveBonuse.Actor.Class.ID, thisTargetAttackActiveBonuse.BaseCard.CardType != CBaseCard.ECardType.ScenarioModifier));
					}
					if (num2 > 0)
					{
						ActiveBonusStrengthBuffs.Add(num2);
					}
				}
				foreach (CConditionalOverride conditionalOverride in ability.ConditionalOverrides)
				{
					CAbility ability2 = ability;
					if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.SubAbility))
					{
						ability2 = ability.ParentAbility;
					}
					else if (conditionalOverride.Requirements.Equals(CAbilityRequirements.EStartAbilityRequirementType.PreviousAbility))
					{
						ability2 = ((CPhaseAction)PhaseManager.Phase).PreviousPhaseAbilities.Last().m_Ability;
					}
					if (conditionalOverride.Self || !conditionalOverride.Filter.IsValidTarget(target, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) || !conditionalOverride.Requirements.MeetsAbilityRequirements(ability.TargetingActor, ability2) || (conditionalOverride.TargetRequirements != null && !conditionalOverride.TargetRequirements.MeetsAbilityRequirements(target, ability2)))
					{
						continue;
					}
					int strength2 = AttackAbilityWithOverrides.Strength;
					List<CCondition.ENegativeCondition> list2 = new List<CCondition.ENegativeCondition>();
					List<CCondition.EPositiveCondition> list3 = new List<CCondition.EPositiveCondition>();
					foreach (CAbilityOverride abilityOverride in conditionalOverride.AbilityOverrides)
					{
						CAbilityOverride cAbilityOverride = AttackAbilityWithOverrides.OverrideAbilityValues(abilityOverride, perform: false, null, conditionalOverride.Filter, target);
						AbilityData.MiscAbilityData miscAbilityData = cAbilityOverride.MiscAbilityData;
						if (miscAbilityData != null && miscAbilityData.AddTargetPropertyToStrength == CActor.EOverrideLookupProperty.Shield)
						{
							int num6 = ((cAbilityOverride.MiscAbilityData == null || !cAbilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier.HasValue) ? 1 : cAbilityOverride.MiscAbilityData.AddTargetPropertyToStrengthMultiplier.Value);
							AttackAbilityWithOverrides.Strength += target.CalculateShield(ability) * num6;
						}
						if (cAbilityOverride.NegativeConditions != null)
						{
							list2.AddRange(cAbilityOverride.NegativeConditions);
						}
						if (cAbilityOverride.PositiveConditions != null)
						{
							list3.AddRange(cAbilityOverride.PositiveConditions);
						}
					}
					if (AttackAbilityWithOverrides.Strength - strength2 > 0)
					{
						ConditionalOverridesStrengthBuffs.Add(AttackAbilityWithOverrides.Strength - strength2);
					}
					m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Conditional, ability.GetAbilityBaseCard()?.Name, AttackAbilityWithOverrides.Strength - strength2, list2, list3));
					if (conditionalOverride.XP > 0)
					{
						ConditionalOverridesXPBuffs.Add(conditionalOverride.XP);
					}
				}
				foreach (CItem item2 in ability.ActiveSingleTargetItems.Where((CItem x) => x.SingleTarget == target))
				{
					int strength3 = AttackAbilityWithOverrides.Strength;
					List<CCondition.ENegativeCondition> list4 = new List<CCondition.ENegativeCondition>();
					List<CCondition.EPositiveCondition> list5 = new List<CCondition.EPositiveCondition>();
					foreach (CAbilityOverride @override in item2.YMLData.Data.Overrides)
					{
						CAbilityOverride cAbilityOverride2 = AttackAbilityWithOverrides.OverrideAbilityValues(@override, perform: false, null, null, target);
						if (cAbilityOverride2.NegativeConditions != null)
						{
							list4.AddRange(cAbilityOverride2.NegativeConditions);
						}
						if (cAbilityOverride2.PositiveConditions != null)
						{
							list5.AddRange(cAbilityOverride2.PositiveConditions);
						}
					}
					int num7 = 0;
					CAbilityAttack cAbilityAttack4 = AttackAbilityWithOverrides as CAbilityAttack;
					foreach (CAttackEffect attackEffect in cAbilityAttack4.AttackEffects)
					{
						attackEffect.GetBonus(ActorToAttack, cAbilityAttack4, out var attackBuff, out xpBuff, out var _, out var _, out var _, out var _, out var _, out var _);
						num7 += attackBuff;
					}
					int num8 = Math.Max(0, cAbilityAttack4.Strength + num7);
					if (num8 - strength3 > 0)
					{
						ItemStrengthBuffs.Add(num8 - strength3);
					}
					m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, item2.Name, num8 - strength3, list4, list5));
				}
				foreach (CActiveBonus item3 in ability.ActiveSingleTargetActiveBonuses.Where((CActiveBonus x) => x.SingleTarget == target))
				{
					int strength4 = AttackAbilityWithOverrides.Strength;
					List<CCondition.ENegativeCondition> overrideNegativeConditions = new List<CCondition.ENegativeCondition>();
					List<CCondition.EPositiveCondition> overridePositiveConditions = new List<CCondition.EPositiveCondition>();
					if (item3.Ability.Song != null)
					{
						foreach (CSong.SongEffect songEffect in item3.Ability.Song.SongEffects)
						{
							songEffect.OnSongEffectToggled(AttackAbilityWithOverrides, toggledOn: true);
						}
					}
					if (AttackAbilityWithOverrides.Strength - strength4 > 0)
					{
						ItemStrengthBuffs.Add(AttackAbilityWithOverrides.Strength - strength4);
					}
					if (!item3.Ability.ActiveBonusData.HiddenInLog)
					{
						m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, item3.BaseCard.Name, AttackAbilityWithOverrides.Strength - strength4, overrideNegativeConditions, overridePositiveConditions));
					}
				}
				foreach (CItem activeOverrideItem in ability.ActiveOverrideItems)
				{
					int num9 = 0;
					List<CCondition.ENegativeCondition> list6 = new List<CCondition.ENegativeCondition>();
					List<CCondition.EPositiveCondition> list7 = new List<CCondition.EPositiveCondition>();
					foreach (CAbilityOverride override2 in activeOverrideItem.YMLData.Data.Overrides)
					{
						if (override2.Strength.HasValue)
						{
							num9 += override2.Strength.Value;
						}
						if (override2.NegativeConditions != null)
						{
							list6.AddRange(override2.NegativeConditions);
						}
						if (override2.PositiveConditions != null)
						{
							list7.AddRange(override2.PositiveConditions);
						}
					}
					m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.Item, activeOverrideItem.Name, num9, list6, list7));
				}
				foreach (CActiveBonus attackActiveBonuse in parent.AttackActiveBonuses)
				{
					if (attackActiveBonuse.WillBeValidOnReachTargetActor(AttackAbilityWithOverrides, ActorToAttack))
					{
						int num10 = attackActiveBonuse.ReferenceAbilityStrengthScalar(ability, target);
						int num11 = m_PreModifierAttackStrength * num10 - m_PreModifierAttackStrength;
						AttackAbilityScalar += ((num10 > 1) ? num10 : 0);
						if (num11 > 0)
						{
							ActiveBonusStrengthBuffs.Add(num11);
						}
						if (!attackActiveBonuse.Ability.ActiveBonusData.HiddenInLog && num11 != 0)
						{
							m_eventOverrideBuffData.Add(new SEventAttackModifier.OverrideBuffData(SEventAttackModifier.OverrideBuffData.EOverrideBuffType.ActiveBonus, attackActiveBonuse.BaseCard.Name, num11, attackActiveBonuse.Ability.NegativeConditions.Keys.ToList(), attackActiveBonuse.Ability.PositiveConditions.Keys.ToList(), attackActiveBonuse.Actor.Class.ID, attackActiveBonuse.BaseCard.CardType != CBaseCard.ECardType.ScenarioModifier, isScalar: true));
						}
					}
				}
				m_PreModifierAttackStrength += ActiveBonusStrengthBuffs.Sum() + ConditionalOverridesStrengthBuffs.Sum() + ItemStrengthBuffs.Sum();
				if (ability is CAbilityAttack { ChainAttack: not false } cAbilityAttack5)
				{
					int num12 = 0;
					num12 = ((!cAbilityAttack5.ActorsToTarget.Contains(target)) ? cAbilityAttack5.ActorsToTarget.Count : cAbilityAttack5.ActorsToTarget.IndexOf(target));
					m_PreModifierAttackStrength -= cAbilityAttack5.ChainAttackDamageReduction * num12;
				}
				if (AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack6)
				{
					AttackEffectBuff = 0;
					AttackEffectXP = 0;
					AttackEffectPierce = 0;
					foreach (CAttackEffect attackEffect2 in cAbilityAttack6.AttackEffects)
					{
						attackEffect2.GetBonus(target, cAbilityAttack6, out var attackBuff2, out var xpBuff2, out var _, out var _, out var pierceBuff2, out var _, out var _, out var _);
						AttackEffectBuff += attackBuff2;
						AttackEffectXP += xpBuff2;
						AttackEffectPierce += pierceBuff2;
					}
					foreach (CAttackActiveBonus_BuffAttack item4 in from y in ThisTargetAttackActiveBonuses.SelectMany((CActiveBonus x) => x.BespokeBehaviours)
						where y is CAttackActiveBonus_BuffAttack
						select y)
					{
						AttackEffectPierce += item4.ReferencePierce(cAbilityAttack6, target);
					}
					foreach (CAttackActiveBonus_BuffIncomingAttacks item5 in from y in ThisTargetAttackActiveBonuses.SelectMany((CActiveBonus x) => x.BespokeBehaviours)
						where y is CAttackActiveBonus_BuffIncomingAttacks
						select y)
					{
						AttackEffectPierce += item5.ReferencePierce(cAbilityAttack6, target);
					}
					m_PreModifierAttackStrength += AttackEffectBuff;
					CalcOverallAdvantageDisadvantage();
					Poison = Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison);
					if (Poison)
					{
						m_PreModifierAttackStrength++;
					}
					if (cAbilityAttack6.ExploitVulnerability && !ExploitedVulnerability)
					{
						ExploitedVulnerabilityStrength = 0;
						foreach (CActiveBonus item6 in Actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ExploitableVulnerability))
						{
							ExploitedVulnerability = true;
							ExploitedVulnerabilityStrength += item6.Ability.Strength * 2;
						}
						m_PreModifierAttackStrength += ExploitedVulnerabilityStrength;
					}
					AttackScalar = 0;
					foreach (CActiveBonus attackActiveBonuse2 in parent.AttackActiveBonuses)
					{
						int num13 = attackActiveBonuse2.ReferenceStrengthScalar(ability, target);
						int num14 = m_PreModifierAttackStrength * num13 - m_PreModifierAttackStrength;
						AttackScalar += ((num13 > 1) ? num13 : 0);
						if (num14 > 0)
						{
							ActiveBonusStrengthBuffs.Add(num14);
							m_PreModifierAttackStrength += num14;
						}
					}
				}
				if (ScenarioManager.CurrentScenarioState != null)
				{
					foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
					{
						scenarioScript.TickAttackSummaryCreated(this, null);
					}
				}
				Invulnerable = Actor.Invulnerable && !ability.TargetingActor.PierceInvulnerability;
				CacheFinalAttackStrength(initial: true, beforeModifiers: true);
			}
		}

		public void CacheFinalAttackStrength(bool initial, bool beforeModifiers)
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				CActor cActor = null;
				if (DamageSourceActor != null)
				{
					cActor = DamageSourceActor;
				}
				else if (OriginalAbility != null)
				{
					cActor = ((OriginalAbility.OriginalTargetingActor != null) ? OriginalAbility.OriginalTargetingActor : OriginalAbility.TargetingActor);
				}
				int modifiedStrength = m_PreModifierAttackStrength + StrengthFromModifierOverrides;
				if (initial && cActor == Actor && Actor.FindApplicableActiveBonuses(CAbility.EAbilityType.ImmunityTo).Any((CActiveBonus it) => (it.Ability as CAbilityImmunityTo).ImmuneToSelfDamage))
				{
					modifiedStrength = 0;
				}
				AttackAbilityWithOverrides?.SetModifiedStrength(modifiedStrength);
				CalcShieldPierce();
			}
		}

		public void CalcShieldPierce()
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				if (AttackAbilityWithOverrides is CAbilityAttack cAbilityAttack)
				{
					CActor actor = Actor;
					Shield = Actor.BaseShield();
					int num = actor.CalculateShield(OriginalAbility, ignoreNeutralizeShield: false, includeItemShieldValues: true, cache: true, forceActiveItems: true);
					ActiveBonusShieldBuff = num - Shield;
					Shield += ActiveBonusShieldBuff;
					GameState.TotalShieldDuringLastAttack = Shield;
					Pierce = cAbilityAttack.Pierce + AttackEffectPierce;
					if (Pierce >= 99999)
					{
						ShieldMinusPierce = 0;
					}
					else
					{
						ShieldMinusPierce = Math.Max(0, Shield - Pierce);
					}
				}
			}
		}

		public void CalcDisadvantage()
		{
			CAbility attackAbilityWithOverrides = AttackAbilityWithOverrides;
			CAbilityAttack attackAbility = attackAbilityWithOverrides as CAbilityAttack;
			if (attackAbility == null)
			{
				return;
			}
			CActor targetingActor = attackAbility.TargetingActor;
			bool flag = ScenarioManager.IsTileAdjacent(targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y, Actor.ArrayIndex.X, Actor.ArrayIndex.Y);
			if (Actor is CObjectActor)
			{
				CObjectActor cObjectActor = (CObjectActor)Actor;
				if (cObjectActor.AttachedProp != null && cObjectActor.AttachedProp is CObjectObstacle)
				{
					foreach (TileIndex pathingBlocker in (cObjectActor.AttachedProp as CObjectObstacle).PathingBlockers)
					{
						flag |= ScenarioManager.IsTileAdjacent(targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y, pathingBlocker.X, pathingBlocker.Y);
					}
				}
			}
			if (!flag && Actor.MultiHexArrayIndexes != null && Actor.MultiHexArrayIndexes.Count > 0)
			{
				foreach (Point multiHexArrayIndex in Actor.MultiHexArrayIndexes)
				{
					flag |= ScenarioManager.IsTileAdjacent(targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y, multiHexArrayIndex.X, multiHexArrayIndex.Y);
				}
			}
			DisadvantageFromAdjacent = !attackAbility.IsMeleeAttack & flag;
			AttackersGainDisadvantage = (Actor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.AttackersGainDisadv) || CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.AttackersGainDisadvantage).Count > 0 || CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.Shield).FindAll((CActiveBonus x) => x is CShieldActiveBonus && x.HasBehaviour<CShieldActiveBonus_ShieldAndDisadvantage>()).Count > 0;
			AttackersGainDisadvantage |= ThisTargetAttackActiveBonuses.Any((CActiveBonus x) => x.TryGetBehaviour(out CAttackActiveBonus_BuffAttack behaviour) && behaviour.ReferenceDisadvantage(attackAbility, Actor));
			bool attackersGainDisadvantage = AttackersGainDisadvantage;
			AbilityData.MiscAbilityData miscAbilityData = attackAbility.MiscAbilityData;
			AttackersGainDisadvantage = attackersGainDisadvantage | (miscAbilityData != null && miscAbilityData.AttackHasDisadvantage == true);
			AttackersGainDisadvantage |= DisadvantageFromAdjacent;
		}

		public void CalcAdvantage()
		{
			CAbility attackAbilityWithOverrides = AttackAbilityWithOverrides;
			CAbilityAttack attackAbility = attackAbilityWithOverrides as CAbilityAttack;
			if (attackAbility == null)
			{
				return;
			}
			AttackersGainAdvantage = CActiveBonus.FindApplicableActiveBonuses(Actor, CAbility.EAbilityType.Attack).Any((CActiveBonus x) =>
			{
				if (x.HasBehaviour<CAttackActiveBonus_BuffIncomingAttacks>())
				{
					AbilityData.MiscAbilityData miscAbilityData2 = x.Ability.MiscAbilityData;
					if (miscAbilityData2 == null)
					{
						return false;
					}
					return miscAbilityData2.AttackHasAdvantage == true;
				}
				return false;
			});
			AttackersGainAdvantage |= ThisTargetAttackActiveBonuses.Any((CActiveBonus x) => x.TryGetBehaviour(out CAttackActiveBonus_BuffAttack behaviour) && behaviour.ReferenceAdvantage(attackAbility, Actor) && (!x.Ability.ActiveBonusData.IsSingleTargetBonus || x.SingleTarget == Actor));
			bool attackersGainAdvantage = AttackersGainAdvantage;
			AbilityData.MiscAbilityData miscAbilityData = attackAbility.MiscAbilityData;
			AttackersGainAdvantage = attackersGainAdvantage | (miscAbilityData != null && miscAbilityData.AttackHasAdvantage == true);
		}

		public void CalcOverallAdvantageDisadvantage()
		{
			CActor cActor = ((AttackAbilityWithOverrides.OriginalTargetingActor != null) ? AttackAbilityWithOverrides.OriginalTargetingActor : AttackAbilityWithOverrides.TargetingActor);
			CChallenge.CheckTrigger(CChallenge.Trigger.OnAttack, new CChallenge.TriggerParams
			{
				actor = cActor,
				actorBeingAttacked = Actor,
				attackAbility = AttackAbility
			});
			CalcAdvantage();
			CalcDisadvantage();
			OverallAdvantage = cActor.GetAdvantageStatus(AttackersGainAdvantage, AttackersGainDisadvantage, TargetNumber);
		}

		public int CalcExpressionString(bool calcPostShieldScalars, bool calcShield)
		{
			return CalcExpressionString(ShieldMinusPierce, calcPostShieldScalars, calcShield);
		}

		public int CalcExpressionString(int shield, bool calcPostShieldScalars, bool calcShield)
		{
			int strength = AttackAbilityWithOverrides?.ModifiedStrength() ?? m_PreModifierAttackStrength;
			return CalcExpressionString(strength, shield, calcPostShieldScalars, calcShield);
		}

		public int CalcExpressionString(int strength, int shield, bool calcPostShieldScalars, bool calcShield)
		{
			int num = strength - m_PreModifierAttackStrength;
			string text = $"({m_PreModifierAttackStrength + num}";
			if (OverrideMathModifier != null)
			{
				text += OverrideMathModifier;
			}
			text += ")";
			foreach (string nonScalarMathModifier in NonScalarMathModifiers)
			{
				text += nonScalarMathModifier;
			}
			foreach (string additionalNonScalarModifier in AdditionalNonScalarModifiers)
			{
				text += additionalNonScalarModifier;
			}
			if (PreShieldScalarModifiers.Count > 0)
			{
				text = text.Insert(0, "(");
				text += ")";
				foreach (string preShieldScalarModifier in PreShieldScalarModifiers)
				{
					text += preShieldScalarModifier;
				}
			}
			if (calcShield && shield != 0)
			{
				text += $"-{shield}";
			}
			if (calcPostShieldScalars)
			{
				text = text.Insert(0, "(");
				text += ")";
				foreach (var postShieldScalarModifier in PostShieldScalarModifiers)
				{
					string item = postShieldScalarModifier.Item1;
					if (item.StartsWith("/"))
					{
						text = text.Insert(0, "floor(");
						text = text + item + ")";
					}
					else
					{
						text += item;
					}
				}
				if (Brittle)
				{
					text += "*2";
				}
				if (Ward)
				{
					text = text.Insert(0, "floor(");
					text += "/2)";
				}
			}
			int result = 0;
			try
			{
				result = Math.Max(0, Convert.ToInt32(GameState.s_MathParser.Parse(text, isRadians: false)));
			}
			catch
			{
				Log.Error("Can't parse attack modifier expression string");
			}
			finally
			{
				Log.Info("Attack modifier Expression String: " + text + "\tResult: " + result);
			}
			return result;
		}

		public void DrawAttackModifiers(CActor actor, TargetSummary summary, CAbilityAttack attackAbility, int attackIndex, out int addTargetsDrawn)
		{
			Log.SimpleInfo("Drawing attack modifiers. OverallAdvantage status is: " + OverallAdvantage);
			CActor actorToAttack = summary.ActorToAttack;
			GameState.DrawAndApplyAttackModifierCards(attackAbility, OriginalAbility as CAbilityAttack, actor, summary, m_PreModifierAttackStrength, OverallAdvantage, out m_UsedAttackMods, out m_NotUsedAttackMods, out var outPierce, out var outAttackModifierEvent, out addTargetsDrawn, attackIndex, out m_DrawnAttackMods);
			int pierce = Pierce;
			Pierce += outPierce;
			CacheFinalAttackStrength(initial: false, beforeModifiers: false);
			IsTargeted = true;
			Actor.IncomingAttackDamage = CalcExpressionString(calcPostShieldScalars: true, calcShield: true);
			PrepareDamageIfTargetMovesAbility(actor);
			outAttackModifierEvent.TargetActorShield = Shield;
			outAttackModifierEvent.DrawingActorPierce = pierce;
			outAttackModifierEvent.FinalPierce = Pierce;
			outAttackModifierEvent.FinalAttackStrength = Actor.IncomingAttackDamage;
			outAttackModifierEvent.AttackAbilityAttackStrength = m_InitialAbilityAttackStrength;
			outAttackModifierEvent.OverrideBuffs = m_eventOverrideBuffData;
			outAttackModifierEvent.TargetActorWasPoisoned = Poison;
			outAttackModifierEvent.ExploitedVulnerabilityStrength = ExploitedVulnerabilityStrength;
			outAttackModifierEvent.TargetActorWasInvulnerable = Invulnerable;
			outAttackModifierEvent.TargetActorHasWard = Ward;
			outAttackModifierEvent.TargetActorHasBrittle = Brittle;
			AttackModifierEvent = outAttackModifierEvent;
			CLogAttackModifier_MessageData message = new CLogAttackModifier_MessageData(attackAbility.TargetingActor)
			{
				m_Ability = attackAbility,
				m_ActorBeingAttacked = actorToAttack,
				m_AttackingActor = attackAbility.TargetingActor,
				m_AttackEvent = attackAbility.GetLogEvent(ESESubTypeAbility.AbilityPerform),
				m_ModifierEvent = outAttackModifierEvent
			};
			ScenarioRuleClient.MessageHandler(message);
		}

		private CActiveBonus FindDamageIfMoveAddActiveBonus()
		{
			return ActorToAttack.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnMoved).SingleOrDefault((CActiveBonus x) => x.Ability.Name == "DamageIfMoveAbility");
		}

		public void PrepareDamageIfTargetMovesAbility(CActor targetingActor)
		{
			DamageIfTargetMoves = 0;
			foreach (AttackModifierYMLData usedAttackMod in m_UsedAttackMods)
			{
				if (usedAttackMod.DamageIfTargetMoves.HasValue)
				{
					DamageIfTargetMoves += usedAttackMod.DamageIfTargetMoves.Value;
				}
			}
			if (DamageIfTargetMoves <= 0)
			{
				return;
			}
			if (!(FindDamageIfMoveAddActiveBonus()?.Ability is CAbilityAddActiveBonus { AddAbility: var addAbility }))
			{
				CAbility cAbility = CAbilityDamage.CreateDefaultDamage(DamageIfTargetMoves, 0, 1, isMonster: true, "", isMultipass: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				CAbilityAddActiveBonus cAbilityAddActiveBonus2 = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
				{
					Duration = CActiveBonus.EActiveBonusDurationType.Ability,
					Behaviour = CActiveBonus.EActiveBonusBehaviourType.DuringActionAbilityOnMoved,
					Hidden = true,
					Restriction = CActiveBonus.EActiveBonusRestrictionType.OncePerAction,
					Tracker = new List<int> { 0 }
				}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: "DamageIfMoveAbility", abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: cAbility, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue) as CAbilityAddActiveBonus;
				cAbilityAddActiveBonus2.Start(targetingActor, ActorToAttack);
				cAbilityAddActiveBonus2.ApplyToActor(ActorToAttack);
				if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility == null)
				{
					return;
				}
				CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
				currentPhaseAbility.m_PostEndAction = (Action)Delegate.Combine(currentPhaseAbility.m_PostEndAction, (Action)(() =>
				{
					CActiveBonus cActiveBonus = FindDamageIfMoveAddActiveBonus();
					if (cActiveBonus != null && !cActiveBonus.Finished())
					{
						cActiveBonus.Finish();
					}
				}));
			}
			else
			{
				addAbility.Strength += DamageIfTargetMoves;
			}
		}

		public void FinishDrawingModifiers()
		{
			State = SummaryState.FinishedDrawingModifiers;
			CActorFinishedDrawingModifiers_MessageData message = new CActorFinishedDrawingModifiers_MessageData(DamageSourceActor)
			{
				m_TargetSummary = this
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public int ModifiedAttackStrength { get; private set; }

	public CAbilityAttack AttackAbility { get; private set; }

	public List<TargetSummary> Targets { get; private set; }

	public List<CActiveBonus> AttackActiveBonuses { get; private set; }

	public List<CActiveBonus> AddTargetActiveBonuses { get; private set; }

	public List<CActiveBonus> AddRangeActiveBonuses { get; private set; }

	public int ActiveBonusAddTargetBuff { get; private set; }

	public int ActiveBonusAddRangeBuff { get; private set; }

	public List<ConsumeData> StrengthBuffConsumes { get; private set; }

	public int ConsumeStrengthBuff { get; private set; }

	public int ConsumeRangeBuff { get; private set; }

	public int ConsumeTargetsBuff { get; private set; }

	public int ItemOverrideStrengthBuff { get; private set; }

	public CAttackSummary()
	{
	}

	public CAttackSummary(CAbility ability, bool useBothLists = false)
	{
		Log.Info("[ATTACK SUMMARY] Created new AttackSummary for Ability Type:" + ability.AbilityType);
		if (ability is CAbilityAttack attackAbility)
		{
			AttackAbility = attackAbility;
		}
		ModifiedAttackStrength = ability.Strength;
		StrengthBuffConsumes = new List<ConsumeData>();
		foreach (CActionAugmentation currentActionSelectedAugmentation in GameState.CurrentActionSelectedAugmentations)
		{
			bool flag = false;
			ConsumeStrengthBuff = 0;
			ConsumeRangeBuff = 0;
			ConsumeTargetsBuff = 0;
			foreach (CActionAugmentationOp augmentationOp in currentActionSelectedAugmentation.AugmentationOps)
			{
				if (augmentationOp.ParentAbilityName == ability.Name && augmentationOp.Type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
				{
					if (augmentationOp.AbilityOverride.Strength.HasValue)
					{
						ConsumeStrengthBuff += augmentationOp.AbilityOverride.Strength.Value;
						flag = true;
					}
					if (augmentationOp.AbilityOverride.Range.HasValue)
					{
						ConsumeRangeBuff += augmentationOp.AbilityOverride.Range.Value;
					}
					if (augmentationOp.AbilityOverride.NumberOfTargets.HasValue)
					{
						ConsumeTargetsBuff += augmentationOp.AbilityOverride.NumberOfTargets.Value;
					}
				}
			}
			if (flag)
			{
				StrengthBuffConsumes.Add(new ConsumeData(ConsumeStrengthBuff, currentActionSelectedAugmentation.Elements));
			}
		}
		AttackActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.Attack)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		AddTargetActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.AddTarget)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		AddRangeActiveBonuses = (from w in CActiveBonus.FindApplicableActiveBonuses(ability.TargetingActor, CAbility.EAbilityType.AddRange)
			where w.Ability.Augment == null && w.RequirementsMet(ability.TargetingActor)
			select w).ToList();
		ActiveBonusAddTargetBuff = 0;
		if (AddTargetActiveBonuses != null)
		{
			ActiveBonusAddTargetBuff = AddTargetActiveBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(ability, null));
		}
		ActiveBonusAddRangeBuff = 0;
		if (AddRangeActiveBonuses != null)
		{
			ActiveBonusAddRangeBuff = AddRangeActiveBonuses.Sum((CActiveBonus x) => x.ReferenceStrength(ability, null));
		}
		foreach (CItem activeOverrideItem in ability.ActiveOverrideItems)
		{
			ItemOverrideStrengthBuff = 0;
			foreach (CAbilityOverride @override in activeOverrideItem.YMLData.Data.Overrides)
			{
				if (@override.Strength.HasValue)
				{
					ItemOverrideStrengthBuff += @override.Strength.Value;
				}
			}
		}
		UpdateTargetData(ability, null, null, useActorsToTarget: false, useBothLists, useBonusTracker: true);
	}

	public CAttackSummary Copy()
	{
		Log.Info("[ATTACK SUMMARY] Copying Attack Summary");
		return new CAttackSummary
		{
			ModifiedAttackStrength = ModifiedAttackStrength,
			AttackAbility = AttackAbility,
			Targets = Targets.ToList(),
			AttackActiveBonuses = AttackActiveBonuses.ToList(),
			AddTargetActiveBonuses = AddTargetActiveBonuses.ToList(),
			AddRangeActiveBonuses = AddRangeActiveBonuses.ToList(),
			ActiveBonusAddTargetBuff = ActiveBonusAddTargetBuff,
			ActiveBonusAddRangeBuff = ActiveBonusAddRangeBuff,
			StrengthBuffConsumes = StrengthBuffConsumes.ToList(),
			ConsumeStrengthBuff = ConsumeStrengthBuff,
			ConsumeRangeBuff = ConsumeRangeBuff,
			ConsumeTargetsBuff = ConsumeTargetsBuff,
			ItemOverrideStrengthBuff = ItemOverrideStrengthBuff
		};
	}

	public void UpdateTargetData(CAbility ability, List<CItem> activeSingleTargetItems, List<CActiveBonus> activeSingleTargetBonuses, bool useActorsToTarget = false, bool useBothLists = false, bool useBonusTracker = false)
	{
		Log.Info("[ATTACK SUMMARY] Updating Target Summary Data");
		if (Targets == null)
		{
			Targets = new List<TargetSummary>();
		}
		else
		{
			Targets.Clear();
			Log.Info("[ATTACK SUMMARY] Cleared all Target Summarys");
		}
		_ = ability.TargetingActor.Type;
		List<CActor> list = ability.ValidActorsInRange;
		if (ability.AreaEffect != null)
		{
			list = new List<CActor>();
			foreach (CTile item in ability.ValidTilesInAreaAffected)
			{
				CAreaEffect.EHexFlag eHexFlag = ability.FlagsOfValidTiles[item];
				if (!eHexFlag.HasFlag(CAreaEffect.EHexFlag.Enemy) || !eHexFlag.HasFlag(CAreaEffect.EHexFlag.Enabled))
				{
					continue;
				}
				foreach (CActor livingActor in item.LivingActors)
				{
					CAbilityFilterContainer abilityFilter = ability.AbilityFilter;
					if (abilityFilter == null || abilityFilter.IsValidTarget(livingActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, false))
					{
						list.Add(livingActor);
					}
				}
			}
		}
		List<CActor> list2;
		if (useBothLists)
		{
			list2 = ability.ActorsToTarget.Union(list).ToList();
		}
		else
		{
			list2 = (useActorsToTarget ? ability.ActorsToTarget : list);
		}
		List<CActor> list3 = list2.ToList();
		int num = 1;
		foreach (CActor item2 in list3)
		{
			if (!ability.IsMirroredHealthCloneTargeted(item2))
			{
				Targets.Add(new TargetSummary(this, item2, ability, num));
				Log.Info("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + item2.ActorGuid);
				if (useBonusTracker)
				{
					num++;
				}
			}
		}
		if (!(ability is CAbilityAttack { MultiPassAttack: not false, MultiPassActors: not null, MultiPassActors: var multiPassActors }))
		{
			return;
		}
		List<CActor> list4 = multiPassActors.OrderBy((CActor actor) => actor.ID).ToList();
		num = 1;
		foreach (CActor item3 in list4)
		{
			if (!list2.Contains(item3))
			{
				Targets.Add(new TargetSummary(this, item3, ability, num));
				Log.Info("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + item3.ActorGuid);
				ability.ApplySingleTargetActiveBonus(item3);
				ability.ApplySingleTargetItem(item3);
				if (useBonusTracker)
				{
					num++;
				}
			}
		}
	}

	public void AutoToggleSingleTargetBonuses()
	{
		if (AttackAbility == null || AttackAbility.AttackSummary == null || AttackAbility.ActorsToTarget.Count != 1)
		{
			return;
		}
		CActor target = AttackAbility.ActorsToTarget[0];
		foreach (CActiveBonus item in AttackActiveBonuses.Where((CActiveBonus x) => x.Ability.ActiveBonusData.IsSingleTargetBonus && !x.ToggledBonus && !x.IsRestricted(target) && x.Ability.ActiveBonusData.GetVar("AutoToggle").GetBool()).ToList())
		{
			item.ToggleActiveBonus(null, AttackAbility.TargetingActor);
		}
	}

	public void UpdateSingleTargetData(CAbilityAttack attackAbility, CActor actor, bool force = false)
	{
		Log.Info("[ATTACK SUMMARY] Updating Single Target Summary Data");
		int targetIndex = 0;
		if (FindTarget(actor, ref targetIndex) == null)
		{
			Targets.Add(new TargetSummary(this, actor, attackAbility));
			Log.Info("[ATTACK SUMMARY] Created new Target Summary for Actor Guid: " + actor.ActorGuid);
		}
		else if (force)
		{
			TargetSummary targetSummary = Targets[targetIndex - 1];
			TargetSummary value = new TargetSummary(this, actor, attackAbility, targetSummary.TargetNumber);
			Targets[targetIndex - 1] = value;
		}
	}

	public void RefreshingTargetDataForMultipassAttack(CAbilityAttack attackAbility, CActor actor)
	{
		Log.Info("[ATTACK SUMMARY] Refreshing Target Summary data for multipass attack for Actor Guid: " + actor.ActorGuid);
		int num = Targets.FindIndex((TargetSummary x) => x.Actor == actor);
		if (num > -1)
		{
			TargetSummary targetSummary = Targets[num];
			Targets[num] = new TargetSummary(this, targetSummary.Actor, attackAbility)
			{
				ItemOverrideAbility = targetSummary.ItemOverrideAbility,
				RedirectedFromActor = targetSummary.RedirectedFromActor,
				IsTargeted = targetSummary.IsTargeted,
				UsedAttackMods = targetSummary.UsedAttackMods,
				NotUsedAttackMods = targetSummary.NotUsedAttackMods,
				DrawnAttackMods = targetSummary.DrawnAttackMods,
				State = targetSummary.State,
				PreShieldScalarModifiers = targetSummary.PreShieldScalarModifiers?.ToList(),
				NonScalarMathModifiers = targetSummary.NonScalarMathModifiers?.ToList(),
				PostShieldScalarModifiers = targetSummary.PostShieldScalarModifiers?.ToList(),
				AdditionalNonScalarModifiers = targetSummary.AdditionalNonScalarModifiers?.ToList(),
				ActualDamageTaken = targetSummary.ActualDamageTaken
			};
			if (GameState.LastTargetSummary == targetSummary)
			{
				GameState.LastTargetSummary = Targets[num];
			}
			if (attackAbility.CurrentAttackingTargetSummary == targetSummary)
			{
				attackAbility.CurrentAttackingTargetSummary = Targets[num];
			}
			int num2 = attackAbility.TotalTargetSummaries.IndexOf(targetSummary);
			if (num2 != -1)
			{
				attackAbility.TotalTargetSummaries[num2] = Targets[num];
			}
		}
	}

	public void RefreshingTargetDataAfterToggleItemOrActiveBonus(CAbilityAttack attackAbility, CActor actor, TargetSummary currentAttackingTargetSummary)
	{
		if (currentAttackingTargetSummary.State != TargetSummary.SummaryState.SelectingTargets && currentAttackingTargetSummary.State != TargetSummary.SummaryState.FinishedDrawingModifiers)
		{
			return;
		}
		Log.Info("[ATTACK SUMMARY] Refreshing Target Summary data for active bonus toggled for Actor Guid: " + actor.ActorGuid);
		int num = Targets.FindIndex((TargetSummary x) => x.ActorToAttack == currentAttackingTargetSummary.ActorToAttack && x.State == currentAttackingTargetSummary.State);
		if (num <= -1)
		{
			return;
		}
		TargetSummary targetSummary = Targets[num];
		Targets[num] = new TargetSummary(this, targetSummary.Actor, attackAbility)
		{
			ItemOverrideAbility = targetSummary.ItemOverrideAbility,
			RedirectedFromActor = targetSummary.RedirectedFromActor,
			IsTargeted = targetSummary.IsTargeted,
			UsedAttackMods = targetSummary.UsedAttackMods,
			NotUsedAttackMods = targetSummary.NotUsedAttackMods,
			DrawnAttackMods = targetSummary.DrawnAttackMods,
			State = targetSummary.State,
			PreShieldScalarModifiers = targetSummary.PreShieldScalarModifiers?.ToList(),
			NonScalarMathModifiers = targetSummary.NonScalarMathModifiers?.ToList(),
			PostShieldScalarModifiers = targetSummary.PostShieldScalarModifiers?.ToList(),
			AdditionalNonScalarModifiers = targetSummary.AdditionalNonScalarModifiers?.ToList(),
			ActualDamageTaken = targetSummary.ActualDamageTaken
		};
		if (GameState.LastTargetSummary == targetSummary)
		{
			GameState.LastTargetSummary = Targets[num];
		}
		if (attackAbility.CurrentAttackingTargetSummary == targetSummary)
		{
			attackAbility.CurrentAttackingTargetSummary = Targets[num];
		}
		int num2 = attackAbility.TotalTargetSummaries.IndexOf(targetSummary);
		if (num2 != -1)
		{
			attackAbility.TotalTargetSummaries[num2] = Targets[num];
		}
		if (attackAbility.CurrentAttackingTargetSummary.UsedAttackMods == null || attackAbility.CurrentAttackingTargetSummary.UsedAttackMods.Count <= 0)
		{
			return;
		}
		foreach (AttackModifierYMLData usedAttackMod in attackAbility.CurrentAttackingTargetSummary.UsedAttackMods)
		{
			GameState.ApplyModifierToAttack(usedAttackMod, attackAbility.CurrentAttackingTargetSummary);
		}
	}

	public TargetSummary FindTarget(CActor actor, ref int targetIndex)
	{
		if (Targets.Count((TargetSummary x) => x.Actor.ActorGuid == actor.ActorGuid) > 1)
		{
			if (targetIndex < Targets.Count)
			{
				int? num = Targets.Skip(targetIndex).Select((TargetSummary summary, int index) => new { summary, index }).First(x => x.summary.Actor.ActorGuid == actor.ActorGuid)
					.index;
				if (num.HasValue)
				{
					targetIndex += num.Value;
					if (targetIndex < Targets.Count)
					{
						TargetSummary result = Targets[targetIndex];
						targetIndex++;
						return result;
					}
				}
			}
			return null;
		}
		targetIndex++;
		return Targets.SingleOrDefault((TargetSummary x) => x.Actor.ActorGuid == actor.ActorGuid);
	}

	public TargetSummary FindTarget(CActor actor)
	{
		int targetIndex = 0;
		return FindTarget(actor, ref targetIndex);
	}

	public TargetSummary FindTargetAfterRedirect(CActor actorOriginallyAttacked, CActor actorRedirectedTo)
	{
		if (actorRedirectedTo == null || actorRedirectedTo == actorOriginallyAttacked)
		{
			return Targets.FirstOrDefault((TargetSummary x) => x.RedirectedFromActor == null && x.ActorToAttack == actorOriginallyAttacked);
		}
		return Targets.FirstOrDefault((TargetSummary x) => x.RedirectedFromActor == actorOriginallyAttacked && x.ActorToAttack == actorRedirectedTo);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using FH.Tools;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class CAttackEffect : ISerializable
{
	[Serializable]
	[KronosEnumPolice(1)]
	public enum EAttackEffect
	{
		None,
		StandardBuff,
		BuffForEachNegativeConditionOnTarget,
		BuffForEachPositiveConditionOnTarget,
		BuffForEachConditionOnTarget,
		BuffIfAnyConditionsOnTarget,
		BuffIfCanPerformSubAbility,
		BuffIfAnItemUsedInAction,
		BuffIfFormIsRanged,
		BuffIfAttackIsMelee,
		BuffIfAttackIsRanged,
		BasedOnVarsCondition,
		BasedOnActiveBonusVarsStrength,
		SkipAttackModifiers,
		FirstTargetOnly,
		SingleTarget
	}

	public static readonly EAttackEffect[] AttackEffects = (EAttackEffect[])Enum.GetValues(typeof(EAttackEffect));

	private bool stackedSubAbilityForBuff;

	private CActor actorTargetedForStackedSubAbilityBuff;

	[JsonProperty]
	public EAttackEffect Effect { get; private set; }

	[JsonProperty]
	public int Strength { get; private set; }

	[JsonProperty]
	public int XP { get; private set; }

	[JsonProperty]
	public int Pierce { get; private set; }

	[JsonProperty]
	public int Range { get; private set; }

	[JsonProperty]
	public int TargetsCount { get; private set; }

	[JsonProperty]
	public CAbilityFilter Filter { get; private set; }

	[JsonProperty]
	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	[JsonProperty]
	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	[JsonProperty]
	public CAbility.EAttackType AttackType { get; private set; }

	[JsonProperty]
	public List<CAbility> SubAbilities { get; private set; }

	[JsonProperty]
	public CAbilityFilter TargetingActorFilter { get; private set; }

	[JsonProperty]
	public List<(string, TValue)> Vars { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Effect", Effect);
		info.AddValue("Strength", Strength);
		info.AddValue("XP", XP);
		info.AddValue("Pierce", Pierce);
		info.AddValue("Range", Range);
		info.AddValue("TargetsCount", TargetsCount);
		info.AddValue("Filter", Filter);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("AttackType", AttackType);
		info.AddValue("SubAbilities", SubAbilities.Select((CAbility x) => CAbility.ToJson(x, force: true)).ToList());
		info.AddValue("TargetingActorFilter", TargetingActorFilter);
	}

	public CAttackEffect(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "Effect":
					Effect = (EAttackEffect)info.GetValue("Effect", typeof(EAttackEffect));
					break;
				case "Strength":
					Strength = info.GetInt32("Strength");
					break;
				case "XP":
					XP = info.GetInt32("XP");
					break;
				case "Pierce":
					Pierce = info.GetInt32("Pierce");
					break;
				case "Range":
					Range = info.GetInt32("Range");
					break;
				case "TargetsCount":
					TargetsCount = info.GetInt32("TargetsCount");
					break;
				case "Filter":
					Filter = (CAbilityFilter)info.GetValue("Filter", typeof(CAbilityFilter));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "AttackType":
					AttackType = (CAbility.EAttackType)info.GetValue("AttackType", typeof(CAbility.EAttackType));
					break;
				case "SubAbilities":
				{
					List<string> source = (List<string>)info.GetValue("SubAbilities", typeof(List<string>));
					SubAbilities = source.Select((string x) => CAbility.FromJson(x)).ToList();
					break;
				}
				case "TargetingActorFilter":
					TargetingActorFilter = (CAbilityFilter)info.GetValue("TargetingActorFilter", typeof(CAbilityFilter));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CAttackEffect entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[JsonConstructor]
	public CAttackEffect(EAttackEffect effect, int strength, int xp, int pierce, int range, int targetsCount, CAbilityFilter filter, List<CCondition.ENegativeCondition> negCons, List<CCondition.EPositiveCondition> posCons, CAbility.EAttackType attackType, List<CAbility> subAbilities, CAbilityFilter targetingActorFilter, List<(string, TValue)> vars)
	{
		Effect = effect;
		Strength = strength;
		XP = xp;
		Pierce = pierce;
		Range = range;
		TargetsCount = targetsCount;
		Filter = filter;
		NegativeConditions = negCons ?? new List<CCondition.ENegativeCondition>();
		PositiveConditions = posCons ?? new List<CCondition.EPositiveCondition>();
		AttackType = attackType;
		SubAbilities = subAbilities;
		TargetingActorFilter = targetingActorFilter;
		Vars = vars;
	}

	private void CalcVars(CActor actor, CActor target, CAbility ability, out List<CCondition.EPositiveCondition> positiveConditions, out List<CCondition.ENegativeCondition> negativeConditions)
	{
		positiveConditions = null;
		negativeConditions = null;
		if (Vars == null)
		{
			return;
		}
		CVars cVars = new CVars();
		CVarsContext context = new CVarsContext(cVars);
		cVars.Set("Actor", new TValue(actor));
		cVars.Set("Target", new TValue(target));
		cVars.Set("Ability", new TValue(ability));
		foreach (var var in Vars)
		{
			string item = var.Item1;
			TValue item2 = var.Item2;
			TValue value = item2.Calc(context);
			cVars.Set(item, value);
			if (!(item == "AddConditions"))
			{
				continue;
			}
			positiveConditions = new List<CCondition.EPositiveCondition>();
			negativeConditions = new List<CCondition.ENegativeCondition>();
			foreach (string item3 in value.GetObject<IList>())
			{
				CCondition.ENegativeCondition result2;
				if (Enum.TryParse<CCondition.EPositiveCondition>(item3, out var result))
				{
					positiveConditions.Add(result);
				}
				else if (Enum.TryParse<CCondition.ENegativeCondition>(item3, out result2) && result2 != CCondition.ENegativeCondition.Disadvantage)
				{
					negativeConditions.Add(result2);
				}
			}
		}
	}

	public void GetBonus(CActor target, CAbilityAttack attackAbility, out int attackBuff, out int xpBuff, out int rangeBuff, out int targetsCountBuff, out int pierceBuff, out bool skipAttackModifiers, out List<CCondition.EPositiveCondition> positiveConditionsBuff, out List<CCondition.ENegativeCondition> negativeConditionsBuff, CActiveBonus activeBonus = null)
	{
		attackBuff = 0;
		xpBuff = 0;
		rangeBuff = 0;
		targetsCountBuff = 0;
		pierceBuff = 0;
		skipAttackModifiers = false;
		positiveConditionsBuff = null;
		negativeConditionsBuff = null;
		if ((attackAbility != null && attackAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA) || (attackAbility != null && AttackType == CAbility.EAttackType.Melee && !attackAbility.IsMeleeAttack) || (attackAbility != null && AttackType == CAbility.EAttackType.Ranged && attackAbility.IsMeleeAttack) || (Effect == EAttackEffect.FirstTargetOnly && (attackAbility?.AreaEffect != null || (attackAbility != null && attackAbility.ActorsToTarget.Count > 0 && (target != attackAbility.ActorsToTarget[0] || target == attackAbility.TargetingActor)))) || (Effect == EAttackEffect.SingleTarget && (attackAbility?.AreaEffect != null || (attackAbility != null && attackAbility.OriginalTargetCount != 1))))
		{
			return;
		}
		bool flag = attackAbility == null || Filter.IsValidTarget(target, attackAbility.TargetingActor, attackAbility.IsTargetedAbility, useTargetOriginalType: false, attackAbility.MiscAbilityData?.CanTargetInvisible);
		if (TargetingActorFilter != null)
		{
			flag &= attackAbility == null || TargetingActorFilter.IsValidTarget(attackAbility.TargetingActor, attackAbility.TargetingActor, attackAbility.IsTargetedAbility, useTargetOriginalType: false, true);
		}
		switch (Effect)
		{
		case EAttackEffect.FirstTargetOnly:
			if (flag)
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			rangeBuff += Range;
			targetsCountBuff += TargetsCount;
			pierceBuff += Pierce;
			if (PositiveConditions.Count > 0)
			{
				positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
			}
			if (NegativeConditions.Count > 0)
			{
				negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
			}
			break;
		case EAttackEffect.StandardBuff:
		case EAttackEffect.SingleTarget:
			if (flag)
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			rangeBuff += Range;
			targetsCountBuff += TargetsCount;
			pierceBuff += Pierce;
			if (PositiveConditions.Count > 0)
			{
				positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
			}
			if (NegativeConditions.Count > 0)
			{
				negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
			}
			break;
		case EAttackEffect.BuffForEachNegativeConditionOnTarget:
			if (flag)
			{
				attackBuff += target.Tokens.GetAllNegativeConditions().Count * Strength;
				xpBuff += target.Tokens.GetAllNegativeConditions().Count * XP;
			}
			pierceBuff += Pierce;
			break;
		case EAttackEffect.BuffForEachPositiveConditionOnTarget:
			if (flag)
			{
				attackBuff += target.Tokens.GetAllPositiveConditions().Count * Strength;
				xpBuff += target.Tokens.GetAllPositiveConditions().Count * XP;
			}
			pierceBuff += Pierce;
			break;
		case EAttackEffect.BuffForEachConditionOnTarget:
		{
			int num = 0;
			if (NegativeConditions.Count > 0)
			{
				num = target.Tokens.CheckNegativeTokens.Count((NegativeConditionPair w) => NegativeConditions.Contains(w.NegativeCondition));
			}
			if (PositiveConditions.Count > 0)
			{
				num += target.Tokens.CheckPositiveTokens.Count((PositiveConditionPair w) => PositiveConditions.Contains(w.PositiveCondition));
			}
			if (flag)
			{
				attackBuff += num * Strength;
				xpBuff += num * XP;
			}
			pierceBuff += Pierce;
			break;
		}
		case EAttackEffect.BuffIfAnyConditionsOnTarget:
			if (NegativeConditions.Count > 0 && target.Tokens.CheckNegativeTokens.Any((NegativeConditionPair a) => NegativeConditions.Contains(a.NegativeCondition)))
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				pierceBuff += Pierce;
			}
			else if (PositiveConditions.Count > 0 && target.Tokens.CheckPositiveTokens.Any((PositiveConditionPair a) => PositiveConditions.Contains(a.PositiveCondition)))
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				pierceBuff += Pierce;
			}
			break;
		case EAttackEffect.BuffIfCanPerformSubAbility:
			if (stackedSubAbilityForBuff && target.Equals(actorTargetedForStackedSubAbilityBuff))
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				pierceBuff += Pierce;
			}
			break;
		case EAttackEffect.BuffIfAnItemUsedInAction:
		{
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (attackAbility != null && (attackAbility.ActiveOverrideItems.Count > 0 || attackAbility.ActiveSingleTargetItems.Count > 0 || cPhaseAction.ItemsUsedThisPhase.Count > 0 || GameState.InternalCurrentActor.Inventory.SelectedItems.Count > 0 || attackAbility.AttackSummary?.AttackActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null || attackAbility.AttackSummary?.AddTargetActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null || attackAbility.AttackSummary?.AddRangeActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null))
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				rangeBuff += Range;
				targetsCountBuff += TargetsCount;
				pierceBuff += Pierce;
				if (PositiveConditions.Count > 0)
				{
					positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
				}
				if (NegativeConditions.Count > 0)
				{
					negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
				}
			}
			break;
		}
		case EAttackEffect.BuffIfFormIsRanged:
			if (attackAbility != null && attackAbility.TargetingActor is CPlayerActor { Form: CPlayerActor.EForm.Range })
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				rangeBuff += Range;
				targetsCountBuff += TargetsCount;
				pierceBuff += Pierce;
				if (PositiveConditions.Count > 0)
				{
					positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
				}
				if (NegativeConditions.Count > 0)
				{
					negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
				}
			}
			break;
		case EAttackEffect.BuffIfAttackIsMelee:
			if (attackAbility != null && attackAbility.IsMeleeAttack)
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				rangeBuff += Range;
				targetsCountBuff += TargetsCount;
				pierceBuff += Pierce;
				if (PositiveConditions.Count > 0)
				{
					positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
				}
				if (NegativeConditions.Count > 0)
				{
					negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
				}
			}
			break;
		case EAttackEffect.BuffIfAttackIsRanged:
			if (attackAbility != null && !attackAbility.IsMeleeAttack)
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				rangeBuff += Range;
				targetsCountBuff += TargetsCount;
				pierceBuff += Pierce;
				if (PositiveConditions.Count > 0)
				{
					positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
				}
				if (NegativeConditions.Count > 0)
				{
					negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
				}
			}
			break;
		case EAttackEffect.BasedOnVarsCondition:
			if (activeBonus == null)
			{
				activeBonus = CActiveBonus.FindApplicableActiveBonuses(attackAbility?.TargetingActor, CAbility.EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack).FirstOrDefault();
			}
			if (activeBonus != null && activeBonus.SetupTriggerContext(null, null, attackAbility, attackAbility?.TargetingActor, target))
			{
				if (flag)
				{
					attackBuff += Strength;
					xpBuff += XP;
				}
				rangeBuff += Range;
				targetsCountBuff += TargetsCount;
				pierceBuff += Pierce;
				if (PositiveConditions.Count > 0)
				{
					positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
				}
				if (NegativeConditions.Count > 0)
				{
					negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
				}
			}
			break;
		case EAttackEffect.BasedOnActiveBonusVarsStrength:
			if (flag)
			{
				if (activeBonus == null)
				{
					activeBonus = CActiveBonus.FindApplicableActiveBonuses(attackAbility?.TargetingActor, CAbility.EAbilityType.Attack, CActiveBonus.EActiveBonusBehaviourType.BuffAttack).FirstOrDefault();
				}
				if (activeBonus != null)
				{
					List<(string, TValue)> list = activeBonus?.Ability?.ActiveBonusData?.Vars;
					TValue tValue = list?.Find(((string, TValue) pair) => pair.Item1 == "Strength").Item2 ?? TValue.Unknown;
					if (tValue != TValue.Unknown)
					{
						int val = list.Find(((string, TValue) pair) => pair.Item1 == "TargetSelectionNumber").Item2.GetInt();
						CVars cVars = new CVars();
						CVarsContext context = new CVarsContext(cVars);
						cVars.Set("obj", new TValue(activeBonus));
						cVars.Set("TargetSelectionNumber", new TValue(val));
						attackBuff += tValue.Calc(context).GetInt();
					}
				}
				attackBuff += Strength;
				xpBuff += XP;
			}
			rangeBuff += Range;
			targetsCountBuff += TargetsCount;
			pierceBuff += Pierce;
			if (PositiveConditions.Count > 0)
			{
				positiveConditionsBuff = new List<CCondition.EPositiveCondition>(PositiveConditions);
			}
			if (NegativeConditions.Count > 0)
			{
				negativeConditionsBuff = new List<CCondition.ENegativeCondition>(NegativeConditions);
			}
			break;
		case EAttackEffect.SkipAttackModifiers:
			skipAttackModifiers = true;
			break;
		default:
			Log.ErrorWithReport("Unrecognised Attack Effect Type " + Effect);
			break;
		}
		CalcVars(attackAbility?.TargetingActor, target, attackAbility, out var positiveConditions, out var negativeConditions);
		if (positiveConditions != null)
		{
			if (positiveConditionsBuff == null)
			{
				positiveConditionsBuff = new List<CCondition.EPositiveCondition>();
			}
			positiveConditionsBuff.AddRange(positiveConditions);
		}
		if (negativeConditions != null)
		{
			if (negativeConditionsBuff == null)
			{
				negativeConditionsBuff = new List<CCondition.ENegativeCondition>();
			}
			negativeConditionsBuff.AddRange(negativeConditions);
		}
	}

	public bool StackAbilityForBonus(CActor target, CAbilityAttack attackAbility, List<CAbility> stackAbilities)
	{
		if (attackAbility != null && AttackType == CAbility.EAttackType.Melee && !attackAbility.IsMeleeAttack)
		{
			return false;
		}
		if (attackAbility != null && AttackType == CAbility.EAttackType.Ranged && attackAbility.IsMeleeAttack)
		{
			return false;
		}
		stackedSubAbilityForBuff = false;
		if (Effect.Equals(EAttackEffect.BuffIfCanPerformSubAbility))
		{
			bool flag = true;
			List<CTile> list = new List<CTile>();
			foreach (CAbility stackAbility in stackAbilities)
			{
				bool flag2 = false;
				switch (stackAbility.AbilityType)
				{
				case CAbility.EAbilityType.DestroyObstacle:
					foreach (CTile item in GameState.GetTilesInRange(attackAbility?.TargetingActor, stackAbility.Range, stackAbility.Targeting, emptyTilesOnly: false, ignoreBlocked: true))
					{
						CTile propTile = null;
						CObjectObstacle cObjectObstacle = item.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
						if (cObjectObstacle == null)
						{
							cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile);
						}
						if (cObjectObstacle != null && !cObjectObstacle.OverrideDisallowDestroyAndMove && !cObjectObstacle.Activated && (cObjectObstacle.PropHealthDetails == null || !cObjectObstacle.PropHealthDetails.HasHealth))
						{
							flag2 = true;
							list.Add(item);
						}
					}
					break;
				case CAbility.EAbilityType.AddActiveBonus:
					flag2 = true;
					list.AddRange(attackAbility?.ActorsToTarget.Select((CActor actor) => actor.Tile));
					break;
				default:
					Log.ErrorWithReport("Unrecognised Ability Type to perform for buff " + Effect);
					break;
				}
				flag = flag2;
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(stackAbilities, null, performNow: true, stopPlayerSkipping: true, false, stopPlayerUndo: true);
				stackedSubAbilityForBuff = true;
				actorTargetedForStackedSubAbilityBuff = target;
				if (list.Count == 1)
				{
					PhaseManager.TileSelected(list[0], new List<CTile>());
				}
			}
		}
		return stackedSubAbilityForBuff;
	}

	public void StackSubAbilities(CAbilityAttack attackAbility)
	{
		if ((attackAbility != null && AttackType == CAbility.EAttackType.Melee && !attackAbility.IsMeleeAttack) || (attackAbility != null && AttackType == CAbility.EAttackType.Ranged && attackAbility.IsMeleeAttack) || SubAbilities == null)
		{
			return;
		}
		List<CAbility> list = SubAbilities.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false)).ToList();
		foreach (CAbility item in list)
		{
			if (item.IsInlineSubAbility && attackAbility.AbilityFilter != null)
			{
				item.AbilityFilter = attackAbility.AbilityFilter.Copy();
			}
		}
		attackAbility.MergeOrAddAbilityOfType(CAbility.EAbilityType.Push, list);
		attackAbility.MergeOrAddAbilityOfType(CAbility.EAbilityType.Pull, list);
		foreach (CAbility item2 in list)
		{
			attackAbility.AddSubAbility(item2, item2.IsInlineSubAbility);
		}
	}

	public void UpdateStackedAbilityForBonusTarget(CActor target)
	{
		actorTargetedForStackedSubAbilityBuff = target;
	}

	public void ResetStackedSubAbilityForBuffOnAttack()
	{
		stackedSubAbilityForBuff = false;
	}
}

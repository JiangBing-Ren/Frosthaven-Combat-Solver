using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CTokens
{
	private volatile List<PositiveConditionPair> PositiveTokens;

	private volatile List<NegativeConditionPair> NegativeTokens;

	public CActor Actor { get; private set; }

	public CCondition.EPositiveCondition ModifiedPositiveCondition { get; set; }

	public CCondition.ENegativeCondition ModifiedNegativeCondition { get; set; }

	public List<PositiveConditionPair> CheckPositiveTokens
	{
		get
		{
			lock (PositiveTokens)
			{
				return PositiveTokens.ToList();
			}
		}
	}

	public List<NegativeConditionPair> CheckNegativeTokens
	{
		get
		{
			lock (NegativeTokens)
			{
				return NegativeTokens.ToList();
			}
		}
	}

	public CTokens(CActor actor)
	{
		Actor = actor;
		PositiveTokens = new List<PositiveConditionPair>();
		NegativeTokens = new List<NegativeConditionPair>();
	}

	public CTokens(CActor actor, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons)
	{
		Actor = actor;
		PositiveTokens = posCons.ToList();
		NegativeTokens = negCons.ToList();
	}

	public void AddPositiveToken(CActor actor, PositiveConditionPair pair, bool recall = false)
	{
		bool flag = true;
		ModifiedPositiveCondition = CCondition.EPositiveCondition.NA;
		if (!recall)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.ChangeCondition);
			if (list != null)
			{
				foreach (CChangeConditionActiveBonus item in list)
				{
					flag = item.ChangeCondition(this, pair.PositiveCondition, pair.RoundDuration, pair.ConditionDecTrigger, actor, 1);
				}
			}
		}
		if (flag)
		{
			lock (PositiveTokens)
			{
				PositiveTokens.Add(pair);
			}
			Log.SimpleInfo("[TOKENS] Added positive token: " + pair.PositiveCondition.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
		}
		actor.RefreshFlyingFlag();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public void AddPositiveToken(CCondition.EPositiveCondition token, int duration, EConditionDecTrigger decTrigger, CActor actor, CActor caster, bool recall = false)
	{
		bool flag = true;
		ModifiedPositiveCondition = CCondition.EPositiveCondition.NA;
		if (!recall)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.ChangeCondition);
			if (list != null)
			{
				foreach (CChangeConditionActiveBonus item in list)
				{
					flag = item.ChangeCondition(this, token, duration, decTrigger, actor, 1);
				}
			}
		}
		if (flag)
		{
			lock (PositiveTokens)
			{
				PositiveTokens.Add(new PositiveConditionPair(token, RewardCondition.EConditionMapDuration.Now, duration, decTrigger, caster));
			}
			Log.SimpleInfo("[TOKENS] Added positive token: " + token.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
		}
		actor.RefreshFlyingFlag();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool RemovePositiveToken(CCondition.EPositiveCondition token, EConditionDecTrigger decTrigger = EConditionDecTrigger.None, List<EConditionDecTrigger> excludeDecTriggers = null)
	{
		List<PositiveConditionPair> list = PositiveTokens.FindAll((PositiveConditionPair s) => s.PositiveCondition == token);
		bool result = false;
		foreach (PositiveConditionPair item in list)
		{
			if (item != null && (excludeDecTriggers == null || !excludeDecTriggers.Contains(item.ConditionDecTrigger)) && (decTrigger == EConditionDecTrigger.None || item.ConditionDecTrigger == decTrigger))
			{
				lock (PositiveTokens)
				{
					PositiveTokens.Remove(item);
				}
				Log.SimpleInfo("[TOKENS] Removed positive token: " + token.ToString() + " from actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
				result = true;
			}
		}
		Actor.RefreshFlyingFlag();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
		return result;
	}

	public void ActivateTempDisabledConditions()
	{
		foreach (NegativeConditionPair checkNegativeToken in CheckNegativeTokens)
		{
			if (checkNegativeToken.TempDisabled)
			{
				checkNegativeToken.TempDisabled = false;
				if (checkNegativeToken.NegativeCondition == CCondition.ENegativeCondition.StopFlying)
				{
					CAbilityStopFlying.CheckPostStopFlying(Actor);
				}
			}
		}
	}

	public void AddNegativeToken(CCondition.ENegativeCondition token, int duration, EConditionDecTrigger decTrigger, CActor actor, int strength = 1, bool recall = false, bool tempDisabled = false)
	{
		bool flag = true;
		ModifiedNegativeCondition = CCondition.ENegativeCondition.NA;
		if (!recall)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.ChangeCondition);
			if (list != null)
			{
				foreach (CChangeConditionActiveBonus item in list)
				{
					flag = item.ChangeCondition(this, token, duration, decTrigger, actor, strength);
				}
			}
		}
		if (flag)
		{
			lock (NegativeTokens)
			{
				NegativeTokens.Add(new NegativeConditionPair(token, RewardCondition.EConditionMapDuration.Now, duration, decTrigger, strength, tempDisabled));
			}
			Log.SimpleInfo("[TOKENS] Added negative token: " + token.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
		}
		actor.RefreshFlyingFlag();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool RemoveNegativeToken(CCondition.ENegativeCondition token, EConditionDecTrigger decTrigger = EConditionDecTrigger.None, List<EConditionDecTrigger> excludeDecTriggers = null)
	{
		if (IsTransferableCondition(token) && Actor.IgnoreNegativeConditions())
		{
			return false;
		}
		List<NegativeConditionPair> list = NegativeTokens.FindAll((NegativeConditionPair s) => s.NegativeCondition == token);
		bool result = false;
		foreach (NegativeConditionPair item in list)
		{
			if (item != null && (excludeDecTriggers == null || !excludeDecTriggers.Contains(item.ConditionDecTrigger)) && (decTrigger == EConditionDecTrigger.None || item.ConditionDecTrigger == decTrigger))
			{
				lock (NegativeTokens)
				{
					NegativeTokens.Remove(item);
				}
				Log.SimpleInfo("[TOKENS] Removed negative token: " + token.ToString() + " from actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
				result = true;
			}
		}
		Actor.RefreshFlyingFlag();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
		return result;
	}

	public void ProcessTokens(EConditionDecTrigger trigger, out List<CCondition.EPositiveCondition> positiveConditionsRemoved, out List<CCondition.ENegativeCondition> negativeConditionsRemoved)
	{
		positiveConditionsRemoved = new List<CCondition.EPositiveCondition>();
		negativeConditionsRemoved = new List<CCondition.ENegativeCondition>();
		foreach (PositiveConditionPair checkPositiveToken in CheckPositiveTokens)
		{
			if (checkPositiveToken.ConditionDecTrigger != trigger)
			{
				continue;
			}
			checkPositiveToken.RoundDuration--;
			if (checkPositiveToken.RoundDuration <= 0)
			{
				lock (PositiveTokens)
				{
					PositiveTokens.Remove(checkPositiveToken);
				}
				positiveConditionsRemoved.Add(checkPositiveToken.PositiveCondition);
				Log.SimpleInfo("[TOKENS] Processed and removed positive token: " + checkPositiveToken.PositiveCondition.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
			}
		}
		bool ignoreNeg = Actor.IgnoreNegativeConditions();
		foreach (NegativeConditionPair item in CheckNegativeTokens.Where((NegativeConditionPair x) => !ignoreNeg || !IsTransferableCondition(x.NegativeCondition)))
		{
			if (item.ConditionDecTrigger != trigger)
			{
				continue;
			}
			item.RoundDuration--;
			if (item.RoundDuration <= 0)
			{
				lock (NegativeTokens)
				{
					NegativeTokens.Remove(item);
				}
				negativeConditionsRemoved.Add(item.NegativeCondition);
				Log.SimpleInfo("[TOKENS] Processed and removed negative token: " + item.NegativeCondition.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
			}
		}
		positiveConditionsRemoved = positiveConditionsRemoved.Distinct().ToList();
		negativeConditionsRemoved = negativeConditionsRemoved.Distinct().ToList();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool TryGet(CCondition.ENegativeCondition key, out NegativeConditionPair result)
	{
		NegativeTokens.RemoveAll((NegativeConditionPair r) => r == null);
		if (IsTransferableCondition(key) && Actor.IgnoreNegativeConditions())
		{
			result = null;
			return false;
		}
		result = CheckNegativeTokens.FirstOrDefault((NegativeConditionPair t) => t.NegativeCondition == key);
		return result != null;
	}

	public bool TryGet(CCondition.EPositiveCondition key, out PositiveConditionPair result)
	{
		PositiveTokens.RemoveAll((PositiveConditionPair r) => r == null);
		result = CheckPositiveTokens.FirstOrDefault((PositiveConditionPair t) => t.PositiveCondition == key);
		return result != null;
	}

	public bool HasKey(CCondition.EPositiveCondition token)
	{
		PositiveTokens.RemoveAll((PositiveConditionPair r) => r == null);
		return CheckPositiveTokens.Any((PositiveConditionPair s) => s.PositiveCondition == token);
	}

	public bool HasKey(CCondition.ENegativeCondition token, bool checkImmunities = true)
	{
		NegativeTokens.RemoveAll((NegativeConditionPair r) => r == null);
		if (!CheckNegativeTokens.Any((NegativeConditionPair s) => s.NegativeCondition == token && !s.TempDisabled))
		{
			return false;
		}
		return !checkImmunities || !IsTransferableCondition(token) || !Actor.IgnoreNegativeConditions();
	}

	public List<CCondition.EPositiveCondition> GetAllPositiveConditions()
	{
		List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
		foreach (PositiveConditionPair checkPositiveToken in CheckPositiveTokens)
		{
			if (!list.Contains(checkPositiveToken.PositiveCondition))
			{
				list.Add(checkPositiveToken.PositiveCondition);
			}
		}
		return list;
	}

	public List<CCondition.ENegativeCondition> GetAllNegativeConditions()
	{
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		foreach (NegativeConditionPair checkNegativeToken in CheckNegativeTokens)
		{
			if ((!Actor.IsDowned || checkNegativeToken.NegativeCondition != CCondition.ENegativeCondition.Stun) && !list.Contains(checkNegativeToken.NegativeCondition))
			{
				list.Add(checkNegativeToken.NegativeCondition);
			}
		}
		return list;
	}

	public List<CCondition.ENegativeCondition> GetAllTransferableConditions(bool checkIgnoreConditions = false)
	{
		if (checkIgnoreConditions && Actor.IgnoreNegativeConditions())
		{
			return new List<CCondition.ENegativeCondition>();
		}
		return (from x in GetAllNegativeConditions()
			where IsTransferableCondition(x)
			select x).Distinct().ToList();
	}

	public bool IsTransferableCondition(CCondition.ENegativeCondition condition)
	{
		if (condition != CCondition.ENegativeCondition.StopFlying)
		{
			return condition != CCondition.ENegativeCondition.Disadvantage;
		}
		return false;
	}

	public EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage, int targetNumber = 0)
	{
		bool flag = addAdvantage || CheckPositiveTokens.Any((PositiveConditionPair a) => (a.PositiveCondition == CCondition.EPositiveCondition.Advantage && a.ConditionDecTrigger == EConditionDecTrigger.First && targetNumber == 1) || (a.PositiveCondition == CCondition.EPositiveCondition.Advantage && a.ConditionDecTrigger != EConditionDecTrigger.First) || a.PositiveCondition == CCondition.EPositiveCondition.Strengthen);
		bool flag2 = addDisadvantage || HasKey(CCondition.ENegativeCondition.Muddle) || HasKey(CCondition.ENegativeCondition.Disadvantage);
		if (flag & flag2)
		{
			return EAdvantageStatuses.None;
		}
		if (flag)
		{
			return EAdvantageStatuses.Advantage;
		}
		if (flag2)
		{
			return EAdvantageStatuses.Disadvantage;
		}
		return EAdvantageStatuses.None;
	}

	public int CountNegativeConditions(bool checkIgnoreConditions = false)
	{
		return GetAllTransferableConditions(checkIgnoreConditions).Count();
	}
}

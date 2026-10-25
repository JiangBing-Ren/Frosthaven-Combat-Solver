using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public abstract class CChallenge
{
	public enum Trigger
	{
		None,
		OnGainNegativeCondition,
		OnSufferDamageFromAttackWithNoShield,
		OnEnemySufferDamage,
		OnSpawn,
		OnStartScenario,
		OnRest,
		OnAttack,
		OnEnemySufferDamagePostRetaliate,
		ReverseInitiative,
		OnItemSpent
	}

	public class TriggerParams
	{
		public Trigger trigger;

		public CActor actor;

		public CAbilityAttack attackAbility;

		public CCondition.ENegativeCondition appliedCondition;

		public CActor actorBeingAttacked;

		public CItem item;
	}

	public static Dictionary<uint, CChallenge> Challenges = new Dictionary<uint, CChallenge>();

	public virtual string Name => "";

	private string LocKey => $"{ID}_challenge_cards_{Name}";

	public string NameLocKey => LocKey + "_1";

	public string DescriptionLocKey => LocKey + "_2";

	public virtual uint ID => 0u;

	public virtual void Reset()
	{
	}

	protected virtual string SerializeInternal()
	{
		return string.Empty;
	}

	protected virtual void DeserializeInternal(string storage)
	{
	}

	public virtual void OnFinishLoading()
	{
	}

	public string Serialize()
	{
		return SerializeInternal();
	}

	public void Deserialize(string storage)
	{
		DeserializeInternal(storage);
	}

	public override string ToString()
	{
		return LocKey;
	}

	public virtual IEnumerable<Trigger> GetTriggers()
	{
		yield break;
	}

	protected virtual void Perform(TriggerParams triggerParams = null)
	{
	}

	public static void CheckTrigger(Trigger trigger, TriggerParams triggerParams = null)
	{
		if (triggerParams == null)
		{
			triggerParams = new TriggerParams();
		}
		triggerParams.trigger = trigger;
		foreach (CChallenge item in GetChallengesFromTrigger(trigger))
		{
			item.Perform(triggerParams);
		}
	}

	public static IEnumerable<CChallenge> GetChallengesFromTrigger(Trigger trigger)
	{
		foreach (CChallenge challenge in ScenarioManager.CurrentScenarioState.Challenges)
		{
			if (challenge.GetTriggers().Contains(trigger))
			{
				yield return challenge;
			}
		}
	}
}

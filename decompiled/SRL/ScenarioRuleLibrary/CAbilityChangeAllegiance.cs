using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

public class CAbilityChangeAllegiance : CAbilityTargeting
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ChangeAllegianceAbilityData
	{
		[JsonProperty]
		public CActor.EType ChangeToType;

		[JsonConstructor]
		public ChangeAllegianceAbilityData(CActor.EType changeToType)
		{
			ChangeToType = changeToType;
		}

		public ChangeAllegianceAbilityData Copy()
		{
			return new ChangeAllegianceAbilityData(ChangeToType);
		}
	}

	[JsonProperty]
	public ChangeAllegianceAbilityData ChangeAllegianceData { get; set; }

	public CAbilityChangeAllegiance(ChangeAllegianceAbilityData changeAllegianceAbilityData)
		: base(EAbilityType.ChangeAllegiance)
	{
		ChangeAllegianceData = changeAllegianceAbilityData;
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsApplyingConditionActiveBonus_MessageData message = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, actorApplying)
		{
			m_Ability = this,
			m_ActorsAppliedTo = actorsAppliedTo
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (!actor.IsDead)
		{
			if (actor is CEnemyActor cEnemyActor)
			{
				EnemyState enemyState = ScenarioManager.CurrentScenarioState.AllEnemyStates.SingleOrDefault((EnemyState x) => x.ActorGuid == actor.ActorGuid);
				switch (cEnemyActor.OriginalType)
				{
				case CActor.EType.Enemy:
					ScenarioManager.Scenario.Enemies.Remove(cEnemyActor);
					ScenarioManager.CurrentScenarioState.Monsters.Remove(enemyState);
					break;
				case CActor.EType.Enemy2:
					ScenarioManager.Scenario.Enemy2Monsters.Remove(cEnemyActor);
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Remove(enemyState);
					break;
				case CActor.EType.Ally:
					ScenarioManager.Scenario.AllyMonsters.Remove(cEnemyActor);
					ScenarioManager.CurrentScenarioState.AllyMonsters.Remove(enemyState);
					break;
				case CActor.EType.Neutral:
					ScenarioManager.Scenario.NeutralMonsters.Remove(cEnemyActor);
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Remove(enemyState);
					break;
				}
				switch (ChangeAllegianceData.ChangeToType)
				{
				case CActor.EType.Enemy:
					ScenarioManager.Scenario.Enemies.Add(cEnemyActor);
					ScenarioManager.CurrentScenarioState.Monsters.Add(enemyState);
					enemyState.Type = CActor.EType.Enemy;
					break;
				case CActor.EType.Enemy2:
					ScenarioManager.Scenario.Enemy2Monsters.Add(cEnemyActor);
					ScenarioManager.CurrentScenarioState.Enemy2Monsters.Add(enemyState);
					enemyState.Type = CActor.EType.Enemy2;
					break;
				case CActor.EType.Ally:
					ScenarioManager.Scenario.AllyMonsters.Add(cEnemyActor);
					ScenarioManager.CurrentScenarioState.AllyMonsters.Add(enemyState);
					enemyState.Type = CActor.EType.Ally;
					break;
				case CActor.EType.Neutral:
					ScenarioManager.Scenario.NeutralMonsters.Add(cEnemyActor);
					ScenarioManager.CurrentScenarioState.NeutralMonsters.Add(enemyState);
					enemyState.Type = CActor.EType.Neutral;
					break;
				}
				CActor.EType originalType = actor.OriginalType;
				actor.OriginalType = ChangeAllegianceData.ChangeToType;
				actor.Type = ChangeAllegianceData.ChangeToType;
				CActorChangedAllegiance_MessageData message = new CActorChangedAllegiance_MessageData(this, actor, originalType, ChangeAllegianceData.ChangeToType);
				ScenarioRuleClient.MessageHandler(message);
				CRefreshActorHealth_MessageData message2 = new CRefreshActorHealth_MessageData(actor, actor, actor.Health);
				ScenarioRuleClient.MessageHandler(message2);
				ScenarioManager.CurrentScenarioState.CheckObjectivesComplete();
			}
			else
			{
				Log.ErrorWithReport("Attempted to change allegiance for non supported actor type");
			}
		}
		return false;
	}

	public override void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (actorApplying.Type != CActor.EType.Player)
		{
			validActorsInRange.Sort((CActor x, CActor y) => x.Initiative().CompareTo(y.Initiative()));
			validActorsInRange.Reverse();
		}
	}

	public override bool IsPositive()
	{
		if (!AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) && !AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion))
		{
			return !AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self);
		}
		return false;
	}

	public static string GetAllegianceLoc(CActor.EType actorType)
	{
		string result = string.Empty;
		switch (actorType)
		{
		case CActor.EType.Enemy:
			result = "GUI_ENEMY";
			break;
		case CActor.EType.Ally:
			result = "GUI_ALLY";
			break;
		case CActor.EType.Neutral:
			result = "GUI_NEUTRAL";
			break;
		case CActor.EType.Enemy2:
			result = "GUI_ENEMY2";
			break;
		}
		return result;
	}
}

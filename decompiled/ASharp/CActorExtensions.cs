using System;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public static class CActorExtensions
{
	public static bool IsEnemyByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CMonsterClass;
		}
		return false;
	}

	public static bool IsPlayerByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CCharacterClass;
		}
		return false;
	}

	public static bool IsHeroSummonByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CHeroSummonClass;
		}
		return false;
	}

	public static bool IsPropActorByDefault(this CActor actor, bool checkAttachedToProp = true)
	{
		if (!(actor is CObjectActor cObjectActor))
		{
			return false;
		}
		if (cObjectActor.Class.ID != "PropDummyObject")
		{
			return false;
		}
		if (checkAttachedToProp)
		{
			return cObjectActor.IsAttachedToProp;
		}
		return true;
	}

	public static bool IsDummyEnemyByDefault(this CActor actor)
	{
		if (actor == null || !(actor.Class is CMonsterClass cMonsterClass))
		{
			return false;
		}
		if (cMonsterClass.MonsterType == EMonsterType.Object && actor.Class.ID == "PropDummyObject")
		{
			return true;
		}
		if (cMonsterClass.AbilityCardsPool.Count == 0)
		{
			return cMonsterClass.RoundAbilityCard == null;
		}
		return false;
	}

	public static CActor.EType GetDefaultType(this CActor actor)
	{
		if (actor.IsPlayerByDefault())
		{
			return CActor.EType.Player;
		}
		if (actor.IsEnemyByDefault())
		{
			return CActor.EType.Enemy;
		}
		if (actor.IsHeroSummonByDefault())
		{
			return CActor.EType.HeroSummon;
		}
		throw new Exception("Invalid actor default type");
	}
}

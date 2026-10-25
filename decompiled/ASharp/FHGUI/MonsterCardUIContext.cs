using System;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI;

public class MonsterCardUIContext : IEquatable<MonsterCardUIContext>
{
	public MonsterCardYMLData CardYML;

	public CEnemyActor EnemyForBaseStats;

	public CMonsterClass FallbackMonsterClass;

	public bool ShowFaction;

	public CardPileType CardPile = CardPileType.None;

	public int? OverrideLevel;

	public CMonsterClass AltMonsterClass;

	public CMonsterClass MonsterClass => EnemyForBaseStats?.MonsterClass ?? FallbackMonsterClass;

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((MonsterCardUIContext)obj);
	}

	public bool Equals(MonsterCardUIContext other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (object.Equals(CardYML, other.CardYML) && object.Equals(EnemyForBaseStats, other.EnemyForBaseStats) && object.Equals(FallbackMonsterClass, other.FallbackMonsterClass) && ShowFaction == other.ShowFaction && CardPile == other.CardPile)
		{
			return OverrideLevel == other.OverrideLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(CardYML, EnemyForBaseStats, FallbackMonsterClass, ShowFaction, (int)CardPile, OverrideLevel);
	}

	public static bool operator ==(MonsterCardUIContext left, MonsterCardUIContext right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(MonsterCardUIContext left, MonsterCardUIContext right)
	{
		return !object.Equals(left, right);
	}
}

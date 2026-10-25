using System;
using FH.Tools;

namespace ScenarioRuleLibrary;

public struct LootCard
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum EType
	{
		None,
		Money,
		Lumber,
		Metal,
		Hide,
		Arrowvine,
		Axenut,
		Corpsecap,
		Flamefruit,
		Rockroot,
		Snowthistle,
		RandomItem
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EAmount
	{
		Zero,
		Plus1,
		Plus2,
		Plus3,
		Plus1if4_Plus2if23,
		Plus1if34_Plus2if2
	}

	public static readonly EType[] AllTypes = (EType[])typeof(EType).GetEnumValues();

	public EType Type;

	public EAmount Amount;

	public uint Bonus;

	public string[] AvailableCampaignSectionIDs;

	public int ID;

	public bool Equals(LootCard other)
	{
		if (Type == other.Type && Amount == other.Amount && Bonus == other.Bonus)
		{
			return ID == other.ID;
		}
		return false;
	}

	public static bool operator ==(LootCard a, LootCard b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(LootCard a, LootCard b)
	{
		return !(a == b);
	}

	public override string ToString()
	{
		return string.Format("{0}, Type: {1} Amount: {2}{3}", ID, Type, Amount, (Bonus != 0) ? $" Bonus: {Bonus}" : "");
	}
}

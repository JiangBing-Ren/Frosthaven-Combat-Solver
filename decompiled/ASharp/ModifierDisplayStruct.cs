using ScenarioRuleLibrary;

public struct ModifierDisplayStruct
{
	public enum ModType
	{
		Damage,
		Shield,
		Miss,
		Critical,
		PlusZero,
		DirectDamage,
		Poison,
		Element_Fire,
		Element_Ice,
		Element_Air,
		Element_Earth,
		Element_Light,
		Element_Dark,
		Element_Multiple,
		AttackBonus
	}

	public enum MathOperation
	{
		None,
		Add,
		Subtract,
		Multiply
	}

	public enum Benefit
	{
		Neutral,
		Positive,
		Negative
	}

	public int Value;

	public ModType ModifierType;

	public MathOperation MathType;

	public EAdvantageStatuses Advantage;

	public Benefit BenfitType;

	public ModifierDisplayStruct(ModType modifierType, string modifierData, EAdvantageStatuses advantage = EAdvantageStatuses.None)
	{
		ModifierType = modifierType;
		Advantage = advantage;
		if (int.TryParse(modifierData, out var result))
		{
			result = int.Parse(modifierData);
		}
		else
		{
			string[] array = modifierData.Split("*".ToCharArray());
			for (int i = 0; i < array.Length; i++)
			{
				if (int.TryParse(array[i], out result))
				{
					result = int.Parse(array[i]);
				}
			}
		}
		Value = result;
		MathType = MathOperation.None;
		BenfitType = Benefit.Neutral;
		if (modifierData == "*2")
		{
			MathType = MathOperation.Multiply;
			BenfitType = Benefit.Positive;
		}
		if (modifierData == "*0")
		{
			MathType = MathOperation.Multiply;
			BenfitType = Benefit.Negative;
		}
		if (modifierData.Contains("+"))
		{
			MathType = MathOperation.Add;
			BenfitType = Benefit.Positive;
		}
		if (modifierData.Contains("-"))
		{
			MathType = MathOperation.Subtract;
			BenfitType = Benefit.Negative;
		}
		if (modifierData == "+0")
		{
			MathType = MathOperation.None;
			BenfitType = Benefit.Neutral;
		}
	}
}

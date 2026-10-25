using System.Collections.Generic;

namespace ScenarioRuleLibrary.YML;

public class AttackModifierYMLComparer : IEqualityComparer<AttackModifierYMLData>
{
	public bool Equals(AttackModifierYMLData a1, AttackModifierYMLData a2)
	{
		if (a2 == null && a1 == null)
		{
			return true;
		}
		if (a1 == null || a2 == null)
		{
			return false;
		}
		if (a1.Name == a2.Name)
		{
			return true;
		}
		return false;
	}

	public int GetHashCode(AttackModifierYMLData ax)
	{
		return (ax.Name + ax.MathModifier).GetHashCode();
	}
}

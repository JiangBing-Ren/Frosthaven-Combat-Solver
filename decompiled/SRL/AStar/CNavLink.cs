using System;

namespace AStar;

public class CNavLink
{
	public readonly int Weight;

	public readonly int? WeightJump;

	public readonly CNode DestinationNode;

	public bool Blocked;

	public bool AllowLOS;

	public CNavLink(int weight, CNode destinationNode, bool blocked = false, bool allowLos = false)
	{
		Weight = weight;
		DestinationNode = destinationNode;
		Blocked = blocked;
		AllowLOS = allowLos;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CNavLink cNavLink))
		{
			return false;
		}
		if (Weight == cNavLink.Weight && DestinationNode.Equals(cNavLink.DestinationNode) && Blocked == cNavLink.Blocked)
		{
			return AllowLOS == cNavLink.AllowLOS;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Weight, DestinationNode, Blocked, AllowLOS);
	}
}

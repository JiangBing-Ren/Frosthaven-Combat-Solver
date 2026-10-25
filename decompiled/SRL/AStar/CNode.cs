using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;

namespace AStar;

public class CNode : IDumpState
{
	public int IslandID = -1;

	private bool m_Blocked;

	public readonly List<CNavLink> NavLinks = new List<CNavLink>();

	public Point Location { get; }

	public bool Walkable { get; set; }

	public bool Blocked
	{
		get
		{
			return m_Blocked;
		}
		set
		{
			m_Blocked = value;
		}
	}

	public bool SuperBlocked { get; set; }

	public bool TransientBlocked { get; set; }

	public bool IsBridge { get; set; }

	public bool IsBridgeOpen { get; set; }

	public bool HasHealth { get; set; }

	public bool IsDifficultTerrain { get; set; }

	public bool IsWaterTerrain { get; set; }

	public bool IsIceTerrain { get; set; }

	public bool IsNegativeHex { get; set; }

	public bool IsSuperNegativeHex { get; set; }

	public bool HasShadow { get; set; }

	public CNode(int x, int y)
	{
		Location = new Point(x, y);
		Walkable = false;
		Blocked = false;
		SuperBlocked = false;
		IsBridge = false;
		IsBridgeOpen = false;
	}

	internal static float GetTraversalCost(Point location, Point otherLocation, bool fly = false, bool jump = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		float num = 0f;
		CNode cNode = ScenarioManager.PathFinder.Nodes[location.X, location.Y];
		if (cNode.IsDifficultTerrain && !ignoreMoveCost && !fly && !jump && !ignoreDifficultTerrain)
		{
			num = 0.2f;
		}
		if (cNode.IsBridge)
		{
			num = 0.2f;
		}
		if (CPathFinder.OddEvenCheck)
		{
			float num2 = (float)otherLocation.X + ((CPathFinder.OddEvenCheck & ((otherLocation.Y & 1) == 1)) ? 0.5f : 0f);
			float num3 = otherLocation.Y;
			float num4 = (float)location.X + ((CPathFinder.OddEvenCheck & ((location.Y & 1) == 1)) ? 0.5f : 0f);
			float num5 = location.Y;
			float num6 = num2 - num4;
			float num7 = num3 - num5;
			return (float)Math.Sqrt(num6 * num6 + num7 * num7) + num;
		}
		float num8 = otherLocation.X - location.X;
		float num9 = otherLocation.Y - location.Y;
		return (float)Math.Sqrt(num8 * num8 + num9 * num9) + num;
	}

	public bool NavTo(CNode toNode, bool ignoreBlocked)
	{
		if (!toNode.Walkable || toNode.SuperBlocked)
		{
			return false;
		}
		if (!ignoreBlocked && (toNode.Blocked || toNode.TransientBlocked))
		{
			return false;
		}
		bool flag = false;
		int i = 0;
		for (int count = NavLinks.Count; i < count; i++)
		{
			if (NavLinks[i].DestinationNode.Equals(toNode))
			{
				flag = true;
				break;
			}
		}
		return (IslandID == toNode.IslandID || IsBridge || toNode.IsBridge) | flag;
	}

	public override bool Equals(object obj)
	{
		return obj == this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public string StateDumpString()
	{
		string flags = "";
		AddFlag("Walkable", Walkable);
		AddFlag("Blocked", Blocked);
		AddFlag("SuperBlocked", SuperBlocked);
		AddFlag("TransientBlocked", TransientBlocked);
		AddFlag("Bridge", IsBridge);
		AddFlag("BridgeClosed", !IsBridgeOpen);
		AddFlag("HasHealth", HasHealth);
		AddFlag("DifficultTerrain", IsDifficultTerrain);
		AddFlag("WaterTerrain", IsWaterTerrain);
		AddFlag("IceTerrain", IsIceTerrain);
		AddFlag("NegativeHex", IsNegativeHex);
		AddFlag("HasShadow", HasShadow);
		if (flags == "")
		{
			flags = "NONE";
		}
		string text = $"{GetType().Name}({Location}, {flags}";
		if (IslandID != -1)
		{
			text += $", IslandID:{IslandID}";
		}
		if (NavLinks.Count != 0)
		{
			text += $", NavLinks:{NavLinks.Count}";
		}
		return text + ")";
		void AddFlag(string name, bool value)
		{
			if (value)
			{
				if (flags != "")
				{
					flags += " | ";
				}
				flags += name;
			}
		}
	}

	public override string ToString()
	{
		return "[" + MF.RefStr(this) + "] " + StateDumpString();
	}

	string IDumpState.ToStateDumpString(StateDump dump)
	{
		return StateDumpString();
	}

	void IDumpState.DumpMembers(StateDump dump)
	{
		if (NavLinks.Count > 0)
		{
			dump.Add("NavLinks", NavLinks);
		}
	}
}

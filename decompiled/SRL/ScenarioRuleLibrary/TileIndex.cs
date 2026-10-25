using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y}")]
[JsonObject(MemberSerialization.OptIn)]
public class TileIndex : ISerializable, IEquatable<TileIndex>
{
	[JsonProperty]
	public int X;

	[JsonProperty]
	public int Y;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
	}

	public TileIndex(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "X"))
				{
					if (name == "Y")
					{
						Y = info.GetInt32("Y");
					}
				}
				else
				{
					X = info.GetInt32("X");
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize TileIndex entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[JsonConstructor]
	public TileIndex(int x, int y)
	{
		X = x;
		Y = y;
	}

	public TileIndex(Point point)
	{
		X = point.X;
		Y = point.Y;
	}

	public TileIndex Copy()
	{
		return new TileIndex(X, Y);
	}

	public Point ToPoint()
	{
		return new Point(X, Y);
	}

	public override string ToString()
	{
		return $"{X}:{Y}";
	}

	public static bool Compare(TileIndex t1, TileIndex t2)
	{
		switch (StateShared.CheckNullsMatch(t1, t2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (t1.X == t2.X)
			{
				return t1.Y == t2.Y;
			}
			return false;
		}
	}

	public static bool Compare(TileIndex t1, Point t2)
	{
		switch (StateShared.CheckNullsMatch(t1, t2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (t1.X == t2.X)
			{
				return t1.Y == t2.Y;
			}
			return false;
		}
	}

	public bool Equals(TileIndex other)
	{
		if (X == other.X)
		{
			return Y == other.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X + 31 * Y;
	}
}

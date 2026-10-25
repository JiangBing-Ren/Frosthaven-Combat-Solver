using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;
using ScenarioRuleLibrary;

namespace AStar;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public struct Point : ISerializable
{
	[JsonProperty]
	public int X;

	[JsonProperty]
	public int Y;

	public bool IsZero
	{
		get
		{
			if (X == 0)
			{
				return Y == 0;
			}
			return false;
		}
	}

	public bool IsValid
	{
		get
		{
			if (X == 0)
			{
				return Y != 0;
			}
			return true;
		}
	}

	[JsonConstructor]
	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Point(TileIndex tileIndex)
	{
		X = tileIndex.X;
		Y = tileIndex.Y;
	}

	public Point(CTile tile)
	{
		X = tile.m_ArrayIndex.X;
		Y = tile.m_ArrayIndex.Y;
	}

	private Point(SerializationInfo info, StreamingContext context)
	{
		try
		{
			X = info.GetInt32("X");
			Y = info.GetInt32("Y");
		}
		catch (Exception ex)
		{
			Log.ErrorWithReport("Exception while trying to deserialize AStar.Point\n" + ex.Message + "\n" + ex.StackTrace);
			throw ex;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
	}

	public static bool operator ==(Point left, Point right)
	{
		if (left.X == right.X)
		{
			return left.Y == right.Y;
		}
		return false;
	}

	public static bool operator !=(Point left, Point right)
	{
		if (left.X == right.X)
		{
			return left.Y != right.Y;
		}
		return true;
	}

	public static Point operator -(Point left, Point right)
	{
		return new Point(left.X - right.X, left.Y - right.Y);
	}

	public static Point operator +(Point left, Point right)
	{
		return new Point(left.X + right.X, left.Y + right.Y);
	}

	public static Point operator *(Point left, int right)
	{
		return new Point(left.X * right, left.Y * right);
	}

	public override bool Equals(object obj)
	{
		Point point = (Point)obj;
		if (X == point.X)
		{
			return Y == point.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X ^ Y;
	}

	public override string ToString()
	{
		return $"{X}:{Y}";
	}

	public TileIndex ToTileIndex()
	{
		return new TileIndex(this);
	}

	public static Point OddRToAxial(Point p)
	{
		int x = p.X - (p.Y - (p.Y & 1)) / 2;
		int y = p.Y;
		return new Point(x, y);
	}

	public static Point AxialToOddR(Point p)
	{
		int x = p.X + (p.Y - (p.Y & 1)) / 2;
		int y = p.Y;
		return new Point(x, y);
	}

	public static int AxialDistance(Point a, Point b)
	{
		return (Math.Abs(a.X - b.X) + Math.Abs(a.X + a.Y - b.X - b.Y) + Math.Abs(a.Y - b.Y)) / 2;
	}

	public static Point AxialSymmetrical_OverAxisR(Point p)
	{
		int y = p.Y;
		if (y == 0)
		{
			return p;
		}
		return new Point(-(-p.X - y), -y);
	}

	public static Point AxialReflection_AlongAxisR(Point p)
	{
		int x = p.X;
		int y = p.Y;
		return new Point(-x - y, y);
	}

	public static int HexDist(Point pt1, Point pt2)
	{
		Point a = OddRToAxial(pt1);
		Point b = OddRToAxial(pt2);
		return AxialDistance(a, b);
	}

	public static int AxialDirectionToRotation(Point axialDir)
	{
		if (axialDir == new Point(-1, 0))
		{
			return 0;
		}
		if (axialDir == new Point(-1, 1))
		{
			return 60;
		}
		if (axialDir == new Point(0, 1))
		{
			return 120;
		}
		if (axialDir == new Point(1, 0))
		{
			return 180;
		}
		if (axialDir == new Point(1, -1))
		{
			return 240;
		}
		if (axialDir == new Point(0, -1))
		{
			return 300;
		}
		throw new ArgumentException(axialDir.ToString());
	}

	public static Point RotationToAxialDirection(int yEulerDegrees)
	{
		return yEulerDegrees switch
		{
			0 => new Point(-1, 0), 
			60 => new Point(-1, 1), 
			120 => new Point(0, 1), 
			180 => new Point(1, 0), 
			240 => new Point(1, -1), 
			300 => new Point(0, -1), 
			_ => throw new ArgumentException(yEulerDegrees.ToString()), 
		};
	}

	public Point Rotate(Point center, int angle)
	{
		Point cubeThis = OddRToAxial(this);
		Point cubeCenter = OddRToAxial(center);
		return AxialToOddR(RotateAxial(cubeThis, cubeCenter, angle));
	}

	public static Point RotateAxial(Point CubeThis, Point CubeCenter, int angle)
	{
		Point point = CubeThis - CubeCenter;
		int x = point.X;
		int y = point.Y;
		int num = x;
		int num2 = y;
		int num3 = -x - y;
		int num4 = num;
		int num5 = num2;
		if (angle > 360)
		{
			angle %= 360;
		}
		if (angle < -360)
		{
			angle %= -360;
		}
		switch (angle)
		{
		case -300:
		case 60:
			num4 = -num3;
			num5 = -num;
			break;
		case -60:
		case 300:
			num4 = -num2;
			num5 = -num3;
			break;
		case -240:
		case 120:
			num4 = num2;
			num5 = num3;
			break;
		case -120:
		case 240:
			num4 = num3;
			num5 = num;
			break;
		case -180:
		case 180:
			num4 = -num;
			num5 = -num2;
			break;
		}
		x = num4;
		y = num5;
		return CubeCenter + new Point(x, y);
	}

	public static bool AreHexesInAStraightLine(IEnumerable<Point> pts)
	{
		if (pts.Count() == 0)
		{
			return false;
		}
		if (pts.Count() == 1)
		{
			return true;
		}
		Point p0 = pts.First();
		bool flag = pts.All((Point point) => point.X == p0.X);
		bool flag2 = pts.All((Point point) => point.Y == p0.Y);
		bool flag3 = pts.All((Point point) => point.X + point.Y == p0.X + p0.Y);
		return flag | flag2 | flag3;
	}
}

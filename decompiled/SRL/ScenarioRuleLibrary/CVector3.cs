using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y} Z:{Z}")]
public class CVector3 : ISerializable
{
	public float X { get; private set; }

	[StateDump(Skip = true)]
	public float Y { get; private set; }

	public float Z { get; private set; }

	public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

	public CVector3 Normalized => this / Magnitude;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
		info.AddValue("Z", Z);
	}

	public CVector3(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "X":
					X = (float)info.GetDouble("X");
					break;
				case "Y":
					Y = (float)info.GetDouble("Y");
					break;
				case "Z":
					Z = (float)info.GetDouble("Z");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CVector3 entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CVector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public override string ToString()
	{
		return "X:" + X.ToString(CultureInfo.InvariantCulture) + " Y:" + Y.ToString(CultureInfo.InvariantCulture) + " Z:" + Z.ToString(CultureInfo.InvariantCulture);
	}

	public string ToStateDumpString()
	{
		return "X:" + X.ToString(CultureInfo.InvariantCulture) + " Z:" + Z.ToString(CultureInfo.InvariantCulture);
	}

	public static bool Compare(CVector3 v1, CVector3 v2)
	{
		switch (StateShared.CheckNullsMatch(v1, v2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (CompareFloats(v1.X, v2.X) && CompareFloats(v1.Y, v2.Y))
			{
				return CompareFloats(v1.Z, v2.Z);
			}
			return false;
		}
	}

	public static bool CompareFloats(float a, float b, float allowedDifference = 0.001f)
	{
		if (a == b)
		{
			return true;
		}
		if (a > b)
		{
			return a - b < allowedDifference;
		}
		return b - a < allowedDifference;
	}

	public static CVector3 operator +(CVector3 a, CVector3 b)
	{
		return new CVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
	}

	public static CVector3 operator -(CVector3 a, CVector3 b)
	{
		return new CVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
	}

	public static CVector3 operator *(CVector3 a, float b)
	{
		return new CVector3(a.X * b, a.Y * b, a.Z * b);
	}

	public static CVector3 operator /(CVector3 a, float b)
	{
		return new CVector3(a.X / b, a.Y / b, a.Z / b);
	}

	public static float Dot(CVector3 a, CVector3 b)
	{
		return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
	}
}

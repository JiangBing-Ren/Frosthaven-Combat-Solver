using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y} Z:{Z} W:{W}")]
public class CVector4 : ISerializable
{
	public float X { get; private set; }

	public float Y { get; private set; }

	public float Z { get; private set; }

	public float W { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
		info.AddValue("Z", Z);
		info.AddValue("W", W);
	}

	public CVector4(SerializationInfo info, StreamingContext context)
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
				case "W":
					W = (float)info.GetDouble("W");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CVector4 entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CVector4(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public override string ToString()
	{
		return "X:" + X.ToString(CultureInfo.InvariantCulture) + " Y:" + Y.ToString(CultureInfo.InvariantCulture) + " Z:" + Z.ToString(CultureInfo.InvariantCulture) + " W:" + W.ToString(CultureInfo.InvariantCulture);
	}

	public static bool Compare(CVector4 v1, CVector4 v2)
	{
		switch (StateShared.CheckNullsMatch(v1, v2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (CompareFloats(v1.X, v2.X) && CompareFloats(v1.Y, v2.Y) && CompareFloats(v1.Z, v2.Z))
			{
				return CompareFloats(v1.W, v2.W);
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
}

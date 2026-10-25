using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y} Z:{Z}")]
public class CVectorInt3 : ISerializable
{
	public int X { get; private set; }

	public int Y { get; private set; }

	public int Z { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
		info.AddValue("Z", Z);
	}

	public CVectorInt3(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "X":
					X = info.GetInt32("X");
					break;
				case "Y":
					Y = info.GetInt32("Y");
					break;
				case "Z":
					Z = info.GetInt32("Z");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CVectorInt3 entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CVectorInt3(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public override string ToString()
	{
		return "X: " + X + " Y: " + Y + " Z: " + Z;
	}

	public static bool Compare(CVectorInt3 v1, CVectorInt3 v2)
	{
		if (v1.X == v2.X && v1.Y == v2.Y)
		{
			return v1.Z == v2.Z;
		}
		return false;
	}
}

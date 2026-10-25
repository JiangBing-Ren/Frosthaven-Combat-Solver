using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CApparanceOverrideDetails : ISerializable, IEquatable<CApparanceOverrideDetails>
{
	public const string cScenarioOverrideGUID = "ScenarioApparanceOverrideGUID";

	public const string cWallOverridePrefix = "ApparanceWall";

	public string GUID;

	public int OverrideSeed;

	public ScenarioPossibleRoom.EBiome OverrideBiome;

	public ScenarioPossibleRoom.ESubBiome OverrideSubBiome;

	public ScenarioPossibleRoom.ETheme OverrideTheme;

	public ScenarioPossibleRoom.ESubTheme OverrideSubTheme;

	public ScenarioPossibleRoom.ETone OverrideTone;

	public ScenarioPossibleRoom.EBiomeState OverrideBiomeState;

	public int OverrideSiblingIndex;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("GUID", GUID);
		info.AddValue("OverrideSeed", OverrideSeed);
		info.AddValue("OverrideBiome", OverrideBiome);
		info.AddValue("OverrideSubBiome", OverrideSubBiome);
		info.AddValue("OverrideTheme", OverrideTheme);
		info.AddValue("OverrideSubTheme", OverrideSubTheme);
		info.AddValue("OverrideTone", OverrideTone);
		info.AddValue("OverrideBiomeState", OverrideBiomeState);
		info.AddValue("OverrideSiblingIndex", OverrideSiblingIndex);
	}

	public CApparanceOverrideDetails(SerializationInfo info, StreamingContext context)
	{
		bool flag = false;
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "GUID":
					GUID = info.GetString("GUID");
					break;
				case "OverrideSeed":
					OverrideSeed = info.GetInt32("OverrideSeed");
					break;
				case "OverrideBiome":
					OverrideBiome = (ScenarioPossibleRoom.EBiome)info.GetValue("OverrideBiome", typeof(ScenarioPossibleRoom.EBiome));
					break;
				case "OverrideSubBiome":
					OverrideSubBiome = (ScenarioPossibleRoom.ESubBiome)info.GetValue("OverrideSubBiome", typeof(ScenarioPossibleRoom.ESubBiome));
					break;
				case "OverrideTheme":
					OverrideTheme = (ScenarioPossibleRoom.ETheme)info.GetValue("OverrideTheme", typeof(ScenarioPossibleRoom.ETheme));
					break;
				case "OverrideSubTheme":
					OverrideSubTheme = (ScenarioPossibleRoom.ESubTheme)info.GetValue("OverrideSubTheme", typeof(ScenarioPossibleRoom.ESubTheme));
					break;
				case "OverrideTone":
					OverrideTone = (ScenarioPossibleRoom.ETone)info.GetValue("OverrideTone", typeof(ScenarioPossibleRoom.ETone));
					break;
				case "OverrideBiomeState":
					OverrideBiomeState = (ScenarioPossibleRoom.EBiomeState)info.GetValue("OverrideBiomeState", typeof(ScenarioPossibleRoom.EBiomeState));
					break;
				case "OverrideSiblingIndex":
					flag = true;
					OverrideSiblingIndex = info.GetInt32("OverrideSiblingIndex");
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CApparanceOverrideDetails entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (!flag)
		{
			OverrideSiblingIndex = -1;
		}
	}

	public CApparanceOverrideDetails(string guid)
	{
		GUID = guid;
		OverrideSeed = 0;
		OverrideBiome = ScenarioPossibleRoom.EBiome.Inherit;
		OverrideSubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
		OverrideTheme = ScenarioPossibleRoom.ETheme.Inherit;
		OverrideSubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
		OverrideTone = ScenarioPossibleRoom.ETone.Inherit;
		OverrideBiomeState = ScenarioPossibleRoom.EBiomeState.Inherit;
		OverrideSiblingIndex = -1;
	}

	public static string GetWallGUID(CMap parentMap, int wallIndex)
	{
		return string.Format("{0}_{1}_{2}", "ApparanceWall", parentMap.MapGuid, wallIndex);
	}

	public static string GetWallGUID(string mapGUID, int wallIndex)
	{
		return string.Format("{0}_{1}_{2}", "ApparanceWall", mapGUID, wallIndex);
	}

	public bool Equals(CApparanceOverrideDetails other)
	{
		if (other == null)
		{
			return false;
		}
		if (GUID.Equals(other.GUID) && OverrideSeed.Equals(other.OverrideSeed) && OverrideBiome.Equals(other.OverrideBiome) && OverrideSubBiome.Equals(other.OverrideSubBiome) && OverrideTheme.Equals(other.OverrideTheme) && OverrideSubTheme.Equals(other.OverrideSubTheme) && OverrideTone.Equals(other.OverrideTone) && OverrideBiomeState.Equals(other.OverrideBiomeState))
		{
			return OverrideSiblingIndex.Equals(other.OverrideSiblingIndex);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((((((17 * 23 + (GUID?.GetHashCode() ?? 0)) * 23 + OverrideSeed.GetHashCode()) * 23 + OverrideBiome.GetHashCode()) * 23 + OverrideSubBiome.GetHashCode()) * 23 + OverrideTheme.GetHashCode()) * 23 + OverrideSubTheme.GetHashCode()) * 23 + OverrideTone.GetHashCode()) * 23 + OverrideBiomeState.GetHashCode()) * 23 + OverrideSiblingIndex.GetHashCode();
	}
}

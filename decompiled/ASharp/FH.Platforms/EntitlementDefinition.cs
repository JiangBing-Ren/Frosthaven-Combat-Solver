using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.Platforms;

[Serializable]
public struct EntitlementDefinition : IComparable<EntitlementDefinition>
{
	public string Internal_ID;

	public string EGS;

	public string EGSStore;

	public string STEAM;

	public string STEAMStore;

	public string GOG;

	public string XBOX;

	public string PSN;

	public string NINTENDO;

	[PlatformIDSelector]
	[SerializeField]
	private List<string> CompatiblePlatforms;

	public string ID => Internal_ID;

	public bool IsCompatibleWithPlatform(string platformID)
	{
		return CompatiblePlatforms.Contains(platformID);
	}

	public int CompareTo(EntitlementDefinition other)
	{
		return string.Compare(Internal_ID, other.Internal_ID, StringComparison.Ordinal);
	}
}

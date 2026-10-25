using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FH.Platforms;

[CreateAssetMenu(menuName = "Defs/Platforms/EntitlementsDatabase")]
public class EntitlementsDatabase : ScriptableObject
{
	[SerializeField]
	private List<EntitlementDefinition> Entitlements;

	public EntitlementDefinition GetEntitlement(string entitlementID)
	{
		if (string.IsNullOrEmpty(entitlementID))
		{
			return Entitlements.First();
		}
		for (int i = 0; i < Entitlements.Count; i++)
		{
			EntitlementDefinition result = Entitlements[i];
			if (result.Internal_ID == entitlementID)
			{
				return result;
			}
		}
		throw new Exception("Couldn't find definition for entitlement named: " + entitlementID);
	}

	public HashSet<EntitlementDefinition> GetCompatibleEntitlements(string platformID)
	{
		HashSet<EntitlementDefinition> hashSet = new HashSet<EntitlementDefinition>();
		foreach (EntitlementDefinition entitlement in Entitlements)
		{
			if (entitlement.IsCompatibleWithPlatform(platformID))
			{
				hashSet.Add(entitlement);
			}
		}
		return hashSet;
	}

	public List<EntitlementDefinition> GetAllEntitlements()
	{
		return new List<EntitlementDefinition>(Entitlements);
	}

	internal static EntitlementsDatabase LoadResource()
	{
		return Resources.Load<EntitlementsDatabase>("Settings/EntitlementsDatabase");
	}
}

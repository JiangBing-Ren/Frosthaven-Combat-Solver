using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

[CreateAssetMenu(menuName = "Asset Bundles/GH Asset Bundle Load Settings")]
public class BundleLoadSettings : ScriptableObject
{
	[Serializable]
	public class BundleLoadConfig : IEquatable<BundleLoadConfig>
	{
		[Serializable]
		public enum EBundleConfigType
		{
			None,
			Misc,
			Hero,
			NPC
		}

		[ReadOnlyField]
		public string AssetBundleName;

		[ReadOnlyField]
		public EBundleConfigType BundleConfigType;

		public DLCRegistry.EDLCKey BundleDLC;

		public bool AlwaysLoaded;

		public bool ExcludeFromBuild;

		public List<ECharacter> AssociatedCharacters;

		public List<CClass.ENPCModel> AssociatedNPCModels;

		public string AssetsBundleLoadPath => Path.Combine(Application.streamingAssetsPath, "AssetBundles", GloomUtility.GetEnumCategory(BundleDLC), AssetBundleName);

		public BundleLoadConfig(string bundleName)
		{
			AssetBundleName = bundleName;
		}

		public bool Equals(BundleLoadConfig other)
		{
			return AssetBundleName == other.AssetBundleName;
		}

		public override int GetHashCode()
		{
			return AssetBundleName.GetHashCode();
		}
	}

	public const string cMiscBundlePrefix = "misc";

	public const string cMiscBundleDirectoryName = "MiscBundles";

	public const string cHeroBundlePrefix = "hero";

	public const string cHeroBundleDirectoryName = "HeroBundles";

	public const string cNPCBundlePrefix = "npc";

	public const string cNPCBundleDirectoryName = "NPCBundles";

	public const string cDLCBundlePrefix = "dlc";

	public const string cDLCBundleDirectoryName = "DLCBundles";

	public List<BundleLoadConfig> BundleConfigs;

	public List<BundleLoadConfig> DLCBundleConfigs;

	public HashSet<string> GetBundlesForBuild(Func<string, bool> checkDefined = null)
	{
		string[] filters = new string[3] { "misc", "hero", "npc" };
		IEnumerable<string> first = from bc in BundleConfigs
			where !bc.ExcludeFromBuild && filters.Any((string f) => bc.AssetBundleName.StartsWith(f + "_"))
			select bc.AssetBundleName;
		IEnumerable<string> second = from bc in DLCBundleConfigs
			where !bc.ExcludeFromBuild && filters.Any((string f) => bc.AssetBundleName.StartsWith(f + "_"))
			select bc.AssetBundleName;
		HashSet<string> hashSet = new HashSet<string>(first.Concat(second));
		if (checkDefined != null && checkDefined("DEMO"))
		{
			string[] demoHeroes = new string[4] { "bannerspear", "boneshaper", "deathwalker", "drifter" };
			string s = string.Empty;
			hashSet.RemoveWhere((string b) => (s = b.ToLower()).StartsWith("hero") && !demoHeroes.Any((string a) => s.Contains("_" + a)));
			string[] demoNPCs = new string[30]
			{
				"algoxarcher", "algoxguard", "algoxpriest", "cityguard", "algoxscout", "hound", "guildmentor", "frozenfist", "snowdancer", "shadowbeast",
				"shadowhorror", "shamblingskeleton", "wraith", "ragingcorpse", "stitchedatrocity", "bonehorde", "reinforcement", "bannerofstrength", "torchbearer", "bannerofhope",
				"bannerofcourage", "trainedfalcon", "bannerofvalor", "longbowman", "bannerofdoom", "algoxsnowdancer", "algoxstormcaller", "algoxfrozenfist", "propdummyobject", "none"
			};
			hashSet.RemoveWhere((string b) => (s = b.ToLower()).StartsWith("npc") && !demoNPCs.Any((string a) => s.Contains("_" + a)));
			Debug.Log("[BundleLoadSettings] Demo mode: " + string.Join(", ", demoHeroes) + " and " + string.Join(", ", demoNPCs) + " are the only heroes and NPCs available in the demo. Bundles: " + string.Join(", ", hashSet));
		}
		return hashSet;
	}

	public List<BundleLoadConfig> GetBundlesNeededForRequirements(List<ECharacter> charactersNeeded, List<CClass.ENPCModel> npcModelsNeeded)
	{
		List<BundleLoadConfig> list = new List<BundleLoadConfig>();
		if (charactersNeeded != null)
		{
			foreach (ECharacter item in charactersNeeded)
			{
				foreach (BundleLoadConfig bundleConfig in BundleConfigs)
				{
					if (!list.Contains(bundleConfig) && bundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(bundleConfig);
					}
				}
				foreach (BundleLoadConfig dLCBundleConfig in DLCBundleConfigs)
				{
					if (!list.Contains(dLCBundleConfig) && dLCBundleConfig.AssociatedCharacters.Contains(item))
					{
						list.Add(dLCBundleConfig);
					}
				}
			}
		}
		if (npcModelsNeeded != null)
		{
			foreach (CClass.ENPCModel item2 in npcModelsNeeded)
			{
				foreach (BundleLoadConfig bundleConfig2 in BundleConfigs)
				{
					if (!list.Contains(bundleConfig2) && bundleConfig2.AssociatedNPCModels.Contains(item2))
					{
						list.Add(bundleConfig2);
					}
				}
				foreach (BundleLoadConfig dLCBundleConfig2 in DLCBundleConfigs)
				{
					if (!list.Contains(dLCBundleConfig2) && dLCBundleConfig2.AssociatedNPCModels.Contains(item2))
					{
						list.Add(dLCBundleConfig2);
					}
				}
			}
		}
		return list;
	}
}

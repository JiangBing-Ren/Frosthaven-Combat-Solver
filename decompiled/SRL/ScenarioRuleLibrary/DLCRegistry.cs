using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public static class DLCRegistry
{
	[Serializable]
	[Flags]
	public enum EDLCKey
	{
		[Category("Base")]
		[Description("NULL")]
		None = 0
	}

	public static readonly EDLCKey[] DLCKeys = (EDLCKey[])Enum.GetValues(typeof(EDLCKey));

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.EBiome>> m_DLCSpecificBiomes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubBiome>> m_DLCSpecificSubBiomes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETheme>> m_DLCSpecificThemes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ESubTheme>> m_DLCSpecificSubThemes;

	private static Dictionary<EDLCKey, List<ScenarioPossibleRoom.ETone>> m_DLCSpecificTones;

	private static Dictionary<EDLCKey, List<ECharacter>> m_DLCSpecificCharacters;

	private static Dictionary<EDLCKey, List<CClass.ENPCModel>> m_DLCSpecificNpcModel;

	private static Dictionary<EDLCKey, List<string>> m_DLCSpecificItemGuids;

	public static Dictionary<EDLCKey, List<ECharacter>> DLCCharactersWithConfigUIs;

	public static Dictionary<EDLCKey, List<string>> DLCSpecificSpriteAssets;

	public static Dictionary<EDLCKey, List<ECharacter>> AllDLCCharacters => m_DLCSpecificCharacters;

	public static EDLCKey GetDLCUsedInLevel(CCustomLevelData levelToCheck)
	{
		EDLCKey eDLCKey = EDLCKey.None;
		foreach (CApparanceOverrideDetails apparanceOverride in levelToCheck.ApparanceOverrideList)
		{
			eDLCKey |= GetDLCUsedInApparanceOverride(apparanceOverride);
		}
		foreach (PlayerState player in levelToCheck.ScenarioState.Players)
		{
			eDLCKey |= GetDLCUsedByPlayerState(player);
		}
		foreach (EnemyState allEnemyState in levelToCheck.ScenarioState.AllEnemyStates)
		{
			eDLCKey |= GetDLCUsedByEnemyState(allEnemyState);
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedInApparanceOverride(CApparanceOverrideDetails overrideDetail)
	{
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey2 in dLCKeys)
		{
			if (eDLCKey2 != EDLCKey.None && (m_DLCSpecificBiomes[eDLCKey2].Contains(overrideDetail.OverrideBiome) || m_DLCSpecificSubBiomes[eDLCKey2].Contains(overrideDetail.OverrideSubBiome) || m_DLCSpecificThemes[eDLCKey2].Contains(overrideDetail.OverrideTheme) || m_DLCSpecificSubThemes[eDLCKey2].Contains(overrideDetail.OverrideSubTheme) || m_DLCSpecificTones[eDLCKey2].Contains(overrideDetail.OverrideTone)))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedByPlayerState(PlayerState playerState)
	{
		ECharacter characterModel = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass m) => m.ID == playerState.ClassID).CharacterModel;
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey dlc in dLCKeys)
		{
			if (dlc != EDLCKey.None && (m_DLCSpecificCharacters[dlc].Contains(characterModel) || playerState.Items.Any((CItem i) => m_DLCSpecificItemGuids[dlc].Contains(i.ItemGuid))))
			{
				eDLCKey |= dlc;
			}
		}
		return eDLCKey;
	}

	public static EDLCKey GetDLCUsedByEnemyState(EnemyState enemyState)
	{
		CMonsterClass enemyClass = MonsterClassManager.MonsterAndObjectClasses.SingleOrDefault((CMonsterClass m) => m.ID == enemyState.ClassID);
		EDLCKey eDLCKey = EDLCKey.None;
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey2 in dLCKeys)
		{
			if (eDLCKey2 != EDLCKey.None && m_DLCSpecificNpcModel != null && m_DLCSpecificNpcModel.ContainsKey(eDLCKey2) && m_DLCSpecificNpcModel[eDLCKey2].Any((CClass.ENPCModel m) => enemyClass?.Models != null && enemyClass.Models.Contains(m.ToString())))
			{
				eDLCKey |= eDLCKey2;
			}
		}
		return eDLCKey;
	}

	public static List<EDLCKey> GetDLCListForFlag(EDLCKey dlcFlag)
	{
		List<EDLCKey> list = new List<EDLCKey>();
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != EDLCKey.None && dlcFlag.HasFlag(eDLCKey))
			{
				list.Add(eDLCKey);
			}
		}
		return list;
	}

	public static List<string> GetDLCNamesForFlag(EDLCKey dlcFlag)
	{
		List<string> list = new List<string>();
		EDLCKey[] dLCKeys = DLCKeys;
		foreach (EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != EDLCKey.None && dlcFlag.HasFlag(eDLCKey))
			{
				list.Add(dlcFlag.ToString());
			}
		}
		return list;
	}

	public static string GetDLCListAsStringFromFlag(EDLCKey dlcFlag)
	{
		List<string> dLCNamesForFlag = GetDLCNamesForFlag(dlcFlag);
		string text = ((dLCNamesForFlag.Count == 0) ? "-" : "");
		for (int i = 0; i < dLCNamesForFlag.Count; i++)
		{
			text += string.Format("{0}{1}", (i > 0) ? ", " : "", dLCNamesForFlag[i]);
		}
		return text;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Scenario.Use_Bar;

[CreateAssetMenu(fileName = "ActiveAbilityDataSO", menuName = "Def/ActiveAbilityDataSO")]
public class ActiveAbilityDataSO : ScriptableObject
{
	[Serializable]
	public struct QuestItemActiveBonusData : IEquatable<QuestItemActiveBonusData>
	{
		public ESpecificPropType SpecificPropType;

		public Sprite Icon;

		public string OverrideBonusName;

		public string OverrideBonusDescription;

		public bool Equals(QuestItemActiveBonusData other)
		{
			if (SpecificPropType == other.SpecificPropType && object.Equals(Icon, other.Icon) && OverrideBonusName == other.OverrideBonusName)
			{
				return OverrideBonusDescription == other.OverrideBonusDescription;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is QuestItemActiveBonusData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)SpecificPropType, Icon, OverrideBonusName, OverrideBonusDescription);
		}

		public static bool operator ==(QuestItemActiveBonusData left, QuestItemActiveBonusData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(QuestItemActiveBonusData left, QuestItemActiveBonusData right)
		{
			return !left.Equals(right);
		}
	}

	[Serializable]
	private struct ScenarioModifierActiveBonusData
	{
		public string ScenarioModifierID;

		public Sprite Icon;
	}

	[Serializable]
	public struct ResourceActiveBonusData : IEquatable<ResourceActiveBonusData>
	{
		public string ResourceID;

		public string NameLocKey;

		public string DescLocKey;

		public Sprite Icon;

		public bool Equals(ResourceActiveBonusData other)
		{
			if (ResourceID == other.ResourceID && NameLocKey == other.NameLocKey && DescLocKey == other.DescLocKey)
			{
				return object.Equals(Icon, other.Icon);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ResourceActiveBonusData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ResourceID, NameLocKey, DescLocKey, Icon);
		}

		public static bool operator ==(ResourceActiveBonusData left, ResourceActiveBonusData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ResourceActiveBonusData left, ResourceActiveBonusData right)
		{
			return !left.Equals(right);
		}
	}

	[SerializeField]
	private Sprite[] _activeAbilityIcons;

	[SerializeField]
	private Sprite[] _activeAbilityDurationIcons;

	[SerializeField]
	private Sprite[] _durationIcons;

	[SerializeField]
	private QuestItemActiveBonusData[] _questItems;

	[SerializeField]
	private ResourceActiveBonusData[] _resourceBonuses;

	private Lazy<Dictionary<string, Sprite>> _spriteMapper;

	private void Awake()
	{
		EnsureMapper();
	}

	private void EnsureMapper()
	{
		if (_spriteMapper != null)
		{
			return;
		}
		_spriteMapper = new Lazy<Dictionary<string, Sprite>>(() =>
		{
			Dictionary<string, Sprite> dictionary = new Dictionary<string, Sprite>();
			Sprite[] activeAbilityIcons = _activeAbilityIcons;
			foreach (Sprite sprite in activeAbilityIcons)
			{
				dictionary.TryAdd(sprite.name.ToLowerInvariant(), sprite);
			}
			return dictionary;
		});
	}

	public Sprite GetActiveAbilityIcon(string iconName, string fallbackIconName = null, bool displayError = true)
	{
		if (iconName.IsNullOrEmptyAfterTrimmed())
		{
			return null;
		}
		EnsureMapper();
		_spriteMapper.Value.TryGetValue(iconName.ToLowerInvariant(), out var value);
		if (value != null)
		{
			return value;
		}
		if (fallbackIconName.IsNOTNullOrEmpty())
		{
			_spriteMapper.Value.TryGetValue(fallbackIconName.ToLowerInvariant(), out value);
			if (value != null)
			{
				return value;
			}
		}
		if (value == null && iconName != null && !iconName.StartsWith("AA_"))
		{
			value = GetActiveAbilityIcon("AA_" + iconName, null, displayError);
		}
		if (value == null)
		{
			value = _activeAbilityIcons.FirstOrDefault((Sprite s) => s.name.Contains(iconName, StringComparison.OrdinalIgnoreCase));
		}
		if (value != null)
		{
			return value;
		}
		if (value == null)
		{
			value = UIInfoTools.Instance.MissingSpriteIcon;
			if (displayError)
			{
				Log.GUIWarning("Missing active ability icon " + iconName);
			}
		}
		return value;
	}

	private static bool IsMatch(string spriteName, string targetName, bool ignoreCase)
	{
		int length = targetName.Length;
		int length2 = spriteName.Length;
		for (int i = 0; i < length2; i++)
		{
			int num = i;
			int j = 0;
			while (j < length && num < length2)
			{
				char c = spriteName[num];
				char c2 = targetName[j];
				if (c == ' ' || c == '_')
				{
					num++;
					continue;
				}
				if (c2 == ' ' || c2 == '_')
				{
					j++;
					continue;
				}
				if (ignoreCase && (char.ToLowerInvariant(c) != char.ToLowerInvariant(c2) || c != c2))
				{
					break;
				}
				num++;
				j++;
			}
			for (; j < length && (targetName[j] == ' ' || targetName[j] == '_'); j++)
			{
			}
			if (j >= length)
			{
				return true;
			}
		}
		return false;
	}

	public Sprite GetActiveAbilityIcon(CActiveBonus activeBonus)
	{
		string text = null;
		if (activeBonus?.Layout?.IconNames != null && activeBonus.Layout.IconNames.Count > 0)
		{
			text = activeBonus.Layout.IconNames[0];
		}
		else if (activeBonus?.Ability?.Augment != null)
		{
			text = "Mindthief";
		}
		else if (activeBonus != null && activeBonus.IsSong)
		{
			text = "Soothsinger";
		}
		else if (activeBonus != null && activeBonus.Ability?.AbilityType == CAbility.EAbilityType.Shield && GetActiveAbilityIcon(text, null, displayError: false) == null)
		{
			text = "Voidwarden";
		}
		if (!text.IsNullOrEmpty())
		{
			return GetActiveAbilityIcon(text);
		}
		return GetActiveAbilityIcon(activeBonus?.Caster?.GetPrefabName(), null, displayError: false);
	}

	public Sprite GetActiveAbilityDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return _activeAbilityDurationIcons.GetSprite(duration.ToString(), toLower: true, useDefaultWhenMissing: true, UIInfoTools.Instance.BlankSpriteIcon);
	}

	public Sprite GetDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return _durationIcons.GetSprite(duration.ToString(), toLower: true, useDefaultWhenMissing: true, UIInfoTools.Instance.BlankSpriteIcon);
	}

	public string GetActiveAbilityIconName(CActiveBonus activeBonus)
	{
		return _activeAbilityIcons.GetObjectName((activeBonus?.Layout?.IconNames == null || activeBonus.Layout.IconNames.Count == 0) ? null : activeBonus.Layout.IconNames[0]);
	}

	public bool GetQuestItemData(ESpecificPropType specificPropType, out QuestItemActiveBonusData questItem)
	{
		questItem = _questItems.FirstOrDefault((QuestItemActiveBonusData c) => c.SpecificPropType == specificPropType);
		return !EqualityComparer<QuestItemActiveBonusData>.Default.Equals(questItem, default);
	}

	public Sprite GetResourceIcon(CCharacterResource resource)
	{
		if (!GetResourceData(resource.ID, out var data))
		{
			return null;
		}
		return data.Icon;
	}

	public bool GetResourceData(string resourceID, out ResourceActiveBonusData data)
	{
		data = _resourceBonuses.FirstOrDefault((ResourceActiveBonusData r) => r.ResourceID == resourceID);
		return !EqualityComparer<ResourceActiveBonusData>.Default.Equals(data, default);
	}

	public Sprite GetQuestItemIcon(ESpecificPropType specificPropType)
	{
		GetQuestItemData(specificPropType, out var questItem);
		return questItem.Icon;
	}
}

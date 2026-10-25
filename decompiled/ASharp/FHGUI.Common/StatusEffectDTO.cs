using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public record StatusEffectDTO
{
	public enum StatusEffectType
	{
		Positive,
		Negative,
		Permanent
	}

	public string ID;

	public string Name;

	public string Description;

	public Sprite Icon;

	public StatusEffectType EffectType;

	public int RoundDuration;

	public int Strength;

	public StatusEffectDTO()
	{
	}

	public StatusEffectDTO(CCondition.ENegativeCondition condition)
	{
		ID = condition.ToString();
		Name = LocalizationManager.GetTranslation(condition.ToString());
		Description = LocalizationManager.GetTranslation($"Glossary_{condition}");
		Icon = UIInfoTools.Instance.GetIconNegativeCondition(condition);
		RoundDuration = 1;
		Strength = 1;
		EffectType = StatusEffectType.Negative;
	}

	public StatusEffectDTO(CCondition.EPositiveCondition condition)
	{
		ID = condition.ToString();
		Name = LocalizationManager.GetTranslation(condition.ToString());
		Description = LocalizationManager.GetTranslation($"Glossary_{condition}");
		Icon = UIInfoTools.Instance.GetIconPositiveCondition(condition);
		RoundDuration = 1;
		Strength = 1;
		EffectType = StatusEffectType.Positive;
	}

	public StatusEffectDTO(NegativeConditionPair conditionPair)
		: this(conditionPair.NegativeCondition)
	{
		RoundDuration = conditionPair.RoundDuration;
		Strength = conditionPair.Strength;
	}

	public StatusEffectDTO(PositiveConditionPair conditionPair)
		: this(conditionPair.PositiveCondition)
	{
		RoundDuration = conditionPair.RoundDuration;
		Strength = 1;
	}

	public StatusEffectDTO(string id, string name, string description, int strength, Sprite icon, StatusEffectType type, bool localize = false)
	{
		ID = id;
		Name = (localize ? LocalizationManager.GetTranslation(name) : name);
		Description = (string.IsNullOrEmpty(description) ? description : (localize ? LocalizationManager.GetTranslation(description) : description));
		Icon = icon;
		RoundDuration = 1;
		Strength = strength;
		EffectType = type;
	}
}

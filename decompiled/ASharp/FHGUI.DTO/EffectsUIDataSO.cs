using System;
using System.Linq;
using FHGUI.Configs;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "EffectsUIDataSO", menuName = "Def/EffectsUIDataSO")]
public class EffectsUIDataSO : ScriptableObject
{
	[Serializable]
	public struct NegativeConditionIcon
	{
		public CCondition.ENegativeCondition Condition;

		[ConfigIcon(48, true)]
		public Sprite Icon;
	}

	[Serializable]
	public struct PositiveConditionIcon
	{
		public CCondition.EPositiveCondition Condition;

		[ConfigIcon(48, true)]
		public Sprite Icon;
	}

	[Serializable]
	public struct EffectIcon
	{
		public string EffectLocKey;

		[ConfigIcon(48, true)]
		public Sprite Icon;
	}

	[SerializeField]
	public NegativeConditionIcon[] NegativeConditions;

	[SerializeField]
	public PositiveConditionIcon[] PositiveConditions;

	[SerializeField]
	public EffectIcon[] Effects;

	public Sprite GetIcon(CCondition.ENegativeCondition condition)
	{
		NegativeConditionIcon negativeConditionIcon = NegativeConditions.FirstOrDefault((NegativeConditionIcon it) => it.Condition == condition);
		if (negativeConditionIcon.Icon == null)
		{
			Log.GUIError($"Not found enhancement icon for {condition}", this);
		}
		return negativeConditionIcon.Icon;
	}

	public Sprite GetIcon(CCondition.EPositiveCondition condition)
	{
		PositiveConditionIcon positiveConditionIcon = PositiveConditions.FirstOrDefault((PositiveConditionIcon it) => it.Condition == condition);
		if (positiveConditionIcon.Icon == null)
		{
			Log.GUIError($"Not found enhancement icon for {condition}", this);
		}
		return positiveConditionIcon.Icon;
	}

	public Sprite GetIcon(string locKey)
	{
		EffectIcon effectIcon = Effects.FirstOrDefault((EffectIcon it) => it.EffectLocKey.Equals(locKey, StringComparison.InvariantCulture));
		if (effectIcon.Icon == null)
		{
			if (Enum.TryParse<CCondition.ENegativeCondition>(locKey, out var result))
			{
				return GetIcon(result);
			}
			if (Enum.TryParse<CCondition.EPositiveCondition>(locKey, out var result2))
			{
				return GetIcon(result2);
			}
			Log.GUIError("Not found enhancement icon for " + locKey, this);
			return UIInfoTools.Instance.MissingSpriteIcon;
		}
		return effectIcon.Icon;
	}
}

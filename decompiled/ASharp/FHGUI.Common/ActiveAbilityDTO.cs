using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

namespace FHGUI.Common;

public struct ActiveAbilityDTO(string id, string localizedName, string description, Sprite icon, CActiveBonus.EActiveBonusDurationType duration = CActiveBonus.EActiveBonusDurationType.NA, DiscardType discard = DiscardType.None, int instances = 1)
{
	public string ID = id;

	public string Name = localizedName;

	public string Description = description;

	public Sprite Icon = icon;

	public CActiveBonus.EActiveBonusDurationType Duration = duration;

	public DiscardType Discard = discard;

	[CanBeNull]
	public CActiveBonus Bonus = null;

	public int Instances = instances;

	public ActiveAbilityDTO(CActiveBonus activeBonus, CActor contextActor = null, int instances = 1)
		: this(activeBonus.ID.ToString(), ActiveBonusUIUtils.GetActiveBonusName(activeBonus, contextActor), ActiveBonusUIUtils.GetActiveBonusDescription(activeBonus, contextActor), ActiveBonusUIUtils.GetActiveBonusIcon(activeBonus, contextActor), activeBonus.Duration, activeBonus.Layout?.Discard ?? DiscardType.None, instances)
	{
		Bonus = activeBonus;
		if (activeBonus.Layout == null)
		{
			UIActiveAbility.CheckForLayoutToGenerate(activeBonus);
			Discard = activeBonus.Layout?.Discard ?? DiscardType.None;
		}
	}
}

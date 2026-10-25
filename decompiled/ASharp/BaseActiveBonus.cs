using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public abstract class BaseActiveBonus<T> : IActiveBonus where T : CActiveBonus
{
	protected T bonus;

	protected CActor actor;

	public virtual bool IsToggled => bonus.ToggledBonus;

	public virtual bool IsToggleLocked => bonus.ToggleLocked;

	protected BaseActiveBonus(T bonus, CActor actor)
	{
		this.bonus = bonus;
		this.actor = actor;
	}

	public List<ElementInfusionBoardManager.EElement> GetConsumes()
	{
		return bonus.Ability.ActiveBonusData.Consuming;
	}

	public abstract Sprite GetIcon();

	public ElementInfusionBoardManager.EElement? GetSelectedConsume()
	{
		return bonus.ToggledElement;
	}

	public abstract void ToggleActiveBonus(ElementInfusionBoardManager.EElement? eElement, bool fromClick = false);

	public abstract void UntoggleActiveBonus(bool fromClick = false);

	public virtual string GetSelectAudioItem()
	{
		return UIInfoTools.Instance.toggleUseCharacterSlotAudioItem;
	}

	public T GetBonus()
	{
		return bonus;
	}
}

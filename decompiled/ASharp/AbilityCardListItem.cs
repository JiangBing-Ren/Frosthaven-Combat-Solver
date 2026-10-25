using System;
using ScenarioRuleLibrary;
using UnityEngine;

public class AbilityCardListItem : MonoBehaviour
{
	[SerializeField]
	private RectTransform holder;

	private AbilityCardUI abilityCard;

	private bool highlighted;

	public bool Selected
	{
		get
		{
			return abilityCard.IsSelected;
		}
		set
		{
			abilityCard.ToggleSelect(value, highlighted);
		}
	}

	public int Index { get; set; }

	public event EventHandler MouseEnterEvent;

	public event EventHandler MouseExitEvent;

	public event EventHandler ClickEvent;

	protected virtual void OnMouseEnterEvent()
	{
		if (MouseEnterEvent != null)
		{
			MouseEnterEvent(this, EventArgs.Empty);
		}
	}

	protected virtual void OnMouseExitEvent()
	{
		if (MouseExitEvent != null)
		{
			MouseExitEvent(this, EventArgs.Empty);
		}
	}

	public void MouseEnter()
	{
		highlighted = true;
		SetBackground();
		OnMouseEnterEvent();
	}

	public void MouseExit()
	{
		highlighted = false;
		SetBackground();
		OnMouseExitEvent();
	}

	protected virtual void OnClickEvent()
	{
		if (ClickEvent != null)
		{
			ClickEvent(this, EventArgs.Empty);
		}
	}

	private void SetBackground()
	{
		abilityCard.ToggleSelect(Selected, highlighted);
	}

	public bool Init(CAbilityCard card, string characterName, Transform cardHighlightHolder)
	{
		GameObject gameObject = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Ability, holder);
		if (gameObject != null)
		{
			abilityCard = gameObject.GetComponent<AbilityCardUI>();
			abilityCard.transform.localScale = Vector3.one;
			abilityCard.Init(card, cardHighlightHolder, characterName, (AbilityCardUI cardUI, bool networkAction) =>
			{
				OnClickEvent();
			}, (AbilityCardUI cardUI, bool networkAction) =>
			{
				OnClickEvent();
			}, null);
			abilityCard.PlayEffect(InitiativeTrack.InitiativeEffects.None);
			return true;
		}
		return false;
	}

	private void OnDisable()
	{
		ObjectPool.RecycleCard(abilityCard.CardID, ObjectPool.ECardType.Ability, abilityCard.gameObject);
	}
}

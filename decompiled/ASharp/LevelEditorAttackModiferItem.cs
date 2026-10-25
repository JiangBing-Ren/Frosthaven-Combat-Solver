using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorAttackModiferItem : MonoBehaviour
{
	public TextMeshProUGUI DescText;

	public Toggle AvailableToggle;

	public Toggle DiscardToggle;

	public Button ReorderUpButton;

	public Button ReorderDownButton;

	public UnityAction<LevelEditorAttackModiferItem> TogglesChangedAction;

	public UnityAction<LevelEditorAttackModiferItem> DeleteButtonPressedAction;

	public UnityAction<LevelEditorAttackModiferItem, bool> ReorderPressedAction;

	public AttackModifierYMLData AttackModifier { get; private set; }

	public EAttackModiferPile CardPile { get; private set; }

	public int CardIndex { get; private set; }

	public void InitForModifier(AttackModifierYMLData modifier, EAttackModiferPile pile, int cardIndex)
	{
		AttackModifier = modifier;
		DescText.text = modifier.Name + " (" + modifier.MathModifier + ")";
		CardPile = pile;
		CardIndex = cardIndex;
		if (CardPile == EAttackModiferPile.Discarded)
		{
			DiscardToggle.isOn = true;
			AvailableToggle.isOn = false;
		}
		else
		{
			AvailableToggle.isOn = true;
			DiscardToggle.isOn = false;
		}
	}

	public void OnHandClicked()
	{
		if (CardPile != EAttackModiferPile.Available && AvailableToggle.isOn)
		{
			CardPile = EAttackModiferPile.Available;
			TogglesChangedAction?.Invoke(this);
		}
	}

	public void OnDiscardClicked()
	{
		if (CardPile != EAttackModiferPile.Discarded && DiscardToggle.isOn)
		{
			CardPile = EAttackModiferPile.Discarded;
			TogglesChangedAction?.Invoke(this);
		}
	}

	public void DeletePressed()
	{
		DeleteButtonPressedAction?.Invoke(this);
	}

	public void OnReorderInListPressed(bool moveUp)
	{
		ReorderPressedAction?.Invoke(this, moveUp);
	}
}

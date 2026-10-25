using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditorAbilityDeckItem : MonoBehaviour
{
	public Button BGButton;

	public TextMeshProUGUI DescText;

	public Toggle HandToggle;

	public Toggle DiscardToggle;

	public Toggle LostToggle;

	public Toggle PermaLostToggle;

	public Button ReorderUpButton;

	public Button ReorderDownButton;

	public UnityAction<LevelEditorAbilityDeckItem, EAbilityPile> TogglesChangedAction;

	public UnityAction<LevelEditorAbilityDeckItem> DeleteButtonPressedAction;

	public UnityAction<LevelEditorAbilityDeckItem, bool> ReorderPressedAction;

	public UnityAction<LevelEditorAbilityDeckItem> BGButtonPressedAction;

	private bool m_PreventToggleEvents;

	public CAbilityCard AbilityOnDisplay { get; private set; }

	public CMonsterAbilityCard MonsterAbilityOnDisplay { get; private set; }

	public EAbilityPile CardPile { get; private set; }

	public void InitForAbility(CAbilityCard ability, EAbilityPile pile)
	{
		AbilityOnDisplay = ability;
		DescText.text = $"{AbilityOnDisplay.ID} [I:{AbilityOnDisplay.Initiative}]";
		CardPile = pile;
		SetToggle();
	}

	public void InitForMonsterAbility(CMonsterAbilityCard monsterAbility, EAbilityPile pile)
	{
		LostToggle.gameObject.SetActive(value: false);
		PermaLostToggle.gameObject.SetActive(value: false);
		MonsterAbilityOnDisplay = monsterAbility;
		DescText.text = MonsterAbilityOnDisplay.ID + "[I:" + MonsterAbilityOnDisplay.Initiative + "]";
		CardPile = pile;
		SetToggle();
		BGButton.enabled = true;
	}

	public void OnHandClicked()
	{
		if (!m_PreventToggleEvents && CardPile != EAbilityPile.Hand && HandToggle.isOn)
		{
			EAbilityPile cardPile = CardPile;
			CardPile = EAbilityPile.Hand;
			TogglesChangedAction?.Invoke(this, cardPile);
		}
	}

	public void OnDiscardedClicked()
	{
		if (!m_PreventToggleEvents && CardPile != EAbilityPile.Discarded && DiscardToggle.isOn)
		{
			EAbilityPile cardPile = CardPile;
			CardPile = EAbilityPile.Discarded;
			TogglesChangedAction?.Invoke(this, cardPile);
		}
	}

	public void OnLostClicked()
	{
		if (!m_PreventToggleEvents && CardPile != EAbilityPile.Lost && LostToggle.isOn)
		{
			EAbilityPile cardPile = CardPile;
			CardPile = EAbilityPile.Lost;
			TogglesChangedAction?.Invoke(this, cardPile);
		}
	}

	public void OnPermaLostClicked()
	{
		if (!m_PreventToggleEvents && CardPile != EAbilityPile.PermaLost && PermaLostToggle.isOn)
		{
			EAbilityPile cardPile = CardPile;
			CardPile = EAbilityPile.PermaLost;
			TogglesChangedAction?.Invoke(this, cardPile);
		}
	}

	public void OnReorderInListPressed(bool moveUp)
	{
		ReorderPressedAction?.Invoke(this, moveUp);
	}

	public void DeletePressed()
	{
		DeleteButtonPressedAction?.Invoke(this);
	}

	public void BGButtonPressed()
	{
		BGButtonPressedAction?.Invoke(this);
	}

	private void SetToggle()
	{
		m_PreventToggleEvents = true;
		HandToggle.isOn = CardPile == EAbilityPile.Hand;
		DiscardToggle.isOn = CardPile == EAbilityPile.Discarded;
		LostToggle.isOn = CardPile == EAbilityPile.Lost;
		PermaLostToggle.isOn = CardPile == EAbilityPile.PermaLost;
		m_PreventToggleEvents = false;
	}
}

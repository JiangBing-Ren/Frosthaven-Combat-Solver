using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using Libs.Utils;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorAbilityDeckPanel : MonoBehaviour
{
	public TextMeshProUGUI ListTitle;

	public LayoutGroup StartingHandParent;

	public GameObject StartingHandItemPrefab;

	public TMP_Dropdown AbilityAddDropdown;

	private IEnumerator m_OptionalMonsterCardDisplayRoutine;

	private bool m_IsShowingForEnemy;

	private CPlayerActor m_PlayerShowingFor;

	private PlayerState m_PlayerStateShowingFor;

	private CEnemyActor m_EnemyShowingFor;

	private CMonsterClass m_MonsterClassShowingFor;

	private List<LevelEditorAbilityDeckItem> m_AbilityDeckItems;

	private void OnDisable()
	{
		CanvasGroup[] array = new CanvasGroup[2]
		{
			LevelEditorController.s_Instance.m_LevelEditorUIInstance.AbilityCardUI?.GetComponentInParent<CanvasGroup>(),
			LevelEditorController.s_Instance.m_LevelEditorUIInstance.MonsterCardUI?.GetComponentInParent<CanvasGroup>()
		};
		foreach (CanvasGroup canvasGroup in array)
		{
			if (canvasGroup != null)
			{
				canvasGroup.alpha = 0f;
			}
		}
	}

	public void ResetAbilityDeckToDefault()
	{
		if (m_IsShowingForEnemy)
		{
			m_MonsterClassShowingFor.ResetAbilityCardsToDefault();
			FillList();
		}
	}

	public void DisplayAbilityDeckForPlayer(CPlayerActor playerActor)
	{
		ListTitle.text = "Ability Deck for " + LocalizationManager.GetTranslation(playerActor.ActorLocKey()) + ":";
		m_PlayerShowingFor = playerActor;
		FillList();
	}

	public void DisplayAbilityDeckForEnemyClass(CEnemyActor enemyActor)
	{
		m_IsShowingForEnemy = true;
		m_EnemyShowingFor = enemyActor;
		m_MonsterClassShowingFor = enemyActor.MonsterClass;
		ListTitle.text = "Ability Deck for Enemy CLASS " + LocalizationManager.GetTranslation(m_MonsterClassShowingFor.LocKey) + ":";
		m_MonsterClassShowingFor.ClearRoundAbilityCardForEditor();
		FillList();
	}

	public void AddItemForAbility(CAbilityCard abilityToAdd, EAbilityPile pile)
	{
		LevelEditorAbilityDeckItem component = Object.Instantiate(StartingHandItemPrefab, StartingHandParent.transform).GetComponent<LevelEditorAbilityDeckItem>();
		component.InitForAbility(abilityToAdd, pile);
		component.TogglesChangedAction = AbilityStateChanged;
		component.DeleteButtonPressedAction = AbilityDeleted;
		component.ReorderPressedAction = AbilityReordered;
		component.BGButtonPressedAction = DisplayAbilityCard;
		m_AbilityDeckItems.Add(component);
	}

	public void AddItemForMonsterAbility(CMonsterAbilityCard monsterAbilityToAdd, EAbilityPile pile)
	{
		LevelEditorAbilityDeckItem component = Object.Instantiate(StartingHandItemPrefab, StartingHandParent.transform).GetComponent<LevelEditorAbilityDeckItem>();
		component.InitForMonsterAbility(monsterAbilityToAdd, pile);
		component.TogglesChangedAction = AbilityStateChanged;
		component.DeleteButtonPressedAction = AbilityDeleted;
		component.ReorderPressedAction = AbilityReordered;
		component.BGButtonPressedAction = DisplayMonsterCard;
		m_AbilityDeckItems.Add(component);
	}

	public void AbilityStateChanged(LevelEditorAbilityDeckItem itemChanged, EAbilityPile previousState)
	{
		if (m_IsShowingForEnemy)
		{
			switch (previousState)
			{
			case EAbilityPile.Hand:
				m_MonsterClassShowingFor.AbilityCards.Remove(itemChanged.MonsterAbilityOnDisplay);
				break;
			case EAbilityPile.Discarded:
				m_MonsterClassShowingFor.DiscardedAbilityCards.Remove(itemChanged.MonsterAbilityOnDisplay);
				break;
			}
			switch (itemChanged.CardPile)
			{
			case EAbilityPile.Hand:
				m_MonsterClassShowingFor.AbilityCards.Add(itemChanged.MonsterAbilityOnDisplay);
				break;
			case EAbilityPile.Discarded:
				m_MonsterClassShowingFor.DiscardedAbilityCards.Add(itemChanged.MonsterAbilityOnDisplay);
				break;
			}
			return;
		}
		switch (previousState)
		{
		case EAbilityPile.Hand:
			m_PlayerShowingFor.CharacterClass.HandAbilityCards.Remove(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.Discarded:
			m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards.Remove(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.Lost:
			m_PlayerShowingFor.CharacterClass.LostAbilityCards.Remove(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.PermaLost:
			m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards.Remove(itemChanged.AbilityOnDisplay);
			break;
		}
		switch (itemChanged.CardPile)
		{
		case EAbilityPile.Hand:
			m_PlayerShowingFor.CharacterClass.HandAbilityCards.Add(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.Discarded:
			m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards.Add(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.Lost:
			m_PlayerShowingFor.CharacterClass.LostAbilityCards.Add(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.PermaLost:
			m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards.Add(itemChanged.AbilityOnDisplay);
			break;
		case EAbilityPile.Round:
		case EAbilityPile.Activated:
			break;
		}
	}

	public void AbilityDeleted(LevelEditorAbilityDeckItem itemDeleted)
	{
		if (m_IsShowingForEnemy)
		{
			switch (itemDeleted.CardPile)
			{
			case EAbilityPile.Hand:
				m_MonsterClassShowingFor.AbilityCards.Remove(itemDeleted.MonsterAbilityOnDisplay);
				break;
			case EAbilityPile.Discarded:
				m_MonsterClassShowingFor.DiscardedAbilityCards.Remove(itemDeleted.MonsterAbilityOnDisplay);
				break;
			}
		}
		else
		{
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.RemoveAll((CAbilityCard a) => a.ID == itemDeleted.AbilityOnDisplay.ID);
			switch (itemDeleted.CardPile)
			{
			case EAbilityPile.Hand:
				m_PlayerShowingFor.CharacterClass.HandAbilityCards.RemoveAll((CAbilityCard a) => a.ID == itemDeleted.AbilityOnDisplay.ID);
				break;
			case EAbilityPile.Discarded:
				m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards.RemoveAll((CAbilityCard a) => a.ID == itemDeleted.AbilityOnDisplay.ID);
				break;
			case EAbilityPile.Lost:
				m_PlayerShowingFor.CharacterClass.LostAbilityCards.RemoveAll((CAbilityCard a) => a.ID == itemDeleted.AbilityOnDisplay.ID);
				break;
			case EAbilityPile.PermaLost:
				m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards.RemoveAll((CAbilityCard a) => a.ID == itemDeleted.AbilityOnDisplay.ID);
				break;
			}
		}
		GameObject obj = itemDeleted.gameObject;
		m_AbilityDeckItems.Remove(itemDeleted);
		Object.Destroy(obj);
		RefreshAddDropDown();
	}

	public void AbilityReordered(LevelEditorAbilityDeckItem itemReordered, bool movedUp)
	{
		if (m_IsShowingForEnemy)
		{
			List<CMonsterAbilityCard> list = null;
			switch (itemReordered.CardPile)
			{
			case EAbilityPile.Hand:
				list = m_MonsterClassShowingFor.AbilityCards;
				break;
			case EAbilityPile.Discarded:
				list = m_MonsterClassShowingFor.DiscardedAbilityCards;
				break;
			}
			if (list != null)
			{
				int num = list.IndexOf(itemReordered.MonsterAbilityOnDisplay);
				int num2 = (movedUp ? (num - 1) : (num + 1));
				num2 = ((num2 >= 0) ? ((num2 > list.Count - 1) ? (list.Count - 1) : num2) : 0);
				list.RemoveAt(num);
				list.Insert(num2, itemReordered.MonsterAbilityOnDisplay);
			}
		}
		else
		{
			List<CAbilityCard> list2 = null;
			switch (itemReordered.CardPile)
			{
			case EAbilityPile.Hand:
				list2 = m_PlayerShowingFor.CharacterClass.HandAbilityCards;
				break;
			case EAbilityPile.Discarded:
				list2 = m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards;
				break;
			case EAbilityPile.Lost:
				list2 = m_PlayerShowingFor.CharacterClass.LostAbilityCards;
				break;
			case EAbilityPile.PermaLost:
				list2 = m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards;
				break;
			}
			if (list2 != null)
			{
				int num3 = list2.IndexOf(itemReordered.AbilityOnDisplay);
				int num4 = (movedUp ? (num3 - 1) : (num3 + 1));
				num4 = ((num4 >= 0) ? ((num4 > list2.Count - 1) ? (list2.Count - 1) : num4) : 0);
				list2.RemoveAt(num3);
				list2.Insert(num4, itemReordered.AbilityOnDisplay);
			}
		}
		FillList();
	}

	public void DisplayAbilityCard(LevelEditorAbilityDeckItem item)
	{
		CAbilityCard abilityOnDisplay = item.AbilityOnDisplay;
		GameObject abilityCardUI = LevelEditorController.s_Instance.m_LevelEditorUIInstance.AbilityCardUI;
		if (abilityCardUI != null)
		{
			if (PersistentData.CreateAbilityCard(abilityOnDisplay.GetAbilityCardYML, out var newAbilityCard, out var abilityCardUI2, generateLayout: true, usePool: false))
			{
				abilityCardUI.transform.DestroyChildren(immediate: true);
				newAbilityCard.transform.SetParent(abilityCardUI.transform);
				newAbilityCard.transform.localScale = new Vector3(1f, 1f, 1f);
				((RectTransform)newAbilityCard.transform).anchoredPosition = Vector3.zero;
				abilityCardUI2.MiniAbilityCard.gameObject.SetActive(value: false);
				abilityCardUI2.Init(abilityOnDisplay, disableEventDetection: true);
				abilityCardUI2.ToggleFullCard(active: true);
				abilityCardUI2.fullAbilityCard.BlockRaycasts(block: false);
				DisplayCardTemporarily(abilityCardUI);
			}
			else
			{
				Debug.LogError("Failed to create ability card ID: " + abilityOnDisplay.ID + " CardInstanceID: " + abilityOnDisplay.CardInstanceID);
			}
		}
	}

	public void DisplayMonsterCard(LevelEditorAbilityDeckItem item)
	{
		CMonsterAbilityCard monsterAbilityOnDisplay = item.MonsterAbilityOnDisplay;
		if (LevelEditorController.s_Instance.m_LevelEditorUIInstance.MonsterCardUI != null)
		{
			LevelEditorController.s_Instance.m_LevelEditorUIInstance.MonsterCardUI.GenerateCard(monsterAbilityOnDisplay, m_EnemyShowingFor);
			DisplayCardTemporarily(LevelEditorController.s_Instance.m_LevelEditorUIInstance.MonsterCardUI.gameObject);
		}
	}

	private void ClearList()
	{
		if (m_AbilityDeckItems == null)
		{
			m_AbilityDeckItems = new List<LevelEditorAbilityDeckItem>();
			return;
		}
		for (int i = 0; i < m_AbilityDeckItems.Count; i++)
		{
			Object.Destroy(m_AbilityDeckItems[i].gameObject);
		}
		m_AbilityDeckItems.Clear();
	}

	private void FillList()
	{
		ClearList();
		if (m_IsShowingForEnemy)
		{
			foreach (CMonsterAbilityCard abilityCard in m_MonsterClassShowingFor.AbilityCards)
			{
				AddItemForMonsterAbility(abilityCard, EAbilityPile.Hand);
			}
			foreach (CMonsterAbilityCard discardedAbilityCard in m_MonsterClassShowingFor.DiscardedAbilityCards)
			{
				AddItemForMonsterAbility(discardedAbilityCard, EAbilityPile.Discarded);
			}
		}
		else
		{
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.Clear();
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.HandAbilityCards);
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards);
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.ActivatedAbilityCards);
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.RoundAbilityCards);
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.LostAbilityCards);
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.AddRange(m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards);
			foreach (CAbilityCard handAbilityCard in m_PlayerShowingFor.CharacterClass.HandAbilityCards)
			{
				AddItemForAbility(handAbilityCard, EAbilityPile.Hand);
			}
			foreach (CAbilityCard discardedAbilityCard2 in m_PlayerShowingFor.CharacterClass.DiscardedAbilityCards)
			{
				AddItemForAbility(discardedAbilityCard2, EAbilityPile.Discarded);
			}
			foreach (CAbilityCard activatedAbilityCard in m_PlayerShowingFor.CharacterClass.ActivatedAbilityCards)
			{
				AddItemForAbility(activatedAbilityCard, EAbilityPile.Activated);
			}
			foreach (CAbilityCard roundAbilityCard in m_PlayerShowingFor.CharacterClass.RoundAbilityCards)
			{
				AddItemForAbility(roundAbilityCard, EAbilityPile.Round);
			}
			foreach (CAbilityCard lostAbilityCard in m_PlayerShowingFor.CharacterClass.LostAbilityCards)
			{
				AddItemForAbility(lostAbilityCard, EAbilityPile.Lost);
			}
			foreach (CAbilityCard permanentlyLostAbilityCard in m_PlayerShowingFor.CharacterClass.PermanentlyLostAbilityCards)
			{
				AddItemForAbility(permanentlyLostAbilityCard, EAbilityPile.PermaLost);
			}
		}
		RefreshAddDropDown();
	}

	private void RefreshAddDropDown()
	{
		AbilityAddDropdown.ClearOptions();
		AbilityAddDropdown.options.Add(new TMP_Dropdown.OptionData("<SELECT ABILITY TO ADD>"));
		if (m_IsShowingForEnemy)
		{
			AbilityAddDropdown.AddOptions(m_MonsterClassShowingFor.AbilityCardsPool.Select((CMonsterAbilityCard s) => LevelEditorUI.FormatFHName(s.Name)).ToList());
		}
		else
		{
			AbilityAddDropdown.AddOptions((from s in m_PlayerShowingFor.CharacterClass.AbilityCardsPool
				where m_AbilityDeckItems.All((LevelEditorAbilityDeckItem a) => a.AbilityOnDisplay.Name != s.Name)
				select LevelEditorUI.FormatFHName(s.Name)).ToList());
		}
		AbilityAddDropdown.value = 0;
	}

	public void AddAbilityPressed()
	{
		if (AbilityAddDropdown.value <= 0)
		{
			return;
		}
		if (m_IsShowingForEnemy)
		{
			CMonsterAbilityCard cMonsterAbilityCard = m_MonsterClassShowingFor.AbilityCardsPool.SingleOrDefault((CMonsterAbilityCard s) => LevelEditorUI.FormatFHName(s.Name) == AbilityAddDropdown.options[AbilityAddDropdown.value].text);
			if (cMonsterAbilityCard != null)
			{
				m_MonsterClassShowingFor.AbilityCards.Add(cMonsterAbilityCard);
				AddItemForMonsterAbility(cMonsterAbilityCard, EAbilityPile.Hand);
				RefreshAddDropDown();
			}
			return;
		}
		CAbilityCard cAbilityCard = m_PlayerShowingFor.CharacterClass.AbilityCardsPool.SingleOrDefault((CAbilityCard s) => LevelEditorUI.FormatFHName(s.Name) == AbilityAddDropdown.options[AbilityAddDropdown.value].text);
		if (cAbilityCard != null && !m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.Contains(cAbilityCard))
		{
			m_PlayerShowingFor.CharacterClass.SelectedAbilityCards.Add(cAbilityCard);
			m_PlayerShowingFor.CharacterClass.HandAbilityCards.Add(cAbilityCard);
			AddItemForAbility(cAbilityCard, EAbilityPile.Hand);
			RefreshAddDropDown();
		}
	}

	private void DisplayCardTemporarily(GameObject cardUI)
	{
		if (m_OptionalMonsterCardDisplayRoutine != null)
		{
			StopCoroutine(m_OptionalMonsterCardDisplayRoutine);
			m_OptionalMonsterCardDisplayRoutine = null;
		}
		m_OptionalMonsterCardDisplayRoutine = DisplayCardTemporarilyCrt(cardUI.GetComponentInParent<CanvasGroup>());
		StartCoroutine(m_OptionalMonsterCardDisplayRoutine);
	}

	private IEnumerator DisplayCardTemporarilyCrt(CanvasGroup canvasGroup)
	{
		float timeToDisplay = 4f;
		yield return GloomUtility.FadeCanvasGroup(canvasGroup, 0f, 0.3f, 1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
		yield return new WaitForSecondsRealtime(timeToDisplay);
		yield return GloomUtility.FadeCanvasGroup(canvasGroup, 0f, 0.3f, 0f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
	}
}

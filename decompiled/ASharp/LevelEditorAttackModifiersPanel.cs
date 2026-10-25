using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorAttackModifiersPanel : MonoBehaviour
{
	public enum ELevelEditorAttackModifierPanelDisplayType
	{
		None,
		Player,
		Enemy,
		ScenarioModifier
	}

	public TextMeshProUGUI ListTitle;

	public LayoutGroup AttackModifiersParent;

	public GameObject AttackModifierItemPrefab;

	public TMP_Dropdown ModifierAddDropdown;

	public List<string> ScenarioModifierAttackModifierCardNames;

	private ELevelEditorAttackModifierPanelDisplayType m_ShowingForDisplayType;

	private CPlayerActor m_PlayerShowingFor;

	private List<LevelEditorAttackModiferItem> m_ModifierItems;

	public void DisplayAttackModifiersForPlayer(CPlayerActor playerToShowFor)
	{
		ListTitle.text = "Attack Modifiers for " + LocalizationManager.GetTranslation(playerToShowFor.ActorLocKey()) + ":";
		m_ShowingForDisplayType = ELevelEditorAttackModifierPanelDisplayType.Player;
		m_PlayerShowingFor = playerToShowFor;
		ScenarioModifierAttackModifierCardNames = null;
		FillList();
	}

	public void DisplayAttackModifiersForEnemies()
	{
		ListTitle.text = "Attack Modifiers for all Enemies:";
		m_ShowingForDisplayType = ELevelEditorAttackModifierPanelDisplayType.Enemy;
		m_PlayerShowingFor = null;
		ScenarioModifierAttackModifierCardNames = null;
		FillList();
	}

	public void DisplayAttackModifiersForScenarioModifier(List<string> scenarioModifierAttackModifierCardNames)
	{
		ListTitle.text = "Attack Modifiers for Scenario Modifier to Add:";
		m_ShowingForDisplayType = ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier;
		m_PlayerShowingFor = null;
		ScenarioModifierAttackModifierCardNames = scenarioModifierAttackModifierCardNames;
		FillList();
	}

	private void FillList()
	{
		ClearList();
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
		{
			for (int num = 0; num < m_PlayerShowingFor.CharacterClass.AttackModifierCards.Count; num++)
			{
				AddItemForAttackModifer(m_PlayerShowingFor.CharacterClass.AttackModifierCards[num], EAttackModiferPile.Available, num);
			}
			for (int num2 = 0; num2 < m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.Count; num2++)
			{
				AddItemForAttackModifer(m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards[num2], EAttackModiferPile.Discarded, num2);
			}
			break;
		}
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
		{
			for (int num3 = 0; num3 < MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count; num3++)
			{
				AddItemForAttackModifer(MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards[num3], EAttackModiferPile.Available, num3);
			}
			for (int num4 = 0; num4 < MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count; num4++)
			{
				AddItemForAttackModifer(MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards[num4], EAttackModiferPile.Discarded, num4);
			}
			break;
		}
		case ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier:
		{
			int i;
			for (i = 0; i < ScenarioModifierAttackModifierCardNames.Count; i++)
			{
				AttackModifierYMLData attackModifierYMLData = null;
				if (ScenarioModifierAttackModifierCardNames[i] == "Bless")
				{
					attackModifierYMLData = AttackModifiersYML.CreateBless();
				}
				else
				{
					attackModifierYMLData = ((!(ScenarioModifierAttackModifierCardNames[i] == "Curse")) ? ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == ScenarioModifierAttackModifierCardNames[i]) : AttackModifiersYML.CreateCurse());
				}
				AddItemForAttackModifer(attackModifierYMLData, EAttackModiferPile.Available, i);
			}
			break;
		}
		}
		RefreshAddDropDown();
	}

	private void RefreshAddDropDown()
	{
		ModifierAddDropdown.ClearOptions();
		ModifierAddDropdown.AddOptions(new List<string> { "<SELECT ATTACK MODIFIER TO ADD>", "Bless", "Curse" });
		List<string> list = null;
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
			list = (from s in ScenarioRuleClient.SRLYML.Characters.Single((CharacterYMLData y) => y.ID == m_PlayerShowingFor.CharacterClass.ID).AttackModifierDeck.GetAllAttackModifiers.Distinct(new AttackModifierYMLComparer())
				select s.Name).ToList();
			break;
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
			list = (from s in MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCardsPool.Distinct(new AttackModifierYMLComparer())
				select s.Name).ToList();
			break;
		case ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier:
			list = (from s in MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCardsPool.Distinct(new AttackModifierYMLComparer())
				select s.Name).ToList();
			break;
		}
		list.Sort();
		ModifierAddDropdown.AddOptions(list);
		ModifierAddDropdown.value = 0;
	}

	private void AddItemForAttackModifer(AttackModifierYMLData attackModifer, EAttackModiferPile pile, int cardIndex)
	{
		LevelEditorAttackModiferItem component = Object.Instantiate(AttackModifierItemPrefab, AttackModifiersParent.transform).GetComponent<LevelEditorAttackModiferItem>();
		component.TogglesChangedAction = ModifierStateChanged;
		component.DeleteButtonPressedAction = ModifierDeleted;
		component.ReorderPressedAction = ModifierReordered;
		component.InitForModifier(attackModifer, pile, cardIndex);
		m_ModifierItems.Add(component);
	}

	private void ClearList()
	{
		if (m_ModifierItems == null)
		{
			m_ModifierItems = new List<LevelEditorAttackModiferItem>();
			return;
		}
		for (int i = 0; i < m_ModifierItems.Count; i++)
		{
			Object.Destroy(m_ModifierItems[i].gameObject);
		}
		m_ModifierItems.Clear();
	}

	private void ModifierStateChanged(LevelEditorAttackModiferItem itemChanged)
	{
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
			switch (itemChanged.CardPile)
			{
			case EAttackModiferPile.Available:
				m_PlayerShowingFor.CharacterClass.AttackModifierCards.Add(itemChanged.AttackModifier);
				if (m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.Any((AttackModifierYMLData a) => a.Name == itemChanged.AttackModifier.Name))
				{
					m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.Remove(m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemChanged.AttackModifier.Name));
				}
				break;
			case EAttackModiferPile.Discarded:
				m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.Add(itemChanged.AttackModifier);
				if (m_PlayerShowingFor.CharacterClass.AttackModifierCards.Any((AttackModifierYMLData a) => a.Name == itemChanged.AttackModifier.Name))
				{
					m_PlayerShowingFor.CharacterClass.AttackModifierCards.Remove(m_PlayerShowingFor.CharacterClass.AttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemChanged.AttackModifier.Name));
				}
				break;
			}
			break;
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
			switch (itemChanged.CardPile)
			{
			case EAttackModiferPile.Available:
				MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Add(itemChanged.AttackModifier);
				MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Add(itemChanged.AttackModifier);
				if (MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Any((AttackModifierYMLData a) => a.Name == itemChanged.AttackModifier.Name))
				{
					MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Remove(MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemChanged.AttackModifier.Name));
				}
				break;
			case EAttackModiferPile.Discarded:
				MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Add(itemChanged.AttackModifier);
				if (MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Any((AttackModifierYMLData a) => a.Name == itemChanged.AttackModifier.Name))
				{
					MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Remove(MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemChanged.AttackModifier.Name));
				}
				break;
			}
			break;
		}
		UpdateStateWithChanges();
	}

	public void ModifierDeleted(LevelEditorAttackModiferItem itemDeleted)
	{
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
			switch (itemDeleted.CardPile)
			{
			case EAttackModiferPile.Available:
				m_PlayerShowingFor.CharacterClass.AttackModifierCards.Remove(m_PlayerShowingFor.CharacterClass.AttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemDeleted.AttackModifier.Name));
				break;
			case EAttackModiferPile.Discarded:
				m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.Remove(m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemDeleted.AttackModifier.Name));
				break;
			}
			break;
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
			switch (itemDeleted.CardPile)
			{
			case EAttackModiferPile.Available:
				MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Remove(MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemDeleted.AttackModifier.Name));
				break;
			case EAttackModiferPile.Discarded:
				MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Remove(MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.First((AttackModifierYMLData f) => f.Name == itemDeleted.AttackModifier.Name));
				break;
			}
			break;
		case ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier:
			ScenarioModifierAttackModifierCardNames.Remove(itemDeleted.AttackModifier.Name);
			break;
		}
		GameObject obj = itemDeleted.gameObject;
		m_ModifierItems.Remove(itemDeleted);
		Object.Destroy(obj);
		RefreshAddDropDown();
		UpdateStateWithChanges();
	}

	public void ModifierReordered(LevelEditorAttackModiferItem itemReordered, bool movedUp)
	{
		List<AttackModifierYMLData> list = null;
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
			switch (itemReordered.CardPile)
			{
			case EAttackModiferPile.Available:
				list = m_PlayerShowingFor.CharacterClass.AttackModifierCards;
				break;
			case EAttackModiferPile.Discarded:
				list = m_PlayerShowingFor.CharacterClass.DiscardedAttackModifierCards;
				break;
			}
			break;
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
			switch (itemReordered.CardPile)
			{
			case EAttackModiferPile.Available:
				list = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards;
				break;
			case EAttackModiferPile.Discarded:
				list = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards;
				break;
			}
			break;
		}
		if (list != null)
		{
			int cardIndex = itemReordered.CardIndex;
			int num = (movedUp ? (cardIndex - 1) : (cardIndex + 1));
			num = ((num > 0) ? ((num > list.Count - 1) ? (list.Count - 1) : num) : 0);
			list.RemoveAt(cardIndex);
			list.Insert(num, itemReordered.AttackModifier);
		}
		FillList();
	}

	public void AddModifierPressed()
	{
		if (ModifierAddDropdown.value > 0)
		{
			AttackModifierYMLData attackModifierYMLData = null;
			string text = ModifierAddDropdown.options[ModifierAddDropdown.value].text;
			if (text == "Bless")
			{
				attackModifierYMLData = AttackModifiersYML.CreateBless();
			}
			else
			{
				attackModifierYMLData = ((!(text == "Curse")) ? ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == ModifierAddDropdown.options[ModifierAddDropdown.value].text) : AttackModifiersYML.CreateCurse());
			}
			switch (m_ShowingForDisplayType)
			{
			case ELevelEditorAttackModifierPanelDisplayType.Player:
				m_PlayerShowingFor.CharacterClass.AttackModifierCards.Add(attackModifierYMLData);
				AddItemForAttackModifer(attackModifierYMLData, EAttackModiferPile.Available, m_PlayerShowingFor.CharacterClass.AttackModifierCards.Count - 1);
				break;
			case ELevelEditorAttackModifierPanelDisplayType.Enemy:
				MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Add(attackModifierYMLData);
				AddItemForAttackModifer(attackModifierYMLData, EAttackModiferPile.Available, MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count - 1);
				break;
			case ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier:
				ScenarioModifierAttackModifierCardNames.Add(attackModifierYMLData.Name);
				AddItemForAttackModifer(attackModifierYMLData, EAttackModiferPile.Available, ScenarioModifierAttackModifierCardNames.Count - 1);
				break;
			}
			RefreshAddDropDown();
			UpdateStateWithChanges();
		}
	}

	public void UpdateStateWithChanges()
	{
		switch (m_ShowingForDisplayType)
		{
		case ELevelEditorAttackModifierPanelDisplayType.Player:
			ScenarioManager.CurrentScenarioState.Players.Single((PlayerState x) => x.ActorGuid == m_PlayerShowingFor.ActorGuid).AttackModifierDeck.SaveCharacter(m_PlayerShowingFor.CharacterClass);
			break;
		case ELevelEditorAttackModifierPanelDisplayType.Enemy:
			ScenarioManager.CurrentScenarioState.EnemyClassManager.EnemyAttackModifierDeck.SaveMonster(MonsterClassManager.EnemyMonsterAttackModifierDeck);
			ScenarioManager.CurrentScenarioState.EnemyClassManager.AlliedMonsterAttackModifierDeck.SaveMonster(MonsterClassManager.AlliedMonsterAttackModifierDeck);
			ScenarioManager.CurrentScenarioState.EnemyClassManager.Enemy2MonsterAttackModifierDeck.SaveMonster(MonsterClassManager.Enemy2MonsterAttackModifierDeck);
			ScenarioManager.CurrentScenarioState.EnemyClassManager.NeutralMonsterAttackModifierDeck.SaveMonster(MonsterClassManager.NeutralMonsterAttackModifierDeck);
			break;
		case ELevelEditorAttackModifierPanelDisplayType.ScenarioModifier:
			break;
		}
	}
}

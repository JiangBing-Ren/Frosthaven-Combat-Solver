using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelEditorUnitsPanel : MonoBehaviour
{
	public enum UnitType
	{
		None,
		Enemy,
		Player,
		Prop,
		Spawner,
		Objects
	}

	public UnitType TypeOfUnit;

	public GameObject AddUnitPanel;

	public LayoutGroup UnitListPanel;

	public GameObject UnitListItemPrefab;

	public LevelEditorUnitDisplay UnitDisplayPanel;

	[HideInInspector]
	public List<LevelEditorUnitListItem> ListItems = new List<LevelEditorUnitListItem>();

	private void OnEnable()
	{
		ClearList();
		FillList();
		UnitDisplayPanel.UpdateUIForDisplay();
		if (ListItems.Count > 0)
		{
			UnitDisplayPanel.SetListItemToDisplay(ListItems[0]);
		}
	}

	public void TryPlaceUnitInHex(string unitID, CAreaEffect areaEffect = null)
	{
		UnitType typeOfUnit = TypeOfUnit;
		if ((uint)(typeOfUnit - 1) <= 4u)
		{
			LevelEditorController.BeginPlacingUnit(unitID, TypeOfUnit, areaEffect);
		}
		else
		{
			Log.Error($"TypeOfUnit:{TypeOfUnit} not handled yet. Please, implement it");
		}
	}

	public void SuccessfullyPlacedUnitInHex(string unitName, CClientTile tilePlacedIn)
	{
		AddUnitPanel.gameObject.SetActive(value: false);
		ListItemPressed(NewUnit(unitName, unitName, tilePlacedIn));
	}

	public void FailedToPlaceUnitInHex()
	{
		AddUnitPanel.gameObject.SetActive(value: false);
	}

	public void AddItemPressed()
	{
		AddUnitPanel.gameObject.SetActive(!AddUnitPanel.gameObject.activeSelf);
	}

	public void ListItemPressed(LevelEditorUnitListItem itemPressed)
	{
		if (UnitDisplayPanel != null)
		{
			UnitDisplayPanel.SetListItemToDisplay(itemPressed);
			UnitDisplayPanel.gameObject.SetActive(value: true);
		}
	}

	public void ListItemMovePressed(LevelEditorUnitListItem itemToRemove)
	{
		switch (itemToRemove.type)
		{
		case UnitType.Player:
			LevelEditorController.BeginMovingUnit(itemToRemove.playerObj.ActorGuid, itemToRemove.type);
			break;
		case UnitType.Enemy:
			LevelEditorController.BeginMovingUnit(itemToRemove.enemyObj.ActorGuid, itemToRemove.type);
			break;
		case UnitType.Objects:
			LevelEditorController.BeginMovingUnit(itemToRemove.objectObj.ActorGuid, itemToRemove.type);
			break;
		case UnitType.Prop:
			LevelEditorController.BeginMovingUnit(itemToRemove.propObj.PropGuid, itemToRemove.type);
			break;
		case UnitType.Spawner:
			LevelEditorController.BeginMovingUnit(itemToRemove.spawnerObj.SpawnerGuid, itemToRemove.type);
			break;
		}
	}

	public void ListItemLinkPressed(LevelEditorUnitListItem itemToRemove)
	{
		LevelEditorController.BeginLinkProp(itemToRemove.propObj.PropGuid, itemToRemove.type);
	}

	public void ListItemUnlinkPressed(LevelEditorUnitListItem itemToRemove)
	{
		LevelEditorController.BeginUnlinkProp(itemToRemove.propObj.PropGuid, itemToRemove.type);
	}

	public void ListItemRemovePressed(LevelEditorUnitListItem itemToRemove)
	{
		switch (itemToRemove.type)
		{
		case UnitType.Player:
			if (LevelEditorController.RemovePlayer(itemToRemove.playerObj))
			{
				int index2 = ListItems.IndexOf(itemToRemove);
				UnityEngine.Object.Destroy(itemToRemove.gameObject);
				ListItems.RemoveAt(index2);
				UnitDisplayPanel.gameObject.SetActive(value: false);
				LevelEditorController.s_Instance.m_LevelEditorUIInstance.FinishDeletingUnit();
			}
			break;
		case UnitType.Enemy:
			if (LevelEditorController.RemoveMonster(itemToRemove.enemyObj))
			{
				int index4 = ListItems.IndexOf(itemToRemove);
				UnityEngine.Object.Destroy(itemToRemove.gameObject);
				ListItems.RemoveAt(index4);
				UnitDisplayPanel.gameObject.SetActive(value: false);
				LevelEditorController.s_Instance.m_LevelEditorUIInstance.FinishDeletingUnit();
			}
			break;
		case UnitType.Objects:
			if (LevelEditorController.RemoveObject(itemToRemove.objectObj))
			{
				int index5 = ListItems.IndexOf(itemToRemove);
				UnityEngine.Object.Destroy(itemToRemove.gameObject);
				ListItems.RemoveAt(index5);
				UnitDisplayPanel.gameObject.SetActive(value: false);
				LevelEditorController.s_Instance.m_LevelEditorUIInstance.FinishDeletingUnit();
			}
			break;
		case UnitType.Prop:
			if (LevelEditorController.RemoveProp(itemToRemove.propObj))
			{
				int index3 = ListItems.IndexOf(itemToRemove);
				UnityEngine.Object.Destroy(itemToRemove.gameObject);
				ListItems.RemoveAt(index3);
				UnitDisplayPanel.gameObject.SetActive(value: false);
				LevelEditorController.s_Instance.m_LevelEditorUIInstance.FinishDeletingUnit();
			}
			break;
		case UnitType.Spawner:
			if (LevelEditorController.RemoveSpawner(itemToRemove.spawnerObj))
			{
				int index = ListItems.IndexOf(itemToRemove);
				UnityEngine.Object.Destroy(itemToRemove.gameObject);
				ListItems.RemoveAt(index);
				UnitDisplayPanel.gameObject.SetActive(value: false);
				LevelEditorController.s_Instance.m_LevelEditorUIInstance.FinishDeletingUnit();
			}
			break;
		}
	}

	public LevelEditorUnitListItem NewUnit(string unitName, string locKey, CClientTile tilePlacedIn)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(UnitListItemPrefab, UnitListPanel.transform);
		LevelEditorUnitListItem CreatedListItem = gameObject.GetComponent<LevelEditorUnitListItem>();
		CreatedListItem.button.onClick.AddListener(() =>
		{
			ListItemPressed(CreatedListItem);
		});
		CreatedListItem.Initialise(TypeOfUnit, unitName, locKey, tilePlacedIn);
		ListItems.Add(CreatedListItem);
		return CreatedListItem;
	}

	public void ClearList()
	{
		foreach (LevelEditorUnitListItem listItem in ListItems)
		{
			UnityEngine.Object.Destroy(listItem.gameObject);
		}
		ListItems.Clear();
	}

	public void FillList()
	{
		switch (TypeOfUnit)
		{
		case UnitType.Enemy:
			foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
			{
				LoadEnemy(enemy);
			}
			foreach (CEnemyActor allyMonster in ScenarioManager.Scenario.AllyMonsters)
			{
				LoadEnemy(allyMonster);
			}
			foreach (CEnemyActor enemy2Monster in ScenarioManager.Scenario.Enemy2Monsters)
			{
				LoadEnemy(enemy2Monster);
			}
			foreach (CEnemyActor neutralMonster in ScenarioManager.Scenario.NeutralMonsters)
			{
				LoadEnemy(neutralMonster);
			}
			break;
		case UnitType.Objects:
			foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
			{
				LoadObject(@object);
			}
			break;
		case UnitType.Player:
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				LoadPlayer(playerActor);
			}
			break;
		case UnitType.Prop:
			foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
			{
				LoadProp(prop);
			}
			foreach (CObjectProp activatedProp in ScenarioManager.CurrentScenarioState.ActivatedProps)
			{
				LoadProp(activatedProp);
			}
			break;
		case UnitType.Spawner:
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				LoadSpawner(spawner);
			}
			break;
		}
		ListItems.Sort((LevelEditorUnitListItem x, LevelEditorUnitListItem y) => x.text.text.CompareTo(y.text.text));
		foreach (LevelEditorUnitListItem listItem in ListItems)
		{
			listItem.transform.SetAsLastSibling();
		}
	}

	public void LoadPlayer(CPlayerActor playerActor)
	{
		if (TypeOfUnit != UnitType.Player)
		{
			throw new Exception("LoadPlayer called for UnitPanel not set to handle Player units.");
		}
		NewUnit(playerActor.CharacterClass.ID, playerActor.ActorLocKey(), ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[playerActor.ArrayIndex.X, playerActor.ArrayIndex.Y]).SetValuesFromObject(playerActor);
	}

	public void LoadEnemy(CEnemyActor enemyActor)
	{
		if (TypeOfUnit != UnitType.Enemy)
		{
			throw new Exception("LoadEnemy called for UnitPanel not set to handle Enemy units.");
		}
		NewUnit(enemyActor.MonsterClass.ID, enemyActor.ActorLocKey(), ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[enemyActor.ArrayIndex.X, enemyActor.ArrayIndex.Y]).SetValuesFromObject(enemyActor);
	}

	public void LoadObject(CObjectActor objectActor)
	{
		if (TypeOfUnit != UnitType.Objects)
		{
			throw new Exception("LoadObject called for UnitPanel not set to handle Objects units.");
		}
		NewUnit(objectActor.MonsterClass.ID, objectActor.ActorLocKey(), ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[objectActor.ArrayIndex.X, objectActor.ArrayIndex.Y]).SetValuesFromObject(objectActor);
	}

	public void LoadProp(CObjectProp prop)
	{
		if (TypeOfUnit != UnitType.Prop)
		{
			throw new Exception("LoadProp called for UnitPanel not set to handle Prop units.");
		}
		NewUnit(prop.ObjectType.ToString(), prop.ObjectType.ToString(), ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[prop.ArrayIndex.X, prop.ArrayIndex.Y]).SetValuesFromObject(prop);
	}

	public void LoadSpawner(CSpawner spawner)
	{
		if (TypeOfUnit != UnitType.Spawner)
		{
			throw new Exception("LoadProp called for UnitPanel not set to handle Spawner unit.");
		}
		NewUnit(spawner.SpawnerGuid, spawner.SpawnerGuid, ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[spawner.ArrayIndex.X, spawner.ArrayIndex.Y]).SetValuesFromObject(spawner);
	}
}

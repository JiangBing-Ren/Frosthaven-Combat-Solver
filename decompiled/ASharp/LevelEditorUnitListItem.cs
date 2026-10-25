using System;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUnitListItem : MonoBehaviour
{
	public LevelEditorUnitsPanel.UnitType type;

	public TextMeshProUGUI text;

	public Button button;

	[HideInInspector]
	public CPlayerActor playerObj;

	[HideInInspector]
	public CEnemyActor enemyObj;

	[HideInInspector]
	public CObjectActor objectObj;

	[HideInInspector]
	public CObjectProp propObj;

	[HideInInspector]
	public CSpawner spawnerObj;

	private Color m_BackgroundOriginalColor;

	public void Initialise(LevelEditorUnitsPanel.UnitType typeToSet, string ID, string locKey, CClientTile tilePlacedIn)
	{
		type = typeToSet;
		switch (type)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			playerObj = ScenarioManager.Scenario.FindPlayerAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			text.text = LocalizationManager.GetTranslation(playerObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
			enemyObj = ScenarioManager.Scenario.FindEnemyAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindAllyMonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindNeutralMonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindEnemy2MonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			text.text = LocalizationManager.GetTranslation(enemyObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			objectObj = ScenarioManager.Scenario.FindObjectActorAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			text.text = LocalizationManager.GetTranslation(objectObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
		{
			ScenarioManager.ObjectImportType result = GlobalSettings.GetObjectImportType(ID);
			if (result == ScenarioManager.ObjectImportType.None && !Enum.TryParse<ScenarioManager.ObjectImportType>(ID, out result))
			{
				result = ScenarioManager.ObjectImportType.None;
			}
			propObj = tilePlacedIn.m_Tile.FindProp(result);
			text.text = locKey;
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Spawner:
			spawnerObj = tilePlacedIn.m_Tile.FindSpawner(ID);
			text.text = locKey;
			break;
		}
	}

	public void SetValuesFromObject(CPlayerActor playerActor)
	{
		text.text = LocalizationManager.GetTranslation(playerActor.ActorLocKey());
		playerObj = playerActor;
	}

	public void SetValuesFromObject(CEnemyActor enemyActor)
	{
		text.text = LocalizationManager.GetTranslation(enemyActor.ActorLocKey());
		enemyObj = enemyActor;
	}

	public void SetValuesFromObject(CObjectActor objectActor)
	{
		text.text = LocalizationManager.GetTranslation(objectActor.ActorLocKey());
		objectObj = objectActor;
	}

	public void SetValuesFromObject(CObjectProp prop)
	{
		propObj = prop;
		if (prop.SpecificPropType == ESpecificPropType.None)
		{
			text.text = ((prop.PropType == EPropType.None) ? prop.ObjectType.ToString() : prop.PropType.ToString());
			if (prop.PropType == EPropType.OneHexObstacle)
			{
				text.text = "Obstacle";
			}
		}
		else
		{
			text.text = prop.SpecificPropType.ToString();
		}
		if (!string.IsNullOrWhiteSpace(prop.PropHealthDetails?.ActorClass))
		{
			text.text = prop.PropHealthDetails.ActorClass;
		}
	}

	public void SetValuesFromObject(CSpawner prop)
	{
		spawnerObj = prop;
		text.text = spawnerObj.SpawnerGuid;
	}

	public void SetSelectedState(bool selected)
	{
		button.targetGraphic.color = (selected ? new Color(0.85f, 0.745f, 0.541f) : m_BackgroundOriginalColor);
	}

	private void Awake()
	{
		m_BackgroundOriginalColor = button.targetGraphic.color;
	}
}

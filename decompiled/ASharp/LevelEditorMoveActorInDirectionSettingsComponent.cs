using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMoveActorInDirectionSettingsComponent : MonoBehaviour
{
	[Header("Directions")]
	public GameObject InlineRemovalListItemPrefab;

	public LevelEditorGenericListPanel DirectionsPanel;

	public List<ScenarioManager.EAdjacentPosition> Directions = new List<ScenarioManager.EAdjacentPosition>();

	[Header("Settings")]
	public Toggle ShouldTakeDamageIfMovementBlockedToggle;

	public TMP_InputField DamageToTakeInputField;

	public Toggle PreventMovementIfBehindObstacleToggle;

	private List<LevelEditorListItemInlineButtons> m_DirectionsIndexItems = new List<LevelEditorListItemInlineButtons>();

	public void SetScenarioModifier(CScenarioModifier modifier)
	{
		Directions.Clear();
		ShouldTakeDamageIfMovementBlockedToggle.SetIsOnWithoutNotify(value: false);
		PreventMovementIfBehindObstacleToggle.SetIsOnWithoutNotify(value: false);
		DamageToTakeInputField.gameObject.SetActive(value: false);
		if (modifier != null && modifier is CScenarioModifierMoveActorsInDirections cScenarioModifierMoveActorsInDirections)
		{
			foreach (ScenarioManager.EAdjacentPosition moveDirection in cScenarioModifierMoveActorsInDirections.MoveDirections)
			{
				Directions.Add(moveDirection);
			}
			ShouldTakeDamageIfMovementBlockedToggle.interactable = true;
			ShouldTakeDamageIfMovementBlockedToggle.SetIsOnWithoutNotify(cScenarioModifierMoveActorsInDirections.ShouldTakeDamageIfMovementBlocked);
			if (cScenarioModifierMoveActorsInDirections.ShouldTakeDamageIfMovementBlocked)
			{
				DamageToTakeInputField.gameObject.SetActive(value: true);
				DamageToTakeInputField.SetValue(cScenarioModifierMoveActorsInDirections.DamageToTake.ToString());
			}
			PreventMovementIfBehindObstacleToggle.interactable = true;
			PreventMovementIfBehindObstacleToggle.SetIsOnWithoutNotify(cScenarioModifierMoveActorsInDirections.PreventMovementIfBehindObstacle);
		}
		List<string> itemsThatCanBeAdded = ScenarioManager.AdjacentPositionTypes.Select((ScenarioManager.EAdjacentPosition x) => x.ToString()).ToList();
		DirectionsPanel.SetupItemsAvailableToAdd(itemsThatCanBeAdded);
		DirectionsPanel.SetupDelegateActions(OnDirectionAddPressed, OnDeleteDirectionIndexKey);
		ShouldTakeDamageIfMovementBlockedToggle.onValueChanged.AddListener(OnShouldTakeDamageValueChanged);
	}

	public void UpdateDirectionsPanel()
	{
		if (!DirectionsPanel.gameObject.activeInHierarchy)
		{
			return;
		}
		if (m_DirectionsIndexItems == null)
		{
			m_DirectionsIndexItems = new List<LevelEditorListItemInlineButtons>();
		}
		foreach (LevelEditorListItemInlineButtons directionsIndexItem in m_DirectionsIndexItems)
		{
			Object.Destroy(directionsIndexItem.gameObject);
		}
		m_DirectionsIndexItems.Clear();
		if (Directions != null)
		{
			for (int i = 0; i < Directions.Count; i++)
			{
				LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, DirectionsPanel.ListItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
				m_DirectionsIndexItems.Add(component);
				component.SetupListItem("Direction: " + Directions[i], i);
			}
		}
	}

	private void OnDirectionAddPressed(string s = null)
	{
		if (s != null)
		{
			ScenarioManager.EAdjacentPosition item = ScenarioManager.AdjacentPositionTypes.SingleOrDefault((ScenarioManager.EAdjacentPosition x) => x.ToString() == s);
			Directions.Add(item);
			UpdateDirectionsPanel();
		}
	}

	private void OnDeleteDirectionIndexKey(string itemText, int itemIndex)
	{
		Directions.RemoveAt(itemIndex);
		UpdateDirectionsPanel();
	}

	private void OnShouldTakeDamageValueChanged(bool value)
	{
		DamageToTakeInputField.gameObject.SetActive(value);
	}
}

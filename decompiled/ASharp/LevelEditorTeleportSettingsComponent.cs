using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorTeleportSettingsComponent : MonoBehaviour
{
	[Header("Settings")]
	public Toggle TeleportTakeTilePriority;

	public Toggle OpenDoorsToTeleportedLocation;

	[Header("Tiles")]
	public GameObject InlineRemovalListItemPrefab;

	public LevelEditorGenericListPanel TilesPanel;

	private List<LevelEditorListItemInlineButtons> m_TileIndexItems = new List<LevelEditorListItemInlineButtons>();

	public List<TileIndex> TileIndexes = new List<TileIndex>();

	public void SetScenarioModifier(CScenarioModifier modifier)
	{
		TileIndexes.Clear();
		TeleportTakeTilePriority.SetIsOnWithoutNotify(value: false);
		OpenDoorsToTeleportedLocation.SetIsOnWithoutNotify(value: false);
		if (modifier == null)
		{
			return;
		}
		if (modifier is CScenarioModifierPhaseInAndTeleport cScenarioModifierPhaseInAndTeleport)
		{
			foreach (TileIndex teleportTileIndex in cScenarioModifierPhaseInAndTeleport.TeleportTileIndexes)
			{
				TileIndexes.Add(teleportTileIndex);
			}
			TeleportTakeTilePriority.interactable = false;
			OpenDoorsToTeleportedLocation.interactable = false;
		}
		else
		{
			if (!(modifier is CScenarioModifierTeleport cScenarioModifierTeleport))
			{
				return;
			}
			foreach (TileIndex teleportTileIndex2 in cScenarioModifierTeleport.TeleportTileIndexes)
			{
				TileIndexes.Add(teleportTileIndex2);
			}
			TeleportTakeTilePriority.interactable = true;
			TeleportTakeTilePriority.SetIsOnWithoutNotify(cScenarioModifierTeleport.MovesOtherThingsOffTile);
			OpenDoorsToTeleportedLocation.interactable = true;
			OpenDoorsToTeleportedLocation.SetIsOnWithoutNotify(cScenarioModifierTeleport.OpenDoorsToTeleportedLocation);
		}
	}

	public void UpdateTileIndexPanel()
	{
		if (!TilesPanel.gameObject.activeInHierarchy)
		{
			return;
		}
		if (m_TileIndexItems == null)
		{
			m_TileIndexItems = new List<LevelEditorListItemInlineButtons>();
		}
		foreach (LevelEditorListItemInlineButtons tileIndexItem in m_TileIndexItems)
		{
			Object.Destroy(tileIndexItem.gameObject);
		}
		m_TileIndexItems.Clear();
		if (TileIndexes != null)
		{
			for (int i = 0; i < TileIndexes.Count; i++)
			{
				LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, TilesPanel.ListItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
				m_TileIndexItems.Add(component);
				TileIndex tileIndex = TileIndexes[i];
				component.SetupListItem("Tile: " + tileIndex.X + "," + tileIndex.Y, i, OnDeleteTileIndexKey, TileItemSelected);
			}
		}
	}

	public void OnButtonSelectTilePressed()
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	public void TileItemSelected(LevelEditorListItemInlineButtons tileItemSelected)
	{
		TileIndex tileIndex = TileIndexes[tileItemSelected.ItemIndex];
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
		LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		TileIndex newTileIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		if (!TileIndexes.Any((TileIndex x) => TileIndex.Compare(x, newTileIndex)))
		{
			TileIndexes.Add(new TileIndex(tileSelected.m_Tile.m_ArrayIndex));
		}
		UpdateTileIndexPanel();
	}

	private void OnDeleteTileIndexKey(LevelEditorListItemInlineButtons item)
	{
		TileIndexes.RemoveAt(item.ItemIndex);
		UpdateTileIndexPanel();
	}
}

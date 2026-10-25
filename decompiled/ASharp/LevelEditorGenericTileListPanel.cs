using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

public class LevelEditorGenericTileListPanel : MonoBehaviour
{
	[Header("Tiles")]
	public LevelEditorGenericListPanel TilesPanel;

	[HideInInspector]
	public List<TileIndex> TileIndexes = new List<TileIndex>();

	public void SetTileList(List<TileIndex> tileList)
	{
		TileIndexes = tileList?.ToList() ?? new List<TileIndex>();
		List<string> items = TileIndexes?.Select((TileIndex t) => $"Tile: {t.X},{t.Y}").ToList();
		TilesPanel.RefreshUIWithItems(items);
		TilesPanel.SetupDelegateActions(OnAddTilePressed, OnDeleteTileIndexKey, OnTileItemPressed);
	}

	public void OnAddTilePressed(string s = null)
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	private void OnDeleteTileIndexKey(string itemText, int itemIndex)
	{
		_ = TileIndexes[itemIndex];
		TileIndexes.RemoveAt(itemIndex);
		SetTileList(TileIndexes);
	}

	public void OnTileItemPressed(string itemText, int itemIndex)
	{
		TileIndex tileIndex = TileIndexes[itemIndex];
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
		LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		TileIndex item = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		if (!TileIndexes.Contains(item))
		{
			TileIndexes.Add(item);
			SetTileList(TileIndexes);
		}
	}
}

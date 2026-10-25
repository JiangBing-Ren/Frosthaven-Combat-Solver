using System.Collections.Generic;
using UnityEngine;

public interface IHexControl
{
	public enum HexType
	{
		NegativeEffect,
		PositiveEffect,
		Move,
		InvalidPlacement,
		ExitDungeonRevealed,
		ExitDungeonUnrevealed,
		ActiveBonusCondition,
		DifficultTerrain,
		IcyTerrain,
		Objective,
		ElevatedTerrain
	}

	public enum HexMode
	{
		Reach,
		PossibleTarget,
		Selected,
		SelectedAOEWithoutTarget,
		Cursor,
		ReachAllHexes
	}

	GameObject GameObject { get; }

	HexType Type { get; set; }

	HexMode Mode { get; set; }

	bool Hover { get; set; }

	Color? ColorOverride { get; set; }

	void RefreshHexUI(bool forceUpdate = true);

	void HexUpdate();

	void SetNeighbors(CClientTile currentTile, HashSet<CClientTile> activeTiles, List<CClientTile> hoveredTiles = null);

	void SetAllNeighborsOn();
}

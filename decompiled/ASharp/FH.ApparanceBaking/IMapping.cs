using System.Collections.Generic;
using UnityEngine;

namespace FH.ApparanceBaking;

public interface IMapping
{
	GameObject EnvironmentRoot { get; }

	GameObject GetProp(string guid);

	GameObject GetTile(string guid);

	GameObject GetTileHex(string guid);

	GameObject GetMap(string guid);

	IEnumerable<GameObject> GetMaps();

	IEnumerable<GameObject> GetProps();

	IEnumerable<GameObject> GetTiles();

	void AddMap(GameObject map, string guid);

	void AddProp(GameObject prop, string guid);

	void AddTile(EnvironmentLoader.Mapping.TileData tile, string guid);

	void RemoveTile(string guid);
}

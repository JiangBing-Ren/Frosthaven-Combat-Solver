using UnityEngine;

namespace FH.LevelComponents.ProcGen;

internal interface IRoomVisibility
{
	GameObject gameObject { get; }

	ProceduralMapTile.Visibility GetVisibility();
}

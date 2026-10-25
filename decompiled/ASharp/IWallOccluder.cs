using FH.LevelComponents.ProcGen;
using UnityEngine;

public interface IWallOccluder : IOccluder<WallOcclusionCheckResult>
{
	Vector2 Forward { get; }

	Vector2 LeftCorner { get; }

	Vector2 RightCorner { get; }
}

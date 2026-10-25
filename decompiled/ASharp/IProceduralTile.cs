using UnityEngine;

public interface IProceduralTile
{
	bool IsActive { get; }

	Vector3 Position { get; }

	float Orientation { get; }

	int Flags { get; }

	float ContentRadius { get; }

	float ContentHeight { get; }
}

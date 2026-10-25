using FH.LevelComponents.ProcGen;
using UnityEngine;

public interface ICharacterOccluder : IOccluder<bool>
{
	Vector2 Front { get; }

	Vector2 Back { get; }

	Vector2 Left { get; }

	Vector2 Right { get; }
}

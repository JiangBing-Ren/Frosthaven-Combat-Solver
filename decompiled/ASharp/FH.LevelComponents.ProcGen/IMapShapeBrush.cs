using System.Collections.Generic;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public interface IMapShapeBrush
{
	List<Vector2> GetShape();
}

using System.Collections.Generic;
using UnityEngine;

namespace FH.LevelComponents.TopDownProjection;

public interface ICelContent
{
	IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents();
}

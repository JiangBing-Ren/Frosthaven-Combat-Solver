using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.TopDownProjection;
using UnityEngine;

namespace FH.LevelComponents.WorldLayerBlending;

internal class KronosWorldLevelCelContent : ICelContent
{
	public List<GameObject> Objects;

	public KronosWorldLevelCelContent(List<GameObject> objects)
	{
		Objects = objects;
	}

	public IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents()
	{
		List<(Renderer, int)> list = new List<(Renderer, int)>();
		foreach (MeshRenderer item in Objects.SelectMany((GameObject a) => a.GetComponentsInChildren<MeshRenderer>()))
		{
			list.Add((item.GetComponent<Renderer>(), item.GetComponent<MeshFilter>().sharedMesh.subMeshCount));
		}
		return list;
	}
}

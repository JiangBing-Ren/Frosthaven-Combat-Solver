using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using UnityEngine;

namespace FH.PostProcessing;

internal class WaterCelContent : ICelContent
{
	public IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents()
	{
		IEnumerable<MeshRenderer> enumerable = (from x in EnvironmentLoader.Instance.GetMaps()
			select x.GetComponent<ProceduralMapTile>()).ToArray().SelectMany((ProceduralMapTile map) => map.GetComponentsInChildren<MeshRenderer>());
		List<(Renderer, int)> list = new List<(Renderer, int)>();
		foreach (MeshRenderer item in enumerable)
		{
			Mesh sharedMesh = item.GetComponent<MeshFilter>().sharedMesh;
			if (sharedMesh.hideFlags != HideFlags.DontSave)
			{
				list.Add((item, sharedMesh.subMeshCount));
			}
		}
		return list;
	}
}

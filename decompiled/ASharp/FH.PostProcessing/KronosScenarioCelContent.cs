using System.Collections.Generic;
using System.Linq;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using UnityEngine;

namespace FH.PostProcessing;

internal class KronosScenarioCelContent : ICelContent
{
	private readonly IEnumerable<ProceduralMapTile> maps;

	private readonly ProceduralMapTile.Visibility? visibility;

	private readonly string[] hexMatching = new string[2] { "_hex", "fshape" };

	public KronosScenarioCelContent(IEnumerable<ProceduralMapTile> maps, ProceduralMapTile.Visibility? visibility)
	{
		this.maps = maps;
		this.visibility = visibility;
	}

	public IEnumerable<(Renderer renderer, int submeshCount)> GetMultiplaneCelContents()
	{
		if (!visibility.HasValue)
		{
			return from a in maps.SelectMany((ProceduralMapTile a) => a.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: false))
				where a.name.Contains("_ProceduralMeshSlice") || (hexMatching.Any((string b) => a.name.ToLower().Contains(b)) && !a.transform.parent.parent.name.Contains("Wall"))
				select ((Renderer, int subMeshCount))(a, subMeshCount: a.GetComponent<MeshFilter>().sharedMesh.subMeshCount);
		}
		return from a in (from a in maps.SelectMany((ProceduralMapTile a) => a.GetComponentsInChildren<IRoomVisibility>())
				where a.GetVisibility() == visibility.Value
				select a).SelectMany((IRoomVisibility a) => a.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: false))
			where a.GetComponentInParent<IRoomVisibility>() == null || a.GetComponentInParent<IRoomVisibility>().GetVisibility() == visibility.Value
			select ((Renderer, int subMeshCount))(a, subMeshCount: a.GetComponent<MeshFilter>().sharedMesh.subMeshCount);
	}
}

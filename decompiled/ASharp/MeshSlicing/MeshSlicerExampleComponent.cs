using System.Linq;
using SETUtil.Profiling;
using UnityEngine;

namespace MeshSlicing;

[ExecuteAlways]
public class MeshSlicerExampleComponent : MonoBehaviour
{
	public SliceFillerGenerationSettingsDef SliceFillerSettingsDef;

	public Material Material;

	private VerticalMeshSlicer slicer;

	public bool NoCache;

	public bool Cut;

	[ContextMenu("Generate")]
	private void Generate()
	{
		using (new StopwatchProfiler())
		{
			if (NoCache)
			{
				slicer?.Dispose();
				slicer = null;
			}
			if (slicer == null)
			{
				slicer = new VerticalMeshSlicer(SliceFillerSettingsDef.CuttingPlaneHeight);
			}
			slicer.Push((from a in Object.FindObjectsOfType<MeshRenderer>()
				select (a: a, a.GetComponent<MeshFilter>())).ToArray(), Material, NoCache);
			if (Cut)
			{
				Shader.SetGlobalFloat("_WallFadeBottomThreshold", SliceFillerSettingsDef.CuttingPlaneHeight);
				Shader.SetGlobalInt("_WallFadeMode", 2);
			}
			else
			{
				Shader.SetGlobalInt("_WallFadeMode", 0);
			}
		}
	}

	private void Update()
	{
		slicer?.Tick();
	}

	private void OnDestroy()
	{
		slicer?.Dispose();
		Shader.SetGlobalInt("_WallFadeMode", 0);
	}
}

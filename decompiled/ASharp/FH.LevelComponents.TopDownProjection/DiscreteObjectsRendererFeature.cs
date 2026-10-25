using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FH.LevelComponents.TopDownProjection;

[ExecuteAlways]
public class DiscreteObjectsRendererFeature : ScriptableRendererFeature
{
	public DiscreteObjectsRenderPass CustomPass;

	public override void Create()
	{
		CustomPass = new DiscreteObjectsRenderPass();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(CustomPass);
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FH.LevelComponents.TopDownProjection;

public class DiscreteObjectsRenderPass : ScriptableRenderPass
{
	public static List<(Renderer renderer, int submeshCount)> ObjectsToRender;

	public static Material ReplacementMaterial;

	public DiscreteObjectsRenderPass()
	{
		renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (ObjectsToRender == null)
		{
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get("DiscreteObjectsRenderPass");
		foreach (var item in ObjectsToRender)
		{
			for (int i = 0; i < item.submeshCount; i++)
			{
				Material material = (ReplacementMaterial ? ReplacementMaterial : item.renderer.sharedMaterials[i]);
				for (int j = 0; j < material.passCount; j++)
				{
					if (material.GetShaderPassEnabled(material.GetPassName(j)))
					{
						commandBuffer.DrawRenderer(item.renderer, material, i, j);
					}
				}
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}

using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public static class RenderingLayerHelpler
{
	public static Renderer SetLayer(this Renderer renderer, RenderingLayer layer)
	{
		return RenderingLayer.Friend.SetLayer_Internal(renderer, layer);
	}

	public static Light SetLayer(this Light renderer, RenderingLayer layer)
	{
		return RenderingLayer.Friend.SetLayer_Internal(renderer, layer);
	}

	public static Light SetCullingMask(this Light light, RenderingLayer layer)
	{
		return RenderingLayer.Friend.SetCullingMask_Internal(light, layer);
	}
}

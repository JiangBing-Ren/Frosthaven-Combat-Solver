using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FH.LevelComponents.ProcGen;

public class RenderingLayer
{
	internal class Friend
	{
		public static Renderer SetLayer_Internal(Renderer renderer, RenderingLayer layer)
		{
			renderer.renderingLayerMask = (uint)layer._layer;
			return renderer;
		}

		public static Light SetLayer_Internal(Light light, RenderingLayer layer)
		{
			UniversalAdditionalLightData universalAdditionalLightData = light.GetComponent<UniversalAdditionalLightData>();
			if (!universalAdditionalLightData)
			{
				universalAdditionalLightData = light.gameObject.AddComponent<UniversalAdditionalLightData>();
			}
			universalAdditionalLightData.renderingLayers = (uint)layer._layer;
			return light;
		}

		public static Light SetCullingMask_Internal(Light light, RenderingLayer layer)
		{
			light.cullingMask = layer._layer;
			return light;
		}
	}

	private readonly int _layer;

	public static RenderingLayer NOTHING = new RenderingLayer(0);

	private const uint U_EVERYTHING = uint.MaxValue;

	public static RenderingLayer EVERYTHING = new RenderingLayer(uint.MaxValue);

	public bool Equals(RenderingLayer b)
	{
		return _layer == b._layer;
	}

	public RenderingLayer(int layer)
	{
		_layer = layer;
	}

	public RenderingLayer(uint layer)
	{
		_layer = (int)layer;
	}

	public int GetFirstIndex()
	{
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((num & _layer) == num)
			{
				return i;
			}
		}
		return -1;
	}

	public RenderingLayer Merge(RenderingLayer layer)
	{
		return new RenderingLayer(_layer | layer._layer);
	}
}

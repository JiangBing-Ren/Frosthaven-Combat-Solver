using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CreateLightProbes : MonoBehaviour
{
	public void Generate()
	{
		if (LightmapSettings.lightProbes != null)
		{
			SphericalHarmonicsL2[] array = new SphericalHarmonicsL2[LightmapSettings.lightProbes.count];
			Vector4[] occlusionProbes = new Vector4[LightmapSettings.lightProbes.count];
			LightProbes.CalculateInterpolatedLightAndOcclusionProbes(LightmapSettings.lightProbes.positions, array, occlusionProbes);
			LightmapSettings.lightProbes.bakedProbes = array;
		}
	}
}

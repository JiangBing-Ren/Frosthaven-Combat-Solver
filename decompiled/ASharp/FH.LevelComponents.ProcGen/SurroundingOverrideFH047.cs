using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class SurroundingOverrideFH047 : MonoBehaviour, ISurroundingOverride
{
	private const float MapMistHeightMax0 = 3.6f;

	private const float MapMistDecay0 = 0.9f;

	public bool MapHidden0 { get; set; }

	public float GetDecay(float defaultDecay)
	{
		if (!MapHidden0)
		{
			return 0.9f;
		}
		return defaultDecay;
	}

	public float GetHeightMax(float defaultHeightMax)
	{
		if (!MapHidden0)
		{
			return 3.6f;
		}
		return defaultHeightMax;
	}

	public float GetHeightMin(float defaultHeightMin)
	{
		return defaultHeightMin;
	}

	public Material GetSolidMaterial(Material defaultSolidMaterial)
	{
		return defaultSolidMaterial;
	}

	public Material GetTransparentMaterial(Material defaultTransparentMaterial)
	{
		return defaultTransparentMaterial;
	}

	public void ModifyMeshData(ref ProceduralMistSurrounding.CMeshData data)
	{
	}
}

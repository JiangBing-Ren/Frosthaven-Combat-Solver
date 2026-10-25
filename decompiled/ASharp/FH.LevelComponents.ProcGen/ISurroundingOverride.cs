using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public interface ISurroundingOverride
{
	float GetDecay(float defaultDecay);

	float GetHeightMax(float defaultHeightMax);

	float GetHeightMin(float defaultHeightMin);

	Material GetSolidMaterial(Material defaultSolidMaterial);

	Material GetTransparentMaterial(Material defaultTransparentMaterial);

	void ModifyMeshData(ref ProceduralMistSurrounding.CMeshData data);
}

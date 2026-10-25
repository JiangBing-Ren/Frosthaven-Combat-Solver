using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Defs/Multiplane/VFX_LOS_SettingsDef")]
public class VFX_LOS_SettingsDef : ScriptableObject
{
	public Material MultiplaneRenderingMaterial;

	public List<Material> MultiplaneRenderingPostProcessing;

	public int MaskResolution = 256;

	public int MaskDepthBufferPrecision = 16;
}

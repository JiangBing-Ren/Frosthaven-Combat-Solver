using System.Collections.Generic;
using UnityEngine;

namespace FH.PostProcessing;

[CreateAssetMenu(menuName = "Defs/Multiplane/KronosFogOfWarSettingsDef")]
public class KronosFogOfWarSettingsDef : ScriptableObject
{
	public List<Material> FowMapPostProcessing;

	public Material OverrideMaterial;
}

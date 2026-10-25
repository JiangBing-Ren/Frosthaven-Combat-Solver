using UnityEngine;

namespace FH.LevelComponents.ProcGen;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
[ExecuteInEditMode]
public class ProceduralBackground : ProceduralProp, IContentUpdateMonitor
{
}

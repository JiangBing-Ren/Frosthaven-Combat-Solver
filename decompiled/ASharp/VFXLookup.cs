using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimFXTrigger))]
public class VFXLookup : MonoBehaviour
{
	public List<Renderer> CharacterRenderers;

	public SkinnedMeshRenderer CharacterBodyMesh;

	public List<Transform> TrailPoints;
}

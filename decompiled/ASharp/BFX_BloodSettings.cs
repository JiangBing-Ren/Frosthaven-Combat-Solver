using UnityEngine;

public class BFX_BloodSettings : MonoBehaviour
{
	public enum _DecalRenderinMode
	{
		Floor_XZ,
		AverageRayBetwenForwardAndFloor
	}

	public float AnimationSpeed = 1f;

	public float GroundHeight;

	[Range(0f, 1f)]
	public float LightIntensityMultiplier = 1f;

	public bool FreezeDecalDisappearance;

	public _DecalRenderinMode DecalRenderinMode;

	public bool ClampDecalSideSurface;
}

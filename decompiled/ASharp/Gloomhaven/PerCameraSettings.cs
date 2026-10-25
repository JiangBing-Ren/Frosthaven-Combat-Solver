using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gloomhaven;

[RequireComponent(typeof(Camera))]
public class PerCameraSettings : MonoBehaviour
{
	private void Start()
	{
		Init();
	}

	public void Init()
	{
		Init(GraphicsController.GetAntiAliasing());
	}

	public void Init(AntialiasingRender antiAliasing)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
		if (name.Contains("UI"))
		{
			universalAdditionalCameraData.antialiasing = ((antiAliasing == AntialiasingRender.SMAA) ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None);
		}
		else
		{
			universalAdditionalCameraData.antialiasing = (AntialiasingMode)antiAliasing;
		}
	}
}

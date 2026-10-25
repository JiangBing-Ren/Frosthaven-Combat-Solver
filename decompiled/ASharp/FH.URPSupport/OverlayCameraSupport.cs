using System.Collections;
using EPOOutline;
using SETUtil;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FH.URPSupport;

[RequireComponent(typeof(UniversalAdditionalCameraData))]
public class OverlayCameraSupport : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(AttachUICameraCrt());
		SimplifyOutliners();
	}

	private IEnumerator AttachUICameraCrt()
	{
		UniversalAdditionalCameraData urpCamera = GetComponent<UniversalAdditionalCameraData>();
		while (!UIManager.Instance)
		{
			yield return null;
		}
		UIManager.Instance.UICamera.GetComponent<UniversalAdditionalCameraData>().renderType = CameraRenderType.Overlay;
		urpCamera.cameraStack.Clear();
		urpCamera.cameraStack.Add(UIManager.Instance.UICamera);
	}

	private void SimplifyOutliners()
	{
		Outliner[] components = GetComponents<Outliner>();
		if (components.Length == 0)
		{
			return;
		}
		Outliner obj = components[0];
		obj.RenderingStrategy = OutlineRenderingStrategy.Default;
		obj.OutlineLayerMask = -1L;
		if (components.Length != 1)
		{
			for (int i = 1; i < components.Length; i++)
			{
				SceneUtil.SmartDestroy(components[i]);
			}
		}
	}
}

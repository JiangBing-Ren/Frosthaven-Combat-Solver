using System;
using UnityEngine;

namespace FH.LevelComponents.TopDownProjection;

public class Cel : IDisposable
{
	public CelParams CelParams;

	internal bool PendingRefresh;

	private RenderTexture _renderTexture;

	public const float VERTICAL_CAMERA_OFFSET = 10f;

	public bool MarkForRemoval { get; private set; }

	public RenderTexture RenderTextureTarget => _renderTexture;

	internal Cel(CelParams celParams)
	{
		CelParams = celParams;
		_renderTexture = RenderTexture.GetTemporary(CelParams.Resolution.x, CelParams.Resolution.y, CelParams.DepthBufferPrecision, CelParams.TextureFormat);
	}

	public void Dispose()
	{
		if (!MarkForRemoval)
		{
			MarkForRemoval = true;
			if ((bool)_renderTexture)
			{
				RenderTexture.ReleaseTemporary(_renderTexture);
				_renderTexture = null;
			}
		}
	}

	internal bool ShouldRefreshThisFrame()
	{
		return PendingRefresh;
	}

	internal void AttachToCamera(Camera camera)
	{
		camera.targetTexture = _renderTexture;
		float num = CelParams.LevelBounds.extents.y + 10f;
		camera.transform.position = new Vector3(CelParams.LevelBounds.center.x, num, CelParams.LevelBounds.center.z);
		camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
		camera.orthographicSize = CelParams.OrthographicAreaSize / 2f;
		camera.farClipPlane = num - CelParams.MinClipHeight;
		camera.cullingMask = CelParams.CullingMask;
		if ((bool)CelParams.CustomOverrideShader)
		{
			camera.SetReplacementShader(CelParams.CustomOverrideShader, string.Empty);
		}
		else
		{
			camera.ResetReplacementShader();
		}
	}

	public void RequestRefresh(ICelContent newContent = null)
	{
		PendingRefresh = true;
		if (newContent != null)
		{
			CelParams.Content = newContent;
		}
	}
}

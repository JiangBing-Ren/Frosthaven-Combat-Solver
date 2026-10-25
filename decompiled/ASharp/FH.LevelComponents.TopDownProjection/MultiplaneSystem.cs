using System.Collections.Generic;
using System.Linq;
using Libs.GameConsole;
using SETUtil;
using SETUtil.Functional.Extend;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FH.LevelComponents.TopDownProjection;

[ExecuteAlways]
public sealed class MultiplaneSystem : MonoBehaviour
{
	public class Friend
	{
		public static IEnumerable<Cel> GetCels(MultiplaneSystem mps)
		{
			return mps._cels;
		}
	}

	private Camera _camera;

	private UniversalAdditionalCameraData _URPCamera;

	private List<Cel> _cels = new List<Cel>();

	[SerializeField]
	private int _rendererIndex = 2;

	private void OnEnable()
	{
		Init();
	}

	public static MultiplaneSystem GetInstance()
	{
		return Object.FindObjectOfType<MultiplaneSystem>();
	}

	public void Init()
	{
		Camera[] componentsInChildren = transform.GetComponentsInChildren<Camera>();
		foreach (Camera camera in componentsInChildren)
		{
			if (camera != _camera)
			{
				SceneUtil.SmartDestroy(camera.gameObject);
			}
		}
		if (!_camera)
		{
			_camera = GetComponent<Camera>();
			_camera = new GameObject("_MultiplaneCamera").AddComponent<Camera>();
			_camera.gameObject.hideFlags = HideFlags.DontSave;
			_camera.depth = -5f;
			_camera.clearFlags = CameraClearFlags.Color;
			_camera.backgroundColor = Color.black;
			_camera.enabled = false;
			_camera.orthographic = true;
			_camera.cullingMask = 0;
			_URPCamera = _camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
			_URPCamera.renderShadows = false;
			_URPCamera.transform.SetParent(transform);
			_URPCamera.renderType = CameraRenderType.Base;
			_URPCamera.renderPostProcessing = false;
			_URPCamera.volumeLayerMask = 0;
			_URPCamera.SetRenderer(_rendererIndex);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_camera)
		{
			SceneUtil.SmartDestroy(_camera.gameObject);
			_camera = null;
			_URPCamera = null;
		}
		foreach (Cel cel in _cels)
		{
			cel.Dispose();
		}
		_cels.Clear();
	}

	private void Update()
	{
		for (int num = _cels.Count - 1; num >= 0; num--)
		{
			if (_cels[num].MarkForRemoval)
			{
				_cels.RemoveAt(num);
			}
		}
		if (!_camera)
		{
			return;
		}
		foreach (Cel cel in _cels)
		{
			if (cel.ShouldRefreshThisFrame())
			{
				RenderCel(cel);
			}
		}
	}

	public Cel NewCel(CelParams celParams)
	{
		Cel cel = new Cel(celParams);
		_cels.Add(cel);
		return cel;
	}

	private void RenderCel(Cel cel)
	{
		cel.PendingRefresh = false;
		cel.AttachToCamera(_camera);
		DiscreteObjectsRenderPass.ObjectsToRender = cel.CelParams.Content.GetMultiplaneCelContents().ToList();
		DiscreteObjectsRenderPass.ReplacementMaterial = cel.CelParams.ReplacementMaterial;
		_camera.Render();
		DiscreteObjectsRenderPass.ObjectsToRender.Clear();
		DiscreteObjectsRenderPass.ReplacementMaterial = null;
		if (cel.CelParams.PostProcessing == null)
		{
			return;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(cel.RenderTextureTarget.width, cel.RenderTextureTarget.height, cel.RenderTextureTarget.depth, cel.RenderTextureTarget.format);
		foreach (Material item in cel.CelParams.PostProcessing)
		{
			Graphics.Blit(cel.RenderTextureTarget, temporary, item);
			Graphics.Blit(temporary, cel.RenderTextureTarget);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	[ConsoleCommand(Command = "DebugRefreshMultiplane")]
	public static void DebugRenderCmd(IConsole console)
	{
		GetInstance().Then((MultiplaneSystem a) =>
		{
			a._cels.ForEach((Cel b) =>
			{
				b.RequestRefresh();
			});
		});
	}

	[ConsoleCommand(Command = "DebugPrintCelContents")]
	public static void DebugPrintContentsCmd(IConsole console)
	{
		GetInstance().Then((MultiplaneSystem a) =>
		{
			console.WriteLine($"Cels: {a._cels.Count}");
		}).Then((MultiplaneSystem a) =>
		{
			a._cels.ForEach((Cel b) =>
			{
				console.WriteLine(string.Join(",\n", from tuple in b.CelParams.Content.GetMultiplaneCelContents()
					select $"{tuple.renderer.name} : {tuple.renderer.transform.localScale}"));
			});
		});
	}
}

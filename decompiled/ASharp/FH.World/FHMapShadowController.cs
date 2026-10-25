using System;
using CorgiCachedShadows;
using FHGUI;
using Gloomhaven;
using SETUtil;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FH.World;

[RequireComponent(typeof(FHMapCameraController))]
public class FHMapShadowController : MonoBehaviour
{
	public bool AdjustShadowDistance = true;

	[SerializeField]
	private float _minShadowDistance;

	[SerializeField]
	private float _maxShadowDistance;

	[SerializeField]
	private CachedShadowsRenderFeature[] _cachedShadowsFeatures;

	[SerializeField]
	private CachedAdditionalShadowsRenderFeature[] _cachedAdditionalShadowsFeatures;

	private UniversalRenderPipelineAsset _pipeline;

	private FHMapCameraController _cameraController;

	private float _originalShadowDistance;

	private static Action OnShadowUpdateRequested;

	private void OnEnable()
	{
		if ((object)_cameraController == null)
		{
			_cameraController = GetComponent<FHMapCameraController>();
		}
		_pipeline = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
		_originalShadowDistance = _pipeline.shadowDistance;
		FHMapCameraController cameraController = _cameraController;
		cameraController.OnZoom = (Action<float>)Delegate.Combine(cameraController.OnZoom, new Action<float>(OnZoom));
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Remove(GraphicsController.OnQualityLevelChanged, new Action(OnProfileChanged));
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Combine(GraphicsController.OnQualityLevelChanged, new Action(OnProfileChanged));
		OnShadowUpdateRequested = (Action)Delegate.Combine(OnShadowUpdateRequested, new Action(TriggerShadowUpdate));
		TriggerShadowUpdate();
	}

	private void OnZoom(float zoomFactor)
	{
		if (AdjustShadowDistance)
		{
			_pipeline.shadowDistance = MathUtil.Map(zoomFactor, 0f, 1f, _minShadowDistance, _maxShadowDistance);
		}
	}

	private void OnProfileChanged()
	{
		float shadowDistance = _pipeline.shadowDistance;
		_pipeline.shadowDistance = _originalShadowDistance;
		_pipeline = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
		_originalShadowDistance = _pipeline.shadowDistance;
		_pipeline.shadowDistance = shadowDistance;
		TriggerShadowUpdate();
	}

	[ContextMenu("Trigger Shadow Update")]
	private void TriggerShadowUpdate()
	{
		CachedShadowsRenderFeature[] cachedShadowsFeatures = _cachedShadowsFeatures;
		for (int i = 0; i < cachedShadowsFeatures.Length; i++)
		{
			cachedShadowsFeatures[i].TriggerShadowsRenderForFrame();
		}
		CachedAdditionalShadowsRenderFeature[] cachedAdditionalShadowsFeatures = _cachedAdditionalShadowsFeatures;
		for (int i = 0; i < cachedAdditionalShadowsFeatures.Length; i++)
		{
			cachedAdditionalShadowsFeatures[i].TriggerShadowsRenderForFrame();
		}
	}

	public static void RequestShadowUpdate()
	{
		OnShadowUpdateRequested?.Invoke();
	}

	private void OnDisable()
	{
		_pipeline.shadowDistance = _originalShadowDistance;
		FHMapCameraController cameraController = _cameraController;
		cameraController.OnZoom = (Action<float>)Delegate.Remove(cameraController.OnZoom, new Action<float>(OnZoom));
		GraphicsController.OnQualityLevelChanged = (Action)Delegate.Remove(GraphicsController.OnQualityLevelChanged, new Action(OnProfileChanged));
		OnShadowUpdateRequested = (Action)Delegate.Remove(OnShadowUpdateRequested, new Action(TriggerShadowUpdate));
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Libs.GameConsole;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature("Screen Space Global Illumination")]
[Tooltip("The Screen Space Global Illumination uses the depth and color buffer of the screen to calculate diffuse light bounces.")]
[HelpURL("https://github.com/jiaozi158/UnitySSGIURP/blob/main")]
public class ScreenSpaceGlobalIlluminationURP : ScriptableRendererFeature
{
	public class PreRenderScreenSpaceGlobalIlluminationPass : ScriptableRenderPass
	{
		private const string m_ProfilerTag = "Prepare Screen Space Global Illumination";

		private readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Prepare Screen Space Global Illumination");

		public Material m_SSGIMaterial;

		private Matrix4x4 camVPMatrix;

		private Matrix4x4 prevCamVPMatrix;

		private const string _PrevViewProjMatrix = "_PrevViewProjMatrix";

		private const string _NonJitteredViewProjMatrix = "_NonJitteredViewProjMatrix";

		private const string motionColorHandleName = "m_Color";

		private const string motionDepthHandleName = "m_Depth";

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				commandBuffer.SetGlobalMatrix("_PrevViewProjMatrix", prevCamVPMatrix);
				commandBuffer.SetGlobalMatrix("_NonJitteredViewProjMatrix", camVPMatrix);
				prevCamVPMatrix = camVPMatrix;
				object value = motionVectorPassFieldInfo.GetValue(renderingData.cameraData.renderer);
				if (value != null)
				{
					FieldInfo field = value.GetType().GetField("m_Color", BindingFlags.Instance | BindingFlags.NonPublic);
					FieldInfo field2 = value.GetType().GetField("m_Depth", BindingFlags.Instance | BindingFlags.NonPublic);
					if (field != null && field2 != null && field.GetValue(value) is RTHandle rTHandle && field2.GetValue(value) is RTHandle rTHandle2)
					{
						commandBuffer.SetRenderTarget(rTHandle, rTHandle2);
						Blitter.BlitTexture(commandBuffer, rTHandle, m_ScaleBias, m_SSGIMaterial, 7);
					}
				}
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			CameraData cameraData = renderingData.cameraData;
			Camera camera = cameraData.camera;
			camVPMatrix = GL.GetGPUProjectionMatrix(camera.nonJitteredProjectionMatrix, renderIntoTexture: true) * cameraData.GetViewMatrix();
			_ = prevCamVPMatrix;
			prevCamVPMatrix = prevCamVPMatrix;
		}

		public void Dispose()
		{
		}
	}

	public class ScreenSpaceGlobalIlluminationPass : ScriptableRenderPass
	{
		private const string m_ProfilerTag = "Screen Space Global Illumination";

		private readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Screen Space Global Illumination");

		public ScreenSpaceGlobalIlluminationVolume ssgiVolume;

		public bool enableRenderingLayers;

		public bool overrideAmbientLighting;

		public bool isForwardPlus;

		public bool outputAPVLighting;

		public Material m_SSGIMaterial;

		private RTHandle m_IntermediateCameraColorHandle;

		private RTHandle m_DiffuseHandle;

		private RTHandle m_IntermediateDiffuseHandle;

		private RTHandle m_HistoryIndirectDiffuseHandle;

		private RTHandle m_HistoryDepthHandle;

		private RTHandle m_AccumulateSampleHandle;

		private RTHandle m_AccumulateHistorySampleHandle;

		private RTHandle m_HistoryCameraColorHandle;

		private RTHandle m_APVLightingHandle;

		private readonly RenderTargetIdentifier[] rTHandles = new RenderTargetIdentifier[2];

		private Matrix4x4 prevCamInvVPMatrix;

		private Vector3 prevCameraPositionWS;

		private bool isHistoryTextureValid;

		private bool enableDenoise;

		private int frameCount;

		private float resolutionScale = 1f;

		private float historyCameraScaledWidth;

		private float historyCameraScaledHeight;

		public static readonly float[] k_PreBlurRands = new float[32]
		{
			0.840188f, 0.394383f, 0.783099f, 0.79844f, 0.911647f, 0.197551f, 0.335223f, 0.76823f, 0.277775f, 0.55397f,
			0.477397f, 0.628871f, 0.364784f, 0.513401f, 0.95223f, 0.916195f, 0.635712f, 0.717297f, 0.141603f, 0.606969f,
			0.0163006f, 0.242887f, 0.137232f, 0.804177f, 0.156679f, 0.400944f, 0.12979f, 0.108809f, 0.998924f, 0.218257f,
			0.512932f, 0.839112f
		};

		public static readonly float[] k_BlurRands = new float[32]
		{
			0.61264f,
			0.296032f,
			0.637552f,
			0.524287f,
			0.493583f,
			0.972775f,
			43f / 147f,
			0.771358f,
			0.526745f,
			0.769914f,
			0.400229f,
			0.891529f,
			0.283315f,
			0.352458f,
			0.807725f,
			0.919026f,
			0.0697553f,
			0.949327f,
			0.525995f,
			0.0860558f,
			0.192214f,
			0.663227f,
			0.890233f,
			0.348893f,
			0.0641713f,
			0.020023f,
			0.457702f,
			0.0630958f,
			0.23828f,
			0.970634f,
			0.902208f,
			0.85092f
		};

		public static readonly float[] k_PostBlurRands = new float[32]
		{
			0.266666f, 0.53976f, 0.375207f, 0.760249f, 0.512535f, 0.667724f, 0.531606f, 0.0392803f, 0.437638f, 0.931835f,
			0.93081f, 0.720952f, 0.284293f, 0.738534f, 0.639979f, 0.354049f, 0.687861f, 0.165974f, 0.440105f, 0.880075f,
			0.829201f, 0.330337f, 0.228968f, 0.893372f, 0.35036f, 0.68667f, 0.956468f, 0.58864f, 0.657304f, 0.858676f,
			0.43956f, 0.92397f
		};

		public ScreenSpaceGlobalIlluminationPass(Material material)
		{
			m_SSGIMaterial = material;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				if (overrideAmbientLighting)
				{
					if (outputAPVLighting)
					{
						rTHandles[0] = m_IntermediateCameraColorHandle;
						rTHandles[1] = m_APVLightingHandle;
						commandBuffer.SetRenderTarget(rTHandles, m_IntermediateCameraColorHandle);
						Blitter.BlitTexture(commandBuffer, cameraColorTargetHandle, m_ScaleBias, m_SSGIMaterial, 0);
						m_SSGIMaterial.SetTexture(apvLightingTexture, m_APVLightingHandle);
					}
					else
					{
						Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, m_IntermediateCameraColorHandle, m_SSGIMaterial, 0);
					}
				}
				else
				{
					Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, m_IntermediateCameraColorHandle);
				}
				if (enableDenoise)
				{
					Blitter.BlitCameraTexture(commandBuffer, m_IntermediateCameraColorHandle, m_IntermediateDiffuseHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_SSGIMaterial, 1);
					m_SSGIMaterial.SetTexture(indirectDiffuseTexture, m_DiffuseHandle);
					commandBuffer.SetRenderTarget(m_AccumulateSampleHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_AccumulateSampleHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
					rTHandles[0] = m_DiffuseHandle;
					rTHandles[1] = m_AccumulateSampleHandle;
					commandBuffer.SetRenderTarget(rTHandles, m_AccumulateSampleHandle);
					Blitter.BlitTexture(commandBuffer, m_IntermediateDiffuseHandle, m_ScaleBias, m_SSGIMaterial, 2);
					if (ssgiVolume.denoiserAlgorithmSS == ScreenSpaceGlobalIlluminationVolume.DenoiserAlgorithm.Aggressive)
					{
						Blitter.BlitCameraTexture(commandBuffer, m_DiffuseHandle, m_IntermediateDiffuseHandle, m_SSGIMaterial, 8);
						Blitter.BlitCameraTexture(commandBuffer, m_IntermediateDiffuseHandle, m_DiffuseHandle, m_SSGIMaterial, 8);
					}
					if (ssgiVolume.secondDenoiserPassSS)
					{
						Blitter.BlitCameraTexture(commandBuffer, m_DiffuseHandle, m_IntermediateDiffuseHandle, m_SSGIMaterial, 3);
						Blitter.BlitCameraTexture(commandBuffer, m_IntermediateDiffuseHandle, m_DiffuseHandle, m_SSGIMaterial, 4);
					}
					commandBuffer.CopyTexture(m_DiffuseHandle, m_HistoryIndirectDiffuseHandle);
					Blitter.BlitCameraTexture(commandBuffer, m_HistoryDepthHandle, m_HistoryDepthHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_SSGIMaterial, 5);
					commandBuffer.CopyTexture(m_AccumulateSampleHandle, m_AccumulateHistorySampleHandle);
				}
				else
				{
					Blitter.BlitCameraTexture(commandBuffer, m_IntermediateCameraColorHandle, m_DiffuseHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_SSGIMaterial, 1);
					m_SSGIMaterial.SetTexture(indirectDiffuseTexture, m_DiffuseHandle);
					Blitter.BlitCameraTexture(commandBuffer, m_HistoryDepthHandle, m_HistoryDepthHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_SSGIMaterial, 5);
				}
				Blitter.BlitCameraTexture(commandBuffer, m_IntermediateCameraColorHandle, cameraColorTargetHandle, m_SSGIMaterial, 6);
				Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, m_HistoryCameraColorHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_SSGIMaterial, 9);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			NativeArray<VisibleReflectionProbe> visibleReflectionProbes = renderingData.cullResults.visibleReflectionProbes;
			Camera camera = renderingData.cameraData.camera;
			if (!isForwardPlus)
			{
				UpdateReflectionProbe(visibleReflectionProbes, camera.transform.position);
			}
			else
			{
				m_SSGIMaterial.SetFloat(probeSet, 0f);
			}
			m_SSGIMaterial.SetFloat(frameIndex, frameCount);
			m_SSGIMaterial.SetVector(_ReBlurBlurRotator, EvaluateRotator(k_BlurRands[frameCount % 32]));
			frameCount += 33;
			frameCount %= 64000;
			int num = (int)((float)camera.scaledPixelWidth * renderingData.cameraData.renderScale);
			int num2 = (int)((float)camera.scaledPixelHeight * renderingData.cameraData.renderScale);
			bool flag = ssgiVolume.denoiseSS != enableDenoise;
			bool flag2 = (ssgiVolume.fullResolutionSS ? (resolutionScale != 1f) : (ssgiVolume.resolutionScaleSS != resolutionScale));
			flag2 |= historyCameraScaledWidth != (float)num || historyCameraScaledHeight != (float)num2;
			if (flag | flag2)
			{
				isHistoryTextureValid = false;
			}
			historyCameraScaledWidth = num;
			historyCameraScaledHeight = num2;
			resolutionScale = (ssgiVolume.fullResolutionSS ? 1f : ssgiVolume.resolutionScaleSS);
			m_SSGIMaterial.SetFloat(downSample, resolutionScale);
			enableDenoise = ssgiVolume.denoiseSS;
			if (camera.cameraType != CameraType.Reflection)
			{
				camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
				_ = prevCamInvVPMatrix;
				m_SSGIMaterial.SetMatrix(_PrevInvViewProjMatrix, prevCamInvVPMatrix);
				_ = prevCameraPositionWS;
				m_SSGIMaterial.SetVector(_PrevCameraPositionWS, prevCameraPositionWS);
				prevCamInvVPMatrix = (GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * renderingData.cameraData.GetViewMatrix()).inverse;
				prevCameraPositionWS = camera.transform.position;
				m_SSGIMaterial.SetFloat(_PixelSpreadAngleTangent, Mathf.Tan(camera.fieldOfView * (MathF.PI / 180f) * 0.5f) * 2f / (float)Mathf.Min(Mathf.FloorToInt((float)camera.scaledPixelWidth * resolutionScale), Mathf.FloorToInt((float)camera.scaledPixelHeight * resolutionScale)));
			}
			if (overrideAmbientLighting)
			{
				SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
				m_SSGIMaterial.SetVector(shAr, new Vector4(ambientProbe[0, 3], ambientProbe[0, 1], ambientProbe[0, 2], ambientProbe[0, 0] - ambientProbe[0, 6]));
				m_SSGIMaterial.SetVector(shAg, new Vector4(ambientProbe[1, 3], ambientProbe[1, 1], ambientProbe[1, 2], ambientProbe[1, 0] - ambientProbe[1, 6]));
				m_SSGIMaterial.SetVector(shAb, new Vector4(ambientProbe[2, 3], ambientProbe[2, 1], ambientProbe[2, 2], ambientProbe[2, 0] - ambientProbe[2, 6]));
				m_SSGIMaterial.SetVector(shBr, new Vector4(ambientProbe[0, 4], ambientProbe[0, 5], ambientProbe[0, 6] * 3f, ambientProbe[0, 7]));
				m_SSGIMaterial.SetVector(shBg, new Vector4(ambientProbe[1, 4], ambientProbe[1, 5], ambientProbe[1, 6] * 3f, ambientProbe[1, 7]));
				m_SSGIMaterial.SetVector(shBb, new Vector4(ambientProbe[2, 4], ambientProbe[2, 5], ambientProbe[2, 6] * 3f, ambientProbe[2, 7]));
				m_SSGIMaterial.SetVector(shC, new Vector4(ambientProbe[0, 8], ambientProbe[1, 8], ambientProbe[2, 8], 1f));
			}
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			if (descriptor.width != num)
			{
				descriptor = new RenderTextureDescriptor(num, num2);
			}
			descriptor.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
			descriptor.depthBufferBits = 0;
			descriptor.stencilFormat = GraphicsFormat.None;
			descriptor.msaaSamples = 1;
			RenderTextureDescriptor descriptor2 = descriptor;
			RenderingUtils.ReAllocateIfNeeded(ref m_IntermediateCameraColorHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_IntermediateCameraColorTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_APVLightingHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_APVLightingTexture");
			descriptor.width = Mathf.FloorToInt((float)descriptor.width * resolutionScale);
			descriptor.height = Mathf.FloorToInt((float)descriptor.height * resolutionScale);
			RenderingUtils.ReAllocateIfNeeded(ref m_HistoryCameraColorHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSGIHistoryCameraColorTexture");
			if (isHistoryTextureValid)
			{
				m_SSGIMaterial.SetFloat(_HistoryTextureValid, 1f);
				m_SSGIMaterial.SetTexture(ssgiHistoryCameraColorTexture, m_HistoryCameraColorHandle);
			}
			else
			{
				m_SSGIMaterial.SetFloat(_HistoryTextureValid, 0f);
				isHistoryTextureValid = true;
			}
			descriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
			descriptor2.colorFormat = RenderTextureFormat.RFloat;
			RenderingUtils.ReAllocateIfNeeded(ref m_DiffuseHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_IndirectDiffuseTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_IntermediateDiffuseHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_IntermediateIndirectDiffuseTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_HistoryIndirectDiffuseHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_HistoryIndirectDiffuseTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_HistoryDepthHandle, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSGIHistoryDepthTexture");
			descriptor.colorFormat = RenderTextureFormat.RHalf;
			RenderingUtils.ReAllocateIfNeeded(ref m_AccumulateSampleHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSGISampleTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_AccumulateHistorySampleHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSGIHistorySampleTexture");
			m_SSGIMaterial.SetTexture(ssgiHistoryDepthTexture, m_HistoryDepthHandle);
			m_SSGIMaterial.SetTexture(historyIndirectDiffuseTexture, m_HistoryIndirectDiffuseHandle);
			m_SSGIMaterial.SetTexture(ssgiSampleTexture, m_AccumulateSampleHandle);
			m_SSGIMaterial.SetTexture(ssgiHistorySampleTexture, m_AccumulateHistorySampleHandle);
			ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Motion);
		}

		public void Dispose()
		{
			m_IntermediateCameraColorHandle?.Release();
			m_DiffuseHandle?.Release();
			m_IntermediateDiffuseHandle?.Release();
			m_HistoryIndirectDiffuseHandle?.Release();
			m_HistoryDepthHandle?.Release();
			m_AccumulateSampleHandle?.Release();
			m_AccumulateHistorySampleHandle?.Release();
			m_HistoryCameraColorHandle?.Release();
			m_APVLightingHandle?.Release();
		}

		private Vector4 EvaluateRotator(float rand)
		{
			float num = Mathf.Cos(rand);
			float num2 = Mathf.Sin(rand);
			return new Vector4(num, num2, 0f - num2, num);
		}

		private void UpdateReflectionProbe(NativeArray<VisibleReflectionProbe> visibleReflectionProbes, Vector3 cameraPosition)
		{
			if (ssgiVolume.IsFallbackReflectionProbes() && !Shader.IsKeywordEnabled("_FORWARD_PLUS"))
			{
				ReflectionProbe closestProbe = GetClosestProbe(visibleReflectionProbes, cameraPosition);
				if (closestProbe != null)
				{
					m_SSGIMaterial.SetTexture(specCube0, closestProbe.texture);
					m_SSGIMaterial.SetVector(specCube0_HDR, closestProbe.textureHDRDecodeValues);
					bool boxProjection = closestProbe.boxProjection;
					if (boxProjection)
					{
						Vector3 position = closestProbe.transform.position;
						float w = (boxProjection ? 1f : 0f);
						m_SSGIMaterial.SetVector(specCube0_BoxMin, closestProbe.bounds.min);
						m_SSGIMaterial.SetVector(specCube0_BoxMax, closestProbe.bounds.max);
						m_SSGIMaterial.SetVector(specCube0_ProbePosition, new Vector4(position.x, position.y, position.z, w));
					}
					m_SSGIMaterial.SetFloat(probeWeight, 0f);
					m_SSGIMaterial.SetFloat(probeSet, 1f);
				}
				else
				{
					m_SSGIMaterial.SetFloat(probeSet, 0f);
				}
			}
			else
			{
				m_SSGIMaterial.SetFloat(probeSet, 0f);
			}
		}

		private static ReflectionProbe GetClosestProbe(NativeArray<VisibleReflectionProbe> visibleReflectionProbes, Vector3 cameraPosition)
		{
			ReflectionProbe result = null;
			float num = float.MaxValue;
			int num2 = int.MinValue;
			float num3 = float.MaxValue;
			foreach (VisibleReflectionProbe item in visibleReflectionProbes)
			{
				ReflectionProbe reflectionProbe = item.reflectionProbe;
				Bounds bounds = reflectionProbe.bounds;
				int importance = reflectionProbe.importance;
				float magnitude = bounds.size.magnitude;
				if (bounds.Contains(cameraPosition))
				{
					float num4 = Vector3.Distance(cameraPosition, reflectionProbe.transform.position);
					bool flag = importance > num2;
					bool flag2 = importance == num2 && magnitude < num3;
					bool flag3 = magnitude == num3 && num4 < num;
					if (flag | flag2 | flag3)
					{
						num = num4;
						num2 = importance;
						num3 = magnitude;
						result = reflectionProbe;
					}
				}
			}
			return result;
		}
	}

	public class BackfaceDataPass : ScriptableRenderPass
	{
		private const string m_ProfilerTag = "Render Backface Data";

		private readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render Backface Data");

		private RTHandle m_BackDepthHandle;

		private RTHandle m_BackColorHandle;

		public bool backfaceLighting;

		private RenderStateBlock m_DepthRenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		private readonly ShaderTagId[] m_LitTags = new ShaderTagId[2];

		private const string k_DepthOnly = "DepthOnly";

		private const string k_UniversalForward = "UniversalForward";

		private const string k_UniversalForwardOnly = "UniversalForwardOnly";

		private readonly ShaderTagId depthOnly = new ShaderTagId("DepthOnly");

		private readonly ShaderTagId universalForward = new ShaderTagId("UniversalForward");

		private readonly ShaderTagId universalForwardOnly = new ShaderTagId("UniversalForwardOnly");

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.msaaSamples = 1;
			descriptor.graphicsFormat = GraphicsFormat.None;
			if (!backfaceLighting)
			{
				RenderingUtils.ReAllocateIfNeeded(ref m_BackDepthHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraBackDepthTexture");
				cmd.SetGlobalTexture(cameraBackDepthTexture, m_BackDepthHandle);
				ConfigureTarget(m_BackDepthHandle, m_BackDepthHandle);
				ConfigureClear(ClearFlag.Depth, Color.clear);
				return;
			}
			RenderTextureDescriptor descriptor2 = renderingData.cameraData.cameraTargetDescriptor;
			descriptor2.depthStencilFormat = GraphicsFormat.None;
			descriptor2.msaaSamples = 1;
			descriptor2.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
			RenderingUtils.ReAllocateIfNeeded(ref m_BackColorHandle, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraBackOpaqueTexture");
			RenderingUtils.ReAllocateIfNeeded(ref m_BackDepthHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraBackDepthTexture");
			cmd.SetGlobalTexture(cameraBackDepthTexture, m_BackDepthHandle);
			cmd.SetGlobalTexture(cameraBackOpaqueTexture, m_BackColorHandle);
			ConfigureTarget(m_BackColorHandle, m_BackDepthHandle);
			ConfigureClear(ClearFlag.Color | ClearFlag.Depth, Color.clear);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			if (!backfaceLighting)
			{
				using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
				{
					RendererListDesc desc = new RendererListDesc(depthOnly, renderingData.cullResults, renderingData.cameraData.camera);
					m_DepthRenderStateBlock.depthState = new DepthState(writeEnabled: true, CompareFunction.LessEqual);
					m_DepthRenderStateBlock.mask |= RenderStateMask.Depth;
					m_DepthRenderStateBlock.rasterState = new RasterState(CullMode.Front);
					m_DepthRenderStateBlock.mask |= RenderStateMask.Raster;
					desc.stateBlock = m_DepthRenderStateBlock;
					desc.sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
					desc.renderQueueRange = RenderQueueRange.opaque;
					RendererList rendererList = context.CreateRendererList(desc);
					commandBuffer.DrawRendererList(rendererList);
				}
			}
			else
			{
				using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
				{
					m_LitTags[0] = universalForward;
					m_LitTags[1] = universalForwardOnly;
					RendererListDesc desc2 = new RendererListDesc(m_LitTags, renderingData.cullResults, renderingData.cameraData.camera);
					m_DepthRenderStateBlock.depthState = new DepthState(writeEnabled: true, CompareFunction.LessEqual);
					m_DepthRenderStateBlock.mask |= RenderStateMask.Depth;
					m_DepthRenderStateBlock.rasterState = new RasterState(CullMode.Front);
					m_DepthRenderStateBlock.mask |= RenderStateMask.Raster;
					desc2.stateBlock = m_DepthRenderStateBlock;
					desc2.sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
					desc2.renderQueueRange = RenderQueueRange.opaque;
					RendererList rendererList2 = context.CreateRendererList(desc2);
					commandBuffer.DrawRendererList(rendererList2);
				}
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		public void Dispose()
		{
			m_BackDepthHandle?.Release();
			if (backfaceLighting)
			{
				m_BackColorHandle?.Release();
			}
		}
	}

	public class ForwardGBufferPass : ScriptableRenderPass
	{
		private const string m_ProfilerTag = "Render Forward GBuffer";

		private readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render Forward GBuffer");

		private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

		private FilteringSettings m_filter;

		private RenderStateBlock m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		public RTHandle m_GBuffer0;

		public RTHandle m_GBuffer1;

		public RTHandle m_GBuffer2;

		public RTHandle m_GBufferDepth;

		private RTHandle[] m_GBuffers;

		public ForwardGBufferPass(string[] PassNames)
		{
			RenderQueueRange opaque = RenderQueueRange.opaque;
			m_filter = new FilteringSettings(opaque);
			if (PassNames != null && PassNames.Length != 0)
			{
				foreach (string name in PassNames)
				{
					m_ShaderTagIdList.Add(new ShaderTagId(name));
				}
			}
		}

		public GraphicsFormat GetGBufferFormat(int index)
		{
			switch (index)
			{
			case 0:
				if (QualitySettings.activeColorSpace != ColorSpace.Linear)
				{
					return GraphicsFormat.R8G8B8A8_UNorm;
				}
				return GraphicsFormat.R8G8B8A8_SRGB;
			case 1:
				return GraphicsFormat.R8G8B8A8_UNorm;
			case 2:
				if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8A8_SNorm, FormatUsage.Render))
				{
					return GraphicsFormat.R8G8B8A8_SNorm;
				}
				return GraphicsFormat.R16G16B16A16_SFloat;
			default:
				return GraphicsFormat.None;
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			SortingCriteria defaultOpaqueSortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				RendererListDesc desc = new RendererListDesc(m_ShaderTagIdList[0], renderingData.cullResults, renderingData.cameraData.camera);
				desc.stateBlock = m_RenderStateBlock;
				desc.sortingCriteria = defaultOpaqueSortFlags;
				desc.renderQueueRange = m_filter.renderQueueRange;
				RendererList rendererList = context.CreateRendererList(desc);
				commandBuffer.DrawRendererList(rendererList);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = 0;
			descriptor.stencilFormat = GraphicsFormat.None;
			descriptor.msaaSamples = 1;
			descriptor.graphicsFormat = GetGBufferFormat(0);
			RenderingUtils.ReAllocateIfNeeded(ref m_GBuffer0, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_GBuffer0");
			cmd.SetGlobalTexture(gBuffer0, m_GBuffer0);
			descriptor.graphicsFormat = GetGBufferFormat(1);
			RenderingUtils.ReAllocateIfNeeded(ref m_GBuffer1, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_GBuffer1");
			cmd.SetGlobalTexture(gBuffer1, m_GBuffer1);
			descriptor.graphicsFormat = GetGBufferFormat(2);
			RenderingUtils.ReAllocateIfNeeded(ref m_GBuffer2, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_GBuffer2");
			cmd.SetGlobalTexture(gBuffer2, m_GBuffer2);
			m_GBuffers = new RTHandle[3] { m_GBuffer0, m_GBuffer1, m_GBuffer2 };
			bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
			bool num = !flag && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth) && renderingData.cameraData.cameraTargetDescriptor.msaaSamples == descriptor.msaaSamples;
			RenderTextureDescriptor descriptor2 = renderingData.cameraData.cameraTargetDescriptor;
			descriptor2.msaaSamples = 1;
			descriptor2.graphicsFormat = GraphicsFormat.None;
			RenderingUtils.ReAllocateIfNeeded(ref m_GBufferDepth, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_GBufferDepthTexture");
			if (num)
			{
				ConfigureTarget(m_GBuffers, renderingData.cameraData.renderer.cameraDepthTargetHandle);
			}
			else
			{
				ConfigureTarget(m_GBuffers, m_GBufferDepth);
			}
			ConfigureInput(ScriptableRenderPassInput.Depth);
			if (flag)
			{
				ConfigureClear(ClearFlag.Color | ClearFlag.Depth, Color.black);
			}
			else
			{
				ConfigureClear(ClearFlag.Color, Color.clear);
			}
			if (num)
			{
				m_RenderStateBlock.depthState = new DepthState(writeEnabled: false, CompareFunction.Equal);
				m_RenderStateBlock.mask |= RenderStateMask.Depth;
			}
			else if (m_RenderStateBlock.depthState.compareFunction == CompareFunction.Equal)
			{
				m_RenderStateBlock.depthState = new DepthState(writeEnabled: true, CompareFunction.LessEqual);
				m_RenderStateBlock.mask |= RenderStateMask.Depth;
			}
		}

		public void Dispose()
		{
			m_GBuffer0?.Release();
			m_GBuffer1?.Release();
			m_GBuffer2?.Release();
			m_GBufferDepth?.Release();
		}
	}

	private Material m_SSGIMaterial;

	[SerializeField]
	private ScreenSpaceGlobalIlluminationVolume ssgiVolume;

	[Header("Setup")]
	[Tooltip("The shader of screen space global illumination.")]
	[SerializeField]
	private Shader m_Shader;

	[Tooltip("Specifies if URP computes screen space global illumination in Rendering Debugger view. \nThis is disabled by default to avoid affecting the individual lighting previews.")]
	[SerializeField]
	private bool m_RenderingDebugger;

	[Header("Performance")]
	[Tooltip("Specifies if URP computes screen space global illumination in both real-time and baked reflection probes. \nScreen space global illumination in real-time reflection probes may reduce performace.")]
	[SerializeField]
	private bool m_ReflectionProbes = true;

	[Tooltip("Enables high-quality upscaling for screen space global illumination. \nThis may impact performance.")]
	[SerializeField]
	private bool m_HighQualityUpscaling;

	[Header("Lighting")]
	[Tooltip("Specifies if screen space global illumination overrides ambient lighting. \nThis ensures the accuracy of indirect lighting from SSGI.")]
	[SerializeField]
	private bool m_OverrideAmbientLighting = true;

	[Header("Advanced")]
	[Tooltip("Renders back-face lighting when using automatic thickness mode. \nThis improves accuracy in some cases, but may severely impact performance.")]
	[SerializeField]
	private bool m_BackfaceLighting;

	private const string m_SSGIShaderName = "Hidden/Lighting/ScreenSpaceGlobalIllumination";

	private readonly string[] m_GBufferPassNames = new string[1] { "UniversalGBuffer" };

	private PreRenderScreenSpaceGlobalIlluminationPass m_PreRenderSSGIPass;

	private ScreenSpaceGlobalIlluminationPass m_SSGIPass;

	private BackfaceDataPass m_BackfaceDataPass;

	private ForwardGBufferPass m_ForwardGBufferPass;

	private static readonly FieldInfo gBufferFieldInfo = typeof(UniversalRenderer).GetField("m_GBufferPass", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo motionVectorPassFieldInfo = typeof(UniversalRenderer).GetField("m_MotionVectorPass", BindingFlags.Instance | BindingFlags.NonPublic);

	private bool isShaderMismatchLogPrinted;

	private bool isDebuggerLogPrinted;

	private bool isBackfaceLightingLogPrinted;

	private static readonly int _MaxSteps = Shader.PropertyToID("_MaxSteps");

	private static readonly int _MaxSmallSteps = Shader.PropertyToID("_MaxSmallSteps");

	private static readonly int _MaxMediumSteps = Shader.PropertyToID("_MaxMediumSteps");

	private static readonly int _Thickness = Shader.PropertyToID("_Thickness");

	private static readonly int _Thickness_Increment = Shader.PropertyToID("_Thickness_Increment");

	private static readonly int _StepSize = Shader.PropertyToID("_StepSize");

	private static readonly int _SmallStepSize = Shader.PropertyToID("_SmallStepSize");

	private static readonly int _MediumStepSize = Shader.PropertyToID("_MediumStepSize");

	private static readonly int _RayCount = Shader.PropertyToID("_RayCount");

	private static readonly int _TemporalIntensity = Shader.PropertyToID("_TemporalIntensity");

	private static readonly int _MaxBrightness = Shader.PropertyToID("_MaxBrightness");

	private static readonly int _IsProbeCamera = Shader.PropertyToID("_IsProbeCamera");

	private static readonly int _BackDepthEnabled = Shader.PropertyToID("_BackDepthEnabled");

	private static readonly int _PrevInvViewProjMatrix = Shader.PropertyToID("_PrevInvViewProjMatrix");

	private static readonly int _PrevCameraPositionWS = Shader.PropertyToID("_PrevCameraPositionWS");

	private static readonly int _PixelSpreadAngleTangent = Shader.PropertyToID("_PixelSpreadAngleTangent");

	private static readonly int _HistoryTextureValid = Shader.PropertyToID("_HistoryTextureValid");

	private static readonly int _IndirectDiffuseLightingMultiplier = Shader.PropertyToID("_IndirectDiffuseLightingMultiplier");

	private static readonly int _IndirectDiffuseRenderingLayers = Shader.PropertyToID("_IndirectDiffuseRenderingLayers");

	private static readonly int _AggressiveDenoise = Shader.PropertyToID("_AggressiveDenoise");

	private static readonly int _ReBlurBlurRotator = Shader.PropertyToID("_ReBlurBlurRotator");

	private static readonly int _ReBlurDenoiserRadius = Shader.PropertyToID("_ReBlurDenoiserRadius");

	private const string _CameraDepthTexture = "_CameraDepthTexture";

	private const string _IndirectDiffuseTexture = "_IndirectDiffuseTexture";

	private const string _IntermediateIndirectDiffuseTexture = "_IntermediateIndirectDiffuseTexture";

	private const string _IntermediateCameraColorTexture = "_IntermediateCameraColorTexture";

	private const string _SSGIHistoryDepthTexture = "_SSGIHistoryDepthTexture";

	private const string _CameraBackDepthTexture = "_CameraBackDepthTexture";

	private const string _CameraBackOpaqueTexture = "_CameraBackOpaqueTexture";

	private const string _HistoryIndirectDiffuseTexture = "_HistoryIndirectDiffuseTexture";

	private const string _SSGISampleTexture = "_SSGISampleTexture";

	private const string _SSGIHistorySampleTexture = "_SSGIHistorySampleTexture";

	private const string _SSGIHistoryCameraColorTexture = "_SSGIHistoryCameraColorTexture";

	private const string _APVLightingTexture = "_APVLightingTexture";

	private static readonly int cameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int indirectDiffuseTexture = Shader.PropertyToID("_IndirectDiffuseTexture");

	private static readonly int ssgiHistoryDepthTexture = Shader.PropertyToID("_SSGIHistoryDepthTexture");

	private static readonly int cameraBackDepthTexture = Shader.PropertyToID("_CameraBackDepthTexture");

	private static readonly int cameraBackOpaqueTexture = Shader.PropertyToID("_CameraBackOpaqueTexture");

	private static readonly int historyIndirectDiffuseTexture = Shader.PropertyToID("_HistoryIndirectDiffuseTexture");

	private static readonly int ssgiSampleTexture = Shader.PropertyToID("_SSGISampleTexture");

	private static readonly int ssgiHistorySampleTexture = Shader.PropertyToID("_SSGIHistorySampleTexture");

	private static readonly int ssgiHistoryCameraColorTexture = Shader.PropertyToID("_SSGIHistoryCameraColorTexture");

	private static readonly int apvLightingTexture = Shader.PropertyToID("_APVLightingTexture");

	private const string _GBuffer0 = "_GBuffer0";

	private const string _GBuffer1 = "_GBuffer1";

	private const string _GBuffer2 = "_GBuffer2";

	private const string _GBufferDepth = "_GBufferDepthTexture";

	private static readonly int gBuffer0 = Shader.PropertyToID("_GBuffer0");

	private static readonly int gBuffer1 = Shader.PropertyToID("_GBuffer1");

	private static readonly int gBuffer2 = Shader.PropertyToID("_GBuffer2");

	private static readonly int specCube0 = Shader.PropertyToID("_SpecCube0");

	private static readonly int specCube0_HDR = Shader.PropertyToID("_SpecCube0_HDR");

	private static readonly int specCube0_BoxMin = Shader.PropertyToID("_SpecCube0_BoxMin");

	private static readonly int specCube0_BoxMax = Shader.PropertyToID("_SpecCube0_BoxMax");

	private static readonly int specCube0_ProbePosition = Shader.PropertyToID("_SpecCube0_ProbePosition");

	private static readonly int probeWeight = Shader.PropertyToID("_ProbeWeight");

	private static readonly int probeSet = Shader.PropertyToID("_ProbeSet");

	private static readonly int downSample = Shader.PropertyToID("_DownSample");

	private static readonly int frameIndex = Shader.PropertyToID("_FrameIndex");

	private static readonly int shAr = Shader.PropertyToID("ssgi_SHAr");

	private static readonly int shAg = Shader.PropertyToID("ssgi_SHAg");

	private static readonly int shAb = Shader.PropertyToID("ssgi_SHAb");

	private static readonly int shBr = Shader.PropertyToID("ssgi_SHBr");

	private static readonly int shBg = Shader.PropertyToID("ssgi_SHBg");

	private static readonly int shBb = Shader.PropertyToID("ssgi_SHBb");

	private static readonly int shC = Shader.PropertyToID("ssgi_SHC");

	private const string _FP_REFL_PROBE_ATLAS = "_FP_REFL_PROBE_ATLAS";

	private const string _RAYMARCHING_FALLBACK_SKY = "_RAYMARCHING_FALLBACK_SKY";

	private const string _RAYMARCHING_FALLBACK_REFLECTION_PROBES = "_RAYMARCHING_FALLBACK_REFLECTION_PROBES";

	private const string _BACKFACE_TEXTURES = "_BACKFACE_TEXTURES";

	private const string _FORWARD_PLUS = "_FORWARD_PLUS";

	private const string _WRITE_RENDERING_LAYERS = "_WRITE_RENDERING_LAYERS";

	private const string _USE_RENDERING_LAYERS = "_USE_RENDERING_LAYERS";

	private const string _DEPTH_NORMALS_UPSCALE = "_DEPTH_NORMALS_UPSCALE";

	private const string PROBE_VOLUMES_L1 = "PROBE_VOLUMES_L1";

	private const string PROBE_VOLUMES_L2 = "PROBE_VOLUMES_L2";

	private const string _APV_LIGHTING_BUFFER = "_APV_LIGHTING_BUFFER";

	private const string SSGI_RENDER_GBUFFER = "SSGI_RENDER_GBUFFER";

	private const string SSGI_RENDER_BACKFACE_DEPTH = "SSGI_RENDER_BACKFACE_DEPTH";

	private const string SSGI_RENDER_BACKFACE_COLOR = "SSGI_RENDER_BACKFACE_COLOR";

	private const float k_BlurMaxRadius = 0.04f;

	private static readonly Vector4 m_ScaleBias = new Vector4(1f, 1f, 0f, 0f);

	public Material SSGIMaterial => m_SSGIMaterial;

	public Shader SSGIShader
	{
		get
		{
			return m_Shader;
		}
		set
		{
			m_Shader = ((value == Shader.Find("Hidden/Lighting/ScreenSpaceGlobalIllumination")) ? value : m_Shader);
		}
	}

	public bool RenderingDebugger
	{
		get
		{
			return m_RenderingDebugger;
		}
		set
		{
			m_RenderingDebugger = value;
		}
	}

	public bool ReflectionProbes
	{
		get
		{
			return m_ReflectionProbes;
		}
		set
		{
			m_ReflectionProbes = value;
		}
	}

	public bool HighQualityUpscaling
	{
		get
		{
			return m_HighQualityUpscaling;
		}
		set
		{
			m_HighQualityUpscaling = value;
		}
	}

	public bool OverrideAmbientLighting
	{
		get
		{
			return m_OverrideAmbientLighting;
		}
		set
		{
			m_OverrideAmbientLighting = value;
		}
	}

	public bool BackfaceLighting
	{
		get
		{
			return m_BackfaceLighting;
		}
		set
		{
			m_BackfaceLighting = value;
		}
	}

	public override void Create()
	{
		if (!(m_Shader != Shader.Find("Hidden/Lighting/ScreenSpaceGlobalIllumination")))
		{
			isShaderMismatchLogPrinted = false;
			m_SSGIMaterial = CoreUtils.CreateEngineMaterial(m_Shader);
			if (m_PreRenderSSGIPass == null)
			{
				m_PreRenderSSGIPass = new PreRenderScreenSpaceGlobalIlluminationPass();
				m_PreRenderSSGIPass.renderPassEvent = (RenderPassEvent)449;
			}
			if (m_SSGIPass == null)
			{
				m_SSGIPass = new ScreenSpaceGlobalIlluminationPass(m_SSGIMaterial);
				m_SSGIPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
			}
			m_SSGIPass.m_SSGIMaterial = m_SSGIMaterial;
			if (m_BackfaceDataPass == null)
			{
				m_BackfaceDataPass = new BackfaceDataPass();
				m_BackfaceDataPass.renderPassEvent = (RenderPassEvent)299;
			}
			if (m_ForwardGBufferPass == null)
			{
				m_ForwardGBufferPass = new ForwardGBufferPass(m_GBufferPassNames);
				m_ForwardGBufferPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (m_PreRenderSSGIPass != null)
		{
			m_PreRenderSSGIPass.Dispose();
		}
		if (m_SSGIPass != null)
		{
			m_SSGIPass.Dispose();
		}
		if (m_BackfaceDataPass != null)
		{
			if (m_SSGIMaterial != null)
			{
				m_SSGIMaterial.SetFloat(_BackDepthEnabled, 0f);
			}
			m_BackfaceDataPass.Dispose();
		}
		if (m_ForwardGBufferPass != null)
		{
			m_ForwardGBufferPass.Dispose();
		}
		if (m_SSGIMaterial != null)
		{
			CoreUtils.Destroy(m_SSGIMaterial);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (isShaderMismatchLogPrinted || renderingData.cameraData.camera.cameraType == CameraType.Preview)
		{
			return;
		}
		bool num = ssgiVolume != null && ssgiVolume.IsActive();
		bool isAnyDebugUIActive = DebugManager.instance.isAnyDebugUIActive;
		bool flag = !m_ReflectionProbes && renderingData.cameraData.camera.cameraType == CameraType.Reflection;
		flag |= ssgiVolume.indirectDiffuseLightingMultiplier == 0f && !m_OverrideAmbientLighting;
		if (!num | flag)
		{
			return;
		}
		bool flag2 = ssgiVolume.maxRaySteps <= 16;
		int num2 = ssgiVolume.maxRaySteps / 8;
		int num3 = ((!flag2) ? Mathf.Max(num2, 4) : 0);
		int num4 = (flag2 ? (num2 + 2) : (num3 + num2 * 2));
		float t = (ssgiVolume.fullResolutionSS ? 1f : ssgiVolume.resolutionScaleSS);
		float value = Mathf.Lerp(ssgiVolume.denoiseIntensitySS + 0.02f, ssgiVolume.denoiseIntensitySS - 0.04f, t);
		m_SSGIMaterial.SetFloat(_MaxSteps, ssgiVolume.maxRaySteps);
		m_SSGIMaterial.SetFloat(_MaxSmallSteps, num3);
		m_SSGIMaterial.SetFloat(_MaxMediumSteps, num4);
		m_SSGIMaterial.SetFloat(_StepSize, flag2 ? 0.5f : 0.4f);
		m_SSGIMaterial.SetFloat(_SmallStepSize, (num3 < 4) ? 0.05f : 0.015f);
		m_SSGIMaterial.SetFloat(_MediumStepSize, flag2 ? 0.1f : 0.05f);
		m_SSGIMaterial.SetFloat(_Thickness, ssgiVolume.depthBufferThickness);
		m_SSGIMaterial.SetFloat(_Thickness_Increment, ssgiVolume.depthBufferThickness * 0.25f);
		m_SSGIMaterial.SetFloat(_RayCount, ssgiVolume.sampleCount);
		m_SSGIMaterial.SetFloat(_TemporalIntensity, value);
		m_SSGIMaterial.SetFloat(_ReBlurDenoiserRadius, ssgiVolume.denoiserRadiusSS * 2f * 0.04f);
		m_SSGIMaterial.SetFloat(_IndirectDiffuseLightingMultiplier, ssgiVolume.indirectDiffuseLightingMultiplier);
		m_SSGIMaterial.SetFloat(_MaxBrightness, 7f);
		m_SSGIMaterial.SetFloat(_AggressiveDenoise, (ssgiVolume.denoiserAlgorithmSS == ScreenSpaceGlobalIlluminationVolume.DenoiserAlgorithm.Aggressive) ? 1f : 0f);
		bool enableRenderingLayers = false;
		m_SSGIMaterial.DisableKeyword("_USE_RENDERING_LAYERS");
		m_SSGIPass.ssgiVolume = ssgiVolume;
		m_SSGIPass.enableRenderingLayers = enableRenderingLayers;
		m_SSGIPass.overrideAmbientLighting = m_OverrideAmbientLighting;
		if (ssgiVolume.IsFallbackSky())
		{
			m_SSGIMaterial.EnableKeyword("_RAYMARCHING_FALLBACK_SKY");
		}
		else
		{
			m_SSGIMaterial.DisableKeyword("_RAYMARCHING_FALLBACK_SKY");
		}
		m_SSGIPass.outputAPVLighting = false;
		m_SSGIMaterial.DisableKeyword("_APV_LIGHTING_BUFFER");
		bool flag3 = ssgiVolume.IsFallbackReflectionProbes();
		if (flag3)
		{
			m_SSGIMaterial.EnableKeyword("_RAYMARCHING_FALLBACK_REFLECTION_PROBES");
		}
		else
		{
			m_SSGIMaterial.DisableKeyword("_RAYMARCHING_FALLBACK_REFLECTION_PROBES");
		}
		bool flag4 = Shader.IsKeywordEnabled("_FORWARD_PLUS");
		if (flag4 & flag3)
		{
			m_SSGIMaterial.EnableKeyword("_FP_REFL_PROBE_ATLAS");
		}
		else
		{
			m_SSGIMaterial.DisableKeyword("_FP_REFL_PROBE_ATLAS");
		}
		m_SSGIPass.isForwardPlus = flag4;
		bool flag5 = renderingData.cameraData.camera.cameraType == CameraType.Reflection;
		m_SSGIMaterial.SetFloat(_IsProbeCamera, flag5 ? 1f : 0f);
		if (m_HighQualityUpscaling)
		{
			m_SSGIMaterial.EnableKeyword("_DEPTH_NORMALS_UPSCALE");
		}
		else
		{
			m_SSGIMaterial.DisableKeyword("_DEPTH_NORMALS_UPSCALE");
		}
		if (renderingData.cameraData.camera.cameraType != CameraType.Preview && (!isAnyDebugUIActive || m_RenderingDebugger))
		{
			renderer.EnqueuePass(m_SSGIPass);
		}
		bool flag6 = gBufferFieldInfo.GetValue(renderer) != null;
		flag6 &= (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3) & (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore);
		if (ssgiVolume.thicknessMode != ScreenSpaceGlobalIlluminationVolume.ThicknessMode.Constant)
		{
			bool flag7 = m_BackfaceLighting && !flag6;
			m_BackfaceDataPass.backfaceLighting = flag7;
			renderer.EnqueuePass(m_BackfaceDataPass);
			m_SSGIMaterial.EnableKeyword("_BACKFACE_TEXTURES");
			Shader.EnableKeyword("SSGI_RENDER_BACKFACE_DEPTH");
			if (flag7)
			{
				m_SSGIMaterial.SetFloat(_BackDepthEnabled, 2f);
				Shader.EnableKeyword("SSGI_RENDER_BACKFACE_COLOR");
			}
			else
			{
				m_SSGIMaterial.SetFloat(_BackDepthEnabled, 1f);
				Shader.DisableKeyword("SSGI_RENDER_BACKFACE_COLOR");
			}
		}
		else
		{
			m_SSGIMaterial.DisableKeyword("_BACKFACE_TEXTURES");
			Shader.DisableKeyword("SSGI_RENDER_BACKFACE_DEPTH");
			Shader.DisableKeyword("SSGI_RENDER_BACKFACE_COLOR");
			m_SSGIMaterial.SetFloat(_BackDepthEnabled, 0f);
		}
		if (!flag6)
		{
			renderer.EnqueuePass(m_ForwardGBufferPass);
			Shader.EnableKeyword("SSGI_RENDER_GBUFFER");
		}
		else
		{
			Shader.DisableKeyword("SSGI_RENDER_GBUFFER");
		}
	}

	[ConsoleCommand(Command = "SSGI", Description = "Toggle a cinematic screen-space GI, off by default")]
	public static void ToggleCinematicGI(IConsole console)
	{
		ScreenSpaceGlobalIlluminationVolume.enable = !ScreenSpaceGlobalIlluminationVolume.enable;
	}
}

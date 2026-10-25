using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPShadowOptimization : ScriptableRendererFeature
{
	[Serializable]
	internal class ScreenSpaceShadowsSettings
	{
	}

	private class RestoreShadowKeywordsPass : ScriptableRenderPass
	{
		private const string m_ProfilerTag = "Restore Shadow Keywords Pass";

		private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Restore Shadow Keywords Pass");

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureTarget(BuiltinRenderTextureType.CurrentActive);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				int mainLightShadowCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
				bool supportsMainLightShadows = renderingData.shadowData.supportsMainLightShadows;
				bool state = supportsMainLightShadows && mainLightShadowCascadesCount == 1;
				bool state2 = supportsMainLightShadows && mainLightShadowCascadesCount > 1;
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state2);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	private class CustomShadowPass : ScriptableRenderPass
	{
		internal enum URPProfileId
		{
			UniversalRenderTotal,
			UpdateVolumeFramework,
			RenderCameraStack,
			AdditionalLightsShadow,
			ColorGradingLUT,
			CopyColor,
			CopyDepth,
			DepthNormalPrepass,
			DepthPrepass,
			DrawOpaqueObjects,
			DrawTransparentObjects,
			LightCookies,
			MainLightShadow,
			ResolveShadows,
			SSAO,
			StopNaNs,
			SMAA,
			GaussianDepthOfField,
			BokehDepthOfField,
			MotionBlur,
			PaniniProjection,
			UberPostProcess,
			Bloom,
			LensFlareDataDriven,
			MotionVectors,
			FinalBlit
		}

		internal string profilerTag;

		internal Material material;

		private ScreenSpaceShadowsSettings m_CurrentSettings;

		private RenderTextureDescriptor m_RenderTextureDescriptor;

		private RenderTargetHandle m_RenderTarget;

		private const string k_SSShadowsTextureName = "_ScreenSpaceShadowmapTexture";

		public void Init(ref RenderingData renderingData)
		{
			renderingData.shadowData.supportsAdditionalLightShadows = false;
			renderingData.shadowData.supportsMainLightShadows = false;
			renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (material == null)
			{
				Debug.LogErrorFormat("{0}.Execute(): Missing material. {1} render pass will not execute. Check for missing reference in the renderer resources.", GetType().Name, profilerTag);
				return;
			}
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.ResolveShadows)))
			{
				RenderTargetIdentifier dest = m_RenderTarget.Identifier();
				commandBuffer.Blit(null, dest, material);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: true);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}

		internal bool Setup(ScreenSpaceShadowsSettings featureSettings)
		{
			m_CurrentSettings = featureSettings;
			ConfigureInput(ScriptableRenderPassInput.Depth);
			m_CurrentSettings = new ScreenSpaceShadowsSettings();
			m_RenderTarget.Init("_ScreenSpaceShadowmapTexture");
			return material != null;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			m_RenderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			m_RenderTextureDescriptor.depthBufferBits = 0;
			m_RenderTextureDescriptor.msaaSamples = 1;
			m_RenderTextureDescriptor.graphicsFormat = (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R8_UNorm, FormatUsage.Blend) ? GraphicsFormat.R8_UNorm : GraphicsFormat.B8G8R8A8_UNorm);
			cmd.GetTemporaryRT(m_RenderTarget.id, m_RenderTextureDescriptor, FilterMode.Point);
			RenderTargetIdentifier renderTargetIdentifier = m_RenderTarget.Identifier();
			ConfigureTarget(renderTargetIdentifier);
			ConfigureClear(ClearFlag.None, Color.white);
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			if (cmd == null)
			{
				throw new ArgumentNullException("cmd");
			}
			cmd.ReleaseTemporaryRT(m_RenderTarget.id);
		}
	}

	[SerializeField]
	[HideInInspector]
	private Shader m_Shader;

	[SerializeField]
	private ScreenSpaceShadowsSettings m_Settings = new ScreenSpaceShadowsSettings();

	private Material m_Material;

	private CustomShadowPass shadowPass;

	private RestoreShadowKeywordsPass m_RestoreShadowKeywordsPass;

	private const string k_ShaderName = "Hidden/Universal Render Pipeline/ScreenSpaceShadows";

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		new CustomShadowPass().Init(ref renderingData);
		if (!LoadMaterial())
		{
			Debug.LogErrorFormat("{0}.AddRenderPasses(): Missing material. {1} render pass will not be added. Check for missing reference in the renderer resources.", GetType().Name, shadowPass.profilerTag);
			return;
		}
		bool flag = renderingData.shadowData.supportsMainLightShadows && renderingData.lightData.mainLightIndex != -1;
		if (shadowPass.Setup(m_Settings) & flag)
		{
			renderer.EnqueuePass(shadowPass);
			renderer.EnqueuePass(m_RestoreShadowKeywordsPass);
		}
	}

	public override void Create()
	{
		if (shadowPass == null)
		{
			shadowPass = new CustomShadowPass();
		}
		if (m_RestoreShadowKeywordsPass == null)
		{
			m_RestoreShadowKeywordsPass = new RestoreShadowKeywordsPass();
		}
		LoadMaterial();
		shadowPass.profilerTag = name;
		shadowPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
		m_RestoreShadowKeywordsPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_Material);
	}

	private bool LoadMaterial()
	{
		if (m_Material != null)
		{
			return true;
		}
		if (m_Shader == null)
		{
			m_Shader = Shader.Find("Hidden/Universal Render Pipeline/ScreenSpaceShadows");
			if (m_Shader == null)
			{
				Debug.LogErrorFormat(GetType().Name + ".LoadMaterial(): Shader Hidden/Universal Render Pipeline/ScreenSpaceShadows not found. " + shadowPass.profilerTag + " render pass will not be added. Check for missing reference in the renderer resources.");
				return false;
			}
		}
		m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
		shadowPass.material = m_Material;
		return m_Material != null;
	}
}

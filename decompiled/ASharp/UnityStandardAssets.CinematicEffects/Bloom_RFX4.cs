using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Cinematic/RFX4_Bloom")]
[ImageEffectAllowedInSceneView]
public class Bloom_RFX4 : MonoBehaviour
{
	[Serializable]
	public struct Settings
	{
		[SerializeField]
		[Tooltip("Filters out pixels under this level of brightness.")]
		public float threshold;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Makes transition between under/over-threshold gradual.")]
		public float softKnee;

		[SerializeField]
		[Range(1f, 7f)]
		[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
		public float radius;

		[SerializeField]
		[Tooltip("Blend factor of the result image.")]
		public float intensity;

		[SerializeField]
		[Tooltip("Controls filter quality and buffer resolution.")]
		public bool highQuality;

		[SerializeField]
		[Tooltip("Reduces flashing noise with an additional filter.")]
		public bool antiFlicker;

		[Tooltip("Dirtiness texture to add smudges or dust to the lens.")]
		public Texture dirtTexture;

		[MinAttribute_RFX4(0f)]
		[Tooltip("Amount of lens dirtiness.")]
		public float dirtIntensity;

		public float thresholdGamma
		{
			get
			{
				return Mathf.Max(0f, threshold);
			}
			set
			{
				threshold = value;
			}
		}

		public float thresholdLinear
		{
			get
			{
				return Mathf.GammaToLinearSpace(thresholdGamma);
			}
			set
			{
				threshold = Mathf.LinearToGammaSpace(value);
			}
		}

		public static Settings defaultSettings => new Settings
		{
			threshold = 1.1f,
			softKnee = 0.1f,
			radius = 7f,
			intensity = 0.5f,
			highQuality = true,
			antiFlicker = true,
			dirtTexture = null,
			dirtIntensity = 2.5f
		};
	}

	[SerializeField]
	public Settings settings = Settings.defaultSettings;

	[SerializeField]
	[HideInInspector]
	private Shader m_Shader;

	private Material m_Material;

	private const int kMaxIterations = 16;

	private RenderTexture[] m_blurBuffer1 = new RenderTexture[16];

	private RenderTexture[] m_blurBuffer2 = new RenderTexture[16];

	private int m_Threshold;

	private int m_Curve;

	private int m_PrefilterOffs;

	private int m_SampleScale;

	private int m_Intensity;

	private int m_DirtTex;

	private int m_DirtIntensity;

	private int m_BaseTex;

	public Shader shader
	{
		get
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("Hidden/Image Effects/Cinematic/Bloom_RFX4");
			}
			return m_Shader;
		}
	}

	public Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = ImageEffectHelper_RFX4.CheckShaderAndCreateMaterial(shader);
			}
			return m_Material;
		}
	}

	private void Awake()
	{
		m_Threshold = Shader.PropertyToID("_Threshold");
		m_Curve = Shader.PropertyToID("_Curve");
		m_PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");
		m_SampleScale = Shader.PropertyToID("_SampleScale");
		m_Intensity = Shader.PropertyToID("_Intensity");
		m_DirtTex = Shader.PropertyToID("_DirtTex");
		m_DirtIntensity = Shader.PropertyToID("_DirtIntensity");
		m_BaseTex = Shader.PropertyToID("_BaseTex");
	}

	private void OnEnable()
	{
		if (!ImageEffectHelper_RFX4.IsSupported(shader, needDepth: true, needHdr: false, this))
		{
			enabled = false;
		}
	}

	private void OnDisable()
	{
		if (m_Material != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Material);
		}
		m_Material = null;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		bool isMobilePlatform = Application.isMobilePlatform;
		int num = source.width;
		int num2 = source.height;
		if (!settings.highQuality)
		{
			num /= 2;
			num2 /= 2;
		}
		RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		float num3 = Mathf.Log(num2, 2f) + settings.radius - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = settings.thresholdLinear;
		material.SetFloat(m_Threshold, thresholdLinear);
		float num6 = thresholdLinear * settings.softKnee + 1E-05f;
		Vector3 vector = new Vector3(thresholdLinear - num6, num6 * 2f, 0.25f / num6);
		material.SetVector(m_Curve, vector);
		bool flag = !settings.highQuality && settings.antiFlicker;
		material.SetFloat(m_PrefilterOffs, flag ? (-0.5f) : 0f);
		material.SetFloat(m_SampleScale, 0.5f + num3 - (float)num4);
		material.SetFloat(m_Intensity, Mathf.Max(0f, settings.intensity));
		bool flag2 = false;
		if (settings.dirtTexture != null)
		{
			material.SetTexture(m_DirtTex, settings.dirtTexture);
			material.SetFloat(m_DirtIntensity, settings.dirtIntensity);
			flag2 = true;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format);
		Graphics.Blit(source, temporary, material, settings.antiFlicker ? 1 : 0);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			m_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			RenderTexture source2 = renderTexture;
			RenderTexture dest = m_blurBuffer1[i];
			Material mat = material;
			int pass;
			if (i != 0)
			{
				pass = 4;
			}
			else
			{
				pass = (settings.antiFlicker ? 3 : 2);
			}
			Graphics.Blit(source2, dest, mat, pass);
			renderTexture = m_blurBuffer1[i];
		}
		for (int num7 = num5 - 2; num7 >= 0; num7--)
		{
			RenderTexture renderTexture2 = m_blurBuffer1[num7];
			material.SetTexture(m_BaseTex, renderTexture2);
			m_blurBuffer2[num7] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer2[num7], material, settings.highQuality ? 6 : 5);
			renderTexture = m_blurBuffer2[num7];
		}
		int num8 = (flag2 ? 9 : 7);
		num8 += (settings.highQuality ? 1 : 0);
		material.SetTexture(m_BaseTex, source);
		Graphics.Blit(renderTexture, destination, material, num8);
		for (int j = 0; j < 16; j++)
		{
			if (m_blurBuffer1[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer1[j]);
			}
			if (m_blurBuffer2[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer2[j]);
			}
			m_blurBuffer1[j] = null;
			m_blurBuffer2[j] = null;
		}
		RenderTexture.ReleaseTemporary(temporary);
	}
}

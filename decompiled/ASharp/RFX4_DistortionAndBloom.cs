using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("KriptoFX/RFX1_BloomAndDistortion")]
public class RFX4_DistortionAndBloom : MonoBehaviour
{
	[Range(0.05f, 1f)]
	[Tooltip("Camera render texture resolution")]
	public float RenderTextureResolutoinFactor = 0.25f;

	public bool UseBloom = true;

	[Range(0.1f, 3f)]
	[Tooltip("Filters out pixels under this level of brightness.")]
	public float Threshold = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Makes transition between under/over-threshold gradual.")]
	public float SoftKnee;

	[Range(1f, 7f)]
	[Tooltip("Changes extent of veiling effects in A screen resolution-independent fashion.")]
	public float Radius = 7f;

	[Tooltip("Blend factor of the result image.")]
	public float Intensity = 0.5f;

	[Tooltip("Controls filter quality and buffer resolution.")]
	public bool HighQuality;

	[Tooltip("Reduces flashing noise with an additional filter.")]
	public bool AntiFlicker;

	private const string shaderName = "Hidden/KriptoFX/PostEffects/RFX4_Bloom";

	private const string shaderAdditiveName = "Hidden/KriptoFX/PostEffects/RFX4_BloomAdditive";

	private const string cameraName = "MobileCamera(Distort_Bloom_Depth)";

	private RenderTexture source;

	private RenderTexture depth;

	private RenderTexture destination;

	private int previuosFrameWidth;

	private int previuosFrameHeight;

	private float previousScale;

	private Camera addCamera;

	private GameObject tempGO;

	private bool HDRSupported;

	private Material m_Material;

	private Material m_MaterialAdditive;

	private const int kMaxIterations = 16;

	private readonly RenderTexture[] m_blurBuffer1 = new RenderTexture[16];

	private readonly RenderTexture[] m_blurBuffer2 = new RenderTexture[16];

	public Material mat
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = CheckShaderAndCreateMaterial(Shader.Find("Hidden/KriptoFX/PostEffects/RFX4_Bloom"));
			}
			return m_Material;
		}
	}

	public Material matAdditive
	{
		get
		{
			if (m_MaterialAdditive == null)
			{
				m_MaterialAdditive = CheckShaderAndCreateMaterial(Shader.Find("Hidden/KriptoFX/PostEffects/RFX4_BloomAdditive"));
				m_MaterialAdditive.renderQueue = 3900;
			}
			return m_MaterialAdditive;
		}
	}

	public static Material CheckShaderAndCreateMaterial(Shader s)
	{
		if (s == null || !s.isSupported)
		{
			return null;
		}
		return new Material(s)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	private void OnDisable()
	{
		if (m_Material != null)
		{
			Object.DestroyImmediate(m_Material);
		}
		m_Material = null;
		if (m_MaterialAdditive != null)
		{
			Object.DestroyImmediate(m_MaterialAdditive);
		}
		m_MaterialAdditive = null;
		if (tempGO != null)
		{
			Object.DestroyImmediate(tempGO);
		}
		Shader.DisableKeyword("DISTORT_OFF");
		Shader.DisableKeyword("_MOBILEDEPTH_ON");
	}

	private void Start()
	{
		InitializeRenderTarget();
	}

	private void LateUpdate()
	{
		if (previuosFrameWidth != Screen.width || previuosFrameHeight != Screen.height || Mathf.Abs(previousScale - RenderTextureResolutoinFactor) > 0.01f)
		{
			InitializeRenderTarget();
			previuosFrameWidth = Screen.width;
			previuosFrameHeight = Screen.height;
			previousScale = RenderTextureResolutoinFactor;
		}
		Shader.EnableKeyword("DISTORT_OFF");
		Shader.EnableKeyword("_MOBILEDEPTH_ON");
		GrabImage();
		if (UseBloom && HDRSupported)
		{
			UpdateBloom();
		}
		Shader.SetGlobalTexture("_GrabTexture", source);
		Shader.SetGlobalTexture("_GrabTextureMobile", source);
		Shader.SetGlobalTexture("_CameraDepthTexture", depth);
		Shader.SetGlobalFloat("_GrabTextureScale", RenderTextureResolutoinFactor);
		Shader.SetGlobalFloat("_GrabTextureMobileScale", RenderTextureResolutoinFactor);
		Shader.DisableKeyword("DISTORT_OFF");
	}

	private void OnPostRender()
	{
		Graphics.Blit(destination, null, matAdditive);
	}

	private void InitializeRenderTarget()
	{
		int num = (int)((float)Screen.width * RenderTextureResolutoinFactor);
		int num2 = (int)((float)Screen.height * RenderTextureResolutoinFactor);
		RenderTextureFormat format = RenderTextureFormat.RGB111110Float;
		if (SystemInfo.SupportsRenderTextureFormat(format))
		{
			source = new RenderTexture(num, num2, 0, format);
			depth = new RenderTexture(num, num2, 8, RenderTextureFormat.Depth);
			HDRSupported = true;
			if (UseBloom)
			{
				destination = new RenderTexture(((double)RenderTextureResolutoinFactor > 0.99) ? num : (num / 2), ((double)RenderTextureResolutoinFactor > 0.99) ? num2 : (num2 / 2), 0, format);
			}
		}
		else
		{
			HDRSupported = false;
			source = new RenderTexture(num, num2, 0, RenderTextureFormat.RGB565);
			depth = new RenderTexture(num, num2, 8, RenderTextureFormat.Depth);
		}
	}

	private void UpdateBloom()
	{
		bool isMobilePlatform = Application.isMobilePlatform;
		if (source == null)
		{
			return;
		}
		int num = source.width;
		int num2 = source.height;
		if (!HighQuality)
		{
			num /= 2;
			num2 /= 2;
		}
		RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		float num3 = Mathf.Log(num2, 2f) + Radius - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float num6 = Mathf.GammaToLinearSpace(Threshold);
		mat.SetFloat("_Threshold", num6);
		float num7 = num6 * SoftKnee + 1E-05f;
		Vector3 vector = new Vector3(num6 - num7, num7 * 2f, 0.25f / num7);
		mat.SetVector("_Curve", vector);
		bool flag = !HighQuality && AntiFlicker;
		mat.SetFloat("_PrefilterOffs", flag ? (-0.5f) : 0f);
		mat.SetFloat("_SampleScale", 0.5f + num3 - (float)num4);
		mat.SetFloat("_Intensity", Mathf.Max(0f, Intensity));
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format);
		Graphics.Blit(source, temporary, mat, AntiFlicker ? 1 : 0);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			m_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			RenderTexture renderTexture2 = renderTexture;
			RenderTexture dest = m_blurBuffer1[i];
			Material material = mat;
			int pass;
			if (i != 0)
			{
				pass = 4;
			}
			else
			{
				pass = (AntiFlicker ? 3 : 2);
			}
			Graphics.Blit(renderTexture2, dest, material, pass);
			renderTexture = m_blurBuffer1[i];
		}
		for (int num8 = num5 - 2; num8 >= 0; num8--)
		{
			RenderTexture renderTexture3 = m_blurBuffer1[num8];
			mat.SetTexture("_BaseTex", renderTexture3);
			m_blurBuffer2[num8] = RenderTexture.GetTemporary(renderTexture3.width, renderTexture3.height, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer2[num8], mat, HighQuality ? 6 : 5);
			renderTexture = m_blurBuffer2[num8];
		}
		destination.DiscardContents();
		Graphics.Blit(renderTexture, destination, mat, HighQuality ? 8 : 7);
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

	private void GrabImage()
	{
		Camera camera = Camera.current;
		if (camera == null)
		{
			camera = Camera.main;
		}
		if (tempGO == null)
		{
			tempGO = new GameObject();
			tempGO.hideFlags = HideFlags.HideAndDontSave;
			tempGO.name = "MobileCamera(Distort_Bloom_Depth)";
			addCamera = tempGO.AddComponent<Camera>();
			addCamera.enabled = false;
		}
		else
		{
			addCamera = tempGO.GetComponent<Camera>();
		}
		addCamera.CopyFrom(camera);
		addCamera.SetTargetBuffers(source.colorBuffer, depth.depthBuffer);
		addCamera.depth--;
		addCamera.Render();
	}
}

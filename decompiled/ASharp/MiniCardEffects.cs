using System.Collections;
using Chronos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniCardEffects : MonoBehaviour
{
	public enum FXTask
	{
		BurnCard,
		HighlightBurn,
		DiscardMode,
		LostMode,
		HighlightGhost
	}

	public Image[] imgComp;

	public TextMeshProUGUI[] txtComp;

	private Color[] txtColourStore;

	private bool[] txtGradientStore;

	private Color[] imgColourStore;

	private int txtCount;

	private int imgCount;

	private bool[] txtAffected;

	private bool[] imgAffected;

	public Image fgFx;

	private string fxAnim = "_FXAnim";

	public FXTask task;

	private bool isBurning;

	private bool isGhosting;

	private float mc_Burn;

	private Color mc_Burn_ColourTint;

	private float mc_Flow;

	private float mc_Flow_Offset;

	private float mc_Flow_Speed;

	private float mc_AnimNoise_Mask_tile;

	private float mc_Dissolve;

	private float mc_Dissolve_VerticalGradient;

	private float mc_GreyOut;

	private Color fx_Overlay_color;

	private float fx_Overlay_anim;

	private Texture2D fx_Overlay_texture;

	private float fx_Overlay_FlowSpeed;

	private float fx_Overlay_OffsetStrength;

	private float fx_Overlay_ThinHighlights;

	private float fx_Overlay_ThinHighlight_Min;

	private float fx_Overlay_ThinHighlight_Max;

	private float fx_Overlay_Flow_NoiseTiling;

	private float fx_Overlay_Flow_NoiseSpeed;

	private float fx_Overlay_Glow;

	public Texture2D overlayFrameGhost;

	public Texture2D overlayFrameBurn;

	private void Awake()
	{
		if (transform.parent.gameObject.GetComponent<Image>() != null)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		txtColourStore = new Color[txtComp.Length];
		txtGradientStore = new bool[txtComp.Length];
		txtCount = txtComp.Length;
		for (int i = 0; i < txtCount; i++)
		{
			if (txtComp[i] != null)
			{
				txtColourStore[i] = txtComp[i].color;
				txtGradientStore[i] = txtComp[i].enableVertexGradient;
			}
		}
		imgColourStore = new Color[imgComp.Length];
		imgCount = imgComp.Length;
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgColourStore[j] = imgComp[j].color;
			}
		}
		RectTransform rectTransform = transform as RectTransform;
		Image[] array = imgComp;
		foreach (Image image in array)
		{
			if (image != null)
			{
				image.material = new Material(image.material);
			}
		}
		fgFx.material = new Material(fgFx.material);
		for (int l = 0; l < imgCount; l++)
		{
			if (imgComp[l] != null)
			{
				imgComp[l].material.SetVector("_PosAndBounds", new Vector4(rectTransform.position.x, rectTransform.position.y, rectTransform.rect.width, rectTransform.rect.height));
			}
		}
		RestoreCard();
	}

	public void ToggleEffect(bool active, FXTask effect)
	{
		RestoreCard();
		txtAffected = new bool[txtCount];
		imgAffected = new bool[imgCount];
		for (int i = 0; i < txtCount; i++)
		{
			txtAffected[i] = false;
		}
		for (int j = 0; j < imgCount; j++)
		{
			imgAffected[j] = false;
		}
		ToggleAdditiveEffect(active, effect);
	}

	public void ToggleAdditiveEffect(bool active, FXTask effect)
	{
		if (active)
		{
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: true);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: true);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: true);
				break;
			}
		}
		else
		{
			switch (effect)
			{
			case FXTask.DiscardMode:
				GhostOutOn(ghostAnim: false);
				break;
			case FXTask.LostMode:
				BurnCard(burnAnim: false);
				break;
			case FXTask.BurnCard:
				BurnCard(burnAnim: false);
				break;
			}
		}
	}

	private void BurnCard(bool burnAnim)
	{
		if (Choreographer.s_Choreographer == null)
		{
			BurnCard();
		}
		else
		{
			Choreographer.s_Choreographer.StartCoroutine(BurnCardTimeline(burnAnim));
		}
	}

	private void HighlightBurnOn()
	{
	}

	private void HighlightBurnOff()
	{
	}

	private void GhostOutOn(bool ghostAnim)
	{
		if (Choreographer.s_Choreographer == null)
		{
			GhostCard();
		}
		else
		{
			Choreographer.s_Choreographer.StartCoroutine(GhostOutOnTimeline(ghostAnim));
		}
	}

	public void RestoreCard()
	{
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_GreyOut", 0f);
				imgComp[i].material.SetFloat("_Flow", 0f);
				imgComp[i].material.SetFloat("_Dissolve", 0f);
				imgComp[i].material.SetFloat("_Burn", 0f);
			}
		}
		if (fgFx != null)
		{
			fgFx.material.SetFloat(fxAnim, 0f);
		}
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgComp[j].GetComponent<Image>().color = imgColourStore[j];
			}
		}
		for (int k = 0; k < txtCount; k++)
		{
			if (txtAffected != null && txtAffected[k] && txtComp[k] != null)
			{
				txtComp[k].color = txtColourStore[k];
			}
		}
	}

	public IEnumerator BurnCardTimeline(bool burnAnim)
	{
		fgFx.gameObject.SetActive(value: true);
		float burnTime = 2f;
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		mc_Burn = 0.691f;
		mc_Burn_ColourTint = new Color(94f / 255f, 37f / 255f, 19f / 255f, 0.601f);
		mc_Flow = 1f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.4f;
		mc_AnimNoise_Mask_tile = 40f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Overlay_color = new Color(1f, 0.3f, 0f, 0.8f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameBurn;
		fx_Overlay_FlowSpeed = 0.5f;
		fx_Overlay_OffsetStrength = 0.07f;
		fx_Overlay_ThinHighlights = 0.463f;
		fx_Overlay_ThinHighlight_Min = 0f;
		fx_Overlay_ThinHighlight_Max = 0.195f;
		fx_Overlay_Flow_NoiseTiling = 6f;
		fx_Overlay_Glow = 3f;
		float value = -0.522f;
		float value2 = 0.709f;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_Burn", mc_Burn);
				imgComp[i].material.SetColor("_Burn_ColourTint", mc_Burn_ColourTint);
				imgComp[i].material.SetFloat("_Flow_Offset", mc_Flow_Offset);
				imgComp[i].material.SetFloat("_Flow_Speed", mc_Flow_Speed);
				imgComp[i].material.SetTextureScale("_AnimNoise_Mask", new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
				imgComp[i].material.SetFloat("_Dissolve_VerticalGradient", mc_Dissolve_VerticalGradient);
			}
		}
		fgFx.material.SetColor("_TintColor", fx_Overlay_color);
		fgFx.material.SetTexture("_ParticleTexture", fx_Overlay_texture);
		fgFx.material.SetFloat("_FlowSpeed", fx_Overlay_FlowSpeed);
		fgFx.material.SetFloat("_OffsetStrength", fx_Overlay_OffsetStrength);
		fgFx.material.SetFloat("_ThinHighlights", fx_Overlay_ThinHighlights);
		fgFx.material.SetFloat("_ThinHighlight_Min", fx_Overlay_ThinHighlight_Min);
		fgFx.material.SetFloat("_ThinHighlight_Max", fx_Overlay_ThinHighlight_Max);
		fgFx.material.SetFloat("_Flow_NoiseTiling", fx_Overlay_Flow_NoiseTiling);
		fgFx.material.SetFloat("_Glow", fx_Overlay_Glow);
		fgFx.material.SetFloat("_FXAnim_Min", value);
		fgFx.material.SetFloat("_FXAnim_Max", value2);
		if (burnAnim)
		{
			while (Timekeeper.instance.m_GlobalClock.time - startTime < burnTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / burnTime;
				for (int j = 0; j < imgCount; j++)
				{
					if (imgComp[j] != null)
					{
						imgComp[j].material.SetFloat("_GreyOut", Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat("_Flow", Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat("_Dissolve", Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = new Color(0.5f, 0.5f, 0.5f, 1f);
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			BurnCard();
		}
	}

	private void BurnCard()
	{
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_GreyOut", 1f);
				imgComp[i].material.SetFloat("_Flow", 1f);
				imgComp[i].material.SetFloat("_Dissolve", mc_Dissolve);
			}
		}
		fgFx.material.SetFloat(fxAnim, 0.5f);
		for (int j = 0; j < txtCount; j++)
		{
			if (txtAffected[j] && txtComp[j] != null)
			{
				txtComp[j].color = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
		}
	}

	public IEnumerator GhostOutOnTimeline(bool ghostAnim)
	{
		isGhosting = true;
		if (fgFx != null)
		{
			fgFx.gameObject.SetActive(value: true);
		}
		float animTime = 2f;
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		Color greyOutCol = new Color(1f, 1f, 1f, 1f);
		mc_Burn = 0.7f;
		mc_Burn_ColourTint = new Color(62f / 255f, 72f / 255f, 87f / 255f, 0.5f);
		mc_Flow = 1f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.2f;
		mc_AnimNoise_Mask_tile = 10f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Overlay_color = new Color(0.8f, 0.8f, 1f, 0.25f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameGhost;
		fx_Overlay_FlowSpeed = 0.2f;
		fx_Overlay_OffsetStrength = 0.07f;
		fx_Overlay_ThinHighlights = 0.125f;
		fx_Overlay_ThinHighlight_Min = 0.77f;
		fx_Overlay_ThinHighlight_Max = 0.9f;
		fx_Overlay_Flow_NoiseTiling = 2f;
		fx_Overlay_Glow = 0f;
		float value = -0.22f;
		float value2 = 1.31f;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_Burn", mc_Burn);
				imgComp[i].material.SetColor("_Burn_ColourTint", mc_Burn_ColourTint);
				imgComp[i].material.SetFloat("_Flow_Offset", mc_Flow_Offset);
				imgComp[i].material.SetFloat("_Flow_Speed", mc_Flow_Speed);
				imgComp[i].material.SetTextureScale("_AnimNoise_Mask", new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
				imgComp[i].material.SetFloat("_Dissolve_VerticalGradient", mc_Dissolve_VerticalGradient);
			}
		}
		fgFx.material.SetColor("_TintColor", fx_Overlay_color);
		fgFx.material.SetTexture("_ParticleTexture", fx_Overlay_texture);
		fgFx.material.SetFloat("_FlowSpeed", fx_Overlay_FlowSpeed);
		fgFx.material.SetFloat("_OffsetStrength", fx_Overlay_OffsetStrength);
		fgFx.material.SetFloat("_ThinHighlights", fx_Overlay_ThinHighlights);
		fgFx.material.SetFloat("_ThinHighlight_Min", fx_Overlay_ThinHighlight_Min);
		fgFx.material.SetFloat("_ThinHighlight_Max", fx_Overlay_ThinHighlight_Max);
		fgFx.material.SetFloat("_Flow_NoiseTiling", fx_Overlay_Flow_NoiseTiling);
		fgFx.material.SetFloat("_Glow", fx_Overlay_Glow);
		fgFx.material.SetFloat("_FXAnim_Min", value);
		fgFx.material.SetFloat("_FXAnim_Max", value2);
		if (ghostAnim)
		{
			while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
			{
				dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
				for (int j = 0; j < imgCount; j++)
				{
					if (imgComp[j] != null)
					{
						imgComp[j].material.SetFloat("_GreyOut", Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat("_Flow", Mathf.Clamp01(dTime));
						imgComp[j].material.SetFloat("_Dissolve", Mathf.Lerp(0f, mc_Dissolve, Mathf.Clamp01(dTime)));
					}
				}
				if (fgFx != null)
				{
					fgFx.material.SetFloat(fxAnim, Mathf.Clamp(dTime * 4f, 0f, 1f) / 2f);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = Color.Lerp(txtColourStore[k], greyOutCol, dTime);
						if (dTime > 0.5f)
						{
							txtComp[k].enableVertexGradient = false;
						}
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			GhostCard();
		}
		yield return new WaitForEndOfFrame();
	}

	private void GhostCard()
	{
		Color b = new Color(1f, 1f, 1f, 1f);
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_GreyOut", 1f);
				imgComp[i].material.SetFloat("_Flow", 1f);
				imgComp[i].material.SetFloat("_Dissolve", mc_Dissolve);
			}
		}
		fgFx.material.SetFloat(fxAnim, 0.5f);
		for (int j = 0; j < txtCount; j++)
		{
			if (txtAffected[j] && txtComp[j] != null)
			{
				txtComp[j].color = Color.Lerp(txtColourStore[j], b, 1f);
				txtComp[j].enableVertexGradient = false;
			}
		}
	}
}

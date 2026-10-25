using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEffects : MonoBehaviour
{
	public enum FXTask
	{
		BurnCard,
		DiscardMode,
		LostMode
	}

	private Image[] imgComp;

	private TextMeshProUGUI[] txtComp;

	private Color[] txtColourStore;

	private bool[] txtGradientStore;

	private Color[] imgColourStore;

	private int txtCount;

	private int imgCount;

	private bool[] txtAffected;

	private bool[] imgAffected;

	[SerializeField]
	private Image fgFx;

	[SerializeField]
	private Image header;

	[SerializeField]
	private Image topAction;

	[SerializeField]
	private Image bottomAction;

	[SerializeField]
	private Image topDefAction;

	[SerializeField]
	private Image bottomDefAction;

	[SerializeField]
	private Image topDefActionIcon;

	[SerializeField]
	private Image bottomDefActionIcon;

	[SerializeField]
	private Image defActions;

	private Image defActionsText;

	[SerializeField]
	private TextMeshProUGUI headerTxt;

	[SerializeField]
	private TextMeshProUGUI topActionTxt;

	[SerializeField]
	private TextMeshProUGUI bottomActionTxt;

	[SerializeField]
	private TextMeshProUGUI topDefActionTxt;

	[SerializeField]
	private TextMeshProUGUI bottomDefActionTxt;

	[SerializeField]
	private TextMeshProUGUI initiativeTxt;

	private string fxAnim = "_FXAnim";

	public bool affectHeader = true;

	public bool affectTop = true;

	public bool affectBottom = true;

	public bool affectTopDefault = true;

	public bool affectBottomDefault = true;

	public FXTask task;

	private bool isBurning;

	private bool isGhosting;

	public Vector2 canvasPosition;

	public Vector2 cardBounds;

	private Image parentImage;

	private float mc_Burn;

	private Color mc_Burn_ColourTint;

	private float mc_Flow;

	private float mc_Flow_Offset;

	private float mc_Flow_Speed;

	private float mc_AnimNoise_Mask_tile;

	private float mc_Dissolve;

	private float mc_Dissolve_VerticalGradient;

	private float mc_GreyOut;

	private ParticleSystem fx_Smoke;

	private Color fx_Smoke_color;

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

	private Color burntTextColor = new Color32(143, 58, 44, byte.MaxValue);

	private Coroutine coroutine;

	private HashSet<FXTask> toggledEffects = new HashSet<FXTask>();

	private bool initialized;

	private bool _imagesDirty;

	private void Awake()
	{
		if (isActiveAndEnabled)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		txtComp = new TextMeshProUGUI[6];
		txtComp[0] = headerTxt;
		txtComp[1] = topActionTxt;
		txtComp[2] = bottomActionTxt;
		txtComp[3] = topDefActionTxt;
		txtComp[4] = bottomDefActionTxt;
		txtComp[5] = initiativeTxt;
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
		imgComp = new Image[10];
		imgComp[1] = defActionsText;
		imgComp[2] = header;
		imgComp[3] = topAction;
		imgComp[4] = bottomAction;
		imgComp[5] = topDefAction;
		imgComp[6] = bottomDefAction;
		imgComp[7] = topDefActionIcon;
		imgComp[8] = bottomDefActionIcon;
		imgComp[9] = defActions;
		imgColourStore = new Color[imgComp.Length];
		imgCount = imgComp.Length;
		for (int j = 0; j < imgCount; j++)
		{
			if (imgComp[j] != null)
			{
				imgColourStore[j] = imgComp[j].color;
			}
		}
		RectTransform rectTransform = transform.parent.transform as RectTransform;
		parentImage = rectTransform.gameObject.GetComponent<Image>();
		canvasPosition = rectTransform.anchoredPosition;
		if (header != null)
		{
			cardBounds = new Vector2(header.rectTransform.rect.width, header.rectTransform.rect.height);
		}
		Image[] array = imgComp;
		foreach (Image image in array)
		{
			if (image != null)
			{
				image.material = new Material(image.material);
			}
		}
		if (fgFx != null)
		{
			fgFx.material = new Material(fgFx.material);
		}
		for (int l = 0; l < imgCount; l++)
		{
			if (imgComp[l] != null)
			{
				imgComp[l].material.SetVector("_PosAndBounds", new Vector4(canvasPosition.x, canvasPosition.y, cardBounds.x, cardBounds.y));
			}
		}
		RestoreCard();
	}

	private void LateUpdate()
	{
		if (!_imagesDirty)
		{
			return;
		}
		_imagesDirty = false;
		if (imgComp == null)
		{
			return;
		}
		Image[] array = imgComp;
		foreach (Image image in array)
		{
			if (!(image == null))
			{
				image.enabled = false;
				image.enabled = true;
			}
		}
	}

	public bool HasEffect(FXTask effect)
	{
		return toggledEffects.Contains(effect);
	}

	public void ToggleEffect(bool active, FXTask effect)
	{
		Initialize();
		RestoreCard();
		if (txtCount < 0 || imgCount < 0)
		{
			return;
		}
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
		if (affectHeader)
		{
			if (txtAffected.Length != 0)
			{
				txtAffected[0] = true;
				if (txtAffected.Length > 5)
				{
					txtAffected[5] = true;
				}
			}
			if (imgAffected.Length > 2)
			{
				imgAffected[2] = true;
			}
		}
		if (affectTop)
		{
			if (txtAffected.Length > 1)
			{
				txtAffected[1] = true;
			}
			if (imgAffected.Length > 3)
			{
				imgAffected[3] = true;
			}
		}
		if (affectBottom)
		{
			if (txtAffected.Length > 2)
			{
				txtAffected[2] = true;
			}
			if (imgAffected.Length > 4)
			{
				imgAffected[4] = true;
			}
		}
		if (affectTopDefault)
		{
			if (txtAffected.Length > 3)
			{
				txtAffected[3] = true;
				if (txtAffected.Length > 5)
				{
					imgAffected[5] = true;
				}
			}
			if (imgAffected.Length > 7)
			{
				imgAffected[7] = true;
				if (imgAffected.Length > 9)
				{
					imgAffected[9] = true;
				}
			}
		}
		if (affectBottomDefault)
		{
			if (txtAffected.Length > 4)
			{
				txtAffected[4] = true;
			}
			if (imgAffected.Length > 6)
			{
				imgAffected[6] = true;
				if (imgAffected.Length > 8)
				{
					imgAffected[8] = true;
				}
				if (imgAffected.Length > 9)
				{
					imgAffected[9] = true;
				}
			}
		}
		ToggleAdditiveEffect(active, effect);
	}

	private void ToggleAdditiveEffect(bool active, FXTask effect)
	{
		if (coroutine != null && CoroutineHelper.instance != null)
		{
			CoroutineHelper.instance.StopCoroutine(coroutine);
		}
		coroutine = null;
		if (active)
		{
			if (!toggledEffects.Contains(effect))
			{
				toggledEffects.Add(effect);
			}
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
			toggledEffects.Remove(effect);
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
		coroutine = CoroutineHelper.instance.StartCoroutine(BurnCardTimeline(burnAnim));
	}

	private void HighlightBurnOn()
	{
	}

	private void HighlightBurnOff()
	{
	}

	private void GhostOutOn(bool ghostAnim)
	{
		coroutine = CoroutineHelper.instance.StartCoroutine(GhostOutOnTimeline(ghostAnim));
	}

	public void RestoreCard()
	{
		toggledEffects?.Clear();
		if (coroutine != null && CoroutineHelper.instance != null)
		{
			CoroutineHelper.instance.StopCoroutine(coroutine);
		}
		coroutine = null;
		if (imgComp == null || txtComp == null)
		{
			return;
		}
		int num = Math.Max(0, Math.Min(imgComp.Length, imgCount));
		int num2 = Math.Max(0, Math.Min(txtComp.Length, txtCount));
		if (imgComp.Length != 0)
		{
			for (int i = 0; i < num; i++)
			{
				if (imgComp[i] != null)
				{
					imgComp[i].material.SetFloat("_GreyOut", 0f);
					imgComp[i].material.SetFloat("_Flow", 0f);
					imgComp[i].material.SetFloat("_Dissolve", 0f);
					imgComp[i].material.SetFloat("_Burn", 0f);
					_imagesDirty = true;
				}
			}
		}
		if (fx_Smoke != null)
		{
			fx_Smoke.gameObject.SetActive(value: false);
		}
		if (fgFx != null)
		{
			fgFx.material.SetFloat(fxAnim, 0f);
		}
		if (imgComp.Length != 0)
		{
			for (int j = 0; j < num; j++)
			{
				if (imgComp[j] != null)
				{
					imgComp[j].GetComponent<Image>().color = imgColourStore[j];
				}
			}
		}
		if (txtComp.Length == 0)
		{
			return;
		}
		for (int k = 0; k < num2; k++)
		{
			if (txtAffected != null && k < txtAffected.Length && txtAffected[k] && txtComp[k] != null)
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
		float startTime = GetTime();
		mc_Burn = 0.691f;
		mc_Burn_ColourTint = new Color(94f / 255f, 37f / 255f, 19f / 255f, 0.601f);
		mc_Flow = 0.3f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.4f;
		mc_AnimNoise_Mask_tile = 40f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Smoke_color = new Color(38f / 255f, 38f / 255f, 38f / 255f, 0.5f);
		fx_Overlay_color = new Color(1f, 0.3f, 0f, 0.8f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameBurn;
		fx_Overlay_FlowSpeed = 0.5f;
		fx_Overlay_OffsetStrength = 0.2f;
		fx_Overlay_ThinHighlights = 0.463f;
		fx_Overlay_ThinHighlight_Min = 0f;
		fx_Overlay_ThinHighlight_Max = 0.195f;
		fx_Overlay_Flow_NoiseTiling = 6f;
		fx_Overlay_Glow = 3f;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_Burn", mc_Burn);
				imgComp[i].material.SetColor("_Burn_ColourTint", mc_Burn_ColourTint);
				imgComp[i].material.SetFloat("_Flow_Offset", mc_Flow_Offset);
				imgComp[i].material.SetFloat("_Flow_Speed", mc_Flow_Speed);
				if (imgComp[i].material.HasProperty("_AnimNoise_Mask"))
				{
					imgComp[i].material.SetTextureScale("_AnimNoise_Mask", new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
				}
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
		if ((bool)fx_Smoke)
		{
			ParticleSystem.MainModule main = fx_Smoke.main;
			main.startColor = fx_Smoke_color;
		}
		if (burnAnim)
		{
			while (GetTime() - startTime < burnTime)
			{
				dTime += GetDeltaTime() / burnTime;
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
				if (fx_Smoke != null)
				{
					fx_Smoke.gameObject.SetActive(value: true);
				}
				for (int k = 0; k < txtCount; k++)
				{
					if (txtAffected[k] && txtComp[k] != null)
					{
						txtComp[k].color = ((txtComp[k] == initiativeTxt || txtComp[k] == headerTxt) ? burntTextColor : new Color(0.5f, 0.5f, 0.5f, 1f));
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material.SetFloat("_GreyOut", 1f);
					imgComp[l].material.SetFloat("_Flow", 1f);
					imgComp[l].material.SetFloat("_Dissolve", mc_Dissolve);
					_imagesDirty = true;
				}
			}
			fgFx.material.SetFloat(fxAnim, 0.5f);
			if (fx_Smoke != null)
			{
				fx_Smoke.gameObject.SetActive(value: true);
			}
			for (int m = 0; m < txtCount; m++)
			{
				if (txtAffected[m] && txtComp[m] != null)
				{
					txtComp[m].color = new Color(0.5f, 0.5f, 0.5f, 1f);
				}
			}
		}
		coroutine = null;
	}

	private float GetTime()
	{
		if (!(Timekeeper.instance != null))
		{
			return Time.time;
		}
		return Timekeeper.instance.m_GlobalClock.time;
	}

	private float GetDeltaTime()
	{
		if (!(Timekeeper.instance != null))
		{
			return Time.deltaTime;
		}
		return Timekeeper.instance.m_GlobalClock.deltaTime;
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
		float startTime = GetTime();
		Color greyOutCol = new Color(1f, 1f, 1f, 1f);
		mc_Burn = 0.7f;
		mc_Burn_ColourTint = new Color(62f / 255f, 72f / 255f, 87f / 255f, 0.5f);
		mc_Flow = 0.3f;
		mc_Flow_Offset = 0.03f;
		mc_Flow_Speed = 0.2f;
		mc_AnimNoise_Mask_tile = 10f;
		mc_Dissolve = 0.646f;
		mc_Dissolve_VerticalGradient = 0.2f;
		mc_GreyOut = 1f;
		fx_Smoke_color = new Color(77f / 255f, 77f / 255f, 99f / 255f, 0.4f);
		fx_Overlay_color = new Color(0.8f, 0.8f, 1f, 0.6f);
		fx_Overlay_anim = 0.5f;
		fx_Overlay_texture = overlayFrameGhost;
		fx_Overlay_FlowSpeed = 0.2f;
		fx_Overlay_OffsetStrength = 0.8f;
		fx_Overlay_ThinHighlights = 0.125f;
		fx_Overlay_ThinHighlight_Min = 0.77f;
		fx_Overlay_ThinHighlight_Max = 0.9f;
		fx_Overlay_Flow_NoiseTiling = 2f;
		fx_Overlay_Glow = 0f;
		for (int i = 0; i < imgCount; i++)
		{
			if (imgComp[i] != null)
			{
				imgComp[i].material.SetFloat("_Burn", mc_Burn);
				imgComp[i].material.SetColor("_Burn_ColourTint", mc_Burn_ColourTint);
				imgComp[i].material.SetFloat("_Flow_Offset", mc_Flow_Offset);
				imgComp[i].material.SetFloat("_Flow_Speed", mc_Flow_Speed);
				if (imgComp[i].material.HasProperty("_AnimNoise_Mask"))
				{
					imgComp[i].material.SetTextureScale("_AnimNoise_Mask", new Vector2(mc_AnimNoise_Mask_tile, mc_AnimNoise_Mask_tile));
				}
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
		if ((bool)fx_Smoke)
		{
			ParticleSystem.MainModule main = fx_Smoke.main;
			main.startColor = fx_Smoke_color;
		}
		if (ghostAnim)
		{
			if (fgFx != null && (bool)fx_Smoke)
			{
				fx_Smoke.gameObject.SetActive(value: true);
			}
			while (GetTime() - startTime < animTime)
			{
				dTime += GetDeltaTime() / animTime;
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
			for (int l = 0; l < imgCount; l++)
			{
				if (imgComp[l] != null)
				{
					imgComp[l].material.SetFloat("_GreyOut", 1f);
					imgComp[l].material.SetFloat("_Flow", 1f);
					imgComp[l].material.SetFloat("_Dissolve", mc_Dissolve);
					_imagesDirty = true;
				}
			}
			Log.Info("right");
			if ((bool)fx_Smoke)
			{
				fx_Smoke.gameObject.SetActive(value: true);
			}
			fgFx.material.SetFloat(fxAnim, 0.5f);
			for (int m = 0; m < txtCount; m++)
			{
				if (txtAffected[m] && txtComp[m] != null)
				{
					txtComp[m].color = Color.Lerp(txtColourStore[m], greyOutCol, 1f);
					txtComp[m].enableVertexGradient = false;
				}
			}
		}
		yield return new WaitForEndOfFrame();
		coroutine = null;
	}
}

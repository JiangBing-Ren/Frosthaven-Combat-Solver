using System.Collections.Generic;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Slider Extended", 58)]
public class UISliderExtended : Slider
{
	public enum TextEffectType
	{
		None,
		Shadow,
		Outline
	}

	public enum TransitionType
	{
		None,
		ColorTint
	}

	public enum TransitionTarget
	{
		Image,
		Text
	}

	[SerializeField]
	private List<string> m_Options = new List<string>();

	[SerializeField]
	private List<GameObject> m_OptionGameObjects = new List<GameObject>();

	[SerializeField]
	private GameObject m_OptionsContGameObject;

	[SerializeField]
	private RectTransform m_OptionsContRect;

	[SerializeField]
	private GridLayoutGroup m_OptionsContGrid;

	private GameObject m_CurrentOptionGameObject;

	[SerializeField]
	private RectOffset m_OptionsPadding = new RectOffset();

	[SerializeField]
	private Sprite m_OptionSprite;

	[SerializeField]
	private Font m_OptionTextFont;

	[SerializeField]
	private FontStyle m_OptionTextStyle = FontData.defaultFontData.fontStyle;

	[SerializeField]
	private int m_OptionTextSize = FontData.defaultFontData.fontSize;

	[SerializeField]
	private Color m_OptionTextColor = Color.white;

	[SerializeField]
	private TextEffectType m_OptionTextEffect;

	[SerializeField]
	private Color m_OptionTextEffectColor = new Color(0f, 0f, 0f, 128f);

	[SerializeField]
	private Vector2 m_OptionTextEffectDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_OptionTextEffectUseGraphicAlpha = true;

	[SerializeField]
	private Vector2 m_OptionTextOffset = Vector2.zero;

	[SerializeField]
	private TransitionType m_OptionTransition;

	[SerializeField]
	private TransitionTarget m_OptionTransitionTarget = TransitionTarget.Text;

	[SerializeField]
	private Color m_OptionTransitionColorNormal = ColorBlock.defaultColorBlock.normalColor;

	[SerializeField]
	private Color m_OptionTransitionColorActive = ColorBlock.defaultColorBlock.highlightedColor;

	[SerializeField]
	[Range(1f, 6f)]
	private float m_OptionTransitionMultiplier = 1f;

	[SerializeField]
	private float m_OptionTransitionDuration = 0.1f;

	public List<string> options
	{
		get
		{
			return m_Options;
		}
		set
		{
			m_Options = value;
			RebuildOptions();
			ValidateOptions();
		}
	}

	public GameObject selectedOptionGameObject => m_CurrentOptionGameObject;

	public int selectedOptionIndex
	{
		get
		{
			int num = Mathf.RoundToInt(value);
			if (num < 0 || num >= m_Options.Count)
			{
				return 0;
			}
			return num;
		}
	}

	public RectOffset optionsPadding
	{
		get
		{
			return m_OptionsPadding;
		}
		set
		{
			m_OptionsPadding = value;
		}
	}

	public bool HasOptions()
	{
		if (m_Options != null)
		{
			return m_Options.Count > 0;
		}
		return false;
	}

	protected override void Start()
	{
		base.Start();
		if (Application.isPlaying)
		{
			onValueChanged.AddListener(OnValueChanged);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ValidateOptions();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (IsActive())
		{
			UpdateGridProperties();
		}
	}

	public void OnValueChanged(float value)
	{
		if (IsActive() && HasOptions() && m_OptionTransition == TransitionType.ColorTint)
		{
			Graphic target = ((m_OptionTransitionTarget == TransitionTarget.Text) ? ((Graphic)m_CurrentOptionGameObject.GetComponentInChildren<Text>()) : ((Graphic)m_CurrentOptionGameObject.GetComponent<Image>()));
			StartColorTween(target, m_OptionTransitionColorNormal * m_OptionTransitionMultiplier, m_OptionTransitionDuration);
			int num = Mathf.RoundToInt(value);
			if (num < 0 || num >= m_Options.Count)
			{
				num = 0;
			}
			GameObject gameObject = m_OptionGameObjects[num];
			if (gameObject != null)
			{
				Graphic target2 = ((m_OptionTransitionTarget == TransitionTarget.Text) ? ((Graphic)gameObject.GetComponentInChildren<Text>()) : ((Graphic)gameObject.GetComponent<Image>()));
				StartColorTween(target2, m_OptionTransitionColorActive * m_OptionTransitionMultiplier, m_OptionTransitionDuration);
			}
			m_CurrentOptionGameObject = gameObject;
		}
	}

	private void StartColorTween(Graphic target, Color targetColor, float duration)
	{
		if (!(target == null))
		{
			if (!Application.isPlaying || duration == 0f)
			{
				target.canvasRenderer.SetColor(targetColor);
			}
			else
			{
				target.CrossFadeColor(targetColor, duration, ignoreTimeScale: true, useAlpha: true);
			}
		}
	}

	protected void ValidateOptions()
	{
		if (!IsActive())
		{
			return;
		}
		if (!HasOptions())
		{
			if (m_OptionsContGameObject != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(m_OptionsContGameObject);
				}
				else
				{
					Object.DestroyImmediate(m_OptionsContGameObject);
				}
			}
			return;
		}
		if (m_OptionsContGameObject == null)
		{
			CreateOptionsContainer();
		}
		if (!wholeNumbers)
		{
			wholeNumbers = true;
		}
		minValue = 0f;
		maxValue = (float)m_Options.Count - 1f;
		UpdateGridProperties();
		UpdateOptionsProperties();
	}

	public void UpdateGridProperties()
	{
		if (!(m_OptionsContGrid == null))
		{
			if (!m_OptionsContGrid.padding.Equals(m_OptionsPadding))
			{
				m_OptionsContGrid.padding = m_OptionsPadding;
			}
			Vector2 vector = ((m_OptionSprite != null) ? new Vector2(m_OptionSprite.rect.width, m_OptionSprite.rect.height) : Vector2.zero);
			if (!m_OptionsContGrid.cellSize.Equals(vector))
			{
				m_OptionsContGrid.cellSize = vector;
			}
			float num = (m_OptionsContRect.rect.width - ((float)m_OptionsPadding.left + (float)m_OptionsPadding.right) - (float)m_Options.Count * vector.x) / ((float)m_Options.Count - 1f);
			if (m_OptionsContGrid.spacing.x != num)
			{
				m_OptionsContGrid.spacing = new Vector2(num, 0f);
			}
		}
	}

	public void UpdateOptionsProperties()
	{
		if (!HasOptions())
		{
			return;
		}
		int num = 0;
		foreach (GameObject optionGameObject in m_OptionGameObjects)
		{
			bool flag = Mathf.RoundToInt(value) == num;
			if (flag)
			{
				m_CurrentOptionGameObject = optionGameObject;
			}
			Image component = optionGameObject.GetComponent<Image>();
			if (component != null)
			{
				component.sprite = m_OptionSprite;
				component.rectTransform.pivot = new Vector2(0f, 1f);
				if (m_OptionTransition == TransitionType.ColorTint && m_OptionTransitionTarget == TransitionTarget.Image)
				{
					component.canvasRenderer.SetColor(flag ? m_OptionTransitionColorActive : m_OptionTransitionColorNormal);
				}
				else
				{
					component.canvasRenderer.SetColor(Color.white);
				}
			}
			Text componentInChildren = optionGameObject.GetComponentInChildren<Text>();
			if (componentInChildren != null)
			{
				componentInChildren.font = m_OptionTextFont;
				componentInChildren.fontStyle = m_OptionTextStyle;
				componentInChildren.fontSize = m_OptionTextSize;
				componentInChildren.color = m_OptionTextColor;
				if (m_OptionTransition == TransitionType.ColorTint && m_OptionTransitionTarget == TransitionTarget.Text)
				{
					componentInChildren.canvasRenderer.SetColor(flag ? m_OptionTransitionColorActive : m_OptionTransitionColorNormal);
				}
				else
				{
					componentInChildren.canvasRenderer.SetColor(Color.white);
				}
				(componentInChildren.transform as RectTransform).anchoredPosition = m_OptionTextOffset;
				UpdateTextEffect(componentInChildren.gameObject);
			}
			num++;
		}
	}

	protected void RebuildOptions()
	{
		if (!HasOptions())
		{
			return;
		}
		if (m_OptionsContGameObject == null)
		{
			CreateOptionsContainer();
		}
		DestroyOptions();
		int num = 0;
		foreach (string option in m_Options)
		{
			GameObject gameObject = new GameObject("Option " + num, typeof(RectTransform), typeof(Image));
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetParent(m_OptionsContGameObject.transform, worldPositionStays: false);
			GameObject gameObject2 = new GameObject("Text", typeof(RectTransform));
			gameObject2.layer = base.gameObject.layer;
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			gameObject2.AddComponent<Text>().text = option;
			ContentSizeFitter contentSizeFitter = gameObject2.AddComponent<ContentSizeFitter>();
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			m_OptionGameObjects.Add(gameObject);
			num++;
		}
		UpdateOptionsProperties();
	}

	private void AddTextEffect(GameObject gObject)
	{
		if (m_OptionTextEffect == TextEffectType.Shadow)
		{
			Shadow shadow = gObject.AddComponent<Shadow>();
			shadow.effectColor = m_OptionTextEffectColor;
			shadow.effectDistance = m_OptionTextEffectDistance;
			shadow.useGraphicAlpha = m_OptionTextEffectUseGraphicAlpha;
		}
		else if (m_OptionTextEffect == TextEffectType.Outline)
		{
			Outline outline = gObject.AddComponent<Outline>();
			outline.effectColor = m_OptionTextEffectColor;
			outline.effectDistance = m_OptionTextEffectDistance;
			outline.useGraphicAlpha = m_OptionTextEffectUseGraphicAlpha;
		}
	}

	private void UpdateTextEffect(GameObject gObject)
	{
		if (m_OptionTextEffect == TextEffectType.Shadow)
		{
			Shadow shadow = gObject.GetComponent<Shadow>();
			if (shadow == null)
			{
				shadow = gObject.AddComponent<Shadow>();
			}
			shadow.effectColor = m_OptionTextEffectColor;
			shadow.effectDistance = m_OptionTextEffectDistance;
			shadow.useGraphicAlpha = m_OptionTextEffectUseGraphicAlpha;
		}
		else if (m_OptionTextEffect == TextEffectType.Outline)
		{
			Outline outline = gObject.GetComponent<Outline>();
			if (outline == null)
			{
				outline = gObject.AddComponent<Outline>();
			}
			outline.effectColor = m_OptionTextEffectColor;
			outline.effectDistance = m_OptionTextEffectDistance;
			outline.useGraphicAlpha = m_OptionTextEffectUseGraphicAlpha;
		}
	}

	public void RebuildTextEffects()
	{
		foreach (GameObject optionGameObject in m_OptionGameObjects)
		{
			Text componentInChildren = optionGameObject.GetComponentInChildren<Text>();
			if (!(componentInChildren != null))
			{
				continue;
			}
			Shadow component = componentInChildren.gameObject.GetComponent<Shadow>();
			Outline component2 = componentInChildren.gameObject.GetComponent<Outline>();
			if (Application.isPlaying)
			{
				if (component != null)
				{
					Object.Destroy(component);
				}
				if (component2 != null)
				{
					Object.Destroy(component2);
				}
			}
			else
			{
				if (component != null)
				{
					Object.DestroyImmediate(component);
				}
				if (component2 != null)
				{
					Object.DestroyImmediate(component2);
				}
			}
			AddTextEffect(componentInChildren.gameObject);
		}
	}

	protected void DestroyOptions()
	{
		foreach (GameObject optionGameObject in m_OptionGameObjects)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(optionGameObject);
			}
			else
			{
				Object.DestroyImmediate(optionGameObject);
			}
		}
		m_OptionGameObjects.Clear();
	}

	protected void CreateOptionsContainer()
	{
		m_OptionsContGameObject = new GameObject("Options Grid", typeof(RectTransform), typeof(GridLayoutGroup));
		m_OptionsContGameObject.layer = gameObject.layer;
		m_OptionsContGameObject.transform.SetParent(transform, worldPositionStays: false);
		m_OptionsContGameObject.transform.SetAsFirstSibling();
		m_OptionsContRect = m_OptionsContGameObject.GetComponent<RectTransform>();
		m_OptionsContRect.sizeDelta = new Vector2(0f, 0f);
		m_OptionsContRect.anchorMin = new Vector2(0f, 0f);
		m_OptionsContRect.anchorMax = new Vector2(1f, 1f);
		m_OptionsContRect.anchoredPosition = new Vector2(0f, 0f);
		m_OptionsContRect.pivot = new Vector2(0f, 1f);
		m_OptionsContGrid = m_OptionsContGameObject.GetComponent<GridLayoutGroup>();
	}
}

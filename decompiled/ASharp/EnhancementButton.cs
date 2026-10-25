using System.Linq;
using FHGUI.DTO;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class EnhancementButton : EnhancementButtonBase
{
	public InfuseElement InfuseElement;

	private bool animating;

	private CanvasGroup canvasGroup;

	[SerializeField]
	private UIFX_MaterialFX_Control effectsControl;

	[SerializeField]
	private Image enhancementImage;

	[SerializeField]
	private EnhancementsUIDataSO m_DataSO;

	private bool highlighted;

	[SerializeField]
	[HideInInspector]
	public bool isEnabled;

	[SerializeField]
	[HideInInspector]
	private bool isInitialized;

	private Material effectsMaterial;

	[SerializeField]
	private UITextTooltipTarget tooltipTarget;

	private void Awake()
	{
		effectsMaterial = ((enhancementImage.material == enhancementImage.defaultMaterial) ? null : enhancementImage.material);
		canvasGroup = GetComponent<CanvasGroup>();
		tooltipTarget = GetComponent<UITextTooltipTarget>();
	}

	private void Start()
	{
		effectsControl.ToggleEnable(active: false);
		if (tooltipTarget != null)
		{
			tooltipTarget.Initialize(UITooltipManager.Corner.Auto, new Vector2(27.5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
			tooltipTarget.SetText(CreateLayout.LocaliseText("$Glossary_Enhancement_" + EnhancementType.ToString() + "$"));
		}
		EEnhancement enhancementType = EnhancementType;
		if ((uint)(enhancementType - 10) <= 6u)
		{
			InfuseElement.gameObject.SetActive(value: true);
		}
		else
		{
			InfuseElement.gameObject.SetActive(value: false);
		}
	}

	public override void Init(CEnhancement enhancement, string abilityNameLookupText, RectTransform parentContainer)
	{
		base.Init(enhancement, abilityNameLookupText, parentContainer);
		isInitialized = true;
		LayoutElement component = GetComponent<LayoutElement>();
		component.preferredWidth = GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSize;
		component.preferredHeight = GlobalSettings.Instance.m_AbilityCardSettings.EnhancementIconSize;
		UpdateEnhancement(enhancement.Enhancement);
	}

	public void Init(CEnhancement enhancement)
	{
		Init(enhancement, enhancement.AbilityName, ParentContainer ?? (transform.parent as RectTransform));
	}

	public void UpdateEnhancement(EEnhancement enhancement)
	{
		EnhancementType = enhancement;
		EEnhancement eEnhancement = enhancement;
		if ((uint)(eEnhancement - 10) <= 6u)
		{
			InfuseElement.gameObject.SetActive(value: true);
			enhancementImage.enabled = false;
			ElementInfusionBoardManager.EElement element;
			try
			{
				element = ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement s) => s.ToString() == enhancement.ToString());
			}
			catch
			{
				element = ElementInfusionBoardManager.EElement.Any;
			}
			InfuseElement.Init(element, isEnhancement: true);
		}
		else
		{
			InfuseElement.gameObject.SetActive(value: false);
			enhancementImage.enabled = true;
		}
		enhancementImage.sprite = GetEnhancementSprite(enhancement);
		tooltipTarget.SetText(CreateLayout.LocaliseText("$Glossary_Enhancement_" + enhancement.ToString() + "$"));
	}

	[ContextMenu("Update based on settings")]
	private void UpdateEnhancementToNone()
	{
		UpdateEnhancement(EEnhancement.NoEnhancement);
	}

	public Sprite GetEnhancementSprite(EEnhancement enhancement)
	{
		if (enhancement != EEnhancement.NoEnhancement)
		{
			return m_DataSO.GetEnhancementIcon(enhancement);
		}
		return m_DataSO.GetEnhancementSlotIcon(MarkType);
	}

	private void OnEnhancementPicked(EEnhancement enhancement)
	{
		isInitialized = true;
		UpdateEnhancement(enhancement);
		effectsControl.ApplyEffectType(enhancement);
	}

	public void Enable()
	{
		if (isInitialized)
		{
			isEnabled = true;
		}
	}

	public void Disable()
	{
		isEnabled = false;
	}

	public void ToggleHoverEffect(bool active)
	{
		UpdateEffectsView(active);
	}

	public void TogglePermanentHighlight(bool active)
	{
		highlighted = active;
		UpdateEffectsView(active);
	}

	public void AnimateGeneratedEnhancement()
	{
		if (gameObject.activeInHierarchy && !animating)
		{
			animating = true;
			effectsControl.ConsumableSelect(() =>
			{
				animating = false;
				TogglePermanentHighlight(active: false);
			});
			UpdateEffectsView(active: true);
		}
	}

	private void UpdateEffectsView(bool active)
	{
		if (!animating)
		{
			bool flag = active || highlighted;
			if (canvasGroup != null)
			{
				canvasGroup.ignoreParentGroups = flag;
			}
			enhancementImage.material = (flag ? effectsMaterial : null);
			effectsControl.ToggleEnable(flag);
		}
	}

	private void OnDisable()
	{
		animating = false;
		TogglePermanentHighlight(active: false);
	}
}

using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfusionElementUI : MonoBehaviour
{
	[SerializeField]
	private Image elementImage;

	[SerializeField]
	private Image creationImage;

	[SerializeField]
	private UIFX_MaterialFX_Control effectsControl;

	[SerializeField]
	private Image availableHighlight;

	[Header("Animations")]
	[SerializeField]
	private TextMeshProUGUI createElementText;

	[SerializeField]
	private TextMeshProUGUI creatingElementText;

	[SerializeField]
	private Image creationBumpImage;

	[SerializeField]
	private Image creationTextBackgroundImage;

	[SerializeField]
	private GUIAnimator animatorCreating;

	[SerializeField]
	private GUIAnimator animatorCreated;

	[SerializeField]
	private LoopAnimator loopAnimatorCreating;

	[SerializeField]
	private string changeToStrongElementAudioItem = "PlaySound_ElementsCharged";

	private ElementInfusionBoardManager.EElement elementType;

	private Sprite completeElement;

	private Sprite waningElement;

	private UITextTooltipTarget tooltipTarget;

	private ElementInfusionBoardManager.EColumn? lastState;

	private void Awake()
	{
		tooltipTarget = GetComponent<UITextTooltipTarget>();
		Material fontMaterial = new Material(createElementText.fontSharedMaterial);
		createElementText.fontMaterial = fontMaterial;
		creatingElementText.fontMaterial = fontMaterial;
	}

	public void Init(ElementInfusionBoardManager.EElement type, Sprite completeElement, Sprite elementCreation, Sprite waningElement, Sprite elementCreationHighlight, Color elementColor)
	{
		creationImage.sprite = elementCreation;
		creationBumpImage.sprite = elementCreation;
		creationTextBackgroundImage.sprite = elementCreationHighlight;
		this.completeElement = completeElement;
		this.waningElement = waningElement;
		elementType = type;
		SetState(null);
		effectsControl.ApplyEffectType(type);
		effectsControl.ToggleEnable(active: false);
		createElementText.text = string.Format(LocalizationManager.GetTranslation("GUI_ELEMENT_INFUSION_CREATED"), LocalizationManager.GetTranslation("GUI_ELEMENT_" + type.ToString().ToUpperInvariant()));
		createElementText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, elementColor);
		creatingElementText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, elementColor);
		elementImage.enabled = false;
		availableHighlight.color = UIInfoTools.Instance.GetElementHighlightColor(type, availableHighlight.color.a);
		SetAvailable(available: false);
	}

	private void OnDisable()
	{
		StopAnimations();
	}

	public void SetState(ElementInfusionBoardManager.EColumn? newState, bool isCreating = false, bool isReserved = false, bool flash = false)
	{
		if (!newState.HasValue)
		{
			StopAnimations();
			lastState = null;
			return;
		}
		if (flash)
		{
			effectsControl.ConsumableSelect();
		}
		switch (newState)
		{
		case ElementInfusionBoardManager.EColumn.Inert:
			effectsControl.ToggleEnable(active: true);
			elementImage.enabled = false;
			creationImage.enabled = isCreating;
			if (lastState != newState)
			{
				ShowCreating();
			}
			if (isCreating)
			{
				tooltipTarget?.SetText(LocalizationManager.GetTranslation("GUI_ELEMENT_" + elementType.ToString().ToUpper()) + ": " + LocalizationManager.GetTranslation("GUI_TOOLTIP_ELEMENT_INFUSING"));
			}
			else
			{
				tooltipTarget?.SetText(LocalizationManager.GetTranslation("GUI_TOOLTIP_ELEMENT_INERT"));
			}
			SetAvailable(available: false);
			break;
		case ElementInfusionBoardManager.EColumn.Strong:
			if (!lastState.HasValue || (lastState == ElementInfusionBoardManager.EColumn.Inert && gameObject.activeInHierarchy))
			{
				ShowCreated();
			}
			if (lastState != ElementInfusionBoardManager.EColumn.Strong)
			{
				AudioControllerUtils.PlaySound(changeToStrongElementAudioItem);
			}
			effectsControl.ToggleEnable(active: true);
			elementImage.enabled = !isCreating;
			creationImage.enabled = isCreating;
			elementImage.sprite = completeElement;
			tooltipTarget?.SetText(LocalizationManager.GetTranslation("GUI_ELEMENT_" + elementType.ToString().ToUpper()) + ": " + LocalizationManager.GetTranslation("GUI_TOOLTIP_ELEMENT_STRONG"));
			break;
		case ElementInfusionBoardManager.EColumn.Waning:
			if (!lastState.HasValue || lastState == ElementInfusionBoardManager.EColumn.Inert)
			{
				ShowCreated();
			}
			else
			{
				StopAnimations();
			}
			effectsControl.ToggleEnable(active: false);
			elementImage.enabled = !isCreating;
			creationImage.enabled = isCreating;
			elementImage.sprite = waningElement;
			tooltipTarget?.SetText(LocalizationManager.GetTranslation("GUI_ELEMENT_" + elementType.ToString().ToUpper()) + ": " + LocalizationManager.GetTranslation("GUI_TOOLTIP_ELEMENT_WANING"));
			break;
		}
		lastState = newState;
	}

	private void ShowCreating()
	{
		animatorCreated.GoToFinishState();
		animatorCreating.Play();
		transform.SetAsLastSibling();
	}

	private void ShowCreated()
	{
		loopAnimatorCreating.StopLoop(resetToInitial: true);
		animatorCreating.GoInitState();
		if (gameObject.activeInHierarchy)
		{
			animatorCreated.Play();
		}
	}

	private void StopAnimations()
	{
		animatorCreated.GoInitState();
		loopAnimatorCreating.StopLoop(resetToInitial: true);
		animatorCreating.GoInitState();
	}

	public void SetAvailable(bool available)
	{
		availableHighlight.enabled = available;
	}
}

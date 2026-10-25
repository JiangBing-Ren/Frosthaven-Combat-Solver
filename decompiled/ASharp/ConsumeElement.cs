using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class ConsumeElement : MonoBehaviour
{
	private enum ConsumeState
	{
		Disabled,
		Unavailable,
		Available,
		Clicked
	}

	[SerializeField]
	private Image elementImage;

	[SerializeField]
	private Image availableHighlight;

	[SerializeField]
	private Sprite[] elementSprites;

	[SerializeField]
	private Sprite[] highlightElementSprites;

	[HideInInspector]
	[SerializeField]
	public bool isAnyElement;

	[SerializeField]
	private ElementInfusionBoardManager.EElement selectedElement;

	[SerializeField]
	private UITextTooltipTarget tooltipTarget;

	[SerializeField]
	private GlossaryProvider glossaryProvider;

	private bool isSelected;

	private int elementIndex;

	private bool available;

	private bool isEnabled;

	public bool IsInitialised { get; private set; }

	public ElementInfusionBoardManager.EElement SelectedElement => selectedElement;

	private void Awake()
	{
		RefreshTooltip();
	}

	public void Init(ElementInfusionBoardManager.EElement element, int elementIndex = 0)
	{
		this.elementIndex = elementIndex;
		isAnyElement = element == ElementInfusionBoardManager.EElement.Any;
		selectedElement = element;
		if (elementSprites.Length > (int)element)
		{
			elementImage.sprite = elementSprites[(int)element];
		}
		else
		{
			Log.GUIError("Element image sprite is not set.");
		}
		IsInitialised = true;
		if (Application.isPlaying)
		{
			availableHighlight.color = UIInfoTools.Instance.GetElementHighlightColor(element, availableHighlight.color.a);
		}
		SetAvailable(available: false);
		SetUnselected();
	}

	public void SetSelected(ElementInfusionBoardManager.EElement element)
	{
		if (isAnyElement)
		{
			selectedElement = element;
			elementImage.sprite = elementSprites[(int)element];
			availableHighlight.color = UIInfoTools.Instance.GetElementHighlightColor(element, availableHighlight.color.a);
		}
		isSelected = true;
		RefreshTooltip();
	}

	public void SetUnselected()
	{
		if (isAnyElement)
		{
			selectedElement = ElementInfusionBoardManager.EElement.Any;
			elementImage.sprite = elementSprites[6];
			if (Application.isPlaying)
			{
				availableHighlight.color = UIInfoTools.Instance.GetElementHighlightColor(ElementInfusionBoardManager.EElement.Any, availableHighlight.color.a);
			}
		}
		isSelected = false;
		RefreshTooltip();
	}

	private void SetTooltipText(ConsumeState consumeState, bool refreshTooltip = false)
	{
		tooltipTarget?.SetText(LocalizationManager.GetTranslation("Glossary_Consume" + selectedElement) + ": " + LocalizationManager.GetTranslation("GUI_TOOLTIP_CONSUMING_" + consumeState.ToString().ToUpperInvariant()), refreshTooltip);
	}

	public void SetAvailable(bool available)
	{
		this.available = available;
		availableHighlight.gameObject.SetActive(available && isEnabled);
	}

	public void SetEnabled(bool isEnabled)
	{
		this.isEnabled = isEnabled;
		availableHighlight.gameObject.SetActive(available & isEnabled);
	}

	[ContextMenu("Update based on selected element")]
	private void UpdateBasedOnSelectedElement()
	{
		isAnyElement = selectedElement == ElementInfusionBoardManager.EElement.Any;
		elementImage.sprite = elementSprites[(int)selectedElement];
	}

	private void RefreshTooltip()
	{
		if (isSelected)
		{
			SetTooltipText(ConsumeState.Clicked, refreshTooltip: true);
		}
		else
		{
			SetTooltipText(ConsumeState.Disabled);
		}
		if (glossaryProvider != null)
		{
			glossaryProvider.OverrideGlossaryKey(selectedElement.ToString());
		}
	}
}

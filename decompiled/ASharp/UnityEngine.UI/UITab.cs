using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UI/Tab", 58)]
public class UITab : Toggle
{
	public enum TextTransition
	{
		None,
		ColorTint,
		Material
	}

	[SerializeField]
	private GameObject m_TargetContent;

	[SerializeField]
	private Image m_ImageTarget;

	[SerializeField]
	private Image[] m_AdditionalImageTargets;

	[SerializeField]
	private Transition m_ImageTransition;

	[SerializeField]
	private ColorBlockExtended m_ImageColors = ColorBlockExtended.defaultColorBlock;

	[SerializeField]
	private SpriteStateExtended m_ImageSpriteState;

	[SerializeField]
	private AnimationTriggersExtended m_ImageAnimationTriggers = new AnimationTriggersExtended();

	[SerializeField]
	private TextMeshProUGUI m_TextTarget;

	[SerializeField]
	private TextTransition m_TextTransition;

	[SerializeField]
	private ColorBlockExtended m_TextColors = ColorBlockExtended.defaultColorBlock;

	[SerializeField]
	private MaterialBlockExtended m_TextMaterials = new MaterialBlockExtended();

	[SerializeField]
	private string mouseDownAudioItem;

	[SerializeField]
	private string mouseUpAudioItem;

	[SerializeField]
	private string mouseEnterAudioItem;

	[SerializeField]
	private string mouseExitAudioItem;

	[SerializeField]
	private AudioButtonProfile audioProfile;

	[SerializeField]
	private bool playAudioWhenNoInteractable;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	private SelectionState m_CurrentState;

	public GameObject TargetContent
	{
		get
		{
			return m_TargetContent;
		}
		set
		{
			m_TargetContent = value;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		onValueChanged.AddListener(OnToggleStateChanged);
		InternalEvaluateAndTransitionState(instant: true);
	}

	protected void OnToggleStateChanged(bool state)
	{
		if (IsActive() && Application.isPlaying)
		{
			InternalEvaluateAndTransitionState(instant: false);
		}
	}

	private void EvaluateAndToggleContent()
	{
		if (m_TargetContent != null)
		{
			m_TargetContent.SetActive(isOn);
		}
	}

	private void InternalEvaluateAndTransitionState(bool instant)
	{
		EvaluateAndToggleContent();
		if (graphic != null && graphic.transform.childCount > 0)
		{
			foreach (Transform item in graphic.transform)
			{
				Graphic component = item.GetComponent<Graphic>();
				if (component != null)
				{
					if (instant)
					{
						component.canvasRenderer.SetAlpha((!isOn) ? 0f : 1f);
					}
					else
					{
						component.CrossFadeAlpha((!isOn) ? 0f : 1f, 0.1f, ignoreTimeScale: true);
					}
				}
			}
		}
		DoStateTransition(m_CurrentState, instant);
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		m_CurrentState = state;
		Color color = m_ImageColors.normalColor;
		Color color2 = m_TextColors.normalColor;
		Sprite newSprite = null;
		string triggername = m_ImageAnimationTriggers.normalTrigger;
		Material material = m_TextMaterials.normal;
		switch (state)
		{
		case SelectionState.Normal:
			color = ((!isOn) ? m_ImageColors.normalColor : m_ImageColors.activeColor);
			color2 = ((!isOn) ? m_TextColors.normalColor : m_TextColors.activeColor);
			newSprite = ((!isOn) ? null : m_ImageSpriteState.activeSprite);
			triggername = ((!isOn) ? m_ImageAnimationTriggers.normalTrigger : m_ImageAnimationTriggers.activeTrigger);
			material = ((!isOn) ? m_TextMaterials.normal : m_TextMaterials.active);
			break;
		case SelectionState.Highlighted:
			color = ((!isOn) ? m_ImageColors.highlightedColor : m_ImageColors.activeHighlightedColor);
			color2 = ((!isOn) ? m_TextColors.highlightedColor : m_TextColors.activeHighlightedColor);
			newSprite = ((!isOn) ? m_ImageSpriteState.highlightedSprite : m_ImageSpriteState.activeHighlightedSprite);
			triggername = ((!isOn) ? m_ImageAnimationTriggers.highlightedTrigger : m_ImageAnimationTriggers.activeHighlightedTrigger);
			material = ((!isOn) ? m_TextMaterials.highlighted : m_TextMaterials.activeHighlighted);
			break;
		case SelectionState.Selected:
			color = ((!isOn) ? m_ImageColors.selectedColor : m_ImageColors.activeSelectedColor);
			color2 = ((!isOn) ? m_TextColors.selectedColor : m_TextColors.activeSelectedColor);
			newSprite = ((!isOn) ? m_ImageSpriteState.selectedSprite : m_ImageSpriteState.activeSelectedSprite);
			triggername = ((!isOn) ? m_ImageAnimationTriggers.selectedTrigger : m_ImageAnimationTriggers.activeSelectedTrigger);
			material = ((!isOn) ? m_TextMaterials.selected : m_TextMaterials.activeSelected);
			break;
		case SelectionState.Pressed:
			color = ((!isOn) ? m_ImageColors.pressedColor : m_ImageColors.activePressedColor);
			color2 = ((!isOn) ? m_TextColors.pressedColor : m_TextColors.activePressedColor);
			newSprite = ((!isOn) ? m_ImageSpriteState.pressedSprite : m_ImageSpriteState.activePressedSprite);
			triggername = ((!isOn) ? m_ImageAnimationTriggers.pressedTrigger : m_ImageAnimationTriggers.activePressedTrigger);
			material = ((!isOn) ? m_TextMaterials.pressed : m_TextMaterials.activePressed);
			break;
		case SelectionState.Disabled:
			color = ((!isOn) ? m_ImageColors.disabledColor : m_ImageColors.activeDisabledColor);
			color2 = ((!isOn) ? m_TextColors.disabledColor : m_TextColors.activeDisabledColor);
			newSprite = m_ImageSpriteState.disabledSprite;
			triggername = m_ImageAnimationTriggers.disabledTrigger;
			material = m_TextMaterials.disabled;
			break;
		}
		if (!gameObject.activeInHierarchy)
		{
			return;
		}
		switch (m_ImageTransition)
		{
		case Transition.ColorTint:
		{
			StartColorTween(m_ImageTarget, color * m_ImageColors.colorMultiplier, instant ? 0f : m_ImageColors.fadeDuration);
			for (int j = 0; j < m_AdditionalImageTargets.Length; j++)
			{
				StartColorTween(m_AdditionalImageTargets[j], color * m_ImageColors.colorMultiplier, instant ? 0f : m_ImageColors.fadeDuration);
			}
			break;
		}
		case Transition.SpriteSwap:
		{
			DoSpriteSwap(m_ImageTarget, newSprite);
			for (int k = 0; k < m_AdditionalImageTargets.Length; k++)
			{
				DoSpriteSwap(m_AdditionalImageTargets[k], newSprite);
			}
			break;
		}
		case Transition.Animation:
		{
			TriggerAnimation(m_ImageTarget.gameObject, triggername);
			for (int i = 0; i < m_AdditionalImageTargets.Length; i++)
			{
				TriggerAnimation(m_AdditionalImageTargets[i].gameObject, triggername);
			}
			break;
		}
		}
		switch (m_TextTransition)
		{
		case TextTransition.ColorTint:
			StartColorTween(m_TextTarget, color2 * m_TextColors.colorMultiplier, instant ? 0f : m_TextColors.fadeDuration);
			break;
		case TextTransition.Material:
			if (material == null)
			{
				material = m_TextMaterials.normal;
			}
			if (material != null)
			{
				m_TextTarget.fontSharedMaterial = material;
			}
			break;
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

	private void DoSpriteSwap(Image target, Sprite newSprite)
	{
		if (!(target == null))
		{
			target.overrideSprite = newSprite;
		}
	}

	private void TriggerAnimation(GameObject target, string triggername)
	{
		if (!(target == null))
		{
			Animator component = target.GetComponent<Animator>();
			if (!(component == null) && component.isActiveAndEnabled && !(component.runtimeAnimatorController == null) && !string.IsNullOrEmpty(triggername))
			{
				component.ResetTrigger(m_ImageAnimationTriggers.normalTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.pressedTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.highlightedTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.activeTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.activeHighlightedTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.activePressedTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.disabledTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.selectedTrigger);
				component.ResetTrigger(m_ImageAnimationTriggers.activeSelectedTrigger);
				component.SetTrigger(triggername);
			}
		}
	}

	public void Activate()
	{
		if (!isOn)
		{
			isOn = true;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseDownAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseDownAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseUpAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseUpAudioItem : mouseUpAudioItem);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTab(this))
		{
			Log.GUIInfo("Toggle press for tab " + gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerClick(eventData);
		if (interactable)
		{
			AutoTestController.LogButtonClick(gameObject, this, "OnPointerClick");
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
		AutoTestController.LogButtonHover(gameObject, this);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			if (IsInteractable() || playAudioWhenNoInteractable)
			{
				AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
			}
			OnSelected.Invoke();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (Singleton<InputFHManager>.Instance.UsingGamepad && (IsInteractable() || playAudioWhenNoInteractable))
		{
			AudioControllerUtils.PlaySound((mouseExitAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseExitAudioItem : mouseExitAudioItem);
		}
		OnDeselected.Invoke();
	}

	public new void SetIsOnWithoutNotify(bool value)
	{
		base.SetIsOnWithoutNotify(value);
		OnToggleStateChanged(isOn);
	}
}

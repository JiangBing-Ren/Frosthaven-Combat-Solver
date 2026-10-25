using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FHTabButton : FHButton
{
	[HideInInspector]
	[SerializeField]
	private GameObject OnObject;

	[HideInInspector]
	[SerializeField]
	private GameObject OffObject;

	[HideInInspector]
	[SerializeField]
	private Sprite m_ActiveNormalSprite;

	[SerializeField]
	private ColorBlock m_ActiveColors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_ActiveSpriteState;

	[SerializeField]
	private AnimationTriggers m_ActiveAnimationTriggers = new AnimationTriggers();

	public UnityAction OnInteractableStateChanged;

	[SerializeField]
	private bool _isFocused;

	private bool m_WasInteractable;

	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (_isFocused != value)
			{
				_isFocused = value;
				DoStateTransition(currentSelectionState, instant: false);
				if ((bool)OnObject)
				{
					OnObject.SetActive(_isFocused);
				}
				if ((bool)OffObject)
				{
					OffObject.SetActive(!_isFocused);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		bool isFocused = _isFocused;
		_isFocused = !_isFocused;
		IsFocused = isFocused;
	}

	private void CheckInteractable()
	{
		if (m_WasInteractable != interactable)
		{
			m_WasInteractable = interactable;
			OnInteractableStateChanged?.Invoke();
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		CheckInteractable();
		if (!IsFocused)
		{
			base.DoStateTransition(state, instant);
		}
		else
		{
			if (!gameObject.activeInHierarchy)
			{
				return;
			}
			Color color;
			Sprite overrideSprite;
			string triggername;
			switch (state)
			{
			case SelectionState.Normal:
				color = m_ActiveColors.normalColor;
				overrideSprite = m_ActiveNormalSprite;
				triggername = m_ActiveAnimationTriggers.normalTrigger;
				break;
			case SelectionState.Highlighted:
				color = m_ActiveColors.highlightedColor;
				overrideSprite = m_ActiveSpriteState.highlightedSprite;
				triggername = m_ActiveAnimationTriggers.highlightedTrigger;
				break;
			case SelectionState.Pressed:
				color = m_ActiveColors.pressedColor;
				overrideSprite = m_ActiveSpriteState.pressedSprite;
				triggername = m_ActiveAnimationTriggers.pressedTrigger;
				break;
			case SelectionState.Selected:
				color = m_ActiveColors.selectedColor;
				overrideSprite = m_ActiveSpriteState.selectedSprite;
				triggername = m_ActiveAnimationTriggers.selectedTrigger;
				break;
			case SelectionState.Disabled:
				color = m_ActiveColors.disabledColor;
				overrideSprite = m_ActiveSpriteState.disabledSprite;
				triggername = m_ActiveAnimationTriggers.disabledTrigger;
				break;
			default:
				color = Color.black;
				overrideSprite = null;
				triggername = string.Empty;
				break;
			}
			switch (transition)
			{
			case Transition.ColorTint:
				if ((bool)targetGraphic)
				{
					targetGraphic.CrossFadeColor(color * m_ActiveColors.colorMultiplier, instant ? 0f : m_ActiveColors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
				}
				break;
			case Transition.SpriteSwap:
				if ((bool)image)
				{
					image.overrideSprite = overrideSprite;
				}
				break;
			case Transition.Animation:
				TriggerAnimation(m_ActiveAnimationTriggers, triggername);
				break;
			}
			InvokeOnSelectionStateChanged(state);
		}
	}

	private void TriggerAnimation(AnimationTriggers animationTriggers, string triggername)
	{
	}

	public override void RemoveAllEventListeners()
	{
		base.RemoveAllEventListeners();
		OnInteractableStateChanged = null;
	}
}

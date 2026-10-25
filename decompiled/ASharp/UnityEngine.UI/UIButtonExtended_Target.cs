using System;

namespace UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/Button Extended - Target", 58)]
[RequireComponent(typeof(UIButtonExtended))]
public class UIButtonExtended_Target : MonoBehaviour
{
	[Serializable]
	public struct SpriteState
	{
		[SerializeField]
		private Sprite m_HighlightedSprite;

		[SerializeField]
		private Sprite m_PressedSprite;

		[SerializeField]
		private Sprite m_DisabledSprite;

		public Sprite highlightedSprite
		{
			get
			{
				return m_HighlightedSprite;
			}
			set
			{
				m_HighlightedSprite = value;
			}
		}

		public Sprite pressedSprite
		{
			get
			{
				return m_PressedSprite;
			}
			set
			{
				m_PressedSprite = value;
			}
		}

		public Sprite disabledSprite
		{
			get
			{
				return m_DisabledSprite;
			}
			set
			{
				m_DisabledSprite = value;
			}
		}
	}

	[SerializeField]
	private Selectable.Transition m_Transition = Selectable.Transition.ColorTint;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

	[SerializeField]
	[Tooltip("Graphic that will have the selected transtion applied.")]
	private Graphic m_TargetGraphic;

	[SerializeField]
	[Tooltip("GameObject that will have the selected transtion applied.")]
	private GameObject m_TargetGameObject;

	public Selectable.Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public ColorBlock colors
	{
		get
		{
			return m_Colors;
		}
		set
		{
			m_Colors = value;
		}
	}

	public SpriteState spriteState
	{
		get
		{
			return m_SpriteState;
		}
		set
		{
			m_SpriteState = value;
		}
	}

	public AnimationTriggers animationTriggers
	{
		get
		{
			return m_AnimationTriggers;
		}
		set
		{
			m_AnimationTriggers = value;
		}
	}

	public Graphic targetGraphic
	{
		get
		{
			return m_TargetGraphic;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public GameObject targetGameObject
	{
		get
		{
			return m_TargetGameObject;
		}
		set
		{
			m_TargetGameObject = value;
		}
	}

	public Animator animator
	{
		get
		{
			if (m_TargetGameObject != null)
			{
				return m_TargetGameObject.GetComponent<Animator>();
			}
			return null;
		}
	}

	public UIButtonExtended button => GetComponent<UIButtonExtended>();

	protected void Awake()
	{
		if (button != null)
		{
			button.onStateChange.AddListener(OnStateChange);
		}
	}

	protected void OnEnable()
	{
		InternalEvaluateAndTransitionToNormalState(instant: true);
	}

	protected void OnDisable()
	{
		InstantClearState();
	}

	protected void OnDestroy()
	{
		if (button != null)
		{
			button.onStateChange.RemoveListener(OnStateChange);
		}
	}

	protected void InstantClearState()
	{
		switch (m_Transition)
		{
		case Selectable.Transition.ColorTint:
			StartColorTween(Color.white, instant: true);
			break;
		case Selectable.Transition.SpriteSwap:
			DoSpriteSwap(null);
			break;
		case Selectable.Transition.Animation:
			TriggerAnimation(m_AnimationTriggers.normalTrigger);
			break;
		}
	}

	private void InternalEvaluateAndTransitionToNormalState(bool instant)
	{
		OnStateChange(UIButtonExtended.VisualState.Normal, instant);
	}

	protected virtual void OnStateChange(UIButtonExtended.VisualState state, bool instant)
	{
		if (isActiveAndEnabled)
		{
			Color color = m_Colors.normalColor;
			Sprite newSprite = null;
			string triggername = m_AnimationTriggers.normalTrigger;
			switch (state)
			{
			case UIButtonExtended.VisualState.Normal:
				color = m_Colors.normalColor;
				newSprite = null;
				triggername = m_AnimationTriggers.normalTrigger;
				break;
			case UIButtonExtended.VisualState.Highlighted:
				color = m_Colors.highlightedColor;
				newSprite = m_SpriteState.highlightedSprite;
				triggername = m_AnimationTriggers.highlightedTrigger;
				break;
			case UIButtonExtended.VisualState.Pressed:
				color = m_Colors.pressedColor;
				newSprite = m_SpriteState.pressedSprite;
				triggername = m_AnimationTriggers.pressedTrigger;
				break;
			case UIButtonExtended.VisualState.Disabled:
				color = m_Colors.disabledColor;
				newSprite = m_SpriteState.disabledSprite;
				triggername = m_AnimationTriggers.disabledTrigger;
				break;
			}
			switch (m_Transition)
			{
			case Selectable.Transition.ColorTint:
				StartColorTween(color * m_Colors.colorMultiplier, instant);
				break;
			case Selectable.Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Selectable.Transition.Animation:
				TriggerAnimation(triggername);
				break;
			}
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			m_TargetGraphic.CrossFadeColor(targetColor, (!instant) ? m_Colors.fadeDuration : 0f, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		Image image = m_TargetGraphic as Image;
		if (!(image == null))
		{
			image.overrideSprite = newSprite;
		}
	}

	private void TriggerAnimation(string triggername)
	{
		if (!(targetGameObject == null))
		{
			Animator component = targetGameObject.GetComponent<Animator>();
			if (!(component == null) && component.enabled && component.isActiveAndEnabled && !(component.runtimeAnimatorController == null) && !string.IsNullOrEmpty(triggername))
			{
				component.ResetTrigger(m_AnimationTriggers.normalTrigger);
				component.ResetTrigger(m_AnimationTriggers.pressedTrigger);
				component.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
				component.ResetTrigger(m_AnimationTriggers.disabledTrigger);
				component.SetTrigger(triggername);
			}
		}
	}
}

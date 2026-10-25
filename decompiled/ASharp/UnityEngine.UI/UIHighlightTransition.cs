using UnityEngine.EventSystems;

namespace UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/Highlight Transition")]
public class UIHighlightTransition : MonoBehaviour, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
	public enum VisualState
	{
		Normal,
		Highlighted,
		Selected
	}

	public enum Transition
	{
		None,
		ColorTint,
		SpriteSwap,
		Animation
	}

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private Color m_NormalColor = ColorBlock.defaultColorBlock.normalColor;

	[SerializeField]
	private Color m_HighlightedColor = ColorBlock.defaultColorBlock.highlightedColor;

	[SerializeField]
	private Color m_SelectedColor = ColorBlock.defaultColorBlock.highlightedColor;

	[SerializeField]
	private float m_Duration = 0.1f;

	[SerializeField]
	[Range(1f, 6f)]
	private float m_ColorMultiplier = 1f;

	[SerializeField]
	private Sprite m_HighlightedSprite;

	[SerializeField]
	private Sprite m_SelectedSprite;

	[SerializeField]
	private string m_NormalTrigger = "Normal";

	[SerializeField]
	private string m_HighlightedTrigger = "Highlighted";

	[SerializeField]
	private string m_SelectedTrigger = "Selected";

	[SerializeField]
	[Tooltip("Graphic that will have the selected transtion applied.")]
	private Graphic m_TargetGraphic;

	[SerializeField]
	[Tooltip("GameObject that will have the selected transtion applied.")]
	private GameObject m_TargetGameObject;

	private bool m_Highlighted;

	private bool m_Selected;

	public Transition transition
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

	protected void OnEnable()
	{
		InternalEvaluateAndTransitionToNormalState(instant: true);
	}

	protected void OnDisable()
	{
		InstantClearState();
	}

	protected void InstantClearState()
	{
		switch (m_Transition)
		{
		case Transition.ColorTint:
			StartColorTween(Color.white, instant: true);
			break;
		case Transition.SpriteSwap:
			DoSpriteSwap(null);
			break;
		case Transition.Animation:
			TriggerAnimation(m_NormalTrigger);
			break;
		}
	}

	private void InternalEvaluateAndTransitionToNormalState(bool instant)
	{
		DoStateTransition(VisualState.Normal, instant);
	}

	public void OnSelect(BaseEventData eventData)
	{
		m_Selected = true;
		DoStateTransition(VisualState.Selected, instant: false);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_Selected = false;
		DoStateTransition(m_Highlighted ? VisualState.Highlighted : VisualState.Normal, instant: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_Highlighted = true;
		if (!m_Selected)
		{
			DoStateTransition(VisualState.Highlighted, instant: false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_Highlighted = false;
		if (!m_Selected)
		{
			DoStateTransition(VisualState.Normal, instant: false);
		}
	}

	protected virtual void DoStateTransition(VisualState state, bool instant)
	{
		if (enabled && gameObject.activeInHierarchy)
		{
			Color color = m_NormalColor;
			Sprite newSprite = null;
			string triggername = m_NormalTrigger;
			switch (state)
			{
			case VisualState.Normal:
				color = m_NormalColor;
				newSprite = null;
				triggername = m_NormalTrigger;
				break;
			case VisualState.Highlighted:
				color = m_HighlightedColor;
				newSprite = m_HighlightedSprite;
				triggername = m_HighlightedTrigger;
				break;
			case VisualState.Selected:
				color = m_SelectedColor;
				newSprite = m_SelectedSprite;
				triggername = m_SelectedTrigger;
				break;
			}
			switch (m_Transition)
			{
			case Transition.ColorTint:
				StartColorTween(color * m_ColorMultiplier, instant);
				break;
			case Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Transition.Animation:
				TriggerAnimation(triggername);
				break;
			}
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			if (instant || m_Duration == 0f || !Application.isPlaying)
			{
				m_TargetGraphic.canvasRenderer.SetColor(targetColor);
			}
			else
			{
				m_TargetGraphic.CrossFadeColor(targetColor, m_Duration, ignoreTimeScale: true, useAlpha: true);
			}
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
		if (!(targetGameObject == null) && !(animator == null) && animator.enabled && animator.isActiveAndEnabled && !(animator.runtimeAnimatorController == null) && !string.IsNullOrEmpty(triggername))
		{
			animator.ResetTrigger(m_HighlightedTrigger);
			animator.ResetTrigger(m_SelectedTrigger);
			animator.SetTrigger(triggername);
		}
	}
}

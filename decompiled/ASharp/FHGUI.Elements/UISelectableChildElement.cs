using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

[ExecuteAlways]
public class UISelectableChildElement : MonoBehaviour
{
	[SerializeField]
	public Selectable _parent;

	private IFHSelectable _selectableParent;

	[SerializeField]
	private Selectable.Transition m_Transition = Selectable.Transition.ColorTint;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

	[SerializeField]
	private Graphic m_TargetGraphic;

	private IFHSelectable.SelectionState _currentState;

	public Image image
	{
		get
		{
			return m_TargetGraphic as Image;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	private void Awake()
	{
		m_TargetGraphic = GetComponent<Graphic>();
		if (_parent != null && _selectableParent == null)
		{
			FindParent();
		}
	}

	private void OnEnable()
	{
		if (_selectableParent != null)
		{
			DoStateTransition(GetParentState(), instant: true);
			WireParentEvents();
		}
	}

	private void OnDisable()
	{
		if (_selectableParent != null)
		{
			_selectableParent.OnSelectionStateChanged -= OnParentStateChanged;
		}
	}

	private void Reset()
	{
		if (_parent == null)
		{
			_parent = (Selectable)GetComponentInParent<IFHSelectable>(includeInactive: true);
		}
		FindParent();
		DoStateTransition(IFHSelectable.SelectionState.Normal, instant: true);
	}

	private void FindParent()
	{
		if (_selectableParent == null)
		{
			_selectableParent = _parent as IFHSelectable;
			if (_selectableParent != null)
			{
				WireParentEvents();
			}
		}
	}

	private void WireParentEvents()
	{
		_selectableParent.OnSelectionStateChanged -= OnParentStateChanged;
		_selectableParent.OnSelectionStateChanged += OnParentStateChanged;
	}

	private void OnParentStateChanged(IFHSelectable.SelectionState state)
	{
		DoStateTransition(state, instant: false);
	}

	public void RefreshState()
	{
		DoStateTransition(GetParentState(), instant: true);
	}

	private void DoStateTransition(IFHSelectable.SelectionState state, bool instant)
	{
		if (gameObject.activeInHierarchy && _selectableParent != null)
		{
			_currentState = state;
			Color color;
			Sprite newSprite;
			string triggername;
			switch (state)
			{
			case IFHSelectable.SelectionState.Normal:
				color = m_Colors.normalColor;
				newSprite = null;
				triggername = m_AnimationTriggers.normalTrigger;
				break;
			case IFHSelectable.SelectionState.Highlighted:
				color = m_Colors.highlightedColor;
				newSprite = m_SpriteState.highlightedSprite;
				triggername = m_AnimationTriggers.highlightedTrigger;
				break;
			case IFHSelectable.SelectionState.Pressed:
				color = m_Colors.pressedColor;
				newSprite = m_SpriteState.pressedSprite;
				triggername = m_AnimationTriggers.pressedTrigger;
				break;
			case IFHSelectable.SelectionState.Selected:
				color = m_Colors.selectedColor;
				newSprite = m_SpriteState.selectedSprite;
				triggername = m_AnimationTriggers.selectedTrigger;
				break;
			case IFHSelectable.SelectionState.Disabled:
				color = m_Colors.disabledColor;
				newSprite = m_SpriteState.disabledSprite;
				triggername = m_AnimationTriggers.disabledTrigger;
				break;
			default:
				color = Color.black;
				newSprite = null;
				triggername = string.Empty;
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

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(image == null))
		{
			image.overrideSprite = newSprite;
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void TriggerAnimation(string triggername)
	{
	}

	private IFHSelectable.SelectionState GetParentState()
	{
		if (!_selectableParent.IsInteractable())
		{
			return IFHSelectable.SelectionState.Disabled;
		}
		if (_selectableParent.IsSelected())
		{
			return IFHSelectable.SelectionState.Selected;
		}
		if (_selectableParent.IsHighlighted())
		{
			return IFHSelectable.SelectionState.Highlighted;
		}
		return IFHSelectable.SelectionState.Normal;
	}
}

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuOption : MonoBehaviour
{
	[Header("Highlight")]
	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private Color highlightColor;

	[SerializeField]
	private float unhighlightDuration = 0.1f;

	[Header("Select")]
	[SerializeField]
	private GUIAnimator selectAnimation;

	[SerializeField]
	private LoopAnimator selectedAnimation;

	protected bool isHovered;

	private Action<bool> onHovered;

	private Action onSelected;

	private Action onDeselected;

	protected bool isSelected;

	private Tween unhighlightAnimation;

	public bool IsSelected => isSelected;

	public virtual void Init(Action onSelected, Action onDeselected = null, Action<bool> onHovered = null, bool isSelected = false)
	{
		this.onHovered = onHovered;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		CancelSelectedAnimation();
		CancelHighlightAnimations();
		OnHovered(hovered: false);
		SetSelected(isSelected);
	}

	public virtual void SetSelected(bool isSelected)
	{
		this.isSelected = isSelected;
		if (!isSelected)
		{
			CancelSelectedAnimation();
		}
		RefreshHighlight();
	}

	protected virtual void OnHovered(bool hovered)
	{
		isHovered = hovered;
		if (!isSelected)
		{
			RefreshHighlight();
		}
		onHovered?.Invoke(hovered);
	}

	public virtual void Select()
	{
		if (!isSelected)
		{
			SetSelected(isSelected: true);
			selectAnimation.Play();
			onSelected?.Invoke();
		}
	}

	public virtual void Deselect()
	{
		if (isSelected)
		{
			CancelSelectedAnimation();
			SetSelected(isSelected: false);
			onDeselected?.Invoke();
		}
	}

	protected void ToggleSelection(bool isOn)
	{
		if (isOn)
		{
			Select();
		}
		else
		{
			Deselect();
		}
	}

	protected void RefreshHighlight()
	{
		RefreshHighlight(isHovered || isSelected);
	}

	protected virtual void RefreshHighlight(bool isHighlighted)
	{
		if (isHighlighted)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			unhighlightAnimation = highlightImage.DOFade(0f, unhighlightDuration).SetUpdate(isIndependentUpdate: true).OnComplete(() =>
			{
				unhighlightAnimation = null;
			});
		}
	}

	private void CancelSelectedAnimation()
	{
		selectAnimation.Stop();
		selectedAnimation.StopLoop();
		selectAnimation.GoInitState();
	}

	private void CancelHighlightAnimations()
	{
		if (unhighlightAnimation != null)
		{
			unhighlightAnimation.Kill();
			unhighlightAnimation = null;
			highlightImage.SetAlpha(0f);
		}
	}

	protected virtual void OnDisable()
	{
		CancelHighlightAnimations();
		CancelSelectedAnimation();
	}
}

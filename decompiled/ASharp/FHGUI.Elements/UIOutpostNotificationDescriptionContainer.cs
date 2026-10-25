using DG.Tweening;
using FHGUI.DTO.Outpost;
using FHGUI.Elements.Notifications;
using UnityEngine;

namespace FHGUI.Elements;

public class UIOutpostNotificationDescriptionContainer : ListContainerBase<UIOutpostNotificationDescriptionElement, UIOutpostNotificationElement.OutpostNotificationParams, OutpostNotificationDTO>
{
	[SerializeField]
	private UICarousel _carousel;

	[SerializeField]
	private RectTransform _carouselContainer;

	[SerializeField]
	private CanvasGroup _carouselContainerCanvas;

	[Header("Animation")]
	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private float _showDuration = 0.35f;

	[SerializeField]
	private float _hideDuration = 0.35f;

	[SerializeField]
	private float _animationDelay = 0.02f;

	[SerializeField]
	private Vector2 _collapsedPos;

	[SerializeField]
	private Vector2 _expandedPos;

	private Tween _tween;

	public bool IsActive { get; private set; }

	protected override void AfterInit_Impl(UIOutpostNotificationElement.OutpostNotificationParams listParams)
	{
		base.AfterInit_Impl(listParams);
		_carousel.Init();
		_carousel.SetPage(0);
		if (IsActive)
		{
			EnableInteraction();
		}
		else
		{
			DisableInteraction();
		}
	}

	protected override void InitElement(UIOutpostNotificationDescriptionElement element, OutpostNotificationDTO data, UIOutpostNotificationElement.OutpostNotificationParams listParams)
	{
		element.Init(data, (OutpostNotificationDTO dto) =>
		{
			if (listParams.CanDismiss(dto))
			{
				listParams.OnDismiss?.Invoke(dto);
			}
		});
	}

	private void OnDisable()
	{
		_tween?.Kill();
		_tween = null;
		IsActive = true;
		HideCarousel(immediate: true);
	}

	public void ResetState()
	{
		_carousel.SetPage(0);
	}

	public void ShowCarousel(bool immediate = false)
	{
		if (!IsActive)
		{
			IsActive = true;
			if (immediate)
			{
				_carouselContainerCanvas.alpha = 1f;
				_container.anchoredPosition = _expandedPos;
			}
			else
			{
				_carouselContainerCanvas.DOFade(1f, _hideDuration);
				AnimateTo(_expandedPos, _showDuration, Ease.OutCubic, _animationDelay, EnableInteraction);
			}
		}
	}

	public void HideCarousel(bool immediate = false)
	{
		if (IsActive)
		{
			IsActive = false;
			if (immediate)
			{
				_carouselContainerCanvas.alpha = 0.5f;
				_container.anchoredPosition = _collapsedPos;
			}
			else
			{
				_carouselContainerCanvas.DOFade(0.5f, _hideDuration);
				AnimateTo(_collapsedPos, _hideDuration, Ease.InCubic, _animationDelay, DisableInteraction);
			}
		}
	}

	public void ToggleCarousel(bool immediate = false)
	{
		if (IsActive)
		{
			HideCarousel(immediate);
		}
		else
		{
			ShowCarousel(immediate);
		}
	}

	private void AnimateTo(Vector2 target, float duration, Ease ease, float delay = 0f, TweenCallback onDone = null)
	{
		_tween?.Kill();
		float num = Vector2.Distance(_collapsedPos, _expandedPos);
		float num2 = Vector2.Distance(_container.anchoredPosition, target);
		float duration2 = duration * (num2 / num);
		_tween = _container.DOAnchorPos(target, duration2).SetEase(ease);
		if (delay > 0f)
		{
			_tween.SetDelay(delay);
		}
		if (onDone != null)
		{
			_tween.OnComplete(onDone);
		}
	}

	private void DisableInteraction()
	{
		SetInteractable(interactable: false);
	}

	private void EnableInteraction()
	{
		SetInteractable(interactable: true);
	}

	private void SetInteractable(bool interactable)
	{
		_carouselContainerCanvas.interactable = interactable;
	}
}

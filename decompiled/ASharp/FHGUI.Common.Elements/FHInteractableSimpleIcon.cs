using FHGUI.DTO;
using FHGUI.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common.Elements;

public class FHInteractableSimpleIcon : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private FHInteractableElement _interactContainer;

	private UISimpleInteractionEvents _interactions;

	private object _additionalData;

	private void Awake()
	{
		_interactContainer.Init(new UISimpleInteractionEvents
		{
			OnClick = (object _) =>
			{
				_interactions?.OnClick?.Invoke(_additionalData);
			},
			OnRightClick = (object _) =>
			{
				_interactions?.OnRightClick?.Invoke(_additionalData);
			},
			OnHover = (object _, bool raised) =>
			{
				_interactions?.OnHover?.Invoke(_additionalData, raised);
			},
			OnDoubleClick = (object _) =>
			{
				_interactions?.OnDoubleClick?.Invoke(_additionalData);
			}
		});
	}

	private void OnDisable()
	{
		_interactions = null;
	}

	public void Init(SpriteWithTooltipDTO dto, UISimpleInteractionEvents interactions = null, object additionalData = null)
	{
		_icon.sprite = dto.Sprite;
		_icon.color = dto.Tint;
		_additionalData = additionalData;
		_interactions = interactions;
	}
}

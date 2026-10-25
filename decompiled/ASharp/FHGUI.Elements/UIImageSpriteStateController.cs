using System;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

[RequireComponent(typeof(Image))]
public class UIImageSpriteStateController : MonoBehaviour
{
	[SerializeField]
	private SpriteState _spriteState;

	private Image _image;

	private IFHSelectable.SelectionState _state;

	public void SetState(IFHSelectable.SelectionState selectionState)
	{
		if (_image == null)
		{
			_image = GetComponent<Image>();
		}
		_state = selectionState;
		Image image = _image;
		image.overrideSprite = selectionState switch
		{
			IFHSelectable.SelectionState.Normal => null, 
			IFHSelectable.SelectionState.Highlighted => _spriteState.highlightedSprite, 
			IFHSelectable.SelectionState.Pressed => _spriteState.pressedSprite, 
			IFHSelectable.SelectionState.Selected => _spriteState.selectedSprite, 
			IFHSelectable.SelectionState.Disabled => _spriteState.disabledSprite, 
			_ => throw new ArgumentOutOfRangeException("selectionState", selectionState, null), 
		};
	}

	private void Start()
	{
		SetState(IFHSelectable.SelectionState.Normal);
	}
}

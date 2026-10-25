using System;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

[RequireComponent(typeof(Graphic))]
public class UIElementStateActivatorColor : MonoBehaviour, IUIElementStateListener
{
	[Serializable]
	public struct ColorState
	{
		public string State;

		public Color Color;
	}

	[SerializeField]
	private ColorState[] _colorStates;

	private Graphic _graphic;

	private Color _defaultColor;

	private void Awake()
	{
		_graphic = GetComponent<Graphic>();
		_defaultColor = _graphic.color;
	}

	public void StateChanged(string state)
	{
		for (int i = 0; i < _colorStates.Length; i++)
		{
			ColorState colorState = _colorStates[i];
			if (string.Compare(state, colorState.State, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				_graphic.color = colorState.Color;
				break;
			}
		}
	}

	public void ResetState()
	{
		_graphic.color = _defaultColor;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverGraphicEffects : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Serializable]
	public class HoverGraphicEffect
	{
		public Graphic Graphic;

		public Color HoverColor;

		public bool UseOnlyAlpha;

		private Color _originalColor;

		public void SetOriginalColor(Color originalColor)
		{
			_originalColor = originalColor;
		}

		public void Revert()
		{
			if (Graphic != null)
			{
				Graphic.color = _originalColor;
			}
		}
	}

	[SerializeField]
	private List<HoverGraphicEffect> _hoverGraphics = new List<HoverGraphicEffect>(10);

	private void OnEnable()
	{
		for (int i = 0; i < _hoverGraphics.Count; i++)
		{
			HoverGraphicEffect hoverGraphicEffect = _hoverGraphics[i];
			if (!(hoverGraphicEffect.Graphic == null))
			{
				_hoverGraphics[i].SetOriginalColor(hoverGraphicEffect.Graphic.color);
			}
		}
	}

	private void OnDisable()
	{
		foreach (HoverGraphicEffect hoverGraphic in _hoverGraphics)
		{
			if (!(hoverGraphic.Graphic == null))
			{
				hoverGraphic.Revert();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		foreach (HoverGraphicEffect hoverGraphic in _hoverGraphics)
		{
			Graphic graphic = hoverGraphic.Graphic;
			if (!(graphic == null))
			{
				if (hoverGraphic.UseOnlyAlpha)
				{
					graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, hoverGraphic.HoverColor.a);
				}
				else
				{
					graphic.color = hoverGraphic.HoverColor;
				}
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		foreach (HoverGraphicEffect hoverGraphic in _hoverGraphics)
		{
			hoverGraphic.Revert();
		}
	}

	private void OnValidate()
	{
		foreach (HoverGraphicEffect hoverGraphic in _hoverGraphics)
		{
			if (hoverGraphic.Graphic == null)
			{
				Log.GUIError(name + " has UIHoverGraphicEffects with missing graphics", this);
			}
		}
	}
}

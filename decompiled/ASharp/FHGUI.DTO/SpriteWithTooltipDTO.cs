using UnityEngine;

namespace FHGUI.DTO;

public class SpriteWithTooltipDTO
{
	public string Title;

	public string Description;

	public string TextValue;

	public Sprite Sprite;

	public Material SpriteMaterial;

	public Color Tint = Color.white;

	public object AdditionalData;
}

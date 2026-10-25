using EPOOutline;
using FHRL;
using UnityEngine;

namespace Logging.AdventureMap.OutpostVillage;

public class OutpostBuildingSlotVisuals : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private GameObject _winterVariant;

	[SerializeField]
	private GameObject _summerVariant;

	[Header("Settings")]
	[SerializeField]
	private bool _ignoreHighlights;

	[SerializeField]
	private float _VerticalGradientMax = -0.32f;

	[SerializeField]
	private float _VerticalGradientMin = -0.79f;

	[Tooltip("White list of eligible renderers. Leave empty to include all renderers")]
	[SerializeField]
	private MeshRenderer[] _exclusiveRenderers;

	private MeshRenderer[] _renderers;

	private Outlinable[] _outlines;

	private MaterialPropertyBlock _propertyBlock;

	private static readonly int _gradientColorID = Shader.PropertyToID("_HGradientColor");

	private static readonly int _stampOpacityID = Shader.PropertyToID("_StampOpacity");

	private readonly Color32 _colorOutlineNormal = new Color32(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private readonly Color32 _colorOutlineWrecked = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	private readonly Color32 _colorInnerFillNormal = new Color32(0, 200, 200, byte.MaxValue);

	private readonly Color32 _colorInnerFillWrecked = new Color32(96, 0, 0, byte.MaxValue);

	private readonly Color32 _colorInnerFillNone = new Color32(0, 0, 0, byte.MaxValue);

	private bool _isSelected;

	private bool _isHovered;

	private bool _isWrecked;

	private void Awake()
	{
		_propertyBlock = new MaterialPropertyBlock();
		if (_exclusiveRenderers != null && _exclusiveRenderers.Length != 0)
		{
			_renderers = _exclusiveRenderers;
		}
		else
		{
			_renderers = GetComponentsInChildren<MeshRenderer>();
		}
		_outlines = GetComponentsInChildren<Outlinable>(includeInactive: true);
		MeshRenderer[] renderers = _renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].SetPropertyBlock(_propertyBlock);
		}
	}

	public void Init(CalendarSeason season)
	{
		gameObject.SetActive(value: true);
		if (_winterVariant != null)
		{
			_winterVariant.SetActive(season == CalendarSeason.Winter);
		}
		if (_summerVariant != null)
		{
			_summerVariant.SetActive(season == CalendarSeason.Summer);
		}
		enabled = true;
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	public void SetSelectedState(bool selected)
	{
		_isSelected = selected;
		enabled = true;
	}

	public void SetHoveredState(bool hovered)
	{
		_isHovered = hovered;
		enabled = true;
	}

	public void SetWreckedState(bool wreck)
	{
		_isWrecked = wreck;
		enabled = true;
	}

	private void UpdateVisuals()
	{
		bool flag = _isHovered || _isSelected;
		Outlinable[] outlines = _outlines;
		foreach (Outlinable outlinable in outlines)
		{
			outlinable.enabled = flag;
			if (outlinable.enabled)
			{
				outlinable.OutlineParameters.Color = (_isWrecked ? _colorOutlineWrecked : _colorOutlineNormal);
			}
		}
		_propertyBlock.Clear();
		Color32 color;
		if (_ignoreHighlights || !flag)
		{
			color = _colorInnerFillNone;
		}
		else
		{
			color = (_isWrecked ? _colorInnerFillWrecked : _colorInnerFillNormal);
		}
		Color value = color;
		_propertyBlock.SetColor(_gradientColorID, value);
		_propertyBlock.SetFloat(_stampOpacityID, _isWrecked ? 1 : 0);
		MeshRenderer[] renderers = _renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].SetPropertyBlock(_propertyBlock);
		}
	}

	private void LateUpdate()
	{
		UpdateVisuals();
		enabled = false;
	}
}

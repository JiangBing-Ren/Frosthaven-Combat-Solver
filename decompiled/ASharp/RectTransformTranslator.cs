using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(MeshRenderer), typeof(MeshFilter))]
public class RectTransformTranslator : MonoBehaviour
{
	private RectTransform _rectTransform;

	private MeshRenderer _renderer;

	private MeshFilter _filter;

	private List<Material> _materialsToDispose = new List<Material>();

	private void OnEnable()
	{
		if ((object)_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		_renderer = GetComponent<MeshRenderer>();
		_filter = GetComponent<MeshFilter>();
	}

	private void Update()
	{
		ApplyTransformToRenderer();
	}

	private void ApplyTransformToRenderer()
	{
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using SETUtil.Extend;
using SETUtil.Materials;
using UnityEngine;

namespace FH.LevelComponents.ProcGen;

public class OccludingRendererGroup : MonoBehaviour, IWallOccluder, IOccluder<WallOcclusionCheckResult>
{
	private List<MaterialModification> _renderers = new List<MaterialModification>();

	private List<Material[]> _animatedMaterials = new List<Material[]>();

	private List<bool[]> _validMaterials = new List<bool[]>();

	private float _targetFadeValue = 1f;

	private float _lerpedFadeValue = 1f;

	private bool _isFaded;

	private bool _isAnimating;

	private ProceduralWall wall;

	private Action _destructionCallback;

	private static readonly int FadeHeightProperty = Shader.PropertyToID("_FadeHeight");

	private const float InterpolationSpeed = 2f;

	private const float VisibleValue = 1f;

	private const float FadedValue = 0f;

	public Vector2 Position => new Vector2(transform.position.x, transform.position.z);

	public Vector2 Forward { get; protected set; }

	public Vector2 LeftCorner
	{
		get
		{
			Vector3 vector = ((wall == null) ? transform.position : wall.LeftCorner.Position);
			return new Vector2(vector.x, vector.z);
		}
	}

	public Vector2 RightCorner
	{
		get
		{
			Vector3 vector = ((wall == null) ? transform.position : wall.RightCorner.Position);
			return new Vector2(vector.x, vector.z);
		}
	}

	public void SetOcclusionActive(bool active)
	{
		enabled = active;
	}

	private void Update()
	{
		if (_isAnimating)
		{
			_lerpedFadeValue = Mathf.Lerp(_lerpedFadeValue, _targetFadeValue, 2f * Time.deltaTime);
			for (int i = 0; i < _renderers.Count; i++)
			{
				SetFadeHeight(_animatedMaterials[i], _validMaterials[i], _lerpedFadeValue);
			}
			if (Mathf.Approximately(_lerpedFadeValue, 1f))
			{
				_lerpedFadeValue = 1f;
				_isAnimating = false;
				RemoveNullRenderers();
				RestoreMaterials(_renderers);
			}
		}
		else if (_isFaded)
		{
			RemoveNullRenderers();
			ReplaceMaterials(_renderers, _animatedMaterials);
			_isAnimating = true;
		}
	}

	private void RestoreMaterials(List<MaterialModification> renderers)
	{
		foreach (MaterialModification renderer in renderers)
		{
			if (renderer != null)
			{
				renderer.Restore();
			}
		}
	}

	protected virtual void OnDisable()
	{
		_lerpedFadeValue = 1f;
		_isAnimating = false;
		RestoreMaterials(_renderers);
	}

	protected virtual void OnDestroy()
	{
		if (Application.isPlaying)
		{
			CleanUp();
			_destructionCallback?.Invoke();
		}
	}

	public void SetOnDestroyCallback(Action callback)
	{
		_destructionCallback = callback;
	}

	public void OnUpdatedOcclusionCheck(WallOcclusionCheckResult occlusionResult)
	{
		_isFaded = occlusionResult >= WallOcclusionCheckResult.HitCenter;
		_targetFadeValue = (_isFaded ? 0f : 1f);
	}

	public void Setup(Vector2 occlusionDirection = default(Vector2), List<Renderer> renderers = null, bool includeChildRenderers = true)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		CleanUp();
		if (wall == null)
		{
			wall = GetComponent<ProceduralWall>();
		}
		Forward = occlusionDirection;
		if (includeChildRenderers)
		{
			_renderers.AddRange(from a in gameObject.GetComponentsInChildren<Renderer>(includeInactive: true)
				select a.GetOrAdd<MaterialModification>());
		}
		if (renderers != null)
		{
			_renderers.AddRange(from a in renderers
				where a != null
				select a.GetOrAdd<MaterialModification>());
		}
		for (int num = 0; num < _renderers.Count; num++)
		{
			Material[] clones = _renderers[num].GetClones();
			_animatedMaterials.Add(clones);
			bool[] array = new bool[clones.Length];
			for (int num2 = 0; num2 < clones.Length; num2++)
			{
				array[num2] = clones[num2] != null;
			}
			_validMaterials.Add(array);
		}
	}

	private static void SetFadeHeight(Material[] materials, bool[] validIndices, float height)
	{
		for (int i = 0; i < materials.Length; i++)
		{
			if (validIndices[i])
			{
				materials[i].SetFloat(FadeHeightProperty, height);
			}
		}
	}

	private static void ReplaceMaterials(List<MaterialModification> renderers, List<Material[]> newMaterials)
	{
		for (int i = 0; i < renderers.Count; i++)
		{
			if ((bool)renderers[i])
			{
				renderers[i].SetMaterials(newMaterials[i]);
			}
		}
	}

	private void CleanUp()
	{
		for (int i = 0; i < _renderers.Count; i++)
		{
			if ((bool)_renderers[i])
			{
				_renderers[i].Restore();
			}
			_validMaterials[i] = null;
		}
		_renderers.Clear();
		_animatedMaterials.Clear();
		_validMaterials.Clear();
	}

	private void RemoveNullRenderers()
	{
		for (int num = _renderers.Count - 1; num >= 0; num--)
		{
			if (_renderers[num] == null)
			{
				_renderers.RemoveAt(num);
				_animatedMaterials.RemoveAt(num);
				_validMaterials.RemoveAt(num);
			}
		}
	}
}

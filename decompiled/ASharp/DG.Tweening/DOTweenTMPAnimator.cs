using System;
using System.Collections.Generic;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace DG.Tweening;

public class DOTweenTMPAnimator : IDisposable
{
	private struct CharVertices(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
	{
		public Vector3 bottomLeft = bottomLeft;

		public Vector3 topLeft = topLeft;

		public Vector3 topRight = topRight;

		public Vector3 bottomRight = bottomRight;
	}

	private struct CharTransform
	{
		public int charIndex;

		public Vector3 offset;

		public Quaternion rotation;

		public Vector3 scale;

		private Vector3 _topLeftShift;

		private Vector3 _topRightShift;

		private Vector3 _bottomLeftShift;

		private Vector3 _bottomRightShift;

		private Vector3 _charMidBaselineOffset;

		private int _matIndex;

		private int _firstVertexIndex;

		private TMP_MeshInfo _meshInfo;

		public bool isVisible { get; private set; }

		public CharTransform(int charIndex, TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos)
		{
			this = default;
			this.charIndex = charIndex;
			offset = Vector3.zero;
			rotation = Quaternion.identity;
			scale = Vector3.one;
			Refresh(textInfo, cachedMeshInfos);
		}

		public void Refresh(TMP_TextInfo textInfo, TMP_MeshInfo[] cachedMeshInfos)
		{
			TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[charIndex];
			bool flag = tMP_CharacterInfo.character == ' ';
			isVisible = tMP_CharacterInfo.isVisible && !flag;
			_matIndex = tMP_CharacterInfo.materialReferenceIndex;
			_firstVertexIndex = tMP_CharacterInfo.vertexIndex;
			_meshInfo = textInfo.meshInfo[_matIndex];
			Vector3[] vertices = cachedMeshInfos[_matIndex].vertices;
			_charMidBaselineOffset = (flag ? Vector3.zero : ((vertices[_firstVertexIndex] + vertices[_firstVertexIndex + 2]) * 0.5f));
		}

		public void ResetAll(TMP_Text target, TMP_MeshInfo[] meshInfos, TMP_MeshInfo[] cachedMeshInfos)
		{
			ResetGeometry(target, cachedMeshInfos);
			ResetColors(target, meshInfos);
		}

		public void ResetTransformationData()
		{
			offset = Vector3.zero;
			rotation = Quaternion.identity;
			scale = Vector3.one;
			_topLeftShift = (_topRightShift = (_bottomLeftShift = (_bottomRightShift = Vector3.zero)));
		}

		public void ResetGeometry(TMP_Text target, TMP_MeshInfo[] cachedMeshInfos)
		{
			ResetTransformationData();
			Vector3[] vertices = _meshInfo.vertices;
			Vector3[] vertices2 = cachedMeshInfos[_matIndex].vertices;
			vertices[_firstVertexIndex] = vertices2[_firstVertexIndex];
			vertices[_firstVertexIndex + 1] = vertices2[_firstVertexIndex + 1];
			vertices[_firstVertexIndex + 2] = vertices2[_firstVertexIndex + 2];
			vertices[_firstVertexIndex + 3] = vertices2[_firstVertexIndex + 3];
			_meshInfo.mesh.vertices = _meshInfo.vertices;
			target.UpdateGeometry(_meshInfo.mesh, _matIndex);
		}

		public void ResetColors(TMP_Text target, TMP_MeshInfo[] meshInfos)
		{
			Color color = target.color;
			Color32[] colors = meshInfos[_matIndex].colors32;
			colors[_firstVertexIndex] = color;
			colors[_firstVertexIndex + 1] = color;
			colors[_firstVertexIndex + 2] = color;
			colors[_firstVertexIndex + 3] = color;
			target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}

		public Color32 GetColor(TMP_MeshInfo[] meshInfos)
		{
			return meshInfos[_matIndex].colors32[_firstVertexIndex];
		}

		public CharVertices GetVertices()
		{
			return new CharVertices(_meshInfo.vertices[_firstVertexIndex], _meshInfo.vertices[_firstVertexIndex + 1], _meshInfo.vertices[_firstVertexIndex + 2], _meshInfo.vertices[_firstVertexIndex + 3]);
		}

		public void UpdateAlpha(TMP_Text target, Color alphaColor, TMP_MeshInfo[] meshInfos, bool apply = true)
		{
			byte a = (byte)(alphaColor.a * 255f);
			Color32[] colors = meshInfos[_matIndex].colors32;
			colors[_firstVertexIndex].a = a;
			colors[_firstVertexIndex + 1].a = a;
			colors[_firstVertexIndex + 2].a = a;
			colors[_firstVertexIndex + 3].a = a;
			if (apply)
			{
				target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
			}
		}

		public void UpdateColor(TMP_Text target, Color32 color, TMP_MeshInfo[] meshInfos, bool apply = true)
		{
			Color32[] colors = meshInfos[_matIndex].colors32;
			colors[_firstVertexIndex] = color;
			colors[_firstVertexIndex + 1] = color;
			colors[_firstVertexIndex + 2] = color;
			colors[_firstVertexIndex + 3] = color;
			if (apply)
			{
				target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
			}
		}

		public void UpdateGeometry(TMP_Text target, Vector3 offset, Quaternion rotation, Vector3 scale, TMP_MeshInfo[] cachedMeshInfos, bool apply = true)
		{
			this.offset = offset;
			this.rotation = rotation;
			this.scale = scale;
			if (apply)
			{
				Vector3[] vertices = _meshInfo.vertices;
				Vector3[] vertices2 = cachedMeshInfos[_matIndex].vertices;
				vertices[_firstVertexIndex] = vertices2[_firstVertexIndex] - _charMidBaselineOffset;
				vertices[_firstVertexIndex + 1] = vertices2[_firstVertexIndex + 1] - _charMidBaselineOffset;
				vertices[_firstVertexIndex + 2] = vertices2[_firstVertexIndex + 2] - _charMidBaselineOffset;
				vertices[_firstVertexIndex + 3] = vertices2[_firstVertexIndex + 3] - _charMidBaselineOffset;
				Matrix4x4 matrix4x = Matrix4x4.TRS(this.offset, this.rotation, this.scale);
				vertices[_firstVertexIndex] = matrix4x.MultiplyPoint3x4(vertices[_firstVertexIndex]) + _charMidBaselineOffset + _bottomLeftShift;
				vertices[_firstVertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices[_firstVertexIndex + 1]) + _charMidBaselineOffset + _topLeftShift;
				vertices[_firstVertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices[_firstVertexIndex + 2]) + _charMidBaselineOffset + _topRightShift;
				vertices[_firstVertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices[_firstVertexIndex + 3]) + _charMidBaselineOffset + _bottomRightShift;
				_meshInfo.mesh.vertices = _meshInfo.vertices;
				target.UpdateGeometry(_meshInfo.mesh, _matIndex);
			}
		}

		public void ShiftVertices(TMP_Text target, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
		{
			_topLeftShift += topLeftShift;
			_topRightShift += topRightShift;
			_bottomLeftShift += bottomLeftShift;
			_bottomRightShift += bottomRightShift;
			Vector3[] vertices = _meshInfo.vertices;
			vertices[_firstVertexIndex] += _bottomLeftShift;
			vertices[_firstVertexIndex + 1] = vertices[_firstVertexIndex + 1] + _topLeftShift;
			vertices[_firstVertexIndex + 2] = vertices[_firstVertexIndex + 2] + _topRightShift;
			vertices[_firstVertexIndex + 3] = vertices[_firstVertexIndex + 3] + _bottomRightShift;
			_meshInfo.mesh.vertices = _meshInfo.vertices;
			target.UpdateGeometry(_meshInfo.mesh, _matIndex);
		}

		public void ResetVerticesShift(TMP_Text target)
		{
			Vector3[] vertices = _meshInfo.vertices;
			vertices[_firstVertexIndex] -= _bottomLeftShift;
			vertices[_firstVertexIndex + 1] = vertices[_firstVertexIndex + 1] - _topLeftShift;
			vertices[_firstVertexIndex + 2] = vertices[_firstVertexIndex + 2] - _topRightShift;
			vertices[_firstVertexIndex + 3] = vertices[_firstVertexIndex + 3] - _bottomRightShift;
			_meshInfo.mesh.vertices = _meshInfo.vertices;
			target.UpdateGeometry(_meshInfo.mesh, _matIndex);
			_topLeftShift = (_topRightShift = (_bottomLeftShift = (_bottomRightShift = Vector3.zero)));
		}
	}

	private static readonly Dictionary<TMP_Text, DOTweenTMPAnimator> _targetToAnimator = new Dictionary<TMP_Text, DOTweenTMPAnimator>();

	private readonly List<CharTransform> _charTransforms = new List<CharTransform>();

	private TMP_MeshInfo[] _cachedMeshInfos;

	private bool _ignoreTextChangedEvent;

	public TMP_Text target { get; private set; }

	public TMP_TextInfo textInfo { get; private set; }

	public DOTweenTMPAnimator(TMP_Text target)
	{
		if (target == null)
		{
			Debugger.LogError("DOTweenTMPAnimator target can't be null");
			return;
		}
		if (!target.gameObject.activeInHierarchy)
		{
			Debugger.LogError("You can't create a DOTweenTMPAnimator if its target is disabled");
			return;
		}
		if (_targetToAnimator.ContainsKey(target))
		{
			if (Debugger.logPriority >= 2)
			{
				Debugger.Log($"A DOTweenTMPAnimator for \"{target}\" already exists: disposing it because you can't have more than one DOTweenTMPAnimator for the same TextMesh Pro object. If you have tweens running on the disposed DOTweenTMPAnimator you should kill them manually");
			}
			_targetToAnimator[target].Dispose();
			_targetToAnimator.Remove(target);
		}
		this.target = target;
		_targetToAnimator.Add(target, this);
		Refresh();
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
	}

	public static void DisposeInstanceFor(TMP_Text target)
	{
		if (_targetToAnimator.ContainsKey(target))
		{
			_targetToAnimator[target].Dispose();
			_targetToAnimator.Remove(target);
		}
	}

	public void Dispose()
	{
		target = null;
		_charTransforms.Clear();
		textInfo = null;
		_cachedMeshInfos = null;
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
	}

	public void Refresh()
	{
		_ignoreTextChangedEvent = true;
		target.ForceMeshUpdate(ignoreActiveState: true);
		textInfo = target.textInfo;
		_cachedMeshInfos = textInfo.CopyMeshInfoVertexData();
		int characterCount = textInfo.characterCount;
		int num = _charTransforms.Count;
		if (num > characterCount)
		{
			_charTransforms.RemoveRange(characterCount, num - characterCount);
			num = characterCount;
		}
		for (int i = 0; i < num; i++)
		{
			CharTransform value = _charTransforms[i];
			value.ResetTransformationData();
			value.Refresh(textInfo, _cachedMeshInfos);
			_charTransforms[i] = value;
		}
		for (int j = num; j < characterCount; j++)
		{
			_charTransforms.Add(new CharTransform(j, textInfo, _cachedMeshInfos));
		}
		_ignoreTextChangedEvent = false;
	}

	public void Reset()
	{
		int count = _charTransforms.Count;
		for (int i = 0; i < count; i++)
		{
			_charTransforms[i].ResetAll(target, textInfo.meshInfo, _cachedMeshInfos);
		}
	}

	private void OnTextChanged(UnityEngine.Object obj)
	{
		if (!_ignoreTextChangedEvent && !(target == null) && !(obj != target))
		{
			Refresh();
		}
	}

	private bool ValidateChar(int charIndex, bool isTween = true)
	{
		if (textInfo.characterCount <= charIndex)
		{
			Debugger.LogError($"CharIndex {charIndex} doesn't exist");
			return false;
		}
		if (!textInfo.characterInfo[charIndex].isVisible)
		{
			if (Debugger.logPriority > 1)
			{
				if (isTween)
				{
					Debugger.Log($"CharIndex {charIndex} isn't visible, ignoring it and returning an empty tween (TextMesh Pro will behave weirdly if invisible chars are included in the animation)");
				}
				else
				{
					Debugger.Log($"CharIndex {charIndex} isn't visible, ignoring it");
				}
			}
			return false;
		}
		return true;
	}

	private bool ValidateSpan(int fromCharIndex, int toCharIndex, out int firstVisibleCharIndex, out int lastVisibleCharIndex)
	{
		firstVisibleCharIndex = -1;
		lastVisibleCharIndex = -1;
		int characterCount = textInfo.characterCount;
		if (fromCharIndex >= characterCount)
		{
			return false;
		}
		if (toCharIndex >= characterCount)
		{
			toCharIndex = characterCount - 1;
		}
		for (int i = fromCharIndex; i < toCharIndex + 1; i++)
		{
			if (_charTransforms[i].isVisible)
			{
				firstVisibleCharIndex = i;
				break;
			}
		}
		if (firstVisibleCharIndex == -1)
		{
			return false;
		}
		for (int num = toCharIndex; num > firstVisibleCharIndex - 1; num--)
		{
			if (_charTransforms[num].isVisible)
			{
				lastVisibleCharIndex = num;
				break;
			}
		}
		if (lastVisibleCharIndex == -1)
		{
			return false;
		}
		return true;
	}

	public void SkewSpanX(int fromCharIndex, int toCharIndex, float skewFactor, bool skewTop = true)
	{
		if (!ValidateSpan(fromCharIndex, toCharIndex, out var firstVisibleCharIndex, out var lastVisibleCharIndex))
		{
			return;
		}
		for (int i = firstVisibleCharIndex; i < lastVisibleCharIndex + 1; i++)
		{
			if (_charTransforms[i].isVisible)
			{
				_charTransforms[i].GetVertices();
				SkewCharX(i, skewFactor, skewTop);
			}
		}
	}

	public void SkewSpanY(int fromCharIndex, int toCharIndex, float skewFactor, TMPSkewSpanMode mode = TMPSkewSpanMode.Default, bool skewRight = true)
	{
		if (!ValidateSpan(fromCharIndex, toCharIndex, out var firstVisibleCharIndex, out var lastVisibleCharIndex))
		{
			return;
		}
		if (mode == TMPSkewSpanMode.AsMaxSkewFactor)
		{
			CharVertices vertices = _charTransforms[firstVisibleCharIndex].GetVertices();
			CharVertices vertices2 = _charTransforms[lastVisibleCharIndex].GetVertices();
			float num = Mathf.Abs(vertices2.bottomRight.x - vertices.bottomLeft.x);
			float num2 = Mathf.Abs(vertices2.topRight.y - vertices2.bottomRight.y) / num;
			skewFactor *= num2;
		}
		float num3 = 0f;
		CharVertices charVertices = default;
		float num4 = 0f;
		if (skewRight)
		{
			for (int i = firstVisibleCharIndex; i < lastVisibleCharIndex + 1; i++)
			{
				if (_charTransforms[i].isVisible)
				{
					CharVertices vertices3 = _charTransforms[i].GetVertices();
					float num5 = SkewCharY(i, skewFactor, skewRight);
					if (i > firstVisibleCharIndex)
					{
						float num6 = Mathf.Abs(charVertices.bottomLeft.x - charVertices.bottomRight.x);
						float num7 = Mathf.Abs(vertices3.bottomLeft.x - charVertices.bottomRight.x);
						num3 += num4 + num4 * num7 / num6;
						SetCharOffset(i, new Vector3(0f, _charTransforms[i].offset.y + num3, 0f));
					}
					charVertices = vertices3;
					num4 = num5;
				}
			}
			return;
		}
		for (int num8 = lastVisibleCharIndex; num8 > firstVisibleCharIndex - 1; num8--)
		{
			if (_charTransforms[num8].isVisible)
			{
				CharVertices vertices4 = _charTransforms[num8].GetVertices();
				float num9 = SkewCharY(num8, skewFactor, skewRight);
				if (num8 < lastVisibleCharIndex)
				{
					float num10 = Mathf.Abs(charVertices.bottomLeft.x - charVertices.bottomRight.x);
					float num11 = Mathf.Abs(vertices4.bottomRight.x - charVertices.bottomLeft.x);
					num3 += num4 + num4 * num11 / num10;
					SetCharOffset(num8, new Vector3(0f, _charTransforms[num8].offset.y + num3, 0f));
				}
				charVertices = vertices4;
				num4 = num9;
			}
		}
	}

	public Color GetCharColor(int charIndex)
	{
		if (!ValidateChar(charIndex))
		{
			return Color.white;
		}
		return _charTransforms[charIndex].GetColor(textInfo.meshInfo);
	}

	public Vector3 GetCharOffset(int charIndex)
	{
		if (!ValidateChar(charIndex))
		{
			return Vector3.zero;
		}
		return _charTransforms[charIndex].offset;
	}

	public Vector3 GetCharRotation(int charIndex)
	{
		if (!ValidateChar(charIndex))
		{
			return Vector3.zero;
		}
		return _charTransforms[charIndex].rotation.eulerAngles;
	}

	public Vector3 GetCharScale(int charIndex)
	{
		if (!ValidateChar(charIndex))
		{
			return Vector3.zero;
		}
		return _charTransforms[charIndex].scale;
	}

	public void SetCharColor(int charIndex, Color32 color)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateColor(target, color, textInfo.meshInfo);
			_charTransforms[charIndex] = value;
		}
	}

	public void SetCharOffset(int charIndex, Vector3 offset)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, offset, value.rotation, value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}
	}

	public void SetCharRotation(int charIndex, Vector3 rotation)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, Quaternion.Euler(rotation), value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}
	}

	public void SetCharScale(int charIndex, Vector3 scale)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, value.rotation, scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}
	}

	public void ShiftCharVertices(int charIndex, Vector3 topLeftShift, Vector3 topRightShift, Vector3 bottomLeftShift, Vector3 bottomRightShift)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.ShiftVertices(target, topLeftShift, topRightShift, bottomLeftShift, bottomRightShift);
			_charTransforms[charIndex] = value;
		}
	}

	public float SkewCharX(int charIndex, float skewFactor, bool skewTop = true)
	{
		if (!ValidateChar(charIndex))
		{
			return 0f;
		}
		Vector3 vector = new Vector3(skewFactor, 0f, 0f);
		CharTransform value = _charTransforms[charIndex];
		if (skewTop)
		{
			value.ShiftVertices(target, vector, vector, Vector3.zero, Vector3.zero);
		}
		else
		{
			value.ShiftVertices(target, Vector3.zero, Vector3.zero, vector, vector);
		}
		_charTransforms[charIndex] = value;
		return skewFactor;
	}

	public float SkewCharY(int charIndex, float skewFactor, bool skewRight = true, bool fixedSkew = false)
	{
		if (!ValidateChar(charIndex))
		{
			return 0f;
		}
		float num = (fixedSkew ? skewFactor : (skewFactor * textInfo.characterInfo[charIndex].aspectRatio));
		Vector3 vector = new Vector3(0f, num, 0f);
		CharTransform value = _charTransforms[charIndex];
		if (skewRight)
		{
			value.ShiftVertices(target, Vector3.zero, vector, Vector3.zero, vector);
		}
		else
		{
			value.ShiftVertices(target, vector, Vector3.zero, vector, Vector3.zero);
		}
		_charTransforms[charIndex] = value;
		return num;
	}

	public void ResetVerticesShift(int charIndex)
	{
		if (ValidateChar(charIndex))
		{
			CharTransform value = _charTransforms[charIndex];
			value.ResetVerticesShift(target);
			_charTransforms[charIndex] = value;
		}
	}

	public TweenerCore<Color, Color, ColorOptions> DOFadeChar(int charIndex, float endValue, float duration)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		return DOTween.ToAlpha(() => _charTransforms[charIndex].GetColor(textInfo.meshInfo), (Color x) =>
		{
			_charTransforms[charIndex].UpdateAlpha(target, x, textInfo.meshInfo);
		}, endValue, duration);
	}

	public TweenerCore<Color, Color, ColorOptions> DOColorChar(int charIndex, Color endValue, float duration)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		return DOTween.To(() => _charTransforms[charIndex].GetColor(textInfo.meshInfo), (Color x) =>
		{
			_charTransforms[charIndex].UpdateColor(target, x, textInfo.meshInfo);
		}, endValue, duration);
	}

	public TweenerCore<Vector3, Vector3, VectorOptions> DOOffsetChar(int charIndex, Vector3 endValue, float duration)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		return DOTween.To(() => _charTransforms[charIndex].offset, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, x, value.rotation, value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, endValue, duration);
	}

	public TweenerCore<Quaternion, Vector3, QuaternionOptions> DORotateChar(int charIndex, Vector3 endValue, float duration, RotateMode mode = RotateMode.Fast)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		TweenerCore<Quaternion, Vector3, QuaternionOptions> tweenerCore = DOTween.To(() => _charTransforms[charIndex].rotation, (Quaternion x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, x, value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, endValue, duration);
		tweenerCore.plugOptions.rotateMode = mode;
		return tweenerCore;
	}

	public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, float endValue, float duration)
	{
		return DOScaleChar(charIndex, new Vector3(endValue, endValue, endValue), duration);
	}

	public TweenerCore<Vector3, Vector3, VectorOptions> DOScaleChar(int charIndex, Vector3 endValue, float duration)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		return DOTween.To(() => _charTransforms[charIndex].scale, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, value.rotation, x, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, endValue, duration);
	}

	public Tweener DOPunchCharOffset(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Punch(() => _charTransforms[charIndex].offset, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, x, value.rotation, value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, punch, duration, vibrato, elasticity);
	}

	public Tweener DOPunchCharRotation(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Punch(() => _charTransforms[charIndex].rotation.eulerAngles, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, Quaternion.Euler(x), value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, punch, duration, vibrato, elasticity);
	}

	public Tweener DOPunchCharScale(int charIndex, float punch, float duration, int vibrato = 10, float elasticity = 1f)
	{
		return DOPunchCharScale(charIndex, new Vector3(punch, punch, punch), duration, vibrato, elasticity);
	}

	public Tweener DOPunchCharScale(int charIndex, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Punch(() => _charTransforms[charIndex].scale, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, value.rotation, x, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, punch, duration, vibrato, elasticity);
	}

	public Tweener DOShakeCharOffset(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
	{
		return DOShakeCharOffset(charIndex, duration, new Vector3(strength, strength, strength), vibrato, randomness, fadeOut);
	}

	public Tweener DOShakeCharOffset(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Shake(() => _charTransforms[charIndex].offset, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, x, value.rotation, value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, duration, strength, vibrato, randomness, fadeOut);
	}

	public Tweener DOShakeCharRotation(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Shake(() => _charTransforms[charIndex].rotation.eulerAngles, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, Quaternion.Euler(x), value.scale, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, duration, strength, vibrato, randomness, fadeOut);
	}

	public Tweener DOShakeCharScale(int charIndex, float duration, float strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
	{
		return DOShakeCharScale(charIndex, duration, new Vector3(strength, strength, strength), vibrato, randomness, fadeOut);
	}

	public Tweener DOShakeCharScale(int charIndex, float duration, Vector3 strength, int vibrato = 10, float randomness = 90f, bool fadeOut = true)
	{
		if (!ValidateChar(charIndex))
		{
			return null;
		}
		if (duration <= 0f)
		{
			if (Debugger.logPriority > 0)
			{
				Debug.LogWarning("Duration can't be 0, returning NULL without creating a tween");
			}
			return null;
		}
		return DOTween.Shake(() => _charTransforms[charIndex].scale, (Vector3 x) =>
		{
			CharTransform value = _charTransforms[charIndex];
			value.UpdateGeometry(target, value.offset, value.rotation, x, _cachedMeshInfos);
			_charTransforms[charIndex] = value;
		}, duration, strength, vibrato, randomness, fadeOut);
	}
}

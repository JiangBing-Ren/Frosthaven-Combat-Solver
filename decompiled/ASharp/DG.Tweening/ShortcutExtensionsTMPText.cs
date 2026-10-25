using System.Globalization;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace DG.Tweening;

public static class ShortcutExtensionsTMPText
{
	public static TweenerCore<Color, Color, ColorOptions> DOColor(this TMP_Text target, Color endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.To(() => target.color, (Color x) =>
		{
			target.color = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOFaceColor(this TMP_Text target, Color32 endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.To(() => target.faceColor, (Color x) =>
		{
			target.faceColor = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOOutlineColor(this TMP_Text target, Color32 endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.To(() => target.outlineColor, (Color x) =>
		{
			target.outlineColor = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOGlowColor(this TMP_Text target, Color endValue, float duration, bool useSharedMaterial = false)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = (useSharedMaterial ? target.fontSharedMaterial.DOColor(endValue, "_GlowColor", duration) : target.fontMaterial.DOColor(endValue, "_GlowColor", duration));
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOFade(this TMP_Text target, float endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => target.color, (Color x) =>
		{
			target.color = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOFaceFade(this TMP_Text target, float endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => target.faceColor, (Color x) =>
		{
			target.faceColor = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> DOScale(this TMP_Text target, float endValue, float duration)
	{
		Transform trans = target.transform;
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = DOTween.To(endValue: new Vector3(endValue, endValue, endValue), getter: () => trans.localScale, setter: (Vector3 x) =>
		{
			trans.localScale = x;
		}, duration: duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<int, int, NoOptions> DOCounter(this TMP_Text target, int fromValue, int endValue, float duration, bool addThousandsSeparator = true, CultureInfo culture = null)
	{
		CultureInfo cInfo = ((!addThousandsSeparator) ? null : (culture ?? CultureInfo.InvariantCulture));
		TweenerCore<int, int, NoOptions> tweenerCore = DOTween.To(() => fromValue, (int x) =>
		{
			fromValue = x;
			target.text = (addThousandsSeparator ? fromValue.ToString("N0", cInfo) : fromValue.ToString());
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<float, float, FloatOptions> DOFontSize(this TMP_Text target, float endValue, float duration)
	{
		TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(() => target.fontSize, (float x) =>
		{
			target.fontSize = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<int, int, NoOptions> DOMaxVisibleCharacters(this TMP_Text target, int endValue, float duration)
	{
		TweenerCore<int, int, NoOptions> tweenerCore = DOTween.To(() => target.maxVisibleCharacters, (int x) =>
		{
			target.maxVisibleCharacters = x;
		}, endValue, duration);
		tweenerCore.SetTarget(target);
		return tweenerCore;
	}

	public static TweenerCore<string, string, StringOptions> DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
	{
		TweenerCore<string, string, StringOptions> tweenerCore = DOTween.To(() => target.text, (string x) =>
		{
			target.text = x;
		}, endValue, duration);
		tweenerCore.SetOptions(richTextEnabled, scrambleMode, scrambleChars).SetTarget(target);
		return tweenerCore;
	}
}

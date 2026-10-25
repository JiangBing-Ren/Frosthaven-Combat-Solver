using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UITextMeshProMaterialAnimator : CustomDOTweenAnimator
{
	[SerializeField]
	private List<TextMeshProUGUI> texts;

	[SerializeField]
	private Material originalMaterial;

	[SerializeField]
	private Material finalMaterial;

	private Material mat;

	private void Awake()
	{
		mat = new Material(originalMaterial);
	}

	public override void SetFinalValue()
	{
		foreach (TextMeshProUGUI text in texts)
		{
			text.fontMaterial = finalMaterial;
		}
	}

	public override void RestorOriginalValue()
	{
		foreach (TextMeshProUGUI text in texts)
		{
			text.fontMaterial = originalMaterial;
		}
	}

	public override Tween BuildTweenAction(float duration)
	{
		return DOTween.To((float val) =>
		{
			mat.Lerp(originalMaterial, finalMaterial, val);
			foreach (TextMeshProUGUI text in texts)
			{
				text.fontMaterial = mat;
			}
		}, 0f, 1f, duration).SetTarget(texts[0].gameObject);
	}
}

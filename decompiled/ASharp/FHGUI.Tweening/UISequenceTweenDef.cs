using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace FHGUI.Tweening;

[CreateAssetMenu(menuName = "Defs/UI/Animations/UISequenceTweenDef")]
public class UISequenceTweenDef : UITweenDef
{
	[Header("Sequence ignores the above general settings!")]
	[Space]
	[Space]
	[Space]
	[SerializeField]
	private List<UITweenDef> _defs = new List<UITweenDef>();

	public override Tween StartTween(RectTransform rt, TweenCallback onStart = null, TweenCallback onKill = null, TweenCallback onComplete = null, float executionDuration = -1f)
	{
		return StartTween_Internal(rt, executionDuration).OnStart(onStart).OnKill(onKill).OnComplete(onComplete);
	}

	protected override Tween StartTween_Internal(RectTransform rt, float _)
	{
		Sequence sequence = DOTween.Sequence();
		foreach (UITweenDef item in _defs.Where((UITweenDef def) => def != null))
		{
			sequence.Append(item.StartTween(rt));
		}
		return sequence.Play();
	}

	public override void SnapToEndValue(RectTransform rt)
	{
	}
}

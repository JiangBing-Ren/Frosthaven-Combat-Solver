using System;
using FHGUI.Elements;
using UnityEngine;
using UnityEngine.Events;

namespace FHGUI.EventChannels;

[CreateAssetMenu(menuName = "UI Events/Enhancement Highlight Events Channel", fileName = "UIEnhancementHighlightEventsChannel")]
public class UIEnhancementHighlightEventChannel : ScriptableObject
{
	[NonSerialized]
	public UnityEvent<UICardEnhancementElement.Params, bool> OnEnhancementHighlighted;

	[NonSerialized]
	public UnityEvent OnClearHighlights;

	private void OnEnable()
	{
		if (OnEnhancementHighlighted == null)
		{
			OnEnhancementHighlighted = new UnityEvent<UICardEnhancementElement.Params, bool>();
		}
	}

	private void OnDisable()
	{
		if (OnEnhancementHighlighted != null)
		{
			OnEnhancementHighlighted.RemoveAllListeners();
		}
	}

	public void EnhancementHighlighted(UICardEnhancementElement.Params enhancement, bool highlighted)
	{
		OnEnhancementHighlighted.Invoke(enhancement, highlighted);
	}

	public void ClearHighlights()
	{
		OnClearHighlights.Invoke();
	}
}

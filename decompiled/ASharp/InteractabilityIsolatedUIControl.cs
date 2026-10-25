using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class InteractabilityIsolatedUIControl : MonoBehaviour
{
	public class InteractabilityReference
	{
		public delegate void OnInteractabilityChangedDelegate();

		public InteractabilityIsolatedUIControl control;

		public bool cachedInteractable;

		public bool cachedEnabled;

		public OnInteractabilityChangedDelegate onInteractabilityChanged;

		public void Toggle(bool interactable, bool enabled)
		{
			bool flag = false;
			if (interactable != cachedInteractable)
			{
				cachedInteractable = interactable;
				flag = true;
			}
			if (enabled != cachedEnabled)
			{
				cachedEnabled = enabled;
				flag = true;
			}
			if (flag && onInteractabilityChanged != null)
			{
				onInteractabilityChanged();
			}
		}
	}

	public CLevelUIInteractionProfile.EIsolatedControlType ControlType;

	public string ControlIdentifier;

	public int ControlIndex;

	public string ControlSecondIdentifier;

	public bool shouldInitialiseOnAwake;

	public bool addAllValidChildren;

	[Header("UI Button Type:")]
	public List<ExtendedButton> ExtendedButtonsToAllow = new List<ExtendedButton>();

	public List<ExtendedToggle> ExtendedTogglesToAllow = new List<ExtendedToggle>();

	public List<TrackedButton> TrackedButtonsToAllow = new List<TrackedButton>();

	public List<TrackedToggle> TrackedTogglesToAllow = new List<TrackedToggle>();

	public List<FHButton> FHButtonsToAllow = new List<FHButton>();

	public List<FHToggle> FHTogglesToAllow = new List<FHToggle>();

	public List<UITab> TabsToAllow = new List<UITab>();

	public Func<bool> ValidateHighlight;

	public RectTransform SpecifiedRectTransformToFollow;

	public void Initialise()
	{
		if (ControlType == CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen || addAllValidChildren)
		{
			FillAllowedButtonsListWithChildrenOnly();
		}
		InteractabilityManager.RegisterControlForInteractionLimiting(this);
	}

	private void Awake()
	{
		if (shouldInitialiseOnAwake)
		{
			Initialise();
		}
	}

	private void OnDestroy()
	{
		InteractabilityManager.DeregisterControlForInteractionLimiting(this);
		ExtendedButtonsToAllow.Clear();
		ExtendedTogglesToAllow.Clear();
		TrackedButtonsToAllow.Clear();
		TrackedTogglesToAllow.Clear();
		FHButtonsToAllow.Clear();
		FHTogglesToAllow.Clear();
		TabsToAllow.Clear();
	}

	public RectTransform GetHighlightRectTransform()
	{
		if (SpecifiedRectTransformToFollow != null)
		{
			return SpecifiedRectTransformToFollow;
		}
		if (ExtendedButtonsToAllow.Count > 0)
		{
			return ExtendedButtonsToAllow[0].GetComponent<RectTransform>();
		}
		if (ExtendedTogglesToAllow.Count > 0)
		{
			return ExtendedTogglesToAllow[0].GetComponent<RectTransform>();
		}
		if (TrackedButtonsToAllow.Count > 0)
		{
			return TrackedButtonsToAllow[0].GetComponent<RectTransform>();
		}
		if (TrackedTogglesToAllow.Count > 0)
		{
			return TrackedTogglesToAllow[0].GetComponent<RectTransform>();
		}
		if (TabsToAllow.Count > 0)
		{
			return TabsToAllow[0].GetComponent<RectTransform>();
		}
		return GetComponent<RectTransform>();
	}

	public void FillAllowedButtonsListWithChildrenOnly()
	{
		ExtendedButtonsToAllow.Clear();
		ExtendedTogglesToAllow.Clear();
		TrackedButtonsToAllow.Clear();
		TrackedTogglesToAllow.Clear();
		TabsToAllow.Clear();
		ExtendedButtonsToAllow = gameObject.GetComponentsInChildren<ExtendedButton>(includeInactive: true).ToList();
		ExtendedTogglesToAllow = gameObject.GetComponentsInChildren<ExtendedToggle>(includeInactive: true).ToList();
		TrackedButtonsToAllow = gameObject.GetComponentsInChildren<TrackedButton>(includeInactive: true).ToList();
		TrackedTogglesToAllow = gameObject.GetComponentsInChildren<TrackedToggle>(includeInactive: true).ToList();
		FHButtonsToAllow = gameObject.GetComponentsInChildren<FHButton>(includeInactive: true).ToList();
		FHTogglesToAllow = gameObject.GetComponentsInChildren<FHToggle>(includeInactive: true).ToList();
		TabsToAllow = gameObject.GetComponentsInChildren<UITab>(includeInactive: true).ToList();
	}
}

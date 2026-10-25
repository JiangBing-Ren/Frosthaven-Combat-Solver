using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRulesetSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedToggle toggle;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private GUIAnimator highlightAnimation;

	private Action<IRuleset, bool> onToggled;

	private Action<IRuleset, bool> onHovered;

	private IRuleset ruleset;

	public bool IsHovered { get; internal set; }

	private void Awake()
	{
		toggle.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		toggle.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: false);
		});
		toggle.onValueChanged.AddListener((bool isOn) =>
		{
			onToggled?.Invoke(ruleset, isOn);
		});
	}

	public void SetRuleset(IRuleset ruleset, ToggleGroup toggleGroup, Action<IRuleset, bool> onToggled, Action<IRuleset, bool> onHovered = null)
	{
		this.ruleset = ruleset;
		this.onToggled = onToggled;
		this.onHovered = onHovered;
		nameText.text = ruleset.Name;
		toggle.group = toggleGroup;
		highlightAnimation.Stop();
		IsHovered = false;
		SetSelected(selected: false);
	}

	public void Highlight()
	{
		highlightAnimation.Play();
	}

	private void OnHovered(bool hovered)
	{
		IsHovered = hovered;
		onHovered?.Invoke(ruleset, hovered);
	}

	private void OnDisabled()
	{
		highlightAnimation.Stop();
	}

	public void SetSelected(bool selected)
	{
		toggle.SetValue(selected);
	}
}

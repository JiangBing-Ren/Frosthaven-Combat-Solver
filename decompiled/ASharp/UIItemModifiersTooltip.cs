using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class UIItemModifiersTooltip : MonoBehaviour
{
	[SerializeField]
	private Vector2 offset;

	[SerializeField]
	private RectTransform window;

	[SerializeField]
	private RectTransform modifiersContainer;

	[SerializeField]
	private UIPerkAttackModifier modifierPrefab;

	private List<UIPerkAttackModifier> modifiersUI = new List<UIPerkAttackModifier>();

	public bool EnableTooltip { get; set; }

	public void Initialize(Dictionary<AttackModifierYMLData, int> modifiers)
	{
		modifiersUI.ForEach((UIPerkAttackModifier it) =>
		{
			ObjectPool.Recycle(it.gameObject);
		});
		modifiersUI.Clear();
		if (modifiers == null || modifiers.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<AttackModifierYMLData, int> modifier in modifiers)
		{
			UIPerkAttackModifier uIPerkAttackModifier = ObjectPool.Spawn(modifierPrefab, modifiersContainer);
			uIPerkAttackModifier.Init(modifier.Key, modifier.Value);
			modifiersUI.Add(uIPerkAttackModifier);
		}
	}

	public void ShowTooltip(bool showLeftToParent)
	{
		gameObject.SetActive(EnableTooltip && modifiersUI.Count > 0);
		if (EnableTooltip && modifiersUI.Count > 0)
		{
			window.anchorMax = new Vector2((!showLeftToParent) ? 1 : 0, window.anchorMax.y);
			window.anchorMin = new Vector2((!showLeftToParent) ? 1 : 0, window.anchorMin.y);
			window.pivot = new Vector2(showLeftToParent ? 1 : 0, window.pivot.y);
			window.anchoredPosition = offset * new Vector2((!showLeftToParent) ? 1 : (-1), 1f);
		}
	}

	public void HideTooltip()
	{
		gameObject.SetActive(value: false);
	}
}

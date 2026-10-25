using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPerkModifiersTooltip : UITooltipTarget
{
	private CharacterPerk perk;

	public UIPerkAttackModifier modifierPrefab;

	public UIPerkAttackModifier passiveItemsModifier;

	public UIPerkAttackModifier passiveScenarioModifier;

	public TextMeshProUGUI textPrefab;

	private List<UIPerkAttackModifier> modifiers = new List<UIPerkAttackModifier>();

	private List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

	public void Initialize(CharacterPerk perk)
	{
		this.perk = perk;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		if (perk != null)
		{
			List<IGrouping<string, AttackModifierYMLData>> list = (from it in perk.Perk.CardsToAdd
				group it by it.Name).ToList();
			List<IGrouping<string, AttackModifierYMLData>> list2 = (from it in perk.Perk.CardsToRemove
				group it by it.Name).ToList();
			if (list2.Count > 0)
			{
				AddText((list.Count == 0) ? "GUI_PERK_MODIFIER_REMOVE" : "GUI_PERK_MODIFIER_REPLACE");
				list2.ForEach((IGrouping<string, AttackModifierYMLData> it) =>
				{
					UIPerkAttackModifier uIPerkAttackModifier = ObjectPool.Spawn(modifierPrefab, UITooltipManager.GetTransform());
					uIPerkAttackModifier.Init(it.First(), it.Count());
					modifiers.Add(uIPerkAttackModifier);
				});
			}
			if (list.Count > 0)
			{
				AddText((list2.Count == 0) ? "GUI_PERK_MODIFIER_ADD" : "GUI_PERK_MODIFIER_REPLACE_WITH");
				list.ForEach((IGrouping<string, AttackModifierYMLData> it) =>
				{
					UIPerkAttackModifier uIPerkAttackModifier = ObjectPool.Spawn(modifierPrefab, UITooltipManager.GetTransform());
					uIPerkAttackModifier.Init(it.First(), it.Count());
					modifiers.Add(uIPerkAttackModifier);
				});
			}
			if ((perk.Perk.Flags & EPerkFlag.IgnoreNegativeItemEffects) != 0)
			{
				if (list.Count == 0)
				{
					AddText("GUI_PERK_MODIFIER_ADD");
				}
				UIPerkAttackModifier item = ObjectPool.Spawn(passiveItemsModifier, UITooltipManager.GetTransform());
				modifiers.Add(item);
			}
			if ((perk.Perk.Flags & EPerkFlag.IgnoreNegativeScenarioEffects) != 0)
			{
				if (list.Count == 0 && (perk.Perk.Flags & EPerkFlag.IgnoreNegativeItemEffects) == 0)
				{
					AddText("GUI_PERK_MODIFIER_ADD");
				}
				UIPerkAttackModifier item2 = ObjectPool.Spawn(passiveScenarioModifier, UITooltipManager.GetTransform());
				modifiers.Add(item2);
			}
			base.ShowTooltip(delay);
		}
		else
		{
			Log.GUIError("Trying to show a perk but is null.");
		}
	}

	private void AddText(string key)
	{
		TextMeshProUGUI textMeshProUGUI = Object.Instantiate(textPrefab, UITooltipManager.GetTransform());
		texts.Add(textMeshProUGUI);
		textMeshProUGUI.text = LocalizationManager.GetTranslation(key);
	}

	protected override void SetControls()
	{
		UITooltipManager.SetVerticalControls(autoAdjustHeight, tempIsPrefabTooltip: true);
	}

	public override void HideTooltip(float delay = -1f)
	{
		base.HideTooltip(delay);
		modifiers.ForEach((UIPerkAttackModifier it) =>
		{
			ObjectPool.Recycle(it.gameObject);
		});
		modifiers.Clear();
		foreach (TextMeshProUGUI text in texts)
		{
			Object.Destroy(text.gameObject);
		}
		texts.Clear();
	}
}

using System;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPerksWindow : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIPerksInventory inventory;

	[SerializeField]
	private UIPerksActiveModifiers activePerkssDisplay;

	private Action<AttackModifierYMLData> onHoveredPerk;

	private ICharacterService characterData;

	private RectTransform position;

	private Action onHidden;

	public bool IsVisible => window.IsOpen;

	public UIPerksInventory PerkDisplay => inventory;

	public UIPerksActiveModifiers ModifierDisplay => activePerkssDisplay;

	private void Awake()
	{
		window.onHidden.AddListener(() =>
		{
			onHidden?.Invoke();
		});
		inventory.OnHoverSlot.AddListener((UIPerkInventorySlot slot) =>
		{
			activePerkssDisplay.PreviewAdd(slot.Perk);
		});
		inventory.OnUnhoverSlot.AddListener((UIPerkInventorySlot slot) =>
		{
			activePerkssDisplay.ClearPreview();
		});
		inventory.OnEnabledPerk.AddListener((CharacterPerk perk) =>
		{
			activePerkssDisplay.ClearPreview();
		});
		activePerkssDisplay.OnHovered.AddListener((AttackModifierYMLData perk) =>
		{
			onHoveredPerk?.Invoke(perk);
		});
	}

	public void Show(ICharacterService characterData, RectTransform position, Action<AttackModifierYMLData> onHoveredPerk = null, bool editAllowed = true, Action onHidden = null)
	{
		window.Show();
		Log.GUIInfo("Open perks window " + characterData.CharacterModel);
		this.characterData = characterData;
		this.position = position;
		this.onHidden = onHidden;
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.perk_selection);
		this.onHoveredPerk = onHoveredPerk;
		activePerkssDisplay.Display(characterData);
		inventory.Display(characterData, position, editAllowed);
	}

	public void Refresh(bool editAllowed = true)
	{
		if (IsVisible && characterData != null)
		{
			Log.GUIInfo("Refresh perks window " + characterData.CharacterModel);
			PerkDisplay.Display(characterData, position, editAllowed);
			ModifierDisplay.Display(characterData);
		}
	}

	public void Hide(bool playHideAudio = true)
	{
		UIWindow uIWindow = window;
		if ((object)uIWindow != null && uIWindow.IsOpen)
		{
			string audioItemHide = window.AudioItemHide;
			if (!playHideAudio)
			{
				window.AudioItemHide = null;
			}
			Log.GUIInfo("Hide Perks Window");
			window.Hide();
			window.AudioItemHide = audioItemHide;
		}
	}
}

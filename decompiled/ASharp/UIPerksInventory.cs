using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPerksInventory : MonoBehaviour
{
	[Serializable]
	public class HoverPerkSlotEvent : UnityEvent<UIPerkInventorySlot>
	{
	}

	[Serializable]
	public class SelectPerkSlotEvent : UnityEvent<CharacterPerk>
	{
	}

	[SerializeField]
	private Transform content;

	[SerializeField]
	private UIPerkInventorySlot perkPrefab;

	[SerializeField]
	private TextMeshProUGUI pointsText;

	[SerializeField]
	private Color availablePerkPointsColor;

	[SerializeField]
	private Color unavailablePerkPointsColor;

	[SerializeField]
	private Image availablePointsMask;

	[SerializeField]
	private UINewNotificationTip availablePointsNotification;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private string audioItemEnablePerk = "PlaySound_UIReceivedItem";

	[Header("Perk Checks")]
	[SerializeField]
	private RectTransform perksChecksContainer;

	[SerializeField]
	private List<Toggle> perksChecks;

	public HoverPerkSlotEvent OnHoverSlot = new HoverPerkSlotEvent();

	public HoverPerkSlotEvent OnUnhoverSlot = new HoverPerkSlotEvent();

	public SelectPerkSlotEvent OnEnabledPerk = new SelectPerkSlotEvent();

	private List<UIPerkInventorySlot> perksUI = new List<UIPerkInventorySlot>();

	private ICharacterService characterData;

	public void Display(ICharacterService characterData, RectTransform sourceUI, bool isInteractable)
	{
		scroll.verticalNormalizedPosition = 1f;
		verticalPointer.PointAt(sourceUI);
		foreach (UIPerkInventorySlot item in perksUI)
		{
			ObjectPool.Recycle(item.gameObject);
		}
		perksUI.Clear();
		this.characterData = characterData;
		UpdateDisplay(isInteractable);
		CancelAnimations();
		showAnimation?.Play();
	}

	private void RefreshPerkPoints()
	{
		if (characterData.PerkPoints == 0)
		{
			pointsText.text = string.Format(LocalizationManager.GetTranslation("GUI_PERKS_POINTS"), $"<color=#{unavailablePerkPointsColor.ToHex()}>{characterData.PerkPoints}</color>");
			availablePointsMask.enabled = false;
			availablePointsNotification.Hide();
		}
		else
		{
			pointsText.text = string.Format(LocalizationManager.GetTranslation("GUI_PERKS_POINTS"), $"<color=#{availablePerkPointsColor.ToHex()}>{characterData.PerkPoints}</color>");
			availablePointsMask.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(characterData.CharacterModel, highlighted: true, characterData.Class.CustomCharacterConfig);
			availablePointsMask.enabled = true;
			availablePointsNotification.Show();
		}
	}

	private void UpdateDisplay(bool isInteractable)
	{
		RefreshPerkPoints();
		if (characterData.PerkChecksToComplete == 0)
		{
			perksChecksContainer.gameObject.SetActive(value: false);
		}
		else
		{
			HelperTools.NormalizePool(ref perksChecks, perksChecks[0].gameObject, perksChecksContainer, characterData.PerkChecksToComplete);
			for (int i = 0; i < characterData.PerkChecksToComplete; i++)
			{
				perksChecks[i].isOn = i < characterData.PerkChecks;
			}
			perksChecksContainer.gameObject.SetActive(value: true);
		}
		foreach (IGrouping<string, CharacterPerk> item in from it in characterData.Perks
			group it by it.PerkID)
		{
			UIPerkInventorySlot component = ObjectPool.Spawn(perkPrefab.gameObject, content).GetComponent<UIPerkInventorySlot>();
			component.Init(characterData, item.ToList(), OnHoveredPerk, OnSelectedPerk, isInteractable);
			perksUI.Add(component);
		}
	}

	private void OnHoveredPerk(bool hovered, UIPerkInventorySlot perkSlot)
	{
		if (hovered)
		{
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				scroll.ScrollVerticallyTo(perkSlot.transform as RectTransform);
			}
			if (perkSlot.TotalCounters != perkSlot.ActiveCounters)
			{
				CharacterPerk characterPerk = characterData.Perks.FirstOrDefault((CharacterPerk it) => !it.IsActive && it.PerkID == perkSlot.Perk.PerkID);
				if (characterPerk == null || ValidatePreconditions(characterPerk))
				{
					OnHoverSlot.Invoke(perkSlot);
				}
			}
		}
		else
		{
			OnUnhoverSlot.Invoke(perkSlot);
		}
	}

	private void OnSelectedPerk(UIPerkInventorySlot perkUi)
	{
		if (characterData.PerkPoints > 0)
		{
			CharacterPerk perk = characterData.Perks.FirstOrDefault((CharacterPerk it) => !it.IsActive && it.PerkID == perkUi.Perk.PerkID);
			if (perk == null)
			{
				Log.GUIError("Should not arrived until here since UIPerksInventorySlot is not selected if there is not an inactive perk available");
				return;
			}
			if (!ValidatePreconditions(perk))
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
				Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_PERK_MISSING_REMOVE_CONDITION"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_PERK_MISSING_REMOVE_CONDITION_WARNING"), UIInfoTools.Instance.warningColor);
				return;
			}
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.generalAudioButtonProfile.mouseDownAudioItem);
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(string.Format(LocalizationManager.GetTranslation("GUI_CONFIRMATION_ENABLE_PERK"), LocalizationManager.GetTranslation(perk.Perk.Name), perkUi.ActiveCounters + 1, perkUi.TotalCounters), LocalizationManager.GetTranslation("GUI_CONFIRMATION_ENABLE_PERK_WARNING"), () =>
			{
				EnabledPerk(perk, perkUi);
			});
		}
		else
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
		}
	}

	private bool ValidatePreconditions(CharacterPerk perk)
	{
		return perk.Perk.CardsToRemove.All((AttackModifierYMLData it) => characterData.AttackModifierDeck.Exists((AttackModifierYMLData modif) => modif.Name == it.Name));
	}

	private void EnabledPerk(CharacterPerk perk, UIPerkInventorySlot perkUi)
	{
		perk.IsActive = true;
		characterData.UpdatePerkPoints(Math.Max(0, characterData.PerkPoints - 1));
		OnEnabledPerk.Invoke(perk);
		RefreshPerkPoints();
		foreach (UIPerkInventorySlot item in perksUI)
		{
			item.RefreshCounters();
		}
		AudioControllerUtils.PlaySound(audioItemEnablePerk);
		SaveData.Instance.ManualSaveCurrentAdventureData();
		if (FFSNetwork.IsOnline)
		{
			int controllableID = ControllableRegistry.GetControllableID(characterData.CharacterID);
			Synchronizer.ReplicateControllableStateChange(GameActionType.ModifyPerks, ActionProcessor.CurrentPhase, controllableID, 0, 0, 0, supplementaryDataBoolean: false, null, new PerkPointsToken(characterData.PerkPoints), new PerksToken(characterData.Perks));
		}
		Log.SimpleInfo("MapCharacter: " + characterData.CharacterID + " enabled perk: " + perk.PerkID);
	}

	private void CancelAnimations()
	{
		showAnimation?.Stop();
	}

	private void OnDisable()
	{
		CancelAnimations();
	}

	private void OnDestroy()
	{
		if (GloomUtility.IsAppQuitting)
		{
			return;
		}
		foreach (UIPerkInventorySlot item in perksUI)
		{
			ObjectPool.Recycle(item.gameObject);
		}
	}
}

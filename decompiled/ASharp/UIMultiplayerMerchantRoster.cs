using System.Collections.Generic;
using FFSNet;
using FHRL;
using GLOOM;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.Events;

public class UIMultiplayerMerchantRoster : MonoBehaviour
{
	public class HeroSlotEvent : UnityEvent<UIMultiplayerMerchantSlot>
	{
	}

	[SerializeField]
	private List<UIMultiplayerMerchantSlot> slots;

	public UnityEvent OnDeselectedSlot = new UnityEvent();

	public HeroSlotEvent OnSelectedSlot = new HeroSlotEvent();

	private UIMultiplayerMerchantSlot currentSlot;

	private CMapCharacter[] currentCharacters;

	public UIMultiplayerMerchantSlot CurrentSlot => currentSlot;

	private void Awake()
	{
		slots.ForEach((UIMultiplayerMerchantSlot it) =>
		{
			it.Init(() =>
			{
				OnSelectSlot(it);
			}, () =>
			{
				OnDeselectSlot(it);
			});
		});
	}

	private void OnDeselectSlot(UIMultiplayerMerchantSlot slot)
	{
		if (currentSlot == slot)
		{
			currentSlot = null;
			OnDeselectedSlot.Invoke();
		}
	}

	private void OnSelectSlot(UIMultiplayerMerchantSlot slot)
	{
		if (currentSlot == null || currentSlot != slot)
		{
			UIMultiplayerMerchantSlot uIMultiplayerMerchantSlot = currentSlot;
			currentSlot = slot;
			if (uIMultiplayerMerchantSlot != null)
			{
				uIMultiplayerMerchantSlot.Deselect();
			}
			OnSelectedSlot.Invoke(slot);
		}
	}

	public void Init(CMapCharacter[] characters)
	{
		currentSlot = null;
		currentCharacters = characters;
		for (int i = 0; i < 4; i++)
		{
			if (characters[i] != null)
			{
				slots[i].SetCharacter(characters[i], slots[i] == currentSlot);
			}
			else
			{
				slots[i].SetEmpty(string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HERO_SLOT"), i + 1), slots[i] == currentSlot);
			}
		}
		RefreshInteractable();
	}

	public void RefreshInteractable()
	{
		foreach (UIMultiplayerMerchantSlot slot in slots)
		{
			bool flag = true;
			slot.SetInteractable(flag);
			if (!flag)
			{
				slot.Deselect();
			}
		}
	}

	public void AssignPlayerTo(PartySlotID slot, NetworkPlayer player, bool isOnline)
	{
		slots[slot.Index].AssignTo(player, isOnline);
	}

	public void UpdateConnectionStateSlot(NetworkPlayer player, bool isOnline)
	{
		foreach (UIMultiplayerMerchantSlot slot in slots)
		{
			if (slot.AssignedPlayer == player)
			{
				slot.UpdateConnectionStatus(isOnline);
			}
		}
	}

	public void AssignPlayerToSelectedSlot(NetworkPlayer player, bool isOnline)
	{
		CurrentSlot?.AssignTo(player, isOnline);
	}

	public void ResetSelection()
	{
		if (currentSlot != null)
		{
			currentSlot.SetSelected(false);
		}
		currentSlot = null;
	}

	public void Focus(bool focus, bool interactable = true)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].Focus(focus || currentSlot == slots[i], interactable);
		}
	}
}

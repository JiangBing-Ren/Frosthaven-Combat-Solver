using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerSelectPlayerScreen : Singleton<UIMultiplayerSelectPlayerScreen>
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private List<UIMultiplayerPlayerOption> slots;

	[SerializeField]
	private VerticalPointerUI pointer;

	[SerializeField]
	private UIMultiplayerConfirmationBox confirmationBox;

	[SerializeField]
	private float defaultPositionX;

	private NetworkPlayer currentSelection;

	private Dictionary<NetworkPlayer, UIMultiplayerPlayerOption> assignedSlots = new Dictionary<NetworkPlayer, UIMultiplayerPlayerOption>();

	private INetworkHeroAssignService service;

	private string characterID;

	private string characterName;

	private Action onHiddenCallback;

	private Action<NetworkPlayer> onSelectedPlayer;

	private int m_CurrentSlotIndex;

	public bool IsOpen => window.IsOpen;

	public string Id { get; private set; }

	public INetworkHeroAssignService Service => service;

	protected override void AwakeInit()
	{
		slots = slots.OrderByDescending((UIMultiplayerPlayerOption it) => it.transform.GetSiblingIndex()).ToList();
		window.onHidden.AddListener(() =>
		{
			onHiddenCallback?.Invoke();
		});
		service = GetComponent<INetworkHeroAssignService>();
		int num = 0;
		foreach (UIMultiplayerPlayerOption slot in slots)
		{
			slot.OnSelect.AddListener(OnSelectedPlayer);
			slot.OnRemove.AddListener(OnSelectedRemovePlayer);
			slot.SlotIndex = num;
			num++;
		}
	}

	private void OnSelectedPlayer(NetworkPlayer player)
	{
		if (currentSelection == null || currentSelection != player)
		{
			if (currentSelection != null && assignedSlots.TryGetValue(currentSelection, out var value))
			{
				value.SetSelected(false);
			}
			currentSelection = player;
			Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, 0, player.PlayerID.ID, m_CurrentSlotIndex);
			if (characterID == null)
			{
				service.AssignSlotToPlayer(player, m_CurrentSlotIndex);
			}
			else
			{
				service.AssignHeroToPlayer(player, characterID, characterName, m_CurrentSlotIndex);
			}
			onSelectedPlayer?.Invoke(player);
		}
	}

	private void OnSelectedRemovePlayer(NetworkPlayer player)
	{
		if (currentSelection == player && player.IsClient)
		{
			confirmationBox.ShowConfirmation(player, () =>
			{
				service.RemoveClient(player);
			});
		}
	}

	public void RefreshPlayers()
	{
		if (!window.IsOpen)
		{
			return;
		}
		List<NetworkPlayer> list = (from w in service.GetAllPlayers()
			where w.PlayerReadyForAssignment || w.IsClient
			select w).ToList();
		if (currentSelection != null && !list.Contains(currentSelection))
		{
			if (assignedSlots.TryGetValue(currentSelection, out var value))
			{
				value.SetSelected(false);
			}
			currentSelection = null;
		}
		slots.AddRange(assignedSlots.Values.OrderByDescending((UIMultiplayerPlayerOption it) => it.transform.GetSiblingIndex()));
		assignedSlots.Clear();
		for (int num = 0; num < list.Count; num++)
		{
			AddPlayer(list[num]);
		}
		for (int num2 = 0; num2 < slots.Count; num2++)
		{
			slots[num2].gameObject.SetActive(value: false);
		}
	}

	public void AddPlayer(NetworkPlayer player)
	{
		if (window.IsOpen && !assignedSlots.ContainsKey(player))
		{
			UIMultiplayerPlayerOption slot = slots[slots.Count - 1];
			slots.RemoveAt(slots.Count - 1);
			assignedSlots[player] = slot;
			slot.SetPlayer(player, player == currentSelection);
			slot.gameObject.SetActive(value: true);
			int num = (from it in slots.Concat(assignedSlots.Values)
				orderby it.transform.GetSiblingIndex() descending
				select it).ToList().FindIndex((UIMultiplayerPlayerOption x) => x == slot);
			Log.NetworkInfo("[Core] " + player.Username + " (PlayerID: " + player.PlayerID.ToString() + ") was ASSIGNED slot #" + num);
		}
	}

	public void RemovePlayer(NetworkPlayer player)
	{
		if (window.IsOpen && assignedSlots.ContainsKey(player))
		{
			UIMultiplayerPlayerOption slot = assignedSlots[player];
			assignedSlots.Remove(player);
			slot.gameObject.SetActive(value: false);
			slots.Add(slot);
			if (currentSelection == player)
			{
				slot.SetSelected(false);
				currentSelection = null;
			}
			int num = (from it in slots.Concat(assignedSlots.Values)
				orderby it.transform.GetSiblingIndex() descending
				select it).ToList().FindIndex((UIMultiplayerPlayerOption x) => x == slot);
			Log.NetworkInfo("[Core] " + player.Username + " (PlayerID: " + player.PlayerID.ToString() + ") was UNASSIGNED from slot #" + num);
		}
	}

	public void SetSelected(NetworkPlayer selection)
	{
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(false);
		}
		currentSelection = selection;
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(true);
		}
	}

	public void Show(string id, string characterID, string characterName, int slotIndex, string title, NetworkPlayer selection, RectTransform point, Action onHidden = null, Action<NetworkPlayer> onSelectedPlayer = null, float? displacement = null)
	{
		Id = id;
		this.characterID = characterID;
		this.characterName = characterName;
		this.title.text = title;
		onHiddenCallback = onHidden;
		this.onSelectedPlayer = onSelectedPlayer;
		m_CurrentSlotIndex = slotIndex;
		(transform as RectTransform).anchoredPosition = new Vector2(displacement ?? defaultPositionX, 0f);
		window.Show();
		currentSelection = selection;
		RefreshPlayers();
		if ((bool)pointer)
		{
			pointer.PointAt(point);
		}
		RemoveEvents();
		INetworkHeroAssignService networkHeroAssignService = service;
		networkHeroAssignService.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(networkHeroAssignService.OnPlayerLeft, new PlayersChangedEvent(RemovePlayer));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
	}

	private void RemoveEvents()
	{
		if (service != null)
		{
			INetworkHeroAssignService networkHeroAssignService = service;
			networkHeroAssignService.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(networkHeroAssignService.OnPlayerLeft, new PlayersChangedEvent(RemovePlayer));
		}
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
	}

	private void OnControllableOwnersChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		string text = null;
		text = ((!(Singleton<FHMapChoreographer>.InstanceFast != null)) ? ControllableRegistry.GetCharacterIDByControllerID(controllable.ID) : Singleton<FHMapChoreographer>.InstanceFast.GameClient.GetControllableCharacter(controllable.ID)?.Character.ClassID.ID);
		if (characterID == text)
		{
			SetSelected(newController);
		}
	}

	public void Hide()
	{
		RemoveEvents();
		window.Hide();
		confirmationBox.Hide();
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(false);
		}
		currentSelection = null;
	}

	protected override void AwakenedDestroy()
	{
		RemoveEvents();
	}

	public void ServerACKClientLoadedInAndReady(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		if (player != null)
		{
			AddPlayer(player);
			return;
		}
		throw new Exception("Error ACKing client load-in. NetworkPlayer returns null.");
	}
}

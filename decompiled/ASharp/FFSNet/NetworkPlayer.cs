using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FH.Platforms;
using FHRL;
using SETUtil;
using UnityEngine;

namespace FFSNet;

public class NetworkPlayer
{
	public int LastActionID = 1;

	private bool m_PlayerReadyForAssignment;

	private List<PlayerAvatarStreamingDataToken> _avatarStreaming = new List<PlayerAvatarStreamingDataToken>();

	public Action OnMyControllablesChanged;

	private bool _isAvatarCreatedFromStream;

	private readonly PlayerToken _playerToken;

	public PlayerID PlayerID => new PlayerID(_playerToken.PlayerID);

	public bool IsClient => !_playerToken.IsHost;

	public bool HasDesynched { get; set; }

	public bool IsParticipant => MyControllables.FirstOrDefault((NetworkControllable x) => x.IsParticipant) != null;

	public bool IsSlotAssigned => AssignedSlots.Any();

	public IEnumerable<PartySlotID> AssignedSlots => PlayerRegistry.GetPlayerSlots(this);

	public IEnumerable<PartySlotID> AssignedFreeSlots => from s in PlayerRegistry.GetPlayerSlots(this)
		where !PlayerRegistry.SlotHasControllable(s)
		select s;

	public bool IsActive => MyControllables.FirstOrDefault((NetworkControllable x) => x.IsAlive) != null;

	public string Username => _playerToken.Username;

	public bool PlayerReadyForAssignment
	{
		get
		{
			return m_PlayerReadyForAssignment;
		}
		set
		{
			bool flag = PlayerRegistry.OtherPlayersAreJoining();
			m_PlayerReadyForAssignment = value;
			if (flag && !PlayerRegistry.OtherPlayersAreJoining() && FFSNetwork.IsHost)
			{
				Synchronizer.SendSideAction(GameActionType.NotifyClientsPlayersAreFinishedJoining);
				PlayerRegistry.OnUserReadyForAssignment?.Invoke();
			}
			Log.NetworkInfo("Player of ID: " + PlayerID.ToString() + "PlayerReadyForAssignment set to: " + m_PlayerReadyForAssignment);
		}
	}

	public bool SentPlayerReadyForAssignment { get; set; }

	public Sprite Avatar { get; set; }

	public bool AvatarLoaded { get; set; }

	public IEnumerable<NetworkControllable> MyControllables => PlayerRegistry.GetPlayerControllables(this);

	public List<NetworkPlayer> PlayersACKedMyLatestControllableState { get; set; }

	public PlayerToken PlayerToken => _playerToken;

	public void Clear()
	{
		if (_isAvatarCreatedFromStream && Avatar != null)
		{
			SceneUtil.SmartDestroy(Avatar.texture);
			SceneUtil.SmartDestroy(Avatar);
		}
		Avatar = null;
	}

	public NetworkPlayer(PlayerToken playerToken)
	{
		_playerToken = playerToken;
		if (FFSNetwork.IsHost)
		{
			PlayersACKedMyLatestControllableState = new List<NetworkPlayer>();
		}
	}

	public override string ToString()
	{
		return _playerToken?.ToString();
	}

	public IEnumerable WaitForAvatarLoadedCrt()
	{
		while (!AvatarLoaded)
		{
			yield return null;
		}
	}

	public IEnumerator StartLoadingAvatarCrt()
	{
		if (PlayerRegistry.MyPlayer != this)
		{
			yield break;
		}
		bool done = false;
		PlatformLayer.Instance.NewPlatformLayer.GetCurrentUserData((PlatformUserData data) =>
		{
			Avatar = data.Avatar;
			done = true;
		});
		while (!done)
		{
			yield return null;
		}
		if (Avatar != null)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(128, 128, 0);
			Texture2D texture2D = new Texture2D(temporary.width, temporary.height, TextureFormat.RGB24, mipChain: false);
			Graphics.Blit(Avatar.texture, temporary);
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			Avatar = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.one / 2f);
			RenderTexture.ReleaseTemporary(temporary);
		}
		AvatarLoaded = true;
		if (!(Avatar != null) || !FFSNetwork.IsOnline)
		{
			yield break;
		}
		foreach (PlayerAvatarStreamingDataToken item in PlayerAvatarStreamingDataToken.GenerateTokensFromSprite(PlayerID, Avatar))
		{
			FFSNetwork.SendTokenToClients(item, NetworkDataStreamChannel.PlayerAvatar);
		}
	}

	public void SendAvatarDataToPlayer(NetworkPlayer player)
	{
		if (!AvatarLoaded || !(Avatar != null) || !FFSNetwork.IsOnline)
		{
			return;
		}
		foreach (PlayerAvatarStreamingDataToken item in PlayerAvatarStreamingDataToken.GenerateTokensFromSprite(PlayerID, Avatar))
		{
			FFSNetwork.SendTokenToClient(item, player.PlayerID, NetworkDataStreamChannel.PlayerAvatar);
		}
	}

	public void AvatarStreaming(PlayerAvatarStreamingDataToken token)
	{
		_isAvatarCreatedFromStream = false;
		if (token.IsStart)
		{
			_avatarStreaming.Clear();
		}
		_avatarStreaming.Add(token);
		if (token.IsEnd)
		{
			_isAvatarCreatedFromStream = true;
			Avatar = PlayerAvatarStreamingDataToken.ReconstructSprite(_avatarStreaming);
			AvatarLoaded = true;
			_avatarStreaming.Clear();
		}
	}

	public bool HasControlOver<TControllableEnumType>(string controllableEnumMemberName) where TControllableEnumType : struct, IConvertible
	{
		if (Enum.TryParse<TControllableEnumType>(controllableEnumMemberName, out var result) && HasControlOver(Convert.ToInt32(result)))
		{
			return true;
		}
		return false;
	}

	public bool HasControlOver(int controllableID)
	{
		return MyControllables.FirstOrDefault((NetworkControllable x) => x.ID == controllableID) != null;
	}

	public void PrintControllables()
	{
		foreach (NetworkControllable myControllable in MyControllables)
		{
			if (myControllable != null)
			{
				Log.NetworkTrace(Username + " (PlayerID: " + PlayerID.ToString() + "): " + ((myControllable.ControllableObject != null) ? myControllable.ControllableObject.GetName() : "Uninitialized controllable") + " - " + (myControllable.IsParticipant ? "Participant" : "Benched") + " - " + (myControllable.IsAlive ? "Alive" : "Dead"));
			}
			else
			{
				Log.NetworkTrace(Username + " (PlayerID: " + PlayerID.ToString() + "): Null controllable");
			}
		}
	}
}

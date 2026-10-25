using System;
using System.Collections.Generic;
using System.Linq;
using FHRL;
using UdpKit;
using UnityEngine;

namespace FFSNet;

public class PlayerAvatarStreamingDataToken : IProtocolToken
{
	public int PlayerID { get; set; }

	public int Offset { get; private set; }

	public int Total { get; private set; }

	public int TextureWidth { get; private set; }

	public int TextureHeight { get; private set; }

	public TextureFormat TextureFormat { get; private set; }

	public byte[] Array { get; private set; }

	public bool IsStart => Offset == 0;

	public bool IsEnd => Offset + Array.Length == Total;

	public void Read(UdpPacket packet)
	{
		PlayerID = packet.ReadInt();
		Offset = packet.ReadInt();
		Total = packet.ReadInt();
		TextureWidth = packet.ReadInt();
		TextureHeight = packet.ReadInt();
		TextureFormat = (TextureFormat)packet.ReadInt();
		Array = packet.ReadByteArrayWithPrefix();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(PlayerID);
		packet.WriteInt(Offset);
		packet.WriteInt(Total);
		packet.WriteInt(TextureWidth);
		packet.WriteInt(TextureHeight);
		packet.WriteInt((int)TextureFormat);
		packet.WriteByteArrayWithPrefix(Array);
	}

	public static Sprite ReconstructSprite(List<PlayerAvatarStreamingDataToken> tokens)
	{
		byte[] array = null;
		if (tokens.Count == 1)
		{
			array = tokens[0].Array;
		}
		else
		{
			array = new byte[tokens.First().Total];
			foreach (PlayerAvatarStreamingDataToken token in tokens)
			{
				Buffer.BlockCopy(token.Array, 0, array, token.Offset, token.Array.Length);
			}
		}
		array = Utility.DecompressData(array);
		if (array.Length == 0)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(tokens.First().TextureWidth, tokens.First().TextureHeight, tokens.First().TextureFormat, mipChain: false);
		texture2D.LoadImage(array);
		texture2D.Apply();
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
	}

	public static IEnumerable<PlayerAvatarStreamingDataToken> GenerateTokensFromSprite(PlayerID playerID, Sprite sprite)
	{
		return GenerateTokensFromSprite(playerID.ID, sprite);
	}

	public static IEnumerable<PlayerAvatarStreamingDataToken> GenerateTokensFromSprite(int playerID, Sprite sprite)
	{
		byte[] data = sprite.texture.EncodeToPNG();
		data = Utility.CompressData(data);
		if (data.Length == 0)
		{
			yield return new PlayerAvatarStreamingDataToken
			{
				Array = new byte[0],
				PlayerID = playerID
			};
			yield break;
		}
		int size = 900;
		for (int i = 0; i < data.Length; i += size)
		{
			PlayerAvatarStreamingDataToken playerAvatarStreamingDataToken = new PlayerAvatarStreamingDataToken
			{
				Offset = i,
				Total = data.Length,
				TextureWidth = sprite.texture.width,
				TextureHeight = sprite.texture.height,
				TextureFormat = sprite.texture.format,
				PlayerID = playerID
			};
			int num = Math.Min(size, data.Length - i);
			playerAvatarStreamingDataToken.Array = new byte[num];
			Buffer.BlockCopy(data, playerAvatarStreamingDataToken.Offset, playerAvatarStreamingDataToken.Array, 0, playerAvatarStreamingDataToken.Array.Length);
			yield return playerAvatarStreamingDataToken;
		}
	}
}

using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;
using FH.Platforms;
using MapRuleLibrary.State;
using UnityEngine;

[Serializable]
public class SaveOwner : ISerializable
{
	[Serializable]
	public class SerializedAvatar : ISerializable
	{
		private Sprite m_Sprite;

		public int X { get; private set; }

		public int Y { get; private set; }

		public byte[] Bytes { get; private set; }

		public Sprite Avatar
		{
			get
			{
				if (m_Sprite == null && Bytes != null)
				{
					Texture2D texture2D = new Texture2D(X, Y);
					texture2D.LoadImage(Bytes);
					m_Sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				}
				return m_Sprite;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("X", X);
			info.AddValue("Y", Y);
			info.AddValue("Bytes", Bytes);
		}

		public SerializedAvatar(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					switch (item.Name)
					{
					case "X":
						X = info.GetInt32("X");
						break;
					case "Y":
						Y = info.GetInt32("Y");
						break;
					case "Bytes":
						Bytes = (byte[])info.GetValue("Bytes", typeof(byte[]));
						break;
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Exception while trying to deserialize SerializedAvatar entry " + item.Name);
					throw ex;
				}
			}
		}

		public SerializedAvatar(Texture2D tex)
		{
			X = tex.width;
			Y = tex.height;
			Bytes = tex.EncodeToPNG();
		}
	}

	public string PlatformPlayerID { get; private set; }

	public string PlatformAccountID { get; private set; }

	public string Username { get; private set; }

	public SerializedAvatar Avatar { get; private set; }

	public bool AvatarSet { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PlatformPlayerID", PlatformPlayerID);
		info.AddValue("PlatformAccountID", PlatformAccountID);
		info.AddValue("Username", Username);
		info.AddValue("Avatar", Avatar);
		info.AddValue("AvatarSet", AvatarSet);
	}

	public SaveOwner(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PlatformPlayerID":
					PlatformPlayerID = info.GetString("PlatformPlayerID");
					break;
				case "PlatformAccountID":
					PlatformAccountID = info.GetString("PlatformAccountID");
					break;
				case "Username":
					Username = info.GetString("Username");
					break;
				case "Avatar":
					Avatar = (SerializedAvatar)info.GetValue("Avatar", typeof(SerializedAvatar));
					break;
				case "AvatarSet":
					AvatarSet = info.GetBoolean("AvatarSet");
					break;
				case "SteamID":
					PlatformPlayerID = ((ulong)info.GetValue("SteamID", typeof(ulong))).ToString();
					break;
				case "AccountID":
					PlatformAccountID = info.GetUInt32("AccountID").ToString();
					break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize SaveOwner entry " + item.Name);
				throw ex;
			}
		}
	}

	public SaveOwner()
	{
		PlatformPlayerID = PlatformLayer.UserData.PlatformPlayerID;
		PlatformAccountID = PlatformLayer.UserData.PlatformAccountID;
		Username = PlatformLayer.UserData.UserName;
		Avatar = null;
	}

	public SaveOwner(string platformUserID, string platformAccountID, string username)
	{
		PlatformPlayerID = platformUserID;
		PlatformAccountID = platformAccountID;
		Username = username;
		Avatar = null;
	}

	public SaveOwner(MapStateSaveOwner saveOwner)
	{
		PlatformPlayerID = saveOwner.PlatformPlayerID;
		PlatformAccountID = saveOwner.PlatformAccountID;
		Username = saveOwner.Username;
		Avatar = null;
	}

	public MapStateSaveOwner ConvertToMapStateSaveOwner()
	{
		return new MapStateSaveOwner(PlatformPlayerID, PlatformAccountID, Username);
	}

	public void SetAvatar(Texture2D avatarTexture)
	{
		Avatar = (avatarTexture ? new SerializedAvatar(avatarTexture) : null);
		AvatarSet = true;
	}

	public async Task LoadAvatarFromPlatform()
	{
		bool done = false;
		PlatformLayer.Instance.NewPlatformLayer.GetOtherUserData(PlatformPlayerID, (PlatformUserData userData) =>
		{
			SetAvatar(userData.Avatar ? userData.Avatar.texture : null);
			done = true;
		});
		while (!done)
		{
			await Task.Yield();
		}
	}

	public IEnumerator LoadAvatarFromPlatformCrt()
	{
		bool done = false;
		PlatformLayer.Instance.NewPlatformLayer.GetOtherUserData(PlatformPlayerID, (PlatformUserData userData) =>
		{
			SetAvatar(userData.Avatar ? userData.Avatar.texture : null);
			done = true;
		});
		while (!done)
		{
			yield return null;
		}
	}
}

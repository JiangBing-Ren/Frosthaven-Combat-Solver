using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SETUtil;
using UnityEngine;
using UnityEngine.Networking;

namespace FH.Platforms;

public class NewPlatformStandaloneUser : NewPlatformUser
{
	private class UserProfile
	{
		public string PlayerID = "0";

		public string Name = "Player";

		public string Avatar = "player.jpg";
	}

	private UserProfile _userProfile;

	private Texture2D _avatarTexture;

	private Sprite _avatarSprite;

	private bool _avatarLoaded;

	private INewPlatformStorage _storage;

	public override string PlatformPlayerID => _userProfile.PlayerID;

	public override string PlatformAccountID => _userProfile.PlayerID;

	public override bool IsSignedIn => _userProfile != null;

	public override string Name => _userProfile.Name;

	public override PresenceStatus PlatformPresence => PresenceStatus.UnavailableOnPlatform;

	public void SetStorage(INewPlatformStorage storage)
	{
		_storage = storage;
	}

	public override IEnumerator SignIn()
	{
		string file = Path.Combine(GetProfileDir(), "player.json");
		if (File.Exists(file))
		{
			Task<string> task = File.ReadAllTextAsync(file);
			while (!task.IsCompleted)
			{
				yield return null;
			}
			Task<UserProfile> jsonTask = Task.Run(() => JsonConvert.DeserializeObject<UserProfile>(File.ReadAllText(file)));
			while (!jsonTask.IsCompleted)
			{
				yield return null;
			}
			if (jsonTask.Result != null)
			{
				_userProfile = jsonTask.Result;
			}
		}
		_userProfile = _userProfile ?? new UserProfile();
	}

	private UserProfile SignInInstant()
	{
		IEnumerator enumerator = SignIn();
		while (enumerator.MoveNext())
		{
		}
		return _userProfile;
	}

	public override void Dispose()
	{
		SceneUtil.SmartDestroy(_avatarTexture);
		_avatarTexture = null;
		SceneUtil.SmartDestroy(_avatarSprite);
		_avatarSprite = null;
	}

	private string GetProfileDir()
	{
		if (Application.isEditor)
		{
			return Path.GetDirectoryName(Application.dataPath);
		}
		return _storage.GetPersistentDataPath();
	}

	private IEnumerator LoadAvatarFromPathCrt(string file, Action<Sprite> onComplete)
	{
		_avatarLoaded = true;
		if (File.Exists(file))
		{
			UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + file);
			yield return www.SendWebRequest();
			_avatarTexture = DownloadHandlerTexture.GetContent(www);
			_avatarSprite = Sprite.Create(_avatarTexture, new Rect(0f, 0f, _avatarTexture.width, _avatarTexture.height), new Vector2(0.5f, 0.5f));
		}
		onComplete?.Invoke(_avatarSprite);
	}

	public override void GetAvatar(string playerId, Action<Sprite> onComplete)
	{
		if (_avatarLoaded)
		{
			onComplete?.Invoke(_avatarSprite);
			return;
		}
		string file = Path.Combine(GetProfileDir(), _userProfile.Avatar);
		PlatformLayer.Instance.StartCoroutine(LoadAvatarFromPathCrt(file, onComplete));
	}
}

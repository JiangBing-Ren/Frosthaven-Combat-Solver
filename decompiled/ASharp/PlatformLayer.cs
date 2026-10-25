using FH.Platforms;
using UnityEngine;

[ExecuteAlways]
public class PlatformLayer : MonoBehaviour
{
	public static class UserData
	{
		public static string UserName => Instance.NewPlatformLayer.GetCurrentUserName();

		public static string PlatformPlayerID => Instance.NewPlatformLayer.GetCurrentPlayerID();

		public static string PlatformAccountID => Instance.NewPlatformLayer.GetCurrentAccountID();

		public static bool IsSignedIn => Instance.NewPlatformLayer.IsSignedIn();

		public static void GetAvatarForSaveOwner(SaveOwner saveOwner)
		{
			Instance.NewPlatformLayer.GetOtherUserData(saveOwner.PlatformPlayerID, (PlatformUserData userData) =>
			{
				saveOwner.SetAvatar(userData.Avatar ? userData.Avatar.texture : null);
			});
		}

		public static bool CanLogOutEpicStore()
		{
			return false;
		}
	}

	public static PlatformLayer Instance;

	public NewPlatformLayer NewPlatformLayer = new NewPlatformLayer();

	private void Awake()
	{
		Instance = this;
		NewPlatformLayer.StartCoroutine(NewPlatformLayer.InitCrt());
	}

	private void Start()
	{
		if (Instance == null)
		{
			Awake();
		}
	}

	private void OnDestroy()
	{
		NewPlatformLayer.Dispose();
	}
}

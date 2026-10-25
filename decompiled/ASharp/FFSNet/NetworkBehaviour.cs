using UnityEngine;

namespace FFSNet;

public class NetworkBehaviour : MonoBehaviour
{
	[SerializeField]
	private NetworkManager networkManager;

	private void Awake()
	{
		Object.DontDestroyOnLoad(gameObject);
		StartCoroutine(FFSNetwork.InitializeCrt(networkManager, this));
	}

	private void OnDestroy()
	{
		FFSNetwork.Unintiialize();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (FFSNetwork.IsOnline)
		{
			Log.NetworkInfo("Application " + (hasFocus ? " GAINED " : " LOST ") + " focus.");
		}
	}
}

using System;
using System.Collections;
using UnityEngine;

namespace BestHTTP.Examples;

public sealed class AssetBundleSample : MonoBehaviour
{
	private Uri URI = new Uri(GUIHelper.BaseURL + "/AssetBundles/WebGL/demobundle.assetbundle");

	private string status = "Waiting for user interaction";

	private AssetBundle cachedBundle;

	private Texture2D texture;

	private bool downloading;

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, drawHeader: true, () =>
		{
			GUILayout.Label("Status: " + status);
			if (texture != null)
			{
				GUILayout.Box(texture, GUILayout.MaxHeight(256f));
			}
			if (!downloading && GUILayout.Button("Start Download"))
			{
				UnloadBundle();
				StartCoroutine(DownloadAssetBundle());
			}
		});
	}

	private void OnDestroy()
	{
		UnloadBundle();
	}

	private IEnumerator DownloadAssetBundle()
	{
		downloading = true;
		HTTPRequest request = new HTTPRequest(URI).Send();
		status = "Download started";
		while (request.State < HTTPRequestStates.Finished)
		{
			yield return new WaitForSeconds(0.1f);
			status += ".";
		}
		switch (request.State)
		{
		case HTTPRequestStates.Finished:
			if (request.Response.IsSuccess)
			{
				status = $"AssetBundle downloaded! Loaded from local cache: {request.Response.IsFromCache.ToString()}";
				AssetBundleCreateRequest async = AssetBundle.LoadFromMemoryAsync(request.Response.Data);
				yield return async;
				yield return StartCoroutine(ProcessAssetBundle(async.assetBundle));
			}
			else
			{
				status = $"Request finished Successfully, but the server sent an error. Status Code: {request.Response.StatusCode}-{request.Response.Message} Message: {request.Response.DataAsText}";
				Debug.LogWarning(status);
			}
			break;
		case HTTPRequestStates.Error:
			status = "Request Finished with Error! " + ((request.Exception != null) ? (request.Exception.Message + "\n" + request.Exception.StackTrace) : "No Exception");
			Debug.LogError(status);
			break;
		case HTTPRequestStates.Aborted:
			status = "Request Aborted!";
			Debug.LogWarning(status);
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			status = "Connection Timed Out!";
			Debug.LogError(status);
			break;
		case HTTPRequestStates.TimedOut:
			status = "Processing the request Timed Out!";
			Debug.LogError(status);
			break;
		}
		downloading = false;
	}

	private IEnumerator ProcessAssetBundle(AssetBundle bundle)
	{
		if (!(bundle == null))
		{
			cachedBundle = bundle;
			AssetBundleRequest asyncAsset = cachedBundle.LoadAssetAsync("9443182_orig", typeof(Texture2D));
			yield return asyncAsset;
			texture = asyncAsset.asset as Texture2D;
		}
	}

	private void UnloadBundle()
	{
		if (cachedBundle != null)
		{
			cachedBundle.Unload(unloadAllLoadedObjects: true);
			cachedBundle = null;
		}
	}
}

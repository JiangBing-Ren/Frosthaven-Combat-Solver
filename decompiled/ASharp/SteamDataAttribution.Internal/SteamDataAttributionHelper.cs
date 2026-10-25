using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SteamDataAttribution.Internal;

public class SteamDataAttributionHelper : MonoBehaviour
{
	public void SendForm(SteamDataAttributionConfig config)
	{
		if (!PlayerPrefs.HasKey("steamDataSuiteSuccess"))
		{
			StartCoroutine(HandleFormPost(config));
		}
	}

	private IEnumerator HandleFormPost(SteamDataAttributionConfig config)
	{
		List<IMultipartFormSection> multipartFormSections = new List<IMultipartFormSection>();
		string uri = "http://ldns.co/ca/" + config.LicenseKey + "/";
		UnityWebRequest www = UnityWebRequest.Post(uri, multipartFormSections);
		yield return www.SendWebRequest();
		if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
		{
			if (config.DebugMode)
			{
				Debug.LogError("SteamDataSuite Error: " + www.error);
			}
		}
		else if (www.isDone && www.responseCode == 200)
		{
			SaveData.Instance.Global.SteamDataSuiteAttributionDone = true;
			if (config.DebugMode)
			{
				Debug.Log("SteamDataSuite Succesful call");
				Debug.Log(www.downloadHandler.text);
			}
		}
		www.Dispose();
	}
}

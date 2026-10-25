using System;
using System.Collections.Generic;
using UnityEngine;

namespace BestHTTP.Examples;

public sealed class LargeFileDownloadSample : MonoBehaviour
{
	private const string URL = "https://uk3.testmy.net/dl-102400";

	private HTTPRequest request;

	private string status = string.Empty;

	private float progress;

	private int fragmentSize = 4096;

	private void Awake()
	{
		if (PlayerPrefs.HasKey("DownloadLength"))
		{
			progress = (float)PlayerPrefs.GetInt("DownloadProgress") / (float)PlayerPrefs.GetInt("DownloadLength");
		}
	}

	private void OnDestroy()
	{
		if (request != null && request.State < HTTPRequestStates.Finished)
		{
			request.OnProgress = null;
			request.Callback = null;
			request.Abort();
		}
	}

	private void OnGUI()
	{
		GUIHelper.DrawArea(GUIHelper.ClientArea, drawHeader: true, () =>
		{
			GUILayout.Label("Request status: " + status);
			GUILayout.Space(5f);
			GUILayout.Label(string.Format("Progress: {0:P2} of {1:N0}Mb", progress, PlayerPrefs.GetInt("DownloadLength") / 1048576));
			GUILayout.HorizontalSlider(progress, 0f, 1f);
			GUILayout.Space(50f);
			if (request == null)
			{
				GUILayout.Label($"Desired Fragment Size: {(float)fragmentSize / 1024f:N} KBytes");
				fragmentSize = (int)GUILayout.HorizontalSlider(fragmentSize, 4096f, 10485760f);
				GUILayout.Space(5f);
				if (GUILayout.Button(PlayerPrefs.HasKey("DownloadProgress") ? "Continue Download" : "Start Download"))
				{
					StreamLargeFileTest();
				}
			}
			else if (request.State == HTTPRequestStates.Processing && GUILayout.Button("Abort Download"))
			{
				request.Abort();
			}
		});
	}

	private void StreamLargeFileTest()
	{
		request = new HTTPRequest(new Uri("https://uk3.testmy.net/dl-102400"), (HTTPRequest req, HTTPResponse resp) =>
		{
			switch (req.State)
			{
			case HTTPRequestStates.Processing:
				if (!PlayerPrefs.HasKey("DownloadLength"))
				{
					string firstHeaderValue = resp.GetFirstHeaderValue("content-length");
					if (!string.IsNullOrEmpty(firstHeaderValue))
					{
						PlayerPrefs.SetInt("DownloadLength", int.Parse(firstHeaderValue));
					}
				}
				ProcessFragments(resp.GetStreamedFragments());
				status = "Processing";
				break;
			case HTTPRequestStates.Finished:
				if (resp.IsSuccess)
				{
					ProcessFragments(resp.GetStreamedFragments());
					if (resp.IsStreamingFinished)
					{
						status = "Streaming finished!";
						PlayerPrefs.DeleteKey("DownloadProgress");
						PlayerPrefs.Save();
						request = null;
					}
					else
					{
						status = "Processing";
					}
				}
				else
				{
					status = $"Request finished Successfully, but the server sent an error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}";
					Debug.LogWarning(status);
					request = null;
				}
				break;
			case HTTPRequestStates.Error:
				status = "Request Finished with Error! " + ((req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
				Debug.LogError(status);
				request = null;
				break;
			case HTTPRequestStates.Aborted:
				status = "Request Aborted!";
				Debug.LogWarning(status);
				request = null;
				break;
			case HTTPRequestStates.ConnectionTimedOut:
				status = "Connection Timed Out!";
				Debug.LogError(status);
				request = null;
				break;
			case HTTPRequestStates.TimedOut:
				status = "Processing the request Timed Out!";
				Debug.LogError(status);
				request = null;
				break;
			}
		});
		if (PlayerPrefs.HasKey("DownloadProgress"))
		{
			request.SetRangeHeader(PlayerPrefs.GetInt("DownloadProgress"));
		}
		else
		{
			PlayerPrefs.SetInt("DownloadProgress", 0);
		}
		request.DisableCache = true;
		request.UseStreaming = true;
		request.StreamFragmentSize = fragmentSize;
		request.Send();
	}

	private void ProcessFragments(List<byte[]> fragments)
	{
		if (fragments != null && fragments.Count > 0)
		{
			for (int i = 0; i < fragments.Count; i++)
			{
				int value = PlayerPrefs.GetInt("DownloadProgress") + fragments[i].Length;
				PlayerPrefs.SetInt("DownloadProgress", value);
			}
			PlayerPrefs.Save();
			progress = (float)PlayerPrefs.GetInt("DownloadProgress") / (float)PlayerPrefs.GetInt("DownloadLength");
		}
	}
}

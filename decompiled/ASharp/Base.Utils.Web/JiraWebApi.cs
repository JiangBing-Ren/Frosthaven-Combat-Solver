using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Base.Utils.Web;

public class JiraWebApi
{
	private const string JiraApiUri = "https://snapshotgames.atlassian.net/rest/api/2";

	private const string JiraIssueUri = "https://snapshotgames.atlassian.net/browse/{0}";

	private readonly NameValueCollection _params = new NameValueCollection();

	private readonly string _tempFolder;

	private readonly string _auth;

	public JiraWebApi(string auth, string tempFolder)
	{
		_auth = auth;
		_tempFolder = tempFolder;
	}

	public string CallGet(string method, float timeoutSeconds = 10f)
	{
		using TrustyWebClientWithTimeout trustyWebClientWithTimeout = new TrustyWebClientWithTimeout
		{
			TimeoutMilliseconds = (int)(timeoutSeconds * 1000f)
		};
		trustyWebClientWithTimeout.QueryString.Add(_params);
		trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.Authorization, "Basic " + _auth);
		trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.ContentType, "application/json");
		return trustyWebClientWithTimeout.DownloadString("https://snapshotgames.atlassian.net/rest/api/2/" + method);
	}

	public string CallPost(string method, string data, float timeoutSeconds = 10f)
	{
		using TrustyWebClientWithTimeout trustyWebClientWithTimeout = new TrustyWebClientWithTimeout
		{
			TimeoutMilliseconds = (int)(timeoutSeconds * 1000f)
		};
		if (data == null)
		{
			throw new InvalidOperationException("Calling web method with 'POST' verb without data");
		}
		trustyWebClientWithTimeout.QueryString.Add(_params);
		trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.Authorization, "Basic " + _auth);
		trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.ContentType, "application/json");
		string address = "https://snapshotgames.atlassian.net/rest/api/2/" + method;
		return trustyWebClientWithTimeout.UploadString(address, "POST", data);
	}

	private void UploadFile(string method, string fileName, string contentType, byte[] data, float timeoutSeconds = 10f)
	{
		if (data == null)
		{
			throw new InvalidOperationException("Calling web method with 'POST' verb without data");
		}
		string text = Path.Combine(_tempFolder, fileName);
		try
		{
			using (FileStream fileStream = File.Create(text))
			{
				fileStream.Write(data, 0, data.Length);
				fileStream.Close();
			}
			using TrustyWebClientWithTimeout trustyWebClientWithTimeout = new TrustyWebClientWithTimeout
			{
				TimeoutMilliseconds = (int)(timeoutSeconds * 1000f)
			};
			trustyWebClientWithTimeout.QueryString.Add(_params);
			trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.Authorization, "Basic " + _auth);
			trustyWebClientWithTimeout.Headers.Add(HttpRequestHeader.ContentType, contentType ?? "application/json");
			trustyWebClientWithTimeout.Headers.Add("X-Atlassian-Token", "nocheck");
			trustyWebClientWithTimeout.UploadFile("https://snapshotgames.atlassian.net/rest/api/2/" + method, "POST", text);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	public string CreateIssue(string project, string issueType, IDictionary<string, object> fields)
	{
		_params.Clear();
		_params.Add("projectKeys", project);
		_params.Add("issuetypeNames", issueType);
		_params.Add("expand", "projects.issuetypes.fields");
		IDictionary<string, string> dictionary = ((JObject.Parse(CallGet("issue/createmeta"))["projects"].FirstOrDefault((JToken p) => (string?)p["key"] == project) ?? throw new InvalidCredentialException("No matching project \"" + project + "\" found or credentials are invalid."))["issuetypes"].FirstOrDefault((JToken i) => (string?)i["name"] == issueType) ?? throw new InvalidCredentialException("No matching issue type \"" + issueType + "\" found."))["fields"].Where((JToken f) => (string?)((JProperty)f).Value["name"] != "Project" || (bool)((JProperty)f).Value["required"]).ToDictionary((JToken f) => (string?)((JProperty)f).Value["name"], (JToken f) => (string?)((JProperty)f).Value["key"]);
		JObject jObject = JObject.FromObject(new
		{
			fields = new
			{
				project = new
				{
					key = project
				},
				issuetype = new
				{
					name = issueType
				}
			}
		});
		foreach (KeyValuePair<string, object> field in fields)
		{
			if (!dictionary.TryGetValue(field.Key, out var value))
			{
				value = field.Key;
			}
			if (field.Value is Dictionary<string, object> dictionary2)
			{
				JObject jObject2 = new JObject();
				foreach (KeyValuePair<string, object> item in dictionary2)
				{
					jObject2[item.Key] = JToken.FromObject(item.Value);
				}
				jObject["fields"][value] = jObject2;
			}
			else
			{
				jObject["fields"][value] = JToken.FromObject(field.Value);
			}
		}
		string data = jObject.ToString(Formatting.None);
		return JObject.Parse(CallPost("issue", data)).GetValue<string>("key");
	}

	public void AttachFileToIssue(string issueId, string fileName, string contentType, byte[] fileData)
	{
		string method = "issue/" + issueId + "/attachments";
		UploadFile(method, fileName, contentType, fileData, 180f);
	}

	public static string GetIssueUrl(string key)
	{
		return $"https://snapshotgames.atlassian.net/browse/{key}";
	}

	public static IList<string> GetErrorsFromException(Exception e)
	{
		WebException ex = e as WebException;
		HttpStatusCode? httpStatusCode = (ex?.Response as HttpWebResponse)?.StatusCode;
		if (httpStatusCode.HasValue)
		{
			if (httpStatusCode.Value == HttpStatusCode.BadRequest)
			{
				try
				{
					string text = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					Debug.Log(text);
					return (from tk in ((JObject)JObject.Parse(text)["errors"]).Properties()
						select (string?)tk.Value).ToList();
				}
				catch (Exception)
				{
				}
			}
			if (httpStatusCode.Value == HttpStatusCode.RequestEntityTooLarge)
			{
				return new string[1] { "Request Entity Too Large" };
			}
		}
		return new string[0];
	}
}

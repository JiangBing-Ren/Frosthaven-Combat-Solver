using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Base.Utils.Web;
using FH;
using Libs.Build;
using Libs.Time;
using Libs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AmplitudeClient
{
	private const string AmplitudeApiUri = "https://api2.amplitude.com/2/httpapi";

	private const string AmplitudeApiKey = "45d35d9b56df461cd316c44700eadf70";

	private const string AmplitudeApiKeyUrl = "https://tickets.snapshotgames.com/EFHA";

	private const int EventsInQueueToBeginSending = 10;

	private const int SendRetriesCount = 10;

	private const int MaxEventsInRequest = 10;

	private static readonly TimeSpan RetryTime = TimeSpan.FromSeconds(180.0);

	private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10.0);

	private static AmplitudeClient _instance;

	private string _amplitudeApiKey = "45d35d9b56df461cd316c44700eadf70";

	private bool _forceEnabled;

	private System.Threading.Timer _eventsSendTimer;

	private readonly ConcurrentQueue<JToken> _events = new ConcurrentQueue<JToken>();

	private readonly DateTime _sessionStartTime;

	private TimeSpan _gameStartTime;

	private int _eventId = 1;

	private readonly RefCountedBool _suppressEvents = new RefCountedBool();

	public static AmplitudeClient Instance => _instance ?? (_instance = new AmplitudeClient());

	public bool Enabled
	{
		get
		{
			if (TelemetryAndEulaPolicy.IsTelemetryEnabled)
			{
				return !_suppressEvents;
			}
			return false;
		}
	}

	public bool UserHasAllowed
	{
		get
		{
			if (TelemetryAndEulaPolicy.IsTelemetryEnabled)
			{
				return !_suppressEvents;
			}
			return false;
		}
	}

	private AmplitudeClient()
	{
		_sessionStartTime = DateTime.UtcNow;
		_amplitudeApiKey = string.Empty;
	}

	public void IncSuppressEvents()
	{
		_suppressEvents.IncRef();
	}

	public void DecSuppressEvents()
	{
		_suppressEvents.DecRef();
	}

	public void LogEvent(string eventType, Dictionary<string, object> properties)
	{
		if (TelemetryAndEulaPolicy.IsTelemetryEnabled)
		{
			JObject properties2 = JObject.FromObject(properties);
			JObject ev = CreateEvent(eventType, properties2);
			AddEvent(ev);
		}
	}

	private void ProcessEvents(object state)
	{
		List<JToken> list = null;
		JToken result;
		while ((list == null || list.Count < 10) && _events.TryDequeue(out result))
		{
			if (list == null)
			{
				list = new List<JToken>();
			}
			list.Add(result);
		}
		if (list != null)
		{
			if (list.Count == 1)
			{
				new Task(SendEventTask, list[0]).Start();
			}
			else
			{
				JArray jArray = new JArray();
				foreach (JToken item in list)
				{
					jArray.Add(item);
				}
				new Task(SendEventTask, jArray).Start();
			}
		}
		_eventsSendTimer.Change(CheckInterval, TimeSpan.FromMilliseconds(-1.0));
	}

	private JObject CreateEvent(string eventType, JObject properties = null)
	{
		JObject jObject = new JObject
		{
			["device_id"] = SystemInfo.deviceUniqueIdentifier,
			["session_id"] = _sessionStartTime.ToUnixTime(),
			["event_id"] = _eventId++,
			["insert_id"] = Guid.NewGuid().ToString(),
			["event_type"] = eventType,
			["time"] = DateTime.UtcNow.ToUnixTime(),
			["app_version"] = RuntimeBuildInfo.BuildVersion,
			["platform"] = RuntimeBuildInfo.Platform,
			["os_name"] = RuntimeBuildInfo.OperatingSystem,
			["ip"] = "$remote"
		};
		if (properties != null)
		{
			jObject["event_properties"] = properties;
		}
		return jObject;
	}

	private void AddEvent(JObject ev)
	{
		if (_eventsSendTimer == null)
		{
			_eventsSendTimer = new System.Threading.Timer(ProcessEvents, null, CheckInterval, TimeSpan.FromMilliseconds(-1.0));
		}
		_events.Enqueue(ev);
		if (_events.Count >= 10)
		{
			FlushEvents();
		}
	}

	private void FlushEvents()
	{
		_eventsSendTimer.Change(0, -1);
	}

	private void SendEventTask(object param)
	{
		if (_amplitudeApiKey == string.Empty)
		{
			try
			{
				using TrustyWebClientWithTimeout trustyWebClientWithTimeout = new TrustyWebClientWithTimeout
				{
					TimeoutMilliseconds = 10000
				};
				string cipherText = trustyWebClientWithTimeout.DownloadString("https://tickets.snapshotgames.com/EFHA");
				_amplitudeApiKey = AesEncryptor.DecryptString(cipherText);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Cannot retrieve Amplitude API key: " + ex);
				_amplitudeApiKey = "45d35d9b56df461cd316c44700eadf70";
			}
		}
		JToken jToken = (JToken)param;
		Debug.Log("Sending analytics event: " + jToken.ToString(Formatting.None));
		int num = 10;
		while (num > 0)
		{
			using (TrustyWebClientWithTimeout trustyWebClientWithTimeout2 = new TrustyWebClientWithTimeout())
			{
				trustyWebClientWithTimeout2.Encoding = Encoding.UTF8;
				JObject jObject = new JObject
				{
					["api_key"] = _amplitudeApiKey,
					["events"] = ((jToken is JArray) ? jToken : new JArray { jToken })
				};
				try
				{
					string text = trustyWebClientWithTimeout2.UploadString("https://api2.amplitude.com/2/httpapi", "POST", jObject.ToString(Formatting.None));
					try
					{
						JToken jToken2 = JToken.Parse(text)["code"];
						if (jToken2 != null && jToken2.Value<int>() == 200)
						{
							break;
						}
					}
					catch (Exception ex2)
					{
						Debug.LogWarning("Cannot parse response from Amplitude: " + ex2);
					}
					Debug.LogWarning("Cannot send events to Amplitude, the response is: " + text);
				}
				catch (WebException ex3)
				{
					HttpStatusCode? httpStatusCode = (ex3?.Response as HttpWebResponse)?.StatusCode;
					if (httpStatusCode.HasValue)
					{
						if (httpStatusCode.Value == HttpStatusCode.BadRequest)
						{
							Instance.IncSuppressEvents();
							Debug.LogError("Received BadRequest response from Amplitude. Stop sending future events: " + ex3);
							break;
						}
					}
					else
					{
						Debug.LogWarning("Cannot send events to Amplitude: " + ex3);
					}
				}
				catch (Exception ex4)
				{
					Debug.LogWarning("Cannot send events to Amplitude: " + ex4);
				}
			}
			num--;
			Task.Delay(RetryTime);
		}
	}
}

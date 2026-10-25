using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using BestHTTP.Authentication;
using BestHTTP.Caching;
using BestHTTP.Cookies;
using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.TcpClient.General;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Tls;

namespace BestHTTP;

internal sealed class HTTPConnection : ConnectionBase
{
	private TcpClient Client;

	private Stream Stream;

	private KeepAliveHeader KeepAlive;

	public override bool IsRemovable
	{
		get
		{
			if (base.IsRemovable)
			{
				return true;
			}
			if (IsFree && KeepAlive != null && DateTime.UtcNow - LastProcessTime >= KeepAlive.TimeOut)
			{
				return true;
			}
			return false;
		}
	}

	internal HTTPConnection(string serverAddress)
		: base(serverAddress)
	{
	}

	protected override void ThreadFunc(object param)
	{
		bool flag = false;
		bool flag2 = false;
		RetryCauses retryCauses = RetryCauses.None;
		try
		{
			if (TryLoadAllFromCache())
			{
				return;
			}
			if (Client != null && !Client.IsConnected())
			{
				Close();
			}
			do
			{
				if (retryCauses == RetryCauses.Reconnect)
				{
					Close();
					Thread.Sleep(100);
				}
				LastProcessedUri = CurrentRequest.CurrentUri;
				retryCauses = RetryCauses.None;
				Connect();
				if (State == HTTPConnectionStates.AbortRequested)
				{
					throw new Exception("AbortRequested");
				}
				if (!CurrentRequest.DisableCache)
				{
					HTTPCacheService.SetHeaders(CurrentRequest);
				}
				bool flag3 = false;
				try
				{
					Client.NoDelay = CurrentRequest.TryToMinimizeTCPLatency;
					CurrentRequest.SendOutTo(Stream);
					flag3 = true;
				}
				catch (Exception ex)
				{
					Close();
					if (State == HTTPConnectionStates.TimedOut || State == HTTPConnectionStates.AbortRequested)
					{
						throw new Exception("AbortRequested");
					}
					if (flag || CurrentRequest.DisableRetry)
					{
						throw ex;
					}
					flag = true;
					retryCauses = RetryCauses.Reconnect;
				}
				if (!flag3)
				{
					continue;
				}
				bool flag4 = Receive();
				if (State == HTTPConnectionStates.TimedOut || State == HTTPConnectionStates.AbortRequested)
				{
					throw new Exception("AbortRequested");
				}
				if (!flag4 && !flag && !CurrentRequest.DisableRetry)
				{
					flag = true;
					retryCauses = RetryCauses.Reconnect;
				}
				if (CurrentRequest.Response == null)
				{
					continue;
				}
				if (CurrentRequest.IsCookiesEnabled)
				{
					CookieJar.Set(CurrentRequest.Response);
					CookieJar.Persist();
				}
				switch (CurrentRequest.Response.StatusCode)
				{
				case 401:
				{
					string text2 = DigestStore.FindBest(CurrentRequest.Response.GetHeaderValues("www-authenticate"));
					if (!string.IsNullOrEmpty(text2))
					{
						Digest orCreate2 = DigestStore.GetOrCreate(CurrentRequest.CurrentUri);
						orCreate2.ParseChallange(text2);
						if (CurrentRequest.Credentials != null && orCreate2.IsUriProtected(CurrentRequest.CurrentUri) && (!CurrentRequest.HasHeader("Authorization") || orCreate2.Stale))
						{
							retryCauses = RetryCauses.Authenticate;
						}
					}
					break;
				}
				case 407:
				{
					if (!CurrentRequest.HasProxy)
					{
						break;
					}
					string text = DigestStore.FindBest(CurrentRequest.Response.GetHeaderValues("proxy-authenticate"));
					if (!string.IsNullOrEmpty(text))
					{
						Digest orCreate = DigestStore.GetOrCreate(CurrentRequest.Proxy.Address);
						orCreate.ParseChallange(text);
						if (CurrentRequest.Proxy.Credentials != null && orCreate.IsUriProtected(CurrentRequest.Proxy.Address) && (!CurrentRequest.HasHeader("Proxy-Authorization") || orCreate.Stale))
						{
							retryCauses = RetryCauses.ProxyAuthenticate;
						}
					}
					break;
				}
				case 301:
				case 302:
				case 307:
				case 308:
					if (CurrentRequest.RedirectCount < CurrentRequest.MaxRedirects)
					{
						CurrentRequest.RedirectCount++;
						string firstHeaderValue = CurrentRequest.Response.GetFirstHeaderValue("location");
						if (string.IsNullOrEmpty(firstHeaderValue))
						{
							throw new MissingFieldException($"Got redirect status({CurrentRequest.Response.StatusCode.ToString()}) without 'location' header!");
						}
						Uri redirectUri = GetRedirectUri(firstHeaderValue);
						if (HTTPManager.Logger.Level == Loglevels.All)
						{
							HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - Redirected to Location: '{1}' redirectUri: '{1}'", CurrentRequest.CurrentUri.ToString(), firstHeaderValue, redirectUri));
						}
						if (!CurrentRequest.CallOnBeforeRedirection(redirectUri))
						{
							HTTPManager.Logger.Information("HTTPConnection", "OnBeforeRedirection returned False");
							break;
						}
						CurrentRequest.RemoveHeader("Host");
						CurrentRequest.SetHeader("Referer", CurrentRequest.CurrentUri.ToString());
						CurrentRequest.RedirectUri = redirectUri;
						CurrentRequest.Response = null;
						bool flag5 = (CurrentRequest.IsRedirected = true);
						flag2 = flag5;
					}
					break;
				}
				TryStoreInCache();
				if (CurrentRequest.Response != null && CurrentRequest.Response.IsClosedManually)
				{
					continue;
				}
				bool num = CurrentRequest.Response == null || CurrentRequest.Response.HasHeaderWithValue("connection", "close");
				bool flag7 = !CurrentRequest.IsKeepAlive;
				if (num | flag7)
				{
					Close();
				}
				else
				{
					if (CurrentRequest.Response == null)
					{
						continue;
					}
					List<string> headerValues = CurrentRequest.Response.GetHeaderValues("keep-alive");
					if (headerValues != null && headerValues.Count > 0)
					{
						if (KeepAlive == null)
						{
							KeepAlive = new KeepAliveHeader();
						}
						KeepAlive.Parse(headerValues);
					}
				}
			}
			while (retryCauses != RetryCauses.None);
		}
		catch (TimeoutException exception)
		{
			CurrentRequest.Response = null;
			CurrentRequest.Exception = exception;
			CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
			Close();
		}
		catch (Exception exception2)
		{
			if (CurrentRequest != null)
			{
				if (CurrentRequest.UseStreaming)
				{
					HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);
				}
				CurrentRequest.Response = null;
				switch (State)
				{
				case HTTPConnectionStates.AbortRequested:
				case HTTPConnectionStates.Closed:
					CurrentRequest.State = HTTPRequestStates.Aborted;
					break;
				case HTTPConnectionStates.TimedOut:
					CurrentRequest.State = HTTPRequestStates.TimedOut;
					break;
				default:
					CurrentRequest.Exception = exception2;
					CurrentRequest.State = HTTPRequestStates.Error;
					break;
				}
			}
			Close();
		}
		finally
		{
			if (CurrentRequest != null)
			{
				lock (HTTPManager.Locker)
				{
					if (CurrentRequest != null && CurrentRequest.Response != null && CurrentRequest.Response.IsUpgraded)
					{
						State = HTTPConnectionStates.Upgraded;
					}
					else
					{
						HTTPConnection hTTPConnection = this;
						HTTPConnectionStates state;
						if (flag2)
						{
							state = HTTPConnectionStates.Redirected;
						}
						else
						{
							state = ((Client == null) ? HTTPConnectionStates.Closed : HTTPConnectionStates.WaitForRecycle);
						}
						hTTPConnection.State = state;
					}
					if (CurrentRequest.State == HTTPRequestStates.Processing && (State == HTTPConnectionStates.Closed || State == HTTPConnectionStates.WaitForRecycle))
					{
						if (CurrentRequest.Response != null)
						{
							CurrentRequest.State = HTTPRequestStates.Finished;
						}
						else
						{
							CurrentRequest.Exception = new Exception($"Remote server closed the connection before sending response header! Previous request state: {CurrentRequest.State.ToString()}. Connection state: {State.ToString()}");
							CurrentRequest.State = HTTPRequestStates.Error;
						}
					}
					if (CurrentRequest.State == HTTPRequestStates.ConnectionTimedOut)
					{
						State = HTTPConnectionStates.Closed;
					}
					LastProcessTime = DateTime.UtcNow;
					if (OnConnectionRecycled != null)
					{
						RecycleNow();
					}
				}
			}
		}
	}

	private void Connect()
	{
		Uri uri = (CurrentRequest.HasProxy ? CurrentRequest.Proxy.Address : CurrentRequest.CurrentUri);
		if (Client == null)
		{
			Client = new TcpClient();
		}
		if (!Client.Connected)
		{
			Client.ConnectTimeout = CurrentRequest.ConnectTimeout;
			if (HTTPManager.Logger.Level == Loglevels.All)
			{
				HTTPManager.Logger.Verbose("HTTPConnection", $"'{CurrentRequest.CurrentUri.ToString()}' - Connecting to {uri.Host}:{uri.Port.ToString()}");
			}
			Client.SendBufferSize = HTTPManager.SendBufferSize;
			Client.ReceiveBufferSize = HTTPManager.ReceiveBufferSize;
			if (HTTPManager.Logger.Level == Loglevels.All)
			{
				HTTPManager.Logger.Verbose("HTTPConnection", $"'{CurrentRequest.CurrentUri.ToString()}' - Buffer sizes - Send: {Client.SendBufferSize.ToString()} Receive: {Client.ReceiveBufferSize.ToString()} Blocking: {Client.Client.Blocking.ToString()}");
			}
			Client.Connect(uri.Host, uri.Port);
			if ((int)HTTPManager.Logger.Level <= 1)
			{
				HTTPManager.Logger.Information("HTTPConnection", "Connected to " + uri.Host + ":" + uri.Port);
			}
		}
		else if ((int)HTTPManager.Logger.Level <= 1)
		{
			HTTPManager.Logger.Information("HTTPConnection", "Already connected to " + uri.Host + ":" + uri.Port);
		}
		StartTime = DateTime.UtcNow;
		if (Stream != null)
		{
			return;
		}
		bool flag = HTTPProtocolFactory.IsSecureProtocol(CurrentRequest.CurrentUri);
		Stream = Client.GetStream();
		if (CurrentRequest.Proxy != null)
		{
			CurrentRequest.Proxy.Connect(Stream, CurrentRequest);
		}
		if (!flag)
		{
			return;
		}
		if (CurrentRequest.UseAlternateSSL)
		{
			TlsClientProtocol tlsClientProtocol = new TlsClientProtocol(Client.GetStream(), new SecureRandom());
			List<string> list = CurrentRequest.CustomTLSServerNameList;
			if ((list == null || list.Count == 0) && !CurrentRequest.CurrentUri.IsHostIsAnIPAddress())
			{
				list = new List<string>(1);
				list.Add(CurrentRequest.CurrentUri.Host);
			}
			Uri currentUri = CurrentRequest.CurrentUri;
			ICertificateVerifyer verifyer;
			if (CurrentRequest.CustomCertificateVerifyer != null)
			{
				verifyer = CurrentRequest.CustomCertificateVerifyer;
			}
			else
			{
				ICertificateVerifyer certificateVerifyer = new AlwaysValidVerifyer();
				verifyer = certificateVerifyer;
			}
			tlsClientProtocol.Connect(new LegacyTlsClient(currentUri, verifyer, CurrentRequest.CustomClientCredentialsProvider, list));
			Stream = tlsClientProtocol.Stream;
		}
		else
		{
			SslStream sslStream = new SslStream(Client.GetStream(), leaveInnerStreamOpen: false, (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) => CurrentRequest.CallCustomCertificationValidator(cert, chain));
			if (!sslStream.IsAuthenticated)
			{
				sslStream.AuthenticateAsClient(CurrentRequest.CurrentUri.Host);
			}
			Stream = sslStream;
		}
	}

	private bool Receive()
	{
		SupportedProtocols protocol = ((CurrentRequest.ProtocolHandler == SupportedProtocols.Unknown) ? HTTPProtocolFactory.GetProtocolFromUri(CurrentRequest.CurrentUri) : CurrentRequest.ProtocolHandler);
		if (HTTPManager.Logger.Level == Loglevels.All)
		{
			HTTPManager.Logger.Verbose("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - Receive - protocol: {protocol.ToString()}");
		}
		CurrentRequest.Response = HTTPProtocolFactory.Get(protocol, CurrentRequest, Stream, CurrentRequest.UseStreaming, isFromCache: false);
		if (!CurrentRequest.Response.Receive())
		{
			if (HTTPManager.Logger.Level == Loglevels.All)
			{
				HTTPManager.Logger.Verbose("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - Receive - Failed! Response will be null, returning with false.");
			}
			CurrentRequest.Response = null;
			return false;
		}
		if (CurrentRequest.Response.StatusCode == 304 && !CurrentRequest.DisableCache)
		{
			if (CurrentRequest.IsRedirected)
			{
				if (!LoadFromCache(CurrentRequest.RedirectUri))
				{
					LoadFromCache(CurrentRequest.Uri);
				}
			}
			else
			{
				LoadFromCache(CurrentRequest.Uri);
			}
		}
		if (HTTPManager.Logger.Level == Loglevels.All)
		{
			HTTPManager.Logger.Verbose("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - Receive - Finished Successfully!");
		}
		return true;
	}

	private bool LoadFromCache(Uri uri)
	{
		if (HTTPManager.Logger.Level == Loglevels.All)
		{
			HTTPManager.Logger.Verbose("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - LoadFromCache for Uri: {uri.ToString()}");
		}
		HTTPCacheFileInfo entity = HTTPCacheService.GetEntity(uri);
		if (entity == null)
		{
			HTTPManager.Logger.Warning("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - LoadFromCache for Uri: {uri.ToString()} - Cached entity not found!");
			return false;
		}
		CurrentRequest.Response.CacheFileInfo = entity;
		int length;
		using (Stream stream = entity.GetBodyStream(out length))
		{
			if (stream == null)
			{
				return false;
			}
			if (!CurrentRequest.Response.HasHeader("content-length"))
			{
				CurrentRequest.Response.Headers.Add("content-length", new List<string>(1) { length.ToString() });
			}
			CurrentRequest.Response.IsFromCache = true;
			if (!CurrentRequest.CacheOnly)
			{
				CurrentRequest.Response.ReadRaw(stream, length);
			}
		}
		return true;
	}

	private bool TryLoadAllFromCache()
	{
		if (CurrentRequest.DisableCache || !HTTPCacheService.IsSupported)
		{
			return false;
		}
		try
		{
			if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(CurrentRequest))
			{
				if (HTTPManager.Logger.Level == Loglevels.All)
				{
					HTTPManager.Logger.Verbose("HTTPConnection", $"{CurrentRequest.CurrentUri.ToString()} - TryLoadAllFromCache - whole response loading from cache");
				}
				CurrentRequest.Response = HTTPCacheService.GetFullResponse(CurrentRequest);
				if (CurrentRequest.Response != null)
				{
					return true;
				}
			}
		}
		catch
		{
			HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);
		}
		return false;
	}

	private void TryStoreInCache()
	{
		if (!CurrentRequest.UseStreaming && !CurrentRequest.DisableCache && CurrentRequest.Response != null && HTTPCacheService.IsSupported && HTTPCacheService.IsCacheble(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response))
		{
			if (CurrentRequest.IsRedirected)
			{
				HTTPCacheService.Store(CurrentRequest.Uri, CurrentRequest.MethodType, CurrentRequest.Response);
			}
			else
			{
				HTTPCacheService.Store(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response);
			}
			HTTPCacheService.SaveLibrary();
		}
	}

	private Uri GetRedirectUri(string location)
	{
		Uri uri = null;
		try
		{
			uri = new Uri(location);
			if (uri.IsFile || uri.AbsolutePath == location)
			{
				uri = null;
			}
		}
		catch (UriFormatException)
		{
			uri = null;
		}
		if (uri == null)
		{
			Uri uri2 = CurrentRequest.Uri;
			uri = new UriBuilder(uri2.Scheme, uri2.Host, uri2.Port, location).Uri;
		}
		return uri;
	}

	internal override void Abort(HTTPConnectionStates newState)
	{
		State = newState;
		if (State == HTTPConnectionStates.TimedOut)
		{
			TimedOutStart = DateTime.UtcNow;
		}
		if (Stream != null)
		{
			try
			{
				Stream.Dispose();
			}
			catch
			{
			}
		}
	}

	private void Close()
	{
		KeepAlive = null;
		LastProcessedUri = null;
		if (Client == null)
		{
			return;
		}
		try
		{
			Client.Close();
		}
		catch
		{
		}
		finally
		{
			Stream = null;
			Client = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		Close();
		base.Dispose(disposing);
	}
}

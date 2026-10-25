using System;
using System.IO;
using BestHTTP.PlatformSupport.FileSystem;

namespace BestHTTP;

internal sealed class FileConnection : ConnectionBase
{
	public FileConnection(string serverAddress)
		: base(serverAddress)
	{
	}

	internal override void Abort(HTTPConnectionStates newState)
	{
		State = newState;
		if (State == HTTPConnectionStates.TimedOut)
		{
			TimedOutStart = DateTime.UtcNow;
		}
		throw new NotImplementedException();
	}

	protected override void ThreadFunc(object param)
	{
		try
		{
			using Stream stream = HTTPManager.IOService.CreateFileStream(CurrentRequest.CurrentUri.LocalPath, FileStreamModes.Open);
			using StreamList streamList = new StreamList(new MemoryStream(), stream);
			streamList.Write("HTTP/1.1 200 Ok\r\n");
			streamList.Write("Content-Type: application/octet-stream\r\n");
			streamList.Write("Content-Length: " + stream.Length + "\r\n");
			streamList.Write("\r\n");
			streamList.Seek(0L, SeekOrigin.Begin);
			CurrentRequest.Response = new HTTPResponse(CurrentRequest, streamList, CurrentRequest.UseStreaming, isFromCache: false);
			if (!CurrentRequest.Response.Receive())
			{
				CurrentRequest.Response = null;
			}
		}
		catch (Exception exception)
		{
			if (CurrentRequest != null)
			{
				CurrentRequest.Response = null;
				switch (State)
				{
				case HTTPConnectionStates.AbortRequested:
					CurrentRequest.State = HTTPRequestStates.Aborted;
					break;
				case HTTPConnectionStates.TimedOut:
					CurrentRequest.State = HTTPRequestStates.TimedOut;
					break;
				default:
					CurrentRequest.Exception = exception;
					CurrentRequest.State = HTTPRequestStates.Error;
					break;
				}
			}
		}
		finally
		{
			State = HTTPConnectionStates.Closed;
			if (CurrentRequest.State == HTTPRequestStates.Processing)
			{
				if (CurrentRequest.Response != null)
				{
					CurrentRequest.State = HTTPRequestStates.Finished;
				}
				else
				{
					CurrentRequest.State = HTTPRequestStates.Error;
				}
			}
		}
	}
}

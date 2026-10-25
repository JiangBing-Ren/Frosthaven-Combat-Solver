using FFSNet.Backend;

namespace FFSNet;

public class DataStreamingManager : Singleton<DataStreamingManager>
{
	private ByteChunkifier chunkifier;

	public void CompressAndSendDataInChunks(DataActionType dataActionType, byte[] data, NetworkTargets targets, NetworkBackendPlayer connection = null)
	{
		switch (targets)
		{
		case NetworkTargets.None:
			Log.NetworkError("ERROR_MULTIPLAYER_00020", "Network target for sending custom data uninitialized. Returning.");
			return;
		case NetworkTargets.TargetClient:
			if (connection == null)
			{
				Log.NetworkError("ERROR_MULTIPLAYER_00021", "Trying to send data to null connection. Returning.");
				return;
			}
			break;
		}
		data = Utility.CompressData(data);
		int num = 600;
		ByteChunkifier byteChunkifier = new ByteChunkifier(data, num);
		int num2 = 0;
		Log.NetworkInfo("Started streaming data to " + ((targets == NetworkTargets.TargetClient) ? connection.ToString() : targets.ToString()));
		byte[] buffer;
		while (byteChunkifier.ReadChunk(out buffer))
		{
			Log.NetworkInfo("Sending Data: " + num2 + ", " + (num2 + 1) * num + " / " + data.Length);
			num2++;
		}
	}

	public byte[] ReassembleData(GameDataEventToken evnt)
	{
		if (chunkifier == null)
		{
			chunkifier = new ByteChunkifier(evnt.TotalSize, evnt.ChunkSize);
		}
		Log.NetworkInfo("Receiving Data: " + evnt.ChunkIndex + ", " + (evnt.ChunkIndex + 1) * evnt.ChunkSize + " / " + evnt.TotalSize);
		if (evnt.Complete)
		{
			byte[] result = Utility.DecompressData(chunkifier.Data);
			chunkifier = null;
			Log.NetworkInfo("All data received.");
			return result;
		}
		return null;
	}
}

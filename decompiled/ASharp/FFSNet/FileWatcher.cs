using System;
using System.IO;

namespace FFSNet;

public static class FileWatcher
{
	private static FileSystemWatcher watcher;

	private static Action<string> onNewFileIDGenerated;

	public static void Start(string path, Action<string> onFileIDChanged)
	{
		onNewFileIDGenerated = onFileIDChanged;
		watcher = new FileSystemWatcher();
		watcher.Path = path;
		watcher.NotifyFilter = NotifyFilters.LastWrite;
		watcher.Changed += OnChanged;
		watcher.EnableRaisingEvents = true;
	}

	public static void Stop()
	{
		if (watcher != null)
		{
			watcher.EnableRaisingEvents = false;
			watcher.Changed -= OnChanged;
		}
	}

	private static void OnChanged(object source, FileSystemEventArgs e)
	{
		if (!FFSNetwork.IsOnline)
		{
			string randomCode = Utility.GetRandomCode(16);
			onNewFileIDGenerated?.Invoke(randomCode);
			Log.NetworkInfo("New save file ID generated: " + randomCode);
		}
	}
}

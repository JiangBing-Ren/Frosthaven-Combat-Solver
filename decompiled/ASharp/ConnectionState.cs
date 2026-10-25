public enum ConnectionState
{
	None,
	StartingClient,
	SearchingForSession,
	SessionFound,
	Connecting,
	WaitUntilSavePoint,
	SavePointReached,
	DownloadingNewSave,
	CancelingConnection,
	DownloadingRuleset
}

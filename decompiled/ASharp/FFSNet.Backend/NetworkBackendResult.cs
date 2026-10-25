namespace FFSNet.Backend;

public struct NetworkBackendResult
{
	public string ErrorMessage;

	public int ErrorCode;

	public bool Ok => ErrorCode == 0;
}

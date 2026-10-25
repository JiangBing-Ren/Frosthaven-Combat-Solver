using System;

namespace FHGUI.DTO;

public class ConfirmActionDTO
{
	public Action<Action, Action, string, string> ExecuteAction;

	public string TitleKey;

	public string DescKey;

	public void Execute(Action confirm, Action cancel)
	{
		ExecuteAction?.Invoke(confirm, cancel, TitleKey, DescKey);
	}
}

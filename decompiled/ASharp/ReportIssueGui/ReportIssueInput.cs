using System.Collections;
using FH.GameConsole;
using Libs.GameConsole;
using UnityEngine;

namespace ReportIssueGui;

public class ReportIssueInput : MonoBehaviour
{
	public delegate void OnCompleteAction(bool errorReportSent);

	private ReportIssueWindow _window;

	private bool _prevVisible;

	public void Awake()
	{
		_window = GetComponentInChildren<ReportIssueWindow>();
	}

	private void Update()
	{
		if (_prevVisible != _window.Visible)
		{
			if (_window.Visible)
			{
				GameConsoleInput.Instance.EnableInput(enable: false);
			}
			else
			{
				GameConsoleInput.Instance.EnableInput(enable: true);
			}
			_prevVisible = _window.Visible;
		}
		if (Input.GetKeyDown(KeyCode.F10) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
		{
			_window.ToggleVisibility();
		}
	}

	[ConsoleCommand(Command = "open_report_window", Description = "Opens the F10 report issue window")]
	public static void OpenWithConsole(IConsole console)
	{
		ReportIssueWindow reportIssueWindow = Object.FindObjectOfType<ReportIssueWindow>();
		if (reportIssueWindow == null)
		{
			console.WriteLine("Failed to find an instance of ReportIssueWindow");
		}
		else
		{
			reportIssueWindow.ToggleVisibility();
		}
	}

	public void ShowAndWait(OnCompleteAction onCompleteAction)
	{
		ReportIssueWindow reportIssueWindow = Object.FindObjectOfType<ReportIssueWindow>();
		if (!reportIssueWindow.Visible)
		{
			reportIssueWindow.ToggleVisibility();
		}
		StartCoroutine(ShowAndWaitCrt(onCompleteAction));
	}

	private IEnumerator ShowAndWaitCrt(OnCompleteAction onCompleteAction)
	{
		yield return null;
		ReportIssueWindow instance = Object.FindObjectOfType<ReportIssueWindow>();
		while (instance.Visible)
		{
			yield return null;
		}
		onCompleteAction?.Invoke(instance.ReportSentSuccessfuly);
	}
}

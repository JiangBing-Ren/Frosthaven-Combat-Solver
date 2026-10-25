using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FH;
using I2.Loc;
using Libs.Build;
using Libs.GameConsole;
using Libs.Scripting;
using Libs.Threading;
using Logging;
using ReportIssueGui;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
	[Tooltip("Delay for three frames to ensure every background process has init and does not lag the splashscreen.")]
	public bool DelaySplashscreen = true;

	private AsyncOperation _loadScene;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	[ConsoleVariable(Alias = "show_splash_in_editor")]
	public static bool ShowSplashInEditor_cvar;

	[ConsoleVariable(Alias = "no_splash")]
	public static bool SkipSplash_cvar;

	private bool _hasQGL;

	[ConsoleCommand(Command = "localize")]
	public static void Localize(IConsole console, string key, string languageCode = null)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (LocalizationManager.GetTermData(key) == null)
		{
			console.WriteLineWithStyle("Could not find key '" + key + "'", CommandOutputStyle.Error);
		}
		else
		{
			console.WriteLine("'" + LocalizationManager.GetTranslation(key, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, languageCode) + "'");
		}
		stopwatch.Stop();
		console.WriteLine($"Localize Time: {stopwatch.ElapsedMilliseconds:F2} ms");
	}

	[ConsoleCommand(Command = "localize_test")]
	public static void LocalizeTest(IConsole console, int count = 100)
	{
		Stopwatch stopwatch = new Stopwatch();
		List<string> list = (from s in LocalizationManager.GetTermsList()
			select LanguageSourceData.GetKeyFromFullTerm(s)).ToList();
		stopwatch.Start();
		list.Reverse();
		if (count > 0)
		{
			list = list.Take(count).ToList();
		}
		foreach (string item in list)
		{
			LocalizationManager.GetTranslation(item);
		}
		stopwatch.Stop();
		console.WriteLine($"Localize time for {list.Count} keys: {stopwatch.ElapsedMilliseconds:F2} ms");
	}

	[ConsoleCommand(Command = "toggle_job_system")]
	public static void ToggleJobSystem(IConsole console)
	{
		JobSystem.Enabled = !JobSystem.Enabled;
		console.WriteLine("Job system is " + (JobSystem.Enabled ? "enabled" : "disabled"));
	}

	private IEnumerator Start()
	{
		TelemetryAndEulaPolicy.Cache();
		GloomhavenShared.LogBuildInfo();
		GameConsoleWindow.Create();
		ReportIssueWindow.Create();
		ScriptingConsoleCommands.Create();
		GetComponent<ThreadManagerComponent>().Initialize();
		GetComponent<LogHandler>().Initialize();
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			Task<string> globalSavePathTask = FHTaskExtensions.CombineAsyncPaths(RootSaveData.SaveRootFolder, "GlobalData.dat");
			yield return new WaitForTask(globalSavePathTask);
			if (!File.Exists(globalSavePathTask.Result))
			{
				Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
			}
		}
		_stopwatch.Restart();
		Log.Info(string.Format(GetVersionLoc(), RuntimeBuildInfo.UserVersion));
		Log.Info("[Bootstrap] Load scene started.");
		_loadScene = SceneManager.LoadSceneAsync("Gloomhaven", LoadSceneMode.Single);
		_loadScene.allowSceneActivation = false;
		if ((!Application.isEditor || ShowSplashInEditor_cvar) && !SkipSplash_cvar)
		{
			yield return ShowSplash();
		}
		_loadScene.allowSceneActivation = true;
		yield return _loadScene;
		Log.Info($"[Bootstrap] Loaded scene in:{_stopwatch.ElapsedMilliseconds}ms");
	}

	public static string GetVersionLoc()
	{
		string text = LocalizationManager.GetTranslation("GUI_GAME_VERSION_WIP");
		if (text.IsNullOrEmpty())
		{
			text = "Version: {0} <color=red>Work in progress</color>";
		}
		return text;
	}

	private IEnumerator ShowSplash()
	{
		if (DelaySplashscreen)
		{
			Log.Info($"[SplashScreen] time (pre-delay): {Time.time}");
			yield return null;
			yield return null;
			yield return null;
			Log.Info($"[SplashScreen] time (post-delay): {Time.time}");
		}
		Log.Info("[Bootstrap] Showing splash screen");
		SplashScreen.Begin();
		while (!SplashScreen.isFinished)
		{
			if (SkipSplash_cvar || Mouse.current.leftButton.wasPressedThisFrame)
			{
				SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
			}
			else
			{
				SplashScreen.Draw();
			}
			yield return null;
		}
		Log.Info("[Bootstrap] Finished showing splash screen.");
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.Repaint)
		{
			if (!_hasQGL)
			{
				QGL.Start();
				_hasQGL = true;
			}
			QGL.Begin();
			QGL.End();
		}
	}

	private void Update()
	{
		DebugMenu.UpdateQGL();
	}

	private void LateUpdate()
	{
		ScenarioScriptFrontend.UnityUpdate();
	}

	private void OnDestroy()
	{
		JobSystem.Clear();
		Log.Info("[Bootstrap] OnDestroy");
	}
}

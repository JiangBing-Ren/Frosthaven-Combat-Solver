using System;
using System.Collections.Generic;
using System.Threading;
using Libs.Threading;
using UnityEngine;
using UnityTools;

namespace Logging;

internal class LogHandler : MonoBehaviour
{
	private volatile int m_MainThreadFrames;

	private volatile float m_MainThreadTime;

	private volatile string m_ErrorLogged;

	private volatile string m_ErrorLoggedStack;

	private object m_LoggingLock = new object();

	private UnityLogFormatter m_UnityLogger;

	private readonly Dictionary<LogLevel, string> m_LogLevelFormat = new Dictionary<LogLevel, string>
	{
		{
			LogLevel.Trace,
			"[TRACE]"
		},
		{
			LogLevel.Info,
			"[INFO]"
		},
		{
			LogLevel.Warning,
			"[WARN]"
		},
		{
			LogLevel.Error,
			"[ERROR]"
		},
		{
			LogLevel.Exception,
			"[EXCEPTION]"
		}
	};

	private readonly Dictionary<LogCategory, string> m_LogCategoryFormat = new Dictionary<LogCategory, string>
	{
		{
			LogCategory.None,
			""
		},
		{
			LogCategory.Simple,
			"[Simple]"
		},
		{
			LogCategory.GUI,
			"[GUI]"
		},
		{
			LogCategory.Input,
			"[INPUT MANAGER]"
		},
		{
			LogCategory.Controller,
			"[Controller]"
		},
		{
			LogCategory.Navigation,
			"[Navigation]"
		},
		{
			LogCategory.Network,
			"[Network]"
		},
		{
			LogCategory.YML,
			"[YML]"
		},
		{
			LogCategory.Gameplay,
			"[Gameplay]"
		},
		{
			LogCategory.Animation,
			"[Animation]"
		}
	};

	public void Initialize()
	{
		m_UnityLogger = new UnityLogFormatter(Debug.unityLogger.logHandler, Thread.CurrentThread);
		Debug.unityLogger.logHandler = m_UnityLogger;
		Log.SharedLogEvent += SharedLogHandler;
		Log.Info("Shared log started!");
		UnitySystemConsoleRedirector.Redirect();
	}

	private void OnDestroy()
	{
		Log.SharedLogEvent -= SharedLogHandler;
		UnitySystemConsoleRedirector.ClearRedirect();
	}

	private void Update()
	{
		m_UnityLogger.LastFrameCount = (m_MainThreadFrames = Time.frameCount);
		m_UnityLogger.LastFrameTime = (m_MainThreadTime = Time.realtimeSinceStartup);
		ErrorMessage.__DisableDebugMessageBoxesHotkeyPressed = Input.GetKey(KeyCode.LeftControl);
		lock (m_LoggingLock)
		{
			if (m_ErrorLogged == null)
			{
				return;
			}
			if ((bool)SceneController.Instance && !SceneController.Instance.GlobalErrorMessage.ShowingMessage)
			{
				if (SceneController.Instance.InitialLoadingComplete)
				{
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00217", "GUI_ERROR_MAIN_MENU_BUTTON", m_ErrorLoggedStack, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, m_ErrorLogged, showErrorReportButton: true, trackDebug: true, allowMessageBox: false);
				}
				else
				{
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00217", "GUI_ERROR_EXIT_GAME_BUTTON", m_ErrorLoggedStack, Application.Quit, m_ErrorLogged, showErrorReportButton: true, trackDebug: true, allowMessageBox: false);
				}
			}
			m_ErrorLogged = null;
			m_ErrorLoggedStack = null;
		}
	}

	public void SharedLogHandler(Log.MessageData data)
	{
		m_LogCategoryFormat.TryGetValue(data.Category, out var value);
		string text = (string.IsNullOrEmpty(value) ? data.Message : (value + " " + data.Message));
		bool isMainThread = ThreadManagerComponent.Manager.IsMainThread;
		string text2 = (isMainThread ? Time.frameCount.ToString() : m_MainThreadFrames.ToString());
		string text3 = (isMainThread ? Time.realtimeSinceStartup.ToString("0.000") : m_MainThreadTime.ToString("0.000"));
		text = text2 + " (" + text3 + ")" + (isMainThread ? " " : "!") + " " + m_LogLevelFormat[data.Level] + " " + text;
		UnityEngine.Object obj = data.Context as UnityEngine.Object;
		if (data.Context != null && obj == null && !(data.Context is bool))
		{
			text += $"\nContext: {data.Context}";
		}
		text = text.Replace("{", "{{").Replace("}", "}}");
		switch (data.Level)
		{
		case LogLevel.Trace:
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_UnityLogger.BaseLogHandler.LogFormat(LogType.Log, obj, text);
			}
			break;
		case LogLevel.Info:
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_UnityLogger.BaseLogHandler.LogFormat(LogType.Log, obj, text);
			}
			break;
		case LogLevel.Warning:
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_UnityLogger.BaseLogHandler.LogFormat(LogType.Warning, obj, text);
			}
			break;
		case LogLevel.Error:
		{
			bool flag = false;
			if (data.Context is bool)
			{
				flag = (bool)data.Context;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_UnityLogger.BaseLogHandler.LogFormat(LogType.Error, obj, text);
			}
			if (!flag)
			{
				break;
			}
			FastReload.StopAutoUndoRedo();
			if (ErrorMessage.IgnoreAllErrors || !ErrorMessage.ShowDebugMessageBox(text))
			{
				break;
			}
			lock (m_LoggingLock)
			{
				if (m_ErrorLogged == null)
				{
					m_ErrorLogged = data.Message;
					m_ErrorLoggedStack = Environment.StackTrace;
				}
			}
			break;
		}
		case LogLevel.Exception:
		{
			Exception ex = data.Context as Exception;
			FastReload.StopAutoUndoRedo();
			if (!ErrorMessage.IgnoreAllErrors)
			{
				ErrorMessage.ShowDebugMessageBox(text, ex?.GetType()?.Name ?? "Exception");
			}
			m_UnityLogger.BaseLogHandler.LogException(ex, obj);
			if (!string.IsNullOrEmpty(data.Message))
			{
				m_UnityLogger.BaseLogHandler.LogFormat(LogType.Error, obj, text);
			}
			break;
		}
		}
		if (data.Category == LogCategory.Network && FFSNetwork.IsOnline)
		{
			switch (data.Level)
			{
			case LogLevel.Trace:
				new Color(45f / 255f, 192f / 255f, 1f);
				break;
			case LogLevel.Info:
				new Color(0.8823529f, 0.8823529f, 0.8823529f);
				break;
			case LogLevel.Warning:
				new Color(1f, 201f / 255f, 14f / 255f);
				break;
			case LogLevel.Error:
			case LogLevel.Exception:
				new Color(1f, 0.4f, 0f);
				break;
			default:
				new Color(0.8823529f, 0.8823529f, 0.8823529f);
				break;
			}
		}
	}
}

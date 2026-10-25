using System;
using System.IO;
using Libs.GameConsole;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SETUtil;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Tools;

[Preserve]
public static class ScriptModLoader
{
	[ConsoleCommand(Command = "ListAvailableScripts", Description = "List all available Scripts in dir StreamingAssets/Scripts.")]
	public static void ListAvailableScripts(IConsole console)
	{
		string subordinateRootFolderName = (Application.isEditor ? "Assets/StreamingAssets" : "StreamingAssets");
		string text = FileUtil.ParseToAbsolutePath("Scripts", subordinateRootFolderName);
		if (!Directory.Exists(text))
		{
			console.WriteLine("No Scripts found in " + text);
			return;
		}
		string[] files = Directory.GetFiles(text);
		foreach (string text2 in files)
		{
			if (!text2.EndsWith(".meta"))
			{
				console.WriteLine(text2);
			}
		}
	}

	[ConsoleCommand(Command = "Execute", Description = "Execute a line of C# code. If a file is provided instead it will attempt to run its contents. Show available scripts using 'ListAvailableScripts' command")]
	public static void CmdLoad(IConsole console, string codeOrFile)
	{
		string filePathFromInput = GetFilePathFromInput("Scripts/" + codeOrFile);
		string code = (string.IsNullOrEmpty(filePathFromInput) ? codeOrFile : File.ReadAllText(filePathFromInput));
		try
		{
			Log(CSharpScript.EvaluateAsync<string>(code, ScriptOptions.Default.WithReferences(typeof(GameObject).Assembly)).Result);
		}
		catch (Exception ex)
		{
			Error(ex.Message);
		}
		void Error(string str)
		{
			console.WriteLine("ERROR:" + str);
			Debug.LogError(str);
		}
		void Log(string str)
		{
			console.WriteLine(str);
			Debug.Log(str);
		}
	}

	private static string GetFilePathFromInput(string codeOrFile)
	{
		try
		{
			string subordinateRootFolderName = (Application.isEditor ? "Assets/StreamingAssets" : "StreamingAssets");
			string text = FileUtil.ParseToAbsolutePath(codeOrFile, subordinateRootFolderName);
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}
		catch
		{
			return null;
		}
	}
}

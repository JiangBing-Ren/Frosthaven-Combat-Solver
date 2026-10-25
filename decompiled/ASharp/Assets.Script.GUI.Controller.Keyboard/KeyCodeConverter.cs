using System.Collections.Generic;
using GLOOM;
using UnityEngine;

namespace Assets.Script.GUI.Controller.Keyboard;

public class KeyCodeConverter
{
	private static Dictionary<KeyCode, string> customNames = new Dictionary<KeyCode, string>
	{
		{
			KeyCode.Alpha0,
			"0"
		},
		{
			KeyCode.Alpha1,
			"1"
		},
		{
			KeyCode.Alpha2,
			"2"
		},
		{
			KeyCode.Alpha3,
			"3"
		},
		{
			KeyCode.Alpha4,
			"4"
		},
		{
			KeyCode.Alpha5,
			"5"
		},
		{
			KeyCode.Alpha6,
			"6"
		},
		{
			KeyCode.Alpha7,
			"7"
		},
		{
			KeyCode.Alpha8,
			"8"
		},
		{
			KeyCode.Alpha9,
			"9"
		},
		{
			KeyCode.Period,
			"."
		},
		{
			KeyCode.Exclaim,
			"!"
		},
		{
			KeyCode.DoubleQuote,
			"\""
		},
		{
			KeyCode.Hash,
			"#"
		},
		{
			KeyCode.Dollar,
			"$"
		},
		{
			KeyCode.Ampersand,
			"&"
		},
		{
			KeyCode.Quote,
			"'"
		},
		{
			KeyCode.LeftParen,
			"("
		},
		{
			KeyCode.RightParen,
			")"
		},
		{
			KeyCode.Plus,
			"+"
		},
		{
			KeyCode.Comma,
			","
		},
		{
			KeyCode.Minus,
			"-"
		},
		{
			KeyCode.Slash,
			"/"
		},
		{
			KeyCode.Colon,
			":"
		},
		{
			KeyCode.Semicolon,
			";"
		},
		{
			KeyCode.Less,
			"<"
		},
		{
			KeyCode.Equals,
			"="
		},
		{
			KeyCode.Greater,
			">"
		},
		{
			KeyCode.Question,
			"?"
		},
		{
			KeyCode.At,
			"@"
		},
		{
			KeyCode.LeftBracket,
			"["
		},
		{
			KeyCode.Backslash,
			"<"
		},
		{
			KeyCode.RightBracket,
			"]"
		},
		{
			KeyCode.Caret,
			"^"
		},
		{
			KeyCode.Underscore,
			"_"
		},
		{
			KeyCode.BackQuote,
			"`"
		},
		{
			KeyCode.Pipe,
			"|"
		},
		{
			KeyCode.Tilde,
			"~"
		},
		{
			KeyCode.LeftCurlyBracket,
			"{"
		},
		{
			KeyCode.RightCurlyBracket,
			"}"
		},
		{
			KeyCode.Percent,
			"%"
		}
	};

	public static string Convert(KeyCode keycode)
	{
		if (customNames.ContainsKey(keycode))
		{
			return customNames[keycode];
		}
		LocalizationManager.TryGetTranslation($"KEYCODE_{keycode}", out var translation);
		return translation ?? keycode.ToString().SplitCamelCase();
	}

	public static string ConvertToValue(KeyCode keycode)
	{
		if (keycode == KeyCode.Space)
		{
			return " ";
		}
		return Convert(keycode);
	}
}

using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class WatchesWindow : MonoBehaviour
{
	public class Watch
	{
		public object obj;

		public string expressionString;

		public CExpression expression;
	}

	public Color32 BackgroundColor = new Color32(70, 70, 70, byte.MaxValue);

	public int RowHeight = 20;

	public int Spacing = 10;

	public int ObjectWidth = 80;

	public int ExpressionWidth = 300;

	public int ValueWidth = 1000;

	public List<Watch> watches = new List<Watch>();

	public Rect windowRect = new Rect(10f, -1f, 0f, 0f);

	public string tooltip;

	public CVars vars = new CVars();

	public CVarsContext ctx;

	public static GUIStyle SolidStyle;

	public static WatchesWindow instance;

	public static void Show()
	{
		if (instance == null)
		{
			Create();
		}
		instance.enabled = true;
	}

	public static void Hide()
	{
		instance.enabled = false;
	}

	public static bool Add(object obj, string expressionString)
	{
		Show();
		CExpression cExpression = CExpression.Parse(expressionString);
		if (cExpression == null)
		{
			return false;
		}
		Watch item = new Watch
		{
			obj = obj,
			expressionString = expressionString,
			expression = cExpression
		};
		instance.watches.Add(item);
		return true;
	}

	private static void Create()
	{
		if (SolidStyle == null)
		{
			SolidStyle = new GUIStyle();
			SolidStyle.normal.background = Texture2D.whiteTexture;
			SolidStyle.normal.textColor = Color.white;
		}
		instance = new GameObject("WatchesWindow", typeof(WatchesWindow)).GetComponent<WatchesWindow>();
		instance.ctx = new CVarsContext(instance.vars);
		instance.watches.Add(new Watch());
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	private void OnGUI()
	{
		AdjustWindowRect();
		GUI.color = BackgroundColor;
		windowRect = GUI.Window(1, windowRect, WindowFunction, "", SolidStyle);
		ShowTooltip();
	}

	public static bool IsPointerOverGameObject()
	{
		Vector2 cursorPosition = InputFHManager.CursorPosition;
		cursorPosition.y = (float)Screen.height - cursorPosition.y;
		if (instance != null)
		{
			return instance.windowRect.Contains(cursorPosition);
		}
		return false;
	}

	private void AdjustWindowRect()
	{
		float num = windowRect.yMax;
		if (num < 0f)
		{
			num = Screen.height - 10;
		}
		else if (num > (float)Screen.height)
		{
			num = Screen.height;
		}
		int num2 = watches.Count * RowHeight;
		float num3 = num - (float)num2;
		if (num3 < 20f)
		{
			num3 = 20f;
		}
		int num4 = RowHeight + ObjectWidth + ExpressionWidth + ValueWidth + Spacing * 3;
		windowRect = new Rect(windowRect.xMin, num3, num4, num2);
	}

	private void WindowFunction(int windowID)
	{
		DebugMenu.ResolveContext(out var selectedActor, out var pickedActor, out var pickedTile);
		DebugMenu.SetupContextVars(vars, null, selectedActor, pickedActor, pickedTile);
		for (int i = 0; i < watches.Count; i++)
		{
			string text;
			string text2;
			string text3;
			string text4;
			string text5;
			string text6;
			if (i == 0)
			{
				text = "Context";
				text2 = null;
				text3 = "Expression";
				text4 = null;
				text5 = "Value";
				text6 = null;
			}
			else
			{
				Watch watch = watches[i];
				text = TValue.ToString(watch.obj);
				text2 = TValue.Dump(watch.obj);
				text3 = watch.expressionString;
				text4 = watch.expression.ToString();
				vars.Set("obj", new TValue(watch.obj));
				TValue tValue = watch.expression.Calc(ctx);
				text5 = tValue.ValueString();
				text6 = TValue.Dump(tValue);
			}
			float num = 0f;
			float y = i * RowHeight;
			if (GUI.Button(new Rect(num, y, RowHeight, RowHeight), "X"))
			{
				if (i == 0)
				{
					Hide();
					return;
				}
				watches.RemoveAt(i);
				i--;
			}
			else
			{
				num += (float)(RowHeight + Spacing);
				GUI.Label(new Rect(num, y, ObjectWidth, RowHeight), new GUIContent(text, text2));
				num += (float)(ObjectWidth + Spacing);
				GUI.Label(new Rect(num, y, ExpressionWidth, RowHeight), new GUIContent(text3, text4));
				num += (float)ExpressionWidth;
				GUI.Label(new Rect(num, y, ValueWidth, RowHeight), new GUIContent(text5, text6));
			}
		}
		if (Event.current.type == EventType.Repaint)
		{
			tooltip = GUI.tooltip;
		}
		GUI.DragWindow();
	}

	private void ShowTooltip()
	{
		if (!string.IsNullOrEmpty(tooltip))
		{
			GUI.color = Color.white;
			GUI.backgroundColor = BackgroundColor;
			GUI.skin.box.alignment = TextAnchor.MiddleLeft;
			Vector2 vector = GUI.skin.box.CalcSize(new GUIContent(tooltip));
			GUI.Box(new Rect(((float)Screen.width - vector.x) / 2f, windowRect.yMin - vector.y - 10f, vector.x, vector.y), tooltip, SolidStyle);
		}
	}
}

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class DebugMenuWBUI
{
	public static Sprite UISprite;

	public static Vector2 mousePosition;

	public static bool leftButtonDown;

	public static bool leftButtonUp;

	public static WrapBox wboxScreen;

	public static Action<string> Error = (string s) =>
	{
	};

	public static void Begin()
	{
		if (!QUI.canvas)
		{
			QUI.canvas = new GameObject("QUI Root").AddComponent<Canvas>();
			QUI.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			Canvas canvas = UIManager.Instance?.GetComponent<Canvas>();
			if ((bool)canvas)
			{
				QUI.canvas.transform.SetParent(canvas.transform.parent);
			}
			QUI.DrawLineRect = (float x, float y, float w, float h) =>
			{
				QGL.LateDrawLineRect(x, y, w, h, Color.magenta);
			};
		}
		mousePosition = Mouse.current.position.ReadValue();
		mousePosition.y = (float)Screen.height - mousePosition.y;
		leftButtonDown = Mouse.current.leftButton.wasPressedThisFrame;
		leftButtonUp = Mouse.current.leftButton.wasReleasedThisFrame;
		if (leftButtonDown)
		{
			QUI.OnMouseButton(down: true);
		}
		if (leftButtonUp)
		{
			QUI.OnMouseButton(down: false);
		}
		wboxScreen = new WrapBox
		{
			w = Screen.width,
			h = Screen.height
		};
		QUI.Begin(mousePosition.x, mousePosition.y);
	}

	public static void End()
	{
		QUI.End();
	}

	public static void Done()
	{
		Begin();
		End();
		if ((bool)QUI.canvas)
		{
			UnityEngine.Object.Destroy(QUI.canvas.gameObject);
		}
	}

	public static void TooltipTopRight(WrapBox wbox, string text)
	{
		int fontSize = 14;
		WrapBox wbox2 = wbox.Center(300f, 300f);
		int num = WBUI.MeasuredText(text, wbox2, out var measureW, out var measureH, null, fontSize, TextAnchor.UpperLeft, VerticalWrapMode.Overflow, null, 0, 73, "TooltipTopRight");
		wbox2 = wbox.TopLeft(measureW, measureH, wbox.W + 10f);
		WBUI.FillRect(wbox2.Center(wbox2.W + 10f, wbox2.H + 10f), Color.black, 0, 77, "TooltipTopRight");
		WrapBox wbox3 = wbox2;
		int handle = num;
		WBUI.Text_wg(text, wbox3, null, fontSize, TextAnchor.UpperLeft, VerticalWrapMode.Overflow, null, handle);
	}

	public static void Text(string text, WrapBox wbox, int fontSize = 0, TextAnchor align = TextAnchor.MiddleCenter, Color? color = null, int handle = 0, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
	{
		Color value = ((!color.HasValue) ? Color.black : color.Value);
		Color? color2 = value;
		WBUI.Text(text, wbox, null, fontSize, align, VerticalWrapMode.Overflow, color2, handle, lineNumber, caller);
	}

	public static int ButtTextSize(WrapBox wbox)
	{
		return (int)(wbox.H * 0.6f);
	}

	public static bool Button(WrapBox wbox, string text, Color? color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
	{
		return ButtonRes(wbox, text, color, lineNumber, caller) == QUI.WidgetResult.Pressed;
	}

	public static QUI.WidgetResult ButtonRes(WrapBox wbox, string text, Color? color = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
	{
		QUI.WidgetResult widgetResult = WBUI.ClickRect(wbox, 1, lineNumber, caller);
		Color color2 = ((!color.HasValue) ? Color.white : color.Value);
		WBUI.Sprite(color: (widgetResult == QUI.WidgetResult.Hot) ? new Color(color2.r * 0.8f, color2.g * 0.8f, color2.b * 0.8f) : color2, wbox: wbox, sprite: UISprite, scissor: false, ppuMultiplier: 1f, type: Image.Type.Sliced, handle: 2, lineNumber: lineNumber, caller: caller);
		Text(text, wbox, ButtTextSize(wbox), TextAnchor.MiddleCenter, null, 3, lineNumber, caller);
		return widgetResult;
	}

	public static WrapBox GetWrapBox(Component c, Canvas canvas)
	{
		RectTransform component = c.GetComponent<RectTransform>();
		if (!component)
		{
			Error($"No rect transform on the ability component {c}");
			return new WrapBox(0f, 0f, 100f, 100f, c.GetHashCode());
		}
		Vector3[] array = new Vector3[4];
		component.GetWorldCorners(array);
		float num = 999999f;
		float num2 = 0f;
		float num3 = 999999f;
		float num4 = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = canvas.worldCamera.WorldToScreenPoint(array[i]);
			num = Mathf.Min(vector.x, num);
			num3 = Mathf.Min((float)Screen.height - vector.y, num3);
			num2 = Mathf.Max(vector.x, num2);
			num4 = Mathf.Max((float)Screen.height - vector.y, num4);
		}
		float x = num;
		float y = num3;
		float w = num2 - num;
		float h = num4 - num3;
		return new WrapBox(x, y, w, h, c.GetHashCode());
	}
}

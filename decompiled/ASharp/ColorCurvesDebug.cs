using System.Collections.Generic;
using Libs.GameConsole;
using Libs.Utils;
using SETUtil;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class ColorCurvesDebug : MonoBehaviour
{
	public float DrawerLineThickness = 0.06f;

	public float DrawerScale = 3.6f;

	public float DrawerCurveMargins = 1.1f;

	public int DrawerCurveResolution = 200;

	private ShapeDrawer _drawer;

	private List<TextMeshPro> _subtitles = new List<TextMeshPro>();

	private void OnEnable()
	{
		transform.DestroyChildren(!Application.isPlaying);
		_drawer = null;
		_subtitles.Clear();
	}

	private void Update()
	{
		if (!_drawer)
		{
			GameObject gameObject = new GameObject("ShapeDrawer");
			gameObject.transform.parent = transform;
			_drawer = gameObject.AddComponent<ShapeDrawer>();
		}
		ColorCurves colorCurves = VolumeManager.instance?.stack?.GetComponent<ColorCurves>();
		if (!(colorCurves == null))
		{
			int shapeStartIndex = 0;
			shapeStartIndex = DrawCurve(_drawer, 0, colorCurves.master.value, shapeStartIndex, Color.white, "Master");
			shapeStartIndex = DrawCurve(_drawer, 1, colorCurves.red.value, ++shapeStartIndex, Color.red, "Red");
			shapeStartIndex = DrawCurve(_drawer, 2, colorCurves.green.value, ++shapeStartIndex, Color.green, "Green");
			shapeStartIndex = DrawCurve(_drawer, 3, colorCurves.blue.value, ++shapeStartIndex, Color.blue, "Blue");
			shapeStartIndex = DrawCurve(_drawer, 4, colorCurves.hueVsHue.value, ++shapeStartIndex, Color.white, "Hue VS Hue");
			shapeStartIndex = DrawCurve(_drawer, 5, colorCurves.hueVsSat.value, ++shapeStartIndex, Color.white, "Hue VS Sat");
			shapeStartIndex = DrawCurve(_drawer, 6, colorCurves.satVsSat.value, ++shapeStartIndex, Color.white, "Sat VS Sat");
			shapeStartIndex = DrawCurve(_drawer, 7, colorCurves.lumVsSat.value, ++shapeStartIndex, Color.white, "Lum VS Sat");
		}
	}

	private int DrawCurve(ShapeDrawer drawer, int curveIndex, TextureCurve curveParameter, int shapeStartIndex, Color color, string subtitleText)
	{
		if (curveParameter == null)
		{
			return shapeStartIndex;
		}
		Vector3 vector = Vector3.forward * DrawerScale;
		Vector3 vector2 = Vector3.right * DrawerScale;
		Vector3 vector3 = -vector * Mathf.FloorToInt((float)curveIndex / 4f) * (DrawerCurveMargins * 1.1f) + vector2 * (curveIndex % 4) * DrawerCurveMargins;
		Vector3 vector4 = transform.position + vector3;
		if (_subtitles.Count <= curveIndex)
		{
			_subtitles.Add(CreateSubtitle(curveIndex, vector4, vector, vector2, subtitleText));
		}
		else
		{
			UpdateSubtitle(_subtitles[curveIndex], vector4, vector, vector2, subtitleText);
		}
		drawer.DrawSegment(shapeStartIndex++, vector4, vector4 + vector, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(shapeStartIndex++, vector4 + vector2, vector4 + vector + vector2, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(shapeStartIndex++, vector4, vector4 + vector2, DrawerLineThickness, Color.gray);
		drawer.DrawSegment(shapeStartIndex++, vector4 + vector, vector4 + vector2 + vector, DrawerLineThickness, Color.gray);
		Vector3[] array = new Vector3[DrawerCurveResolution];
		float num = 1f / Mathf.Max(DrawerCurveResolution, 1f);
		for (int i = 0; i < DrawerCurveResolution; i++)
		{
			float num2 = (float)i * num;
			array[i] = vector4 + vector * curveParameter.Evaluate(num2) + vector2 * num2;
		}
		drawer.DrawPath(shapeStartIndex++, array, DrawerLineThickness, color);
		return shapeStartIndex;
	}

	private TextMeshPro CreateSubtitle(int index, Vector3 position, Vector3 up, Vector3 right, string subtitleText)
	{
		GameObject gameObject = new GameObject($"Subtitle_{index}");
		TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
		gameObject.transform.parent = transform;
		UpdateSubtitle(textMeshPro, position, up, right, subtitleText);
		textMeshPro.alignment = TextAlignmentOptions.Center;
		textMeshPro.overflowMode = TextOverflowModes.Overflow;
		textMeshPro.enableWordWrapping = false;
		return textMeshPro;
	}

	private void UpdateSubtitle(TextMeshPro subtitle, Vector3 position, Vector3 up, Vector3 right, string subtitleText)
	{
		subtitle.text = subtitleText;
		subtitle.transform.forward = Vector3.Cross(right, up);
		subtitle.transform.position = position + right * 0.5f + subtitle.transform.forward * 0.05f - up * 0.1f;
		subtitle.fontSize = 1f * DrawerScale;
	}

	[ConsoleCommand(Command = "debug_color_curves")]
	public static void Cmd_ColorCurvesDebug(IConsole console)
	{
		ColorCurvesDebug colorCurvesDebug = Object.FindObjectOfType<ColorCurvesDebug>();
		if (colorCurvesDebug != null)
		{
			SceneUtil.SmartDestroy(colorCurvesDebug.gameObject);
			return;
		}
		GameObject gameObject = new GameObject("ColorCurvesDebug");
		gameObject.transform.position = Vector3.zero;
		gameObject.AddComponent<ColorCurvesDebug>();
	}
}

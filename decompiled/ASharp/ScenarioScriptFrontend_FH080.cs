using UnityEngine;

internal class ScenarioScriptFrontend_FH080 : ScenarioScriptFrontend
{
	private void tileLabel(int x, int y, int height, string text, Color? color = null)
	{
		x = Mathf.Clamp(x, 0, ScenarioScriptFrontend._cta.GetLength(0) - 1);
		y = Mathf.Clamp(y, 0, ScenarioScriptFrontend._cta.GetLength(1) - 1);
		color = color ?? new Color?(Color.white);
		GameObject gameObject = ScenarioScriptFrontend._cta[x, y]?.m_GameObject;
		if (!gameObject)
		{
			_script.Trace($"Tile Label: Not a valid tile {x} {y}", null, 14);
			return;
		}
		Vector3 worldPos = gameObject.transform.position + new Vector3(0f, height, 0f);
		Vector2 vector = QGL.MeasureString(text);
		Color value = Color.white * 0.4f;
		value.a = 1f;
		QGL.LateBlitWorld(worldPos, vector.x + 10f, vector.y + 10f, value);
		QGL.LatePrintWorld(text, worldPos, color);
	}

	protected override void TickInternal()
	{
	}
}

using UnityEngine;

public class ToggleWallTransparencyGlobal : MonoBehaviour
{
	public bool WallTransparencyEnabled;

	private int m_CurrentValue;

	private void Update()
	{
		m_CurrentValue = Shader.GetGlobalInt("ToggleWallFade");
		if ((m_CurrentValue == 1 && !WallTransparencyEnabled) || (m_CurrentValue == 0 && WallTransparencyEnabled))
		{
			Shader.SetGlobalInt("ToggleWallFade", WallTransparencyEnabled ? 1 : 0);
		}
	}
}

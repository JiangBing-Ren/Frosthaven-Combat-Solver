using UnityEngine;

public class Main : MonoBehaviour
{
	public static bool s_Pause3DWorld;

	public static float s_NonPausedDeltaTime;

	public static float s_NonPausedTime;

	public static bool s_DevMode;

	public static bool s_InternalRelease;

	private static int s_NumberOfPausesRegistered;

	public const int c_AppID = 780290;

	private static Main s_This;

	private float m_RealTimeSinceStartup;

	private void Awake()
	{
		s_This = this;
		s_Pause3DWorld = false;
	}

	public static Main Get()
	{
		return s_This;
	}

	private void Start()
	{
		m_RealTimeSinceStartup = Time.realtimeSinceStartup;
		UnityGameEditorRuntime.Initialise();
		Shader.SetGlobalInt("ToggleWallFade", 1);
	}

	public static void Pause3DWorld()
	{
		s_Pause3DWorld = true;
		s_NumberOfPausesRegistered++;
		TimeManager.PauseTime();
		Log.InfoFormat("Pause3DWorld. Nº of pauses: {0}", s_NumberOfPausesRegistered);
	}

	private void Update()
	{
		s_NonPausedDeltaTime = Time.realtimeSinceStartup - m_RealTimeSinceStartup;
		s_NonPausedTime += s_NonPausedDeltaTime;
		m_RealTimeSinceStartup = Time.realtimeSinceStartup;
	}

	public static void Unpause3DWorld(bool forceUnpause = false)
	{
		if (forceUnpause)
		{
			s_NumberOfPausesRegistered = 0;
		}
		else if (s_NumberOfPausesRegistered > 0)
		{
			s_NumberOfPausesRegistered--;
		}
		if (s_NumberOfPausesRegistered == 0)
		{
			s_Pause3DWorld = false;
			TimeManager.UnpauseTime();
		}
		Log.InfoFormat("Unpause3DWorld. Is paused {0}. Nº of pauses: {1}", s_Pause3DWorld, s_NumberOfPausesRegistered);
	}
}

using UnityEngine;

[CreateAssetMenu(fileName = "GlobalSettingsDef", menuName = "Defs/GlobalSettingsDef")]
public class GlobalSettingsDef : ScriptableObject
{
	public int LoadingTipsCount;

	[AudioPlaylistName]
	public string m_MainMenuMusicPlaylist;

	[AudioPlaylistName]
	public string m_OutpostWinterMusicPlaylist;

	[AudioPlaylistName]
	public string m_OutpostSummerMusicPlaylist;

	[AudioPlaylistName]
	public string m_QuestBriefingMusicPlaylist;

	[AudioEventName]
	public string m_WinMusicItem;

	[AudioEventName]
	public string m_LoseMusicItem;

	public GlobalSettings.BossRoomDef[] BossRooms;

	public GlobalSettings.AttackModifierSettings m_AttackModifierSettings;

	private static GlobalSettingsDef s_instance;

	public static GlobalSettingsDef Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = Resources.Load("Settings/GlobalSettingsDef") as GlobalSettingsDef;
			}
			return s_instance;
		}
	}

	public void CopyFromGlobalSettings()
	{
	}
}

using UnityEngine;

public class OnClickSendScreenEvent : MonoBehaviour
{
	[SerializeField]
	private AWScreenName m_ScreenName;

	public EGameMode m_RequiredGameMode;

	public void OnClick()
	{
		bool flag = true;
		if (m_RequiredGameMode != EGameMode.None && SaveData.Instance.Global.GameMode != m_RequiredGameMode)
		{
			flag = false;
		}
		if (flag)
		{
			AnalyticsWrapper.LogScreenDisplay(m_ScreenName);
		}
	}
}

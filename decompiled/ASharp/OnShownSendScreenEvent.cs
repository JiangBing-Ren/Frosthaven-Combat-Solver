using UnityEngine;

public class OnShownSendScreenEvent : MonoBehaviour
{
	[SerializeField]
	private AWScreenName m_ScreenName;

	public void OnShown()
	{
		AnalyticsWrapper.LogScreenDisplay(m_ScreenName);
	}
}

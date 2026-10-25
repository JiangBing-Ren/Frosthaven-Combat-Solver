using UnityEngine;

public class OnEnableSendScreenEvent : MonoBehaviour
{
	[SerializeField]
	private AWScreenName m_ScreenName;

	private void OnEnable()
	{
		AnalyticsWrapper.LogScreenDisplay(m_ScreenName);
	}
}

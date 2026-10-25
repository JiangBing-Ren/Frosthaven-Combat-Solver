using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VisitWorkshopButton : MonoBehaviour
{
	[SerializeField]
	private Image m_Background;

	private Tween m_Anim;

	private void OnEnable()
	{
		m_Anim?.Kill();
		ExtendedButton component = GetComponent<ExtendedButton>();
		component.onMouseEnter.RemoveAllListeners();
		component.onMouseEnter.AddListener(StopBlinking);
		component.onMouseExit.RemoveAllListeners();
		component.onMouseExit.AddListener(StartBlinking);
		m_Anim = m_Background.DOFade(0.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
	}

	private void StopBlinking()
	{
		m_Anim?.Pause();
		m_Background.color = Color.white;
	}

	private void StartBlinking()
	{
		m_Anim?.Play();
	}

	public void OpenWorkshop()
	{
		Application.OpenURL("steam://url/SteamWorkshopPage/780290");
	}
}

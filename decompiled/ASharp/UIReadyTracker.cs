using UnityEngine;
using UnityEngine.UI;

public class UIReadyTracker : MonoBehaviour
{
	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float notReadyOppacity = 0.5f;

	[SerializeField]
	private GUIAnimator readyAnimation;

	public void Init(Sprite icon, bool isReady = false)
	{
		characterIcon.sprite = icon;
		ShowReady(isReady);
	}

	public void ShowReady(bool isReady)
	{
		canvasGroup.alpha = (isReady ? 1f : notReadyOppacity);
		characterIcon.material = (isReady ? null : UIInfoTools.Instance.greyedOutMaterial);
		if (isReady)
		{
			readyAnimation.Play(fromStart: true);
			return;
		}
		readyAnimation.Stop();
		readyAnimation.GoInitState();
	}

	private void OnDisable()
	{
		readyAnimation.Stop();
		readyAnimation.GoInitState();
	}
}

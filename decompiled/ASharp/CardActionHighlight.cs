using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardActionHighlight : MonoBehaviour
{
	[SerializeField]
	private Image imageHighlight;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[Header("Hover")]
	[SerializeField]
	private float fromAlfa = 1f;

	[SerializeField]
	private float toAlfa = 0.3f;

	[SerializeField]
	private float hoverDuration = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float hoverShineWidth;

	[Header("Selected")]
	[SerializeField]
	[Range(0f, 1f)]
	private float selectedShineWidth = 0.7f;

	[SerializeField]
	[Range(0f, 1f)]
	private float selectedAlfa = 1f;

	private Tween hoverAnim;

	private const string SHINE_PROPERTY_MATERIAL = "_AngularHighlightWidth";

	public void ShowHover()
	{
		CancelAnimation();
		gameObject.SetActive(value: true);
		imageHighlight.material.SetFloat("_AngularHighlightWidth", hoverShineWidth);
		LoopHighlight(fromAlfa, toAlfa);
	}

	private void LoopHighlight(float from, float to)
	{
		canvasGroup.alpha = from;
		hoverAnim = canvasGroup.DOFade(to, hoverDuration).SetLoops(1, LoopType.Yoyo);
	}

	public void ShowSelected()
	{
		CancelAnimation();
		imageHighlight.material.SetFloat("_AngularHighlightWidth", selectedShineWidth);
		canvasGroup.alpha = selectedAlfa;
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		CancelAnimation();
		gameObject.SetActive(value: false);
	}

	private void CancelAnimation()
	{
		if (hoverAnim != null)
		{
			hoverAnim.Kill();
			hoverAnim = null;
		}
	}

	private void OnDisable()
	{
		CancelAnimation();
	}
}

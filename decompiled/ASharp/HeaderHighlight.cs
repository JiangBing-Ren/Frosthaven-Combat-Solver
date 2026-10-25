using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderHighlight : MonoBehaviour
{
	[SerializeField]
	private List<TextMeshProUGUI> highlightTexts;

	[SerializeField]
	private List<Graphic> highlightImages;

	[SerializeField]
	private Material warningFontMaterial;

	[SerializeField]
	private Material highlightFontMaterial;

	[SerializeField]
	protected Color highlightColor;

	[SerializeField]
	protected GUIAnimator highlightAnimator;

	protected void PrepareWarningHighlight()
	{
		highlightAnimator.Stop(goToEnd: true);
		for (int i = 0; i < highlightImages.Count; i++)
		{
			highlightImages[i].color = UIInfoTools.Instance.warningColor;
		}
		for (int j = 0; j < highlightTexts.Count; j++)
		{
			highlightTexts[j].color = UIInfoTools.Instance.warningColor;
			highlightTexts[j].fontMaterial = warningFontMaterial;
		}
	}

	public void ShowWarning()
	{
		PrepareWarningHighlight();
		highlightAnimator.Play();
	}

	public void ShowHighlight()
	{
		PrepareHighlight();
		highlightAnimator.Play();
	}

	protected void PrepareHighlight()
	{
		highlightAnimator.Stop(goToEnd: true);
		for (int i = 0; i < highlightImages.Count; i++)
		{
			highlightImages[i].color = highlightColor;
		}
		for (int j = 0; j < highlightTexts.Count; j++)
		{
			highlightTexts[j].color = highlightColor;
			highlightTexts[j].fontMaterial = highlightFontMaterial;
		}
	}

	public void Hide()
	{
		highlightAnimator.Stop(goToEnd: true);
	}
}

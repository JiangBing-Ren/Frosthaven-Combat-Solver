using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsSlider : UIDistributePointsCounter
{
	[SerializeField]
	private TextMeshProUGUI currentPointsText;

	[SerializeField]
	private Slider currentPointsSlider;

	[SerializeField]
	private Slider extendedPointsSlider;

	[SerializeField]
	private Image extendedPointsFillImage;

	[SerializeField]
	private GUIAnimator extendedPointsHighlightAnimation;

	[SerializeField]
	private Color decreasedPointsColor;

	[SerializeField]
	private Color increasedPointsColor;

	[Header("Divisions")]
	[SerializeField]
	private Vector2 scale1Division = new Vector2(1f, 0.5f);

	[SerializeField]
	private Vector2 scale5Division = new Vector2(2f, 1f);

	[SerializeField]
	private List<RectTransform> divisions;

	private int currentPoints;

	private int extendedPoints;

	public override void Setup(int maxPoints, int currentPoints)
	{
		HelperTools.NormalizePool(ref divisions, divisions[0].gameObject, divisions[0].parent, maxPoints - 1);
		Vector2 one = Vector2.one;
		for (int i = 0; i < maxPoints - 1; i++)
		{
			divisions[i].sizeDelta = one;
			divisions[i].localScale = (((i + 1) % 5 == 0) ? scale5Division : scale1Division);
		}
		extendedPointsSlider.maxValue = maxPoints;
		currentPointsSlider.maxValue = maxPoints;
		base.Setup(maxPoints, currentPoints);
	}

	public override void SetCurrentPoints(int currentPoints)
	{
		this.currentPoints = currentPoints;
		Refresh();
	}

	public override void SetExtendedPoints(int extendedPoints)
	{
		this.extendedPoints = extendedPoints;
		extendedPointsFillImage.color = ((extendedPoints < 0) ? decreasedPointsColor : increasedPointsColor);
		extendedPointsSlider.value = extendedPoints;
		Refresh();
	}

	private void Refresh()
	{
		currentPointsText.text = (currentPoints + extendedPoints).ToString();
		extendedPointsSlider.value = ((extendedPoints > 0) ? (extendedPoints + currentPoints) : currentPoints);
		currentPointsSlider.value = ((extendedPoints < 0) ? (extendedPoints + currentPoints) : currentPoints);
		if (extendedPoints != 0)
		{
			extendedPointsHighlightAnimation.Play();
		}
		else
		{
			extendedPointsHighlightAnimation.Stop();
		}
	}

	private void OnDisable()
	{
		extendedPointsHighlightAnimation.Stop();
	}
}

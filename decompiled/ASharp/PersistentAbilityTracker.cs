using System.Collections.Generic;
using GLOOM;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentAbilityTracker : MonoBehaviour
{
	[SerializeField]
	private Transform stepsHolder;

	[SerializeField]
	private GameObject elementPrefab;

	[SerializeField]
	private Image startImage;

	[SerializeField]
	public Image endingImage;

	[SerializeField]
	private Sprite[] endSprites;

	[SerializeField]
	private TextMeshProUGUI title;

	private List<PersistentAbilityTrackerElement> elements = new List<PersistentAbilityTrackerElement>();

	private CActiveBonus m_ActiveBonus;

	private int startAtTrackerCount;

	private int maxTrackerSize;

	public void Init([CanBeNull] CActiveBonus activeBonus)
	{
		if (activeBonus == null)
		{
			startAtTrackerCount = 0;
			maxTrackerSize = 0;
			gameObject.SetActive(value: false);
			return;
		}
		startAtTrackerCount = (activeBonus.Ability.ActiveBonusData.Requirements?.ActiveBonusGreaterOrEqualToTrackerIndex).GetValueOrDefault();
		int num = activeBonus.Layout?.TrackerPattern.Count ?? 0;
		ActiveBonusLayout layout = activeBonus.Layout;
		int num2;
		if (layout != null && layout.TrackerPattern.Count == 1)
		{
			ActiveBonusLayout layout2 = activeBonus.Layout;
			num2 = ((layout2 != null && layout2.TrackerPattern[0] == 0) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag = (byte)num2 != 0;
		maxTrackerSize = num - startAtTrackerCount;
		HelperTools.NormalizePool(ref elements, elementPrefab, stepsHolder, maxTrackerSize);
		m_ActiveBonus = activeBonus;
		title.text = LocalizationManager.GetTranslation("XP") + ":";
		if (activeBonus.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			startImage.gameObject.SetActive(value: false);
		}
		else
		{
			startImage.gameObject.SetActive(value: true);
			startImage.sprite = UIInfoTools.Instance.GetActiveAbilityDurationIcon(activeBonus.Duration);
		}
		if (activeBonus.Layout != null && maxTrackerSize > 0 && !flag)
		{
			UpdateElements(activeBonus.TrackerIndex, activeBonus.Layout.TrackerPattern);
			gameObject.SetActive(value: true);
		}
		else
		{
			gameObject.SetActive(value: false);
		}
		if (activeBonus.Layout != null && activeBonus.Layout.Discard != DiscardType.None)
		{
			Sprite sprite = endSprites[(int)(activeBonus.Layout.Discard - 1)];
			endingImage.sprite = sprite;
			endingImage.gameObject.SetActive(sprite != null);
		}
		else
		{
			endingImage.gameObject.SetActive(value: false);
		}
	}

	public void OnActiveBonusTriggered()
	{
		if (maxTrackerSize > 0)
		{
			UpdateElements(m_ActiveBonus.TrackerIndex, m_ActiveBonus.Layout.TrackerPattern);
		}
	}

	private void UpdateElements(int selectedElement, List<int> pattern)
	{
		int num = 0;
		foreach (int item in pattern)
		{
			if (num >= startAtTrackerCount)
			{
				int index = num - startAtTrackerCount;
				elements[index].SetElementXP(item, selectedElement == num, num < selectedElement);
			}
			num++;
		}
	}
}

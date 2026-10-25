using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDLCSelector : MonoBehaviour
{
	[Serializable]
	private class DLCStateText
	{
		[SerializeField]
		private TextMeshProUGUI text;

		[SerializeField]
		private Color availableColor;

		[SerializeField]
		private Color unavailableColor;

		public void ShowAvailable()
		{
			text.color = availableColor;
		}

		public void ShowUnavailable()
		{
			text.color = unavailableColor;
		}

		public void SetDefaultColor()
		{
			availableColor = text.color;
		}
	}

	[SerializeField]
	private ScrollRect container;

	[SerializeField]
	private List<UIDLCSelectorOption> optionsPool;

	[SerializeField]
	private DLCStateText[] texts;

	private DLCRegistry.EDLCKey selectedDLC;

	private void Awake()
	{
		for (int i = 0; i < optionsPool.Count; i++)
		{
			optionsPool[i].OnToggledDLC.AddListener(OnToggledDLC);
		}
		CreateOptions();
	}

	private void CreateOptions()
	{
		HelperTools.NormalizePool(ref optionsPool, optionsPool[0].gameObject, container.content, 0, (UIDLCSelectorOption opt) =>
		{
			opt.OnToggledDLC.AddListener(OnToggledDLC);
		});
		for (int num = 0; num < texts.Length; num++)
		{
			texts[num].ShowUnavailable();
		}
	}

	private void OnToggledDLC(DLCRegistry.EDLCKey dlc, bool toggled)
	{
		if (toggled)
		{
			selectedDLC |= dlc;
		}
		else
		{
			selectedDLC &= ~dlc;
		}
	}

	public void SetValue(DLCRegistry.EDLCKey selected)
	{
		selectedDLC = selected;
		for (int i = 0; i < optionsPool.Count && optionsPool[i].gameObject.activeSelf; i++)
		{
			optionsPool[i].SetSelected(selected.HasFlag(optionsPool[i].DLC));
		}
	}

	private void OnEnable()
	{
		container.ScrollToTop();
	}

	public DLCRegistry.EDLCKey GetValue()
	{
		return selectedDLC;
	}
}

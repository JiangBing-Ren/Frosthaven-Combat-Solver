using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPromotionDLCSlot : MonoBehaviour
{
	[Serializable]
	public class DLCEvent : UnityEvent<DLCRegistry.EDLCKey>
	{
	}

	[SerializeField]
	private Image promotionImage;

	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private Button button;

	private DLCRegistry.EDLCKey dlc;

	public DLCEvent OnClick;

	private void Awake()
	{
		button.onClick.AddListener(() =>
		{
			OnClick.Invoke(dlc);
		});
	}

	[Obsolete]
	public virtual void SetDLC(DLCRegistry.EDLCKey dlc, Sprite image)
	{
		this.dlc = dlc;
		if (title != null)
		{
			title.SetTextKey("GUI_SPACE");
		}
		promotionImage.sprite = image;
	}
}

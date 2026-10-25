using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsToggle : UIDistributePointsCounter
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private GameObject activeMask;

	[SerializeField]
	private GameObject disabledMask;

	public bool Interactable
	{
		get
		{
			return toggle.interactable;
		}
		set
		{
			toggle.interactable = value;
		}
	}

	private void Awake()
	{
		toggle.onValueChanged.AddListener((bool isOn) =>
		{
			RefreshMasks();
		});
	}

	public override void SetCurrentPoints(int currentPoints)
	{
	}

	public override void SetExtendedPoints(int extendedPoints)
	{
		toggle.isOn = extendedPoints > 0;
		RefreshMasks();
	}

	private void RefreshMasks()
	{
		activeMask.SetActive(toggle.isOn);
		disabledMask.SetActive(!toggle.isOn);
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDonationRow : MonoBehaviour
{
	public struct ResourceDonationData
	{
		public bool IsPartyLeader;

		public uint InspirationQuantity;

		public int ResourceQuantity;

		public int CurrentDonated;

		public int DonatedByPlayer;

		public int TotalNeeded;
	}

	[SerializeField]
	private FHButton _resourceButton;

	[SerializeField]
	private FHButton _inspirationButton;

	[SerializeField]
	private TMP_Text _resourceQuantity;

	[SerializeField]
	private TMP_Text _inspirationQuantity;

	[SerializeField]
	private Image _sliderFill;

	[SerializeField]
	private TMP_Text _currentQuantity;

	[SerializeField]
	private FHButton _resetButton;

	public Action<bool> OnResourceClick;

	public Action OnResetClicked;

	private void Awake()
	{
		_resourceButton.onClick.AddListener(() =>
		{
			OnResourceClick?.Invoke(obj: false);
		});
		_inspirationButton.onClick.AddListener(() =>
		{
			OnResourceClick?.Invoke(obj: true);
		});
		_resetButton.onClick.AddListener(() =>
		{
			OnResetClicked?.Invoke();
		});
	}

	public void Init(ResourceDonationData data)
	{
		_inspirationButton.gameObject.SetActive(value: false);
		_resourceQuantity.text = data.ResourceQuantity.ToString();
		_inspirationQuantity.text = data.InspirationQuantity.ToString();
		_sliderFill.fillAmount = ((data.TotalNeeded == 0) ? 0f : ((float)data.CurrentDonated / (float)data.TotalNeeded));
		_currentQuantity.text = $"{data.CurrentDonated}/{data.TotalNeeded}({data.DonatedByPlayer})";
		_resourceButton.interactable = data.TotalNeeded > 0 && data.ResourceQuantity > 0 && data.CurrentDonated != data.TotalNeeded;
		_inspirationButton.interactable = data.IsPartyLeader && data.TotalNeeded > 0 && data.InspirationQuantity != 0;
		_resetButton.interactable = data.CurrentDonated > 0;
	}
}

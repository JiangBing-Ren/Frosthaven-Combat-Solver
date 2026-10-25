using FHRL;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPetSlot : MonoBehaviour
{
	public enum SlotState
	{
		None,
		Occupied,
		Available,
		Unavailable
	}

	public UITextTooltipTarget tooltip;

	public Image PetIcon;

	public Image FrameIcon;

	public TextMeshProUGUI PetName;

	private bool _invalidate;

	public SlotState slotState { get; private set; }

	public PetDef petData { get; private set; }

	private void Invalidate()
	{
		_invalidate = true;
	}

	public void SetState(SlotState state)
	{
		slotState = state;
		Invalidate();
	}

	public void SetPetData(PetDef def)
	{
		petData = def;
		if (def != null)
		{
			SetState(SlotState.Occupied);
		}
		Invalidate();
	}

	private void Update()
	{
		if (_invalidate)
		{
			_invalidate = false;
			Refresh();
		}
	}

	private void Refresh()
	{
		switch (slotState)
		{
		case SlotState.Occupied:
			tooltip.tooltipText = petData.Description;
			PetName.gameObject.SetActive(value: true);
			PetName.text = LocalizationManager.GetTranslation(petData.Name);
			PetIcon.gameObject.SetActive(value: true);
			PetIcon.sprite = UIInfoTools.Instance.GetItemMiniSprite(petData.ArtKey);
			FrameIcon.gameObject.SetActive(value: true);
			break;
		case SlotState.Available:
			tooltip.tooltipText = null;
			PetName.gameObject.SetActive(value: false);
			PetIcon.gameObject.SetActive(value: false);
			FrameIcon.gameObject.SetActive(value: true);
			break;
		case SlotState.Unavailable:
			tooltip.tooltipText = null;
			PetName.gameObject.SetActive(value: false);
			PetIcon.gameObject.SetActive(value: false);
			FrameIcon.gameObject.SetActive(value: false);
			break;
		}
	}
}

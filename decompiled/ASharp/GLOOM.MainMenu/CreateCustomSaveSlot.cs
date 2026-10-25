using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

internal class CreateCustomSaveSlot : MonoBehaviour
{
	public TMP_InputField txtStartingCharacters;

	public TMP_InputField txtStartingGold;

	public Toggle tglUnlockAllCharacters;

	public Toggle tglUnlockAllItems;

	public Toggle tglMaximizeCharacters;

	public Button btnCreateSave;

	public Action<CustomPartySetup.Configuration> OnCreateSeve;

	public void Start()
	{
		gameObject.SetActive(value: false);
	}

	private void OnCreateSaveClicked()
	{
		CustomPartySetup.Configuration configuration = new CustomPartySetup.Configuration();
		configuration.UnlockAllCharacters = tglUnlockAllCharacters.isOn;
		configuration.UnlockAllItems = tglUnlockAllItems.isOn;
		configuration.MaximizeCharacters = tglUnlockAllCharacters.isOn;
		int.TryParse(txtStartingCharacters.text.Trim(), out configuration.StartingCharactersCount);
		int.TryParse(txtStartingGold.text.Trim(), out configuration.StartingGold);
		OnCreateSeve?.Invoke(configuration);
	}
}

using System.Linq;
using System.Threading.Tasks;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSaveHotkeysController : MonoBehaviour, IScopeElement
{
	private void OnEnable()
	{
		InputFHManager.Controls.Shared.QuickSave.performed += OnQuickSave;
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.Shared.QuickSave);
		InputFHManager.Controls.Shared.QuickLoad.performed += OnQuickLoad;
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.Shared.QuickLoad);
	}

	private void OnDisable()
	{
		if (InputFHManager.Controls != null)
		{
			InputFHManager.Controls.DisableAll(this);
			InputFHManager.Controls.Shared.QuickSave.performed -= OnQuickSave;
			InputFHManager.Controls.Shared.QuickLoad.performed -= OnQuickLoad;
		}
	}

	private void OnQuickSave(InputAction.CallbackContext obj)
	{
		if (SaveData.Instance.CanSave())
		{
			SaveData.Instance.QuickSaveCurrentAdventureData();
		}
	}

	private void OnQuickLoad(InputAction.CallbackContext obj)
	{
		if (SaveData.Instance.CanLoad() && SaveData.Instance.Global.CurrentAdventureData != null)
		{
			QuickLoadLastSave();
		}
	}

	private async Task QuickLoadLastSave()
	{
		string partyName = SaveData.Instance.Global.CurrentAdventureData.PartyName;
		PartyAdventureData partyAdventureData = (from t in await SaveData.Instance.Global.LoadSavesFromDisk()
			where t.SaveFileType == PartyAdventureData.SaveType.QuickSave && t.PartyName == partyName
			orderby t.LastSavedTimeStamp descending
			select t).FirstOrDefault();
		if (partyAdventureData != null)
		{
			SaveData.Instance.LoadCampaignMode(partyAdventureData, isJoiningMPClient: false);
		}
	}
}

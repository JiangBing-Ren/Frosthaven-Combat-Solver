using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIContinueGamePreview : MonoBehaviour
{
	[SerializeField]
	private UIWindow _window;

	[SerializeField]
	private UILoadGameSlot _slot;

	public async void Show(IGameModeService service)
	{
		if (await service.CanResume())
		{
			PartyAdventureData resume = await SaveData.Instance.Global.GetResumeCampaign();
			IGameSaveData data = (await service.GetSaves()).First((IGameSaveData s) => s.PartyAdventureData == resume);
			_slot.SetData(data);
			_window.Show();
		}
	}

	public void Hide()
	{
		_window.Hide();
	}
}

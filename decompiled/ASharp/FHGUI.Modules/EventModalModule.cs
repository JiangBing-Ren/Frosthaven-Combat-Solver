using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FHGUI.States;
using FHRL;
using I2.Loc;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class EventModalModule : FHMapViewModule
{
	public Button ButtonPrefab;

	public Transform ButtonParent;

	public TMP_Text Text;

	public Action<int> OnOptionSelected;

	public Action OnDone;

	private List<Button> Buttons = new List<Button>();

	private void Awake()
	{
		ButtonParent?.DestroyChildren();
	}

	public void Init(OutpostEventState ev, CampaignController controller, bool isPartyLeader)
	{
		string validTermName = I2Utils.GetValidTermName(NarrationSetting.FromOutpostEventFront(ev).Text);
		Text.text = LocalizationManager.GetTermTranslation("FH/" + validTermName);
		Text.color = Color.white;
		if (Text.text == null)
		{
			Text.text = validTermName;
			Text.color = Color.red;
		}
		List<OutpostEventDef.Option> list = ev.Def.GetOptions(controller.State, ev).ToList();
		if (ev.OutcomeIteration == 0)
		{
			HelperTools.NormalizePool(ref Buttons, ButtonPrefab.gameObject, ButtonParent, list.Count);
			for (int i = 0; i < list.Count(); i++)
			{
				int id = (int)list[i].ID;
				Buttons[i].gameObject.SetActive(value: true);
				Buttons[i].interactable = controller.OutpostEventController.IsValidOption(list[i]) & isPartyLeader;
				Buttons[i].onClick.RemoveAllListeners();
				Buttons[i].onClick.AddListener(() =>
				{
					OnOptionSelected?.Invoke(id);
				});
				NarrationSetting.Setting setting = NarrationSetting.FromOutpostEventOption(ev, list[i]);
				TMP_Text componentInChildren = Buttons[i].GetComponentInChildren<TMP_Text>();
				string validTermName2 = I2Utils.GetValidTermName(setting.Text);
				if (!string.IsNullOrEmpty(setting.TextLocalized))
				{
					componentInChildren.text = setting.TextLocalized;
				}
				else
				{
					componentInChildren.text = LocalizationManager.GetTermTranslation("FH/" + validTermName2);
				}
				componentInChildren.color = Color.black;
				if (componentInChildren.text == null)
				{
					componentInChildren.text = validTermName2;
					componentInChildren.color = Color.red;
				}
			}
		}
		else
		{
			InitOkButton();
		}
	}

	public void Init(EventViewState.Data ev, CampaignController controller)
	{
		Text.text = ev.Text;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		FHMapView.FormatEffects(ev.Effects, stringBuilder);
		Text.text += stringBuilder.ToString();
		InitOkButton();
	}

	private void InitOkButton()
	{
		HelperTools.NormalizePool(ref Buttons, ButtonPrefab.gameObject, ButtonParent, 1);
		foreach (Button button in Buttons)
		{
			button.gameObject.SetActive(value: false);
		}
		Buttons[0].gameObject.SetActive(value: true);
		Buttons[0].interactable = true;
		Buttons[0].onClick.RemoveAllListeners();
		Buttons[0].onClick.AddListener(() =>
		{
			OnDone?.Invoke();
		});
		Buttons[0].GetComponentInChildren<TMP_Text>().text = "Ok";
	}
}

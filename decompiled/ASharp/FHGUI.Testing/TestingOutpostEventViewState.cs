using System.Collections.Generic;
using System.Linq;
using FH.YML;
using FHGUI.DTO;
using FHGUI.Modules;
using FHRL;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace FHGUI.Testing;

public class TestingOutpostEventViewState : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown m_EventList;

	[SerializeField]
	private FHButton m_Prev;

	[SerializeField]
	private FHButton m_Next;

	[Space]
	[SerializeField]
	private OutpostEventModule OutpostEventModule;

	private List<OutpostEventDTO> m_EventDTOs = new List<OutpostEventDTO>();

	private OutpostEventDTO m_SelectedEvent;

	private void Awake()
	{
		m_Prev.onClick.AddListener(OnPrevEvent);
		m_Next.onClick.AddListener(OnNextEvent);
	}

	private void Start()
	{
		BuildDTOs();
		PopulateEventOptions();
		OnEventSelected(0);
		OnShow();
	}

	private void OnShow()
	{
		OutpostEventModule.Init(m_SelectedEvent);
	}

	private void BuildDTOs()
	{
		m_EventDTOs.Clear();
		DefRepository defRepository = YMLDefRepository.Repo.DefRepository;
		foreach (KeyValuePair<OutpostEventID, OutpostEventDef> outpostEvent in defRepository.OutpostEvents)
		{
			m_EventDTOs.Add(CreateDTO(outpostEvent.Value, defRepository));
		}
	}

	public OutpostEventDTO CreateDTO(OutpostEventDef def, DefRepository repo)
	{
		OutpostEventDTO outpostEventDTO = new OutpostEventDTO();
		outpostEventDTO.ID = def.ID.ID;
		outpostEventDTO.EventType = def.Type.ToString();
		string validTermName = I2Utils.GetValidTermName(NarrationSetting.FromOutpostEventDef(def).Text);
		outpostEventDTO.Text = LocalizationManager.GetTermTranslation("FH/" + validTermName);
		OutpostEventState outpostEventState = new OutpostEventState(def.ID, repo);
		List<OutpostEventDef.Option> list = def.GetOptions(null, outpostEventState).ToList();
		outpostEventDTO.Options.Clear();
		for (int i = 0; i < list.Count(); i++)
		{
			OutpostEventDTO.Option option = new OutpostEventDTO.Option();
			option.Interactable = true;
			string validTermName2 = I2Utils.GetValidTermName(NarrationSetting.FromOutpostEventOption(def, (int)list[i].ID).Text);
			option.Text = LocalizationManager.GetTermTranslation("FH/" + validTermName2);
			outpostEventDTO.Options.Add(option);
		}
		return outpostEventDTO;
	}

	private void PopulateEventOptions()
	{
		if ((object)m_EventList == null)
		{
			return;
		}
		m_EventList.ClearOptions();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>(m_EventDTOs.Count);
		foreach (OutpostEventDTO eventDTO in m_EventDTOs)
		{
			list.Add(new TMP_Dropdown.OptionData(eventDTO.ID));
		}
		m_EventList.AddOptions(list);
		m_EventList.onValueChanged.AddListener((int index) =>
		{
			OnEventSelected(index);
		});
	}

	private void UpdateControls()
	{
		int num = m_EventDTOs.FindIndex((OutpostEventDTO dto) => dto == m_SelectedEvent);
		m_Prev.interactable = num > 0;
		m_Next.interactable = num < m_EventDTOs.Count;
	}

	private void OnEventSelected(int index)
	{
		m_SelectedEvent = m_EventDTOs[index];
		OnShow();
		UpdateControls();
	}

	private void OnNextEvent()
	{
		int num = m_EventDTOs.FindIndex((OutpostEventDTO dto) => dto == m_SelectedEvent);
		num++;
		if (num < m_EventDTOs.Count)
		{
			m_EventList.value = num;
		}
	}

	private void OnPrevEvent()
	{
		int num = m_EventDTOs.FindIndex((OutpostEventDTO dto) => dto == m_SelectedEvent);
		num--;
		if (num >= 0)
		{
			m_EventList.value = num;
		}
	}
}

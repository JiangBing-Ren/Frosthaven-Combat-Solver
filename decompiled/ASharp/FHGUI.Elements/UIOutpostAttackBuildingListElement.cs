using System;
using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIOutpostAttackBuildingListElement : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private TextLocalizedListener m_BuildingName;

	[SerializeField]
	private Image m_Icon;

	[Header("States")]
	[SerializeField]
	private GameObject m_Defended;

	[SerializeField]
	private GameObject m_Repaired;

	[SerializeField]
	private GameObject m_Wreck;

	[SerializeField]
	private GameObject m_Empty;

	[SerializeField]
	private GameObject m_Selected;

	[Header("Settings")]
	[SerializeField]
	private Sprite m_EmptyIcon;

	private Action<UIOutpostAttackBuildingListElement> _onSelect;

	public OutpostAttackEventSummeryEntryDTO Data { get; private set; }

	public bool Selected { get; private set; }

	public FHButton Button { get; private set; }

	private void Awake()
	{
		Button = GetComponent<FHButton>();
		Button.onClick.AddListener(HandleOnClick);
	}

	public void Init(OutpostAttackEventSummeryEntryDTO summeryEntryDto, Action<UIOutpostAttackBuildingListElement> onSelect = null)
	{
		_onSelect = onSelect;
		Data = summeryEntryDto;
		bool flag = Data.IsEmpty();
		bool flag2 = !flag && Data.InProgress;
		if (m_BuildingName != null)
		{
			m_BuildingName.Text.enabled = !flag;
			if (!flag)
			{
				m_BuildingName.SetTextKey(Data.Building.NameLocKey);
			}
		}
		if (m_Icon != null)
		{
			m_Icon.sprite = (flag ? m_EmptyIcon : Data.Building.Icon);
		}
		if ((bool)m_Defended)
		{
			m_Defended.SetActive(!flag && !flag2 && Data.Defended);
		}
		if ((bool)m_Repaired)
		{
			m_Repaired.SetActive(!flag && !flag2 && Data.IsRepaired);
		}
		if ((bool)m_Wreck)
		{
			m_Wreck.SetActive(!flag && !flag2 && Data.Wrecked);
		}
		if ((bool)m_Empty)
		{
			m_Empty.SetActive(flag);
		}
		Button.interactable = !Data.IsEmpty();
	}

	public void HandleOnClick()
	{
		_onSelect?.Invoke(this);
	}

	public void SetSelectedState(bool selected)
	{
		Selected = selected;
		if ((bool)m_Selected)
		{
			m_Selected.gameObject.SetActive(Selected);
		}
		if (selected)
		{
			Button.Select();
		}
	}

	private void OnDisable()
	{
		_onSelect = null;
		Selected = false;
		SetSelectedState(selected: false);
	}
}

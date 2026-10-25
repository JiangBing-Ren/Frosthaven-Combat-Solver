using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class CharacterRetireModule : FHMapViewModule
{
	public class Params
	{
		public PartySlotDTO CharacterData;

		public List<CharacterPersonalQuestDTO> QuestOptions;

		public Action CloseCallback;

		public Action NextStepCallback;

		public Action<string> ChooseQuestCallback;

		public string SelectedAdditionalQuestID;

		public bool HasInspiration;
	}

	[Header("Components")]
	[SerializeField]
	private UIPartySlot m_CharacterSlot;

	[SerializeField]
	private TMP_Text m_CharacterName;

	[SerializeField]
	private Image m_ClassColor;

	[SerializeField]
	private Image m_ClassIcon;

	[SerializeField]
	private Image m_HeaderColor;

	[SerializeField]
	private Image m_HeroIllustration;

	[SerializeField]
	private TMP_Text m_InsufficientInspiration;

	[SerializeField]
	private FHButton m_Close;

	[SerializeField]
	private FHButton m_Next;

	[SerializeField]
	private RectTransform m_GroupQuests;

	[SerializeField]
	private RectTransform m_QuestsContainer;

	[SerializeField]
	private RectTransform m_InfoPanelContainer;

	[Header("Tooltips")]
	public GameObject TooltipPrototype;

	private List<UIPersonalQuestCard> m_QuestCards = new List<UIPersonalQuestCard>();

	private bool m_Initialized;

	private bool m_InvalidateLayout;

	public Params Data { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_QuestCards = m_QuestsContainer.GetComponentsInChildren<UIPersonalQuestCard>(includeInactive: true).ToList();
		}
	}

	public void Init(Params retirementParams)
	{
		InitCache();
		Data = retirementParams;
		Color characterColor = UIInfoTools.Instance.GetCharacterColor(Data.CharacterData.Character);
		Sprite iconClassGold = UIInfoTools.Instance.GetCharacterConfigUI(Data.CharacterData.Character.ToString()).iconClassGold;
		if ((bool)m_ClassColor)
		{
			m_ClassColor.color = characterColor;
		}
		if ((bool)m_ClassIcon)
		{
			m_ClassIcon.sprite = iconClassGold;
		}
		if ((bool)m_HeaderColor)
		{
			m_HeaderColor.color = characterColor;
		}
		if ((bool)m_HeroIllustration)
		{
			m_HeroIllustration.sprite = Data.CharacterData.InfoIllustration;
		}
		if (m_CharacterName != null)
		{
			m_CharacterName.text = retirementParams.CharacterData.Name;
		}
		if (m_CharacterSlot != null)
		{
			m_CharacterSlot.Init(retirementParams.CharacterData);
		}
		if (m_GroupQuests != null)
		{
			GameObject gameObject = m_GroupQuests.gameObject;
			int active;
			if (Data.HasInspiration)
			{
				List<CharacterPersonalQuestDTO> questOptions = Data.QuestOptions;
				active = ((questOptions != null && questOptions.Count > 0) ? 1 : 0);
			}
			else
			{
				active = 0;
			}
			gameObject.SetActive((byte)active != 0);
		}
		if (m_InsufficientInspiration != null)
		{
			m_InsufficientInspiration.gameObject.SetActive(!Data.HasInspiration);
		}
		PopulatePersonalQuests();
		PrepareButton(m_Close, Data?.CloseCallback, null);
		PrepareButton(m_Next, Data?.NextStepCallback, "GUI_RETIRE");
		m_InvalidateLayout = true;
	}

	private void LateUpdate()
	{
		if (m_InvalidateLayout)
		{
			m_InvalidateLayout = false;
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_InfoPanelContainer);
		}
	}

	private void PopulatePersonalQuests()
	{
		int num = 0;
		if (Data != null && Data.HasInspiration)
		{
			num = (Data?.QuestOptions?.Count).GetValueOrDefault();
		}
		HelperTools.NormalizePool(ref m_QuestCards, m_QuestCards[0].gameObject, m_QuestsContainer, num);
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			CharacterPersonalQuestDTO characterPersonalQuestDTO = Data?.QuestOptions[i];
			if (!(characterPersonalQuestDTO == null))
			{
				InitCardDef(m_QuestCards[i], characterPersonalQuestDTO, characterPersonalQuestDTO.Def.ID.ID == Data.SelectedAdditionalQuestID);
			}
		}
		void InitCardDef(UIPersonalQuestCard questCard, CharacterPersonalQuestDTO personalQuestDTO, bool isSelected)
		{
			questCard.Init(personalQuestDTO, () =>
			{
				Data.ChooseQuestCallback(personalQuestDTO.Def.ID.ID);
			});
			questCard.ToggleHighlight(isSelected);
			UIPrefabTooltipTarget component = questCard.GetComponent<UIPrefabTooltipTarget>();
			if (component != null)
			{
				component.Initialize(TooltipPrototype, (GameObject go) =>
				{
					UIGenericTooltipRenderer.Params obj = new UIGenericTooltipRenderer.Params
					{
						Icon = null,
						CaptionLiteral = personalQuestDTO.Name,
						DescriptionLiteral = personalQuestDTO.Description + "\n\n" + personalQuestDTO.GoalDescription,
						Hints = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT")
					};
					go.GetComponent<UIGenericTooltipRenderer>()?.Init(obj);
				});
			}
		}
	}

	private void PrepareButton(FHButton button, Action callback, string localizationKey)
	{
		if (button == null)
		{
			return;
		}
		button.onClick.RemoveAllListeners();
		button.gameObject.SetActive(value: false);
		if (callback != null)
		{
			if (button.Text != null)
			{
				button.SetTextKey(localizationKey);
			}
			button.gameObject.SetActive(value: true);
			button.onClick.AddListener(() =>
			{
				callback();
			});
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		Data = null;
	}
}

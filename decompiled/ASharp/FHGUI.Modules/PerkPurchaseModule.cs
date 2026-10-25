using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Modules;

public class PerkPurchaseModule : FHMapViewModule
{
	public struct Params
	{
		public PerkDTO PerkDto;

		public List<AttackModifierDTO> ModifierToAdd;

		public List<AttackModifierDTO> ModifierToRemove;

		public Action<PerkDTO> OnPurchaseCallback;

		public IEnumerable<string> UnlockHints;

		public string CostLocKey;

		public string HintLocKey;

		public bool ReadOnly;
	}

	[Header("Components")]
	[SerializeField]
	private TMP_Text m_Caption;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private TextLocalizedListener m_Hint;

	[SerializeField]
	private TMP_Text m_Cost;

	[SerializeField]
	private Image m_ActionIcon;

	[SerializeField]
	private Image m_DeckIcon;

	[SerializeField]
	private Image m_TrashIcon;

	[SerializeField]
	private FHButton m_PurchaseButton;

	[SerializeField]
	private UIPerkElement m_PerkIcon;

	[SerializeField]
	private RectTransform m_ModifiersToAddContainer;

	[SerializeField]
	private RectTransform m_ModifiersToRemoveContainer;

	[SerializeField]
	private FHUIAttackModifier m_ModifierIconPrototype;

	[SerializeField]
	private TMP_Text m_UnlockHints;

	[Header("Settings")]
	[SerializeField]
	private Sprite m_ActionIconLearn;

	[SerializeField]
	private Sprite m_ActionIconImprove;

	[SerializeField]
	private Sprite m_ActionIconForget;

	private List<FHUIAttackModifier> m_ToAddModifierIcons = new List<FHUIAttackModifier>();

	private List<FHUIAttackModifier> m_ToRemoveModifierIcons = new List<FHUIAttackModifier>();

	private bool m_Initialized;

	public Params Data { get; private set; }

	private void InitCache()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_PurchaseButton.onClick.AddListener(HandleOnPurchase);
			m_ModifiersToAddContainer.GetComponentsInChildren(m_ToAddModifierIcons);
			m_ModifiersToRemoveContainer.GetComponentsInChildren(m_ToRemoveModifierIcons);
		}
	}

	public void Init(Params @params)
	{
		InitCache();
		Data = @params;
		UpdateVisuals();
		UpdateModifiers(@params.ModifierToAdd, m_ToAddModifierIcons, m_ModifiersToAddContainer);
		UpdateModifiers(@params.ModifierToRemove, m_ToRemoveModifierIcons, m_ModifiersToRemoveContainer);
		UpdateHints(@params.UnlockHints);
	}

	private void UpdateVisuals()
	{
		bool flag = Data.PerkDto == null;
		if ((bool)m_Caption)
		{
			m_Caption.enabled = !flag;
			if (!flag)
			{
				m_Caption.text = Data.PerkDto.NameLocalized;
			}
		}
		if ((bool)m_Description)
		{
			m_Description.enabled = !flag;
			if (!flag)
			{
				m_Description.text = Data.PerkDto.DescriptionLocalized;
			}
		}
		if ((bool)m_Hint)
		{
			m_Hint.SetTextKey(Data.HintLocKey);
		}
		if ((bool)m_Cost)
		{
			m_Cost.enabled = !flag;
			if (!flag)
			{
				m_Cost.text = FHUILocFormatting.PopulateParams(Data.CostLocKey, Data.PerkDto.Cost);
			}
		}
		if ((bool)m_PurchaseButton)
		{
			m_PurchaseButton.gameObject.SetActive(!flag);
			if (!flag)
			{
				m_PurchaseButton.interactable = !Data.PerkDto.IsComplete && Data.PerkDto.Affordable && !Data.ReadOnly;
			}
		}
		if ((bool)m_ActionIcon)
		{
			m_ActionIcon.enabled = !flag;
			if (!flag)
			{
				m_ActionIcon.sprite = GetActonIcon(Data.PerkDto.GetActionType());
			}
		}
		if ((bool)m_DeckIcon)
		{
			bool active = !flag && Data.PerkDto.GetActionType() == PerkDTO.ActionType.Learn;
			m_DeckIcon.gameObject.SetActive(active);
		}
		if ((bool)m_TrashIcon)
		{
			bool active2 = !flag && Data.PerkDto.GetActionType() == PerkDTO.ActionType.Forget;
			m_TrashIcon.gameObject.SetActive(active2);
		}
		if ((bool)m_PerkIcon)
		{
			bool flag2 = !flag && Data.PerkDto.IsTactical;
			m_PerkIcon.gameObject.SetActive(flag2);
			if (flag2)
			{
				m_PerkIcon.Init(Data.PerkDto, null);
			}
		}
	}

	private void UpdateHints(IEnumerable<string> hints)
	{
		if (m_UnlockHints == null)
		{
			return;
		}
		string text = string.Empty;
		foreach (string hint in hints)
		{
			text = text + hint + Environment.NewLine;
		}
		bool flag = !string.IsNullOrEmpty(text);
		m_UnlockHints.enabled = flag;
		if (flag)
		{
			m_UnlockHints.text = text;
		}
	}

	private void UpdateModifiers(List<AttackModifierDTO> modifiers, List<FHUIAttackModifier> icons, RectTransform container)
	{
		UIHelperUtil.UpdateUIList(modifiers?.Count ?? 0, icons, m_ModifierIconPrototype, container, onShow);
		void onShow(int index, FHUIAttackModifier item)
		{
			item.Init(modifiers[index]);
		}
	}

	private Sprite GetActonIcon(PerkDTO.ActionType actionType)
	{
		return actionType switch
		{
			PerkDTO.ActionType.Learn => m_ActionIconLearn, 
			PerkDTO.ActionType.Forget => m_ActionIconForget, 
			PerkDTO.ActionType.Improve => m_ActionIconImprove, 
			_ => null, 
		};
	}

	private void HandleOnPurchase()
	{
		Data.OnPurchaseCallback?.Invoke(Data.PerkDto);
	}
}

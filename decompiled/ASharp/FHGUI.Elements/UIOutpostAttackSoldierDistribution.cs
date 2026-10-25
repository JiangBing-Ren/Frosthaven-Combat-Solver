using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHRL;
using GLOOM;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIOutpostAttackSoldierDistribution : MonoBehaviour
{
	public struct Params
	{
		public OutpostAttackEventDTO EventData;

		public uint SoldiersToSpend;

		public uint AvailableSoldiers;

		public bool HasOperationalBarracks;

		public IEnumerable<FHInputEventElement.IInputEventData> Actions;
	}

	[Header("Components")]
	[SerializeField]
	private FHInputEventContainer SoldierActions;

	[SerializeField]
	private IconListContainer SoldierIconContainer;

	[SerializeField]
	private TMP_Text SoldierHintText;

	[SerializeField]
	private TMP_Text SoldierDescriptionText;

	[SerializeField]
	private TMP_Text NoAvailableSoldersText;

	[SerializeField]
	private GameObject SoldiersGroup;

	[Header("Settings")]
	[SerializeField]
	private Sprite SpriteSolderAvailable;

	[SerializeField]
	private Sprite SpriteSolderSpent;

	[SerializeField]
	private Sprite SpriteSolderSelected;

	[SerializeField]
	private Material MaterialSolderAvailable;

	[SerializeField]
	private Material MaterialSolderSpent;

	[SerializeField]
	private Material MaterialSolderSelected;

	[Header("Colors")]
	[SerializeField]
	private Color InfoTextNormalColor;

	[SerializeField]
	private Color InfoTextPositiveColor;

	[SerializeField]
	private Color InfoTextNegativeColor;

	private uint m_SoldiersToSpend;

	private List<SpriteWithTooltipDTO> m_IconPool = new List<SpriteWithTooltipDTO>();

	public Params Data { get; private set; }

	public uint SoldiersToSpend => m_SoldiersToSpend;

	public void Init(Params @params)
	{
		Data = @params;
		bool flag = Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackFinished;
		m_SoldiersToSpend = Data.SoldiersToSpend;
		if (flag)
		{
			m_SoldiersToSpend = Math.Min(Data.AvailableSoldiers, Data.EventData.LastUsedSoldiers);
		}
		if ((bool)SoldierDescriptionText)
		{
			SoldierDescriptionText.gameObject.SetActive(Data.EventData.CanUseSoldiers);
			if (Data.EventData.CanUseSoldiers)
			{
				SoldierDescriptionText.text = FHUILocFormatting.PopulateParams("GUI_OUTPOSTDEFENCE_SOLDIER_DESCRIPTION", FHUILocFormatting.Color(Data.EventData.PerSolderDefenseValueModifier.ToString(), Color.white));
			}
		}
		if ((bool)SoldierHintText)
		{
			SoldierHintText.gameObject.SetActive(!flag);
		}
		if ((bool)NoAvailableSoldersText)
		{
			bool flag2 = Data.AvailableSoldiers == 0 && Data.SoldiersToSpend == 0 && Data.EventData.LastUsedSoldiers == 0;
			bool active = (!Data.HasOperationalBarracks | flag2) || !Data.EventData.CanUseSoldiers;
			NoAvailableSoldersText.gameObject.SetActive(active);
			if (!Data.HasOperationalBarracks)
			{
				NoAvailableSoldersText.text = LocalizationManager.GetTranslation("GUI_OUTPOST_ATTACK_SOLDIERS_FORBIDDEN_BARRACK_WRECKED");
			}
			else if (flag2)
			{
				NoAvailableSoldersText.text = LocalizationManager.GetTranslation("GUI_OUTPOST_ATTACK_SOLDIERS_DEPLETED");
			}
			else if (!Data.EventData.CanUseSoldiers)
			{
				NoAvailableSoldersText.text = LocalizationManager.GetTranslation("GUI_SOLDIERS_FORBIDDEN");
			}
		}
		SoldierActions.Init(Data.Actions);
		UpdateIcons();
	}

	private void UpdateIcons()
	{
		m_IconPool.Clear();
		bool flag = Data.EventData.CurrentState == OutpostAttackState.AttackState.AttackFinished;
		uint num = ((!flag) ? Data.SoldiersToSpend : Data.EventData.LastUsedSoldiers);
		uint num2 = ((!flag) ? (Data.AvailableSoldiers - num) : Data.AvailableSoldiers);
		for (int i = 0; i < Data.EventData.MaxSolderCount; i++)
		{
			Sprite spriteSolderSpent = SpriteSolderSpent;
			if (!Data.EventData.CanUseSoldiers)
			{
				m_IconPool.Add(new SpriteWithTooltipDTO
				{
					Sprite = spriteSolderSpent
				});
				continue;
			}
			if (i < num)
			{
				spriteSolderSpent = SpriteSolderSelected;
				_ = MaterialSolderSelected;
			}
			else if (i < num + num2)
			{
				spriteSolderSpent = SpriteSolderAvailable;
				_ = MaterialSolderAvailable;
			}
			else
			{
				spriteSolderSpent = SpriteSolderSpent;
				_ = MaterialSolderSpent;
			}
			m_IconPool.Add(new SpriteWithTooltipDTO
			{
				Sprite = spriteSolderSpent
			});
		}
		SoldierIconContainer.Init(null, m_IconPool);
	}
}

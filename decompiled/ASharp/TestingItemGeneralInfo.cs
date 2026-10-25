using System;
using System.Linq;
using FH.YML;
using FHGUI.Common;
using FHGUI.DTO;
using FHRL;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class TestingItemGeneralInfo : MonoBehaviour, IGeneralInfoProvider
{
	public enum ItemGeneralInfoType
	{
		ItemYML,
		ItemDef,
		ItemDTO,
		CItem
	}

	public Image Image;

	public ItemGeneralInfoType _generalInfoType;

	[SerializeField]
	[HideInInspector]
	private int _itemID;

	private ItemCardYMLData _item;

	private void OnEnable()
	{
		SetItemID(_itemID);
	}

	public void SetItemID(int itemID)
	{
		_itemID = itemID;
		try
		{
			_item = ScenarioRuleClient.SRLYML.ItemCards.Single((ItemCardYMLData s) => s.ID == _itemID);
		}
		catch (Exception)
		{
			_item = ScenarioRuleClient.SRLYML.PetCards.Single((ItemCardYMLData s) => s.ID == _itemID);
		}
		Image.sprite = UIInfoTools.Instance.GetItemMiniSprite(_item.Art);
	}

	public IGeneralInfoData GetInfoData()
	{
		return _generalInfoType switch
		{
			ItemGeneralInfoType.ItemYML => (IGeneralInfoData)UIGeneralInfoUtils.GetItemInfo(_item), 
			ItemGeneralInfoType.ItemDef => UIGeneralInfoUtils.GetItemInfo(YMLDefRepository.Repo.DefRepository.Items[new ItemID((uint)_itemID)]), 
			ItemGeneralInfoType.ItemDTO => UIGeneralInfoUtils.GetItemInfo(new ItemDataDTO(YMLDefRepository.Repo.DefRepository.Items[new ItemID((uint)_itemID)])), 
			ItemGeneralInfoType.CItem => UIGeneralInfoUtils.GetItemInfo(new CItem(_itemID, (_item.Slot == CItem.EItemSlot.Pet) ? CBaseCard.ECardType.Pet : CBaseCard.ECardType.Item)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}

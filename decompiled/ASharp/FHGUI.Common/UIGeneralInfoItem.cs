using System.Collections;
using System.Linq;
using FHGUI.Elements;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoItem : UIGeneralInfoBase, IUIGeneralInfo<UIGeneralInfoItem.UIGeneralInfoItemData>, IUIGeneralInfo
{
	public struct UIGeneralInfoItemData : IGeneralInfoData
	{
		public int ItemID;

		public string Name;

		public string Description;

		public CItem.EItemSlot? Slot;

		public Sprite Icon;

		public bool? IsUsed;
	}

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private TMP_Text _typeDescription;

	[Header("Tabs")]
	[SerializeField]
	private FHTabsGroup _tabs;

	[SerializeField]
	private FHTabButton _statusTab;

	[Header("Left side")]
	[SerializeField]
	private Image _icon;

	[Header("Right side")]
	[Header("Status Tab")]
	[SerializeField]
	private RectTransform _descriptionContainer;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private RectTransform _itemCardInfo;

	[SerializeField]
	private UIGeneralInfoSection _glossary;

	private UIGeneralInfoItemData _data;

	private Coroutine _glossaryCrt;

	private FHTabButton CurrentTab => _tabs.ActiveTab;

	private void Awake()
	{
		_statusTab.onClick.AddListener(ShowStatusTab);
	}

	public bool Show(UIGeneralInfoItemData data, params object[] parameters)
	{
		gameObject.SetActive(value: true);
		_data = data;
		_name.text = data.Name;
		_typeDescription.enabled = !data.Description.IsNullOrEmpty();
		_description.text = data.Description;
		string text = ((!data.Slot.HasValue) ? string.Empty : I2.Loc.LocalizationManager.GetTranslation(FHUILocKeys.GetItemSlotKey(data.Slot.Value)));
		_typeDescription.text = GLOOM.LocalizationManager.GetTranslation("Item") + (text.IsNullOrEmpty() ? string.Empty : " - ") + text;
		_icon.sprite = data.Icon;
		ShowStatusTab();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowOpened, null, "GeneralInfoItem"));
		return true;
	}

	protected override void OnHide_Internal()
	{
		base.OnHide_Internal();
		if (_glossaryCrt != null)
		{
			StopCoroutine(_glossaryCrt);
		}
		_glossaryCrt = null;
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.WindowClosed, null, "GeneralInfoItem"));
	}

	private void BuildItemCardLayout(UIGeneralInfoItemData data)
	{
		for (int num = _itemCardInfo.childCount - 1; num >= 0; num--)
		{
			_itemCardInfo.GetChild(num).gameObject.SetActive(value: false);
			Object.Destroy(_itemCardInfo.GetChild(num).gameObject);
		}
		if (PersistentData.CreateItemCardLayout(_data.ItemID, _itemCardInfo))
		{
			_description.enabled = false;
		}
	}

	private IEnumerator BuildGlossary()
	{
		yield return null;
		UIGlossaryData[] glossaryEntries = (_description.enabled ? LocalizationGlossaryUtils.GetGlossaryEntriesFromText(_data.Description).ToArray() : LocalizationGlossaryUtils.GetGlossaryContents(_itemCardInfo.gameObject).ToArray());
		_glossary.Fill(show: true, (UIGlossaryContainer container) =>
		{
			container.Init(null, glossaryEntries);
		});
	}

	private void SetGlossaryTooltips()
	{
		UITextTooltipTarget[] componentsInChildren = GetComponentsInChildren<UITextTooltipTarget>(includeInactive: false);
		for (int i = 0; i < componentsInChildren?.Length; i++)
		{
			if (!(componentsInChildren[i] != null))
			{
				continue;
			}
			TextMeshProUGUI component = componentsInChildren[i].GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				if (LocalizationGlossaryUtils.TryParseGlossaryTexts(component.text, out var parsedText))
				{
					componentsInChildren[i].Initialize(UITooltipManager.Corner.Auto, new Vector2(5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
					componentsInChildren[i].SetText(parsedText);
				}
				else
				{
					componentsInChildren[i].enabled = false;
				}
			}
		}
	}

	private void ShowStatusTab()
	{
		_tabs.SwitchToTab(_statusTab);
		BuildItemCardLayout(_data);
		_glossaryCrt = StartCoroutine(BuildGlossary());
	}
}

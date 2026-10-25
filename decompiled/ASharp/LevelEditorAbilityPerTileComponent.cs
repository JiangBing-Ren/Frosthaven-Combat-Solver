using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class LevelEditorAbilityPerTileComponent : MonoBehaviour
{
	[Header("Settings")]
	public TMP_Dropdown AbilityIdDropDownPartySize1;

	public TMP_Dropdown AbilityIdDropDownPartySize2;

	public TMP_Dropdown AbilityIdDropDownPartySize3;

	public TMP_Dropdown AbilityIdDropDownPartySize4;

	private List<string> m_AbilityIds = new List<string>();

	[Header("Tiles")]
	public GameObject InlineRemovalListItemPrefab;

	public LevelEditorGenericListPanel TilesPanel;

	public TextMeshProUGUI TileTitle;

	public GameObject NeedSelectionOverlay;

	private TileIndex m_CurrentlyEditedTile;

	private List<LevelEditorListItemInlineButtons> m_TileIndexItems = new List<LevelEditorListItemInlineButtons>();

	public List<TileIndex> TileIndexes = new List<TileIndex>();

	public Dictionary<TileIndex, List<string>> AbilityIdByLocation = new Dictionary<TileIndex, List<string>>();

	private bool m_Initialised;

	private void Awake()
	{
		EnsureInitialised();
	}

	private void EnsureInitialised()
	{
		if (!m_Initialised)
		{
			m_AbilityIds.Add("NONE");
			m_AbilityIds.AddRange(ScenarioRuleClient.SRLYML.ScenarioAbilities.Select((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID).ToList());
			AbilityIdDropDownPartySize1.options.Clear();
			AbilityIdDropDownPartySize2.options.Clear();
			AbilityIdDropDownPartySize3.options.Clear();
			AbilityIdDropDownPartySize4.options.Clear();
			AbilityIdDropDownPartySize1.AddOptions(m_AbilityIds);
			AbilityIdDropDownPartySize2.AddOptions(m_AbilityIds);
			AbilityIdDropDownPartySize3.AddOptions(m_AbilityIds);
			AbilityIdDropDownPartySize4.AddOptions(m_AbilityIds);
			m_Initialised = true;
		}
	}

	public void SetScenarioModifier(CScenarioModifier modifier)
	{
		EnsureInitialised();
		AbilityIdByLocation?.Clear();
		TileIndexes.Clear();
		if (modifier != null)
		{
			if (modifier is CScenarioModifierTriggerAbility cScenarioModifierTriggerAbility)
			{
				if (cScenarioModifierTriggerAbility.TileSpecificAbilityIds != null)
				{
					AbilityIdByLocation = cScenarioModifierTriggerAbility.TileSpecificAbilityIds.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value);
				}
				if (AbilityIdByLocation == null)
				{
					AbilityIdByLocation = new Dictionary<TileIndex, List<string>>();
				}
				foreach (TileIndex key in AbilityIdByLocation.Keys)
				{
					TileIndexes.Add(key);
				}
			}
			else if (modifier is CScenarioModifierApplyActiveBonusToActor cScenarioModifierApplyActiveBonusToActor)
			{
				if (cScenarioModifierApplyActiveBonusToActor.TileSpecificAbilityIds != null)
				{
					AbilityIdByLocation = cScenarioModifierApplyActiveBonusToActor.TileSpecificAbilityIds.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value);
				}
				if (AbilityIdByLocation == null)
				{
					AbilityIdByLocation = new Dictionary<TileIndex, List<string>>();
				}
				foreach (TileIndex key2 in AbilityIdByLocation.Keys)
				{
					TileIndexes.Add(key2);
				}
			}
			else if (modifier is CScenarioModifierActivateClosestAI cScenarioModifierActivateClosestAI)
			{
				if (cScenarioModifierActivateClosestAI.TileSpecificAbilityIds != null)
				{
					AbilityIdByLocation = cScenarioModifierActivateClosestAI.TileSpecificAbilityIds.ToDictionary((KeyValuePair<TileIndex, List<string>> kv) => kv.Key, (KeyValuePair<TileIndex, List<string>> kv) => kv.Value);
				}
				if (AbilityIdByLocation == null)
				{
					AbilityIdByLocation = new Dictionary<TileIndex, List<string>>();
				}
				foreach (TileIndex key3 in AbilityIdByLocation.Keys)
				{
					TileIndexes.Add(key3);
				}
			}
		}
		UpdateTileIndexPanel();
	}

	public void UpdateTileIndexPanel()
	{
		if (m_TileIndexItems == null)
		{
			m_TileIndexItems = new List<LevelEditorListItemInlineButtons>();
		}
		foreach (LevelEditorListItemInlineButtons tileIndexItem in m_TileIndexItems)
		{
			Object.Destroy(tileIndexItem.gameObject);
		}
		m_TileIndexItems.Clear();
		if (TileIndexes != null && TileIndexes.Count > 0)
		{
			for (int i = 0; i < TileIndexes.Count; i++)
			{
				LevelEditorListItemInlineButtons component = Object.Instantiate(InlineRemovalListItemPrefab, TilesPanel.ListItemParent.transform).GetComponent<LevelEditorListItemInlineButtons>();
				m_TileIndexItems.Add(component);
				TileIndex tileIndex = TileIndexes[i];
				component.SetupListItem("Tile: " + tileIndex.X + "," + tileIndex.Y, i, OnDeleteTileIndexKey, TileItemSelected);
			}
		}
		SetupDisplayForTile(null);
	}

	public void SetupDisplayForTile(TileIndex tile)
	{
		if (tile == null)
		{
			NeedSelectionOverlay.gameObject.SetActive(value: true);
			AbilityIdDropDownPartySize1.SetValueWithoutNotify(0);
			AbilityIdDropDownPartySize2.SetValueWithoutNotify(0);
			AbilityIdDropDownPartySize3.SetValueWithoutNotify(0);
			AbilityIdDropDownPartySize4.SetValueWithoutNotify(0);
			TileTitle.text = string.Empty;
			return;
		}
		NeedSelectionOverlay.gameObject.SetActive(value: false);
		m_CurrentlyEditedTile = tile;
		int num = AbilityIdDropDownPartySize1.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == AbilityIdByLocation[tile][0]);
		int num2 = AbilityIdDropDownPartySize2.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == AbilityIdByLocation[tile][1]);
		int num3 = AbilityIdDropDownPartySize3.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == AbilityIdByLocation[tile][2]);
		int num4 = AbilityIdDropDownPartySize4.options.FindIndex((TMP_Dropdown.OptionData o) => o.text == AbilityIdByLocation[tile][3]);
		AbilityIdDropDownPartySize1.SetValueWithoutNotify((num > -1) ? num : 0);
		AbilityIdDropDownPartySize2.SetValueWithoutNotify((num2 > -1) ? num2 : 0);
		AbilityIdDropDownPartySize3.SetValueWithoutNotify((num3 > -1) ? num3 : 0);
		AbilityIdDropDownPartySize4.SetValueWithoutNotify((num4 > -1) ? num4 : 0);
		TileTitle.text = $"Ability for tile [{tile.X}:{tile.Y}]:";
	}

	public void OnButtonSelectTilePressed()
	{
		LevelEditorController.SelectTile(TileSelected);
	}

	public void TileItemSelected(LevelEditorListItemInlineButtons tileItemSelected)
	{
		TileIndex tileIndex = TileIndexes[tileItemSelected.ItemIndex];
		CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[tileIndex.X, tileIndex.Y];
		LevelEditorController.s_Instance.ShowLocationIndicator(cClientTile.m_GameObject.transform.position);
		SetupDisplayForTile(tileIndex);
	}

	public void TileSelected(CClientTile tileSelected)
	{
		TileIndex tileIndex = new TileIndex(tileSelected.m_Tile.m_ArrayIndex);
		if (!AbilityIdByLocation.ContainsKey(tileIndex))
		{
			AbilityIdByLocation.Add(tileIndex, new List<string>
			{
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty
			});
			TileIndexes.Add(tileIndex);
			UpdateTileIndexPanel();
			SetupDisplayForTile(tileIndex);
		}
	}

	private void OnDeleteTileIndexKey(LevelEditorListItemInlineButtons item)
	{
		TileIndex key = TileIndexes[item.ItemIndex];
		TileIndexes.RemoveAt(item.ItemIndex);
		AbilityIdByLocation.Remove(key);
		UpdateTileIndexPanel();
	}

	public void OnApplyToAllOthersPressed(int partySize)
	{
		int valueWithoutNotify = 0;
		switch (partySize)
		{
		case 1:
			valueWithoutNotify = AbilityIdDropDownPartySize1.value;
			break;
		case 2:
			valueWithoutNotify = AbilityIdDropDownPartySize2.value;
			break;
		case 3:
			valueWithoutNotify = AbilityIdDropDownPartySize3.value;
			break;
		case 4:
			valueWithoutNotify = AbilityIdDropDownPartySize4.value;
			break;
		}
		AbilityIdDropDownPartySize1.SetValueWithoutNotify(valueWithoutNotify);
		AbilityIdDropDownPartySize2.SetValueWithoutNotify(valueWithoutNotify);
		AbilityIdDropDownPartySize3.SetValueWithoutNotify(valueWithoutNotify);
		AbilityIdDropDownPartySize4.SetValueWithoutNotify(valueWithoutNotify);
		OnAbilityIDDropDownValueChanged();
	}

	public void OnApplyToAllOtherTilesPressed(int partySize)
	{
		int index = 0;
		switch (partySize)
		{
		case 1:
			index = AbilityIdDropDownPartySize1.value;
			break;
		case 2:
			index = AbilityIdDropDownPartySize2.value;
			break;
		case 3:
			index = AbilityIdDropDownPartySize3.value;
			break;
		case 4:
			index = AbilityIdDropDownPartySize4.value;
			break;
		}
		string item = m_AbilityIds[index];
		foreach (TileIndex item2 in AbilityIdByLocation.Keys.ToList())
		{
			AbilityIdByLocation[item2] = new List<string> { item, item, item, item };
		}
		SetupDisplayForTile(m_CurrentlyEditedTile);
	}

	public void OnAbilityIDDropDownValueChanged()
	{
		AbilityIdByLocation[m_CurrentlyEditedTile][0] = m_AbilityIds[AbilityIdDropDownPartySize1.value];
		AbilityIdByLocation[m_CurrentlyEditedTile][1] = m_AbilityIds[AbilityIdDropDownPartySize2.value];
		AbilityIdByLocation[m_CurrentlyEditedTile][2] = m_AbilityIds[AbilityIdDropDownPartySize3.value];
		AbilityIdByLocation[m_CurrentlyEditedTile][3] = m_AbilityIds[AbilityIdDropDownPartySize4.value];
	}
}

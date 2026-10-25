using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class LevelEditorElementInfusePanel : MonoBehaviour
{
	public GameObject InlineRemovalListItemPrefab;

	public TMP_Dropdown InfuseElementDropdown;

	public Transform InertItemParent;

	public Transform WaningItemParent;

	public Transform StrongItemParent;

	private List<LevelEditorListItemInlineButtons> m_CurrentInertItems = new List<LevelEditorListItemInlineButtons>();

	private List<LevelEditorListItemInlineButtons> m_CurrentWaningItems = new List<LevelEditorListItemInlineButtons>();

	private List<LevelEditorListItemInlineButtons> m_CurrentStrongItems = new List<LevelEditorListItemInlineButtons>();

	private void OnEnable()
	{
		InitUI();
	}

	public void InitUI()
	{
		InfuseElementDropdown.options.Clear();
		InfuseElementDropdown.AddOptions((from x in (ElementInfusionBoardManager.EElement[])Enum.GetValues(typeof(ElementInfusionBoardManager.EElement))
			where x != ElementInfusionBoardManager.EElement.Any
			select x.ToString()).ToList());
		RefreshUI();
	}

	public void RefreshUI()
	{
		foreach (LevelEditorListItemInlineButtons currentInertItem in m_CurrentInertItems)
		{
			UnityEngine.Object.Destroy(currentInertItem.gameObject);
		}
		m_CurrentInertItems.Clear();
		foreach (LevelEditorListItemInlineButtons currentWaningItem in m_CurrentWaningItems)
		{
			UnityEngine.Object.Destroy(currentWaningItem.gameObject);
		}
		m_CurrentWaningItems.Clear();
		foreach (LevelEditorListItemInlineButtons currentStrongItem in m_CurrentStrongItems)
		{
			UnityEngine.Object.Destroy(currentStrongItem.gameObject);
		}
		m_CurrentStrongItems.Clear();
		for (int i = 0; i < ElementInfusionBoardManager.GetElementColumn.Length; i++)
		{
			ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)i;
			switch (ElementInfusionBoardManager.GetElementColumn[i])
			{
			case ElementInfusionBoardManager.EColumn.Inert:
			{
				LevelEditorListItemInlineButtons component3 = UnityEngine.Object.Instantiate(InlineRemovalListItemPrefab, InertItemParent).GetComponent<LevelEditorListItemInlineButtons>();
				component3.SetupListItem(eElement.ToString(), i);
				m_CurrentInertItems.Add(component3);
				break;
			}
			case ElementInfusionBoardManager.EColumn.Waning:
			{
				LevelEditorListItemInlineButtons component2 = UnityEngine.Object.Instantiate(InlineRemovalListItemPrefab, WaningItemParent).GetComponent<LevelEditorListItemInlineButtons>();
				component2.SetupListItem(eElement.ToString(), i, OnDeleteElementPressed);
				m_CurrentWaningItems.Add(component2);
				break;
			}
			case ElementInfusionBoardManager.EColumn.Strong:
			{
				LevelEditorListItemInlineButtons component = UnityEngine.Object.Instantiate(InlineRemovalListItemPrefab, StrongItemParent).GetComponent<LevelEditorListItemInlineButtons>();
				component.SetupListItem(eElement.ToString(), i, OnDeleteElementPressed);
				m_CurrentStrongItems.Add(component);
				break;
			}
			}
		}
	}

	public void OnDeleteElementPressed(LevelEditorListItemInlineButtons buttonRemoved)
	{
		ElementInfusionBoardManager.GetElementColumn[buttonRemoved.ItemIndex]--;
		RefreshUI();
	}

	public void OnElementInfusePressed()
	{
		ElementInfusionBoardManager.Infuse(ElementInfusionBoardManager.Elements.Single((ElementInfusionBoardManager.EElement x) => x.ToString() == InfuseElementDropdown.options[InfuseElementDropdown.value].text), null);
		ElementInfusionBoardManager.ProcessEndTurnInfusions();
		RefreshUI();
	}
}

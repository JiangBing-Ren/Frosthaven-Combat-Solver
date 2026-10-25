using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldspaceTileBehaviourUI : WorldspaceDisplayPanelBase
{
	public TextMeshProUGUI ArrayIndexText;

	[SerializeField]
	private Image worldIcon;

	private TileBehaviour m_TileBehaviour;

	private bool initialised;

	public void Init(TileBehaviour tileBehaviour, string arrayIndexText)
	{
		m_TileBehaviour = tileBehaviour;
		transform.localScale = Vector3.one;
		ArrayIndexText.text = arrayIndexText;
		initialised = false;
		RefreshWorldIcon();
	}

	private void Initialize()
	{
		if (!initialised)
		{
			InitTileBehaviour(m_TileBehaviour.gameObject, m_TileBehaviour.transform, Vector2.one);
			initialised = true;
		}
	}

	private void Update()
	{
		if (DebugMenu.ConsoleVar_ShowTileIndex && !initialised)
		{
			Initialize();
		}
		ArrayIndexText.enabled = DebugMenu.ConsoleVar_ShowTileIndex;
	}

	public void RefreshWorldIcon()
	{
		if (m_TileBehaviour == null)
		{
			return;
		}
		if (m_TileBehaviour.m_ClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.CarryableQuestItem) != null)
		{
			ShowIcon(UIInfoTools.Instance.CarryableQuestItemWorldSprite);
			return;
		}
		CObjectProp cObjectProp = m_TileBehaviour.m_ClientTile.m_Tile.FindProp(ScenarioManager.ObjectImportType.Resource);
		if (cObjectProp != null && cObjectProp is CObjectResource cObjectResource && !string.IsNullOrEmpty(cObjectResource.ResourceData.Sprite))
		{
			ShowIcon(UIInfoTools.Instance.GetActiveAbilityIcon(cObjectResource.ResourceData.Sprite));
		}
		else
		{
			HideIcon();
		}
	}

	public void HideIcon()
	{
		worldIcon.gameObject.SetActive(value: false);
	}

	public void ShowIcon(Sprite icon)
	{
		Initialize();
		worldIcon.sprite = icon;
		worldIcon.gameObject.SetActive(value: true);
	}
}

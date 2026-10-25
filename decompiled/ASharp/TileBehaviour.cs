using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class TileBehaviour : MonoBehaviour
{
	public delegate void CallbackType(CClientTile clientTile, List<CTile> optionalTileList, bool networkActionIfOnline = false, bool isUserClick = false, bool actingPlayerHasSecondClickConfirmationEnabled = false, CInteractable.EInputType inputType = CInteractable.EInputType.None);

	public CClientTile m_ClientTile;

	[SerializeField]
	private float m_TooltipWidth = 300f;

	[SerializeField]
	private Vector2 m_TooltipOffset = new Vector2(-20f, 10f);

	private bool isShowingTooltip;

	private WorldspaceTileBehaviourUI m_WorldspaceTileBehaviourUI;

	public static CallbackType s_Callback;

	public static void SetCallback(CallbackType callback)
	{
		s_Callback = callback;
	}

	public void ShowTooltip(string tooltipTitle, string tooltipText = null)
	{
		isShowingTooltip = true;
		UITooltipManager.ResetContent();
		UITooltipManager.ShowBackground(show: true);
		UITooltipManager.SetScreenBound(enable: true, 20f);
		UITooltipManager.SetWidth(m_TooltipWidth);
		UITooltipManager.SetVerticalControls(autoAdjusted: true);
		if (tooltipTitle != null)
		{
			UITooltipManager.AddTitle(string.Format("<color=#{1}>{0}</color>", tooltipTitle, UIInfoTools.DefInstance.basicTextColor.ToHex()));
		}
		if (tooltipText != null)
		{
			UITooltipManager.AddDescription(tooltipText);
		}
		UITooltipManager.SetOffset(m_TooltipOffset);
		UITooltipManager.AnchorToRect(null, UITooltipManager.Corner.Auto);
		UITooltipManager.Show();
	}

	public void HideTooltip()
	{
		if (isShowingTooltip)
		{
			isShowingTooltip = false;
			UITooltipManager.Hide();
		}
	}

	private void OnDisable()
	{
		HideTooltip();
	}

	public void CreateWorldspaceTileBehaviourUI()
	{
		if (m_WorldspaceTileBehaviourUI == null && m_ClientTile != null && m_ClientTile.m_Tile != null)
		{
			GameObject gameObject = ObjectPool.Spawn(WorldspaceUITools.Instance.WorldspaceTileBehaviourUIPrefab, null, transform.position, Quaternion.identity);
			gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceGUIPrefabLevel.transform, worldPositionStays: false);
			m_WorldspaceTileBehaviourUI = gameObject.GetComponent<WorldspaceTileBehaviourUI>();
			CNode cNode = ScenarioManager.PathFinder.Nodes[m_ClientTile.m_Tile.m_ArrayIndex.X, m_ClientTile.m_Tile.m_ArrayIndex.Y];
			m_WorldspaceTileBehaviourUI.Init(this, m_ClientTile.m_Tile.m_ArrayIndex.X + ":" + m_ClientTile.m_Tile.m_ArrayIndex.Y + "(" + ((!cNode.IsBridge) ? "" : (cNode.IsBridgeOpen ? "BO" : "B")) + (cNode.Walkable ? "W" : "") + (cNode.Blocked ? "BL" : "") + ")");
		}
	}

	public void RefreshWorldSpaceIcon()
	{
		m_WorldspaceTileBehaviourUI.RefreshWorldIcon();
	}

	public void HideWorldSpaceIcon()
	{
		m_WorldspaceTileBehaviourUI.HideIcon();
	}
}

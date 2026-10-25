using System;
using System.Collections.Generic;
using AStar;
using FFSNet;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public static class ReproStepsLogger
{
	private static int s_SequentialID;

	private static readonly string[] s_CommonRoots = new string[9] { "Hands holder", "Cards Hands Manager", "InitiativeTrack", "Bottom Options", "GameGUILevel", "MainGUILevel", "Game scenario screen", "Game Elements", "Overlay Canvas" };

	public static void LogButtonClick(GameObject buttonGO, object sender, string additionalInfo)
	{
		string text;
		if (buttonGO != null)
		{
			ResolveButton(buttonGO, out var path, out var labelText);
			text = "'" + labelText + "'";
			if (path != null)
			{
				text = text + " (" + path + ")";
			}
		}
		else
		{
			text = additionalInfo;
		}
		LogEvent("Button Click", text);
	}

	public static void LogTileClick(CClientTile selectedTile, List<CTile> optionalTileList, List<Point> waypoints, CClientTile placementTile, CInteractable.EInputType inputType)
	{
		string highlightType = WorldspaceStarHexDisplay.Instance.GetHighlightType(selectedTile);
		if (highlightType == "none")
		{
			return;
		}
		string text = selectedTile.m_Tile.m_ArrayIndex.ToString();
		CActor livingActor = selectedTile.m_Tile.LivingActor;
		if (livingActor != null)
		{
			text = text + " (" + livingActor.Class.ID + ")";
		}
		if (selectedTile.m_Tile.m_Props != null && selectedTile.m_Tile.m_Props.Count > 0)
		{
			text = ((selectedTile.m_Tile.m_Props.Count != 1) ? (text + " (props: ") : (text + " (prop: "));
			for (int i = 0; i < selectedTile.m_Tile.m_Props.Count; i++)
			{
				if (i > 0)
				{
					text += ", ";
				}
				CObjectProp cObjectProp = selectedTile.m_Tile.m_Props[i];
				text += cObjectProp.PrefabName;
			}
			text += ")";
		}
		text = text + " (highlight: " + highlightType + ")";
		string text2 = Choreographer.s_Choreographer.CurrentActor?.Class?.ID;
		string text3 = null;
		CAbility currentAbility = CExpression.Globals.CurrentAbility;
		if (currentAbility != null)
		{
			text3 = currentAbility.GetType().Name + " '" + currentAbility.Name + "'";
		}
		if (text2 != null || text3 != null)
		{
			text = text + " (" + text2 + "." + text3 + ")";
		}
		text += $" (input type: {inputType})";
		LogEvent("Tile Click", text);
	}

	public static void LogConsoleCommand(string command, CActor selectedActor, Point pickedPoint)
	{
		string text;
		if (selectedActor == null)
		{
			text = "null";
		}
		else
		{
			text = selectedActor.Class.ID;
			if (!(selectedActor is CPlayerActor))
			{
				text += $" at {selectedActor.ArrayIndex}";
			}
		}
		string eventDescr = $"'{command}' (selected: {text}, picked: {pickedPoint})";
		LogEvent("Console Command", eventDescr);
	}

	public static void LogRemoteEvent(GameAction action)
	{
		LogMessage(action.SupplementaryDataString);
	}

	public static void LogEvent(string eventType, string eventDescr, bool localOnly = false)
	{
		string text = "";
		if (!localOnly && FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			string arg = ((!FFSNetwork.IsHost) ? $"Client {PlayerRegistry.MyPlayer.PlayerID.ID}" : "Host");
			text = $"[{arg} #{++s_SequentialID}] ";
		}
		LogMessage(text + eventType + ": " + eventDescr);
		if (!localOnly && FFSNetwork.IsOnline && PlayerRegistry.MyPlayer != null)
		{
			string text2 = text + "Remote " + eventType + ": " + eventDescr;
			if (text2.Length > 512)
			{
				text2 = text2.Substring(0, 509) + "...";
			}
			Synchronizer.SendSideAction(GameActionType.LogReproStep, null, canBeUnreliable: false, sendToHostOnly: false, 0, 0, 0, dataBool: false, text2);
		}
	}

	private static void LogMessage(string message)
	{
		message = Log.Colorize("[REPRO]", Color.cyan / 1.2f) + " [" + DateTime.Now.ToString("HH:mm:ss.fff") + "]: " + message;
		Log.Info(message);
	}

	private static void ResolveButton(GameObject buttonGO, out string path, out string labelText)
	{
		path = null;
		labelText = null;
		AbilityCardUI componentInParent = buttonGO.GetComponentInParent<AbilityCardUI>();
		FullAbilityCard componentInParent2 = buttonGO.GetComponentInParent<FullAbilityCard>();
		if (componentInParent2 != null)
		{
			string title = componentInParent2.Title;
			if (buttonGO == componentInParent2.topActionButton?.gameObject)
			{
				labelText = "Top Action (" + title + ")";
			}
			else if (buttonGO == componentInParent2.bottomActionButton?.gameObject)
			{
				labelText = "Bottom Action (" + title + ")";
			}
			else if (buttonGO == componentInParent2.DefaultAttackButton.gameObject)
			{
				labelText = "Default Attack (" + title + ")";
			}
			else if (buttonGO == componentInParent2.DefaultMoveButton.gameObject)
			{
				labelText = "Default Move (" + title + ")";
			}
			return;
		}
		MiniAbilityCard miniAbilityCard = buttonGO.GetComponentInParent<MiniAbilityCard>();
		if (miniAbilityCard == null && buttonGO == componentInParent?.gameObject && componentInParent.miniAbilityCard != null && componentInParent.miniAbilityCard.gameObject.activeInHierarchy)
		{
			miniAbilityCard = componentInParent.miniAbilityCard;
		}
		if (miniAbilityCard != null)
		{
			labelText = miniAbilityCard.initiativeText?.text + ": " + miniAbilityCard.titleText?.text;
			return;
		}
		CharacterSymbolTab component = buttonGO.GetComponent<CharacterSymbolTab>();
		if (component != null)
		{
			labelText = component.selectedNumberText?.text;
			path = buttonGO.name;
			return;
		}
		string text = buttonGO.transform.Find("Text")?.GetComponent<TextMeshProUGUI>()?.text;
		if (!string.IsNullOrEmpty(text))
		{
			labelText = text;
		}
		if (labelText == null)
		{
			UITextTooltipTarget component2 = buttonGO.GetComponent<UITextTooltipTarget>();
			if (!string.IsNullOrEmpty(component2?.tooltipText))
			{
				labelText = component2.tooltipText;
				if (labelText.Length > 32)
				{
					labelText = labelText.Substring(0, 30) + "...";
				}
			}
		}
		GameObject gameObject = buttonGO;
		while (gameObject != null)
		{
			string name = gameObject.name;
			if (Array.IndexOf(s_CommonRoots, name) >= 0)
			{
				break;
			}
			if (path == null)
			{
				path = name;
			}
			else if (path.Length < 40)
			{
				path = name + " / " + path;
			}
			else if (!path.StartsWith("... / ", StringComparison.Ordinal))
			{
				path = "... / " + path;
			}
			if (labelText == null)
			{
				TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
				if (componentInChildren != null)
				{
					labelText = componentInChildren.text;
				}
			}
			gameObject = gameObject.transform.parent?.gameObject;
		}
	}
}
